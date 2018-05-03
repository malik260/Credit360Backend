using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;

namespace FintrakBanking.Repositories.Credit

{
    public class LoanOperationsRepository : ILoanOperationsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IFinanceTransactionRepository financeTransaction;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        private ICasaLienRepository casaLien;
        private ILoanRepository loan;

        public LoanOperationsRepository(

        FinTrakBankingContext _context, IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail,
            ILoanScheduleRepository _loanSchedule, IWorkflow _workFlow, IApprovalLevelStaffRepository _level, ICasaLienRepository _casaLien
            , ILoanRepository _loan )
        {

            this.context = _context;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.workFlow = _workFlow;
            this.level = _level;
            this.casaLien = _casaLien;
            this.loan = _loan;
        }

        public decimal GetCollateralSearchChargeAmount(int stateId)
        {
            var collateralSearchChargeAmount = this.context.TBL_STATE.FirstOrDefault(x => x.STATEID == stateId).COLLATERALSEARCHCHARGEAMOUNT;


            return collateralSearchChargeAmount;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddCollateralSearchLien(CasaLienViewModel model)
        {

            //var data = new TBL_CASA_LIEN
            //{
            //    //PRODUCTACCOUNTNUMBER = model.productAccountNumber,
            //    //LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
            //    //SOURCEREFERENCENUMBER = model.sourceReferenceNumber,
            //    //BRANCHID = model.userBranchId,
            //    //COMPANYID = model.companyId,
            //    //LIENCREDITAMOUNT = GetCollateralSearchChargeAmount(model.stateId),
            //    //LIENDEBITAMOUNT = 0,
            //   // LIENTYPEID = (short)LienTypeEnum.CollateralSearch,
            //    //CREATEDBY = model.createdBy,
            //    //DESCRIPTION = "lien placed due to loan application collateral search", // model.description,
            //   // DATECREATED = generalSetup.GetApplicationDate()
            //};           

            model.lienAmount = GetCollateralSearchChargeAmount(model.stateId);
            model.branchId = model.userBranchId;
            model.lienTypeId = (short)LienTypeEnum.CollateralSearch;
            model.description = "lien placed due to loan application collateral search";
            model.dateTimeCreated = generalSetup.GetApplicationDate();

            var lienReference = casaLien.PlaceLien(model);

            return true;

        }

        #region Daily Operation

        [OperationBehavior(TransactionScopeRequired = true)]
        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTeamLoansInterestAccrual(DateTime applicationDate)


        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join c in context.TBL_LOAN_SCHEDULE_PERIODIC on b.TERMLOANID equals c.LOANID
                        join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                        where a.DATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                        && a.PAYMENTDATE == c.PAYMENTDATE

                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LOANREFERENCENUMBER,
                            productId = b.PRODUCTID,
                            branchId = b.BRANCHID,
                            companyId = b.COMPANYID,
                            currencyId = b.CURRENCYID,
                            exchangeRate = b.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = (double)a.DAILYINTERESTAMOUNT,
                            mainAmount = c.PERIODINTERESTAMOUNT,
                            categoryId = (short)DailyAccrualCategory.TermLoan,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DAYCOUNTCONVENTIONID,

                        }).ToList();

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);

            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);
            context.SaveChanges();


            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         }).ToList();



            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyLoansInterestAccrual(item);

            }
            context.SaveChanges();
            return model;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyFeeAccrual (DateTime applicationDate)


        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in context.TBL_LOAN_FEE
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                        where b.LOANSTATUSID == (short)LoanStatusEnum.Active && b.MATURITYDATE <= applicationDate


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LOANREFERENCENUMBER,
                            productId = b.PRODUCTID,
                            branchId = b.BRANCHID,
                            companyId = b.COMPANYID,
                            currencyId = b.CURRENCYID,
                            exchangeRate = b.EXCHANGERATE,
                            interestRate = b.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = (double)DailyAccruedInterest((DateTime)b.LASTRESTRUCTUREDATE,b.MATURITYDATE,a.FEEAMOUNT),
                            mainAmount = a.FEEAMOUNT,
                            categoryId = (short)a.CHARGEFEEID,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Principal,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DAYCOUNTCONVENTIONID,

                        }).ToList();

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);

            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);
            context.SaveChanges();


            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             //exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         }).ToList();



            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyLoansInterestAccrual(item);/// change to Daily Fee Accrued 

            }
            context.SaveChanges();
            return model;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTaxAccrual(DateTime applicationDate)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in context.TBL_LOAN_FEE
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                        where b.LOANSTATUSID == (short)LoanStatusEnum.Active && b.MATURITYDATE <= applicationDate


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LOANREFERENCENUMBER,
                            productId = b.PRODUCTID,
                            branchId = b.BRANCHID,
                            companyId = b.COMPANYID,
                            currencyId = b.CURRENCYID,
                            exchangeRate = b.EXCHANGERATE,
                            interestRate = b.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = (double)DailyAccruedInterest((DateTime)b.LASTRESTRUCTUREDATE, b.MATURITYDATE, a.TAXAMOUNT),
                            mainAmount = a.TAXAMOUNT,
                            categoryId = (short)a.CHARGEFEEID,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Principal,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DAYCOUNTCONVENTIONID,

                        }).ToList();

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);

            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);
            context.SaveChanges();


            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             //exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         }).ToList();



            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyLoansInterestAccrual(item);/// change to Daily Tax Accrued 

            }
            context.SaveChanges();
            return model;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.TBL_LOAN_REVOLVING
                        join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                        join c in context.TBL_DAY_COUNT_CONVENTION on a.DAYCOUNTCONVENTIONID equals c.DAYCOUNTCONVENTIONID
                        //where b.ActionDate == DbFunctions.TruncateTime(applicationDate) && a.LoanStatusId == (short)LoanStatusEnum.Active
                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                       && b.AVAILABLEBALANCE < 0 && a.SUSPENDINTEREST == false


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LOANREFERENCENUMBER,
                            productId = a.PRODUCTID,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            currencyId = a.CURRENCYID,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = a.INTERESTRATE,
                            mainAmount = a.OVERDRAFTLIMIT,
                            categoryId = (short)DailyAccrualCategory.AuthorisedOverdraft,
                            availableBalance = b.AVAILABLEBALANCE,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DAYCOUNTCONVENTIONID,
                            daysInAYear = c.DAYSINAYEAR,

                        });

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.AuthorisedOverdraft
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         });

            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.TBL_LOAN
                        join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                        join c in context.TBL_DAY_COUNT_CONVENTION on a.SCHEDULEDAYCOUNTCONVENTIONID equals c.DAYCOUNTCONVENTIONID
                        join d in context.TBL_SETUP_COMPANY on a.COMPANYID equals d.COMPANYID
                        //where b.ActionDate == DbFunctions.TruncateTime(applicationDate) && a.LoanStatusId == (short)LoanStatusEnum.Active
                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                        && b.AVAILABLEBALANCE < 0 && a.ALLOWFORCEDEBITREPAYMENT == false && a.SUSPENDINTEREST == false


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LOANREFERENCENUMBER,
                            productId = a.PRODUCTID,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            currencyId = a.CURRENCYID,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = d.UNAUTHORISD_OVERDRAFT_INT_RATE,
                            mainAmount = b.AVAILABLEBALANCE,
                            categoryId = (short)DailyAccrualCategory.UnauthorisedOverdraft,
                            availableBalance = b.AVAILABLEBALANCE,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DAYCOUNTCONVENTIONID,
                            daysInAYear = c.DAYSINAYEAR,


                        }).ToList();

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.UnauthorisedOverdraft
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         }).ToList();

            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyUnauthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDueInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in context.TBL_LOAN
                            //join b in context.TBL_LOAN_PAST_DUE on a.TERMLOANID equals b.LOANID
                        join c in context.TBL_DAY_COUNT_CONVENTION on a.SCHEDULEDAYCOUNTCONVENTIONID equals c.DAYCOUNTCONVENTIONID
                        join d in context.TBL_SETUP_COMPANY on a.COMPANYID equals d.COMPANYID
                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                        && a.ALLOWFORCEDEBITREPAYMENT == false && a.SUSPENDINTEREST == false && a.PASTDUEINTEREST > 0


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LOANREFERENCENUMBER,
                            productId = a.PRODUCTID,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            currencyId = a.CURRENCYID,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = a.INTERESTRATE,/// change to global charge rate 
                            mainAmount = a.PASTDUEINTEREST,
                            categoryId = (short)DailyAccrualCategory.PastDueInterest,
                            availableBalance = a.PASTDUEINTEREST,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DAYCOUNTCONVENTIONID,
                            daysInAYear = c.DAYSINAYEAR,


                        }).ToList();



            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();
            var count = data.Count();

            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.PastDueInterest
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         }).ToList();

            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyPastDueInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDuePrincipalAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.TBL_LOAN
                            //join b in context.TBL_LOAN_PAST_DUE on a.TERMLOANID equals b.LOANID
                        join c in context.TBL_DAY_COUNT_CONVENTION on a.SCHEDULEDAYCOUNTCONVENTIONID equals c.DAYCOUNTCONVENTIONID
                        join d in context.TBL_SETUP_COMPANY on a.COMPANYID equals d.COMPANYID
                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.ALLOWFORCEDEBITREPAYMENT == false
                        && a.SUSPENDINTEREST == false && a.PASTDUEPRINCIPAL > 0


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LOANREFERENCENUMBER,
                            productId = a.PRODUCTID,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            currencyId = a.CURRENCYID,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = a.INTERESTRATE,/// change to global charge rate 
                            mainAmount = a.PASTDUEPRINCIPAL,
                            categoryId = (short)DailyAccrualCategory.PastDuePrincipal,
                            availableBalance = a.PASTDUEPRINCIPAL,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DAYCOUNTCONVENTIONID,
                            daysInAYear = c.DAYSINAYEAR,


                        }).ToList();

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.PastDuePrincipal
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE} into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             //referenceNumber = groupedQ.Key.REFERENCENUMBER,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         }).ToList();

            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyPastDuePrincipalAccrual(item);
            }
            return data;
        }

        public IEnumerable<LoanClassificationViewModel> CalLoanClassification(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);
            var systemDate = generalSetup.GetApplicationDate();

            var data = (from a in context.TBL_LOAN
                        join b in context.TBL_LOAN_PAST_DUE on a.TERMLOANID equals b.LOANID
                        join c in context.TBL_DAY_COUNT_CONVENTION on a.SCHEDULEDAYCOUNTCONVENTIONID equals c.DAYCOUNTCONVENTIONID
                        join d in context.TBL_SETUP_COMPANY on a.COMPANYID equals d.COMPANYID
                        where b.DATE <= DbFunctions.TruncateTime(applicationDate) && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                       && (b.CREDITAMOUNT - b.DEBITAMOUNT) <= 0 && a.ALLOWFORCEDEBITREPAYMENT == false
                        group b by new { b.LOANID, b.PARENT_PASTDUECODE } into groupedQ
                        select new LoanClassificationViewModel()
                        {
                            loanId = groupedQ.Key.LOANID,
                            refNo = groupedQ.Key.PARENT_PASTDUECODE,
                            amount = groupedQ.Sum(i => i.CREDITAMOUNT - i.DEBITAMOUNT),
                        }).ToList();

            foreach (var item in data)
            {

                var pastDueDate = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == item.loanId).NPLDATE;

                if (pastDueDate == null && item.amount < 0)
                {
                    TBL_LOAN result = (from p in context.TBL_LOAN
                                       where p.TERMLOANID == item.loanId
                                       select p).SingleOrDefault();

                    result.NPLDATE = systemDate;

                    context.SaveChanges();
                }
                else if (pastDueDate != null && item.amount > 0)
                {
                    TBL_LOAN result = (from p in context.TBL_LOAN
                                       where p.TERMLOANID == item.loanId
                                       select p).SingleOrDefault();

                    result.NPLDATE = null;

                    context.SaveChanges();
                }

                else if (pastDueDate != null && item.amount < 0)
                {
                    DateTime nplDate = (DateTime)context.TBL_LOAN.FirstOrDefault(a => a.TERMLOANID == item.loanId).NPLDATE;
                    int prudentialStatus = 0;
                    if ((nplDate - applicationDate).Days <= 60)
                    {
                        prudentialStatus = (int)LoanPrudentialStatusEnum.Performing;
                    }
                    else if ((nplDate - applicationDate).Days > 60 && (nplDate - applicationDate).Days <= 90)
                    {
                        prudentialStatus = (int)LoanPrudentialStatusEnum.WatchList;
                    }
                    else if ((nplDate - applicationDate).Days > 90 && (nplDate - applicationDate).Days <= 180)
                    {
                        prudentialStatus = (int)LoanPrudentialStatusEnum.Substandard;
                    }
                    else if ((nplDate - applicationDate).Days > 180 && (nplDate - applicationDate).Days <= 360)
                    {
                        prudentialStatus = (int)LoanPrudentialStatusEnum.Doubtful;
                    }
                    else if ((nplDate - applicationDate).Days > 360)
                    {
                        prudentialStatus = (int)LoanPrudentialStatusEnum.Lost;
                    }
                    TBL_LOAN result = (from p in context.TBL_LOAN
                                       where p.TERMLOANID == item.loanId
                                       select p).SingleOrDefault();

                    result.EXT_PRUDENT_GUIDELINE_STATUSID = prudentialStatus;

                    context.SaveChanges();
                }

            }
            // context.SaveChanges();

            return data;
        }

        public IEnumerable<CleanUpViewModel> OverdraftCleanUp(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.TBL_LOAN_REVOLVING
                        join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                        join d in context.TBL_LOAN_COVENANT_DETAIL on a.REVOLVINGLOANID equals d.LOANID
                        join e in context.TBL_LOAN_COVENANT_TYPE on d.COVENANTTYPEID equals e.COVENANTTYPEID
                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && d.NEXTCOVENANTDATE == DbFunctions.TruncateTime(applicationDate)
                        select new CleanUpViewModel()
                        {
                            loanId = a.REVOLVINGLOANID,
                            nextCovenantDate = d.NEXTCOVENANTDATE,
                            casaBalance = b.AVAILABLEBALANCE,
                            freqValue = (short)d.FREQUENCYTYPEID,
                            casaAccountId = b.CASAACCOUNTID,
                            branchId = a.BRANCHID,
                        }).ToList();

            foreach (var item in data)
            {
                if (item.casaBalance < 0)
                {
                    //Change during integration
                    // financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
                    var casa = context.TBL_CASA.FirstOrDefault(a => a.CASAACCOUNTID == item.casaAccountId);
                    var refNo = context.TBL_LOAN_REVOLVING.FirstOrDefault(a => a.CASAACCOUNTID == item.casaAccountId);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = refNo.LOANREFERENCENUMBER;
                    lien.lienAmount = item.casaBalance;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.OverdraftCleanUp;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not swinging to positive";

                    casaLien.PlaceLien(lien);
                }
                else
                {
                    int freqValue = context.TBL_FREQUENCY_TYPE.FirstOrDefault(a => a.FREQUENCYTYPEID == item.freqValue).NUMBEROFDAYS;

                    TBL_LOAN_COVENANT_DETAIL result = (from p in context.TBL_LOAN_COVENANT_DETAIL
                                                       where p.LOANID == item.loanId
                                                       select p).SingleOrDefault();

                    result.NEXTCOVENANTDATE = result.NEXTCOVENANTDATE.Value.Date.AddDays(freqValue);
                    context.SaveChanges();
                }

            }
            return data;
        }

        public decimal DailyAccruedInterest(DateTime startDate,DateTime endDate, decimal Amount)
        {
            decimal dailyAmount = (Amount / ((int)(endDate - startDate).TotalDays));
            return dailyAmount;
        }

        //public void OverdraftCleanUp(int loanId)
        //{

        //    bool output = false;
        //    var systemDate = generalSetup.GetApplicationDate();

        //    var covenant = from a in context.TBL_LOAN
        //                   join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
        //                   where a.TERMLOANID == loanId
        //                   let percentage = b.ISPERCENTAGE
        //                   select percentage;

        //    if (covenant.FirstOrDefault() == false)
        //    {
        //        var covenantAmount = from a in context.TBL_LOAN
        //                             join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
        //                             join c in context.TBL_LOAN_COVENANT_TYPE on b.COVENANTTYPEID equals c.COVENANTTYPEID
        //                             where b.COVENANTTYPEID == c.COVENANTTYPEID && a.TERMLOANID == b.LOANID
        //                             && c.COVENANTTYPEID == (short)LoanCovenantTypeEnum.SinkingFund && a.TERMLOANID == loanId
        //                             && b.NEXTCOVENANTDATE == DbFunctions.TruncateTime(systemDate)
        //                             select b.COVENANTAMOUNT;
        //    }
        //    else
        //    {
        //        var covenantRate = from a in context.TBL_LOAN
        //                           join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
        //                           join c in context.TBL_LOAN_COVENANT_TYPE on b.COVENANTTYPEID equals c.COVENANTTYPEID
        //                           where b.COVENANTTYPEID == c.COVENANTTYPEID && a.TERMLOANID == b.LOANID
        //                           && c.COVENANTTYPEID == (short)LoanCovenantTypeEnum.SinkingFund && a.TERMLOANID == loanId
        //                           && b.NEXTCOVENANTDATE == DbFunctions.TruncateTime(systemDate)
        //                           select b.COVENANTAMOUNT;
        //    };


        //    var desc = "Overdraft Top";

        //    TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
        //                                 where p.REVOLVINGLOANID == loanId
        //                                 && p.LOANSTATUSID == (short)LoanStatusEnum.Active
        //                                 select p).SingleOrDefault();

        //    result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT + amount;


        //    AddCASAOverdraft(result.CASAACCOUNTID, amount, desc);

        //    context.SaveChanges();
        //}


        #endregion

        #region Anniversary  Operation

        public void updateloanTablePrincipal(int loanId, decimal amoumt)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.OUTSTANDINGPRINCIPAL = result.OUTSTANDINGPRINCIPAL - amoumt;


            context.SaveChanges();
        }

        public void updateloanTableInterest(int loanId, decimal amoumt)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.OUTSTANDINGINTEREST = result.OUTSTANDINGINTEREST - amoumt;


            context.SaveChanges();
        }

        public void updateloanTablePastDuePrincipal(int loanId, decimal amoumt)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.PASTDUEPRINCIPAL = result.PASTDUEPRINCIPAL + amoumt;

            context.SaveChanges();
        }

        public void updateloanTablePastDueInterest(int loanId, decimal amoumt)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.PASTDUEINTEREST = result.PASTDUEINTEREST + amoumt;

            context.SaveChanges();
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where a.PAYMENTDATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && b.ALLOWFORCEDEBITREPAYMENT == true
                         select new LoanRepaymentViewModel()
                         {
                             productId = b.PRODUCTID,
                             branchId = b.BRANCHID,
                             companyId = b.COMPANYID,
                             currencyId = b.CURRENCYID,
                             exchangeRate = b.EXCHANGERATE,
                             periodInterestAmount = a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = a.PERIODPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             paymentDate = applicationDate,
                             loanId = a.LOANID,
                             totalAmount = a.PERIODINTERESTAMOUNT + a.PERIODPRINCIPALAMOUNT,
                             casaAccountId = b.CASAACCOUNTID,
                             loanRefNo = b.LOANREFERENCENUMBER

                         }).ToList();

            List<TBL_LOAN_FORCE_DEBIT> transForceDebit = new List<TBL_LOAN_FORCE_DEBIT>();


            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                //var productTypeId = context.tbl_Product_Type.FirstOrDefault(x => x.ProductGroupId == item.productId).ProductTypeId;

                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casabalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).AVAILABLEBALANCE;
                if (casabalance >= item.totalAmount)
                {
                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);

                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    TBL_LOAN_FORCE_DEBIT forceDebit = new TBL_LOAN_FORCE_DEBIT();




                    forceDebit.LOANID = item.loanId;
                    forceDebit.FORCEDEBITCODE = forceDebitCode;
                    forceDebit.CREDITAMOUNT = 0;
                    forceDebit.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebit.DEBITAMOUNT = Math.Abs(casabalance - item.totalAmount);
                    forceDebit.DATE = item.paymentDate;
                    forceDebit.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebit.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    //forceDebit.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebit);

                    //var remainingAmount = casabalance - item.periodInterestAmount;

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "partial principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);

                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);
                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    TBL_LOAN_FORCE_DEBIT forceDebitInterest = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitInterest.LOANID = item.loanId;
                    forceDebitInterest.FORCEDEBITCODE = forceDebitCode;
                    forceDebitInterest.CREDITAMOUNT = 0;
                    forceDebitInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DEBITAMOUNT = Math.Abs(casabalance - item.periodInterestAmount);
                    forceDebitInterest.DATE = item.paymentDate;
                    forceDebitInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    //forceDebitInterest.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitInterest);

                    TBL_LOAN_FORCE_DEBIT forceDebitPrincipal = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitPrincipal.LOANID = item.loanId;
                    forceDebitPrincipal.FORCEDEBITCODE = forceDebitCode;
                    forceDebitPrincipal.CREDITAMOUNT = 0;
                    forceDebitPrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DEBITAMOUNT = item.periodPrincipalAmount;
                    forceDebitPrincipal.DATE = item.paymentDate;
                    forceDebitPrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    //forceDebitPrincipal.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);
                }
                else if (casabalance <= 0)
                {
                    TBL_LOAN_FORCE_DEBIT forceDebitInterest = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitInterest.LOANID = item.loanId;
                    forceDebitInterest.FORCEDEBITCODE = forceDebitCode;
                    forceDebitInterest.CREDITAMOUNT = 0;
                    forceDebitInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DEBITAMOUNT = Math.Abs(item.periodInterestAmount);
                    forceDebitInterest.DATE = item.paymentDate;
                    forceDebitInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    forceDebitInterest.PRODUCTTYPEID = (short)item.productId;

                    transForceDebit.Add(forceDebitInterest);

                    TBL_LOAN_FORCE_DEBIT forceDebitPrincipal = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitPrincipal.LOANID = item.loanId;
                    forceDebitPrincipal.FORCEDEBITCODE = forceDebitCode;
                    forceDebitPrincipal.CREDITAMOUNT = 0;
                    forceDebitPrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DEBITAMOUNT = Math.Abs(item.periodPrincipalAmount);
                    forceDebitPrincipal.DATE = item.paymentDate;
                    forceDebitPrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    forceDebitPrincipal.PRODUCTTYPEID = (short)item.productId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);
                }
            }

            this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue(DateTime applicationDate)

        {
            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where a.PAYMENTDATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && b.ALLOWFORCEDEBITREPAYMENT == false && a.PERIODPRINCIPALAMOUNT !=0 && a.PERIODINTERESTAMOUNT != 0
                         select new LoanRepaymentViewModel()
                         {
                             productId = b.PRODUCTID,
                             branchId = b.BRANCHID,
                             companyId = b.COMPANYID,
                             currencyId = b.CURRENCYID,
                             exchangeRate = b.EXCHANGERATE,
                             periodInterestAmount = a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = a.PERIODPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             paymentDate = applicationDate,
                             loanId = a.LOANID,
                             totalAmount = a.PERIODINTERESTAMOUNT + a.PERIODPRINCIPALAMOUNT,
                             casaAccountId = b.CASAACCOUNTID,
                             loanRefNo = b.LOANREFERENCENUMBER
                         }).ToList();

            List<TBL_LOAN_PAST_DUE> transPastDue = new List<TBL_LOAN_PAST_DUE>();

            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                var PastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId);
                var casabalance = casa.AVAILABLEBALANCE;
                decimal principalAmountNotCollected = 0;
                //decimal balanceAfterInterestAmountCollection = 0;
                decimal partialPrincipalAmountCollected = 0;
                decimal partialInterestAmountCollected = 0;
                decimal interestAmountNotCollected = 0;
                //if (casabalance >= item.periodInterestAmount)
                //{
                //    balanceAfterInterestAmountCollection = casabalance - item.periodInterestAmount;
                //}
                //if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                //{
                //    partialPrincipalAmountCollected = balanceAfterInterestAmountCollection;
                //    principalAmountNotCollected = item.periodPrincipalAmount - partialPrincipalAmountCollected;
                //}
                //if (casabalance < item.periodInterestAmount && casabalance > 0)
                //{
                //    partialInterestAmountCollected = casabalance;
                //    interestAmountNotCollected = item.periodInterestAmount - partialInterestAmountCollected;

                //}
                if (casabalance >= item.totalAmount)
                {

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    updateloanTablePrincipal(item.loanId,item.periodPrincipalAmount);
                    updateloanTableInterest(item.loanId, item.periodInterestAmount);
                    //financeTransaction.PostTransaction(inputTransactions);
                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    partialPrincipalAmountCollected = casabalance - item.periodInterestAmount; ;
                    principalAmountNotCollected = item.periodPrincipalAmount - partialPrincipalAmountCollected;
                    TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();


                    pastDue.LOANID = item.loanId;
                    pastDue.PARENT_PASTDUECODE = PastDueCode;
                    pastDue.CREDITAMOUNT = 0;
                    pastDue.DESCRIPTION = "Past Due Entries on Principal as a result of Account not funded";
                    pastDue.DEBITAMOUNT = Math.Abs(principalAmountNotCollected);
                    pastDue.DATE = item.paymentDate;
                    pastDue.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDue.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDue.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDue);
                    context.SaveChanges();
                    updateloanTablePastDuePrincipal(pastDue.LOANID, pastDue.DEBITAMOUNT);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, partialPrincipalAmountCollected, product.PRINCIPALBALANCEGL.Value, "partial principal repayment"));

                    updateloanTableInterest(item.loanId, item.periodInterestAmount);
                    updateloanTablePrincipal(item.loanId, partialPrincipalAmountCollected);
                   
                    // place lien on the customer account on partial principal
                    //financeTransaction.PostTransaction(inputTransactions);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDue.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDue.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Anniversary Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDue.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDue.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Anniversary Date";

                    casaLien.PlaceLien(lien);

                    context.SaveChanges();

                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    partialInterestAmountCollected = casabalance;
                    interestAmountNotCollected = item.periodInterestAmount - partialInterestAmountCollected;
                    TBL_LOAN_PAST_DUE pastDueInterest = new TBL_LOAN_PAST_DUE();

                    pastDueInterest.LOANID = item.loanId;
                    pastDueInterest.PASTDUECODE = PastDueCode;
                    pastDueInterest.CREDITAMOUNT = 0;
                    pastDueInterest.DESCRIPTION = "Past Due Entries on Interest as a result of Account not funded";
                    pastDueInterest.DEBITAMOUNT = Math.Abs(interestAmountNotCollected);
                    pastDueInterest.DATE = item.paymentDate;
                    pastDueInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDueInterest.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDueInterest);

                    context.SaveChanges();

                    updateloanTablePastDueInterest(pastDueInterest.LOANID, pastDueInterest.DEBITAMOUNT);

                    TBL_LOAN_PAST_DUE pastDuePrincipal = new TBL_LOAN_PAST_DUE();

                    pastDuePrincipal.LOANID = item.loanId;
                    pastDuePrincipal.PASTDUECODE = PastDueCode;
                    pastDuePrincipal.CREDITAMOUNT = 0;
                    pastDuePrincipal.DESCRIPTION = "Past Due Entries on Principal as a result of Account not funded";
                    pastDuePrincipal.DEBITAMOUNT = item.periodPrincipalAmount;
                    pastDuePrincipal.DATE = item.paymentDate;
                    pastDuePrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDuePrincipal.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDuePrincipal);

                    context.SaveChanges();

                    updateloanTablePastDuePrincipal(pastDuePrincipal.LOANID, pastDuePrincipal.DEBITAMOUNT);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, partialInterestAmountCollected, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment"));

                    updateloanTableInterest(item.loanId, partialInterestAmountCollected);

                    //financeTransaction.PostTransaction(inputTransactions);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDueInterest.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDueInterest.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.InterestRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Anniversary Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDueInterest.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDueInterest.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.InterestRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not swinging to positive";

                    var lienReference = casaLien.PlaceLien(lien);
                    context.SaveChanges();

                    //var dataP = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDuePrincipal.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDuePrincipal.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Anniversary Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(dataP);

                    CasaLienViewModel lienPrincipal = new CasaLienViewModel();

                    lienPrincipal.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lienPrincipal.sourceReferenceNumber = pastDuePrincipal.PARENT_PASTDUECODE;
                    lienPrincipal.lienAmount = pastDuePrincipal.DEBITAMOUNT;
                    lienPrincipal.branchId = item.branchId;
                    lienPrincipal.companyId = item.companyId;
                    lienPrincipal.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lienPrincipal.createdBy = (int)SystemStaff.System;
                    lienPrincipal.description = "lien placed due to Account not funded at Anniversary Date";

                    lienReference = casaLien.PlaceLien(lienPrincipal);
                    context.SaveChanges();
                }
                else if (casabalance <= 0)
                {
                    TBL_LOAN_PAST_DUE pastDueInterest = new TBL_LOAN_PAST_DUE();

                    pastDueInterest.LOANID = item.loanId;
                    pastDueInterest.PASTDUECODE = PastDueCode;
                    pastDueInterest.CREDITAMOUNT = 0;
                    pastDueInterest.DESCRIPTION = "Past Due Entries on Interest as a result of Account not funded";
                    pastDueInterest.DEBITAMOUNT = Math.Abs(item.periodInterestAmount);
                    pastDueInterest.DATE = item.paymentDate;
                    pastDueInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDueInterest.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDueInterest);
                    context.SaveChanges();
                    updateloanTablePastDueInterest(pastDueInterest.LOANID, pastDueInterest.DEBITAMOUNT);

                    TBL_LOAN_PAST_DUE pastDuePrincipal = new TBL_LOAN_PAST_DUE();

                    pastDuePrincipal.LOANID = item.loanId;
                    pastDuePrincipal.PASTDUECODE = PastDueCode;
                    pastDuePrincipal.CREDITAMOUNT = 0;
                    pastDuePrincipal.DESCRIPTION = "Past Due Entries on Principal as a result of Account not funded";
                    pastDuePrincipal.DEBITAMOUNT = Math.Abs(item.periodPrincipalAmount);
                    pastDuePrincipal.DATE = item.paymentDate;
                    pastDuePrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDuePrincipal.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDuePrincipal);
                    context.SaveChanges();
                    updateloanTablePastDuePrincipal(pastDuePrincipal.LOANID, pastDuePrincipal.DEBITAMOUNT);
                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDueInterest.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDueInterest.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.InterestRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Anniversary Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDueInterest.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDueInterest.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.InterestRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Anniversary Date";

                    var lienReference = casaLien.PlaceLien(lien);
                    context.SaveChanges();
                    //var dataP = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDuePrincipal.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDuePrincipal.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Anniversary Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(dataP);

                    CasaLienViewModel lienPrincipal = new CasaLienViewModel();

                    lienPrincipal.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lienPrincipal.sourceReferenceNumber = pastDuePrincipal.PARENT_PASTDUECODE;
                    lienPrincipal.lienAmount = pastDuePrincipal.DEBITAMOUNT;
                    lienPrincipal.branchId = item.branchId;
                    lienPrincipal.companyId = item.companyId;
                    lienPrincipal.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lienPrincipal.createdBy = (int)SystemStaff.System;
                    lienPrincipal.description = "lien placed due to Account not funded at Anniversary Date";

                    lienReference = casaLien.PlaceLien(lienPrincipal);
                    context.SaveChanges();

                }
            }

            this.context.TBL_LOAN_PAST_DUE.AddRange(transPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join c in context.TBL_DAILY_ACCRUAL on a.LOANREFERENCENUMBER equals c.REFERENCENUMBER
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && c.CATEGORYID == (short)DailyAccrualCategory.AuthorisedOverdraft
                         && b.AVAILABLEBALANCE < 0
                         group c by new
                         {
                             a.PRODUCTID,
                             a.BRANCHID,
                             a.COMPANYID,
                             a.CURRENCYID,
                             a.EXCHANGERATE,
                             a.LOANREFERENCENUMBER,
                             c.INTERESTRATE,
                             a.REVOLVINGLOANID,
                             b.CASAACCOUNTID
                         } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             interestRate = groupedQ.Key.INTERESTRATE,
                             paymentDate = applicationDate,
                             loanId = groupedQ.Key.REVOLVINGLOANID,
                             casaAccountId = groupedQ.Key.CASAACCOUNTID,
                             loanRefNo = groupedQ.Key.LOANREFERENCENUMBER,
                             periodInterestAmount = groupedQ.Sum(i => i.DAILYACCURALAMOUNT)


                         }).ToList();

            List<TBL_LOAN_FORCE_DEBIT> transForceDebit = new List<TBL_LOAN_FORCE_DEBIT>();


            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                item.createdBy = (int)SystemStaff.System;
                //var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId).AvailableBalance;
                //if (casabalance < 0 )
                //{
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildAuthorisedOverdraftRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                financeTransaction.PostTransaction(inputTransactions);
                // }
            }

            this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessUnauthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var model = (from a in context.TBL_LOAN
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join c in context.TBL_DAILY_ACCRUAL on a.LOANREFERENCENUMBER equals c.REFERENCENUMBER
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && c.CATEGORYID == (short)DailyAccrualCategory.UnauthorisedOverdraft
                         && b.AVAILABLEBALANCE < 0
                         group c by new
                         { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE, a.LOANREFERENCENUMBER, c.INTERESTRATE, a.TERMLOANID, b.CASAACCOUNTID } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             interestRate = groupedQ.Key.INTERESTRATE,
                             paymentDate = applicationDate,
                             loanId = groupedQ.Key.TERMLOANID,
                             casaAccountId = groupedQ.Key.CASAACCOUNTID,
                             loanRefNo = groupedQ.Key.LOANREFERENCENUMBER,
                             periodInterestAmount = groupedQ.Sum(i => i.DAILYACCURALAMOUNT)


                         }).ToList();

            List<TBL_LOAN_FORCE_DEBIT> transForceDebit = new List<TBL_LOAN_FORCE_DEBIT>();


            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                item.createdBy = (int)SystemStaff.System;
                //var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId).AvailableBalance;
                //if (casabalance < 0)
                //{
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildAuthorisedOverdraftRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                financeTransaction.PostTransaction(inputTransactions);
                //}
            }

            this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate)

        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var model = (from a in context.TBL_LOAN_PAST_DUE
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                         group a by new
                         { a.LOANID, a.TRANSACTIONTYPEID, a.PASTDUECODE, a.PARENT_PASTDUECODE } into groupedQ
                         select new LoanPastDueViewModel()
                         {
                             loanId = groupedQ.Key.LOANID,
                             transactionTypeId = groupedQ.Key.TRANSACTIONTYPEID,
                             pastDueCode = groupedQ.Key.PASTDUECODE,
                             parent_PastDueCode = groupedQ.Key.PARENT_PASTDUECODE,
                             totalAmount = groupedQ.Sum(i => (i.DEBITAMOUNT - i.CREDITAMOUNT)),
                         }).ToList();

            List<TBL_LOAN_PAST_DUE> loanPastDue = new List<TBL_LOAN_PAST_DUE>();



            foreach (var item in model)
            {
                TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();


                pastDue.LOANID = item.loanId;
                pastDue.PASTDUECODE = item.pastDueCode;
                pastDue.CREDITAMOUNT = 0;
                pastDue.DESCRIPTION = "Interest Accrual as a result of Past Due";
                pastDue.DEBITAMOUNT = Math.Abs(item.totalAmount);
                pastDue.DATE = applicationDate;
                pastDue.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                pastDue.PARENT_PASTDUECODE = item.parent_PastDueCode;

                loanPastDue.Add(pastDue);

            }

            this.context.TBL_LOAN_PAST_DUE.AddRange(loanPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(DateTime applicationDate)

        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var model = (from a in context.TBL_LOAN_PAST_DUE
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Principal
                         group a by new
                         { a.LOANID, a.TRANSACTIONTYPEID, a.PASTDUECODE, a.PARENT_PASTDUECODE } into groupedQ
                         select new LoanPastDueViewModel()
                         {
                             loanId = groupedQ.Key.LOANID,
                             transactionTypeId = groupedQ.Key.TRANSACTIONTYPEID,
                             pastDueCode = groupedQ.Key.PASTDUECODE,
                             parent_PastDueCode = groupedQ.Key.PARENT_PASTDUECODE,
                             totalAmount = groupedQ.Sum(i => (i.DEBITAMOUNT - i.CREDITAMOUNT)),
                         }).ToList();

            List<TBL_LOAN_PAST_DUE> loanPastDue = new List<TBL_LOAN_PAST_DUE>();



            foreach (var item in model)
            {
                TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();


                pastDue.LOANID = item.loanId;
                pastDue.PASTDUECODE = item.pastDueCode;
                pastDue.CREDITAMOUNT = 0;
                pastDue.DESCRIPTION = "Principal Accrual as a result of Past Due";
                pastDue.DEBITAMOUNT = Math.Abs(item.totalAmount);
                pastDue.DATE = applicationDate;
                pastDue.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                pastDue.PARENT_PASTDUECODE = item.parent_PastDueCode;

                loanPastDue.Add(pastDue);

            }

            this.context.TBL_LOAN_PAST_DUE.AddRange(loanPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebitForInterestReview(DateTime applicationDate, int loanId)
        {
            var systemDate = generalSetup.GetApplicationDate();

            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where b.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && b.ALLOWFORCEDEBITREPAYMENT == true && a.LOANID == loanId
                         && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                         select new LoanRepaymentViewModel()
                         {
                             productId = b.PRODUCTID,
                             branchId = b.BRANCHID,
                             companyId = b.COMPANYID,
                             currencyId = b.CURRENCYID,
                             exchangeRate = b.EXCHANGERATE,
                             periodInterestAmount = a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = a.PERIODPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             paymentDate = applicationDate,
                             loanId = a.LOANID,
                             totalAmount = a.PERIODINTERESTAMOUNT + a.PERIODPRINCIPALAMOUNT,
                             casaAccountId = b.CASAACCOUNTID,
                             loanRefNo = b.LOANREFERENCENUMBER

                         }).ToList();

            List<TBL_LOAN_FORCE_DEBIT> transForceDebit = new List<TBL_LOAN_FORCE_DEBIT>();


            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                //var productTypeId = context.tbl_Product_Type.FirstOrDefault(x => x.ProductGroupId == item.productId).ProductTypeId;

                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casabalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).AVAILABLEBALANCE;
                if (casabalance >= item.totalAmount)
                {
                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);

                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    TBL_LOAN_FORCE_DEBIT forceDebit = new TBL_LOAN_FORCE_DEBIT();




                    forceDebit.LOANID = item.loanId;
                    forceDebit.FORCEDEBITCODE = forceDebitCode;
                    forceDebit.CREDITAMOUNT = 0;
                    forceDebit.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebit.DEBITAMOUNT = Math.Abs(casabalance - item.totalAmount);
                    forceDebit.DATE = item.paymentDate;
                    forceDebit.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebit.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    //forceDebit.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebit);

                    //var remainingAmount = casabalance - item.periodInterestAmount;

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "partial principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);

                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);
                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    TBL_LOAN_FORCE_DEBIT forceDebitInterest = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitInterest.LOANID = item.loanId;
                    forceDebitInterest.FORCEDEBITCODE = forceDebitCode;
                    forceDebitInterest.CREDITAMOUNT = 0;
                    forceDebitInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DEBITAMOUNT = Math.Abs(casabalance - item.periodInterestAmount);
                    forceDebitInterest.DATE = item.paymentDate;
                    forceDebitInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    //forceDebitInterest.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitInterest);

                    TBL_LOAN_FORCE_DEBIT forceDebitPrincipal = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitPrincipal.LOANID = item.loanId;
                    forceDebitPrincipal.FORCEDEBITCODE = forceDebitCode;
                    forceDebitPrincipal.CREDITAMOUNT = 0;
                    forceDebitPrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DEBITAMOUNT = item.periodPrincipalAmount;
                    forceDebitPrincipal.DATE = item.paymentDate;
                    forceDebitPrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    //forceDebitPrincipal.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);
                }
                else if (casabalance <= 0)
                {
                    TBL_LOAN_FORCE_DEBIT forceDebitInterest = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitInterest.LOANID = item.loanId;
                    forceDebitInterest.FORCEDEBITCODE = forceDebitCode;
                    forceDebitInterest.CREDITAMOUNT = 0;
                    forceDebitInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DEBITAMOUNT = Math.Abs(item.periodInterestAmount);
                    forceDebitInterest.DATE = item.paymentDate;
                    forceDebitInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    forceDebitInterest.PRODUCTTYPEID = (short)item.productId;

                    transForceDebit.Add(forceDebitInterest);

                    TBL_LOAN_FORCE_DEBIT forceDebitPrincipal = new TBL_LOAN_FORCE_DEBIT();

                    forceDebitPrincipal.LOANID = item.loanId;
                    forceDebitPrincipal.FORCEDEBITCODE = forceDebitCode;
                    forceDebitPrincipal.CREDITAMOUNT = 0;
                    forceDebitPrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DEBITAMOUNT = Math.Abs(item.periodPrincipalAmount);
                    forceDebitPrincipal.DATE = item.paymentDate;
                    forceDebitPrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.PARENT_FORCEDEBITCODE = item.loanRefNo;
                    forceDebitPrincipal.PRODUCTTYPEID = (short)item.productId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    //updateloanTable(item);
                }
            }

            this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview(DateTime applicationDate, int loanId)

        {
            var systemDate = generalSetup.GetApplicationDate();

            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where b.LOANSTATUSID == (short)LoanStatusEnum.Active && b.ALLOWFORCEDEBITREPAYMENT == false && a.LOANID == loanId
                          && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                         group a by new
                         {
                             b.PRODUCTID,
                             b.BRANCHID,
                             b.COMPANYID,
                             b.CURRENCYID,
                             b.EXCHANGERATE,
                             a.INTERESTRATE,
                             a.LOANID,
                             b.CASAACCOUNTID,
                             b.LOANREFERENCENUMBER,
                             applicationDate
                         } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             interestRate = groupedQ.Key.INTERESTRATE,
                             paymentDate = groupedQ.Key.applicationDate,
                             loanId = groupedQ.Key.LOANID,
                             casaAccountId = groupedQ.Key.CASAACCOUNTID,
                             loanRefNo = groupedQ.Key.LOANREFERENCENUMBER,
                             periodInterestAmount = groupedQ.Sum(i => i.PERIODINTERESTAMOUNT),
                             periodPrincipalAmount = groupedQ.Sum(i => i.PERIODPRINCIPALAMOUNT),
                             totalAmount = groupedQ.Sum(i => i.PERIODINTERESTAMOUNT) + groupedQ.Sum(i => i.PERIODPRINCIPALAMOUNT),
                         }).ToList();

            List<TBL_LOAN_PAST_DUE> transPastDue = new List<TBL_LOAN_PAST_DUE>();

            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                var pastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId);
                var casabalance = casa.AVAILABLEBALANCE;
                if (casabalance >= item.totalAmount)
                {

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);
                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();


                    pastDue.LOANID = item.loanId;
                    pastDue.PASTDUECODE = pastDueCode;
                    pastDue.CREDITAMOUNT = 0;
                    pastDue.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDue.DEBITAMOUNT = Math.Abs(casabalance - item.totalAmount);
                    pastDue.DATE = item.paymentDate;
                    pastDue.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDue.PARENT_PASTDUECODE = item.loanRefNo;

                    transPastDue.Add(pastDue);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DEBITAMOUNT, product.PRINCIPALBALANCEGL.Value, "partial principal repayment"));
                    // place lien on the customer account on partial principal
                    financeTransaction.PostTransaction(inputTransactions);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDue.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDue.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDue.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDue.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Restructure Date";

                    var lienReference = casaLien.PlaceLien(lien);

                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    TBL_LOAN_PAST_DUE pastDueInterest = new TBL_LOAN_PAST_DUE();

                    pastDueInterest.LOANID = item.loanId;
                    pastDueInterest.PASTDUECODE = pastDueCode;
                    pastDueInterest.CREDITAMOUNT = 0;
                    pastDueInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDueInterest.DEBITAMOUNT = Math.Abs(casabalance - item.periodInterestAmount);
                    pastDueInterest.DATE = item.paymentDate;
                    pastDueInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.PARENT_PASTDUECODE = item.loanRefNo;

                    transPastDue.Add(pastDueInterest);

                    TBL_LOAN_PAST_DUE pastDuePrincipal = new TBL_LOAN_PAST_DUE();

                    pastDuePrincipal.LOANID = item.loanId;
                    pastDuePrincipal.PASTDUECODE = pastDueCode;
                    pastDuePrincipal.CREDITAMOUNT = 0;
                    pastDuePrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DEBITAMOUNT = item.periodPrincipalAmount;
                    pastDuePrincipal.DATE = item.paymentDate;
                    pastDuePrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.PARENT_PASTDUECODE = item.loanRefNo;

                    transPastDue.Add(pastDuePrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DEBITAMOUNT, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment"));
                    financeTransaction.PostTransaction(inputTransactions);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDueInterest.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDueInterest.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.InterestRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDueInterest.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDueInterest.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.InterestRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Restructure Date";

                    var lienReference = casaLien.PlaceLien(lien);

                    //var dataP = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDuePrincipal.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDuePrincipal.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(dataP);

                    CasaLienViewModel lienPrincipal = new CasaLienViewModel();

                    lienPrincipal.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lienPrincipal.sourceReferenceNumber = pastDuePrincipal.PARENT_PASTDUECODE;
                    lienPrincipal.lienAmount = pastDuePrincipal.DEBITAMOUNT;
                    lienPrincipal.branchId = item.branchId;
                    lienPrincipal.companyId = item.companyId;
                    lienPrincipal.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lienPrincipal.createdBy = (int)SystemStaff.System;
                    lienPrincipal.description = "lien placed due to Account not funded at Restructure Date";

                    casaLien.PlaceLien(lienPrincipal);


                }
                else if (casabalance <= 0)
                {
                    TBL_LOAN_PAST_DUE pastDueInterest = new TBL_LOAN_PAST_DUE();

                    pastDueInterest.LOANID = item.loanId;
                    pastDueInterest.PASTDUECODE = pastDueCode;
                    pastDueInterest.CREDITAMOUNT = 0;
                    pastDueInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDueInterest.DEBITAMOUNT = Math.Abs(item.periodInterestAmount);
                    pastDueInterest.DATE = item.paymentDate;
                    pastDueInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDueInterest.PRODUCTTYPEID = item.productId;

                    transPastDue.Add(pastDueInterest);

                    TBL_LOAN_PAST_DUE pastDuePrincipal = new TBL_LOAN_PAST_DUE();

                    pastDuePrincipal.LOANID = item.loanId;
                    pastDuePrincipal.PASTDUECODE = pastDueCode;
                    pastDuePrincipal.CREDITAMOUNT = 0;
                    pastDuePrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DEBITAMOUNT = Math.Abs(item.periodPrincipalAmount);
                    pastDuePrincipal.DATE = item.paymentDate;
                    pastDuePrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDuePrincipal.PRODUCTTYPEID = item.productId;

                    transPastDue.Add(pastDuePrincipal);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDueInterest.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDueInterest.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.InterestRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDueInterest.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDueInterest.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.InterestRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Restructure Date";

                    var lienReference = casaLien.PlaceLien(lien);

                    //var dataP = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDuePrincipal.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDuePrincipal.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(dataP);

                    CasaLienViewModel lienPrincipal = new CasaLienViewModel();

                    lienPrincipal.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lienPrincipal.sourceReferenceNumber = pastDuePrincipal.PARENT_PASTDUECODE;
                    lienPrincipal.lienAmount = pastDuePrincipal.DEBITAMOUNT;
                    lienPrincipal.branchId = item.branchId;
                    lienPrincipal.companyId = item.companyId;
                    lienPrincipal.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lienPrincipal.createdBy = (int)SystemStaff.System;
                    lienPrincipal.description = "lien placed due to Account not funded at Restructure Date";

                    casaLien.PlaceLien(lienPrincipal);

                }
            }

            this.context.TBL_LOAN_PAST_DUE.AddRange(transPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate)

        {
            var systemDate = generalSetup.GetApplicationDate();

            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where b.LOANSTATUSID == (short)LoanStatusEnum.Active && b.ALLOWFORCEDEBITREPAYMENT == false
                          && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                         group a by new
                         {
                             b.PRODUCTID,
                             b.BRANCHID,
                             b.COMPANYID,
                             b.CURRENCYID,
                             b.EXCHANGERATE,
                             a.INTERESTRATE,
                             a.LOANID,
                             b.CASAACCOUNTID,
                             b.LOANREFERENCENUMBER,
                             applicationDate
                         } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             interestRate = groupedQ.Key.INTERESTRATE,
                             paymentDate = groupedQ.Key.applicationDate,
                             loanId = groupedQ.Key.LOANID,
                             casaAccountId = groupedQ.Key.CASAACCOUNTID,
                             loanRefNo = groupedQ.Key.LOANREFERENCENUMBER,
                             periodInterestAmount = groupedQ.Sum(i => i.PERIODINTERESTAMOUNT),
                             periodPrincipalAmount = groupedQ.Sum(i => i.PERIODPRINCIPALAMOUNT),
                             totalAmount = groupedQ.Sum(i => i.PERIODINTERESTAMOUNT) + groupedQ.Sum(i => i.PERIODPRINCIPALAMOUNT),
                         }).ToList();

            List<TBL_LOAN_PAST_DUE> transPastDue = new List<TBL_LOAN_PAST_DUE>();

            foreach (var item in model)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                var pastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId);
                var casabalance = casa.AVAILABLEBALANCE;
                if (casabalance >= item.totalAmount)
                {

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);
                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();


                    pastDue.LOANID = item.loanId;
                    pastDue.PASTDUECODE = pastDueCode;
                    pastDue.CREDITAMOUNT = 0;
                    pastDue.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDue.DEBITAMOUNT = Math.Abs(casabalance - item.totalAmount);
                    pastDue.DATE = item.paymentDate;
                    pastDue.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDue.PARENT_PASTDUECODE = item.loanRefNo;

                    transPastDue.Add(pastDue);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DEBITAMOUNT, product.PRINCIPALBALANCEGL.Value, "partial principal repayment"));
                    // place lien on the customer account on partial principal
                    financeTransaction.PostTransaction(inputTransactions);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDue.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDue.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDue.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDue.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Restructure Date";

                    var lienReference = casaLien.PlaceLien(lien);

                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    TBL_LOAN_PAST_DUE pastDueInterest = new TBL_LOAN_PAST_DUE();

                    pastDueInterest.LOANID = item.loanId;
                    pastDueInterest.PASTDUECODE = pastDueCode;
                    pastDueInterest.CREDITAMOUNT = 0;
                    pastDueInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDueInterest.DEBITAMOUNT = Math.Abs(casabalance - item.periodInterestAmount);
                    pastDueInterest.DATE = item.paymentDate;
                    pastDueInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.PARENT_PASTDUECODE = item.loanRefNo;

                    transPastDue.Add(pastDueInterest);

                    TBL_LOAN_PAST_DUE pastDuePrincipal = new TBL_LOAN_PAST_DUE();

                    pastDuePrincipal.LOANID = item.loanId;
                    pastDuePrincipal.PASTDUECODE = pastDueCode;
                    pastDuePrincipal.CREDITAMOUNT = 0;
                    pastDuePrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DEBITAMOUNT = item.periodPrincipalAmount;
                    pastDuePrincipal.DATE = item.paymentDate;
                    pastDuePrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.PARENT_PASTDUECODE = item.loanRefNo;

                    transPastDue.Add(pastDuePrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DEBITAMOUNT, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment"));
                    financeTransaction.PostTransaction(inputTransactions);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDueInterest.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDueInterest.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.InterestRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDueInterest.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDueInterest.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.InterestRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Restructure Date";

                    var lienReference = casaLien.PlaceLien(lien);

                    //var dataP = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDuePrincipal.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDuePrincipal.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(dataP);

                    CasaLienViewModel lienPrincipal = new CasaLienViewModel();

                    lienPrincipal.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lienPrincipal.sourceReferenceNumber = pastDuePrincipal.PARENT_PASTDUECODE;
                    lienPrincipal.lienAmount = pastDuePrincipal.DEBITAMOUNT;
                    lienPrincipal.branchId = item.branchId;
                    lienPrincipal.companyId = item.companyId;
                    lienPrincipal.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lienPrincipal.createdBy = (int)SystemStaff.System;
                    lienPrincipal.description = "lien placed due to Account not funded at Restructure Date";

                    lienReference = casaLien.PlaceLien(lienPrincipal);

                }
                else if (casabalance <= 0)
                {
                    TBL_LOAN_PAST_DUE pastDueInterest = new TBL_LOAN_PAST_DUE();

                    pastDueInterest.LOANID = item.loanId;
                    pastDueInterest.PASTDUECODE = pastDueCode;
                    pastDueInterest.CREDITAMOUNT = 0;
                    pastDueInterest.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDueInterest.DEBITAMOUNT = Math.Abs(item.periodInterestAmount);
                    pastDueInterest.DATE = item.paymentDate;
                    pastDueInterest.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDueInterest.PRODUCTTYPEID = item.productId;

                    transPastDue.Add(pastDueInterest);

                    TBL_LOAN_PAST_DUE pastDuePrincipal = new TBL_LOAN_PAST_DUE();

                    pastDuePrincipal.LOANID = item.loanId;
                    pastDuePrincipal.PASTDUECODE = pastDueCode;
                    pastDuePrincipal.CREDITAMOUNT = 0;
                    pastDuePrincipal.DESCRIPTION = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DEBITAMOUNT = Math.Abs(item.periodPrincipalAmount);
                    pastDuePrincipal.DATE = item.paymentDate;
                    pastDuePrincipal.TRANSACTIONTYPEID = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.PARENT_PASTDUECODE = item.loanRefNo;
                    pastDuePrincipal.PRODUCTTYPEID = item.productId;

                    transPastDue.Add(pastDuePrincipal);

                    //var data = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDueInterest.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDueInterest.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.InterestRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(data);

                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDueInterest.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDueInterest.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = (short)LienTypeEnum.InterestRepayment;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Restructure Date";

                    var lienReference = casaLien.PlaceLien(lien);

                    //var dataP = new TBL_CASA_LIEN
                    //{
                    //    PRODUCTACCOUNTNUMBER = casa.PRODUCTACCOUNTNUMBER,
                    //    LIENREFERENCENUMBER = CommonHelpers.GenerateRandomDigitCode(10),
                    //    SOURCEREFERENCENUMBER = pastDuePrincipal.PARENT_PASTDUECODE,
                    //    BRANCHID = item.branchId,
                    //    COMPANYID = item.companyId,
                    //    LIENCREDITAMOUNT = pastDuePrincipal.DEBITAMOUNT,
                    //    LIENDEBITAMOUNT = 0,
                    //    LIENTYPEID = (short)LienTypeEnum.PrincipalRepayment,
                    //    CREATEDBY = (int)SystemStaff.System,
                    //    DESCRIPTION = "lien placed due to Account not funded at Restructure Date", // model.description,
                    //    DATECREATED = generalSetup.GetApplicationDate()

                    //};

                    //context.TBL_CASA_LIEN.Add(dataP);

                    CasaLienViewModel lienPrincipal = new CasaLienViewModel();

                    lienPrincipal.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lienPrincipal.sourceReferenceNumber = pastDuePrincipal.PARENT_PASTDUECODE;
                    lienPrincipal.lienAmount = pastDuePrincipal.DEBITAMOUNT;
                    lienPrincipal.branchId = item.branchId;
                    lienPrincipal.companyId = item.companyId;
                    lienPrincipal.lienTypeId = (short)LienTypeEnum.PrincipalRepayment;
                    lienPrincipal.createdBy = (int)SystemStaff.System;
                    lienPrincipal.description = "lien placed due to Account not funded at Restructure Date";

                    casaLien.PlaceLien(lienPrincipal);

                }
            }

            this.context.TBL_LOAN_PAST_DUE.AddRange(transPastDue);

            context.SaveChanges();
            return model;
        }

        #endregion

        #region Periodic  Operation

        public IEnumerable<LoanViewModel> ProcessIntervalFeeandCommissionPosting(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_FEE
                         join b in context.TBL_PRODUCT_TYPE on a.PRODUCTTYPEID equals b.PRODUCTTYPEID
                         join c in context.TBL_LOAN_FEE_SCHEDULE on a.LOANCHARGEFEEID equals c.LOANCHARGEFEEID
                         join d in context.TBL_LOAN on a.LOANID equals d.TERMLOANID
                         join e in context.TBL_CASA on d.CASAACCOUNTID equals e.CASAACCOUNTID
                         where c.FEEDATE == DbFunctions.TruncateTime(applicationDate) && a.ISRECURRING == true
                         && b.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan
                         select new LoanViewModel()
                         {
                             productId = d.PRODUCTID,
                             branchId = d.BRANCHID,
                             companyId = d.COMPANYID,
                             currencyId = d.CURRENCYID,
                             exchangeRate = d.EXCHANGERATE,
                             interestRate = d.INTERESTRATE,
                             paymentDate = applicationDate,
                             loanId = a.LOANID,
                             totalAmount = c.FEEAMOUNT,
                             casaAccountId = e.CASAACCOUNTID,
                             loanReferenceNumber = d.LOANREFERENCENUMBER,
                             chargeFeeId = a.CHARGEFEEID


                         }).ToList();

            //List<tbl_Loan_Force_Debit> transForceDebit = new List<tbl_Loan_Force_Debit>();


            foreach (var item in model)
            {
                //var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);

                //var feeCode = CommonHelpers.GenerateRandomDigitCode(10);

                financeTransaction.PostBuildLoanChargeFeesPosting(item);

                context.SaveChanges();

            }
            return model;
        }

        public IEnumerable<LimitSuspensionViewModel> ProcessNPLByBranchSuspension()
        {
            var model = (from a in context.TBL_LOAN
                         join b in context.TBL_BRANCH on a.BRANCHID equals b.BRANCHID
                         join c in context.TBL_LIMIT_DETAIL on b.BRANCHID equals c.TARGETID
                         join d in context.TBL_LIMIT on c.LIMITID equals d.LIMITID
                         where b.BRANCHID == c.TARGETID && c.LIMITID == d.LIMITID
                         && d.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
                         && c.LIMITTYPEID == (int)LimitType.Branch
                         group a by new
                         { a.BRANCHID, c.LIMITID, c.MAXIMUMVALUE } into groupedQ
                         select new LimitSuspensionViewModel()
                         {
                             limitId = groupedQ.Key.LIMITID,
                             limitAmount = groupedQ.Key.MAXIMUMVALUE,
                             branchId = groupedQ.Key.BRANCHID,
                             amount = groupedQ.Sum(i => (i.PRINCIPALAMOUNT)),

                         }).ToList();


            foreach (var item in model)
            {
                if (item.amount <= item.limitAmount)
                {
                    TBL_BRANCH result = (from p in context.TBL_BRANCH
                                         where p.BRANCHID == item.branchId
                                         select p).SingleOrDefault();

                    result.NPL_LIMITEXCEEDED = true;


                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LimitSuspensionViewModel> ProcessNPLByRMSuspension()
        {
            var model = (from a in context.TBL_LOAN
                         join b in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals b.STAFFID
                         join c in context.TBL_LIMIT_DETAIL on b.STAFFID equals c.TARGETID
                         join d in context.TBL_LIMIT on c.LIMITID equals d.LIMITID
                         where b.STAFFID == c.TARGETID && c.LIMITID == d.LIMITID
                         && d.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
                         && c.LIMITTYPEID == (int)LimitType.RelationshipManager
                         group a by new
                         { a.RELATIONSHIPMANAGERID, c.LIMITID, c.MAXIMUMVALUE } into groupedQ
                         select new LimitSuspensionViewModel()
                         {
                             limitId = groupedQ.Key.LIMITID,
                             limitAmount = groupedQ.Key.MAXIMUMVALUE,
                             staffId = groupedQ.Key.RELATIONSHIPMANAGERID,
                             amount = groupedQ.Sum(i => (i.PRINCIPALAMOUNT)),

                         }).ToList();


            foreach (var item in model)
            {
                if (item.amount >= item.limitAmount)
                {
                    TBL_STAFF result = (from p in context.TBL_STAFF
                                        where p.STAFFID == item.staffId
                                        select p).SingleOrDefault();

                    result.NPL_LIMITEXCEEDED = true;


                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCleanUp(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join c in context.TBL_LOAN_COVENANT_DETAIL on a.REVOLVINGLOANID equals c.LOANID
                         join d in context.TBL_LOAN_COVENANT_TYPE on c.COVENANTTYPEID equals d.COVENANTTYPEID
                         where a.CASAACCOUNTID == b.CASAACCOUNTID && a.REVOLVINGLOANID == c.LOANID
                         && c.COVENANTTYPEID == d.COVENANTTYPEID && c.NEXTCOVENANTDATE == DbFunctions.TruncateTime(applicationDate)
                          && d.COVENANTTYPEID == (short)LoanCovenantTypeEnum.Cleanup
                         select new LoanCovenantDetailViewModel()
                         {
                             loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                             loanId = a.REVOLVINGLOANID,
                             loanRef = a.LOANREFERENCENUMBER,
                             casaId = a.CASAACCOUNTID,
                             frequencyTypeId = c.FREQUENCYTYPEID,

                         }).ToList();


            foreach (var item in model)
            {
                var casabalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaId).AVAILABLEBALANCE;
                if (casabalance >= 0)
                {
                    TBL_LOAN_COVENANT_DETAIL result = (from p in context.TBL_LOAN_COVENANT_DETAIL
                                                       where p.LOANCOVENANTDETAILID == item.loanCovenantDetailId
                                                       select p).SingleOrDefault();

                    result.COVENANTDATE = applicationDate;
                    result.NEXTCOVENANTDATE = applicationDate.AddMonths((short)item.frequencyTypeId);///check if is monthly otherwise pick from setup


                    context.SaveChanges();
                }
                else
                {
                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CASAACCOUNTID == item.casaId
                                       select p).SingleOrDefault();

                    result.POSTNOSTATUSID = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCovenant(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join c in context.TBL_LOAN_COVENANT_DETAIL on a.REVOLVINGLOANID equals c.LOANID
                         join d in context.TBL_LOAN_COVENANT_TYPE on c.COVENANTTYPEID equals d.COVENANTTYPEID
                         where a.CASAACCOUNTID == b.CASAACCOUNTID && a.REVOLVINGLOANID == c.LOANID
                         && c.COVENANTTYPEID == d.COVENANTTYPEID && c.NEXTCOVENANTDATE == DbFunctions.TruncateTime(applicationDate)
                         && d.COVENANTTYPEID == (short)LoanCovenantTypeEnum.Turnover
                         select new LoanCovenantDetailViewModel()
                         {
                             loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                             loanId = a.REVOLVINGLOANID,
                             loanRef = a.LOANREFERENCENUMBER,
                             casaId = a.CASAACCOUNTID,
                             covenantAmount = c.COVENANTAMOUNT,
                             covenantDate = c.COVENANTDATE,
                             frequencyTypeId = c.FREQUENCYTYPEID,

                         }).ToList();


            foreach (var item in model)
            {
                var casabalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaId).AVAILABLEBALANCE;///pick from transaction table in finnacle to get inflows pass Date Range
                if (casabalance >= item.covenantAmount)
                {
                    TBL_LOAN_COVENANT_DETAIL result = (from p in context.TBL_LOAN_COVENANT_DETAIL
                                                       where p.LOANCOVENANTDETAILID == item.loanCovenantDetailId
                                                       select p).SingleOrDefault();

                    result.COVENANTDATE = applicationDate;
                    result.NEXTCOVENANTDATE = applicationDate.AddMonths((short)item.frequencyTypeId);///check if is monthly otherwise pick from setup


                    context.SaveChanges();
                }
                else
                {
                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CASAACCOUNTID == item.casaId
                                       select p).SingleOrDefault();

                    result.POSTNOSTATUSID = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessLPOExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join e in context.TBL_PRODUCT on a.PRODUCTID equals e.PRODUCTID
                         join f in context.TBL_PRODUCT_TYPE on e.PRODUCTTYPEID equals f.PRODUCTTYPEID
                         where a.CASAACCOUNTID == b.CASAACCOUNTID && a.PRODUCTID == e.PRODUCTID
                         && e.PRODUCTTYPEID == f.PRODUCTTYPEID && a.EFFECTIVEDATE <= DbFunctions.TruncateTime(applicationDate)
                         && e.PRODUCTTYPEID == (short)LoanProductTypeEnum.LPO
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             loanRef = a.LOANREFERENCENUMBER,
                             casaId = a.CASAACCOUNTID,
                             effectiveDate = a.EFFECTIVEDATE,
                             maximumDrawDownDuration = (int)e.MAXIMUMDRAWDOWNDURATION,//(int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         }).ToList();


            foreach (var item in model)
            {
                var maxDay = (applicationDate - item.effectiveDate).TotalDays;
                if (maxDay >= item.maximumDrawDownDuration)
                {
                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CASAACCOUNTID == item.casaId
                                       select p).SingleOrDefault();

                    result.POSTNOSTATUSID = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessCFFExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join e in context.TBL_PRODUCT on a.PRODUCTID equals e.PRODUCTID
                         join f in context.TBL_PRODUCT_TYPE on e.PRODUCTTYPEID equals f.PRODUCTTYPEID
                         where a.CASAACCOUNTID == b.CASAACCOUNTID && a.PRODUCTID == e.PRODUCTID
                         && e.PRODUCTTYPEID == f.PRODUCTTYPEID && a.EFFECTIVEDATE <= DbFunctions.TruncateTime(applicationDate)
                         && e.PRODUCTTYPEID == (short)LoanProductTypeEnum.CFF
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             loanRef = a.LOANREFERENCENUMBER,
                             casaId = a.CASAACCOUNTID,
                             effectiveDate = a.EFFECTIVEDATE,
                             maximumDrawDownDuration = (int)e.MAXIMUMDRAWDOWNDURATION,//(int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         }).ToList();


            foreach (var item in model)
            {
                var maxDay = (applicationDate - item.effectiveDate).TotalDays;
                if (maxDay >= item.maximumDrawDownDuration)
                {
                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CASAACCOUNTID == item.casaId
                                       select p).SingleOrDefault();

                    result.POSTNOSTATUSID = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessIDFExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_CASA on a.CASAACCOUNTID equals b.CASAACCOUNTID
                         join e in context.TBL_PRODUCT on a.PRODUCTID equals e.PRODUCTID
                         join f in context.TBL_PRODUCT_TYPE on e.PRODUCTTYPEID equals f.PRODUCTTYPEID
                         where a.CASAACCOUNTID == b.CASAACCOUNTID && a.PRODUCTID == e.PRODUCTID
                         && e.PRODUCTTYPEID == f.PRODUCTTYPEID && a.EFFECTIVEDATE <= DbFunctions.TruncateTime(applicationDate)
                         && e.PRODUCTTYPEID == (short)LoanProductTypeEnum.IDF
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             loanRef = a.LOANREFERENCENUMBER,
                             casaId = a.CASAACCOUNTID,
                             effectiveDate = a.EFFECTIVEDATE,
                             maximumDrawDownDuration = (int)e.MAXIMUMDRAWDOWNDURATION,//(int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         }).ToList();


            foreach (var item in model)
            {
                var maxDay = (applicationDate - item.effectiveDate).TotalDays;
                if (maxDay >= item.maximumDrawDownDuration)
                {
                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CASAACCOUNTID == item.casaId
                                       select p).SingleOrDefault();

                    result.POSTNOSTATUSID = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool ProcessChargeReversal(LoanChargeFeeViewModel feeInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            feeInput.feeAmountDiff = feeInput.newFeeAmount - feeInput.feeAmount;
            feeInput.date = applicationDate;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildChargeReversalPosting(feeInput));

            //financeTransaction.PostTransaction(inputTransactions);
            AddChargeReversal(feeInput, applicationDate, staffId);


            output = true;

            return output;
        }

        public bool AddChargeReversal(LoanChargeFeeViewModel model, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var productType = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId && x.COMPANYID == model.companyId).PRODUCTTYPEID;

            TBL_LOAN_FEE loanFee = new TBL_LOAN_FEE();

            loanFee.LOANID = model.loanId;
            loanFee.PRODUCTTYPEID = productType;
            loanFee.CHARGEFEEID = model.chargeFeeId;
            loanFee.FEERATEVALUE = model.feeRateValue;
            loanFee.FEEDEPENDENTAMOUNT = model.feeDependentAmount;
            loanFee.FEEAMOUNT = model.feeAmountDiff;
            loanFee.ISINTEGRALFEE = false;
            loanFee.ISRECURRING = false;
            loanFee.RECURRINGPAYMENTDAY = 0;
            loanFee.CREATEDBY = staffId;
            loanFee.DATETIMECREATED = applicationDate;
            loanFee.LASTUPDATEDBY = null;
            loanFee.DATETIMEUPDATED = systemDate;
            loanFee.DELETED = false;
            loanFee.DELETEDBY = null;
            loanFee.DATETIMEDELETED = systemDate;

            this.context.TBL_LOAN_FEE.Add(loanFee); ////change to Temp table

            context.SaveChanges();
            output = true;

            return output;
        }

        public bool AddCASAOverdraft(int casaId, decimal amount, string description)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            TBL_CASA_OVERDRAFT casaOverdraft = new TBL_CASA_OVERDRAFT();

            casaOverdraft.CASAACCOUNTID = casaId;
            casaOverdraft.EFFECTIVEDATE = systemDate;
            casaOverdraft.CREDITAMOUNT = amount;
            casaOverdraft.DEBITAMOUNT = 0;
            casaOverdraft.DESCRIPTION = description;
            casaOverdraft.CREATEDBY = (int)SystemStaff.System;
            casaOverdraft.DATECREATED = systemDate;
            this.context.TBL_CASA_OVERDRAFT.Add(casaOverdraft);

            context.SaveChanges();
            output = true;

            return output;
        }

        public void OverdraftTopUp(int loanId, decimal amount)
        {
            var systemDate = generalSetup.GetApplicationDate();
            DeleteLoanExist(loanId);
            ArchiveOverDraft(loanId);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                         where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                            // casaAccountId2 = a.CASAACCOUNTID2,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             exchangeRate = a.EXCHANGERATE,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                             subSectorId = a.SUBSECTORID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = (double)b.INTERATERATE,
                             effectiveDate = b.EFFECTIVEDATE,
                             maturityDate = (DateTime)b.MATURITYDATE,
                             bookingDate = DateTime.Today,
                             overdraftLimit = (decimal)b.OVERDRAFTTOPUP,
                             //disbursedAmount = a.DISBURSED_AMOUNT,
                             //interestAmount = a.INTEREST_AMOUNT,
                             pastDuePrincipal = a.PASTDUEPRINCIPAL,
                             pastDueInterest = a.PASTDUEINTEREST,
                             interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                             interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                             penalChargeAmount = a.PENALCHARGEAMOUNT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = (int)a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = b.OPERATIONTYPEID,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             dayCountConventionId = a.DAYCOUNTCONVENTIONID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = DateTime.Today,

                         }).ToList();

            List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



            foreach (var item in model)
            {
                item.productTypeId = 6;
                var loanReferenceNumber = loan.GenerateLoanReferenceNumber(item.customerId, item.productId, item.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = item.customerId;
                addOverDraft.PRODUCTID = item.productId;
                addOverDraft.COMPANYID = item.companyId;
                addOverDraft.CASAACCOUNTID = item.casaAccountId;
                //addOverDraft.CASAACCOUNTID2 = item.casaAccountId2;
                addOverDraft.BRANCHID = item.branchId;
                addOverDraft.CURRENCYID = item.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = item.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = item.loanReferenceNumber;
                addOverDraft.SUBSECTORID = item.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addOverDraft.MISCODE = item.misCode;
                addOverDraft.TEAMMISCODE = item.teamMiscode;
                addOverDraft.INTERESTRATE = item.interestRate;
                addOverDraft.EFFECTIVEDATE = item.effectiveDate;
                addOverDraft.MATURITYDATE = item.maturityDate;
                addOverDraft.BOOKINGDATE = item.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = item.overdraftLimit;
                //addOverDraft.DISBURSED_AMOUNT = item.disbursedAmount;
                //addOverDraft.INTEREST_AMOUNT = item.interestAmount;
                addOverDraft.APPROVALSTATUSID = item.approvalStatusId;
                addOverDraft.APPROVEDBY = item.approvedBy;
                addOverDraft.APPROVERCOMMENT = item.approverComment;
                addOverDraft.DATEAPPROVED = item.dateApproved;
                addOverDraft.LOANSTATUSID = item.loanStatusId;
                addOverDraft.ISDISBURSED = item.isDisbursed;
                addOverDraft.DISBURSEDBY = item.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = item.disburserComment;
                addOverDraft.DISBURSEDATE = item.disburseDate;
                addOverDraft.OPERATIONID = item.operationId;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = item.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = item.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)item.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addOverDraft.NPLDATE = item.nplDate;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;

                overDraft.Add(addOverDraft);
            }

            this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

            context.SaveChanges();

            //var desc = "Overdraft Top";

            //TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
            //                             where p.REVOLVINGLOANID == loanId
            //                             && p.LOANSTATUSID == (short)LoanStatusEnum.Active
            //                             select p).SingleOrDefault();

            //result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT + amount;


           // AddCASAOverdraft(result.CASAACCOUNTID, amount, desc);

            //context.SaveChanges();
        }

        public void OverdraftRenewal(int loanId, decimal amount)
        {
            var systemDate = generalSetup.GetApplicationDate();
            DeleteLoanExist(loanId);
            ArchiveOverDraft(loanId);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                         where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                             //casaAccountId2 = a.CASAACCOUNTID2,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             exchangeRate = a.EXCHANGERATE,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                             subSectorId = a.SUBSECTORID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = (double)b.INTERATERATE,
                             effectiveDate = b.EFFECTIVEDATE,
                             maturityDate = (DateTime)b.MATURITYDATE,
                             bookingDate = DateTime.Today,
                             overdraftLimit = (decimal)b.OVERDRAFTTOPUP,
                             //disbursedAmount = a.DISBURSED_AMOUNT,
                             //interestAmount = a.INTEREST_AMOUNT,
                             pastDuePrincipal = a.PASTDUEPRINCIPAL,
                             pastDueInterest = a.PASTDUEINTEREST,
                             interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                             interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                             penalChargeAmount = a.PENALCHARGEAMOUNT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = (int)a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = b.OPERATIONTYPEID,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             dayCountConventionId = a.DAYCOUNTCONVENTIONID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = DateTime.Today,

                         }).ToList();

            List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



            foreach (var item in model)
            {
                item.productTypeId = 6;
                var loanReferenceNumber = loan.GenerateLoanReferenceNumber(item.customerId, item.productId, item.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = item.customerId;
                addOverDraft.PRODUCTID = item.productId;
                addOverDraft.COMPANYID = item.companyId;
                addOverDraft.CASAACCOUNTID = item.casaAccountId;
                //addOverDraft.CASAACCOUNTID2 = item.casaAccountId2;
                addOverDraft.BRANCHID = item.branchId;
                addOverDraft.CURRENCYID = item.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = item.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = item.loanReferenceNumber;
                addOverDraft.SUBSECTORID = item.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addOverDraft.MISCODE = item.misCode;
                addOverDraft.TEAMMISCODE = item.teamMiscode;
                addOverDraft.INTERESTRATE = item.interestRate;
                addOverDraft.EFFECTIVEDATE = item.effectiveDate;
                addOverDraft.MATURITYDATE = item.maturityDate;
                addOverDraft.BOOKINGDATE = item.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = item.overdraftLimit;
                //addOverDraft.DISBURSED_AMOUNT = item.disbursedAmount;
                //addOverDraft.INTEREST_AMOUNT = item.interestAmount;
                addOverDraft.APPROVALSTATUSID = item.approvalStatusId;
                addOverDraft.APPROVEDBY = item.approvedBy;
                addOverDraft.APPROVERCOMMENT = item.approverComment;
                addOverDraft.DATEAPPROVED = item.dateApproved;
                addOverDraft.LOANSTATUSID = item.loanStatusId;
                addOverDraft.ISDISBURSED = item.isDisbursed;
                addOverDraft.DISBURSEDBY = item.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = item.disburserComment;
                addOverDraft.DISBURSEDATE = item.disburseDate;
                addOverDraft.OPERATIONID = item.operationId;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = item.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = item.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)item.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addOverDraft.NPLDATE = item.nplDate;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;

                overDraft.Add(addOverDraft);
            }

            this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

            context.SaveChanges();

            //var desc = "Overdraft Top";

            //TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
            //                             where p.REVOLVINGLOANID == loanId
            //                             && p.LOANSTATUSID == (short)LoanStatusEnum.Active
            //                             select p).SingleOrDefault();

            //result.OVERDRAFTLIMIT = amount;


            // AddCASAOverdraft(result.CASAACCOUNTID, amount, desc);

            //context.SaveChanges();
        }

        public void OverdraftExtension(int loanId, decimal amount)
        {
            var systemDate = generalSetup.GetApplicationDate();
            DeleteLoanExist(loanId);
            ArchiveOverDraft(loanId);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                         where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                            // casaAccountId2 = a.CASAACCOUNTID2,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             exchangeRate = a.EXCHANGERATE,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                             subSectorId = a.SUBSECTORID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = (double)b.INTERATERATE,
                             effectiveDate = b.EFFECTIVEDATE,
                             maturityDate = (DateTime)b.MATURITYDATE,
                             bookingDate = DateTime.Today,
                             overdraftLimit = (decimal)b.OVERDRAFTTOPUP,
                            // disbursedAmount = a.DISBURSED_AMOUNT,
                             //interestAmount = a.INTEREST_AMOUNT,
                             pastDuePrincipal = a.PASTDUEPRINCIPAL,
                             pastDueInterest = a.PASTDUEINTEREST,
                             interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                             interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                             penalChargeAmount = a.PENALCHARGEAMOUNT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = (int)a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = b.OPERATIONTYPEID,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             dayCountConventionId = a.DAYCOUNTCONVENTIONID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = DateTime.Today,

                         }).ToList();

            List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



            foreach (var item in model)
            {
                var loanReferenceNumber = loan.GenerateLoanReferenceNumber(item.customerId, item.productId, item.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = item.customerId;
                addOverDraft.PRODUCTID = item.productId;
                addOverDraft.COMPANYID = item.companyId;
                addOverDraft.CASAACCOUNTID = item.casaAccountId;
                //addOverDraft.CASAACCOUNTID2 = item.casaAccountId2;
                addOverDraft.BRANCHID = item.branchId;
                addOverDraft.CURRENCYID = item.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = item.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = item.loanReferenceNumber;
                addOverDraft.SUBSECTORID = item.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addOverDraft.MISCODE = item.misCode;
                addOverDraft.TEAMMISCODE = item.teamMiscode;
                addOverDraft.INTERESTRATE = item.interestRate;
                addOverDraft.EFFECTIVEDATE = item.effectiveDate;
                addOverDraft.MATURITYDATE = item.maturityDate;
                addOverDraft.BOOKINGDATE = item.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = item.overdraftLimit;
                //addOverDraft.DISBURSED_AMOUNT = item.disbursedAmount;
                //addOverDraft.INTEREST_AMOUNT = item.interestAmount;
                addOverDraft.APPROVALSTATUSID = item.approvalStatusId;
                addOverDraft.APPROVEDBY = item.approvedBy;
                addOverDraft.APPROVERCOMMENT = item.approverComment;
                addOverDraft.DATEAPPROVED = item.dateApproved;
                addOverDraft.LOANSTATUSID = item.loanStatusId;
                addOverDraft.ISDISBURSED = item.isDisbursed;
                addOverDraft.DISBURSEDBY = item.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = item.disburserComment;
                addOverDraft.DISBURSEDATE = item.disburseDate;
                addOverDraft.OPERATIONID = item.operationId;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = item.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = item.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)item.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addOverDraft.NPLDATE = item.nplDate;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;

                overDraft.Add(addOverDraft);
            }

            this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

            context.SaveChanges();

            //var desc = "Overdraft Top";

            //TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
            //                             where p.REVOLVINGLOANID == loanId
            //                             && p.LOANSTATUSID == (short)LoanStatusEnum.Active
            //                             select p).SingleOrDefault();

            //result.OVERDRAFTLIMIT = amount;



            // AddCASAOverdraft(result.CASAACCOUNTID, amount, desc);

            context.SaveChanges();
        }

        public void ChangeOperativeAccount(int casaAccountId, int newCasaAccountId)
        {


            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.CASAACCOUNTID == casaAccountId
                                && p.LOANSTATUSID == (short)LoanStatusEnum.Active
                               select p).SingleOrDefault();
            var casa = this.context.TBL_CASA.Where(x => x.CASAACCOUNTID == newCasaAccountId && x.ACCOUNTSTATUSID == (short)CASAAccountStatusEnum.Active).FirstOrDefault().CASAACCOUNTID;

            result.CASAACCOUNTID = casa;
            context.SaveChanges();
        }

        public void SubAllocation(int loanId, decimal amount, DateTime applicationDate, int staffId)
        {

            var systemDate = generalSetup.GetApplicationDate();
            DeleteLoanExist(loanId);
            ArchiveOverDraft(loanId);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                         where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                             casaAccountId2 = b.CASA_ACCOUNTID,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             exchangeRate = a.EXCHANGERATE,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                             subSectorId = a.SUBSECTORID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = (double)b.INTERATERATE,
                             effectiveDate = b.EFFECTIVEDATE,
                             maturityDate = (DateTime)b.MATURITYDATE,
                             bookingDate = DateTime.Today,
                             overdraftLimit = (decimal)b.OVERDRAFTTOPUP,
                             //disbursedAmount = a.DISBURSED_AMOUNT,
                             //interestAmount = a.INTEREST_AMOUNT,
                             pastDuePrincipal = a.PASTDUEPRINCIPAL,
                             pastDueInterest = a.PASTDUEINTEREST,
                             interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                             interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                             penalChargeAmount = a.PENALCHARGEAMOUNT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = (int)a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = b.OPERATIONTYPEID,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             dayCountConventionId = a.DAYCOUNTCONVENTIONID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = DateTime.Today,

                         }).ToList();

            List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



            foreach (var item in model)
            {
                item.productTypeId = 6;
                var loanReferenceNumber = loan.GenerateLoanReferenceNumber(item.customerId, item.productId, item.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = item.customerId;
                addOverDraft.PRODUCTID = item.productId;
                addOverDraft.COMPANYID = item.companyId;
                addOverDraft.CASAACCOUNTID = item.casaAccountId;
               // addOverDraft.CASAACCOUNTID2 = item.casaAccountId2;
                addOverDraft.BRANCHID = item.branchId;
                addOverDraft.CURRENCYID = item.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = item.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = item.loanReferenceNumber;
                addOverDraft.SUBSECTORID = item.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addOverDraft.MISCODE = item.misCode;
                addOverDraft.TEAMMISCODE = item.teamMiscode;
                addOverDraft.INTERESTRATE = item.interestRate;
                addOverDraft.EFFECTIVEDATE = item.effectiveDate;
                addOverDraft.MATURITYDATE = item.maturityDate;
                addOverDraft.BOOKINGDATE = item.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = item.overdraftLimit;
               // addOverDraft.DISBURSED_AMOUNT = item.disbursedAmount;
                //addOverDraft.INTEREST_AMOUNT = item.interestAmount;
                addOverDraft.APPROVALSTATUSID = item.approvalStatusId;
                addOverDraft.APPROVEDBY = item.approvedBy;
                addOverDraft.APPROVERCOMMENT = item.approverComment;
                addOverDraft.DATEAPPROVED = item.dateApproved;
                addOverDraft.LOANSTATUSID = item.loanStatusId;
                addOverDraft.ISDISBURSED = item.isDisbursed;
                addOverDraft.DISBURSEDBY = item.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = item.disburserComment;
                addOverDraft.DISBURSEDATE = item.disburseDate;
                addOverDraft.OPERATIONID = item.operationId;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = item.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = item.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)item.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addOverDraft.NPLDATE = item.nplDate;
                addOverDraft.CREATEDBY = item.createdBy;
                addOverDraft.DATETIMECREATED = item.dateTimeCreated;

                overDraft.Add(addOverDraft);
            }

            this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

            context.SaveChanges();


                TBL_LOAN_REVOLVING results = (from p in context.TBL_LOAN_REVOLVING
                                              where p.REVOLVINGLOANID == loanId
                                              && p.LOANSTATUSID == (short)LoanStatusEnum.Active
                                              select p).SingleOrDefault();

                results.OVERDRAFTLIMIT = results.OVERDRAFTLIMIT - amount;
            context.SaveChanges();
  
        }

        #endregion

        #region Re-phasement  Operation
        //---------------------------------- Begining of Loan re-phasement-------------------------------------

        //---------------------------------- Rate Revision-------------------------------------

        public int LoanExist(int loanId)
        {
            var loanRef = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                           where a.LOANID == loanId
                           select a);
            int loanRefResults = loanRef.Count();

            return loanRefResults;
        }

        public bool DeleteLoanExist(int loanId)
        {
            bool output = false;

            var removeLoan_Archive = (from p in context.TBL_LOAN_ARCHIVE
                                      where p.LOANID == loanId
                                      select p);
            var removeLoan_Schedule_Periodic_Archive = (from p in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                                                        where p.LOANID == loanId
                                                        select p);
            var removeLoan_Schedule_Daily_Archive = (from p in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                                                     where p.LOANID == loanId
                                                     select p);
            var removeLoan_Schedule_Daily_Temp = (from p in context.TBL_LOAN_SCHEDULE_DAILY_TEMP
                                                  where p.LOANID == loanId
                                                  select p);
            var removeLoan_Schedule_Periodic_Temp = (from p in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                                                     where p.LOANID == loanId
                                                     select p);

            var removeOD_Archive = (from p in context.TBL_LOAN_REVOLVING_ARCHIVE
                                      where p.REVOLVINGLOANID == loanId
                                      select p);

            if (removeLoan_Archive != null)
            {
                context.TBL_LOAN_ARCHIVE.RemoveRange(removeLoan_Archive);
            }
            if (removeLoan_Schedule_Periodic_Archive != null)
            {
                context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.RemoveRange(removeLoan_Schedule_Periodic_Archive);

            }
            if (removeLoan_Schedule_Daily_Archive != null)
            {
                context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV.RemoveRange(removeLoan_Schedule_Daily_Archive);

            }
            if (removeLoan_Schedule_Daily_Temp != null)
            {
                context.TBL_LOAN_SCHEDULE_DAILY_TEMP.RemoveRange(removeLoan_Schedule_Daily_Temp);

            }
            if (removeLoan_Schedule_Periodic_Temp != null)
            {
                context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.RemoveRange(removeLoan_Schedule_Periodic_Temp);

            }
            if (removeOD_Archive != null)
            {
                context.TBL_LOAN_REVOLVING_ARCHIVE.RemoveRange(removeOD_Archive);

            }
            try
            {
                context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new System.Data.Entity.Validation.DbEntityValidationException(errorMessages);
            }
            //context.SaveChanges();
            output = true;

            return output;
        }

        public IEnumerable<LoanViewModel> ArchiveLoan(int loanId, int operationId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN
                         where a.TERMLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new LoanViewModel()
                         {
                             loanId = a.TERMLOANID,
                             productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                             customerRiskRatingId = a.CUSTOMERRISKRATINGID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             exchangeRate = a.EXCHANGERATE,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             subSectorId = a.SUBSECTORID,
                             principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID,
                             interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID,
                             principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                             interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = a.INTERESTRATE,
                             effectiveDate = a.EFFECTIVEDATE,
                             lastRestructureDate = (DateTime)a.LASTRESTRUCTUREDATE,
                             maturityDate = a.MATURITYDATE,
                             bookingDate = a.BOOKINGDATE,
                             principalAmount = a.PRINCIPALAMOUNT,
                             principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                             interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             scheduleTypeId = a.SCHEDULETYPEID,
                             scheduleDayCountConventionId = a.SCHEDULEDAYCOUNTCONVENTIONID,
                             scheduleDayInterestTypeId = a.SCHEDULEDAYINTERESTTYPEID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = a.OPERATIONID,
                             customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                             loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                             equityContribution = a.EQUITYCONTRIBUTION,
                             firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                             firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                             outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                             outstandingInterest = a.OUTSTANDINGINTEREST,
                             pastDuePrincipal = a.PASTDUEPRINCIPAL,
                             pastDueInterest = a.PASTDUEINTEREST,
                             interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                             interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                             penalChargeAmount = a.PENALCHARGEAMOUNT,
                             principalAdditionCount = a.PRINCIPALADDITIONCOUNT,
                             principalReductionCount = a.PRINCIPALREDUCTIONCOUNT,
                             fixedPrincipal = a.FIXEDPRINCIPAL,
                             profileLoan = a.PROFILELOAN,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             isScheduledPrepayment = a.ISSCHEDULEDPREPAYMENT,
                             allowForceDebitRepayment = a.ALLOWFORCEDEBITREPAYMENT,
                             scheduledPrepaymentAmount = a.SCHEDULEDPREPAYMENTAMOUNT,
                             scheduledPrepaymentDate = a.SCHEDULEDPREPAYMENTDATE,
                             scheduledPrepaymentFrequencyTypeId = a.SCH_PREPAYMENT_FREQUENCY_TYPID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,

                         }).ToList();

            List<TBL_LOAN_ARCHIVE> loanArchive = new List<TBL_LOAN_ARCHIVE>();



            foreach (var item in model)
            {

                TBL_LOAN_ARCHIVE addLoanArchive = new TBL_LOAN_ARCHIVE();


                addLoanArchive.CHANGEEFFECTIVEDATE = DateTime.Today;
                addLoanArchive.ISAPPLIED = false;
                addLoanArchive.CHANGEREASON = "Rephasement";
                addLoanArchive.LOANID = item.loanId;
                addLoanArchive.PRODUCTPRICEINDEXRATE = item.productPriceIndexRate;
                addLoanArchive.CUSTOMERRISKRATINGID = item.customerRiskRatingId;
                addLoanArchive.CUSTOMERID = item.customerId;
                addLoanArchive.PRODUCTID = item.productId;
                addLoanArchive.COMPANYID = item.companyId;
                addLoanArchive.CASAACCOUNTID = item.casaAccountId;
                addLoanArchive.BRANCHID = item.branchId;
                addLoanArchive.CURRENCYID = (short)item.currencyId;
                addLoanArchive.EXCHANGERATE = item.exchangeRate;
                addLoanArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addLoanArchive.LOANREFERENCENUMBER = item.loanReferenceNumber;
                addLoanArchive.SUBSECTORID = item.subSectorId;
                addLoanArchive.PRINCIPALFREQUENCYTYPEID = item.principalFrequencyTypeId;
                addLoanArchive.INTERESTFREQUENCYTYPEID = item.interestFrequencyTypeId;
                addLoanArchive.PRINCIPALNUMBEROFINSTALLMENT = item.principalNumberOfInstallment;
                addLoanArchive.INTERESTNUMBEROFINSTALLMENT = item.interestNumberOfInstallment;
                addLoanArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addLoanArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addLoanArchive.MISCODE = item.misCode;
                addLoanArchive.TEAMMISCODE = item.teamMiscode;
                addLoanArchive.INTERESTRATE = item.interestRate;
                addLoanArchive.EFFECTIVEDATE = item.effectiveDate;
                addLoanArchive.MATURITYDATE = item.maturityDate;
                addLoanArchive.BOOKINGDATE = item.bookingDate;
                addLoanArchive.PRINCIPALAMOUNT = item.principalAmount;
                addLoanArchive.PRINCIPALINSTALLMENTLEFT = item.principalInstallmentLeft;
                addLoanArchive.INTERESTINSTALLMENTLEFT = item.interestInstallmentLeft;
                addLoanArchive.APPROVALSTATUSID = item.approvalStatusId;
                addLoanArchive.APPROVEDBY = item.approvedBy;
                addLoanArchive.APPROVERCOMMENT = item.approverComment;
                addLoanArchive.DATEAPPROVED = item.dateApproved;
                addLoanArchive.LOANSTATUSID = item.loanStatusId;
                addLoanArchive.CREATEDBY = item.createdBy;
                addLoanArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanArchive.SCHEDULETYPEID = item.scheduleTypeId;
                addLoanArchive.SCHEDULEDAYCOUNTCONVENTIONID = item.scheduleDayCountConventionId;
                addLoanArchive.SCHEDULEDAYINTERESTTYPEID = item.scheduleDayInterestTypeId;
                addLoanArchive.ISDISBURSED = item.isDisbursed;
                addLoanArchive.DISBURSEDBY = item.disbursedBy;
                addLoanArchive.DISBURSERCOMMENT = item.disburserComment;
                addLoanArchive.DISBURSEDATE = item.disburseDate;
                addLoanArchive.OPERATIONID = operationId;
                //addLoanArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID = item.customerGroupId;
                //addLoanArchive.LOANTYPEID = item.loanTypeId;
                //addLoanArchive.TrancheBatchCode = item.trancheBatchCode;
                addLoanArchive.EQUITYCONTRIBUTION = item.equityContribution;
                addLoanArchive.FIRSTPRINCIPALPAYMENTDATE = item.firstPrincipalPaymentDate;
                addLoanArchive.FIRSTINTERESTPAYMENTDATE = item.firstInterestPaymentDate;
                addLoanArchive.OUTSTANDINGPRINCIPAL = item.outstandingPrincipal;
                addLoanArchive.OUTSTANDINGINTEREST = item.outstandingInterest;

                addLoanArchive.PASTDUEPRINCIPAL = item.pastDuePrincipal;
                addLoanArchive.PASTDUEINTEREST = item.pastDueInterest;
                addLoanArchive.INTERESTONPASTDUEPRINCIPAL = item.interestOnPastDuePrincipal;
                addLoanArchive.INTERESTONPASTDUEINTEREST = item.interesrtOnPastDueInterest;
                addLoanArchive.PENALCHARGEAMOUNT = item.penalChargeAmount;
                addLoanArchive.PRINCIPALADDITIONCOUNT = item.principalAdditionCount;
                addLoanArchive.PRINCIPALREDUCTIONCOUNT = item.principalReductionCount;
                addLoanArchive.FIXEDPRINCIPAL = item.fixedPrincipal;
                addLoanArchive.PROFILELOAN = item.profileLoan;
                addLoanArchive.DISCHARGELETTER = item.dischargeLetter;
                addLoanArchive.SUSPENDINTEREST = item.suspendInterest;
                addLoanArchive.ISSCHEDULEDPREPAYMENT = item.isScheduledPrepayment;
                addLoanArchive.ALLOWFORCEDEBITREPAYMENT = item.allowForceDebitRepayment;
                addLoanArchive.SCHEDULEDPREPAYMENTAMOUNT = item.scheduledPrepaymentAmount;
                addLoanArchive.SCHEDULEDPREPAYMENTDATE = item.scheduledPrepaymentDate;
                addLoanArchive.SCH_PREPAYMENT_FREQUENCY_TYPID = item.principalFrequencyTypeId;//scheduledPrepaymentFrequencyTypeId;
                addLoanArchive.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addLoanArchive.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addLoanArchive.USER_PRUDENTIAL_GUIDE_STATUSID = (int)item.userPrudentialGuidelineStatusId;
                addLoanArchive.NPLDATE = item.nplDate;
                addLoanArchive.CREATEDBY = item.createdBy;
                addLoanArchive.DATETIMECREATED = item.dateTimeCreated;

                loanArchive.Add(addLoanArchive);
            }
            //tbl_Loan
            this.context.TBL_LOAN_ARCHIVE.AddRange(loanArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<RevolvingLoanViewModel> ArchiveOverDraft (int overDraftId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         where a.REVOLVINGLOANID == overDraftId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                            // casaAccountId2 = a.CASAACCOUNTID2,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             exchangeRate = a.EXCHANGERATE,                          
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                             subSectorId = a.SUBSECTORID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = a.INTERESTRATE,
                             effectiveDate = a.EFFECTIVEDATE,
                             maturityDate = a.MATURITYDATE,
                             bookingDate = a.BOOKINGDATE,
                             overdraftLimit = a.OVERDRAFTLIMIT,
                             //disbursedAmount = a.DISBURSED_AMOUNT,
                             //interestAmount = a.INTEREST_AMOUNT,
                             pastDuePrincipal = a.PASTDUEPRINCIPAL,
                             pastDueInterest = a.PASTDUEINTEREST,
                             interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                             interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                             penalChargeAmount = a.PENALCHARGEAMOUNT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = (int)a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = a.OPERATIONID,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             dayCountConventionId = a.DAYCOUNTCONVENTIONID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,

                         }).ToList();

            List<TBL_LOAN_REVOLVING_ARCHIVE> overDraftArchive = new List<TBL_LOAN_REVOLVING_ARCHIVE>();



            foreach (var item in model)
            {

                TBL_LOAN_REVOLVING_ARCHIVE addOverDraftArchive  = new TBL_LOAN_REVOLVING_ARCHIVE();


                addOverDraftArchive.ARCHIVEDATE = DateTime.Today;
                addOverDraftArchive.REVOLVINGLOANID = item.loanId;
                addOverDraftArchive.CUSTOMERID = item.customerId;
                addOverDraftArchive.PRODUCTID = item.productId;
                addOverDraftArchive.COMPANYID = item.companyId;
                addOverDraftArchive.CASAACCOUNTID = item.casaAccountId;
                //addOverDraftArchive.CASAACCOUNTID2 = item.casaAccountId2;
                addOverDraftArchive.BRANCHID = item.branchId;
                addOverDraftArchive.CURRENCYID = item.currencyId;
                addOverDraftArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addOverDraftArchive.EXCHANGERATE = item.exchangeRate;
                addOverDraftArchive.LOANREFERENCENUMBER = item.loanReferenceNumber;
                addOverDraftArchive.RELATED_LOAN_REFERENCE_NUMBER = item.relatedLoanReferenceNumber;
                addOverDraftArchive.SUBSECTORID = item.subSectorId;
                addOverDraftArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addOverDraftArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addOverDraftArchive.MISCODE = item.misCode;
                addOverDraftArchive.TEAMMISCODE = item.teamMiscode;
                addOverDraftArchive.INTERESTRATE = item.interestRate;
                addOverDraftArchive.EFFECTIVEDATE = item.effectiveDate;
                addOverDraftArchive.MATURITYDATE = item.maturityDate;
                addOverDraftArchive.BOOKINGDATE = item.bookingDate;
                addOverDraftArchive.OVERDRAFTLIMIT = item.overdraftLimit;
                //addOverDraftArchive.DISBURSED_AMOUNT = item.disbursedAmount;
                //addOverDraftArchive.INTEREST_AMOUNT = item.interestAmount;
                addOverDraftArchive.APPROVALSTATUSID = item.approvalStatusId;
                addOverDraftArchive.APPROVEDBY = item.approvedBy;
                addOverDraftArchive.APPROVERCOMMENT = item.approverComment;
                addOverDraftArchive.DATEAPPROVED = item.dateApproved;
                addOverDraftArchive.LOANSTATUSID = item.loanStatusId;
                addOverDraftArchive.ISDISBURSED = item.isDisbursed;
                addOverDraftArchive.DISBURSEDBY = item.disbursedBy;
                addOverDraftArchive.DISBURSERCOMMENT = item.disburserComment;
                addOverDraftArchive.DISBURSEDATE = item.disburseDate;
                addOverDraftArchive.OPERATIONID = item.operationId;
                addOverDraftArchive.CREATEDBY = item.createdBy;
                addOverDraftArchive.DATETIMECREATED = item.dateTimeCreated;
                addOverDraftArchive.DISCHARGELETTER = item.dischargeLetter;
                addOverDraftArchive.SUSPENDINTEREST = item.suspendInterest;
                addOverDraftArchive.DAYCOUNTCONVENTIONID = (short)item.dayCountConventionId;
                addOverDraftArchive.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addOverDraftArchive.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addOverDraftArchive.NPLDATE = item.nplDate;
                addOverDraftArchive.CREATEDBY = item.createdBy;
                addOverDraftArchive.DATETIMECREATED = item.dateTimeCreated;

                overDraftArchive.Add(addOverDraftArchive);
            }
            //tbl_Loan
            this.context.TBL_LOAN_REVOLVING_ARCHIVE.AddRange(overDraftArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         where a.LOANID == loanId
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             paymentDate = a.PAYMENTDATE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             periodPaymentAmount = (double)a.PERIODPAYMENTAMOUNT,
                             periodInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             amortisedStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amortisedPeriodPaymentAmount = (double)a.AMORTISEDPERIODPAYMENTAMOUNT,
                             amortisedPeriodInterestAmount = (double)a.AMORTISEDPERIODINTERESTAMOUNT,
                             amortisedPeriodPrincipalAmount = (double)a.AMORTISEDPERIODPRINCIPALAMOUNT,
                             amortisedEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,

                         }).ToList();

            List<TBL_LOAN_SCHEDULE_PERIODIC_ARC> loanSchedulePeriodicArchive = new List<TBL_LOAN_SCHEDULE_PERIODIC_ARC>();



            foreach (var item in model)
            {

                TBL_LOAN_SCHEDULE_PERIODIC_ARC addLoanSchedulePeriodicArchive = new TBL_LOAN_SCHEDULE_PERIODIC_ARC();


                addLoanSchedulePeriodicArchive.LOANID = item.loanId;
                addLoanSchedulePeriodicArchive.PAYMENTNUMBER = item.paymentNumber;
                addLoanSchedulePeriodicArchive.PAYMENTDATE = item.paymentDate;
                addLoanSchedulePeriodicArchive.STARTPRINCIPALAMOUNT = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodicArchive.PERIODPAYMENTAMOUNT = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodicArchive.PERIODINTERESTAMOUNT = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodicArchive.PERIODPRINCIPALAMOUNT = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodicArchive.ENDPRINCIPALAMOUNT = (decimal)item.endPrincipalAmount;


                addLoanSchedulePeriodicArchive.INTERESTRATE = item.interestRate;
                addLoanSchedulePeriodicArchive.AMORTISEDSTARTPRINCIPALAMOUNT = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDPERIODPAYMENTAMOUNT = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDPERIODINTERESTAMOUNT = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDPERIODPRINCIPALAMOUNT = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDENDPRINCIPALAMOUNT = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodicArchive.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanSchedulePeriodicArchive.CREATEDBY = item.createdBy;
                addLoanSchedulePeriodicArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanSchedulePeriodicArchive.ARCHIVEDATE = generalSetup.GetApplicationDate();
                addLoanSchedulePeriodicArchive.ARCHIVEBATCHCODE = batchCode;

                loanSchedulePeriodicArchive.Add(addLoanSchedulePeriodicArchive);

            }

            this.context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.AddRange(loanSchedulePeriodicArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                         where a.LOANID == loanId
                         select new LoanPaymentScheduleDailyViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             date = a.DATE,
                             paymentDate = a.PAYMENTDATE,
                             openingBalance = (double)a.OPENINGBALANCE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             dailyPaymentAmount = (double)a.DAILYPAYMENTAMOUNT,
                             dailyInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                             dailyPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,
                             closingBalance = (double)a.CLOSINGBALANCE,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             amortisedCost = (double)a.AMORTISEDCOST,
                             accruedInterest = (double)a.ACCRUEDINTEREST,
                             norminalInterestRate = a.INTERESTRATE,

                             amOpeningBalance = (double)a.AMORTISEDOPENINGBALANCE,
                             amStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amDailyPaymentAmount = (double)a.AMORTISEDDAILYPAYMENTAMOUNT,
                             amDailyInterestAmount = (double)a.AMORTISEDDAILYINTERESTAMOUNT,
                             amDailyPrincipalAmount = (double)a.AMORTISEDDAILYPRINCIPALAMOUNT,
                             amClosingBalance = (double)a.AMORTISEDCLOSINGBALANCE,
                             amEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             amAccruedInterest = (double)a.AMORTISEDACCRUEDINTEREST,
                             amAmortisedCost = (double)a.AMORTISED_AMORTISEDCOST,
                             discountPremium = (double)a.DISCOUNTPREMIUM,
                             unEarnedFee = (double)a.UNEARNEDFEE,
                             earnedFee = (double)a.EARNEDFEE,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             numberOfPeriods = a.NUMBEROFPERIODS,
                             ballonAmount = (double)a.BALLONAMOUNT,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,


                         }).ToList();

            List<TBL_LOAN_SCHEDULE_DAILY_ARCHIV> loanScheduleDailyArchive = new List<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>();



            foreach (var item in model)
            {
                TBL_LOAN_SCHEDULE_DAILY_ARCHIV addLoanScheduleDailyArchive = new TBL_LOAN_SCHEDULE_DAILY_ARCHIV();


                addLoanScheduleDailyArchive.LOANID = loanId;
                addLoanScheduleDailyArchive.PAYMENTNUMBER = item.paymentNumber;
                addLoanScheduleDailyArchive.DATE = item.date;
                addLoanScheduleDailyArchive.PAYMENTDATE = item.paymentDate;
                addLoanScheduleDailyArchive.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                addLoanScheduleDailyArchive.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                addLoanScheduleDailyArchive.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                addLoanScheduleDailyArchive.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                addLoanScheduleDailyArchive.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                addLoanScheduleDailyArchive.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                addLoanScheduleDailyArchive.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                addLoanScheduleDailyArchive.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                addLoanScheduleDailyArchive.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                addLoanScheduleDailyArchive.INTERESTRATE = item.norminalInterestRate;

                addLoanScheduleDailyArchive.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                addLoanScheduleDailyArchive.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                addLoanScheduleDailyArchive.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                addLoanScheduleDailyArchive.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                addLoanScheduleDailyArchive.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                addLoanScheduleDailyArchive.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                addLoanScheduleDailyArchive.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                addLoanScheduleDailyArchive.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                addLoanScheduleDailyArchive.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                addLoanScheduleDailyArchive.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                addLoanScheduleDailyArchive.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                addLoanScheduleDailyArchive.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                addLoanScheduleDailyArchive.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanScheduleDailyArchive.NUMBEROFPERIODS = item.numberOfPeriods;
                addLoanScheduleDailyArchive.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                addLoanScheduleDailyArchive.CREATEDBY = item.createdBy;
                addLoanScheduleDailyArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanScheduleDailyArchive.ARCHIVEDATE = generalSetup.GetApplicationDate();
                addLoanScheduleDailyArchive.ARCHIVEBATCHCODE = batchCode;

                loanScheduleDailyArchive.Add(addLoanScheduleDailyArchive);

            }

            this.context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV.AddRange(loanScheduleDailyArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule(int loanId, DateTime applicationDate)
        {


            int no = 0;//number.Count() - 1;

            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                         where a.LOANID == loanId && a.PAYMENTDATE < DbFunctions.TruncateTime(applicationDate)
                         orderby a.PAYMENTNUMBER ascending
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             paymentDate = a.PAYMENTDATE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             periodPaymentAmount = (double)a.PERIODPAYMENTAMOUNT,
                             periodInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             amortisedStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amortisedPeriodPaymentAmount = (double)a.AMORTISEDPERIODPAYMENTAMOUNT,
                             amortisedPeriodInterestAmount = (double)a.AMORTISEDPERIODINTERESTAMOUNT,
                             amortisedPeriodPrincipalAmount = (double)a.AMORTISEDPERIODPRINCIPALAMOUNT,
                             amortisedEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                         }).Concat
                         (from a in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                          where a.LOANID == loanId && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate)
                          && a.PAYMENTNUMBER != 0
                          orderby a.PAYMENTNUMBER ascending
                          select new LoanPaymentSchedulePeriodicViewModel()
                          {
                              loanId = a.LOANID,
                              paymentNumber = a.PAYMENTNUMBER,//lastNo + 1,
                              paymentDate = a.PAYMENTDATE,
                              startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                              periodPaymentAmount = (double)a.PERIODPAYMENTAMOUNT,
                              periodInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                              periodPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                              endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                              interestRate = a.INTERESTRATE,
                              amortisedStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                              amortisedPeriodPaymentAmount = (double)a.AMORTISEDPERIODPAYMENTAMOUNT,
                              amortisedPeriodInterestAmount = (double)a.AMORTISEDPERIODINTERESTAMOUNT,
                              amortisedPeriodPrincipalAmount = (double)a.AMORTISEDPERIODPRINCIPALAMOUNT,
                              amortisedEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                              effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                              createdBy = a.CREATEDBY,
                              dateTimeCreated = a.DATETIMECREATED,
                          }).ToList();

            List<TBL_LOAN_SCHEDULE_PERIODIC> loanSchedulePeriodic = new List<TBL_LOAN_SCHEDULE_PERIODIC>();



            foreach (var item in model)
            {
                TBL_LOAN_SCHEDULE_PERIODIC addLoanSchedulePeriodic = new TBL_LOAN_SCHEDULE_PERIODIC();


                addLoanSchedulePeriodic.LOANID = item.loanId;
                addLoanSchedulePeriodic.PAYMENTNUMBER = ++no - 1;//item.paymentNumber;
                addLoanSchedulePeriodic.PAYMENTDATE = item.paymentDate;
                addLoanSchedulePeriodic.STARTPRINCIPALAMOUNT = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodic.PERIODPAYMENTAMOUNT = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodic.PERIODINTERESTAMOUNT = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodic.PERIODPRINCIPALAMOUNT = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodic.ENDPRINCIPALAMOUNT = (decimal)item.endPrincipalAmount;

                addLoanSchedulePeriodic.INTERESTRATE = item.interestRate;
                addLoanSchedulePeriodic.AMORTISEDSTARTPRINCIPALAMOUNT = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodic.AMORTISEDPERIODPAYMENTAMOUNT = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodic.AMORTISEDPERIODINTERESTAMOUNT = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodic.AMORTISEDPERIODPRINCIPALAMOUNT = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodic.AMORTISEDENDPRINCIPALAMOUNT = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodic.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanSchedulePeriodic.CREATEDBY = item.createdBy;
                addLoanSchedulePeriodic.DATETIMECREATED = item.dateTimeCreated;



                loanSchedulePeriodic.Add(addLoanSchedulePeriodic);

            }

            //var itemToRemove = context.tbl_Loan_Schedule_Periodic.SingleOrDefault(x => x.LoanId == loanId);//  confirm if this code will delete all data with loanId

            //if (itemToRemove != null)
            //{
            //    context.tbl_Loan_Schedule_Periodic.Remove(itemToRemove);
            //    context.SaveChanges();
            //}

            var itemToRemove = (from p in context.TBL_LOAN_SCHEDULE_PERIODIC
                                where p.LOANID == loanId
                                select p);

            //itemToRemove.Delete();

            if (itemToRemove != null)
            {
                context.TBL_LOAN_SCHEDULE_PERIODIC.RemoveRange(itemToRemove);
                context.SaveChanges();
            }

            this.context.TBL_LOAN_SCHEDULE_PERIODIC.AddRange(loanSchedulePeriodic);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> MergeDailySchedule(int loanId, DateTime applicationDate)
        {


            int no = 0;//number.Count() - 1;

            var model = (from a in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                         where a.LOANID == loanId && a.DATE < DbFunctions.TruncateTime(applicationDate)
                         orderby a.PAYMENTNUMBER ascending
                         select new LoanPaymentScheduleDailyViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             date = a.DATE,
                             paymentDate = a.PAYMENTDATE,
                             openingBalance = (double)a.OPENINGBALANCE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             dailyPaymentAmount = (double)a.DAILYPAYMENTAMOUNT,
                             dailyInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                             dailyPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,
                             closingBalance = (double)a.CLOSINGBALANCE,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             amortisedCost = (double)a.AMORTISEDCOST,
                             accruedInterest = (double)a.ACCRUEDINTEREST,
                             norminalInterestRate = a.INTERESTRATE,

                             amOpeningBalance = (double)a.AMORTISEDOPENINGBALANCE,
                             amStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amDailyPaymentAmount = (double)a.AMORTISEDDAILYPAYMENTAMOUNT,
                             amDailyInterestAmount = (double)a.AMORTISEDDAILYINTERESTAMOUNT,
                             amDailyPrincipalAmount = (double)a.AMORTISEDDAILYPRINCIPALAMOUNT,
                             amClosingBalance = (double)a.AMORTISEDCLOSINGBALANCE,
                             amEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             amAccruedInterest = (double)a.AMORTISEDACCRUEDINTEREST,
                             amAmortisedCost = (double)a.AMORTISED_AMORTISEDCOST,
                             discountPremium = (double)a.DISCOUNTPREMIUM,
                             unEarnedFee = (double)a.UNEARNEDFEE,
                             earnedFee = (double)a.EARNEDFEE,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             numberOfPeriods = a.NUMBEROFPERIODS,
                             ballonAmount = (double)a.BALLONAMOUNT,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                         }).Concat
                         (from a in context.TBL_LOAN_SCHEDULE_DAILY_TEMP
                          where a.LOANID == loanId && a.DATE >= DbFunctions.TruncateTime(applicationDate)
                          && a.PAYMENTNUMBER != 0
                          orderby a.PAYMENTNUMBER ascending
                          select new LoanPaymentScheduleDailyViewModel()
                          {
                              loanId = a.LOANID,
                              paymentNumber = a.PAYMENTNUMBER,
                              date = a.DATE,
                              paymentDate = a.PAYMENTDATE,
                              openingBalance = (double)a.OPENINGBALANCE,
                              startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                              dailyPaymentAmount = (double)a.DAILYPAYMENTAMOUNT,
                              dailyInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                              dailyPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,
                              closingBalance = (double)a.CLOSINGBALANCE,
                              endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                              amortisedCost = (double)a.AMORTISEDCOST,
                              accruedInterest = (double)a.ACCRUEDINTEREST,
                              norminalInterestRate = a.INTERESTRATE,

                              amOpeningBalance = (double)a.AMORTISEDOPENINGBALANCE,
                              amStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                              amDailyPaymentAmount = (double)a.AMORTISEDDAILYPAYMENTAMOUNT,
                              amDailyInterestAmount = (double)a.AMORTISEDDAILYINTERESTAMOUNT,
                              amDailyPrincipalAmount = (double)a.AMORTISEDDAILYPRINCIPALAMOUNT,
                              amClosingBalance = (double)a.AMORTISEDCLOSINGBALANCE,
                              amEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                              amAccruedInterest = (double)a.AMORTISEDACCRUEDINTEREST,
                              amAmortisedCost = (double)a.AMORTISED_AMORTISEDCOST,
                              discountPremium = (double)a.DISCOUNTPREMIUM,
                              unEarnedFee = (double)a.UNEARNEDFEE,
                              earnedFee = (double)a.EARNEDFEE,
                              effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                              numberOfPeriods = a.NUMBEROFPERIODS,
                              ballonAmount = (double)a.BALLONAMOUNT,
                              createdBy = a.CREATEDBY,
                              dateTimeCreated = a.DATETIMECREATED,
                          }).ToList();

            // List<tbl_Loan_Schedule_Periodic> loanSchedulePeriodic = new List<tbl_Loan_Schedule_Periodic>();
            List<TBL_LOAN_SCHEDULE_DAILY> loanScheduleDaily = new List<TBL_LOAN_SCHEDULE_DAILY>();


            foreach (var item in model)
            {
                TBL_LOAN_SCHEDULE_DAILY addLoanScheduleDaily = new TBL_LOAN_SCHEDULE_DAILY();

                addLoanScheduleDaily.LOANID = loanId;
                addLoanScheduleDaily.PAYMENTNUMBER = ++no - 1;
                addLoanScheduleDaily.DATE = item.date;
                addLoanScheduleDaily.PAYMENTDATE = item.paymentDate;
                addLoanScheduleDaily.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                addLoanScheduleDaily.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                addLoanScheduleDaily.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                addLoanScheduleDaily.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                addLoanScheduleDaily.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                addLoanScheduleDaily.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                addLoanScheduleDaily.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                addLoanScheduleDaily.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                addLoanScheduleDaily.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                addLoanScheduleDaily.INTERESTRATE = item.norminalInterestRate;
                addLoanScheduleDaily.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                addLoanScheduleDaily.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                addLoanScheduleDaily.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                addLoanScheduleDaily.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                addLoanScheduleDaily.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                addLoanScheduleDaily.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                addLoanScheduleDaily.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                addLoanScheduleDaily.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                addLoanScheduleDaily.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                addLoanScheduleDaily.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                addLoanScheduleDaily.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                addLoanScheduleDaily.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                addLoanScheduleDaily.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanScheduleDaily.NUMBEROFPERIODS = item.numberOfPeriods;
                addLoanScheduleDaily.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                addLoanScheduleDaily.CREATEDBY = item.createdBy;
                addLoanScheduleDaily.DATETIMECREATED = item.dateTimeCreated;

                loanScheduleDaily.Add(addLoanScheduleDaily);

            }


            var itemToRemove = (from p in context.TBL_LOAN_SCHEDULE_DAILY
                                where p.LOANID == loanId
                                select p);

            //itemToRemove.Delete();

            if (itemToRemove != null)
            {
                context.TBL_LOAN_SCHEDULE_DAILY.RemoveRange(itemToRemove);
                context.SaveChanges();
            }

            this.context.TBL_LOAN_SCHEDULE_DAILY.AddRange(loanScheduleDaily);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicScheduleForInterest(int loanId, DateTime applicationDate)
        {


            int no = 0;//number.Count() - 1;

            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                         where a.LOANID == loanId && a.PAYMENTDATE < DbFunctions.TruncateTime(applicationDate)
                         orderby a.PAYMENTNUMBER ascending
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             paymentDate = a.PAYMENTDATE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             periodPaymentAmount = (double)a.PERIODPAYMENTAMOUNT,
                             periodInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             previousInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                             previousPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             amortisedStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amortisedPeriodPaymentAmount = (double)a.AMORTISEDPERIODPAYMENTAMOUNT,
                             amortisedPeriodInterestAmount = (double)a.AMORTISEDPERIODINTERESTAMOUNT,
                             amortisedPeriodPrincipalAmount = (double)a.AMORTISEDPERIODPRINCIPALAMOUNT,
                             amortisedEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                         }).Concat
                         (from a in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                          join b in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC on a.LOANID equals b.LOANID
                          where a.LOANID == loanId && a.PAYMENTDATE == b.PAYMENTDATE && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate)
                          && a.PAYMENTNUMBER != 0
                          orderby a.PAYMENTNUMBER ascending
                          select new LoanPaymentSchedulePeriodicViewModel()
                          {
                              loanId = a.LOANID,
                              paymentNumber = a.PAYMENTNUMBER,//lastNo + 1,
                              paymentDate = a.PAYMENTDATE,
                              startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                              periodPaymentAmount = (double)a.PERIODPAYMENTAMOUNT,
                              periodInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                              periodPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                              endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                              previousInterestAmount = (double)b.PERIODINTERESTAMOUNT,
                              previousPrincipalAmount = (double)b.PERIODPRINCIPALAMOUNT,
                              interestRate = a.INTERESTRATE,
                              amortisedStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                              amortisedPeriodPaymentAmount = (double)a.AMORTISEDPERIODPAYMENTAMOUNT,
                              amortisedPeriodInterestAmount = (double)a.AMORTISEDPERIODINTERESTAMOUNT,
                              amortisedPeriodPrincipalAmount = (double)a.AMORTISEDPERIODPRINCIPALAMOUNT,
                              amortisedEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                              effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                              createdBy = a.CREATEDBY,
                              dateTimeCreated = a.DATETIMECREATED,
                          }).ToList();

            List<TBL_LOAN_SCHEDULE_PERIODIC> loanSchedulePeriodic = new List<TBL_LOAN_SCHEDULE_PERIODIC>();



            foreach (var item in model)
            {
                TBL_LOAN_SCHEDULE_PERIODIC addLoanSchedulePeriodic = new TBL_LOAN_SCHEDULE_PERIODIC();


                addLoanSchedulePeriodic.LOANID = item.loanId;
                addLoanSchedulePeriodic.PAYMENTNUMBER = ++no - 1;//item.paymentNumber;
                addLoanSchedulePeriodic.PAYMENTDATE = item.paymentDate;
                addLoanSchedulePeriodic.STARTPRINCIPALAMOUNT = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodic.PERIODPAYMENTAMOUNT = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodic.PERIODINTERESTAMOUNT = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodic.PERIODPRINCIPALAMOUNT = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodic.ENDPRINCIPALAMOUNT = (decimal)item.endPrincipalAmount;
                addLoanSchedulePeriodic.PREVIOUSINTERESTAMOUNT = (decimal)item.previousInterestAmount;
                addLoanSchedulePeriodic.PREVIOUSPRINCIPALAMOUNT = (decimal)item.previousPrincipalAmount;
                addLoanSchedulePeriodic.INTERESTRATE = item.interestRate;
                addLoanSchedulePeriodic.AMORTISEDSTARTPRINCIPALAMOUNT = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodic.AMORTISEDPERIODPAYMENTAMOUNT = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodic.AMORTISEDPERIODINTERESTAMOUNT = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodic.AMORTISEDPERIODPRINCIPALAMOUNT = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodic.AMORTISEDENDPRINCIPALAMOUNT = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodic.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanSchedulePeriodic.CREATEDBY = item.createdBy;
                addLoanSchedulePeriodic.DATETIMECREATED = item.dateTimeCreated;

                loanSchedulePeriodic.Add(addLoanSchedulePeriodic);

            }

            var itemToRemove = (from p in context.TBL_LOAN_SCHEDULE_PERIODIC
                                where p.LOANID == loanId
                                select p);

            //itemToRemove.Delete();

            if (itemToRemove != null)
            {
                context.TBL_LOAN_SCHEDULE_PERIODIC.RemoveRange(itemToRemove);
                context.SaveChanges();
            }

            this.context.TBL_LOAN_SCHEDULE_PERIODIC.AddRange(loanSchedulePeriodic);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> MergeDailyScheduleForInterest(int loanId, DateTime applicationDate)
        {
            int no = 0;//number.Count() - 1;

            var model = (from a in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                         where a.LOANID == loanId && a.DATE < DbFunctions.TruncateTime(applicationDate)
                         orderby a.PAYMENTNUMBER ascending
                         select new LoanPaymentScheduleDailyViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             date = a.DATE,
                             paymentDate = a.PAYMENTDATE,
                             openingBalance = (double)a.OPENINGBALANCE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             dailyPaymentAmount = (double)a.DAILYPAYMENTAMOUNT,
                             dailyInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                             dailyPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,
                             closingBalance = (double)a.CLOSINGBALANCE,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             amortisedCost = (double)a.AMORTISEDCOST,
                             accruedInterest = (double)a.ACCRUEDINTEREST,
                             norminalInterestRate = a.INTERESTRATE,
                             previousInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                             previousPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,

                             amOpeningBalance = (double)a.AMORTISEDOPENINGBALANCE,
                             amStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amDailyPaymentAmount = (double)a.AMORTISEDDAILYPAYMENTAMOUNT,
                             amDailyInterestAmount = (double)a.AMORTISEDDAILYINTERESTAMOUNT,
                             amDailyPrincipalAmount = (double)a.AMORTISEDDAILYPRINCIPALAMOUNT,
                             amClosingBalance = (double)a.AMORTISEDCLOSINGBALANCE,
                             amEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             amAccruedInterest = (double)a.AMORTISEDACCRUEDINTEREST,
                             amAmortisedCost = (double)a.AMORTISED_AMORTISEDCOST,
                             discountPremium = (double)a.DISCOUNTPREMIUM,
                             unEarnedFee = (double)a.UNEARNEDFEE,
                             earnedFee = (double)a.EARNEDFEE,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             numberOfPeriods = a.NUMBEROFPERIODS,
                             ballonAmount = (double)a.BALLONAMOUNT,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                         }).Concat
                          (from a in context.TBL_LOAN_SCHEDULE_DAILY_TEMP
                           join b in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV on a.LOANID equals b.LOANID
                           where a.LOANID == loanId && a.DATE == b.DATE && a.DATE >= DbFunctions.TruncateTime(applicationDate)
                           && a.PAYMENTNUMBER != 0
                           orderby a.PAYMENTNUMBER ascending
                           select new LoanPaymentScheduleDailyViewModel()
                           {
                               loanId = a.LOANID,
                               paymentNumber = a.PAYMENTNUMBER,
                               date = a.DATE,
                               paymentDate = a.PAYMENTDATE,
                               openingBalance = (double)a.OPENINGBALANCE,
                               startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                               dailyPaymentAmount = (double)a.DAILYPAYMENTAMOUNT,
                               dailyInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                               dailyPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,
                               closingBalance = (double)a.CLOSINGBALANCE,
                               endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                               amortisedCost = (double)a.AMORTISEDCOST,
                               accruedInterest = (double)a.ACCRUEDINTEREST,
                               norminalInterestRate = a.INTERESTRATE,
                               previousInterestAmount = (double)b.DAILYINTERESTAMOUNT,
                               previousPrincipalAmount = (double)b.DAILYPRINCIPALAMOUNT,
                               amOpeningBalance = (double)a.AMORTISEDOPENINGBALANCE,
                               amStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                               amDailyPaymentAmount = (double)a.AMORTISEDDAILYPAYMENTAMOUNT,
                               amDailyInterestAmount = (double)a.AMORTISEDDAILYINTERESTAMOUNT,
                               amDailyPrincipalAmount = (double)a.AMORTISEDDAILYPRINCIPALAMOUNT,
                               amClosingBalance = (double)a.AMORTISEDCLOSINGBALANCE,
                               amEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                               amAccruedInterest = (double)a.AMORTISEDACCRUEDINTEREST,
                               amAmortisedCost = (double)a.AMORTISED_AMORTISEDCOST,
                               discountPremium = (double)a.DISCOUNTPREMIUM,
                               unEarnedFee = (double)a.UNEARNEDFEE,
                               earnedFee = (double)a.EARNEDFEE,
                               effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                               numberOfPeriods = a.NUMBEROFPERIODS,
                               ballonAmount = (double)a.BALLONAMOUNT,
                               createdBy = a.CREATEDBY,
                               dateTimeCreated = a.DATETIMECREATED,
                           }).ToList();

            List<TBL_LOAN_SCHEDULE_DAILY> loanScheduleDaily = new List<TBL_LOAN_SCHEDULE_DAILY>();

            foreach (var item in model)
            {
                TBL_LOAN_SCHEDULE_DAILY addLoanScheduleDaily = new TBL_LOAN_SCHEDULE_DAILY();

                addLoanScheduleDaily.LOANID = loanId;
                addLoanScheduleDaily.PAYMENTNUMBER = ++no - 1;
                addLoanScheduleDaily.DATE = item.date;
                addLoanScheduleDaily.PAYMENTDATE = item.paymentDate;
                addLoanScheduleDaily.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                addLoanScheduleDaily.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                addLoanScheduleDaily.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                addLoanScheduleDaily.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                addLoanScheduleDaily.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                addLoanScheduleDaily.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                addLoanScheduleDaily.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                addLoanScheduleDaily.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                addLoanScheduleDaily.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                addLoanScheduleDaily.INTERESTRATE = item.norminalInterestRate;
                addLoanScheduleDaily.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                addLoanScheduleDaily.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                addLoanScheduleDaily.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                addLoanScheduleDaily.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                addLoanScheduleDaily.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                addLoanScheduleDaily.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                addLoanScheduleDaily.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                addLoanScheduleDaily.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                addLoanScheduleDaily.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                addLoanScheduleDaily.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                addLoanScheduleDaily.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                addLoanScheduleDaily.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                addLoanScheduleDaily.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanScheduleDaily.NUMBEROFPERIODS = item.numberOfPeriods;
                addLoanScheduleDaily.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                addLoanScheduleDaily.CREATEDBY = item.createdBy;
                addLoanScheduleDaily.DATETIMECREATED = item.dateTimeCreated;
                addLoanScheduleDaily.PREVIOUSINTERESTAMOUNT = Convert.ToDecimal(item.previousInterestAmount);
                addLoanScheduleDaily.PREVIOUSPRINCIPALAMOUNT = Convert.ToDecimal(item.previousPrincipalAmount);

                loanScheduleDaily.Add(addLoanScheduleDaily);

            }


            var itemToRemove = (from p in context.TBL_LOAN_SCHEDULE_DAILY
                                where p.LOANID == loanId
                                select p);

            //itemToRemove.Delete();

            if (itemToRemove != null)
            {
                context.TBL_LOAN_SCHEDULE_DAILY.RemoveRange(itemToRemove);
                context.SaveChanges();
            }

            this.context.TBL_LOAN_SCHEDULE_DAILY.AddRange(loanScheduleDaily);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool InterestRateReview(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            if (LoanExist(loanId) > 0)
            {
                DeleteLoanExist(loanId);
                ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId);
                ArchiveDailySchedule(loanId);


                //----------generate and save periodic loan schedule -----------------------------------
                List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                foreach (var item in periodicScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.INTERESTRATE = loanInput.interestRate;

                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                foreach (var item in dailyScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.DATE = item.date;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                    scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                    scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();
                //----------update loan details -----------------------------------
                var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);

                loan.INTERESTRATE = loanInput.interestRate;
                loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                //-------------------------------------------------

                MergePeriodicScheduleForInterest(loanId, applicationDate);

                context.SaveChanges();
                //-------------------------------------------------------
            }




            output = true;

            return output;
        }

        //---------------------------------- Rate Revision End-------------------------------------


        //----------------------------------Bulk Rate Revision-------------------------------------
        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> BulkArchivePeriodicSchedule(int priceindexId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where b.TBL_PRODUCT.TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID == priceindexId
                         && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && !context.TBL_LOAN_PRICEINDEX_EXCEPTION.Any(d => d.LOANID == b.TERMLOANID) // a.LoanId == loanId
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             paymentDate = a.PAYMENTDATE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             periodPaymentAmount = (double)a.PERIODPAYMENTAMOUNT,
                             periodInterestAmount = (double)a.PERIODINTERESTAMOUNT,
                             periodPrincipalAmount = (double)a.PERIODPRINCIPALAMOUNT,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             interestRate = a.INTERESTRATE,
                             amortisedStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amortisedPeriodPaymentAmount = (double)a.AMORTISEDPERIODPAYMENTAMOUNT,
                             amortisedPeriodInterestAmount = (double)a.AMORTISEDPERIODINTERESTAMOUNT,
                             amortisedPeriodPrincipalAmount = (double)a.AMORTISEDPERIODPRINCIPALAMOUNT,
                             amortisedEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,

                         }).ToList();

            List<TBL_LOAN_SCHEDULE_PERIODIC_ARC> loanSchedulePeriodicArchive = new List<TBL_LOAN_SCHEDULE_PERIODIC_ARC>();



            foreach (var item in model)
            {

                TBL_LOAN_SCHEDULE_PERIODIC_ARC addLoanSchedulePeriodicArchive = new TBL_LOAN_SCHEDULE_PERIODIC_ARC();


                addLoanSchedulePeriodicArchive.LOANID = item.loanId;
                addLoanSchedulePeriodicArchive.PAYMENTNUMBER = item.paymentNumber;
                addLoanSchedulePeriodicArchive.PAYMENTDATE = item.paymentDate;
                addLoanSchedulePeriodicArchive.STARTPRINCIPALAMOUNT = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodicArchive.PERIODPAYMENTAMOUNT = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodicArchive.PERIODINTERESTAMOUNT = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodicArchive.PERIODPRINCIPALAMOUNT = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodicArchive.ENDPRINCIPALAMOUNT = (decimal)item.endPrincipalAmount;


                addLoanSchedulePeriodicArchive.INTERESTRATE = item.interestRate;
                addLoanSchedulePeriodicArchive.AMORTISEDSTARTPRINCIPALAMOUNT = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDPERIODPAYMENTAMOUNT = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDPERIODINTERESTAMOUNT = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDPERIODPRINCIPALAMOUNT = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodicArchive.AMORTISEDENDPRINCIPALAMOUNT = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodicArchive.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanSchedulePeriodicArchive.CREATEDBY = item.createdBy;
                addLoanSchedulePeriodicArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanSchedulePeriodicArchive.ARCHIVEDATE = generalSetup.GetApplicationDate();
                addLoanSchedulePeriodicArchive.ARCHIVEBATCHCODE = batchCode;

                loanSchedulePeriodicArchive.Add(addLoanSchedulePeriodicArchive);

            }

            this.context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.AddRange(loanSchedulePeriodicArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> BulkArchiveDailySchedule()
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                         join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         where b.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && !context.TBL_LOAN_PRICEINDEX_EXCEPTION.Any(d => d.LOANID == b.TERMLOANID)
                         select new LoanPaymentScheduleDailyViewModel()
                         {
                             loanId = a.LOANID,
                             paymentNumber = a.PAYMENTNUMBER,
                             date = a.DATE,
                             paymentDate = a.PAYMENTDATE,
                             openingBalance = (double)a.OPENINGBALANCE,
                             startPrincipalAmount = (double)a.STARTPRINCIPALAMOUNT,
                             dailyPaymentAmount = (double)a.DAILYPAYMENTAMOUNT,
                             dailyInterestAmount = (double)a.DAILYINTERESTAMOUNT,
                             dailyPrincipalAmount = (double)a.DAILYPRINCIPALAMOUNT,
                             closingBalance = (double)a.CLOSINGBALANCE,
                             endPrincipalAmount = (double)a.ENDPRINCIPALAMOUNT,
                             amortisedCost = (double)a.AMORTISEDCOST,
                             accruedInterest = (double)a.ACCRUEDINTEREST,
                             norminalInterestRate = a.INTERESTRATE,

                             amOpeningBalance = (double)a.AMORTISEDOPENINGBALANCE,
                             amStartPrincipalAmount = (double)a.AMORTISEDSTARTPRINCIPALAMOUNT,
                             amDailyPaymentAmount = (double)a.AMORTISEDDAILYPAYMENTAMOUNT,
                             amDailyInterestAmount = (double)a.AMORTISEDDAILYINTERESTAMOUNT,
                             amDailyPrincipalAmount = (double)a.AMORTISEDDAILYPRINCIPALAMOUNT,
                             amClosingBalance = (double)a.AMORTISEDCLOSINGBALANCE,
                             amEndPrincipalAmount = (double)a.AMORTISEDENDPRINCIPALAMOUNT,
                             amAccruedInterest = (double)a.AMORTISEDACCRUEDINTEREST,
                             amAmortisedCost = (double)a.AMORTISED_AMORTISEDCOST,
                             discountPremium = (double)a.DISCOUNTPREMIUM,
                             unEarnedFee = (double)a.UNEARNEDFEE,
                             earnedFee = (double)a.EARNEDFEE,
                             effectiveInterestRate = a.EFFECTIVEINTERESTRATE,
                             numberOfPeriods = a.NUMBEROFPERIODS,
                             ballonAmount = (double)a.BALLONAMOUNT,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,


                         }).ToList();

            List<TBL_LOAN_SCHEDULE_DAILY_ARCHIV> loanScheduleDailyArchive = new List<TBL_LOAN_SCHEDULE_DAILY_ARCHIV>();



            foreach (var item in model)
            {
                TBL_LOAN_SCHEDULE_DAILY_ARCHIV addLoanScheduleDailyArchive = new TBL_LOAN_SCHEDULE_DAILY_ARCHIV();


                addLoanScheduleDailyArchive.LOANID = item.loanId;
                addLoanScheduleDailyArchive.PAYMENTNUMBER = item.paymentNumber;
                addLoanScheduleDailyArchive.DATE = item.date;
                addLoanScheduleDailyArchive.PAYMENTDATE = item.paymentDate;
                addLoanScheduleDailyArchive.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                addLoanScheduleDailyArchive.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                addLoanScheduleDailyArchive.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                addLoanScheduleDailyArchive.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                addLoanScheduleDailyArchive.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                addLoanScheduleDailyArchive.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                addLoanScheduleDailyArchive.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                addLoanScheduleDailyArchive.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                addLoanScheduleDailyArchive.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                addLoanScheduleDailyArchive.INTERESTRATE = item.norminalInterestRate;

                addLoanScheduleDailyArchive.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                addLoanScheduleDailyArchive.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                addLoanScheduleDailyArchive.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                addLoanScheduleDailyArchive.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                addLoanScheduleDailyArchive.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                addLoanScheduleDailyArchive.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                addLoanScheduleDailyArchive.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                addLoanScheduleDailyArchive.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                addLoanScheduleDailyArchive.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                addLoanScheduleDailyArchive.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                addLoanScheduleDailyArchive.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                addLoanScheduleDailyArchive.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                addLoanScheduleDailyArchive.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                addLoanScheduleDailyArchive.NUMBEROFPERIODS = item.numberOfPeriods;
                addLoanScheduleDailyArchive.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                addLoanScheduleDailyArchive.CREATEDBY = item.createdBy;
                addLoanScheduleDailyArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanScheduleDailyArchive.ARCHIVEDATE = generalSetup.GetApplicationDate();
                addLoanScheduleDailyArchive.ARCHIVEBATCHCODE = batchCode;

                loanScheduleDailyArchive.Add(addLoanScheduleDailyArchive);

            }

            this.context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV.AddRange(loanScheduleDailyArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanViewModel> BulkArchiveLoan()
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN
                         where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                          && !context.TBL_LOAN_PRICEINDEX_EXCEPTION.Any(d => d.LOANID == a.TERMLOANID)
                         //where !context.tbl_Loan_PriceIndex_Exception.Any(d => d.LoanId == a.TermLoanId)

                         select new LoanViewModel()
                         {
                             loanId = a.TERMLOANID,
                             productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                             customerRiskRatingId = a.CUSTOMERRISKRATINGID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             exchangeRate = a.EXCHANGERATE,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             subSectorId = a.SUBSECTORID,
                             principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID,
                             interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID,
                             principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                             interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = a.INTERESTRATE,
                             effectiveDate = a.EFFECTIVEDATE,
                             maturityDate = a.MATURITYDATE,
                             bookingDate = a.BOOKINGDATE,
                             principalAmount = a.PRINCIPALAMOUNT,

                             principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                             interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             scheduleTypeId = a.SCHEDULETYPEID,
                             scheduleDayCountConventionId = a.SCHEDULEDAYCOUNTCONVENTIONID,
                             scheduleDayInterestTypeId = a.SCHEDULEDAYINTERESTTYPEID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             operationId = a.OPERATIONID,
                             customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                             loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                             equityContribution = a.EQUITYCONTRIBUTION,
                             firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                             firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                             outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                             outstandingInterest = a.OUTSTANDINGINTEREST,
                             principalAdditionCount = a.PRINCIPALADDITIONCOUNT,
                             principalReductionCount = a.PRINCIPALREDUCTIONCOUNT,
                             fixedPrincipal = a.FIXEDPRINCIPAL,
                             profileLoan = a.PROFILELOAN,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             isScheduledPrepayment = a.ISSCHEDULEDPREPAYMENT,
                             allowForceDebitRepayment = a.ALLOWFORCEDEBITREPAYMENT,
                             scheduledPrepaymentAmount = a.SCHEDULEDPREPAYMENTAMOUNT,
                             scheduledPrepaymentDate = a.SCHEDULEDPREPAYMENTDATE,
                             scheduledPrepaymentFrequencyTypeId = a.SCH_PREPAYMENT_FREQUENCY_TYPID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,

                         }).ToList();

            List<TBL_LOAN_ARCHIVE> loanArchive = new List<TBL_LOAN_ARCHIVE>();



            foreach (var item in model)
            {

                TBL_LOAN_ARCHIVE addLoanArchive = new TBL_LOAN_ARCHIVE();


                addLoanArchive.CHANGEEFFECTIVEDATE = DateTime.Today;
                addLoanArchive.ISAPPLIED = false;
                addLoanArchive.CHANGEREASON = "Rephasement";
                addLoanArchive.LOANID = item.loanId;
                addLoanArchive.PRODUCTPRICEINDEXRATE = item.productPriceIndexRate;
                addLoanArchive.CUSTOMERRISKRATINGID = item.customerRiskRatingId;
                addLoanArchive.CUSTOMERID = item.customerId;
                addLoanArchive.PRODUCTID = item.productId;
                addLoanArchive.COMPANYID = item.companyId;
                addLoanArchive.CASAACCOUNTID = item.casaAccountId;
                addLoanArchive.BRANCHID = item.branchId;
                addLoanArchive.CURRENCYID = (short)item.currencyId;
                addLoanArchive.EXCHANGERATE = item.exchangeRate;
                addLoanArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addLoanArchive.LOANREFERENCENUMBER = item.loanReferenceNumber;
                addLoanArchive.SUBSECTORID = item.subSectorId;
                addLoanArchive.PRINCIPALFREQUENCYTYPEID = item.principalFrequencyTypeId;
                addLoanArchive.INTERESTFREQUENCYTYPEID = item.interestFrequencyTypeId;
                addLoanArchive.PRINCIPALNUMBEROFINSTALLMENT = item.principalNumberOfInstallment;
                addLoanArchive.INTERESTNUMBEROFINSTALLMENT = item.interestNumberOfInstallment;
                addLoanArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addLoanArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addLoanArchive.MISCODE = item.misCode;
                addLoanArchive.TEAMMISCODE = item.teamMiscode;
                addLoanArchive.INTERESTRATE = item.interestRate;
                addLoanArchive.EFFECTIVEDATE = item.effectiveDate;
                addLoanArchive.MATURITYDATE = item.maturityDate;
                addLoanArchive.BOOKINGDATE = item.bookingDate;
                addLoanArchive.PRINCIPALAMOUNT = item.principalAmount;
                addLoanArchive.PRINCIPALINSTALLMENTLEFT = item.principalInstallmentLeft;
                addLoanArchive.INTERESTINSTALLMENTLEFT = item.interestInstallmentLeft;
                addLoanArchive.APPROVALSTATUSID = item.approvalStatusId;
                addLoanArchive.APPROVEDBY = item.approvedBy;
                addLoanArchive.APPROVERCOMMENT = item.approverComment;
                addLoanArchive.DATEAPPROVED = item.dateApproved;
                addLoanArchive.LOANSTATUSID = item.loanStatusId;
                addLoanArchive.CREATEDBY = item.createdBy;
                addLoanArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanArchive.SCHEDULETYPEID = item.scheduleTypeId;
                addLoanArchive.SCHEDULEDAYCOUNTCONVENTIONID = item.scheduleDayCountConventionId;
                addLoanArchive.SCHEDULEDAYINTERESTTYPEID = item.scheduleDayInterestTypeId;
                addLoanArchive.ISDISBURSED = item.isDisbursed;
                addLoanArchive.DISBURSEDBY = item.disbursedBy;
                addLoanArchive.DISBURSERCOMMENT = item.disburserComment;
                addLoanArchive.DISBURSEDATE = item.disburseDate;
                addLoanArchive.OPERATIONID = (int)item.operationId;
                //addLoanArchive.CUSTOMERGROUPID = item.customerGroupId;
                // addLoanArchive.LOANTYPEID = item.loanTypeId;
                //addLoanArchive.TrancheBatchCode = item.trancheBatchCode;
                addLoanArchive.EQUITYCONTRIBUTION = item.equityContribution;
                addLoanArchive.FIRSTPRINCIPALPAYMENTDATE = item.firstPrincipalPaymentDate;
                addLoanArchive.FIRSTINTERESTPAYMENTDATE = item.firstInterestPaymentDate;
                addLoanArchive.OUTSTANDINGPRINCIPAL = item.outstandingPrincipal;
                addLoanArchive.OUTSTANDINGINTEREST = item.outstandingInterest;
                addLoanArchive.PRINCIPALADDITIONCOUNT = item.principalAdditionCount;
                addLoanArchive.PRINCIPALREDUCTIONCOUNT = item.principalReductionCount;
                addLoanArchive.FIXEDPRINCIPAL = item.fixedPrincipal;
                addLoanArchive.PROFILELOAN = item.profileLoan;
                addLoanArchive.DISCHARGELETTER = item.dischargeLetter;
                addLoanArchive.SUSPENDINTEREST = item.suspendInterest;
                addLoanArchive.ISSCHEDULEDPREPAYMENT = item.isScheduledPrepayment;
                addLoanArchive.ALLOWFORCEDEBITREPAYMENT = item.allowForceDebitRepayment;
                addLoanArchive.SCHEDULEDPREPAYMENTAMOUNT = item.scheduledPrepaymentAmount;
                addLoanArchive.SCHEDULEDPREPAYMENTDATE = item.scheduledPrepaymentDate;
                addLoanArchive.SCH_PREPAYMENT_FREQUENCY_TYPID = item.principalFrequencyTypeId;//scheduledPrepaymentFrequencyTypeId;
                addLoanArchive.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addLoanArchive.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addLoanArchive.NPLDATE = item.nplDate;
                addLoanArchive.CREATEDBY = item.createdBy;
                addLoanArchive.DATETIMECREATED = item.dateTimeCreated;

                loanArchive.Add(addLoanArchive);

            }

            this.context.TBL_LOAN_ARCHIVE.AddRange(loanArchive);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool BulkRateReview(short priceindexId, double newRate, DateTime applicationDate, int staffId, int operationId)
        {
            bool output = false;
            // var applicationDate = generalSetup.GetApplicationDate();

            //BulkArchiveLoan(priceindexId);
            //BulkArchivePeriodicSchedule(priceindexId);
            //BulkArchiveDailySchedule(priceindexId);

            var currentRate = this.context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == priceindexId).FirstOrDefault().PRICEINDEXRATE;

            var rateChange = newRate - currentRate;

            var model = (
                         from a in context.TBL_LOAN
                         where a.TBL_PRODUCT.TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID == priceindexId
                         && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         && applicationDate < a.MATURITYDATE && a.FIRSTINTERESTPAYMENTDATE < a.MATURITYDATE && a.FIRSTPRINCIPALPAYMENTDATE < a.MATURITYDATE
                         //&& a.FIRSTINTERESTPAYMENTDATE > applicationDate && a.FIRSTPRINCIPALPAYMENTDATE > applicationDate
                         && !context.TBL_LOAN_PRICEINDEX_EXCEPTION.Any(d => d.LOANID == a.TERMLOANID)
                         orderby a.TERMLOANID
                         // a.LoanId == loanId
                         select new LoanPaymentRestructureScheduleInputViewModel()
                         {
                             loanId = a.TERMLOANID,
                             scheduleMethodId = a.SCHEDULETYPEID,
                             principalAmount = (double)a.OUTSTANDINGPRINCIPAL,
                             principalFrequency = (short)a.PRINCIPALFREQUENCYTYPEID,
                             interestFrequency = (short)a.INTERESTFREQUENCYTYPEID,
                             newEffectiveDate = applicationDate,
                             principalFirstpaymentDate = (DateTime)a.FIRSTPRINCIPALPAYMENTDATE,
                             interestFirstpaymentDate = (DateTime)a.FIRSTINTERESTPAYMENTDATE,
                             interestRate = a.INTERESTRATE + rateChange,
                             effectiveDate = applicationDate,
                             maturityDate = a.MATURITYDATE,
                             accrualBasis = a.SCHEDULEDAYCOUNTCONVENTIONID,
                             firstDayType = a.SCHEDULEDAYINTERESTTYPEID,
                             integralFeeAmount = 0,
                             operationId = operationId,
                         }).ToList();

            foreach (var item in model)
            {
                var unEarnedFee = from d in context.TBL_LOAN_SCHEDULE_DAILY
                                  where d.LOANID == item.loanId
                                  let sumUnEarnedFee = context.TBL_LOAN_SCHEDULE_DAILY.Where(a => a.LOANID == item.loanId
                                  && a.DATE >= DbFunctions.TruncateTime(applicationDate)).Sum(a => (double?)a.UNEARNEDFEE ?? 0)
                                  select sumUnEarnedFee;
                //item.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;

                var firstPrinDate = this.context.TBL_LOAN.Where(x => x.TERMLOANID == item.loanId).FirstOrDefault().FIRSTPRINCIPALPAYMENTDATE.Value;
                var effectiveDate = this.context.TBL_LOAN.Where(x => x.TERMLOANID == item.loanId).FirstOrDefault().EFFECTIVEDATE;
                var firstIntDate = this.context.TBL_LOAN.Where(x => x.TERMLOANID == item.loanId).FirstOrDefault().FIRSTINTERESTPAYMENTDATE.Value;
                int pricDateDiff = (firstPrinDate - effectiveDate).Days;
                int intDateDiff = (firstIntDate - effectiveDate).Days;
                item.principalFirstpaymentDate = applicationDate.AddDays(pricDateDiff);
                item.interestFirstpaymentDate = applicationDate.AddDays(intDateDiff);

                InterestRateReview(item.loanId, item, applicationDate, staffId);


            }

            context.SaveChanges();
            //---------------------------------------------.----------
            output = true;

            return output;
        }
        //----------------------------------Bulk Rate Revision End-------------------------------------
        public void updateloanTableStatus(int loanId)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.Completed;



            context.SaveChanges();
        }

        public void updateLoanReviewOperation(short loanReviewOperationsId, int loanId)
        {
            TBL_LOAN_REVIEW_OPERATION result = (from p in context.TBL_LOAN_REVIEW_OPERATION
                                                where p.LOANID == loanId && p.LOANREVIEWOPERATIONID == loanReviewOperationsId
                                                select p).SingleOrDefault();

            result.OPERATIONCOMPLETED = true;



            context.SaveChanges();
        }

        public void updateLoanPrincipalInterestPaymentDate(DateTime firstPrincipalPaymentDate, DateTime firstInterestPaymentDate, int loanId)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId //&& p.FIRSTINTERESTPAYMENTDATE == firstPrincipalPaymentDate && p.FIRSTINTERESTPAYMENTDATE == firstInterestPaymentDate
                               select p).SingleOrDefault();

            result.FIRSTINTERESTPAYMENTDATE = firstInterestPaymentDate;
            result.FIRSTPRINCIPALPAYMENTDATE = firstPrincipalPaymentDate;



            context.SaveChanges();
        }

        public void updateLoanFrequency(short PrincipalFrequency, short InterestFrequency, int loanId)
        {
            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId //&& p.FIRSTINTERESTPAYMENTDATE == firstPrincipalPaymentDate && p.FIRSTINTERESTPAYMENTDATE == firstInterestPaymentDate
                               select p).SingleOrDefault();

            if (PrincipalFrequency == null)
            {
                result.INTERESTFREQUENCYTYPEID = InterestFrequency;
            }
            else if (InterestFrequency == null)
            {
                result.PRINCIPALFREQUENCYTYPEID = PrincipalFrequency;
            }
            else
            {
                result.PRINCIPALFREQUENCYTYPEID = PrincipalFrequency;
                result.INTERESTFREQUENCYTYPEID = InterestFrequency;
            }




            context.SaveChanges();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool UpdateLoanPrepaymentSchedule(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var penalCharge = context.TBL_CHARGE_FEE.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.Prepayment);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (principalOutStandingBalance + pastDue);
            if (loanInput.payAmount >= (double)totalamount)
            {
                var refNo = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault().LOANREFERENCENUMBER;

                var penalAmount = loanInput.principalAmount * (penalCharge.RATE / 100);

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest"));

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)loanInput.payAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment"));

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)penalAmount, penalCharge.GLACCOUNTID, "Penal Charge"));///change to charge GL

                financeTransaction.PostTransaction(inputTransactions);
                updateloanTableStatus(loanInput.loanId);
            }
            else
            {

                if (LoanExist(loanId) > 0)
                {
                    DeleteLoanExist(loanId);
                    ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId);
                    ArchiveDailySchedule(loanId);

                    loanInput.principalAmount = (double)totalamount - loanInput.payAmount;
                    //---------------save irregular loan schedule input---------------------------
                    List<TBL_LOAN_REVIEW_OPRATN_IREG_SC> tblIrregularSchedule = new List<TBL_LOAN_REVIEW_OPRATN_IREG_SC>();
                    LoanScheduleTypeEnum scheduleMethod = (LoanScheduleTypeEnum)loanInput.scheduleMethodId;
                    if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                    {
                        var data = loanInput.irregularPaymentSchedule.OrderBy(x => x.paymentDate);
                        foreach (var item in data)
                        {
                            TBL_LOAN_REVIEW_OPRATN_IREG_SC schedule = new TBL_LOAN_REVIEW_OPRATN_IREG_SC();
                            schedule.LOANREVIEWOPERATIONID = loanId;
                            schedule.PAYMENTDATE = item.paymentDate;
                            schedule.PAYMENTAMOUNT = Convert.ToDecimal(item.paymentAmount);
                            schedule.CREATEDBY = staffId;
                            schedule.DATETIMECREATED = applicationDate;

                            tblIrregularSchedule.Add(schedule);
                        }

                    }
                    //----------------------------------------------


                    //----------generate and save periodic loan schedule -----------------------------------

                    //loanInput.principalAmount = loanInput.newAmount;

                    List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                    List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                    foreach (var item in periodicScheduleTemp)
                    {
                        TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                        scheduleTemp.LOANID = loanId;
                        scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                        scheduleTemp.PAYMENTDATE = item.paymentDate;
                        scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                        scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                        scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                        scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.INTERESTRATE = loanInput.interestRate;

                        scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                        scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                        scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                        scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                        scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                        scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                        scheduleTemp.CREATEDBY = staffId;
                        scheduleTemp.DATETIMECREATED = systemDate;

                        tblPeriodicScheduleTemp.Add(scheduleTemp);
                    }
                    //-------------------------------------------------------------------------------------


                    //----------generate and save daily loan schedule -----------------------------------

                    //loanInput.principalAmount = loanInput.newAmount;
                    List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                    List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                    foreach (var item in dailyScheduleTemp)
                    {
                        TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                        scheduleTemp.LOANID = loanId;
                        scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                        scheduleTemp.DATE = item.date;
                        scheduleTemp.PAYMENTDATE = item.paymentDate;
                        scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                        scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                        scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                        scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                        scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                        scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                        scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                        scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                        scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                        scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                        scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                        scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                        scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                        scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                        scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                        scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                        scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                        scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                        scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                        scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                        scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                        scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                        scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                        scheduleTemp.CREATEDBY = staffId;
                        scheduleTemp.DATETIMECREATED = systemDate;

                        tblDailyScheduleTemp.Add(scheduleTemp);
                    }
                    //----------------------------------------------------------------

                    //------------adding records to the database--------------------------

                    //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                    //{ this.context.tbl_Loan_Review_Operation_Irregular_Schedule.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    context.SaveChanges();

                    var outstInterest = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                                        where d.LOANID == loanId
                                        let sumPrincipalAmount = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId).Sum(a => a.PERIODINTERESTAMOUNT)
                                        select sumPrincipalAmount;
                    var outstandingInterest = outstInterest.FirstOrDefault();
                    //----------update loan details -----------------------------------
                    //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                    loan.OUTSTANDINGPRINCIPAL = loan.OUTSTANDINGPRINCIPAL - (decimal)loanInput.payAmount;
                    loan.OUTSTANDINGINTEREST = outstandingInterest;
                    //-------------------------------------------------

                    MergePeriodicSchedule(loanId, applicationDate);
                    MergeDailySchedule(loanId, applicationDate);


                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)loanInput.newAmount, penalCharge.GLAccountId, "Penal Charge"));
                    //financeTransaction.PostTransaction(inputTransactions);
                    context.SaveChanges();
                    //-------------------------------------------------------
                }

            }


            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PaymentFrequencyChange(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            if (LoanExist(loanId) > 0)
            {
                DeleteLoanExist(loanId);
                ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId);
                ArchiveDailySchedule(loanId);


                //----------generate and save periodic loan schedule -----------------------------------

                loanInput.principalFrequency = (short)loanInput.newPrincipalFrequency;
                loanInput.interestFrequency = (short)loanInput.newInterestFrequency;

                List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                foreach (var item in periodicScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.INTERESTRATE = loanInput.interestRate;

                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------

                //loanInput.principalAmount = loanInput.newAmount;
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                foreach (var item in dailyScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.DATE = item.date;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                    scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                    scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();
                //----------update loan details -----------------------------------
                var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                //-------------------------------------------------

                MergePeriodicSchedule(loanId, applicationDate);

                context.SaveChanges();
                //-------------------------------------------------------
            }

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PaymentDateChange(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            if (LoanExist(loanId) > 0)
            {
                DeleteLoanExist(loanId);
                ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId);
                ArchiveDailySchedule(loanId);


                //----------generate and save periodic loan schedule -----------------------------------

                //loanInput.principalFirstpaymentDate = loanInput.newPrincipalFirstpaymentDate;
                //loanInput.interestFirstpaymentDate = loanInput.newInterestFirstpaymentDate;

                List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                foreach (var item in periodicScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.INTERESTRATE = loanInput.interestRate;

                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------

                //loanInput.principalAmount = loanInput.newAmount;
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                foreach (var item in dailyScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.DATE = item.date;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                    scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                    scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();
                //----------update loan details -----------------------------------
                var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                //-------------------------------------------------

                MergePeriodicSchedule(loanId, applicationDate);

                context.SaveChanges();
                //-------------------------------------------------------
            }

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanReversal(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            loanInput.date = applicationDate;
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            var loanScheDaily = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate);
            var loanSchePeriodic = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(x => x.TBL_LOAN.TERMLOANID == loanId 
            && x.PAYMENTDATE >= applicationDate && x.PAYMENTDATE <= systemDate  && x.PAYMENTNUMBER != 0).OrderBy(x => x.PERIODICSCHEDULEID);
            decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
            decimal accruedInterest = loanScheDaily.FirstOrDefault().ACCRUEDINTEREST;
            int Count = loanSchePeriodic.Count();
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            ArchiveLoan(loanId, loanId);/////loanId change this to OperationId
            ArchivePeriodicSchedule(loanId);
            ArchiveDailySchedule(loanId);

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.Terminated;

            //----------generate and save periodic loan schedule -----------------------------------
            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

            List<TBL_LOAN_SCHEDULE_PERIODIC> tblPeriodicSchedule = new List<TBL_LOAN_SCHEDULE_PERIODIC>();
            foreach (var item in periodicSchedule)
            {
                TBL_LOAN_SCHEDULE_PERIODIC schedule = new TBL_LOAN_SCHEDULE_PERIODIC();

                schedule.LOANID = loanId;
                schedule.PAYMENTNUMBER = item.paymentNumber;
                schedule.PAYMENTDATE = item.paymentDate;
                schedule.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                schedule.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                schedule.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                schedule.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.INTERESTRATE = loanInput.interestRate;
                schedule.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                schedule.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                schedule.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                schedule.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                schedule.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                schedule.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                schedule.CREATEDBY = staffId;
                schedule.DATETIMECREATED = systemDate;

                tblPeriodicSchedule.Add(schedule);
            }
            //-------------------------------------------------------------------------------------


            //----------generate and save daily loan schedule -----------------------------------

            //loanInput.principalAmount = loanInput.newAmount;
            List<LoanPaymentScheduleDailyViewModel> dailySchedule = loanSchedule.GenerateDailyLoanSchedule(loanInput);

            List<TBL_LOAN_SCHEDULE_DAILY> tblDailySchedule = new List<TBL_LOAN_SCHEDULE_DAILY>();

            foreach (var item in dailySchedule)
            {
                TBL_LOAN_SCHEDULE_DAILY schedule = new TBL_LOAN_SCHEDULE_DAILY();

                schedule.LOANID = loanId;
                schedule.PAYMENTNUMBER = item.paymentNumber;
                schedule.DATE = item.date;
                schedule.PAYMENTDATE = item.paymentDate;
                schedule.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                schedule.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                schedule.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                schedule.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                schedule.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                schedule.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                schedule.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                schedule.INTERESTRATE = item.norminalInterestRate;
                schedule.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                schedule.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                schedule.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                schedule.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                schedule.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                schedule.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                schedule.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                schedule.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                schedule.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                schedule.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                schedule.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                schedule.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                schedule.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                schedule.NUMBEROFPERIODS = item.numberOfPeriods;
                schedule.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                schedule.CREATEDBY = staffId;
                schedule.DATETIMECREATED = systemDate;

                tblDailySchedule.Add(schedule);
            }
            //----------------------------------------------------------------


            //------------adding records to the database--------------------------

            this.context.TBL_LOAN_SCHEDULE_PERIODIC.AddRange(tblPeriodicSchedule);////change to Temp table

            this.context.TBL_LOAN_SCHEDULE_DAILY.AddRange(tblDailySchedule); ////change to Temp table
            context.SaveChanges();
            //----------update loan details -----------------------------------
            //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
            loan.MATURITYDATE = periodicSchedule.Max(x => x.paymentDate);
            loan.PRINCIPALNUMBEROFINSTALLMENT = periodicSchedule.Count() - 1;
            loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
            //-------------------------------------------------

            context.SaveChanges();
            //-------------------------------------------------------


            var data1 = from d in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                        where d.LOANID == loanId
                        let dailyAccruedInterest = context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV.Where(a => a.LOANID == loanId
                        && a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.DAILYINTERESTAMOUNT ?? 0)
                        select dailyAccruedInterest;

            decimal previousAccruedInterest = (decimal?)data1.FirstOrDefault() ?? 0;
            decimal.Round(previousAccruedInterest, 2, MidpointRounding.AwayFromZero);

            var data2 = from d in context.TBL_LOAN_SCHEDULE_DAILY_TEMP 
                        where d.LOANID == loanId
                        let dailyAccruedInterest = context.TBL_LOAN_SCHEDULE_DAILY_TEMP.Where(a => a.LOANID == loanId
                        && a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.DAILYINTERESTAMOUNT ?? 0)
                        select dailyAccruedInterest;

            decimal currentAccruedInterest = (decimal?)data2.FirstOrDefault() ?? 0;
            decimal.Round(currentAccruedInterest, 2, MidpointRounding.AwayFromZero);

            var data3 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                        where d.LOANID == loanId
                        let periodicInterest = context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.Where(a => a.LOANID == loanId
                        && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.PERIODINTERESTAMOUNT ?? 0)
                        select periodicInterest;

            decimal previousPeriodicInterest = (decimal?)data2.FirstOrDefault() ?? 0;
            decimal.Round(previousPeriodicInterest, 2, MidpointRounding.AwayFromZero);

            var data4 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                        where d.LOANID == loanId
                        let periodicInterest = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId
                        && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.PERIODINTERESTAMOUNT ?? 0)
                        select periodicInterest;

            decimal currentPeriodicInterest = (decimal?)data2.FirstOrDefault() ?? 0;
            decimal.Round(currentPeriodicInterest, 2, MidpointRounding.AwayFromZero);


            decimal accruedInterestDiff = previousAccruedInterest - currentAccruedInterest;
            decimal periodicInterestDiff = previousPeriodicInterest - currentPeriodicInterest;
            if (Count < 1 && applicationDate == systemDate)
            {
            }
            else if (Count < 1 && applicationDate <= systemDate)
            {

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterestDiff, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest Reversal"));

                financeTransaction.PostTransaction(inputTransactions);
            }
            else
            {
                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterestDiff, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest Reversal"));

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, periodicInterestDiff, product.PRINCIPALBALANCEGL.Value, "Interest Reversal"));

                financeTransaction.PostTransaction(inputTransactions);
            }

            output = true;

            return output;
        }

        public bool RegenerateSchedule(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            ///call disturbs loan method and posting
            //---------------save irregular loan schedule input---------------------------
            List<TBL_LOAN_REVIEW_OPRATN_IREG_SC> tblIrregularSchedule = new List<TBL_LOAN_REVIEW_OPRATN_IREG_SC>();
            LoanScheduleTypeEnum scheduleMethod = (LoanScheduleTypeEnum)loanInput.scheduleMethodId;
            if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
            {
                var data = loanInput.irregularPaymentSchedule.OrderBy(x => x.paymentDate);
                foreach (var item in data)
                {
                    TBL_LOAN_REVIEW_OPRATN_IREG_SC schedule = new TBL_LOAN_REVIEW_OPRATN_IREG_SC();
                    schedule.LOANREVIEWOPERATIONID = loanId;
                    schedule.PAYMENTDATE = item.paymentDate;
                    schedule.PAYMENTAMOUNT = Convert.ToDecimal(item.paymentAmount);
                    schedule.CREATEDBY = staffId;
                    schedule.DATETIMECREATED = applicationDate;

                    tblIrregularSchedule.Add(schedule);
                }

            }
            //----------------------------------------------


            //----------generate and save periodic loan schedule -----------------------------------

            //loanInput.principalAmount = loanInput.newAmount;

            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

            List<TBL_LOAN_SCHEDULE_PERIODIC> tblPeriodicSchedule = new List<TBL_LOAN_SCHEDULE_PERIODIC>();
            foreach (var item in periodicSchedule)
            {
                TBL_LOAN_SCHEDULE_PERIODIC schedule = new TBL_LOAN_SCHEDULE_PERIODIC();

                schedule.LOANID = loanId;
                schedule.PAYMENTNUMBER = item.paymentNumber;
                schedule.PAYMENTDATE = item.paymentDate;
                schedule.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                schedule.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                schedule.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                schedule.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.INTERESTRATE = loanInput.interestRate;
                schedule.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                schedule.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                schedule.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                schedule.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                schedule.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                schedule.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                schedule.CREATEDBY = staffId;
                schedule.DATETIMECREATED = systemDate;

                tblPeriodicSchedule.Add(schedule);
            }
            //-------------------------------------------------------------------------------------


            //----------generate and save daily loan schedule -----------------------------------

            //loanInput.principalAmount = loanInput.newAmount;
            List<LoanPaymentScheduleDailyViewModel> dailySchedule = loanSchedule.GenerateDailyLoanSchedule(loanInput);

            List<TBL_LOAN_SCHEDULE_DAILY> tblDailySchedule = new List<TBL_LOAN_SCHEDULE_DAILY>();

            foreach (var item in dailySchedule)
            {
                TBL_LOAN_SCHEDULE_DAILY schedule = new TBL_LOAN_SCHEDULE_DAILY();

                schedule.LOANID = loanId;
                schedule.PAYMENTNUMBER = item.paymentNumber;
                schedule.DATE = item.date;
                schedule.PAYMENTDATE = item.paymentDate;
                schedule.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                schedule.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                schedule.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                schedule.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                schedule.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                schedule.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                schedule.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                schedule.INTERESTRATE = item.norminalInterestRate;
                schedule.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                schedule.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                schedule.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                schedule.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                schedule.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                schedule.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                schedule.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                schedule.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                schedule.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                schedule.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                schedule.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                schedule.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                schedule.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                schedule.NUMBEROFPERIODS = item.numberOfPeriods;
                schedule.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                schedule.CREATEDBY = staffId;
                schedule.DATETIMECREATED = systemDate;

                tblDailySchedule.Add(schedule);
            }
            //----------------------------------------------------------------

            //------------adding records to the database--------------------------

            //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
            //{ this.context.tbl_Loan_Review_Operation_Irregular_Schedule.AddRange(tblIrregularSchedule); }////change to Temp table


            this.context.TBL_LOAN_SCHEDULE_PERIODIC.AddRange(tblPeriodicSchedule);////change to Temp table

            this.context.TBL_LOAN_SCHEDULE_DAILY.AddRange(tblDailySchedule); ////change to Temp table
            context.SaveChanges();

            var outstInterest = from d in context.TBL_LOAN_SCHEDULE_PERIODIC
                                where d.LOANID == loanId
                                let sumPrincipalAmount = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(a => a.LOANID == loanId).Sum(a => a.PERIODINTERESTAMOUNT)
                                select sumPrincipalAmount;
            var outstandingInterest = outstInterest.FirstOrDefault();
            //----------update loan details -----------------------------------
            //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
            loan.EFFECTIVEDATE = loanInput.effectiveDate;
            loan.MATURITYDATE = periodicSchedule.Max(x => x.paymentDate);
            loan.PRINCIPALNUMBEROFINSTALLMENT = periodicSchedule.Count() - 1;
            loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
            loan.OUTSTANDINGPRINCIPAL = (decimal)loanInput.principalAmount;
            loan.OUTSTANDINGINTEREST = (decimal)(periodicSchedule.Select(x => x.periodInterestAmount)).Sum();
            //-------------------------------------------------


            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool TerminateAndRebookLoanSchedule(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            loanInput.date = applicationDate;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, principalOutStandingBalance, product.PRINCIPALBALANCEGL.Value, "Loan Outstanding Principal Balance"));

            financeTransaction.PostTransaction(inputTransactions);

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.Terminated;

            context.SaveChanges();

            CreateLoan(loanId);
            output = true;

            return output;

        }

        public IEnumerable<LoanViewModel> CreateLoan(int loanId)
        {
            var refNo  = CommonHelpers.GenerateRandomDigitCode(10);
            var model = (from b in context.TBL_LOAN_REVIEW_OPERATION
                         join a in context.TBL_LOAN on b.LOANID equals a.TERMLOANID
                         where a.LOANSTATUSID == (short)LoanStatusEnum.Terminated && b.LOANID == loanId && b.OPERATIONCOMPLETED == false
                         select new LoanViewModel()
                         {
                             productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                             customerRiskRatingId = a.CUSTOMERRISKRATINGID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
                             casaAccountId2 = (int)a.CASAACCOUNTID2,
                             branchId = a.BRANCHID,
                             currencyId = a.CURRENCYID,
                             exchangeRate = a.EXCHANGERATE,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             loanReferenceNumber = refNo,
                             RelatedloanReferenceNumber = a.LOANREFERENCENUMBER,
                             subSectorId = a.SUBSECTORID,
                             principalFrequencyTypeId = (short)b.PRINCIPALFREQUENCYTYPEID,
                             interestFrequencyTypeId = (short)b.INTERESTFREQUENCYTYPEID,
                             principalNumberOfInstallment = 0,
                             interestNumberOfInstallment = 0,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             misCode = a.MISCODE,
                             teamMiscode = a.TEAMMISCODE,
                             interestRate = (double)b.INTERATERATE,
                             effectiveDate = b.EFFECTIVEDATE,
                             maturityDate = (DateTime)b.MATURITYDATE,
                             bookingDate = DateTime.Today,
                             lastRestructureDate = (DateTime)a.LASTRESTRUCTUREDATE,
                             principalAmount = (decimal)b.PREPAYMENT,

                             principalInstallmentLeft = 0,
                             interestInstallmentLeft = 0,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = DateTime.Today,
                             loanStatusId = (short)LoanStatusEnum.Active,
                             scheduleTypeId = 1,///add column in table b
                             scheduleDayCountConventionId = 0,
                             scheduleDayInterestTypeId = 0,
                             shouldDisbursed = a.SHOULD_DISBURSE,
                             isDisbursed = true,
                             disbursedBy = null,
                             disburserComment = null,
                             disburseDate = DateTime.Today,
                             operationId = 1,
                             customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                             loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                             equityContribution = 0,
                             firstPrincipalPaymentDate = b.PRINCIPALFIRSTPAYMENTDATE,
                             firstInterestPaymentDate = b.INTERESTFIRSTPAYMENTDATE,
                             outstandingPrincipal = (decimal)b.PREPAYMENT,
                             outstandingInterest = 0,
                             pastDuePrincipal = 0,
                             pastDueInterest = 0,
                             interestOnPastDuePrincipal= 0,
                             interesrtOnPastDueInterest = 0,
                             penalChargeAmount = 0, 
                             principalAdditionCount = 0,
                             principalReductionCount = 0,
                             fixedPrincipal = false,
                             profileLoan = false,
                             dischargeLetter = false,
                             suspendInterest = false,
                             isScheduledPrepayment = a.ISSCHEDULEDPREPAYMENT,
                             allowForceDebitRepayment = a.ALLOWFORCEDEBITREPAYMENT,
                             scheduledPrepaymentAmount = a.SCHEDULEDPREPAYMENTAMOUNT,
                             scheduledPrepaymentDate = a.SCHEDULEDPREPAYMENTDATE,
                             scheduledPrepaymentFrequencyTypeId = a.SCH_PREPAYMENT_FREQUENCY_TYPID,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = 1,
                             userPrudentialGuidelineStatusId = null,
                             nplDate = null,
                             createdBy = -1,
                             dateTimeCreated = DateTime.Today,

                         }).ToList();

            List<TBL_LOAN> loan = new List<TBL_LOAN>();



            foreach (var item in model)
            {

                TBL_LOAN addLoan = new TBL_LOAN();

                addLoan.PRODUCTPRICEINDEXRATE = item.productPriceIndexRate;
                addLoan.CUSTOMERRISKRATINGID = item.customerRiskRatingId;
                addLoan.CUSTOMERID = item.customerId;
                addLoan.PRODUCTID = item.productId;
                addLoan.COMPANYID = item.companyId;
                addLoan.CASAACCOUNTID = item.casaAccountId;
                addLoan.CASAACCOUNTID2 = item.casaAccountId2;
                addLoan.BRANCHID = item.branchId;
                addLoan.CURRENCYID = (short)item.currencyId;
                addLoan.EXCHANGERATE = item.exchangeRate;
                addLoan.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addLoan.LOANREFERENCENUMBER = item.loanReferenceNumber;
                addLoan.RELATED_LOAN_REFERENCE_NUMBER = item.RelatedloanReferenceNumber;
                addLoan.SUBSECTORID = item.subSectorId;
                addLoan.PRINCIPALFREQUENCYTYPEID = item.principalFrequencyTypeId;
                addLoan.INTERESTFREQUENCYTYPEID = item.interestFrequencyTypeId;
                addLoan.PRINCIPALNUMBEROFINSTALLMENT = item.principalNumberOfInstallment;
                addLoan.INTERESTNUMBEROFINSTALLMENT = item.interestNumberOfInstallment;
                addLoan.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addLoan.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addLoan.MISCODE = item.misCode;
                addLoan.TEAMMISCODE = item.teamMiscode;
                addLoan.INTERESTRATE = item.interestRate;
                addLoan.EFFECTIVEDATE = item.effectiveDate;
                addLoan.MATURITYDATE = item.maturityDate;
                addLoan.BOOKINGDATE = item.bookingDate;
                addLoan.PRINCIPALAMOUNT = item.principalAmount;
                addLoan.PRINCIPALINSTALLMENTLEFT = item.principalInstallmentLeft;
                addLoan.INTERESTINSTALLMENTLEFT = item.interestInstallmentLeft;
                addLoan.APPROVALSTATUSID = item.approvalStatusId;
                addLoan.APPROVEDBY = item.approvedBy;
                addLoan.APPROVERCOMMENT = item.approverComment;
                addLoan.DATEAPPROVED = item.dateApproved;
                addLoan.LOANSTATUSID = item.loanStatusId;
                addLoan.SCHEDULETYPEID = item.scheduleTypeId;
                addLoan.SCHEDULEDAYCOUNTCONVENTIONID = item.scheduleDayCountConventionId;
                addLoan.SCHEDULEDAYINTERESTTYPEID = item.scheduleDayInterestTypeId;
                addLoan.SHOULD_DISBURSE = item.shouldDisbursed;
                addLoan.ISDISBURSED = item.isDisbursed;
                addLoan.DISBURSEDBY = item.disbursedBy;
                addLoan.DISBURSERCOMMENT = item.disburserComment;
                addLoan.DISBURSEDATE = item.disburseDate;
                addLoan.OPERATIONID = item.operationId;
                addLoan.EQUITYCONTRIBUTION = item.equityContribution;
                addLoan.FIRSTPRINCIPALPAYMENTDATE = item.firstPrincipalPaymentDate;
                addLoan.FIRSTINTERESTPAYMENTDATE = item.firstInterestPaymentDate;
                addLoan.OUTSTANDINGPRINCIPAL = item.outstandingPrincipal;
                addLoan.OUTSTANDINGINTEREST = item.outstandingInterest;
                addLoan.PASTDUEPRINCIPAL = item.pastDuePrincipal;
                addLoan.PASTDUEINTEREST = item.pastDueInterest;
                addLoan.INTERESTONPASTDUEPRINCIPAL = item.interestOnPastDuePrincipal;
                addLoan.INTERESTONPASTDUEINTEREST = item.interesrtOnPastDueInterest;
                addLoan.PENALCHARGEAMOUNT = item.penalChargeAmount;
                addLoan.PRINCIPALADDITIONCOUNT = item.principalAdditionCount;
                addLoan.PRINCIPALREDUCTIONCOUNT = item.principalReductionCount;
                addLoan.FIXEDPRINCIPAL = item.fixedPrincipal;
                addLoan.PROFILELOAN = item.profileLoan;
                addLoan.DISCHARGELETTER = item.dischargeLetter;
                addLoan.SUSPENDINTEREST = item.suspendInterest;
                addLoan.ISSCHEDULEDPREPAYMENT = item.isScheduledPrepayment;
                addLoan.ALLOWFORCEDEBITREPAYMENT = item.allowForceDebitRepayment;
                addLoan.SCHEDULEDPREPAYMENTAMOUNT = item.scheduledPrepaymentAmount;
                addLoan.SCHEDULEDPREPAYMENTDATE = item.scheduledPrepaymentDate;
                addLoan.SCH_PREPAYMENT_FREQUENCY_TYPID = item.principalFrequencyTypeId;
                addLoan.SCH_PREPAYMENT_FREQUENCY_TYPID = item.scheduledPrepaymentFrequencyTypeId;
                addLoan.INT_PRUDENT_GUIDELINE_STATUSID = (int)item.internalPrudentialGuidelineStatusId;
                addLoan.EXT_PRUDENT_GUIDELINE_STATUSID = (int)item.externalPrudentialGuidelineStatusId;
                addLoan.USER_PRUDENTIAL_GUIDE_STATUSID = (int)item.userPrudentialGuidelineStatusId;
                addLoan.NPLDATE = item.nplDate;
                addLoan.CREATEDBY = item.createdBy;
                addLoan.DATETIMECREATED = item.dateTimeCreated;

                loan.Add(addLoan);
            }
            //tbl_Loan
            this.context.TBL_LOAN.AddRange(loan);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CompleteWriteOff(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

            var sllp = 27;////Get SLLP GL

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, sllp, "Interest Write off"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, principalOutStandingBalance, sllp, "principal Write off"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, pastDue, sllp, "past due Write off"));

            financeTransaction.PostTransaction(inputTransactions);

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.WriteOff;
            context.SaveChanges();

            TBL_LOAN_CAMSOL loanCamsol = new TBL_LOAN_CAMSOL();

            loanCamsol.LOANID = loanId;
            loanCamsol.COMPANYID = loanInput.companyId;
            loanCamsol.AMOUNTAFFECTED = totalamount;
            loanCamsol.DATE = applicationDate;
            loanCamsol.TYPE = "Loan Complete Write Off";
            loanCamsol.CUSTOMERCODE = (context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == loanInput.customerId)).CUSTOMERCODE;

            this.context.TBL_LOAN_CAMSOL.Add(loanCamsol); ////change to Temp table
            context.SaveChanges();
            /// Place a Lien on Customer Repayment Account

            CasaLienViewModel lien = new CasaLienViewModel();

            lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
            lien.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            lien.lienAmount = totalamount;
            lien.branchId = loan.BRANCHID;
            lien.companyId = loan.COMPANYID;
            lien.lienTypeId = (short)LienTypeEnum.WriteOff;
            lien.createdBy = (int)SystemStaff.System;
            lien.description = "lien placed due to Loan Write Off";

            var lienReference = casaLien.PlaceLien(lien);

            context.SaveChanges();

            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanCancellation(int loanId, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.Cancelled;

            context.SaveChanges();

            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanWalkOut(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

            decimal writeOffAmount = totalamount - (decimal)loanInput.payAmount;
            var sllp = 27;////Get SLLP GL

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
               inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, writeOffAmount, sllp, "Loan Write-Off Amount"));

            financeTransaction.PostTransaction(inputTransactions);

            if (LoanExist(loanId) > 0)
            {
                DeleteLoanExist(loanId);
                ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId);
                ArchiveDailySchedule(loanId);


                //----------generate and save periodic loan schedule -----------------------------------
                List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                foreach (var item in periodicScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.INTERESTRATE = loanInput.interestRate;

                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                foreach (var item in dailyScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.DATE = item.date;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                    scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                    scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();
                //----------update loan details -----------------------------------
                //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                //-------------------------------------------------

                MergePeriodicSchedule(loanId, applicationDate);

                context.SaveChanges();
                //-------------------------------------------------------
            }

            //TBL_LOAN result = (from p in context.TBL_LOAN
            //                   where p.TERMLOANID == loanId
            //                   select p).SingleOrDefault();

            //result.LOANSTATUSID = (short)LoanStatusEnum.Terminated;

            //context.SaveChanges();

            ///call disturbs loan method and posting



            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId)


        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.SUSPENDINTEREST = true;



            context.SaveChanges();


            var data = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join c in context.TBL_LOAN_SCHEDULE_PERIODIC on b.TERMLOANID equals c.LOANID
                        join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                        where a.DATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                        && a.PAYMENTDATE == c.PAYMENTDATE && a.LOANID == loanId && b.SUSPENDINTEREST == true

                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LOANREFERENCENUMBER,
                            productId = b.PRODUCTID,
                            branchId = b.BRANCHID,
                            companyId = b.COMPANYID,
                            currencyId = b.CURRENCYID,
                            exchangeRate = b.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = (double)a.DAILYINTERESTAMOUNT,
                            mainAmount = c.PERIODINTERESTAMOUNT,
                            categoryId = (short)DailyAccrualCategory.TermLoan,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DAYCOUNTCONVENTIONID,

                        });

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();




            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;




                transAccrual.Add(dailyAccrual);

            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         });

            foreach (var item in model)
            {
                item.date = applicationDate;
                financeTransaction.PostDailyInterestSuspension(item, loanId, applicationDate, staffId);
            }

            return data;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanSales(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);

            var interest = from d in context.TBL_LOAN_SCHEDULE_DAILY
                           where d.DATE >= DbFunctions.TruncateTime(applicationDate) && d.LOANID == loanId
                           let sumDailyAccuralAmount = context.TBL_LOAN_SCHEDULE_DAILY.Where(a => a.DATE >= DbFunctions.TruncateTime(applicationDate)
                           && d.LOANID == loanId).Sum(a => a.DAILYINTERESTAMOUNT)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.TBL_LOAN_SCHEDULE_PERIODIC
                            where d.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && d.LOANID == loanId
                            let sumPrincipalAmount = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(a => a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate)
                            && d.LOANID == loanId).Sum(a => a.PERIODPRINCIPALAMOUNT)/// add repaymentpostedstatus = false after scaffording
                            select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            var sllp = 11;////Get SLLP GL

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, sllp, "Interest Write off"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedPrincipal, sllp, "principal Write off"));

            financeTransaction.PostTransaction(inputTransactions);

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.WriteOff;

            ///call disturbs loan method and posting

            //tbl_Loan_Camsol loanCamsol = new tbl_Loan_Camsol();

            //loanCamsol.LoanId = loanId;
            //loanCamsol.CompanyId = loanInput.companyId;
            //loanCamsol.AmountAffected = accruedInterest + accruedPrincipal;
            //loanCamsol.Date = applicationDate;
            //loanCamsol.Type = "Loan Complete Write Off";

            //this.context.tbl_Loan_Camsol.Add(loanCamsol); ////change to Temp table

            /// Place a Lien on Customer Repayment Account

            //var data = new tbl_CASA_Lien
            //{
            //    ProductAccountNumber = casa.ProductAccountNumber,
            //    LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
            //    SourceReferenceNumber = loan.LoanReferenceNumber,
            //    BranchId = loan.BranchId,
            //    CompanyId = loan.CompanyId,
            //    LienCreditAmount = accruedInterest + accruedPrincipal,
            //    LienDebitAmount = 0,
            //    LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
            //    CreatedBy = (int)SystemStaff.System,
            //    Description = "lien placed due to Loan Write Off", // model.description,
            //    DateCreated = generalSetup.GetApplicationDate()

            //};

            //context.tbl_CASA_Lien.Add(data);

            // Audit Section ---------------------------            

            //var audit = new tbl_Audit
            //{
            //    AuditTypeId = (short)AuditTypeEnum.LienAdded,
            //    StaffId = (int)SystemStaff.System,
            //    BranchId = item.branchId,
            //    Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
            //    IPAddress = item.userIPAddress,
            //    Url = item.applicationUrl,
            //    ApplicationDate = generalSetup.GetApplicationDate(),
            //    SystemDateTime = DateTime.Now
            //};
            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            context.SaveChanges();

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool TenorExtension(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var penalCharge = context.TBL_CHARGE_FEE.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.TenorChange);

            if (LoanExist(loanId) > 0)
            {
                DeleteLoanExist(loanId);
                ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId);
                ArchiveDailySchedule(loanId);


                //----------generate and save periodic loan schedule -----------------------------------

                //loanInput.principalAmount = loanInput.newAmount;

                List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                foreach (var item in periodicScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.INTERESTRATE = loanInput.interestRate;

                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------

                //loanInput.principalAmount = loanInput.newAmount;
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                foreach (var item in dailyScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.DATE = item.date;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                    scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                    scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();

                //----------update loan details -----------------------------------
                var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                //-------------------------------------------------

                MergePeriodicSchedule(loanId, applicationDate);

                //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, penalCharge.GLAccountId, "Penal Charge"));///change to charge GL
                context.SaveChanges();
                //-------------------------------------------------------
            }



            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool Restructured(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            //var penalCharge = context.TBL_CHARGE_FEE.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.Restructured);

            if (LoanExist(loanId) > 0)
            {
                DeleteLoanExist(loanId);
                ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId);
                ArchiveDailySchedule(loanId);


                //----------generate and save periodic loan schedule -----------------------------------

                //loanInput.principalAmount = loanInput.newAmount;

                List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
                foreach (var item in periodicScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.INTERESTRATE = loanInput.interestRate;

                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------

                //loanInput.principalAmount = loanInput.newAmount;
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

                foreach (var item in dailyScheduleTemp)
                {
                    TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

                    scheduleTemp.LOANID = loanId;
                    scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
                    scheduleTemp.DATE = item.date;
                    scheduleTemp.PAYMENTDATE = item.paymentDate;
                    scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.INTERESTRATE = item.norminalInterestRate;

                    scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
                    scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
                    scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CREATEDBY = staffId;
                    scheduleTemp.DATETIMECREATED = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();

                //----------update loan details -----------------------------------
                var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                //-------------------------------------------------

                MergePeriodicSchedule(loanId, applicationDate);

                //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, penalCharge.GLAccountId, "Penal Charge"));///change to charge GL
                context.SaveChanges();
                //-------------------------------------------------------
            }



            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanTermination (int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            loanInput.date = applicationDate;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, principalOutStandingBalance, product.PRINCIPALBALANCEGL.Value, "Loan Outstanding Principal Balance"));

            financeTransaction.PostTransaction(inputTransactions);

            TBL_LOAN result = (from p in context.TBL_LOAN
                               where p.TERMLOANID == loanId
                               select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.Terminated;

            context.SaveChanges();

            ///call disturbs loan method and posting



            output = true;

            return output;

        }
       
        #endregion

        public LoanViewModel GetRunningLoans(int companyId, string refNo)
        {
            var applicationDate = generalSetup.GetApplicationDate();
            var data = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == refNo && x.COMPANYID == companyId);
            DateTime maturityDate = data.MATURITYDATE;
            DateTime effectiveDate = data.EFFECTIVEDATE;
            decimal outStandingBalance = data.OUTSTANDINGPRINCIPAL;
            TimeSpan difference = maturityDate - applicationDate;
            int days = (int)difference.TotalDays;
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == refNo && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == refNo && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
            outStandingBalance = decimal.Round(outStandingBalance, 2, MidpointRounding.AwayFromZero);
           
            decimal pastDue = decimal.Round((data.PASTDUEINTEREST + data.INTERESTONPASTDUEINTEREST + data.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            decimal totalamount = (accruedInterest + outStandingBalance + pastDue);


            var runningLoan = (from l in context.TBL_LOAN
                               where l.COMPANYID == companyId && l.LOANREFERENCENUMBER == refNo
                               select new LoanViewModel()
                               {
                                   loanId = l.TERMLOANID,
                                   companyName = l.TBL_COMPANY.NAME,
                                   companyId = l.COMPANYID,
                                   customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                   customerId = l.CUSTOMERID,
                                   approvedAmount = l.PRINCIPALAMOUNT,
                                   branchId = l.BRANCHID,
                                   branchName = l.TBL_BRANCH.BRANCHNAME,
                                   interestRate = l.INTERESTRATE,
                                   outstandingInterest = l.OUTSTANDINGINTEREST,
                                   outstandingPrincipal = l.OUTSTANDINGPRINCIPAL,
                                   principalAmount = l.PRINCIPALAMOUNT,
                                   currency = l.TBL_CURRENCY.CURRENCYNAME,
                                   loanReferenceNumber = l.LOANREFERENCENUMBER,
                                   effectiveDate = applicationDate,
                                   previousEffectiveDate = l.EFFECTIVEDATE,
                                   equityContribution = 0,
                                   maintainTenor = true,
                                   maturityDate = l.MATURITYDATE,
                                   scheduleTypeId = l.SCHEDULETYPEID,
                                   scheduleTypeCategoryId = l.TBL_LOAN_SCHEDULE_TYPE.SCHEDULECATEGORYID,
                                   teno = days,
                                   newtenor = 0,
                                   accrualedAmount = accruedInterest,
                                   totalAmount = totalamount,
                                   firstPrincipalPaymentDate = nextPaymentDate,
                                   firstInterestPaymentDate = nextPaymentDate,
                                   principalFrequencyTypeId = l.PRINCIPALFREQUENCYTYPEID,
                                   interestFrequencyTypeId = l.INTERESTFREQUENCYTYPEID,
                                   pastDueTotal = pastDue,
                                   relationshipManagerId =  l.RELATIONSHIPMANAGERID,
                                   relationshipOfficerId =  l.RELATIONSHIPOFFICERID,
                                   productTypeId = l.TBL_PRODUCT.PRODUCTTYPEID
                               }).FirstOrDefault();

            return runningLoan;
        }     

        public IEnumerable<LoanViewModel> GetLoanRateCustomerExcemptions(int companyId)
        {
            var excemptionsList = (from l in context.TBL_LOAN
                                   join p in context.TBL_LOAN_PRICEINDEX_EXCEPTION
                                     on l.TERMLOANID equals p.LOANID
                                   where l.COMPANYID == companyId
                                   select new LoanViewModel()
                                   {
                                       companyName = l.TBL_COMPANY.NAME,
                                       companyId = l.COMPANYID,
                                       customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                       customerId = l.CUSTOMERID,
                                       approvedAmount = l.PRINCIPALAMOUNT,
                                       branchName = l.TBL_BRANCH.BRANCHNAME,
                                       interestRate = l.INTERESTRATE,
                                       outstandingInterest = l.OUTSTANDINGINTEREST,
                                       outstandingPrincipal = l.OUTSTANDINGPRINCIPAL,
                                       principalAmount = l.PRINCIPALAMOUNT,
                                       currency = l.TBL_CURRENCY.CURRENCYCODE,
                                       loanReferenceNumber = l.LOANREFERENCENUMBER

                                   });

            return excemptionsList;
        }

        public IEnumerable<LoanBulkInterestReviewViewModel> GetNewInterestRateReviews(int companyId)
        {
            var newRates = (from l in context.TBL_LOAN_BULK_INTEREST_REVIEW
                            where l.COMPANYID == companyId && l.ISPROCESSED == false
                            select new LoanBulkInterestReviewViewModel()
                            {
                                effectiveDate = l.EFFECTIVEDATE,
                                productPriceIndexId = l.PRODUCTPRICEINDEXID,
                                productPriceIndexName = l.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                oldInterestRate = l.OLDINTERESTRATE,
                                newInterestRate = l.NEWINTERESTRATE,
                                isProcessed = l.ISPROCESSED,
                                createdBy = l.CREATEDBY,

                            });
            return newRates;
        }

        public bool addBulkRateLoanExcemptions(LoanViewModel model)
        {
            var data = new TBL_LOAN_PRICEINDEX_EXCEPTION
            {
                LOANID = model.loanId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = generalSetup.GetApplicationDate(),
            };

            //Audit Section ---------------------------
            var loanReferenceNumber = context.TBL_LOAN.Find(model.loanId).LOANREFERENCENUMBER;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkRateLoanExcemption,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.branchId,
                DETAIL = $"Added bulk rate review loan excemption for loan with ReferenceNumber: {loanReferenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            context.TBL_LOAN_PRICEINDEX_EXCEPTION.Add(data);
            context.TBL_AUDIT.Add(audit);
            return context.SaveChanges() > 0;

        }

        public bool addInterestRateChange(LoanBulkInterestReviewViewModel model)
        {
            var data = new TBL_LOAN_BULK_INTEREST_REVIEW
            {
                COMPANYID = model.companyId,
                EFFECTIVEDATE = model.effectiveDate,
                PRODUCTPRICEINDEXID = model.productPriceIndexId,
                OLDINTERESTRATE = model.oldInterestRate,
                NEWINTERESTRATE = model.newInterestRate,
                ISPROCESSED = false,
                CREATEDBY = model.createdBy,
            };

            //Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkRateLoanExcemption,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added bulk New Interest Rate from  {model.oldInterestRate} to {model.newInterestRate}. Effective from {model.effectiveDate}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            context.TBL_LOAN_BULK_INTEREST_REVIEW.Add(data);
            context.TBL_AUDIT.Add(audit);
            return context.SaveChanges() > 0;

        }

        public IEnumerable<LoanOperationTypeViewModel> GetOperationType()
        {
            return (from data in context.TBL_OPERATIONS
                    where data.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement
                    select new LoanOperationTypeViewModel()
                    {
                        operationTypeId = data.OPERATIONID,
                        operationTypeName = data.OPERATIONNAME
                    });
        }

        public IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByOD()
        {
            return (from data in context.TBL_OPERATIONS
                    where data.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft
                    select new LoanOperationTypeViewModel()
                    {
                        operationTypeId = data.OPERATIONID,
                        operationTypeName = data.OPERATIONNAME
                    });
        }

        public IEnumerable<LoanOperationTypeViewModel> GetRemedialOperationType()
        {
            return (from data in context.TBL_OPERATIONS
                    where data.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial
                    select new LoanOperationTypeViewModel()
                    {
                        operationTypeId = data.OPERATIONID,
                        operationTypeName = data.OPERATIONNAME
                    });
        }

        public IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId)
        {
            var loanOperations = (from data in context.TBL_OPERATIONS
                                  where data.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement
                                  select new LoanOperationTypeViewModel()
                                  {
                                      operationTypeId = data.OPERATIONID,
                                      operationTypeName = data.OPERATIONNAME
                                  });

            List<OperationsEnum> operationList = new List<OperationsEnum>();

            if (productTypeId == LoanProductTypeEnum.TermLoan || productTypeId == LoanProductTypeEnum.SelfLiquidating)
            {
                if ((scheduleTypeId == LoanScheduleTypeEnum.Annuity) || (scheduleTypeId == LoanScheduleTypeEnum.ConstantPrincipalAndInterest))
                {
                    operationList.Add(OperationsEnum.OverdraftTopup);
                    operationList.Add(OperationsEnum.OverdraftSubAllocation);
                    loanOperations = loanOperations.Where(x => !operationList.Contains((OperationsEnum)x.operationTypeId));
                }
                else if (scheduleTypeId == LoanScheduleTypeEnum.IrregularSchedule)
                {
                    operationList.Add(OperationsEnum.InterestandPrincipalFrequencyChange);
                    operationList.Add(OperationsEnum.PrincipalFrequencyChange);
                    operationList.Add(OperationsEnum.InterestFrequencyChange);
                    operationList.Add(OperationsEnum.InterestSuspension);
                    operationList.Add(OperationsEnum.TenorChange);
                    operationList.Add(OperationsEnum.OverdraftTopup);
                    operationList.Add(OperationsEnum.OverdraftSubAllocation);
                    loanOperations = loanOperations.Where(x => !operationList.Contains((OperationsEnum)x.operationTypeId));
                }
                else if (scheduleTypeId == LoanScheduleTypeEnum.BulletPayment)
                {
                    operationList.Add(OperationsEnum.InterestSuspension);
                    operationList.Add(OperationsEnum.InterestandPrincipalFrequencyChange);
                    operationList.Add(OperationsEnum.PrincipalFrequencyChange);
                    operationList.Add(OperationsEnum.InterestFrequencyChange);
                    operationList.Add(OperationsEnum.OverdraftTopup);
                    operationList.Add(OperationsEnum.OverdraftSubAllocation);
                    loanOperations = loanOperations.Where(x => !operationList.Contains((OperationsEnum)x.operationTypeId));
                }
                else if (scheduleTypeId == LoanScheduleTypeEnum.BallonPayment)
                {
                    operationList.Add(OperationsEnum.InterestandPrincipalFrequencyChange);
                    operationList.Add(OperationsEnum.PrincipalFrequencyChange);
                    operationList.Add(OperationsEnum.OverdraftTopup);
                    operationList.Add(OperationsEnum.OverdraftSubAllocation);
                    loanOperations = loanOperations.Where(x => !operationList.Contains((OperationsEnum)x.operationTypeId));
                }
            }
            else if (productTypeId == LoanProductTypeEnum.RevolvingLoan)
            {
                operationList.Add(OperationsEnum.TenorChange);
                operationList.Add(OperationsEnum.OverdraftTopup);
                operationList.Add(OperationsEnum.OverdraftSubAllocation);
                operationList.Add(OperationsEnum.TerminateAndRebook);

                loanOperations = loanOperations.Where(x => operationList.Contains((OperationsEnum)x.operationTypeId));
            }
            else if (productTypeId == LoanProductTypeEnum.ContingentLiability)
            {

            }

            return loanOperations;
        }

        public bool DoesOperationExist(int loanId, int operationTypeId)
        {
            var data = from a in context.TBL_LOAN_REVIEW_OPERATION
                       where a.LOANID == loanId && a.OPERATIONTYPEID == operationTypeId && a.OPERATIONCOMPLETED == false
                       select a;
            if (data.Any())
            {
                return true;
            }
            return false;
        }

        public bool AddOperationReview(LoanReviewOperationViewModel model)
        {

            List<TBL_LOAN_REVIEW_OPRATN_IREG_SC> irregularSchedules = new List<TBL_LOAN_REVIEW_OPRATN_IREG_SC>();
            //Storing the Irregular Schedule Payment Plan
            if (model.reviewIrregularSchedule.Count > 0)
            {
                foreach (var item in model.reviewIrregularSchedule)
                {
                    var irregularPlan = new TBL_LOAN_REVIEW_OPRATN_IREG_SC
                    {

                        PAYMENTAMOUNT = item.PaymentAmount,
                        PAYMENTDATE = item.PaymentDate,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = DateTime.Now
                    };
                    irregularSchedules.Add(irregularPlan);
                }
            }

            var data = new TBL_LOAN_REVIEW_OPERATION
            {
                LOANID = model.loanId,
                PRODUCTTYPEID = model.productTypeId,
                OPERATIONTYPEID = model.operationTypeId,
                EFFECTIVEDATE = model.proposedEffectiveDate,
                REVIEWDETAILS = model.reviewDetails,
                INTERATERATE = model.interateRate == null ? 0 : (double)model.interateRate,
                PREPAYMENT = model.prepayment ?? 0,
                PRINCIPALFREQUENCYTYPEID = model.principalFrequencyTypeId,
                INTERESTFREQUENCYTYPEID = model.interestFrequencyTypeId,
                PRINCIPALFIRSTPAYMENTDATE = model.principalFirstPaymentDate,
                INTERESTFIRSTPAYMENTDATE = model.interestFirstPaymentDate,
                MATURITYDATE = model.maturityDate,
                TENOR = model.tenor,
                CASA_ACCOUNTID = model.cASA_AccountId,
                OVERDRAFTTOPUP = model.overDraftTopup,
                FEE_CHARGES = model.fee_Charges,
                APPROVALSTATUSID = model.approvalStatusId,
                ISMANAGEMENTINTERESTRATE = model.isManagementRate,
                OPERATIONCOMPLETED = false,
                CREATEDBY = model.createdBy,
                DATECREATED = DateTime.Now,
                TBL_LOAN_REVIEW_OPRATN_IREG_SC = irregularSchedules
            };
            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Added tbl_Loan_Review_Operation '{ data.LOANREVIEWOPERATIONID}' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            //end of Audit section -----------------------
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    bool output = false;
                    context.TBL_LOAN_REVIEW_OPERATION.Add(data);
                    auditTrail.AddAuditTrail(audit);
                    try
                    {
                        output = context.SaveChanges() > 0;
                        //  response = context.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {

                        string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }
                    //  var output = context.SaveChanges() > 0;

                    var approvalModel = new ApprovalViewModel
                    {
                        staffId = model.createdBy,
                        companyId = model.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = model.loanId,
                        operationId = model.operationTypeId,
                        BranchId = model.userBranchId,
                        comment = "Initiation",
                        externalInitialization = true
                    };
                    if ((int)OperationsEnum.Prepayment != model.operationTypeId)
                    {
                        var response = workFlow.LogForApproval(approvalModel);
                    }
                    //
                    trans.Commit();

                    if ((int)OperationsEnum.Prepayment == model.operationTypeId)
                    {
                        int loanReviewOperationsId = this.context.TBL_LOAN_REVIEW_OPERATION.FirstOrDefault(x => x.LOANID == model.loanId).LOANREVIEWOPERATIONID;
                        LoanRephasementProcess((short)loanReviewOperationsId, model.loanId, model.staffId);
                    }

                    return output;


                }

                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }



        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId)
        {
            //var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.ContractualInterestRateChange);
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId);
            int staffApprovalLevelId = 0;
            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var dataLoan = (from ln in context.TBL_LOAN
                        join op in context.TBL_LOAN_REVIEW_OPERATION on ln.TERMLOANID equals op.LOANID
                        join tt in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals tt.OPERATIONID
                        join atrail in context.TBL_APPROVAL_TRAIL on op.LOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        && atrail.OPERATIONID == op.OPERATIONTYPEID
                        && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                        && atrail.RESPONSESTAFFID == null
                        orderby op.LOANID descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TERMLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            // loanApplicationDetailId = (int)ln.LoanApplicationDetailId,

                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                            ////tenor = (ln.MaturityDate - ln.EffectiveDate).Days,
                            principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                            pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,
                            interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                            interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,

                            principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                            interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                            relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                            relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                            misCode = ln.MISCODE,
                            teamMiscode = ln.TEAMMISCODE,
                            interestRate = ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            principalAmount = ln.OUTSTANDINGPRINCIPAL, //\\\ln.PrincipalAmount,
                            principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                            interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                            approvalStatusId = op.APPROVALSTATUSID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                            approvedBy = (int)ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            loanStatusId = ln.LOANSTATUSID,
                            scheduleTypeId = ln.SCHEDULETYPEID,
                            isDisbursed = ln.ISDISBURSED,
                            disbursedBy = (int)ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            ////approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                            operationId = ln.OPERATIONID,
                            loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                            equityContribution = ln.EQUITYCONTRIBUTION,
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

                            firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                            principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                            fixedPrincipal = ln.FIXEDPRINCIPAL,
                            profileLoan = ln.PROFILELOAN,
                            dischargeLetter = ln.DISCHARGELETTER,
                            suspendInterest = ln.SUSPENDINTEREST,

                            scheduled = ln.ISSCHEDULEDPREPAYMENT,
                            isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                            scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                            scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,

                            //customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            //customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            comment = "",

                            //Loan Review Operation
                            operationTypeId = op.OPERATIONTYPEID,
                            operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(d => d.OPERATIONID == op.OPERATIONTYPEID).OPERATIONNAME,
                            newEffectiveDate = op.EFFECTIVEDATE,
                            reviewDetails = op.REVIEWDETAILS,
                            //    newInterateRate = (decimal)op.InterateRate
                        }).ToList();

            var dataRevolvingLoan  = (from ln in context.TBL_LOAN_REVOLVING
                        join op in context.TBL_LOAN_REVIEW_OPERATION on ln.REVOLVINGLOANID equals op.LOANID
                        join tt in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals tt.OPERATIONID
                        join atrail in context.TBL_APPROVAL_TRAIL on op.LOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        && atrail.OPERATIONID == op.OPERATIONTYPEID
                        && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                        && atrail.RESPONSESTAFFID == null
                        orderby op.LOANID descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.REVOLVINGLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            // loanApplicationDetailId = (int)ln.LoanApplicationDetailId,

                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                            ////tenor = (ln.MaturityDate - ln.EffectiveDate).Days,
                            //principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                            //pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,
                            //interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                            //interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,

                            //principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                            //interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                            relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                            relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                            misCode = ln.MISCODE,
                            teamMiscode = ln.TEAMMISCODE,
                            interestRate = ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            //principalAmount = ln.OUTSTANDINGPRINCIPAL, //\\\ln.PrincipalAmount,
                            //principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                            //interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                            approvalStatusId = op.APPROVALSTATUSID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                            approvedBy = (int)ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            loanStatusId = ln.LOANSTATUSID,
                            //scheduleTypeId = ln.SCHEDULETYPEID,
                            isDisbursed = ln.ISDISBURSED,
                            //disbursedBy = (int)ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            ////approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                            operationId = ln.OPERATIONID,
                            loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                            //equityContribution = ln.EQUITYCONTRIBUTION,
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

                            //firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            //firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            //outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            //principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                            //principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                            //fixedPrincipal = ln.FIXEDPRINCIPAL,
                            //profileLoan = ln.PROFILELOAN,
                            dischargeLetter = ln.DISCHARGELETTER,
                            suspendInterest = ln.SUSPENDINTEREST,

                            //scheduled = ln.ISSCHEDULEDPREPAYMENT,
                            //isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                            //scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                            //scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,

                            //customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            //customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            comment = "",

                            //Loan Review Operation
                            operationTypeId = op.OPERATIONTYPEID,
                            operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(d => d.OPERATIONID == op.OPERATIONTYPEID).OPERATIONNAME,
                            newEffectiveDate = op.EFFECTIVEDATE,
                            reviewDetails = op.REVIEWDETAILS,
                            //    newInterateRate = (decimal)op.InterateRate
                        }).ToList();
            var data = dataLoan.Union(dataRevolvingLoan);
            return data;
        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetApprovedLoanOperationReview()
        {

            var dataLoan = (from ln in context.TBL_LOAN
                        join op in context.TBL_LOAN_REVIEW_OPERATION on ln.TERMLOANID equals op.LOANID
                        where op.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && op.OPERATIONCOMPLETED == false
                        orderby op.OPERATIONTYPEID descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TERMLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            //applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,
                            principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                            pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,
                            interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                            interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,

                            principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                            interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                            relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                            relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                            misCode = ln.MISCODE,
                            teamMiscode = ln.TEAMMISCODE,
                            interestRate = ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            principalAmount = ln.OUTSTANDINGPRINCIPAL, //\\\ln.PrincipalAmount,
                            principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                            interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                            approvalStatusId = op.APPROVALSTATUSID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                            approvedBy = (int)ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            //loanStatusId = ln.LoanStatusId,
                            scheduleTypeId = ln.SCHEDULETYPEID,
                            isDisbursed = ln.ISDISBURSED,
                            disbursedBy = (int)ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            ////approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                            operationId = ln.OPERATIONID,
                            loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                            equityContribution = ln.EQUITYCONTRIBUTION,
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

                            firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                            principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                            fixedPrincipal = ln.FIXEDPRINCIPAL,
                            profileLoan = ln.PROFILELOAN,
                            dischargeLetter = ln.DISCHARGELETTER,
                            suspendInterest = ln.SUSPENDINTEREST,

                            scheduled = ln.ISSCHEDULEDPREPAYMENT,
                            isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                            scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                            scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,

                            //customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            //customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            comment = "",
                            //Loan Review Operation
                            loanReviewOperationsId = op.LOANREVIEWOPERATIONID,
                            operationTypeId = op.OPERATIONTYPEID,
                            operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(d => d.OPERATIONID == op.OPERATIONTYPEID).OPERATIONNAME,
                            newEffectiveDate = op.EFFECTIVEDATE,
                            reviewDetails = op.REVIEWDETAILS,
                            //    newInterateRate = (decimal)op.InterateRate,
                            prepayment = op.PREPAYMENT,
                            newPrincipalFrequencyTypeId = op.PRINCIPALFREQUENCYTYPEID,
                            newInterestFrequencyTypeId = op.INTERESTFREQUENCYTYPEID,
                            newPrincipalFirstPaymentDate = op.PRINCIPALFIRSTPAYMENTDATE,
                            newInterestFirstPaymentDate = op.INTERESTFIRSTPAYMENTDATE,
                            newTenor = op.TENOR,
                            cASA_AccountId = op.CASA_ACCOUNTID,
                            overDraftTopup = op.OVERDRAFTTOPUP,
                            fee_Charges = op.FEE_CHARGES,
                        }).ToList();


            var dataRevolving = (from ln in context.TBL_LOAN_REVOLVING
                        join op in context.TBL_LOAN_REVIEW_OPERATION on ln.REVOLVINGLOANID equals op.LOANID
                        where op.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && op.OPERATIONCOMPLETED == false
                        orderby op.OPERATIONTYPEID descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.REVOLVINGLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            //applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,
                            //principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                            //pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,
                            //interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                            //interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION,

                            //principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                            //interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                            relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                            relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                            misCode = ln.MISCODE,
                            teamMiscode = ln.TEAMMISCODE,
                            interestRate = ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            //principalAmount = ln.OUTSTANDINGPRINCIPAL, //\\\ln.PrincipalAmount,
                            //principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                            //interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                            approvalStatusId = op.APPROVALSTATUSID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                            approvedBy = (int)ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            //loanStatusId = ln.LoanStatusId,
                            //scheduleTypeId = ln.SCHEDULETYPEID,
                            isDisbursed = ln.ISDISBURSED,
                            //disbursedBy = (int)ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            ////approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                            operationId = ln.OPERATIONID,
                            loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                            //equityContribution = ln.EQUITYCONTRIBUTION,
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

                            //firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            //firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                            //outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            //principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                            //principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                            //fixedPrincipal = ln.FIXEDPRINCIPAL,
                            //profileLoan = ln.PROFILELOAN,
                            dischargeLetter = ln.DISCHARGELETTER,
                            suspendInterest = ln.SUSPENDINTEREST,

                            //scheduled = ln.ISSCHEDULEDPREPAYMENT,
                            //isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                            //scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                            //scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,

                            //customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            //customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            comment = "",
                            //Loan Review Operation
                            loanReviewOperationsId = op.LOANREVIEWOPERATIONID,
                            operationTypeId = op.OPERATIONTYPEID,
                            operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(d => d.OPERATIONID == op.OPERATIONTYPEID).OPERATIONNAME,
                            newEffectiveDate = op.EFFECTIVEDATE,
                            reviewDetails = op.REVIEWDETAILS,
                            //    newInterateRate = (decimal)op.InterateRate,
                            prepayment = op.PREPAYMENT,
                            newPrincipalFrequencyTypeId = op.PRINCIPALFREQUENCYTYPEID,
                            newInterestFrequencyTypeId = op.INTERESTFREQUENCYTYPEID,
                            newPrincipalFirstPaymentDate = op.PRINCIPALFIRSTPAYMENTDATE,
                            newInterestFirstPaymentDate = op.INTERESTFIRSTPAYMENTDATE,
                            newTenor = op.TENOR,
                            cASA_AccountId = op.CASA_ACCOUNTID,
                            overDraftTopup = op.OVERDRAFTTOPUP,
                            fee_Charges = op.FEE_CHARGES,
                        }).ToList();
            var data = dataLoan.Union(dataRevolving);
            return data;
        }

        public IEnumerable<ApprovalTrailDetailsViewModel> GetApprovalDetails(int loanId, int OperationId)
        {

            var data = (from det in context.TBL_APPROVAL_TRAIL
                        where det.TARGETID == loanId && det.OPERATIONID == OperationId
                        select new ApprovalTrailDetailsViewModel
                        {
                            comment = det.COMMENT,
                            approvalStatusName = det.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                            staffName = det.TBL_STAFF.FIRSTNAME + " " + det.TBL_STAFF.FIRSTNAME,
                            targetName = context.TBL_LOAN.FirstOrDefault(l => l.TERMLOANID == det.TARGETID).LOANREFERENCENUMBER,
                            operationName = det.TBL_OPERATIONS.OPERATIONNAME,
                            approvalLevelName = det.TBL_APPROVAL_LEVEL.LEVELNAME
                        }).ToList();
            return data;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.externalInitialization = false;

            workFlow.LogForApproval(entity);

            if (workFlow.Saved)
            {
                return ApproveLoanReview(entity.targetId, entity);
            }

            return false;

        }

        private bool ApproveLoanReview(int loanId, ApprovalViewModel user)
        {
            bool output = false;
            var reviewRecord = (from s in context.TBL_LOAN_REVIEW_OPERATION
                                where s.LOANID == loanId && s.OPERATIONTYPEID == user.operationId
                               && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                && s.OPERATIONCOMPLETED == false
                                select s).FirstOrDefault();
            if (workFlow.NewState != (int)ApprovalState.Ended)
            {
                reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

            }
            else if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
            }

            output = context.SaveChanges() > 0;
            if (output == true && workFlow.NewState == (int)ApprovalState.Ended)
            {
                return output;
            }

            return false;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanRephasementProcess(short loanReviewOperationsId, int loanId, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var checkForOverDraft = this.context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == loanId);
            if(checkForOverDraft != null)
            {
                var model = (
                             from a in context.TBL_LOAN_REVIEW_OPERATION
                             join b in context.TBL_LOAN_REVOLVING on a.LOANID equals b.REVOLVINGLOANID
                             where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false

                             select new LoanPaymentRestructureScheduleInputViewModel()
                             {
                                 loanId = b.REVOLVINGLOANID,
                                 principalAmount = (double)b.OVERDRAFTLIMIT,

                                 interestRate = b.INTERESTRATE,
                                 effectiveDate = a.EFFECTIVEDATE,
                                 maturityDate = b.MATURITYDATE,
                                 integralFeeAmount = 0,
                                 newEffectiveDate = a.EFFECTIVEDATE,
                                 newInterestFirstpaymentDate = (DateTime)a.INTERESTFIRSTPAYMENTDATE,
                                 newInterest = (double)a.INTERATERATE,
                                 newAmount = (double?)a.OVERDRAFTTOPUP ?? 0,
                                 operationId = a.OPERATIONTYPEID,
                                 newPrincipalFirstpaymentDate = (DateTime)a.PRINCIPALFIRSTPAYMENTDATE,
                                 isManagementInterestRate = a.ISMANAGEMENTINTERESTRATE,
                                 proposedTenor = a.TENOR,
                                 newMaturityDate = a.MATURITYDATE,// change to maturity date affter scarfolding
                                 companyId = b.COMPANYID,
                                 staffId = staffId,
                                 createdBy = staffId,
                                 customerId = b.CUSTOMERID,

                             }).ToList();

                foreach (var item in model)
                {
                    string appDate = item.newEffectiveDate.ToString(@"yyyy-MM-dd");
                    var applicationDate = Convert.ToDateTime(appDate);           
                    if ((int)OperationsEnum.OverdraftTopup == item.operationId)
                    {

                        OverdraftTopUp(loanId, (decimal)item.newAmount);
                    }
                    else if ((int)OperationsEnum.OverdraftRenewal == item.operationId)
                    {
                        OverdraftRenewal(loanId, (decimal)item.newAmount);
                    }
                    else if ((int)OperationsEnum.OverdraftTenorExtension == item.operationId)
                    {
                        OverdraftExtension(loanId, (decimal)item.newAmount);
                    }

                    else if ((int)OperationsEnum.OverdraftSubAllocation == item.operationId)
                    {
                        SubAllocation(loanId, (decimal)item.newAmount, applicationDate, staffId);
                    }
                }
            }
            else
            {
                var scheduleMethod = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId).SCHEDULETYPEID;
                var operationType = this.context.TBL_LOAN_REVIEW_OPERATION.FirstOrDefault(x => x.LOANREVIEWOPERATIONID == loanReviewOperationsId).OPERATIONTYPEID;

                if (scheduleMethod == (short)LoanScheduleTypeEnum.IrregularSchedule || operationType == (int)OperationsEnum.LoanWalkout)
                {
                    var model = (
                             from a in context.TBL_LOAN_REVIEW_OPERATION
                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                         //join c in context.tbl_Loan_Review_Operation_Irregular_Schedule on a.LoanReviewOperationId equals c.LoanReviewOperationId
                         where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false

                             select new LoanPaymentRestructureScheduleInputViewModel()
                             {
                                 loanId = b.TERMLOANID,
                                 scheduleMethodId = b.SCHEDULETYPEID,
                                 principalAmount = (double)b.OUTSTANDINGPRINCIPAL,
                                 principalFrequency = b.PRINCIPALFREQUENCYTYPEID,
                                 interestFrequency = b.INTERESTFREQUENCYTYPEID,
                                 principalFirstpaymentDate = (DateTime)b.FIRSTPRINCIPALPAYMENTDATE,
                                 interestFirstpaymentDate = (DateTime)b.FIRSTINTERESTPAYMENTDATE,
                                 interestRate = b.INTERESTRATE,
                                 effectiveDate = a.EFFECTIVEDATE,
                                 maturityDate = b.MATURITYDATE,
                                 accrualBasis = b.SCHEDULEDAYCOUNTCONVENTIONID,
                                 firstDayType = b.SCHEDULEDAYINTERESTTYPEID,
                                 integralFeeAmount = 0,
                                 newEffectiveDate = a.EFFECTIVEDATE,
                                 newInterestFrequency = (short?)a.INTERESTFREQUENCYTYPEID ?? (short)b.INTERESTFREQUENCYTYPEID,
                                 newPrincipalFrequency = (short?)a.PRINCIPALFREQUENCYTYPEID ?? (short)b.PRINCIPALFREQUENCYTYPEID,
                                 newInterestFirstpaymentDate = (DateTime)a.INTERESTFIRSTPAYMENTDATE,
                                 newInterest = (double)a.INTERATERATE,
                                 payAmount = (double?)a.PREPAYMENT ?? 0,
                                 operationId = a.OPERATIONTYPEID,
                                 newPrincipalFirstpaymentDate = (DateTime)a.PRINCIPALFIRSTPAYMENTDATE,
                                 isManagementInterestRate = a.ISMANAGEMENTINTERESTRATE,
                                 proposedTenor = a.TENOR,
                                 newMaturityDate = a.MATURITYDATE,// change to maturity date affter scarfolding
                                 companyId = b.COMPANYID,
                                 staffId = staffId,
                                 createdBy = staffId,
                                 customerId = b.CUSTOMERID,
                             }).ToList();

                    foreach (var item in model)
                    {
                        List<IrregularLoanScheduleInputViewModel> irregularSchedule = new List<IrregularLoanScheduleInputViewModel>();
                        {
                            var scheduleInput = context.TBL_LOAN_REVIEW_OPRATN_IREG_SC.Where(x => x.LOANREVIEWOPERATIONID == loanReviewOperationsId);

                            foreach (var item2 in scheduleInput)
                            {
                                irregularSchedule.Add(new IrregularLoanScheduleInputViewModel { paymentAmount = (double)item2.PAYMENTAMOUNT, paymentDate = item2.PAYMENTDATE });
                            }

                            item.irregularPaymentSchedule = irregularSchedule;

                        }
                        var unEarnedFee = from d in context.TBL_LOAN_SCHEDULE_DAILY
                                          where d.LOANID == item.loanId
                                          let sumUnEarnedFee = context.TBL_LOAN_SCHEDULE_DAILY.Where(a => a.LOANID == item.loanId
                                          && a.DATE >= DbFunctions.TruncateTime(item.newEffectiveDate)).Sum(a => (double?)a.UNEARNEDFEE ?? 0)
                                          select sumUnEarnedFee;
                        item.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;
                        string appDate = item.newEffectiveDate.ToString(@"yyyy-MM-dd");
                        var applicationDate = Convert.ToDateTime(appDate);
                        if ((int)OperationsEnum.ContractualInterestRateChange == item.operationId)
                        {
                            DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            item.interestRate = item.newInterest;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.principalFirstpaymentDate = nextPaymentDate;
                            item.interestFirstpaymentDate = nextPaymentDate;
                            InterestRateReview(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);

                        }
                        else if ((int)OperationsEnum.Prepayment == item.operationId)
                        {
                            decimal accruedAmount = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                            decimal accruedInterest = decimal.Round(accruedAmount, 2, MidpointRounding.AwayFromZero);
                            DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            if (item.isManagementInterestRate == true)
                            {

                                item.maturityDate = (DateTime)item.newMaturityDate;
                                //item.newAmount = ((item.principalAmount + (double)accruedInterest) - item.payAmount);
                                item.effectiveDate = item.newEffectiveDate;
                                item.tenor = item.newTenorPrepayment;
                                item.interestFirstpaymentDate = nextPaymentDate;
                                item.principalFirstpaymentDate = nextPaymentDate;
                            }
                            else
                            {
                                //item.newAmount = ((item.principalAmount + (double)accruedInterest) - item.payAmount);
                                item.effectiveDate = item.newEffectiveDate;
                                item.tenor = item.newTenorPrepayment;
                                item.effectiveDate = item.newEffectiveDate;
                                item.interestFirstpaymentDate = nextPaymentDate;
                                item.principalFirstpaymentDate = nextPaymentDate;
                            }

                            UpdateLoanPrepaymentSchedule(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                        else if ((int)OperationsEnum.PaymentDateChange == item.operationId)
                        {
                            item.interestFirstpaymentDate = (DateTime)item.newInterestFirstpaymentDate;
                            item.principalFirstpaymentDate = (DateTime)item.newPrincipalFirstpaymentDate;

                            PaymentDateChange(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                            updateLoanPrincipalInterestPaymentDate(item.interestFirstpaymentDate, item.principalFirstpaymentDate, loanId);

                        }
                        else if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId || (int)OperationsEnum.InterestFrequencyChange == item.operationId
                            || (int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                        {
                            if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId)
                            {
                                item.principalFrequency = (short)item.newPrincipalFrequency;
                                item.interestFrequency = (short)item.interestFrequency;
                            }
                            if ((int)OperationsEnum.InterestFrequencyChange == item.operationId)
                            {
                                item.interestFrequency = (short)item.newInterestFrequency;
                                item.principalFrequency = (short)item.principalFrequency;
                            }
                            if ((int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                            {
                                item.interestFrequency = (short)item.newInterestFrequency;
                                item.principalFrequency = (short)item.newPrincipalFrequency;
                            }
                            PaymentFrequencyChange(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                            updateLoanFrequency((short)item.principalFrequency, (short)item.interestFrequency, loanId);
                        }
                        else if ((int)OperationsEnum.CompleteWriteOff == item.operationId)
                        {
                            CompleteWriteOff(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                        else if ((int)OperationsEnum.CancelUndisbursedLoan == item.operationId)
                        {
                            LoanCancellation(loanId, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                        else if ((int)OperationsEnum.InterestSuspension == item.operationId)
                        {
                            InterestSuspension(loanId, applicationDate, staffId);

                        }
                        else if ((int)OperationsEnum.TenorChange == item.operationId)
                        {
                            item.maturityDate = item.maturityDate.AddMonths((int)item.proposedTenor);
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.tenor + (int)item.proposedTenor;
                            TenorExtension(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }

                        else if ((int)OperationsEnum.Restructured == item.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            item.interestRate = item.newInterest;
                            item.interestFirstpaymentDate = (DateTime)item.newInterestFirstpaymentDate;//nextPaymentDate;
                            item.principalFirstpaymentDate = (DateTime)item.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            item.maturityDate = (DateTime)item.newMaturityDate;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.interestFrequency = (short)item.newInterestFrequency;
                            item.principalFrequency = (short)item.newPrincipalFrequency;
                            Restructured(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                        else if ((int)OperationsEnum.LoanWalkout == item.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            item.interestRate = item.newInterest;
                            item.effectiveDate = item.newEffectiveDate;
                            item.scheduleMethodId = (short)LoanScheduleTypeEnum.IrregularSchedule;
                            LoanWalkOut(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                    }
                }
                else
                {
                    var model = (
                             from a in context.TBL_LOAN_REVIEW_OPERATION
                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                             where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false

                             select new LoanPaymentRestructureScheduleInputViewModel()
                             {
                                 loanId = b.TERMLOANID,
                                 scheduleMethodId = b.SCHEDULETYPEID,
                                 principalAmount = (double)b.OUTSTANDINGPRINCIPAL,
                                 principalFrequency = b.PRINCIPALFREQUENCYTYPEID,
                                 interestFrequency = b.INTERESTFREQUENCYTYPEID,
                                 principalFirstpaymentDate = (DateTime)b.FIRSTPRINCIPALPAYMENTDATE,
                                 interestFirstpaymentDate = (DateTime)b.FIRSTINTERESTPAYMENTDATE,
                                 interestRate = b.INTERESTRATE,
                                 effectiveDate = a.EFFECTIVEDATE,
                                 maturityDate = b.MATURITYDATE,
                                 accrualBasis = b.SCHEDULEDAYCOUNTCONVENTIONID,
                                 firstDayType = b.SCHEDULEDAYINTERESTTYPEID,
                                 integralFeeAmount = 0,
                                 newEffectiveDate = a.EFFECTIVEDATE,
                                 newInterestFrequency = (short?)a.INTERESTFREQUENCYTYPEID ?? (short)b.INTERESTFREQUENCYTYPEID,
                                 newPrincipalFrequency = (short?)a.PRINCIPALFREQUENCYTYPEID ?? (short)b.PRINCIPALFREQUENCYTYPEID,
                                 newInterestFirstpaymentDate = (DateTime)a.INTERESTFIRSTPAYMENTDATE,
                                 newInterest = (double)a.INTERATERATE,
                                 payAmount = (double?)a.PREPAYMENT ?? 0,
                                 operationId = a.OPERATIONTYPEID,
                                 newPrincipalFirstpaymentDate = (DateTime)a.PRINCIPALFIRSTPAYMENTDATE,
                                 isManagementInterestRate = a.ISMANAGEMENTINTERESTRATE,
                                 proposedTenor = a.TENOR,
                                 newMaturityDate = a.MATURITYDATE,// change to maturity date affter scarfolding
                                 companyId = b.COMPANYID,
                                 staffId = staffId,
                                 createdBy = staffId,
                                 customerId = b.CUSTOMERID,

                             }).ToList();

                    foreach (var item in model)
                    {
                        var unEarnedFee = from d in context.TBL_LOAN_SCHEDULE_DAILY
                                          where d.LOANID == item.loanId
                                          let sumUnEarnedFee = context.TBL_LOAN_SCHEDULE_DAILY.Where(a => a.LOANID == item.loanId
                                          && a.DATE >= DbFunctions.TruncateTime(item.newEffectiveDate)).Sum(a => (double?)a.UNEARNEDFEE ?? 0)
                                          select sumUnEarnedFee;
                        item.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;
                        string appDate = item.newEffectiveDate.ToString(@"yyyy-MM-dd");
                        var applicationDate = Convert.ToDateTime(appDate);
                        if ((int)OperationsEnum.ContractualInterestRateChange == item.operationId)
                        {
                            DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            item.interestRate = item.newInterest;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.principalFirstpaymentDate = nextPaymentDate;
                            item.interestFirstpaymentDate = nextPaymentDate;
                            InterestRateReview(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);

                        }
                        else if ((int)OperationsEnum.Prepayment == item.operationId)
                        {
                            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                            DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            if (item.isManagementInterestRate == true)
                            {
                                item.maturityDate = (DateTime)item.newMaturityDate;
                                //item.newAmount = ((item.principalAmount + (double)accruedInterest) - item.payAmount);
                                item.effectiveDate = item.newEffectiveDate;
                                item.tenor = item.newTenorPrepayment;
                                item.interestFirstpaymentDate = nextPaymentDate;
                                item.principalFirstpaymentDate = nextPaymentDate;
                            }
                            else
                            {
                                item.maturityDate = (DateTime)item.newMaturityDate;
                                //item.newAmount = ((item.principalAmount + (double)accruedInterest) - item.payAmount);
                                item.effectiveDate = item.newEffectiveDate;
                                item.tenor = item.newTenorPrepayment;
                                item.interestFirstpaymentDate = nextPaymentDate;
                                item.principalFirstpaymentDate = nextPaymentDate;
                            }

                            UpdateLoanPrepaymentSchedule(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                        else if ((int)OperationsEnum.PaymentDateChange == item.operationId)
                        {
                            item.interestFirstpaymentDate = (DateTime)item.newInterestFirstpaymentDate;
                            item.principalFirstpaymentDate = (DateTime)item.newPrincipalFirstpaymentDate;

                            PaymentDateChange(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                            updateLoanPrincipalInterestPaymentDate(item.interestFirstpaymentDate, item.principalFirstpaymentDate, loanId);

                        }
                        else if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId || (int)OperationsEnum.InterestFrequencyChange == item.operationId
                            || (int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                        {
                            if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId)
                            {
                                item.principalFrequency = (short)item.newPrincipalFrequency;
                                item.interestFrequency = (short)item.interestFrequency;
                            }
                            if ((int)OperationsEnum.InterestFrequencyChange == item.operationId)
                            {
                                item.interestFrequency = (short)item.newInterestFrequency;
                                item.principalFrequency = (short)item.principalFrequency;
                            }
                            if ((int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                            {
                                item.interestFrequency = (short)item.newInterestFrequency;
                                item.principalFrequency = (short)item.newPrincipalFrequency;
                            }
                            PaymentFrequencyChange(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                            updateLoanFrequency((short)item.principalFrequency, (short)item.interestFrequency, loanId);
                        }
                        else if ((int)OperationsEnum.CompleteWriteOff == item.operationId)
                        {
                            item.interestRate = item.newInterest;
                            item.interestFirstpaymentDate = (DateTime)item.newInterestFirstpaymentDate;//nextPaymentDate;
                            item.principalFirstpaymentDate = (DateTime)item.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            item.maturityDate = (DateTime)item.newMaturityDate;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.interestFrequency = (short)item.newInterestFrequency;
                            item.principalFrequency = (short)item.newPrincipalFrequency;
                            CompleteWriteOff(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                        else if ((int)OperationsEnum.TerminateAndRebook == item.operationId)
                        {
                            //LoanCancellation(loanId, applicationDate, staffId);
                            TerminateAndRebookLoanSchedule(loanId, item, applicationDate, staffId);
                            var loan = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                            var data  = (
                                        from b in context.TBL_LOAN
                                        where b.LOANSTATUSID == (short)LoanStatusEnum.Active && b.RELATED_LOAN_REFERENCE_NUMBER == loan.LOANREFERENCENUMBER
                                        select new LoanPaymentRestructureScheduleInputViewModel()
                                        {
                                            loanId = b.TERMLOANID,
                                            scheduleMethodId = b.SCHEDULETYPEID,
                                            principalAmount = (double)b.OUTSTANDINGPRINCIPAL,
                                            principalFrequency = b.PRINCIPALFREQUENCYTYPEID,
                                            interestFrequency = b.INTERESTFREQUENCYTYPEID,
                                            principalFirstpaymentDate = (DateTime)b.FIRSTPRINCIPALPAYMENTDATE,
                                            interestFirstpaymentDate = (DateTime)b.FIRSTINTERESTPAYMENTDATE,
                                            interestRate = b.INTERESTRATE,
                                            effectiveDate = b.EFFECTIVEDATE,
                                            maturityDate = b.MATURITYDATE,
                                            accrualBasis = b.SCHEDULEDAYCOUNTCONVENTIONID,
                                            firstDayType = b.SCHEDULEDAYINTERESTTYPEID,
                                            integralFeeAmount = 0,
                                            companyId = b.COMPANYID,
                                            staffId = staffId,
                                            createdBy = staffId,
                                            customerId = b.CUSTOMERID,

                                         }).ToList();

                            foreach (var item1 in data)
                            {
                                RegenerateSchedule(item1.loanId, item1, applicationDate, staffId);
                                updateLoanReviewOperation(loanReviewOperationsId, item1.loanId);
                            }
 
                        }
                        else if ((int)OperationsEnum.InterestSuspension == item.operationId)
                        {
                            InterestSuspension(loanId, applicationDate, staffId);

                        }
                        else if ((int)OperationsEnum.TenorChange == item.operationId)
                        {
                            item.maturityDate = item.maturityDate.AddMonths((int)item.proposedTenor);
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.tenor + (int)item.proposedTenor;
                            TenorExtension(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }

                        else if ((int)OperationsEnum.Restructured == item.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == item.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            item.interestRate = item.newInterest;
                            item.interestFirstpaymentDate = (DateTime)item.newInterestFirstpaymentDate;//nextPaymentDate;
                            item.principalFirstpaymentDate = (DateTime)item.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            item.maturityDate = (DateTime)item.newMaturityDate;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.interestFrequency = (short)item.newInterestFrequency;
                            item.principalFrequency = (short)item.newPrincipalFrequency;
                            Restructured(loanId, item, applicationDate, staffId);
                            updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        }
                    }
                }
            }

  
            context.SaveChanges();
            //-------------------------------------------------------
            output = true;

            return output;
        }

        public bool DocumentDeferral(int loanId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var data = (from a in context.TBL_LOAN
                        join b in context.TBL_CHECKLIST_DETAIL on a.LOANAPPLICATIONDETAILID equals b.TARGETID
                        where b.TARGETTYPEID == (short)CheckListTargetTypeEnum.LoanApplicationProductChecklist && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                        && b.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred && DbFunctions.TruncateTime(b.DEFEREDDATE) >= DbFunctions.TruncateTime(systemDate)
                        select new LoanRepaymentViewModel()
                        {
                            loanId = a.TERMLOANID,
                            checklistId = (int)b.CHECKLISTID,
                            loanRefNo = a.LOANREFERENCENUMBER,
                            loanApplicationNumberId = a.LOANAPPLICATIONDETAILID,
                            periodPrincipalAmount = a.OUTSTANDINGPRINCIPAL,
                            periodInterestAmount = a.OUTSTANDINGINTEREST,
                            productId = a.PRODUCTID,
                            casaAccountId = a.CASAACCOUNTID,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            currencyId = a.CURRENCYID,
                            exchangeRate = a.EXCHANGERATE,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATETIMECREATED,

                        }).ToList();

            foreach (var item in data)
            {
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment Debt as a result of Defer Document"));

                inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment Debt as a result of Defer Document"));

                financeTransaction.PostTransaction(inputTransactions);

            }

            context.SaveChanges();

            output = true;

            return output;
        }

        public bool SinkingFund(int loanId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var covenant = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
                           where a.TERMLOANID == loanId
                           let percentage = b.ISPERCENTAGE
                           select percentage;

            //var covenantAmount  = from a in context.TBL_LOAN
            //                     join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
            //                     join c in context.TBL_LOAN_COVENANT_TYPE on b.COVENANTTYPEID equals c.COVENANTTYPEID
            //                     where b.COVENANTTYPEID == c.COVENANTTYPEID && a.TERMLOANID == b.LOANID
            //                     && c.COVENANTTYPEID == (short)LoanCovenantTypeEnum.SinkingFund && a.TERMLOANID == loanId
            //                     && b.NEXTCOVENANTDATE == DbFunctions.TruncateTime(systemDate)
            //                     select b.COVENANTAMOUNT;


            if (covenant.FirstOrDefault() == false)
            {
                var covenantAmount = from a in context.TBL_LOAN
                                     join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
                                     join c in context.TBL_LOAN_COVENANT_TYPE on b.COVENANTTYPEID equals c.COVENANTTYPEID
                                     where b.COVENANTTYPEID == c.COVENANTTYPEID && a.TERMLOANID == b.LOANID
                                     && c.COVENANTTYPEID == (short)LoanCovenantTypeEnum.SinkingFund && a.TERMLOANID == loanId
                                     && b.NEXTCOVENANTDATE == DbFunctions.TruncateTime(systemDate)
                                     select b.COVENANTAMOUNT;
            }
            else
            {
                var covenantRate = from a in context.TBL_LOAN
                                   join b in context.TBL_LOAN_COVENANT_DETAIL on a.TERMLOANID equals b.LOANID
                                   join c in context.TBL_LOAN_COVENANT_TYPE on b.COVENANTTYPEID equals c.COVENANTTYPEID
                                   where b.COVENANTTYPEID == c.COVENANTTYPEID && a.TERMLOANID == b.LOANID
                                   && c.COVENANTTYPEID == (short)LoanCovenantTypeEnum.SinkingFund && a.TERMLOANID == loanId
                                   && b.NEXTCOVENANTDATE == DbFunctions.TruncateTime(systemDate)
                                   select b.COVENANTAMOUNT;
            }


            //select sumPrincipalAmount;
            //var data = (from a in context.TBL_LOAN
            //            join b in context.TBL_CHECKLIST_DETAIL on a.LOANAPPLICATIONDETAILID equals b.TARGETID
            //            where b.TARGETTYPEID == (short)CheckListTargetTypeEnum.Loan && a.LOANSTATUSID == (short)LoanStatusEnum.Active
            //            && b.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred && DbFunctions.TruncateTime(b.DEFEREDDATE) >= DbFunctions.TruncateTime(systemDate)
            //            select new LoanRepaymentViewModel()
            //            {
            //                loanId = a.TERMLOANID,
            //                checklistId = (int)b.CHECKLISTID,
            //                loanRefNo = a.LOANREFERENCENUMBER,
            //                loanApplicationNumberId = a.LOANAPPLICATIONDETAILID,
            //                periodPrincipalAmount = a.OUTSTANDINGPRINCIPAL,
            //                periodInterestAmount = a.OUTSTANDINGINTEREST,
            //                productId = a.PRODUCTID,
            //                casaAccountId = a.CASAACCOUNTID,
            //                branchId = a.BRANCHID,
            //                companyId = a.COMPANYID,
            //                currencyId = a.CURRENCYID,
            //                exchangeRate = a.EXCHANGERATE,
            //                createdBy = a.CREATEDBY,
            //                dateTimeCreated = a.DATETIMECREATED,

            //            }).ToList();

            //foreach (var item in data)
            //{
            //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
            //    var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);

            //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment Debt as a result of Defer Document"));

            //    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment Debt as a result of Defer Document"));

            //    financeTransaction.PostTransaction(inputTransactions);

            //}



            context.SaveChanges();

            output = true;

            return output;


        }

        #region Commercial Paper  Operation

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperRateReview(int aplicationId, double newRate, DateTime applicationDate, int staffId, int operationId)
        {
            bool output = false;

            TBL_LOAN_APPLICATION result = (from p in context.TBL_LOAN_APPLICATION
                                           where p.LOANAPPLICATIONID == aplicationId
                                           select p).SingleOrDefault();

            result.INTERESTRATE = newRate;



            context.SaveChanges();
            output = true;

            return output;
        }

        public bool CommercialPaperTenorReview(int aplicationId, int newTenor, DateTime applicationDate, int staffId, int operationId)
        {
            bool output = false;

            TBL_LOAN_APPLICATION result = (from p in context.TBL_LOAN_APPLICATION
                                           where p.LOANAPPLICATIONID == aplicationId
                                           select p).SingleOrDefault();

            result.APPLICATIONTENOR = newTenor;



            context.SaveChanges();
            output = true;

            return output;
        }

        public bool CommercialPaperTenorReviewDetails(int loanAplicationDetailId, int newTenor, DateTime applicationDate, int staffId, int operationId)
        {
            bool output = false;

            TBL_LOAN_APPLICATION_DETAIL result = (from p in context.TBL_LOAN_APPLICATION_DETAIL
                                                  where p.LOANAPPLICATIONDETAILID == loanAplicationDetailId
                                                  select p).SingleOrDefault();

            result.PROPOSEDTENOR = newTenor;
            result.APPROVEDTENOR = newTenor;



            context.SaveChanges();
            output = true;

            return output;
        }

        public void CommercialPaperChangeOperativeAccount(int casaPayAccountId, int newCasaPayAccountId)
        { //TODO rework
            TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
                                         where //p.CASAACCOUNTID2 == casaPayAccountId &&
                                 p.LOANSTATUSID == (short)LoanStatusEnum.Active
                                         select p).SingleOrDefault();
            var casa = this.context.TBL_CASA.Where(x => x.CASAACCOUNTID == newCasaPayAccountId && x.ACCOUNTSTATUSID == (short)CASAAccountStatusEnum.Active).FirstOrDefault().CASAACCOUNTID;

          // result.CASAACCOUNTID2 = casa;
            context.SaveChanges();
        }

        public IEnumerable<LoanApplicationViewModel> ArchiveLoanApplication(int aplicationId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_APPLICATION
                         where a.APPROVALSTATUSID == (short)LoanStatusEnum.Active && a.LOANAPPLICATIONID == aplicationId

                         select new LoanApplicationViewModel()
                         {
                             loanApplicationId = a.LOANAPPLICATIONID,
                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                             loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                             customerId = a.CUSTOMERID,
                             companyId = a.COMPANYID,
                             branchId = a.BRANCHID,
                             customerGroupId = a.CUSTOMERGROUPID,
                             loanTypeId = a.LOANAPPLICATIONTYPEID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             casaAccountId = a.CASAACCOUNTID,
                             newApplicationDate = a.APPLICATIONDATE,
                             interestRate = a.INTERESTRATE,
                             applicationTenor = a.APPLICATIONTENOR,
                             effectiveDate = a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().EFFECTIVEDATE,
                             expiryDate = a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().EXPIRYDATE,
                             operationId = a.OPERATIONID,
                             productClassId = (short)a.PRODUCTCLASSID,
                             applicationAmount = a.APPLICATIONAMOUNT,
                             approvedAmount = a.APPROVEDAMOUNT,
                             loanInformation = a.LOANINFORMATION,
                             misCode = a.MISCODE,
                             teamMisCode = a.TEAMMISCODE,
                             isInvestmentGrade = a.ISINVESTMENTGRADE,
                             isRelatedParty = a.ISRELATEDPARTY,
                             isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                             lastUpdatedBy = (int)a.LASTUPDATEDBY,
                             dateTimeUpdated = a.DATETIMEUPDATED,
                             deleted = a.DELETED,
                             deletedBy = a.DELETEDBY,
                             dateTimeDeleted = a.DATETIMEDELETED,
                             approvalStatusId = a.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                             customerInfoValidated = a.CUSTOMERINFOVALIDATED,
                             //notInNegativeCrms = a.NOTINNEGATIVECRMS,
                             //notInBlackbook = a.NOTINBLACKBOOK,
                             //notInCamsol = a.NOTINCAMSOL,
                             //notInXds = a.NOTINXDS,
                             //notInCrc = a.NOTINCRC,

                         }).ToList();

            List<TBL_LOAN_APPLICATION_ARCHIVE> LoanApplicationArchive = new List<TBL_LOAN_APPLICATION_ARCHIVE>();

            foreach (var item in model)
            {

                TBL_LOAN_APPLICATION_ARCHIVE addLoanApplicationArchive = new TBL_LOAN_APPLICATION_ARCHIVE();

                addLoanApplicationArchive.ARCHIVEDATE = DateTime.Today;
                addLoanApplicationArchive.LOANAPPLICATIONID = item.loanApplicationId;
                addLoanApplicationArchive.APPLICATIONREFERENCENUMBER = item.applicationReferenceNumber;
                addLoanApplicationArchive.LOANPRELIMINARYEVALUATIONID = item.loanPreliminaryEvaluationId;
                addLoanApplicationArchive.CUSTOMERID = item.customerId;
                addLoanApplicationArchive.COMPANYID = item.companyId;
                addLoanApplicationArchive.BRANCHID = (short)item.branchId;
                addLoanApplicationArchive.CUSTOMERGROUPID = item.customerGroupId;
                addLoanApplicationArchive.LOANAPPLICATIONTYPEID = item.loanTypeId;
                addLoanApplicationArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addLoanApplicationArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addLoanApplicationArchive.CASAACCOUNTID = item.casaAccountId;
                addLoanApplicationArchive.APPLICATIONDATE = item.newApplicationDate;
                addLoanApplicationArchive.INTERESTRATE = item.interestRate;
                addLoanApplicationArchive.APPLICATIONTENOR = (int)item.applicationTenor;
                addLoanApplicationArchive.EFFECTIVEDATE = item.effectiveDate;
                addLoanApplicationArchive.EXPIRYDATE = item.expiryDate;
                addLoanApplicationArchive.OPERATIONID = (int)item.operationId;
                addLoanApplicationArchive.PRODUCTCLASSID = item.productClassId;
                addLoanApplicationArchive.APPLICATIONAMOUNT = item.applicationAmount;
                addLoanApplicationArchive.APPROVEDAMOUNT = item.approvedAmount;
                addLoanApplicationArchive.LOANINFORMATION = item.loanInformation;
                addLoanApplicationArchive.MISCODE = item.misCode;
                addLoanApplicationArchive.TEAMMISCODE = item.teamMisCode;
                addLoanApplicationArchive.ISINVESTMENTGRADE = item.isInvestmentGrade;
                addLoanApplicationArchive.ISRELATEDPARTY = item.isRelatedParty;
                addLoanApplicationArchive.ISPOLITICALLYEXPOSED = item.isPoliticallyExposed;
                addLoanApplicationArchive.CREATEDBY = item.createdBy;
                addLoanApplicationArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanApplicationArchive.LASTUPDATEDBY = item.lastUpdatedBy;
                addLoanApplicationArchive.DATETIMEUPDATED = item.dateTimeUpdated;
                addLoanApplicationArchive.DELETED = item.deleted;
                addLoanApplicationArchive.DELETEDBY = item.deletedBy;
                addLoanApplicationArchive.DATETIMEDELETED = item.dateTimeDeleted;
                addLoanApplicationArchive.APPROVALSTATUSID = item.approvalStatusId;
                addLoanApplicationArchive.APPLICATIONSTATUSID = item.applicationStatusId;
                addLoanApplicationArchive.SUBMITTEDFORAPPRAISAL = item.submittedForAppraisal;
                addLoanApplicationArchive.CUSTOMERINFOVALIDATED = item.customerInfoValidated;
                addLoanApplicationArchive.NOTINNEGATIVECRMS = item.notInNegativeCrms;
                addLoanApplicationArchive.NOTINBLACKBOOK = item.notInNegativeCrms;
                addLoanApplicationArchive.NOTINBLACKBOOK = item.notInBlackbook;
                addLoanApplicationArchive.NOTINCAMSOL = item.notInCamsol;
                addLoanApplicationArchive.NOTINXDS = item.notInXds;
                addLoanApplicationArchive.NOTINCRC = item.notInCrc;
                addLoanApplicationArchive.OPERATIONID = (int)item.operationId;
                addLoanApplicationArchive.CUSTOMERGROUPID = item.customerGroupId;
                addLoanApplicationArchive.LOANAPPLICATIONTYPEID = item.loanTypeId;

                LoanApplicationArchive.Add(addLoanApplicationArchive);

            }

            this.context.TBL_LOAN_APPLICATION_ARCHIVE.AddRange(LoanApplicationArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanApplicationDetailViewModel> ArchiveLoanApplicationDetails(int aplicationId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                         where a.LOANAPPLICATIONID == aplicationId

                         select new LoanApplicationDetailViewModel()
                         {
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             loanApplicationId = a.LOANAPPLICATIONID,
                             customerId = a.CUSTOMERID,
                             proposedProductId = a.PROPOSEDPRODUCTID,
                             proposedTenor = a.PROPOSEDTENOR,
                             proposedInterestRate = a.PROPOSEDINTERESTRATE,
                             proposedAmount = a.PROPOSEDAMOUNT,
                             approvedProductId = a.APPROVEDPRODUCTID,
                             approvedTenor = a.APPROVEDTENOR,
                             approvedInterestRate = a.APPROVEDINTERESTRATE,
                             approvedAmount = a.APPROVEDAMOUNT,
                             currencyId = a.CURRENCYID,
                             exchangeRate = a.EXCHANGERATE,
                             subSectorId = a.SUBSECTORID,
                             statusId = a.STATUSID,
                             loanPurpose = a.LOANPURPOSE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                             lastUpdatedBy = (int)a.LASTUPDATEDBY,
                             dateTimeUpdated = a.DATETIMEUPDATED,
                             deleted = a.DELETED,
                             deletedBy = a.DELETEDBY,
                             dateTimeDeleted = a.DATETIMEDELETED,

                         }).ToList();

            List<TBL_LOAN_APPLICATION_DETL_ARCH> LoanApplicationDetailsArchive = new List<TBL_LOAN_APPLICATION_DETL_ARCH>();

            foreach (var item in model)
            {
                TBL_LOAN_APPLICATION_DETL_ARCH addLoanApplDetailsArchive = new TBL_LOAN_APPLICATION_DETL_ARCH();


                addLoanApplDetailsArchive.ARCHIVEDATE = DateTime.Today;
                addLoanApplDetailsArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addLoanApplDetailsArchive.LOANAPPLICATIONID = item.loanApplicationId;
                addLoanApplDetailsArchive.CUSTOMERID = item.customerId;
                addLoanApplDetailsArchive.PROPOSEDPRODUCTID = item.proposedProductId;
                addLoanApplDetailsArchive.PROPOSEDTENOR = item.proposedTenor;
                addLoanApplDetailsArchive.PROPOSEDINTERESTRATE = item.proposedInterestRate;
                addLoanApplDetailsArchive.PROPOSEDAMOUNT = item.proposedAmount;
                addLoanApplDetailsArchive.APPROVEDPRODUCTID = item.approvedProductId;
                addLoanApplDetailsArchive.APPROVEDTENOR = item.approvedTenor;
                addLoanApplDetailsArchive.APPROVEDINTERESTRATE = item.approvedInterestRate;
                addLoanApplDetailsArchive.APPROVEDAMOUNT = item.approvedAmount;
                addLoanApplDetailsArchive.CURRENCYID = item.currencyId;
                addLoanApplDetailsArchive.EXCHANGERATE = item.exchangeRate;
                addLoanApplDetailsArchive.SUBSECTORID = item.subSectorId;
                addLoanApplDetailsArchive.STATUSID = item.statusId;
                addLoanApplDetailsArchive.LOANPURPOSE = item.loanPurpose;
                addLoanApplDetailsArchive.CREATEDBY = item.createdBy;
                addLoanApplDetailsArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanApplDetailsArchive.LASTUPDATEDBY = item.lastUpdatedBy;
                addLoanApplDetailsArchive.DATETIMEUPDATED = item.dateTimeUpdated;
                addLoanApplDetailsArchive.DELETED = item.deleted;
                addLoanApplDetailsArchive.DELETEDBY = item.deletedBy;
                addLoanApplDetailsArchive.DATETIMEDELETED = item.dateTimeDeleted;

                LoanApplicationDetailsArchive.Add(addLoanApplDetailsArchive);

            }

            this.context.TBL_LOAN_APPLICATION_DETL_ARCH.AddRange(LoanApplicationDetailsArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<RevolvingLoanViewModel> ArchiveRevolvingLoan(int aplicationId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         where a.APPROVALSTATUSID == (short)LoanStatusEnum.Active && a.LOANAPPLICATIONDETAILID == aplicationId

                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             branchId = a.BRANCHID,
                             customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                             loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             casaAccountId = a.CASAACCOUNTID,
                             //casaAccountId2 = a.CASAACCOUNTID2,
                             currencyId = a.CURRENCYID,
                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                             exchangeRate = a.EXCHANGERATE,
                             loanReferenceNumber = a.LOANREFERENCENUMBER,
                             relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                             subSectorId = a.SUBSECTORID,
                             maturityDate = a.MATURITYDATE,
                             bookingDate = a.BOOKINGDATE,
                             interestRate = a.INTERESTRATE,
                             effectiveDate = a.EFFECTIVEDATE,
                             operationId = a.OPERATIONID,
                             misCode = a.MISCODE,
                             teamMisCode = a.TEAMMISCODE,
                             overdraftLimit = a.OVERDRAFTLIMIT,
                             //disbursedAmount = a.DISBURSED_AMOUNT,
                             //interestAmount = a.INTEREST_AMOUNT,
                             approverComment = a.APPROVERCOMMENT,
                             approvedBy = (int)a.APPROVEDBY,
                             dateApproved = a.DATEAPPROVED,
                             loanStatusId = a.LOANSTATUSID,
                             isDisbursed = a.ISDISBURSED,
                             disbursedBy = a.DISBURSEDBY,
                             disburserComment = a.DISBURSERCOMMENT,
                             disburseDate = a.DISBURSEDATE,
                             // trancheBatchCode = a.TRANCHEBATCHCODE,
                             dischargeLetter = a.DISCHARGELETTER,
                             suspendInterest = a.SUSPENDINTEREST,
                             internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                             externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                             approvalStatusId = a.APPROVALSTATUSID,


                         }).ToList();

            List<TBL_LOAN_REVOLVING_ARCHIVE> LoanRevolvingArchive = new List<TBL_LOAN_REVOLVING_ARCHIVE>();



            foreach (var item in model)
            {

                TBL_LOAN_REVOLVING_ARCHIVE addLoanRevolvingArchive = new TBL_LOAN_REVOLVING_ARCHIVE();
                addLoanRevolvingArchive.ARCHIVEDATE = DateTime.Today;
                addLoanRevolvingArchive.CUSTOMERID = item.customerId;
                addLoanRevolvingArchive.PRODUCTID = item.productId;
                addLoanRevolvingArchive.COMPANYID = item.companyId;
                addLoanRevolvingArchive.BRANCHID = item.branchId;
                addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID = item.customerGroupId;
                addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID = item.loanTypeId;
                addLoanRevolvingArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                addLoanRevolvingArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                addLoanRevolvingArchive.CASAACCOUNTID = item.casaAccountId;
               // addLoanRevolvingArchive.CASAACCOUNTID2 = item.casaAccountId2;
                addLoanRevolvingArchive.CURRENCYID = (short)item.currencyId;
                addLoanRevolvingArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                addLoanRevolvingArchive.EXCHANGERATE = item.exchangeRate;
                addLoanRevolvingArchive.LOANREFERENCENUMBER = item.loanReferenceNumber;
                addLoanRevolvingArchive.RELATED_LOAN_REFERENCE_NUMBER = item.relatedLoanReferenceNumber;
                addLoanRevolvingArchive.SUBSECTORID = item.subSectorId;
                addLoanRevolvingArchive.MATURITYDATE = item.maturityDate;
                addLoanRevolvingArchive.BOOKINGDATE = item.bookingDate;
                addLoanRevolvingArchive.INTERESTRATE = item.interestRate;
                addLoanRevolvingArchive.EFFECTIVEDATE = item.effectiveDate;
                addLoanRevolvingArchive.OPERATIONID = item.operationId;
                addLoanRevolvingArchive.MISCODE = item.misCode;
                addLoanRevolvingArchive.TEAMMISCODE = item.teamMiscode;
                addLoanRevolvingArchive.OVERDRAFTLIMIT = item.overdraftLimit;
                //addLoanRevolvingArchive.DISBURSED_AMOUNT = item.disbursedAmount;
                //addLoanRevolvingArchive.INTEREST_AMOUNT = item.interestAmount;
                addLoanRevolvingArchive.APPROVALSTATUSID = item.approvalStatusId;
                addLoanRevolvingArchive.APPROVEDBY = item.approvedBy;
                addLoanRevolvingArchive.APPROVERCOMMENT = item.approverComment;
                addLoanRevolvingArchive.DATEAPPROVED = item.dateApproved;
                addLoanRevolvingArchive.LOANSTATUSID = item.loanStatusId;
                addLoanRevolvingArchive.CREATEDBY = item.createdBy;
                addLoanRevolvingArchive.DATETIMECREATED = item.dateTimeCreated;
                addLoanRevolvingArchive.ISDISBURSED = item.isDisbursed;
                addLoanRevolvingArchive.DISBURSEDBY = item.disbursedBy;
                addLoanRevolvingArchive.DISBURSERCOMMENT = item.disburserComment;
                addLoanRevolvingArchive.DISBURSEDATE = item.disburseDate;
                addLoanRevolvingArchive.OPERATIONID = (int)item.operationId;
                addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID = item.customerGroupId;
                addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID = item.loanTypeId;
                // addLoanRevolvingArchive.TRANCHEBATCHCODE = item.trancheBatchCode;
                addLoanRevolvingArchive.DISCHARGELETTER = item.dischargeLetter;
                addLoanRevolvingArchive.SUSPENDINTEREST = item.suspendInterest;
                addLoanRevolvingArchive.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
                addLoanRevolvingArchive.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
                addLoanRevolvingArchive.NPLDATE = item.nplDate;
                addLoanRevolvingArchive.CREATEDBY = item.createdBy;
                addLoanRevolvingArchive.DATETIMECREATED = item.dateTimeCreated;

                LoanRevolvingArchive.Add(addLoanRevolvingArchive);

            }

            this.context.TBL_LOAN_REVOLVING_ARCHIVE.AddRange(LoanRevolvingArchive);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperCancellation(int aplicationId, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            TBL_LOAN_APPLICATION result = (from p in context.TBL_LOAN_APPLICATION
                                           where p.LOANAPPLICATIONID == aplicationId
                                           select p).SingleOrDefault();

            result.APPLICATIONSTATUSID = (short)LoanStatusEnum.Cancelled;

            context.SaveChanges();

            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperDetailsCancellation(string refNo, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
                                         where p.LOANREFERENCENUMBER == refNo
                                         select p).SingleOrDefault();

            result.LOANSTATUSID = (short)LoanStatusEnum.Cancelled;

            context.SaveChanges();

            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperPrepayment(string refNo, decimal prepaymentAmount, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();



            TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
                                         where p.LOANREFERENCENUMBER == refNo
                                         select p).SingleOrDefault();


            if (result.OVERDRAFTLIMIT == prepaymentAmount)
            {
                result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT - prepaymentAmount;
                result.LOANSTATUSID = (short)LoanStatusEnum.Completed;
            }
            else
            {
                result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT - prepaymentAmount;
            }


            context.SaveChanges();

            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperRollOver(string refNo, decimal prepaymentAmount, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
                                         where p.LOANREFERENCENUMBER == refNo
                                         select p).SingleOrDefault();

            if (result.OVERDRAFTLIMIT == prepaymentAmount)
            {
                result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT;
                result.LOANSTATUSID = (short)LoanStatusEnum.Active;
            }
            else
            {
                result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT + prepaymentAmount;
                result.LOANSTATUSID = (short)LoanStatusEnum.Active;
            }

            context.SaveChanges();

            output = true;

            return output;

        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyCommercialPaperInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.TBL_LOAN_REVOLVING
                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.PRODUCTID == (short)ProductClassEnum.Commercial


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LOANREFERENCENUMBER,
                            productId = a.PRODUCTID,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            currencyId = a.CURRENCYID,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = a.INTERESTRATE,
                            mainAmount = a.OVERDRAFTLIMIT,
                            categoryId = (short)DailyAccrualCategory.AuthorisedOverdraft,
                            //availableBalance = b.AVAILABLEBALANCE,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            //baseReferenceNumber = null,
                            //dayCountConventionId = c.DAYCOUNTCONVENTIONID,
                            //daysInAYear = c.DAYSINAYEAR,

                        });

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.TBL_DAILY_ACCRUAL
                         where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.AuthorisedOverdraft
                         group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.PRODUCTID,
                             branchId = groupedQ.Key.BRANCHID,
                             companyId = groupedQ.Key.COMPANYID,
                             currencyId = groupedQ.Key.CURRENCYID,
                             exchangeRate = groupedQ.Key.EXCHANGERATE,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AutomaticInterestRepricing(DateTime applicationDate)
        {
            bool output = false;
            //var systemDate = generalSetup.GetApplicationDate();


            var dailyRepricing = (from d in context.TBL_PRODUCT_PRICE_INDEX_DAILY
                                  where d.DATE >= DbFunctions.TruncateTime(applicationDate.AddDays(-90)) && d.DATE <= DbFunctions.TruncateTime(applicationDate)
                                  select new
                                  {
                                      d.PRICEINDEXRATE
                                  }).ToList();
            var sumdailyRepricing = dailyRepricing.Select(c => c.PRICEINDEXRATE).Sum();


            /// update the price index rate
            //context.SaveChanges();

            output = true;

            return output;

        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetBandGReadyForRenewal()
        {

            var data = (from ln in context.TBL_LOAN_CONTINGENT
                        where ln.ISTENORED == true && DbFunctions.DiffDays(ln.EFFECTIVEDATE, DateTime.Now) > 365
                        orderby ln.EFFECTIVEDATE descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.CONTINGENTLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
                            isBankFormat = ln.ISBANKFORMAT,
                            companyId = ln.COMPANYID,
                            exchangeRate = ln.EXCHANGERATE,
                            approvedAmount = ln.CONTINGENTAMOUNT,
                            dateTimeCreated = ln.DATETIMECREATED,
                            createdByName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.LASTNAME,
                            dischargeLetter = ln.DISCHARGELETTER,

                            relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                            relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                            misCode = ln.MISCODE,
                            teamMiscode = ln.TEAMMISCODE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,

                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,

                            isDisbursed = ln.ISDISBURSED,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                            operationId = ln.OPERATIONID,
                            loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,
                            currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            loanStatusId = ln.LOANSTATUSID,
                            comment = "",
                        }).ToList();
            return data;
        }
        #endregion
    }
}
