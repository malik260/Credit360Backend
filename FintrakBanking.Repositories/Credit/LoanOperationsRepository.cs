using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Validation;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using FintrakBanking.Common.CustomException;
using FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI;

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
        private ILoanRepository loanGenerate;
        private IOverDraftValidation validate;

        private FinTrakBankingStagingContext stagingContext;
        private IIntegrationWithFinacle finacle;
        bool USE_THIRD_PARTY_INTEGRATION = false;
        public LoanOperationsRepository(
        FinTrakBankingContext _context, IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail,
            ILoanScheduleRepository _loanSchedule, IWorkflow _workFlow, IApprovalLevelStaffRepository _level, ICasaLienRepository _casaLien
            , ILoanRepository _loan, IOverDraftValidation validate, IIntegrationWithFinacle finacle, FinTrakBankingStagingContext _stagingContext)
        {

            this.context = _context;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.workFlow = _workFlow;
            this.level = _level;
            this.casaLien = _casaLien;
            this.loanGenerate = _loan;
            this.finacle = finacle;
            this.stagingContext = _stagingContext;

            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;
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
            try
            {
                bool result = false;
                var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

                var data1 = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                             join c in context.TBL_LOAN_SCHEDULE_PERIODIC on b.TERMLOANID equals c.LOANID
                             join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                             where a.DATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                             && a.PAYMENTDATE == c.PAYMENTDATE && b.PRODUCTID != (short)ProductClassEnum.Commercial

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

                var data2 = (from a in context.TBL_LOAN
                             where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.PRODUCTID == (short)ProductClassEnum.Commercial
                             && a.MATURITYDATE == DbFunctions.TruncateTime(applicationDate)


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
                                 dailyAccuralAmount = ((a.INTERESTRATE / 100) * (double)a.PRINCIPALAMOUNT * 1 / 365),//((a.INTERESTRATE / 100) * (double)a.PRINCIPALAMOUNT * 1 / 365),
                                 mainAmount = a.PRINCIPALAMOUNT,
                                 categoryId = (short)DailyAccrualCategory.AuthorisedOverdraft,/// change to commercial paper 
                                 availableBalance = a.PRINCIPALAMOUNT,
                                 transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                                 baseReferenceNumber = null,
                                 dayCountConventionId = 0,


                             }).ToList();
                var data = data1.Union(data2).ToList();

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

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyTermLoanInterestAccuralToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }

                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }

        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyFeeAccrual(DateTime applicationDate)
        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                bool result = false;
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
                                dailyAccuralAmount = (double)DailyAccruedInterest((DateTime)b.DATEAPPROVED, b.MATURITYDATE, a.FEEAMOUNT),
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

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyFeeAccuralToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;

                //foreach (var item in model)
                //{
                //    item.date = applicationDate;

                //    var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                //    count++;

                //    //addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                //    //addStaging.FLOWTYPE = "fff";
                //    //addStaging.FORCEDEBITACCOUNT = "Y";
                //    //addStaging.VALUEDATENUMBER = 1;
                //    //addStaging.BATCHID = batchCode;
                //    //addStaging.BATCHREFID = count;
                //    //addStaging.SID = count;
                //    //addStaging.COMPANYID = item.companyId;
                //    //addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    //addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    //addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                //    //addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                //    //addStaging.DESCRIPTION = "Fee Daily Interest Accrual Posting";
                //    //addStaging.DESTINATIONBRANCHID = item.branchId;
                //    //addStaging.ISPOSTED = false;
                //    //addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                //    //addStaging.POSTEDBY = "SYSTEM";
                //    //addStaging.POSTEDDATE = item.date;
                //    //addStaging.SOURCEBRANCHID = item.branchId;
                //    //addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                //    //addStaging.VALUEDATE = item.date;
                //    //addStaging.TRANSACTIONTYPE = "BP";
                //    //addStaging.BANKID = "01";
                //    //this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                //    //context.SaveChanges();
                //    if (setup.USE_THIRD_PARTY_INTEGRATION == false)
                //    {
                //        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                //    }

                //    //financeTransaction.PostDailyLoansInterestAccrual(item);/// change to Daily Fee Accrued 

                //}
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //    result = WriteBulkPostingToStaging(applicationDate, "BP", batchCode);

                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }

        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTaxAccrual(DateTime applicationDate)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                bool result = false;
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
                                // dailyAccuralAmount = (double)DailyAccruedInterest((DateTime)b.LASTRESTRUCTUREDATE, b.MATURITYDATE, a.TAXAMOUNT),
                                // mainAmount = a.TAXAMOUNT,
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
                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyTaxAccuralToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;


                //foreach (var item in model)
                //{
                //    item.date = applicationDate;

                //    var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                //    count++;

                //    addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                //    addStaging.FLOWTYPE = "fff";
                //    addStaging.FORCEDEBITACCOUNT = "Y";
                //    addStaging.VALUEDATENUMBER = 1;
                //    addStaging.BATCHID = batchCode;
                //    addStaging.BATCHREFID = count;
                //    addStaging.SID = count;
                //    addStaging.COMPANYID = item.companyId;
                //    addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                //    addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                //    addStaging.DESCRIPTION = "Tax Daily Interest Accrual Posting";
                //    addStaging.DESTINATIONBRANCHID = item.branchId;
                //    addStaging.ISPOSTED = false;
                //    addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                //    addStaging.POSTEDBY = "SYSTEM";
                //    addStaging.POSTEDDATE = item.date;
                //    addStaging.SOURCEBRANCHID = item.branchId;
                //    addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                //    addStaging.VALUEDATE = item.date;
                //    addStaging.TRANSACTIONTYPE = "BP";
                //    addStaging.BANKID = "01";
                //    this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                //    context.SaveChanges();
                //    if (setup.USE_THIRD_PARTY_INTEGRATION == false)
                //    {
                //        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                //    }
                //    //financeTransaction.PostDailyLoansInterestAccrual(item);/// change to Daily Tax Accrued 

                //}
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //    result = WriteBulkPostingToStaging(applicationDate, "BP", batchCode);

                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            //    using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                bool result = false;
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

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyAuthorisedOverdraftInterestAccuralToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;

                //foreach (var item in model)
                //{
                //    item.date = applicationDate;

                //    var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                //    count++;

                //    addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                //    addStaging.FLOWTYPE = "fff";
                //    addStaging.FORCEDEBITACCOUNT = "Y";
                //    addStaging.VALUEDATENUMBER = 1;
                //    addStaging.BATCHID = batchCode;
                //    addStaging.BATCHREFID = count;
                //    addStaging.SID = count;
                //    addStaging.COMPANYID = item.companyId;
                //    addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                //    addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                //    addStaging.DESCRIPTION = "Authorised Overdraft Daily Interest Accrual Posting";
                //    addStaging.DESTINATIONBRANCHID = item.branchId;
                //    addStaging.ISPOSTED = false;
                //    addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                //    addStaging.POSTEDBY = "SYSTEM";
                //    addStaging.POSTEDDATE = item.date;
                //    addStaging.SOURCEBRANCHID = item.branchId;
                //    addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                //    addStaging.VALUEDATE = item.date;
                //    addStaging.TRANSACTIONTYPE = "BP";
                //    addStaging.BANKID = "01";
                //    this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                //    context.SaveChanges();
                //    if (setup.USE_THIRD_PARTY_INTEGRATION == false)
                //    {
                //        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                //    }
                //    // financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
                //}
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //    result = WriteBulkPostingToStaging(applicationDate, "BP", batchCode);

                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            //    using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                bool result = false;
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

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyUnauthorisedOverdraftInterestAccuralToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;

                //foreach (var item in model)
                //{
                //    item.date = applicationDate;

                //    var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                //    count++;

                //    addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                //    addStaging.FLOWTYPE = "fff";
                //    addStaging.FORCEDEBITACCOUNT = "Y";
                //    addStaging.VALUEDATENUMBER = 1;
                //    addStaging.BATCHID = batchCode;
                //    addStaging.BATCHREFID = count;
                //    addStaging.SID = count;
                //    addStaging.COMPANYID = item.companyId;
                //    addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                //    addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                //    addStaging.DESCRIPTION = "Unauthorised Overdraft Daily Interest Accrual Posting";
                //    addStaging.DESTINATIONBRANCHID = item.branchId;
                //    addStaging.ISPOSTED = false;
                //    addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                //    addStaging.POSTEDBY = "SYSTEM";
                //    addStaging.POSTEDDATE = item.date;
                //    addStaging.SOURCEBRANCHID = item.branchId;
                //    addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                //    addStaging.VALUEDATE = item.date;
                //    addStaging.TRANSACTIONTYPE = "BP";
                //    addStaging.BANKID = "01";
                //    this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                //    context.SaveChanges();
                //    if (setup.USE_THIRD_PARTY_INTEGRATION == false)
                //    {
                //        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                //    }
                //    //financeTransaction.PostDailyUnauthorisedOverdraftInterestAccrual(item);
                //}

                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //    result = WriteBulkPostingToStaging(applicationDate, "BP", batchCode);

                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }

        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDueInterestAccrual(DateTime applicationDate)

        {

            //    using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                bool result = false;
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
                //var count = data.Count();

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

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyPastDueInterestAccrualToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;

                //foreach (var item in model)
                //{
                //    item.date = applicationDate;

                //    var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                //    count++;

                //    addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                //    addStaging.FLOWTYPE = "fff";
                //    addStaging.FORCEDEBITACCOUNT = "Y";
                //    addStaging.VALUEDATENUMBER = 1;
                //    addStaging.BATCHID = batchCode;
                //    addStaging.BATCHREFID = count;
                //    addStaging.SID = count;
                //    addStaging.COMPANYID = item.companyId;
                //    addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                //    addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                //    addStaging.DESCRIPTION = "Past Due Daily Interest Accrual Posting";
                //    addStaging.DESTINATIONBRANCHID = item.branchId;
                //    addStaging.ISPOSTED = false;
                //    addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                //    addStaging.POSTEDBY = "SYSTEM";
                //    addStaging.POSTEDDATE = item.date;
                //    addStaging.SOURCEBRANCHID = item.branchId;
                //    addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                //    addStaging.VALUEDATE = item.date;
                //    addStaging.TRANSACTIONTYPE = "BP";
                //    addStaging.BANKID = "01";
                //    this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                //    context.SaveChanges();
                //    if (setup.USE_THIRD_PARTY_INTEGRATION == false)
                //    {
                //        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                //    }
                //    //financeTransaction.PostDailyPastDueInterestAccrual(item);
                //}
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //    result = WriteBulkPostingToStaging(applicationDate, "BP", batchCode);

                if (result)
                {
                    //  trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDuePrincipalAccrual(DateTime applicationDate)

        {
            //    using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                bool result = false;
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

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyPastDueInterestAccrualToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = applicationDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;

                //foreach (var item in model)
                //{
                //    item.date = applicationDate;

                //    var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();

                //    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                //    count++;

                //    addStaging.AMOUNT = (decimal)item.dailyAccuralAmount;
                //    addStaging.FLOWTYPE = "fff";
                //    addStaging.FORCEDEBITACCOUNT = "Y";
                //    addStaging.VALUEDATENUMBER = 1;
                //    addStaging.BATCHID = batchCode;
                //    addStaging.BATCHREFID = count;
                //    addStaging.SID = count;
                //    addStaging.COMPANYID = item.companyId;
                //    addStaging.CREDITACCOUNT = finacle.GetGlAccountCode(product.INTERESTINCOMEEXPENSEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                //    addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                //    addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.date, item.currencyId, item.companyId).sellingRate;
                //    addStaging.DEBITACCOUNT = finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId);// context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                //    addStaging.DESCRIPTION = "Past Due Daily Interest Accrual Posting";
                //    addStaging.DESTINATIONBRANCHID = item.branchId;
                //    addStaging.ISPOSTED = false;
                //    addStaging.OPERATIONID = (int)OperationsEnum.DailyInterestAccural;
                //    addStaging.POSTEDBY = "SYSTEM";
                //    addStaging.POSTEDDATE = item.date;
                //    addStaging.SOURCEBRANCHID = item.branchId;
                //    addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                //    addStaging.VALUEDATE = item.date;
                //    addStaging.TRANSACTIONTYPE = "BP";
                //    addStaging.BANKID = "01";
                //    this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                //    context.SaveChanges();
                //    if (setup.USE_THIRD_PARTY_INTEGRATION == false)
                //    {
                //        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                //    }
                //    //financeTransaction.PostDailyPastDuePrincipalAccrual(item);
                //}
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //    result = WriteBulkPostingToStaging(applicationDate, "BP", batchCode);

                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanClassificationViewModel> CalculateLoanClassification(DateTime applicationDate)

        {
            //    using (var trans = context.Database.BeginTransaction())
            //{
            try
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
                        TBL_LOAN loanResult = (from p in context.TBL_LOAN
                                               where p.TERMLOANID == item.loanId
                                               select p).SingleOrDefault();

                        loanResult.NPLDATE = systemDate;

                        // context.SaveChanges();
                    }
                    else if (pastDueDate != null && item.amount > 0)
                    {
                        TBL_LOAN loanResult = (from p in context.TBL_LOAN
                                               where p.TERMLOANID == item.loanId
                                               select p).SingleOrDefault();

                        loanResult.NPLDATE = null;

                        // context.SaveChanges();
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
                        TBL_LOAN loanResult = (from p in context.TBL_LOAN
                                               where p.TERMLOANID == item.loanId
                                               select p).SingleOrDefault();

                        loanResult.EXT_PRUDENT_GUIDELINE_STATUSID = prudentialStatus;

                        //context.SaveChanges();
                    }

                }
                var result = context.SaveChanges() > 0;
                if (result)
                {
                    // trans.Commit();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanClassificationViewModel> CalculateOverdraftClassification(DateTime applicationDate)

        {
            //    using (var trans = context.Database.BeginTransaction())
            // {
            try
            {
                var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);
                var systemDate = generalSetup.GetApplicationDate();

                var data = (from a in context.TBL_LOAN_REVOLVING
                            join b in context.TBL_LOAN_PAST_DUE on a.REVOLVINGLOANID equals b.LOANID
                            join c in context.TBL_DAY_COUNT_CONVENTION on a.DAYCOUNTCONVENTIONID equals c.DAYCOUNTCONVENTIONID
                            join d in context.TBL_SETUP_COMPANY on a.COMPANYID equals d.COMPANYID
                            where b.DATE <= DbFunctions.TruncateTime(applicationDate) && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                           && (b.CREDITAMOUNT - b.DEBITAMOUNT) <= 0
                            group b by new { b.LOANID, b.PARENT_PASTDUECODE } into groupedQ
                            select new LoanClassificationViewModel()
                            {
                                loanId = groupedQ.Key.LOANID,
                                refNo = groupedQ.Key.PARENT_PASTDUECODE,
                                amount = groupedQ.Sum(i => i.CREDITAMOUNT - i.DEBITAMOUNT),
                            }).ToList();

                foreach (var item in data)
                {

                    var pastDueDate = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == item.loanId).NPLDATE;

                    if (pastDueDate == null && item.amount < 0)
                    {
                        TBL_LOAN_REVOLVING loanResult = (from p in context.TBL_LOAN_REVOLVING
                                                         where p.REVOLVINGLOANID == item.loanId
                                                         select p).SingleOrDefault();

                        loanResult.NPLDATE = systemDate;

                        //context.SaveChanges();
                    }
                    else if (pastDueDate != null && item.amount > 0)
                    {
                        TBL_LOAN_REVOLVING loanResult = (from p in context.TBL_LOAN_REVOLVING
                                                         where p.REVOLVINGLOANID == item.loanId
                                                         select p).SingleOrDefault();

                        loanResult.NPLDATE = null;

                        //context.SaveChanges();
                    }

                    else if (pastDueDate != null && item.amount < 0)
                    {
                        DateTime nplDate = (DateTime)context.TBL_LOAN_REVOLVING.FirstOrDefault(a => a.REVOLVINGLOANID == item.loanId).NPLDATE;
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
                        TBL_LOAN_REVOLVING loanResult = (from p in context.TBL_LOAN_REVOLVING
                                                         where p.REVOLVINGLOANID == item.loanId
                                                         select p).SingleOrDefault();

                        loanResult.EXT_PRUDENT_GUIDELINE_STATUSID = prudentialStatus;

                        //context.SaveChanges();
                    }

                }
                var result = context.SaveChanges() > 0;
                if (result)
                {
                    // trans.Commit();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<CleanUpViewModel> OverdraftCleanUp(DateTime applicationDate)

        {
            //    using (var trans = context.Database.BeginTransaction())
            //{
            try
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
                        lien.description = "lien placed due to Account did not swing to positive";

                        casaLien.PlaceLien(lien);
                    }
                    else
                    {
                        int freqValue = context.TBL_FREQUENCY_TYPE.FirstOrDefault(a => a.FREQUENCYTYPEID == item.freqValue).NUMBEROFDAYS;

                        TBL_LOAN_COVENANT_DETAIL covenantResult = (from p in context.TBL_LOAN_COVENANT_DETAIL
                                                                   where p.LOANID == item.loanId
                                                                   select p).SingleOrDefault();

                        covenantResult.NEXTCOVENANTDATE = covenantResult.NEXTCOVENANTDATE.Value.Date.AddDays(freqValue);
                        //context.SaveChanges();
                    }

                }
                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public decimal DailyAccruedInterest(DateTime startDate, DateTime endDate, decimal Amount)
        {
            decimal dailyAmount = (Amount / ((int)(endDate - startDate).TotalDays));
            return dailyAmount;
        }

        public bool GetRepaymentFromStaging()
        {
            var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
            bool output = false;
            decimal fullAmount = 0;
            decimal partailAmount = 0;
            byte transType = 0;
            short lienType = 0;
            TBL_CUSTOM_TRANSACTION_BULK result = new TBL_CUSTOM_TRANSACTION_BULK();
            bool results = false;
            //var refNo = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in stagingContext.FINTRAK_TRAN_PROC_DETAILS
                        where a.AMT_COLLECTED <= a.AMT && a.FINTRAK_FLG != "Y"  //|| a.PSTD_FLG == "P"
                        //where a.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && a.BATCHID == batchCode
                        select new FinanceTransactionStagingViewModel()
                        {
                            batchId = a.BATCH_ID,
                            batchRefId = (int)a.BATCH_REF_ID,
                            transType = a.TRAN_TYPE,
                            flowType = a.FLOW_TYPE,
                            amount = (decimal)a.AMT,
                            debitGlAccount = a.DR_ACCT,
                            creditGlAccount = a.CR_ACCT,
                            //currencyCode = a.REF_CRNCY_CODE,
                            currencyRate = (double)a.RATE,
                            currencyRateCode = a.RATE_CODE,
                            description = a.NARRATION,
                            amountCollected = (decimal)a.AMT_COLLECTED,
                            bankId = a.BANK_ID,
                            sourceReferenceNumber = a.LOAN_ACCT,

                        }).ToList();
            foreach (var item in data)
            {
                result = (from p in context.TBL_CUSTOM_TRANSACTION_BULK
                          where p.BATCHID == item.batchId && p.BATCHREFID
                                                  == item.batchRefId
                          select p).SingleOrDefault();
                if (item.amountCollected != 0)
                {
                    fullAmount = item.amountCollected - result.AMOUNT;
                    partailAmount = result.AMOUNT - item.amountCollected;
                }
                FinanceTransactionStagingViewModel model = new FinanceTransactionStagingViewModel();
                if (result != null && item.amountCollected != 0 && fullAmount == 0)
                {
                    model.actualAmount = item.amountCollected - result.AMOUNTCOLLECTED;
                    model.operationId = result.OPERATIONID;
                    model.description = result.DESCRIPTION;
                    model.valueDate = result.VALUEDATE;
                    model.transactionDate = result.VALUEDATE;
                    model.currencyId = result.CURRENCYID;
                    model.currencyRate = result.CURRENCYRATE;
                    model.companyId = result.COMPANYID;
                    model.debitGlAccountId = result.DEBITGLACCOUNTID;
                    model.sourceReferenceNumber = result.SOURCEREFERENCENUMBER;
                    model.debitCasaAccountId = result.DEBITCASAACCOUNTID;
                    model.sourceBranchId = (short)result.SOURCEBRANCHID;
                    model.destinationBranchId = (short)result.DESTINATIONBRANCHID;
                    model.creditGlAccountId = result.CREDITGLACCOUNTID;
                    model.creditCasaAccountId = result.CREDITCASAACCOUNTID;
                    model.batchId = result.BATCHID;
                    model.batchRefId = result.BATCHREFID;
                    model.loanId = result.LOANID;

                    results = financeTransaction.BulkIntegrationPosting(model);
                    result.AMOUNTCOLLECTED = item.amountCollected;
                    context.SaveChanges();
                    if (model.operationId == (int)OperationsEnum.CommercialPaperRollOver)
                    {
                        var loan = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == model.sourceReferenceNumber);
                        var instruction = context.TBL_LOAN_MATURITY_INSTRUCTION.FirstOrDefault(x => x.LOANID == loan.TERMLOANID);

                        ArchiveLoan(loan.TERMLOANID, model.operationId, archiveBatchCode);//change to method that will disburs new loan

                    }
                    if (results == true)
                    {
                        if (result.AMOUNT == item.amountCollected)
                        {
                            result.ISPOSTED = true;

                            FINTRAK_TRAN_PROC_DETAILS bulk = (from a in stagingContext.FINTRAK_TRAN_PROC_DETAILS
                                                              where a.BATCH_ID == model.batchId && a.BATCH_REF_ID
                                                                == model.batchRefId
                                                              select a).SingleOrDefault();
                            bulk.FINTRAK_FLG = "Y";
                        }

                    }
                    output = stagingContext.SaveChanges() > 0;
                }
                else if (result != null && item.amountCollected != 0 && fullAmount != 0)
                {
                    model.actualAmount = item.amountCollected - result.AMOUNTCOLLECTED;
                    model.operationId = result.OPERATIONID;
                    model.description = result.DESCRIPTION;
                    model.valueDate = result.VALUEDATE;
                    model.transactionDate = result.VALUEDATE;
                    model.currencyId = result.CURRENCYID;
                    model.currencyRate = result.CURRENCYRATE;
                    model.companyId = result.COMPANYID;
                    model.debitGlAccountId = result.DEBITGLACCOUNTID;
                    model.sourceReferenceNumber = result.SOURCEREFERENCENUMBER;
                    model.debitCasaAccountId = result.DEBITCASAACCOUNTID;
                    model.sourceBranchId = (short)result.SOURCEBRANCHID;
                    model.destinationBranchId = (short)result.DESTINATIONBRANCHID;
                    model.creditGlAccountId = result.CREDITGLACCOUNTID;
                    model.creditCasaAccountId = result.CREDITCASAACCOUNTID;
                    model.batchId = result.BATCHID;
                    model.batchRefId = result.BATCHREFID;
                    model.loanId = result.LOANID;

                    results = financeTransaction.BulkIntegrationPosting(model);

                    if (result.TRANSACTIONTYPE == "BIF")
                    {
                        transType = (byte)LoanTransactionTypeEnum.Interest;
                        lienType = (short)LienTypeEnum.InterestRepayment;
                    }
                    else if (result.TRANSACTIONTYPE == "BPP")
                    {
                        transType = (byte)LoanTransactionTypeEnum.Principal;
                        lienType = (short)LienTypeEnum.PrincipalRepayment;
                    }
                    List<TBL_LOAN_PAST_DUE> transPastDue = new List<TBL_LOAN_PAST_DUE>();
                    TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();
                    var PastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                    var loan = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == item.loanId);
                    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == item.companyId);

                    pastDue.LOANID = (int)item.loanId;
                    pastDue.PARENT_PASTDUECODE = PastDueCode;
                    pastDue.CREDITAMOUNT = 0;
                    pastDue.DESCRIPTION = "Past Due Entries on " + result.DESCRIPTION + "as a result of Account not funded";
                    pastDue.DEBITAMOUNT = Math.Abs(partailAmount);
                    pastDue.DATE = item.transactionDate;
                    pastDue.TRANSACTIONTYPEID = transType;
                    pastDue.PARENT_PASTDUECODE = loan.LOANREFERENCENUMBER;
                    pastDue.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDue);
                    //context.SaveChanges();
                    updateloanTablePastDuePrincipal(pastDue.LOANID, pastDue.DEBITAMOUNT);


                    CasaLienViewModel lien = new CasaLienViewModel();

                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDue.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDue.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = lienType;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Anniversary Date";

                    casaLien.PlaceLien(lien);

                    result.AMOUNTCOLLECTED = item.amountCollected;
                    context.SaveChanges();
                }
                else if (result != null && item.amountCollected == 0)
                {
                    //model.actualAmount = item.amountCollected - result.AMOUNTCOLLECTED;
                    //model.operationId = result.OPERATIONID;
                    //model.description = result.DESCRIPTION;
                    //model.valueDate = result.VALUEDATE;
                    //model.transactionDate = result.VALUEDATE;
                    //model.currencyId = result.CURRENCYID;
                    //model.currencyRate = result.CURRENCYRATE;
                    //model.companyId = result.COMPANYID;
                    //model.debitGlAccountId = result.DEBITGLACCOUNTID;
                    //model.sourceReferenceNumber = result.SOURCEREFERENCENUMBER;
                    //model.debitCasaAccountId = result.DEBITCASAACCOUNTID;
                    //model.sourceBranchId = (short)result.SOURCEBRANCHID;
                    //model.destinationBranchId = (short)result.DESTINATIONBRANCHID;
                    //model.creditGlAccountId = result.CREDITGLACCOUNTID;
                    //model.creditCasaAccountId = result.CREDITCASAACCOUNTID;
                    //model.batchId = result.BATCHID;
                    //model.batchRefId = result.BATCHREFID;
                    //model.loanId = result.LOANID;
                    //result.AMOUNTCOLLECTED = item.amountCollected;

                    if (result.TRANSACTIONTYPE == "BIF")
                    {
                        transType = (byte)LoanTransactionTypeEnum.Interest;
                        lienType = (short)LienTypeEnum.InterestRepayment;
                    }
                    else if (result.TRANSACTIONTYPE == "BPP")
                    {
                        transType = (byte)LoanTransactionTypeEnum.Principal;
                        lienType = (short)LienTypeEnum.PrincipalRepayment;
                    }
                    List<TBL_LOAN_PAST_DUE> transPastDue = new List<TBL_LOAN_PAST_DUE>();
                    TBL_LOAN_PAST_DUE pastDue = new TBL_LOAN_PAST_DUE();
                    var PastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                    var loan = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == item.loanId);
                    var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == item.companyId);

                    pastDue.LOANID = (int)item.loanId;
                    pastDue.PARENT_PASTDUECODE = PastDueCode;
                    pastDue.CREDITAMOUNT = 0;
                    pastDue.DESCRIPTION = "Past Due Entries on " + result.DESCRIPTION + "as a result of Account not funded";
                    pastDue.DEBITAMOUNT = Math.Abs(partailAmount);
                    pastDue.DATE = item.transactionDate;
                    pastDue.TRANSACTIONTYPEID = transType;
                    pastDue.PARENT_PASTDUECODE = loan.LOANREFERENCENUMBER;
                    pastDue.PRODUCTTYPEID = product.PRODUCTTYPEID;

                    transPastDue.Add(pastDue);
                    updateloanTablePastDuePrincipal(pastDue.LOANID, pastDue.DEBITAMOUNT);

                    CasaLienViewModel lien = new CasaLienViewModel();
                    lien.productAccountNumber = casa.PRODUCTACCOUNTNUMBER;
                    lien.sourceReferenceNumber = pastDue.PARENT_PASTDUECODE;
                    lien.lienAmount = pastDue.DEBITAMOUNT;
                    lien.branchId = item.branchId;
                    lien.companyId = item.companyId;
                    lien.lienTypeId = lienType;
                    lien.createdBy = (int)SystemStaff.System;
                    lien.description = "lien placed due to Account not funded at Anniversary Date";

                    casaLien.PlaceLien(lien);
                    result.AMOUNTCOLLECTED = item.amountCollected;
                    context.SaveChanges();
                }



            }

            return output;
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

            //using (var trans = context.Database.BeginTransaction())
            //{



            try
            {
                bool result = false;
                var model = (from b in context.TBL_LOAN
                             where b.MATURITYDATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                             && b.ALLOWFORCEDEBITREPAYMENT == true && b.ISDISBURSED == true
                             select new LoanRepaymentViewModel()
                             {
                                 productId = b.PRODUCTID,
                                 branchId = b.BRANCHID,
                                 companyId = b.COMPANYID,
                                 currencyId = b.CURRENCYID,
                                 exchangeRate = b.EXCHANGERATE,
                                 periodInterestAmount = b.OUTSTANDINGINTEREST,
                                 periodPrincipalAmount = b.OUTSTANDINGPRINCIPAL,
                                 interestRate = b.INTERESTRATE,
                                 paymentDate = applicationDate,
                                 loanId = b.TERMLOANID,
                                 totalAmount = 0,
                                 casaAccountId = b.CASAACCOUNTID,
                                 loanRefNo = b.LOANREFERENCENUMBER,
                                 //maturityInstructionTypeId= a.INSTRUCTIONTYPEID


                             }).ToList();

                List<TBL_LOAN_FORCE_DEBIT> transForceDebit = new List<TBL_LOAN_FORCE_DEBIT>();
                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    //foreach (var item in model)
                    //{
                    //    if (item.maturityInstructionTypeId == (int)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal)
                    //    {
                    //        item.totalAmount = item.periodInterestAmount + item.periodPrincipalAmount;
                    //    }
                    //    else
                    //    {
                    //        item.totalAmount = item.periodPrincipalAmount;
                    //    }
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkLoanRepaymentPostingForceDebitToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);
                    //}

                }
                else
                    foreach (var item in model)
                    {
                        if (item.maturityInstructionTypeId == (int)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal)
                        {
                            item.totalAmount = item.periodInterestAmount + item.periodPrincipalAmount;
                        }
                        else
                        {
                            item.totalAmount = item.periodPrincipalAmount;
                        }
                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                        var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                        var casabalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).AVAILABLEBALANCE;
                        if (casabalance >= item.totalAmount)
                        {
                            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

                            financeTransaction.PostTransaction(inputTransactions);

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

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "partial principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

                            financeTransaction.PostTransaction(inputTransactions);

                            //updateloanTable(item);
                        }
                    }

                this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);
                result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanDisbursmentRollOver(DateTime applicationDate)
        {
            try
            {
                bool result = false;
                var model = (from a in context.TBL_LOAN_MATURITY_INSTRUCTION
                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                             where b.MATURITYDATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                             && b.ALLOWFORCEDEBITREPAYMENT == true && a.ISUSED == false && b.ISDISBURSED == true
                             select new LoanRepaymentViewModel()
                             {
                                 productId = b.PRODUCTID,
                                 branchId = b.BRANCHID,
                                 companyId = b.COMPANYID,
                                 currencyId = b.CURRENCYID,
                                 exchangeRate = b.EXCHANGERATE,
                                 periodInterestAmount = b.OUTSTANDINGINTEREST,
                                 periodPrincipalAmount = b.OUTSTANDINGPRINCIPAL,
                                 interestRate = b.INTERESTRATE,
                                 paymentDate = applicationDate,
                                 loanId = b.TERMLOANID,
                                 totalAmount = 0,
                                 casaAccountId = b.CASAACCOUNTID,
                                 loanRefNo = b.LOANREFERENCENUMBER,
                                 maturityInstructionTypeId = a.INSTRUCTIONTYPEID


                             }).ToList();

                List<TBL_LOAN_FORCE_DEBIT> transForceDebit = new List<TBL_LOAN_FORCE_DEBIT>();
                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {

                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkProcessLoanDisbursmentRollOverToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);
                    //}

                }
                else
                    foreach (var item in model)
                    {
                        if (item.maturityInstructionTypeId == (int)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal)
                        {
                            item.totalAmount = item.periodInterestAmount + item.periodPrincipalAmount;
                        }
                        else
                        {
                            item.totalAmount = item.periodPrincipalAmount;
                        }
                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);

                        var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                        var casabalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).AVAILABLEBALANCE;
                        if (casabalance >= item.totalAmount)
                        {
                            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

                            financeTransaction.PostTransaction(inputTransactions);

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

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "partial principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

                            financeTransaction.PostTransaction(inputTransactions);

                            //updateloanTable(item);
                        }
                    }

                this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);
                result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue(DateTime applicationDate)

        {

            //using (var trans = context.Database.BeginTransaction())
            //{

            try

            {
                bool result = false;
                var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                             where a.PAYMENTDATE == DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active
                             && b.ALLOWFORCEDEBITREPAYMENT == false && a.PERIODPRINCIPALAMOUNT != 0 && a.PERIODINTERESTAMOUNT != 0 && b.ISDISBURSED == true
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


                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    foreach (var item in model)
                    {
                        BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                        result = bulkPosting.WriteBulkLoanRepaymentPostingPastDueToStaging(model, context, stagingContext, finacle, financeTransaction, applicationDate);
                    }

                }
                else
                {

                    foreach (var item in model)
                    {
                        var PastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                        var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId && x.COMPANYID == item.companyId);
                        var casabalance = casa.AVAILABLEBALANCE;
                        decimal principalAmountNotCollected = 0;
                        //decimal balanceAfterInterestAmountCollection = 0;
                        decimal partialPrincipalAmountCollected = 0;
                        decimal partialInterestAmountCollected = 0;
                        decimal interestAmountNotCollected = 0;

                        if (casabalance >= item.totalAmount)
                        {

                            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

                            updateloanTablePrincipal(item.loanId, item.periodPrincipalAmount);
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
                            //context.SaveChanges();
                            updateloanTablePastDuePrincipal(pastDue.LOANID, pastDue.DEBITAMOUNT);

                            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));
                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, partialPrincipalAmountCollected, product.PRINCIPALBALANCEGL.Value, "partial principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                            //context.SaveChanges();

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

                            //context.SaveChanges();

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

                            //context.SaveChanges();

                            updateloanTablePastDuePrincipal(pastDuePrincipal.LOANID, pastDuePrincipal.DEBITAMOUNT);

                            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                            inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, partialInterestAmountCollected, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment", (int)OperationsEnum.InterestLoanRepayment));

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
                            //context.SaveChanges();

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
                            //context.SaveChanges();
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
                            //context.SaveChanges();
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
                            //context.SaveChanges();
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
                            //context.SaveChanges();
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
                            //context.SaveChanges();

                        }
                    }
                }

                this.context.TBL_LOAN_PAST_DUE.AddRange(transPastDue);

                //result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                    inputTransactions.Add(financeTransaction.PostBuildAuthorisedOverdraftRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                    financeTransaction.PostTransaction(inputTransactions);
                    // }
                }

                this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessUnauthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                    inputTransactions.Add(financeTransaction.PostBuildAuthorisedOverdraftRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                    financeTransaction.PostTransaction(inputTransactions);
                    //}
                }

                this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    // trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate)

        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }

        }

        public IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(DateTime applicationDate)

        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    // trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }


        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebitForInterestReview(DateTime applicationDate, int loanId)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "partial principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

                        financeTransaction.PostTransaction(inputTransactions);

                        //updateloanTable(item);
                    }
                }

                this.context.TBL_LOAN_FORCE_DEBIT.AddRange(transForceDebit);

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview(DateTime applicationDate, int loanId)

        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));
                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DEBITAMOUNT, product.PRINCIPALBALANCEGL.Value, "partial principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));
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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DEBITAMOUNT, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment", (int)OperationsEnum.InterestLoanRepayment));
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

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate)

        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));

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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment", (int)OperationsEnum.InterestLoanRepayment));
                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DEBITAMOUNT, product.PRINCIPALBALANCEGL.Value, "partial principal repayment", (int)OperationsEnum.PrincipalLoanRepayment));
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

                        inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DEBITAMOUNT, product.INTERESTRECEIVABLEPAYABLEGL.Value, "partial interest repayment", (int)OperationsEnum.InterestLoanRepayment));
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

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return null;

            }
        }

        #endregion

        #region Periodic  Operation

        public IEnumerable<LoanViewModel> ProcessIntervalFeeandCommissionPosting(DateTime applicationDate)
        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                bool result = false;
                var model1 = (from a in context.TBL_LOAN_FEE
                              join c in context.TBL_LOAN_FEE_SCHEDULE on a.LOANCHARGEFEEID equals c.LOANCHARGEFEEID
                              join d in context.TBL_LOAN on a.LOANID equals d.TERMLOANID
                              join e in context.TBL_CASA on d.CASAACCOUNTID equals e.CASAACCOUNTID
                              where c.FEEDATE == DbFunctions.TruncateTime(applicationDate) && a.ISRECURRING == true
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
                                  chargeFeeId = a.CHARGEFEEID,



                              }).ToList();

                var model2 = (from a in context.TBL_LOAN_FEE
                              join c in context.TBL_LOAN_FEE_SCHEDULE on a.LOANCHARGEFEEID equals c.LOANCHARGEFEEID
                              join d in context.TBL_LOAN_REVOLVING on a.LOANID equals d.REVOLVINGLOANID
                              join e in context.TBL_CASA on d.CASAACCOUNTID equals e.CASAACCOUNTID
                              where c.FEEDATE == DbFunctions.TruncateTime(applicationDate) && a.ISRECURRING == true
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
                                  chargeFeeId = a.CHARGEFEEID,


                              }).ToList();

                var model = model1.Union(model2).ToList();

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                //int count = 0;
                foreach (var item in model)
                {

                    //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId);
                    //item.paymentDate = applicationDate;
                    //var addStaging = new TBL_CUSTOM_TRANSACTION_BULK();
                    //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == item.productId);
                    //count++;

                    //addStaging.AMOUNT = (decimal)item.totalAmount;
                    //addStaging.FLOWTYPE = "fff";
                    //addStaging.FORCEDEBITACCOUNT = "Y";
                    //addStaging.VALUEDATENUMBER = 1;
                    //addStaging.BATCHID = batchCode;
                    //addStaging.BATCHREFID = count;
                    //addStaging.SID = count;
                    //addStaging.COMPANYID = item.companyId;
                    //addStaging.CREDITACCOUNT = context.TBL_CHART_OF_ACCOUNT.Where(x => x.GLACCOUNTID == product.INTERESTINCOMEEXPENSEGL.Value).FirstOrDefault().ACCOUNTCODE;// product.INTERESTINCOMEEXPENSEGL.Value;
                    //addStaging.CURRENCYCODE = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE;
                    //addStaging.CURRENCYRATE = financeTransaction.GetExchangeRate(item.paymentDate, (short)item.currencyId, item.companyId).sellingRate;
                    //addStaging.DEBITACCOUNT = casa.PRODUCTACCOUNTNUMBER;  //context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == product.INTERESTRECEIVABLEPAYABLEGL.Value).ACCOUNTCODE;
                    //addStaging.DESCRIPTION = "Fee charge on Posting";
                    //addStaging.DESTINATIONBRANCHID = item.branchId;
                    //addStaging.ISPOSTED = false;
                    //addStaging.OPERATIONID = (int)OperationsEnum.Fee_chargeChange;/// change to IntervalFeeandCommission posting
                    //addStaging.POSTEDBY = "SYSTEM";
                    //addStaging.POSTEDDATE = item.paymentDate;
                    //addStaging.SOURCEBRANCHID = item.branchId;
                    //addStaging.SOURCEREFERENCENUMBER = product.PRODUCTCODE;
                    //addStaging.VALUEDATE = item.paymentDate;
                    //addStaging.TRANSACTIONTYPE = "BP";
                    //addStaging.BANKID = "01";
                    //this.context.TBL_CUSTOM_TRANSACTION_BULK.Add(addStaging);
                    //context.SaveChanges();

                    result = financeTransaction.PostBuildLoanChargeFeesPosting(item);

                }
                if (result)
                {
                    // trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                return null;

            }
        }

        public IEnumerable<LimitSuspensionViewModel> ProcessNPLByBranchSuspension()
        {
            var model = (from a in context.TBL_LOAN
                         join b in context.TBL_BRANCH on a.BRANCHID equals b.BRANCHID
                         //join c in context.TBL_LIMIT_DETAIL on b.BRANCHID equals c.TARGETID
                         // join d in context.TBL_LIMIT on c.LIMITID equals d.LIMITID
                         //where b.BRANCHID == c.TARGETID && c.LIMITID == d.LIMITID
                         //&& d.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
                         //&& c.LIMITTYPEID == (int)LimitType.Branch
                         group a by new
                         {
                             a.BRANCHID,// c.LIMITID, c.MAXIMUMVALUE
                         } into groupedQ
                         select new LimitSuspensionViewModel()
                         {
                             //limitId = groupedQ.Key.LIMITID,
                             //limitAmount = groupedQ.Key.MAXIMUMVALUE,
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
                         //join c in context.TBL_LIMIT_DETAIL on b.STAFFID equals c.TARGETID
                         //join d in context.TBL_LIMIT on c.LIMITID equals d.LIMITID
                         //where b.STAFFID == c.TARGETID && c.LIMITID == d.LIMITID
                         //&& d.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
                         //&& c.LIMITTYPEID == (int)LimitType.RelationshipManager
                         group a by new
                         {
                             a.RELATIONSHIPMANAGERID, //c.LIMITID, c.MAXIMUMVALUE
                         } into groupedQ
                         select new LimitSuspensionViewModel()
                         {
                             //limitId = groupedQ.Key.LIMITID,
                             //limitAmount = groupedQ.Key.MAXIMUMVALUE,
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
        public bool ProcessChargeReversal(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, int operationId, int staffId)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
            try
            {
                bool output = false;
                var systemDate = generalSetup.GetApplicationDate();
                var chargeDetails = this.context.TBL_LOAN_REVIEW_OPERATION.FirstOrDefault(x => x.LOANID == loanId && x.OPERATIONTYPEID == operationId
                && x.OPERATIONCOMPLETED == false);
                int productType = chargeDetails.PRODUCTTYPEID;

                var tax = this.context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == chargeDetails.INTERESTFREQUENCYTYPEID);
                decimal NewTaxRate = (decimal)tax.FirstOrDefault().VALUE;

                int DateDiff = 0;
                decimal DailyAccruedFee = 0;
                decimal AccruedFeeToDate = 0;
                decimal EarnedFeeAmount = 0;
                decimal NewFeeAmount = 0;
                decimal NewTaxAmount = 0;
                decimal DailyAccruedTax = 0;
                decimal AccruedTaxToDate = 0;
                //decimal feeAmountDiff = 0;

                int chargeTypeId = 0;
                LoanPaymentRestructureScheduleInputViewModel model = new LoanPaymentRestructureScheduleInputViewModel();

                if (productType == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                {
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, operationId, archiveBatchCode);

                    model = (
                    from a in context.TBL_LOAN_REVIEW_OPERATION
                    join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                    join c in context.TBL_LOAN_FEE on a.LOANID equals c.LOANID
                    where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false

                    select new LoanPaymentRestructureScheduleInputViewModel()
                    {
                        loanId = b.TERMLOANID,
                        principalAmount = (double)b.PRINCIPALAMOUNT,
                        interestRate = b.INTERESTRATE,
                        effectiveDate = b.EFFECTIVEDATE,//(DateTime)b.LASTRESTRUCTUREDATE
                        operationId = a.OPERATIONTYPEID,
                        companyId = b.COMPANYID,
                        staffId = staffId,
                        createdBy = staffId,
                        customerId = b.CUSTOMERID,
                        payAmount = (double)a.PREPAYMENT,
                        feeRate = c.FEERATEVALUE,
                        earnedFeeAmount = c.EARNEDFEEAMOUNT,
                        feeAmount = c.FEEAMOUNT,
                        newInterest = (double)a.INTERATERATE,
                        chargeFeeId = (int)a.PRINCIPALFREQUENCYTYPEID,
                        chargeFeeTypeId = (int)a.INTERESTFREQUENCYTYPEID,


                    }).FirstOrDefault();
                    EarnedFeeAmount = model.earnedFeeAmount;
                    chargeTypeId = model.chargeFeeTypeId;
                    NewFeeAmount = (decimal)model.payAmount;
                    NewTaxAmount = NewFeeAmount * (NewTaxRate / 100);
                    DateDiff = (int)(systemDate - model.effectiveDate).TotalDays;
                    DailyAccruedFee = DailyAccruedInterest(model.effectiveDate, systemDate, NewFeeAmount);
                    AccruedFeeToDate = DateDiff * DailyAccruedFee;
                    DailyAccruedTax = DailyAccruedInterest(model.effectiveDate, systemDate, NewTaxAmount);
                    AccruedTaxToDate = DateDiff * DailyAccruedTax;
                    model.feeAmountDiff = EarnedFeeAmount - AccruedFeeToDate;
                }

                if (productType == (int)LoanSystemTypeEnum.OverdraftFacility)
                {
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveOverDraft(loanId, archiveBatchCode);
                    model = (
                   from a in context.TBL_LOAN_REVIEW_OPERATION
                   join b in context.TBL_LOAN_REVOLVING on a.LOANID equals b.REVOLVINGLOANID
                   join c in context.TBL_LOAN_FEE on a.LOANID equals c.LOANID
                   where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false

                   select new LoanPaymentRestructureScheduleInputViewModel()
                   {
                       loanId = b.REVOLVINGLOANID,
                       principalAmount = (double)b.OVERDRAFTLIMIT,
                       interestRate = b.INTERESTRATE,
                       effectiveDate = b.EFFECTIVEDATE,//change to (DateTime)b.LASTRESTRUCTUREDATE
                       operationId = a.OPERATIONTYPEID,
                       companyId = b.COMPANYID,
                       staffId = staffId,
                       createdBy = staffId,
                       customerId = b.CUSTOMERID,
                       payAmount = (double)a.PREPAYMENT,
                       feeRate = c.FEERATEVALUE,
                       earnedFeeAmount = c.EARNEDFEEAMOUNT,
                       feeAmount = c.FEEAMOUNT,
                       newInterest = (double)a.INTERATERATE,
                       chargeFeeId = (int)a.PRINCIPALFREQUENCYTYPEID,
                       chargeFeeTypeId = (int)a.INTERESTFREQUENCYTYPEID,

                   }).FirstOrDefault();
                    EarnedFeeAmount = model.earnedFeeAmount;
                    chargeTypeId = model.chargeFeeTypeId;
                    NewFeeAmount = (decimal)model.payAmount;
                    NewTaxAmount = NewFeeAmount * (NewTaxRate / 100);
                    DateDiff = (int)(systemDate - model.effectiveDate).TotalDays;
                    DailyAccruedFee = DailyAccruedInterest(model.effectiveDate, systemDate, NewFeeAmount);
                    AccruedFeeToDate = DateDiff * DailyAccruedFee;
                    DailyAccruedTax = DailyAccruedInterest(model.effectiveDate, systemDate, NewTaxAmount);
                    AccruedTaxToDate = DateDiff * DailyAccruedTax;
                    model.feeAmountDiff = EarnedFeeAmount - AccruedFeeToDate;
                }

                if (productType == (int)LoanSystemTypeEnum.ContingentLiability)
                {
                    model = (
                    from a in context.TBL_LOAN_REVIEW_OPERATION
                    join b in context.TBL_LOAN_CONTINGENT on a.LOANID equals b.CONTINGENTLOANID
                    join c in context.TBL_LOAN_FEE on a.LOANID equals c.LOANID
                    where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false

                    select new LoanPaymentRestructureScheduleInputViewModel()
                    {
                        loanId = b.CONTINGENTLOANID,
                        principalAmount = (double)b.CONTINGENTAMOUNT,
                        interestRate = 1,
                        effectiveDate = b.EFFECTIVEDATE,//change to (DateTime)b.LASTRESTRUCTUREDATE
                        operationId = a.OPERATIONTYPEID,
                        companyId = b.COMPANYID,
                        staffId = staffId,
                        createdBy = staffId,
                        customerId = b.CUSTOMERID,
                        payAmount = (double)a.PREPAYMENT,
                        feeRate = c.FEERATEVALUE,
                        earnedFeeAmount = c.EARNEDFEEAMOUNT,
                        feeAmount = c.FEEAMOUNT,
                        newInterest = (double)a.INTERATERATE,
                        chargeFeeId = (int)a.PRINCIPALFREQUENCYTYPEID,
                        chargeFeeTypeId = (int)a.INTERESTFREQUENCYTYPEID,

                    }).FirstOrDefault();
                    EarnedFeeAmount = model.earnedFeeAmount;
                    chargeTypeId = model.chargeFeeTypeId;
                    NewFeeAmount = (decimal)model.payAmount;
                    NewTaxAmount = NewFeeAmount * (NewTaxRate / 100);
                    DateDiff = (int)(systemDate - model.effectiveDate).TotalDays;
                    DailyAccruedFee = DailyAccruedInterest(model.effectiveDate, systemDate, NewFeeAmount);
                    AccruedFeeToDate = DateDiff * DailyAccruedFee;
                    DailyAccruedTax = DailyAccruedInterest(model.effectiveDate, systemDate, NewTaxAmount);
                    AccruedTaxToDate = DateDiff * DailyAccruedTax;
                    model.feeAmountDiff = EarnedFeeAmount - AccruedFeeToDate;
                }

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                if (EarnedFeeAmount > AccruedFeeToDate)
                {
                    inputTransactions.Add(financeTransaction.BuildChargeReversalPosting(model, twoFactorAuth));
                    //inputTransactions.Add(financeTransaction.BuildChargeReversalPosting(feeInput));
                }
                if (EarnedFeeAmount < AccruedFeeToDate)
                {
                    //inputTransactions.Add(financeTransaction.BuildChargeReversalPosting(feeInput));
                    //inputTransactions.Add(financeTransaction.BuildChargeReversalPosting(feeInput));
                }


                TBL_LOAN_FEE feeResult = (from p in context.TBL_LOAN_FEE
                                          where p.LOANID == loanId
                                                && p.LOANCHARGEFEEID == chargeTypeId
                                          select p).SingleOrDefault();

                feeResult.FEEAMOUNT = NewFeeAmount;
                feeResult.EARNEDFEEAMOUNT = AccruedFeeToDate;
                feeResult.TAXAMOUNT = NewTaxAmount;
                feeResult.EARNEDTAXAMOUNT = AccruedTaxToDate;
                //output = true;
                var result = context.SaveChanges() > 0;
                //return output;
                if (result)
                {
                    //trans.Commit();
                    output = true; ;
                }
                return false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                return false;

            }

        }

        public bool AddChargeReversal(LoanChargeFeeViewModel model, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var productType = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId && x.COMPANYID == model.companyId).PRODUCTTYPEID;

            TBL_LOAN_FEE loanFee = new TBL_LOAN_FEE();

            loanFee.LOANID = model.loanId;
            // loanFee.PRODUCTTYPEID = productType;
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

        public bool OverdraftTopUp(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, decimal amount)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            bool output = false;
            ResponseMessageViewModel topResult = new ResponseMessageViewModel();
            try
            {

                var systemDate = generalSetup.GetApplicationDate();
                var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                DeleteLoanExist(loanId, systemDate);
                ArchiveOverDraft(loanId, archiveBatchCode);
                var model = (from a in context.TBL_LOAN_REVOLVING
                             join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                             where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                             select new RevolvingLoanViewModel()
                             {
                                 loanId = a.REVOLVINGLOANID,
                                 loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                 customerId = a.CUSTOMERID,
                                 productId = a.PRODUCTID,
                                 companyId = a.COMPANYID,
                                 casaAccountId = a.CASAACCOUNTID,
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
                                 revolvingTypeId = a.REVOLVINGTYPEID,
                                 productAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                 serialNumber = a.SERIALNUMBER,

                             }).FirstOrDefault();

                List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



                //foreach (var model in model)
                //{
                model.productTypeId = (int)LoanSystemTypeEnum.OverdraftFacility;
                var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(model.branchId, model.productId, model.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = model.customerId;
                addOverDraft.LOANSYSTEMTYPEID = model.loanSystemTypeId;
                addOverDraft.PRODUCTID = model.productId;
                addOverDraft.COMPANYID = model.companyId;
                addOverDraft.CASAACCOUNTID = model.casaAccountId;
                addOverDraft.BRANCHID = model.branchId;
                addOverDraft.CURRENCYID = model.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = model.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = model.loanReferenceNumber;
                addOverDraft.SUBSECTORID = model.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = model.relationshipManagerId;
                addOverDraft.MISCODE = model.misCode;
                addOverDraft.TEAMMISCODE = model.teamMiscode;
                addOverDraft.INTERESTRATE = model.interestRate;
                addOverDraft.EFFECTIVEDATE = model.effectiveDate;
                addOverDraft.MATURITYDATE = model.maturityDate;
                addOverDraft.BOOKINGDATE = model.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = model.overdraftLimit;
                addOverDraft.APPROVALSTATUSID = model.approvalStatusId;
                addOverDraft.APPROVEDBY = model.approvedBy;
                addOverDraft.APPROVERCOMMENT = model.approverComment;
                addOverDraft.DATEAPPROVED = model.dateApproved;
                addOverDraft.LOANSTATUSID = model.loanStatusId;
                addOverDraft.ISDISBURSED = model.isDisbursed;
                addOverDraft.DISBURSEDBY = model.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = model.disburserComment;
                addOverDraft.DISBURSEDATE = model.disburseDate;
                addOverDraft.OPERATIONID = model.operationId;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = model.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = model.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)model.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //model.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //model.externalPrudentialGuidelineStatusId;
                addOverDraft.USER_PRUDENTIAL_GUIDE_STATUSID = 1;
                addOverDraft.NPLDATE = model.nplDate;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.REVOLVINGTYPEID = model.revolvingTypeId;


                overDraft.Add(addOverDraft);
                // }

                this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

                if (USE_THIRD_PARTY_INTEGRATION)
                {

                    var data1 = context.TBL_LOAN_REVOLVING.Find(loanId);

                    if (data1.LOANSTATUSID != (int)LoanStatusEnum.Inactive)
                    {
                        if (data1.MATURITYDATE.Date < systemDate.Date)
                        {
                            throw new SecureException("The tenor for the top-up amount is not expected to exceed the expiry date of the current limit");
                        }
                        var loan = model;
                        var reviewDate = data1.BOOKINGDATE.AddMonths(1);
                        var data = new OverDraftTopUpAndRenewViewModel
                        {
                            sanctionLimit = String.Format("{0:0.00}", loan.overdraftLimit),
                            sanctionReferenceNumber = loan.serialNumber,
                            accountNumber = loan.productAccountNumber,
                            expiryDate = loan.maturityDate.ToString("dd-MMM-yyyy", null),
                            reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                            createdDate = systemDate,
                        };
                        topResult = finacle.OverDraftTopUp(data, twoFactorAuth);
                        //return true;

                    }
                    else
                    {
                        throw new SecureException("Limit has experied or is inactive");
                    }
                }
                addOverDraft.SERIALNUMBER = topResult.serialNumber;
                var result = context.SaveChanges() > 0;
                if (topResult != null && result == true)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool OverdraftRenewal(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, decimal amount)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            bool output = false;
            ResponseMessageViewModel renewalResult = new ResponseMessageViewModel();
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                DeleteLoanExist(loanId, systemDate);
                ArchiveOverDraft(loanId, archiveBatchCode);
                var model = (from a in context.TBL_LOAN_REVOLVING
                             join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                             where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                             select new RevolvingLoanViewModel()
                             {
                                 loanId = a.REVOLVINGLOANID,
                                 loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                 customerId = a.CUSTOMERID,
                                 productId = a.PRODUCTID,
                                 companyId = a.COMPANYID,
                                 casaAccountId = a.CASAACCOUNTID,
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
                                 revolvingTypeId = a.REVOLVINGTYPEID,
                                 productAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                 serialNumber = a.SERIALNUMBER,

                             }).FirstOrDefault();

                List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



                //foreach (var model in model)
                //{
                model.productTypeId = (int)LoanSystemTypeEnum.OverdraftFacility;
                var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(model.branchId, model.productId, model.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = model.customerId;
                addOverDraft.LOANSYSTEMTYPEID = model.loanSystemTypeId;
                addOverDraft.PRODUCTID = model.productId;
                addOverDraft.COMPANYID = model.companyId;
                addOverDraft.CASAACCOUNTID = model.casaAccountId;
                addOverDraft.BRANCHID = model.branchId;
                addOverDraft.CURRENCYID = model.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = model.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = model.loanReferenceNumber;
                addOverDraft.SUBSECTORID = model.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = model.relationshipManagerId;
                addOverDraft.MISCODE = model.misCode;
                addOverDraft.TEAMMISCODE = model.teamMiscode;
                addOverDraft.INTERESTRATE = model.interestRate;
                addOverDraft.EFFECTIVEDATE = model.effectiveDate;
                addOverDraft.MATURITYDATE = model.maturityDate;
                addOverDraft.BOOKINGDATE = model.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = model.overdraftLimit;
                addOverDraft.APPROVALSTATUSID = model.approvalStatusId;
                addOverDraft.APPROVEDBY = model.approvedBy;
                addOverDraft.APPROVERCOMMENT = model.approverComment;
                addOverDraft.DATEAPPROVED = model.dateApproved;
                addOverDraft.LOANSTATUSID = model.loanStatusId;
                addOverDraft.ISDISBURSED = model.isDisbursed;
                addOverDraft.DISBURSEDBY = model.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = model.disburserComment;
                addOverDraft.DISBURSEDATE = model.disburseDate;
                addOverDraft.OPERATIONID = model.operationId;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = model.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = model.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)model.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //model.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //model.externalPrudentialGuidelineStatusId;
                addOverDraft.USER_PRUDENTIAL_GUIDE_STATUSID = 1;
                addOverDraft.NPLDATE = model.nplDate;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.REVOLVINGTYPEID = model.revolvingTypeId;

                overDraft.Add(addOverDraft);
                // }

                this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);
                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    var loan = model;
                    var reviewDate = loan.bookingDate.AddMonths(1);
                    var data = new OverDraftTopUpAndRenewViewModel
                    {
                        //sanctionLimit = loan.overdraftLimit.ToString(),
                        //sanctionReferenceNumber = loan.serialNumber,
                        //accountNumber = loan.productAccountNumber,
                        //expiryDate = loan.maturityDate.ToString(),
                        //reviewedDate = loan.effectiveDate.ToString()

                        sanctionLimit = String.Format("{0:0.00}", loan.overdraftLimit),
                        sanctionReferenceNumber = loan.serialNumber,
                        accountNumber = loan.productAccountNumber,
                        expiryDate = loan.maturityDate.ToString("dd-MMM-yyyy", null),
                        reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                        createdDate = systemDate,
                    };
                    renewalResult = finacle.OverDraftRenew(data, twoFactorAuth);
                }
                addOverDraft.SERIALNUMBER = renewalResult.serialNumber;
                var result = context.SaveChanges() > 0;

                if (renewalResult != null && result)
                {
                    //trans.Commit();
                    output = true;
                }
                // output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool OverdraftExtension(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, decimal amount)
        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            bool output = false;
            ResponseMessageViewModel extentionResult = new ResponseMessageViewModel();
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                DeleteLoanExist(loanId, systemDate);
                ArchiveOverDraft(loanId, archiveBatchCode);
                var model = (from a in context.TBL_LOAN_REVOLVING
                             join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                             where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                             select new RevolvingLoanViewModel()
                             {
                                 loanId = a.REVOLVINGLOANID,
                                 loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                 customerId = a.CUSTOMERID,
                                 productId = a.PRODUCTID,
                                 companyId = a.COMPANYID,
                                 casaAccountId = a.CASAACCOUNTID,
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
                                 revolvingTypeId = a.REVOLVINGTYPEID,
                                 productAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                 serialNumber = a.SERIALNUMBER,

                             }).FirstOrDefault();

                List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



                //foreach (var model in model)
                //{
                model.productTypeId = (int)LoanSystemTypeEnum.OverdraftFacility;
                var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(model.branchId, model.productId, model.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = model.customerId;
                addOverDraft.LOANSYSTEMTYPEID = model.loanSystemTypeId;
                addOverDraft.PRODUCTID = model.productId;
                addOverDraft.COMPANYID = model.companyId;
                addOverDraft.CASAACCOUNTID = model.casaAccountId;
                addOverDraft.BRANCHID = model.branchId;
                addOverDraft.CURRENCYID = model.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = model.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = model.loanReferenceNumber;
                addOverDraft.SUBSECTORID = model.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = model.relationshipManagerId;
                addOverDraft.MISCODE = model.misCode;
                addOverDraft.TEAMMISCODE = model.teamMiscode;
                addOverDraft.INTERESTRATE = model.interestRate;
                addOverDraft.EFFECTIVEDATE = model.effectiveDate;
                addOverDraft.MATURITYDATE = model.maturityDate;
                addOverDraft.BOOKINGDATE = model.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = model.overdraftLimit;
                addOverDraft.APPROVALSTATUSID = model.approvalStatusId;
                addOverDraft.APPROVEDBY = model.approvedBy;
                addOverDraft.APPROVERCOMMENT = model.approverComment;
                addOverDraft.DATEAPPROVED = model.dateApproved;
                addOverDraft.LOANSTATUSID = model.loanStatusId;
                addOverDraft.ISDISBURSED = model.isDisbursed;
                addOverDraft.DISBURSEDBY = model.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = model.disburserComment;
                addOverDraft.DISBURSEDATE = model.disburseDate;
                addOverDraft.OPERATIONID = model.operationId;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = model.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = model.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)model.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //model.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //model.externalPrudentialGuidelineStatusId;
                addOverDraft.USER_PRUDENTIAL_GUIDE_STATUSID = 1;
                addOverDraft.NPLDATE = model.nplDate;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.REVOLVINGTYPEID = model.revolvingTypeId;

                overDraft.Add(addOverDraft);
                // }

                this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    var loan = model;
                    var reviewDate = loan.bookingDate.AddMonths(1);
                    var data = new OverDraftExtendViewModel
                    {
                        //sanctionLimit = loan.overdraftLimit.ToString(),
                        //sanctionReferenceNumber = loan.serialNumber,
                        //accountNumber = loan.productAccountNumber,
                        //expiryDate = loan.maturityDate.ToString()

                        sanctionLimit = String.Format("{0:0.00}", loan.overdraftLimit),
                        sanctionReferenceNumber = loan.serialNumber,
                        accountNumber = loan.productAccountNumber,
                        expiryDate = loan.maturityDate.ToString("dd-MMM-yyyy", null),
                        reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                        createdDate = systemDate,
                    };
                    extentionResult = finacle.OverDraftExtend(data, twoFactorAuth);
                }

                addOverDraft.SERIALNUMBER = extentionResult.serialNumber;
                var result = context.SaveChanges() > 0;
                if (extentionResult != null && result)
                {
                    // trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }

            return output;
        }

        public bool ChangeOperativeAccount(int casaAccountId, int newCasaAccountId,int loanId)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            bool output = false;
            try
            {
                var data = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId && x.CASAACCOUNTID == casaAccountId).FirstOrDefault();
                if (data != null)
                {
                    TBL_LOAN loanResult = (from p in context.TBL_LOAN
                                           where p.CASAACCOUNTID == casaAccountId && p.TERMLOANID == loanId
                                            && p.LOANSTATUSID == (short)LoanStatusEnum.Active
                                           select p).SingleOrDefault();
                    var casa = this.context.TBL_CASA.Where(x => x.CASAACCOUNTID == newCasaAccountId && x.ACCOUNTSTATUSID == (short)CASAAccountStatusEnum.Active).FirstOrDefault().CASAACCOUNTID;

                    loanResult.CASAACCOUNTID = casa;
                }
                else
                {
                    TBL_LOAN_REVOLVING loanResult = (from p in context.TBL_LOAN_REVOLVING
                                                     where p.CASAACCOUNTID == casaAccountId && p.REVOLVINGLOANID == loanId
                                            && p.LOANSTATUSID == (short)LoanStatusEnum.Active
                                           select p).SingleOrDefault();
                    var casa = this.context.TBL_CASA.Where(x => x.CASAACCOUNTID == newCasaAccountId && x.ACCOUNTSTATUSID == (short)CASAAccountStatusEnum.Active).FirstOrDefault().CASAACCOUNTID;

                    loanResult.CASAACCOUNTID = casa;
                }
               
            
                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool SubAllocation(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, decimal amount, DateTime applicationDate, int staffId)
        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            bool output = false;
            ResponseMessageViewModel subAllocationExtentionResult = new ResponseMessageViewModel();
            ResponseMessageViewModel subAllocationResult = new ResponseMessageViewModel();
            var initialLimit = this.context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId).FirstOrDefault().OVERDRAFTLIMIT;

            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                DeleteLoanExist(loanId, systemDate);
                ArchiveOverDraft(loanId, archiveBatchCode);
                var model = (from a in context.TBL_LOAN_REVOLVING
                             join b in context.TBL_LOAN_REVIEW_OPERATION on a.REVOLVINGLOANID equals b.LOANID
                             join c in context.TBL_CASA on b.CASA_ACCOUNTID equals c.CASAACCOUNTID
                             where a.REVOLVINGLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                             select new RevolvingLoanViewModel()
                             {
                                 loanId = a.REVOLVINGLOANID,
                                 loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                 customerId = a.CUSTOMERID,
                                 productId = a.PRODUCTID,
                                 companyId = a.COMPANYID,
                                 casaAccountId = a.CASAACCOUNTID,
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
                                 bookingDate = systemDate,
                                 overdraftLimit = (decimal)b.OVERDRAFTTOPUP,
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
                                 revolvingTypeId = a.REVOLVINGTYPEID,
                                 productAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                 serialNumber = a.SERIALNUMBER,
                                 casaAccountNumber = c.PRODUCTACCOUNTNUMBER,


                             }).FirstOrDefault();

                List<TBL_LOAN_REVOLVING> overDraft = new List<TBL_LOAN_REVOLVING>();



                //foreach (var model in model)
                //{
                model.productTypeId = (int)LoanSystemTypeEnum.OverdraftFacility;
                var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(model.branchId, model.productId, model.productTypeId);
                TBL_LOAN_REVOLVING addOverDraft = new TBL_LOAN_REVOLVING();

                addOverDraft.CUSTOMERID = model.customerId;
                addOverDraft.LOANSYSTEMTYPEID = model.loanSystemTypeId;
                addOverDraft.PRODUCTID = model.productId;
                addOverDraft.COMPANYID = model.companyId;
                addOverDraft.CASAACCOUNTID = model.casaAccountId;
                addOverDraft.BRANCHID = model.branchId;
                addOverDraft.CURRENCYID = model.currencyId;
                addOverDraft.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                addOverDraft.EXCHANGERATE = model.exchangeRate;
                addOverDraft.LOANREFERENCENUMBER = loanReferenceNumber;
                addOverDraft.RELATED_LOAN_REFERENCE_NUMBER = model.loanReferenceNumber;
                addOverDraft.SUBSECTORID = model.subSectorId;
                addOverDraft.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
                addOverDraft.RELATIONSHIPMANAGERID = model.relationshipManagerId;
                addOverDraft.MISCODE = model.misCode;
                addOverDraft.TEAMMISCODE = model.teamMiscode;
                addOverDraft.INTERESTRATE = model.interestRate;
                addOverDraft.EFFECTIVEDATE = model.effectiveDate;
                addOverDraft.MATURITYDATE = model.maturityDate;
                addOverDraft.BOOKINGDATE = model.bookingDate;
                addOverDraft.OVERDRAFTLIMIT = model.overdraftLimit;
                addOverDraft.APPROVALSTATUSID = model.approvalStatusId;
                addOverDraft.APPROVEDBY = model.approvedBy;
                addOverDraft.APPROVERCOMMENT = model.approverComment;
                addOverDraft.DATEAPPROVED = model.dateApproved;
                addOverDraft.LOANSTATUSID = model.loanStatusId;
                addOverDraft.ISDISBURSED = model.isDisbursed;
                addOverDraft.DISBURSEDBY = model.disbursedBy;
                addOverDraft.DISBURSERCOMMENT = model.disburserComment;
                addOverDraft.DISBURSEDATE = model.disburseDate;
                addOverDraft.OPERATIONID = model.operationId;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.DISCHARGELETTER = model.dischargeLetter;
                addOverDraft.SUSPENDINTEREST = model.suspendInterest;
                addOverDraft.DAYCOUNTCONVENTIONID = (short)model.dayCountConventionId;
                addOverDraft.INT_PRUDENT_GUIDELINE_STATUSID = 1; //model.internalPrudentialGuidelineStatusId;
                addOverDraft.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //model.externalPrudentialGuidelineStatusId;
                addOverDraft.USER_PRUDENTIAL_GUIDE_STATUSID = 1;
                addOverDraft.NPLDATE = model.nplDate;
                addOverDraft.CREATEDBY = model.createdBy;
                addOverDraft.DATETIMECREATED = model.dateTimeCreated;
                addOverDraft.REVOLVINGTYPEID = model.revolvingTypeId;

                overDraft.Add(addOverDraft);
                // }

                this.context.TBL_LOAN_REVOLVING.AddRange(overDraft);

                //context.SaveChanges();


                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                    var loan = model;
                    var reviewDate = loan.bookingDate.AddMonths(1);
                    var data = new OverDraftExtendViewModel
                    {
                        sanctionLimit = String.Format("{0:0.00}", initialLimit - loan.overdraftLimit),
                        sanctionReferenceNumber = loan.serialNumber,
                        accountNumber = loan.productAccountNumber,
                        expiryDate = loan.maturityDate.ToString("dd-MMM-yyyy", null),
                        reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                        createdDate = systemDate,
                    };
                    subAllocationExtentionResult = finacle.OverDraftExtend(data, twoFactorAuth);
                    var data1 = new OverDraftNormalViewModel
                    {
                        //sanctionLimit = String.Format("{0:0.00}", loan.overdraftLimit),
                        //sanctionReferenceNumber = loan.serialNumber,
                        //accountNumber = loan.productAccountNumber,
                        //expiryDate = loan.maturityDate.ToString("dd-MMM-yyyy", null),
                        //reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                        //createdDate = systemDate,
                        accountNumber = loan.casaAccountNumber,
                        applicationDate = loan.bookingDate.ToString("dd-MMM-yyyy", null),
                        documentDate = loan.bookingDate.ToString("dd-MMM-yyyy", null),
                        expiryDate = loan.maturityDate.ToString("dd-MMM-yyyy", null),
                        reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                        sanctionDate = loan.bookingDate.ToString("dd-MMM-yyyy", null),
                        sanctionLimit = String.Format("{0:0.00}", loan.overdraftLimit),
                        sanctionReferenceNumber = batchCode,//revolvingLoanRecord.LOANREFERENCENUMBER
                    };

                    subAllocationResult = finacle.OverDraftNormal(data1, twoFactorAuth);
                }

                addOverDraft.SERIALNUMBER = subAllocationResult.serialNumber;


                TBL_LOAN_REVOLVING results = (from p in context.TBL_LOAN_REVOLVING
                                              where p.REVOLVINGLOANID == loanId
                                              && p.LOANSTATUSID == (short)LoanStatusEnum.Active
                                              select p).SingleOrDefault();

                results.OVERDRAFTLIMIT = results.OVERDRAFTLIMIT - amount;
                var result = context.SaveChanges() > 0;

                if (subAllocationExtentionResult != null && subAllocationResult != null && result)
                {
                    // trans.Commit();
                    output = true;
                }
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
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

        public bool DeleteLoanExist(int loanId, DateTime applicationDate)
        {
            bool output = false;

            var removeLoan_Archive = (from p in context.TBL_LOAN_ARCHIVE
                                      where p.LOANID == loanId && p.CHANGEEFFECTIVEDATE == applicationDate
                                      select p);
            var removeLoan_Schedule_Periodic_Archive = (from p in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                                                        where p.LOANID == loanId && p.ARCHIVEDATE == applicationDate
                                                        select p);
            var removeLoan_Schedule_Daily_Archive = (from p in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                                                     where p.LOANID == loanId && p.ARCHIVEDATE == applicationDate
                                                     select p);
            var removeLoan_Schedule_Daily_Temp = (from p in context.TBL_LOAN_SCHEDULE_DAILY_TEMP
                                                  where p.LOANID == loanId
                                                  select p);
            var removeLoan_Schedule_Periodic_Temp = (from p in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                                                     where p.LOANID == loanId
                                                     select p);

            var removeOD_Archive = (from p in context.TBL_LOAN_REVOLVING_ARCHIVE
                                    where p.REVOLVINGLOANID == loanId && p.ARCHIVEDATE == applicationDate
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

        public LoanViewModel ArchiveLoan(int loanId, int operationId, string archiveBatchCode)
        {
            var systemDate = generalSetup.GetApplicationDate();
           // var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN
                         where a.TERMLOANID == loanId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new LoanViewModel()
                         {
                             loanId = a.TERMLOANID,
                             productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                             customerRiskRatingId = a.CUSTOMERRISKRATINGID,
                             loanSystemTypeId = (short)a.LOANSYSTEMTYPEID,
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

                         }).FirstOrDefault();

            List<TBL_LOAN_ARCHIVE> loanArchive = new List<TBL_LOAN_ARCHIVE>();



            //foreach (var item in model)
            //{

            TBL_LOAN_ARCHIVE addLoanArchive = new TBL_LOAN_ARCHIVE();


            addLoanArchive.CHANGEEFFECTIVEDATE = systemDate;
            addLoanArchive.ISAPPLIED = false;
            addLoanArchive.CHANGEREASON = "Rephasement";
            addLoanArchive.LOANID = model.loanId;
            addLoanArchive.PRODUCTPRICEINDEXRATE = model.productPriceIndexRate;
            addLoanArchive.CUSTOMERRISKRATINGID = model.customerRiskRatingId;
            addLoanArchive.LOANSYSTEMTYPEID = model.loanSystemTypeId;
            addLoanArchive.CUSTOMERID = model.customerId;
            addLoanArchive.PRODUCTID = model.productId;
            addLoanArchive.COMPANYID = model.companyId;
            addLoanArchive.CASAACCOUNTID = model.casaAccountId;
            addLoanArchive.BRANCHID = model.branchId;
            addLoanArchive.CURRENCYID = (short)model.currencyId;
            addLoanArchive.EXCHANGERATE = model.exchangeRate;
            addLoanArchive.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            addLoanArchive.LOANREFERENCENUMBER = model.loanReferenceNumber;
            addLoanArchive.SUBSECTORID = model.subSectorId;
            addLoanArchive.PRINCIPALFREQUENCYTYPEID = model.principalFrequencyTypeId;
            addLoanArchive.INTERESTFREQUENCYTYPEID = model.interestFrequencyTypeId;
            addLoanArchive.PRINCIPALNUMBEROFINSTALLMENT = model.principalNumberOfInstallment;
            addLoanArchive.INTERESTNUMBEROFINSTALLMENT = model.interestNumberOfInstallment;
            addLoanArchive.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
            addLoanArchive.RELATIONSHIPMANAGERID = model.relationshipManagerId;
            addLoanArchive.MISCODE = model.misCode;
            addLoanArchive.TEAMMISCODE = model.teamMiscode;
            addLoanArchive.INTERESTRATE = model.interestRate;
            addLoanArchive.EFFECTIVEDATE = model.effectiveDate;
            addLoanArchive.LASTRESTRUCTUREDATE = model.lastRestructureDate;
            addLoanArchive.MATURITYDATE = model.maturityDate;
            addLoanArchive.BOOKINGDATE = model.bookingDate;
            addLoanArchive.PRINCIPALAMOUNT = model.principalAmount;
            addLoanArchive.PRINCIPALINSTALLMENTLEFT = model.principalInstallmentLeft;
            addLoanArchive.INTERESTINSTALLMENTLEFT = model.interestInstallmentLeft;
            addLoanArchive.APPROVALSTATUSID = model.approvalStatusId;
            addLoanArchive.APPROVEDBY = model.approvedBy;
            addLoanArchive.APPROVERCOMMENT = model.approverComment;
            addLoanArchive.DATEAPPROVED = model.dateApproved;
            addLoanArchive.LOANSTATUSID = model.loanStatusId;
            addLoanArchive.CREATEDBY = model.createdBy;
            addLoanArchive.DATETIMECREATED = model.dateTimeCreated;
            addLoanArchive.SCHEDULETYPEID = model.scheduleTypeId;
            addLoanArchive.SCHEDULEDAYCOUNTCONVENTIONID = model.scheduleDayCountConventionId;
            addLoanArchive.SCHEDULEDAYINTERESTTYPEID = model.scheduleDayInterestTypeId;
            addLoanArchive.ISDISBURSED = model.isDisbursed;
            addLoanArchive.DISBURSEDBY = model.disbursedBy;
            addLoanArchive.DISBURSERCOMMENT = model.disburserComment;
            addLoanArchive.DISBURSEDATE = model.disburseDate;
            addLoanArchive.OPERATIONID = operationId;
            //addLoanArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID = item.customerGroupId;
            //addLoanArchive.LOANTYPEID = item.loanTypeId;
            //addLoanArchive.TrancheBatchCode = item.trancheBatchCode;
            addLoanArchive.EQUITYCONTRIBUTION = model.equityContribution;
            addLoanArchive.FIRSTPRINCIPALPAYMENTDATE = model.firstPrincipalPaymentDate;
            addLoanArchive.FIRSTINTERESTPAYMENTDATE = model.firstInterestPaymentDate;
            addLoanArchive.OUTSTANDINGPRINCIPAL = model.outstandingPrincipal;
            addLoanArchive.OUTSTANDINGINTEREST = model.outstandingInterest;

            addLoanArchive.PASTDUEPRINCIPAL = model.pastDuePrincipal;
            addLoanArchive.PASTDUEINTEREST = model.pastDueInterest;
            addLoanArchive.INTERESTONPASTDUEPRINCIPAL = model.interestOnPastDuePrincipal;
            addLoanArchive.INTERESTONPASTDUEINTEREST = model.interesrtOnPastDueInterest;
            addLoanArchive.PENALCHARGEAMOUNT = model.penalChargeAmount;
            addLoanArchive.PRINCIPALADDITIONCOUNT = model.principalAdditionCount;
            addLoanArchive.PRINCIPALREDUCTIONCOUNT = model.principalReductionCount;
            addLoanArchive.FIXEDPRINCIPAL = model.fixedPrincipal;
            addLoanArchive.PROFILELOAN = model.profileLoan;
            addLoanArchive.DISCHARGELETTER = model.dischargeLetter;
            addLoanArchive.SUSPENDINTEREST = model.suspendInterest;
            addLoanArchive.ISSCHEDULEDPREPAYMENT = model.isScheduledPrepayment;
            addLoanArchive.ALLOWFORCEDEBITREPAYMENT = model.allowForceDebitRepayment;
            addLoanArchive.SCHEDULEDPREPAYMENTAMOUNT = model.scheduledPrepaymentAmount;
            addLoanArchive.SCHEDULEDPREPAYMENTDATE = model.scheduledPrepaymentDate;
            addLoanArchive.SCH_PREPAYMENT_FREQUENCY_TYPID = model.principalFrequencyTypeId;//scheduledPrepaymentFrequencyTypeId;
            addLoanArchive.INT_PRUDENT_GUIDELINE_STATUSID = (int)model.internalPrudentialGuidelineStatusId;
            addLoanArchive.EXT_PRUDENT_GUIDELINE_STATUSID = (int)model.externalPrudentialGuidelineStatusId;
            addLoanArchive.USER_PRUDENTIAL_GUIDE_STATUSID = (int)model.userPrudentialGuidelineStatusId;
            addLoanArchive.NPLDATE = model.nplDate;
            addLoanArchive.CREATEDBY = model.createdBy;
            addLoanArchive.DATETIMECREATED = model.dateTimeCreated;
            addLoanArchive.ARCHIVEBATCHCODE = archiveBatchCode;


            loanArchive.Add(addLoanArchive);
            //}
            //tbl_Loan
            this.context.TBL_LOAN_ARCHIVE.AddRange(loanArchive);

            context.SaveChanges();
            return model;
        }

        public RevolvingLoanViewModel ArchiveOverDraft(int overDraftId, string archiveBatchCode)
        {
            var systemDate = generalSetup.GetApplicationDate();
            //var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.TBL_LOAN_REVOLVING
                         where a.REVOLVINGLOANID == overDraftId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                         select new RevolvingLoanViewModel()
                         {
                             loanId = a.REVOLVINGLOANID,
                             loanSystemTypeId = (short)a.LOANSYSTEMTYPEID,
                             customerId = a.CUSTOMERID,
                             productId = a.PRODUCTID,
                             companyId = a.COMPANYID,
                             casaAccountId = a.CASAACCOUNTID,
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
                             userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                             nplDate = a.NPLDATE,
                             createdBy = a.CREATEDBY,
                             dateTimeCreated = a.DATETIMECREATED,
                             revolvingTypeId = a.REVOLVINGTYPEID,

                         }).FirstOrDefault();

            List<TBL_LOAN_REVOLVING_ARCHIVE> overDraftArchive = new List<TBL_LOAN_REVOLVING_ARCHIVE>();



            //foreach (var item in model)
            //{

            TBL_LOAN_REVOLVING_ARCHIVE addOverDraftArchive = new TBL_LOAN_REVOLVING_ARCHIVE();


            addOverDraftArchive.ARCHIVEDATE = systemDate;
            addOverDraftArchive.REVOLVINGLOANID = model.loanId;
            addOverDraftArchive.CUSTOMERID = model.customerId;
            addOverDraftArchive.LOANSYSTEMTYPEID = model.loanSystemTypeId;
            addOverDraftArchive.PRODUCTID = model.productId;
            addOverDraftArchive.COMPANYID = model.companyId;
            addOverDraftArchive.CASAACCOUNTID = model.casaAccountId;
            addOverDraftArchive.BRANCHID = model.branchId;
            addOverDraftArchive.CURRENCYID = model.currencyId;
            addOverDraftArchive.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            addOverDraftArchive.EXCHANGERATE = model.exchangeRate;
            addOverDraftArchive.LOANREFERENCENUMBER = model.loanReferenceNumber;
            addOverDraftArchive.RELATED_LOAN_REFERENCE_NUMBER = model.relatedLoanReferenceNumber;
            addOverDraftArchive.SUBSECTORID = model.subSectorId;
            addOverDraftArchive.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
            addOverDraftArchive.RELATIONSHIPMANAGERID = model.relationshipManagerId;
            addOverDraftArchive.MISCODE = model.misCode;
            addOverDraftArchive.TEAMMISCODE = model.teamMiscode;
            addOverDraftArchive.INTERESTRATE = model.interestRate;
            addOverDraftArchive.EFFECTIVEDATE = model.effectiveDate;
            addOverDraftArchive.MATURITYDATE = model.maturityDate;
            addOverDraftArchive.BOOKINGDATE = model.bookingDate;
            addOverDraftArchive.OVERDRAFTLIMIT = model.overdraftLimit;
            addOverDraftArchive.APPROVALSTATUSID = model.approvalStatusId;
            addOverDraftArchive.APPROVEDBY = model.approvedBy;
            addOverDraftArchive.APPROVERCOMMENT = model.approverComment;
            addOverDraftArchive.DATEAPPROVED = model.dateApproved;
            addOverDraftArchive.LOANSTATUSID = model.loanStatusId;
            addOverDraftArchive.ISDISBURSED = model.isDisbursed;
            addOverDraftArchive.DISBURSEDBY = model.disbursedBy;
            addOverDraftArchive.DISBURSERCOMMENT = model.disburserComment;
            addOverDraftArchive.DISBURSEDATE = model.disburseDate;
            addOverDraftArchive.OPERATIONID = model.operationId;
            addOverDraftArchive.CREATEDBY = model.createdBy;
            addOverDraftArchive.DATETIMECREATED = model.dateTimeCreated;
            addOverDraftArchive.DISCHARGELETTER = model.dischargeLetter;
            addOverDraftArchive.SUSPENDINTEREST = model.suspendInterest;
            addOverDraftArchive.DAYCOUNTCONVENTIONID = (short)model.dayCountConventionId;
            addOverDraftArchive.INT_PRUDENT_GUIDELINE_STATUSID = (int)model.internalPrudentialGuidelineStatusId;
            addOverDraftArchive.EXT_PRUDENT_GUIDELINE_STATUSID = (int)model.externalPrudentialGuidelineStatusId;
            addOverDraftArchive.USER_PRUDENTIAL_GUIDE_STATUSID = (int)model.userPrudentialGuidelineStatusId;
            addOverDraftArchive.NPLDATE = model.nplDate;
            addOverDraftArchive.CREATEDBY = model.createdBy;
            addOverDraftArchive.DATETIMECREATED = model.dateTimeCreated;
            addOverDraftArchive.REVOLVINGTYPEID = model.revolvingTypeId;
            addOverDraftArchive.ARCHIVEBATCHCODE = archiveBatchCode;

            overDraftArchive.Add(addOverDraftArchive);
            //}
            //tbl_Loan
            this.context.TBL_LOAN_REVOLVING_ARCHIVE.AddRange(overDraftArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId, string archiveBatchCode)
        {
            var systemDate = generalSetup.GetApplicationDate();
           // var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
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
                addLoanSchedulePeriodicArchive.ARCHIVEDATE = systemDate;
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
                //addLoanSchedulePeriodicArchive.ARCHIVEDATE = generalSetup.GetApplicationDate();
                //addLoanSchedulePeriodicArchive.ARCHIVEBATCHCODE = batchCode;
                addLoanSchedulePeriodicArchive.ARCHIVEBATCHCODE = archiveBatchCode;

                loanSchedulePeriodicArchive.Add(addLoanSchedulePeriodicArchive);

            }

            this.context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.AddRange(loanSchedulePeriodicArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId, string archiveBatchCode)
        {
            var systemDate = generalSetup.GetApplicationDate();
            //var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
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
                addLoanScheduleDailyArchive.ARCHIVEDATE = systemDate;
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
                //addLoanScheduleDailyArchive.ARCHIVEBATCHCODE = batchCode;
                addLoanScheduleDailyArchive.ARCHIVEBATCHCODE = archiveBatchCode;

                loanScheduleDailyArchive.Add(addLoanScheduleDailyArchive);

            }

            this.context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV.AddRange(loanScheduleDailyArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule(int loanId, DateTime applicationDate)
        {

            var systemDate = generalSetup.GetApplicationDate();
            int no = 0;//number.Count() - 1;

            var model = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                         where a.LOANID == loanId && a.ARCHIVEDATE == DbFunctions.TruncateTime(systemDate)
                         && a.PAYMENTDATE < DbFunctions.TruncateTime(applicationDate)
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

            var systemDate = generalSetup.GetApplicationDate();
            int no = 0;//number.Count() - 1;

            var model = (from a in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                         where a.LOANID == loanId && a.ARCHIVEDATE == DbFunctions.TruncateTime(systemDate)
                         && a.DATE < DbFunctions.TruncateTime(applicationDate)
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
                          //&& a.PAYMENTNUMBER != 0
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

        //public bool InterestRateReview(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        //{
        //    bool output = false;
        //    var systemDate = generalSetup.GetApplicationDate();

        //    if (LoanExist(loanId) > 0)
        //    {
        //        DeleteLoanExist(loanId, systemDate);
        //        ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
        //        ArchivePeriodicSchedule(loanId);
        //        ArchiveDailySchedule(loanId);


        //        //----------generate and save periodic loan schedule -----------------------------------
        //        List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

        //        List<TBL_LOAN_SCHEDULE_PERIODIC_TMP> tblPeriodicScheduleTemp = new List<TBL_LOAN_SCHEDULE_PERIODIC_TMP>();
        //        foreach (var item in periodicScheduleTemp)
        //        {
        //            TBL_LOAN_SCHEDULE_PERIODIC_TMP scheduleTemp = new TBL_LOAN_SCHEDULE_PERIODIC_TMP();

        //            scheduleTemp.LOANID = loanId;
        //            scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
        //            scheduleTemp.PAYMENTDATE = item.paymentDate;
        //            scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
        //            scheduleTemp.PERIODPAYMENTAMOUNT = Convert.ToDecimal(item.periodPaymentAmount);
        //            scheduleTemp.PERIODINTERESTAMOUNT = Convert.ToDecimal(item.periodInterestAmount);
        //            scheduleTemp.PERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.periodPrincipalAmount);
        //            scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
        //            scheduleTemp.INTERESTRATE = loanInput.interestRate;

        //            scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
        //            scheduleTemp.AMORTISEDPERIODPAYMENTAMOUNT = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
        //            scheduleTemp.AMORTISEDPERIODINTERESTAMOUNT = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
        //            scheduleTemp.AMORTISEDPERIODPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
        //            scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
        //            scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
        //            scheduleTemp.CREATEDBY = staffId;
        //            scheduleTemp.DATETIMECREATED = systemDate;

        //            tblPeriodicScheduleTemp.Add(scheduleTemp);
        //        }
        //        //-------------------------------------------------------------------------------------


        //        //----------generate and save daily loan schedule -----------------------------------
        //        List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

        //        List<TBL_LOAN_SCHEDULE_DAILY_TEMP> tblDailyScheduleTemp = new List<TBL_LOAN_SCHEDULE_DAILY_TEMP>();

        //        foreach (var item in dailyScheduleTemp)
        //        {
        //            TBL_LOAN_SCHEDULE_DAILY_TEMP scheduleTemp = new TBL_LOAN_SCHEDULE_DAILY_TEMP();

        //            scheduleTemp.LOANID = loanId;
        //            scheduleTemp.PAYMENTNUMBER = item.paymentNumber;
        //            scheduleTemp.DATE = item.date;
        //            scheduleTemp.PAYMENTDATE = item.paymentDate;
        //            scheduleTemp.OPENINGBALANCE = Convert.ToDecimal(item.openingBalance);
        //            scheduleTemp.STARTPRINCIPALAMOUNT = Convert.ToDecimal(item.startPrincipalAmount);
        //            scheduleTemp.DAILYPAYMENTAMOUNT = Convert.ToDecimal(item.dailyPaymentAmount);
        //            scheduleTemp.DAILYINTERESTAMOUNT = Convert.ToDecimal(item.dailyInterestAmount);
        //            scheduleTemp.DAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.dailyPrincipalAmount);
        //            scheduleTemp.CLOSINGBALANCE = Convert.ToDecimal(item.closingBalance);
        //            scheduleTemp.ENDPRINCIPALAMOUNT = Convert.ToDecimal(item.endPrincipalAmount);
        //            scheduleTemp.ACCRUEDINTEREST = Convert.ToDecimal(item.accruedInterest);
        //            scheduleTemp.AMORTISEDCOST = Convert.ToDecimal(item.amortisedCost);
        //            scheduleTemp.INTERESTRATE = item.norminalInterestRate;

        //            scheduleTemp.AMORTISEDOPENINGBALANCE = Convert.ToDecimal(item.amOpeningBalance);
        //            scheduleTemp.AMORTISEDSTARTPRINCIPALAMOUNT = Convert.ToDecimal(item.amStartPrincipalAmount);
        //            scheduleTemp.AMORTISEDDAILYPAYMENTAMOUNT = Convert.ToDecimal(item.amDailyPaymentAmount);
        //            scheduleTemp.AMORTISEDDAILYINTERESTAMOUNT = Convert.ToDecimal(item.amDailyInterestAmount);
        //            scheduleTemp.AMORTISEDDAILYPRINCIPALAMOUNT = Convert.ToDecimal(item.amDailyPrincipalAmount);
        //            scheduleTemp.AMORTISEDCLOSINGBALANCE = Convert.ToDecimal(item.amClosingBalance);
        //            scheduleTemp.AMORTISEDENDPRINCIPALAMOUNT = Convert.ToDecimal(item.amEndPrincipalAmount);
        //            scheduleTemp.AMORTISEDACCRUEDINTEREST = Convert.ToDecimal(item.amAccruedInterest);
        //            scheduleTemp.AMORTISED_AMORTISEDCOST = Convert.ToDecimal(item.amAmortisedCost);
        //            scheduleTemp.DISCOUNTPREMIUM = Convert.ToDecimal(item.discountPremium);
        //            scheduleTemp.UNEARNEDFEE = Convert.ToDecimal(item.unEarnedFee);
        //            scheduleTemp.EARNEDFEE = Convert.ToDecimal(item.earnedFee);
        //            scheduleTemp.EFFECTIVEINTERESTRATE = item.effectiveInterestRate;
        //            scheduleTemp.NUMBEROFPERIODS = item.numberOfPeriods;
        //            scheduleTemp.BALLONAMOUNT = Convert.ToDecimal(item.balloonAmt);
        //            scheduleTemp.CREATEDBY = staffId;
        //            scheduleTemp.DATETIMECREATED = systemDate;

        //            tblDailyScheduleTemp.Add(scheduleTemp);
        //        }
        //        //----------------------------------------------------------------


        //        //------------adding records to the database--------------------------

        //        //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
        //        //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


        //        this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

        //        this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
        //        context.SaveChanges();
        //        //----------update loan details -----------------------------------
        //        var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);

        //        loan.INTERESTRATE = loanInput.interestRate;
        //        loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
        //        loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
        //        loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
        //        //-------------------------------------------------

        //        MergePeriodicScheduleForInterest(loanId, applicationDate);

        //        context.SaveChanges();
        //        //-------------------------------------------------------
        //    }




        //    output = true;

        //    return output;
        //}

        public bool InterestRateReview(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{

            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();
                if (LoanExist(loanId) > 0)
                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);


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

                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);
                    //----------update loan details -----------------------------------
                    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);

                    //loan.INTERESTRATE = loanInput.interestRate;
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;

                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                    loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                    //loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                    //loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                    //loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    //loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                    //-------------------------------------------------




                    //-------------------------------------------------------
                }
                var result = context.SaveChanges() > 0;

                if (result)
                {
                    // trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                // trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public IEnumerable<LoanViewModel> LoanHistory()
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
                var model = (from a in context.TBL_LOAN
                             where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                             select new LoanViewModel()
                             {
                                 loanId = a.TERMLOANID,
                                 productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                                 customerRiskRatingId = a.CUSTOMERRISKRATINGID,
                                 loanSystemTypeId = (short)a.LOANSYSTEMTYPEID,
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

                List<TBL_LOAN_ARCHIVE> loanHistory = new List<TBL_LOAN_ARCHIVE>();



                foreach (var item in model)
                {

                    TBL_LOAN_ARCHIVE addLoanHistory = new TBL_LOAN_ARCHIVE();


                    addLoanHistory.CHANGEEFFECTIVEDATE = systemDate;
                    addLoanHistory.ISAPPLIED = false;
                    addLoanHistory.CHANGEREASON = "Loan History";
                    addLoanHistory.LOANID = item.loanId;
                    addLoanHistory.PRODUCTPRICEINDEXRATE = item.productPriceIndexRate;
                    addLoanHistory.CUSTOMERRISKRATINGID = item.customerRiskRatingId;
                    addLoanHistory.LOANSYSTEMTYPEID = item.loanSystemTypeId;
                    addLoanHistory.CUSTOMERID = item.customerId;
                    addLoanHistory.PRODUCTID = item.productId;
                    addLoanHistory.COMPANYID = item.companyId;
                    addLoanHistory.CASAACCOUNTID = item.casaAccountId;
                    addLoanHistory.BRANCHID = item.branchId;
                    addLoanHistory.CURRENCYID = (short)item.currencyId;
                    addLoanHistory.EXCHANGERATE = item.exchangeRate;
                    addLoanHistory.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                    addLoanHistory.LOANREFERENCENUMBER = item.loanReferenceNumber;
                    addLoanHistory.SUBSECTORID = item.subSectorId;
                    addLoanHistory.PRINCIPALFREQUENCYTYPEID = item.principalFrequencyTypeId;
                    addLoanHistory.INTERESTFREQUENCYTYPEID = item.interestFrequencyTypeId;
                    addLoanHistory.PRINCIPALNUMBEROFINSTALLMENT = item.principalNumberOfInstallment;
                    addLoanHistory.INTERESTNUMBEROFINSTALLMENT = item.interestNumberOfInstallment;
                    addLoanHistory.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                    addLoanHistory.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                    addLoanHistory.MISCODE = item.misCode;
                    addLoanHistory.TEAMMISCODE = item.teamMiscode;
                    addLoanHistory.INTERESTRATE = item.interestRate;
                    addLoanHistory.EFFECTIVEDATE = item.effectiveDate;
                    addLoanHistory.LASTRESTRUCTUREDATE = item.lastRestructureDate;
                    addLoanHistory.MATURITYDATE = item.maturityDate;
                    addLoanHistory.BOOKINGDATE = item.bookingDate;
                    addLoanHistory.PRINCIPALAMOUNT = item.principalAmount;
                    addLoanHistory.PRINCIPALINSTALLMENTLEFT = item.principalInstallmentLeft;
                    addLoanHistory.INTERESTINSTALLMENTLEFT = item.interestInstallmentLeft;
                    addLoanHistory.APPROVALSTATUSID = item.approvalStatusId;
                    addLoanHistory.APPROVEDBY = item.approvedBy;
                    addLoanHistory.APPROVERCOMMENT = item.approverComment;
                    addLoanHistory.DATEAPPROVED = item.dateApproved;
                    addLoanHistory.LOANSTATUSID = item.loanStatusId;
                    addLoanHistory.CREATEDBY = item.createdBy;
                    addLoanHistory.DATETIMECREATED = item.dateTimeCreated;
                    addLoanHistory.SCHEDULETYPEID = item.scheduleTypeId;
                    addLoanHistory.SCHEDULEDAYCOUNTCONVENTIONID = item.scheduleDayCountConventionId;
                    addLoanHistory.SCHEDULEDAYINTERESTTYPEID = item.scheduleDayInterestTypeId;
                    addLoanHistory.ISDISBURSED = item.isDisbursed;
                    addLoanHistory.DISBURSEDBY = item.disbursedBy;
                    addLoanHistory.DISBURSERCOMMENT = item.disburserComment;
                    addLoanHistory.DISBURSEDATE = item.disburseDate;
                    addLoanHistory.OPERATIONID = (int)item.operationId;
                    addLoanHistory.EQUITYCONTRIBUTION = item.equityContribution;
                    addLoanHistory.FIRSTPRINCIPALPAYMENTDATE = item.firstPrincipalPaymentDate;
                    addLoanHistory.FIRSTINTERESTPAYMENTDATE = item.firstInterestPaymentDate;
                    addLoanHistory.OUTSTANDINGPRINCIPAL = item.outstandingPrincipal;
                    addLoanHistory.OUTSTANDINGINTEREST = item.outstandingInterest;
                    addLoanHistory.PASTDUEPRINCIPAL = item.pastDuePrincipal;
                    addLoanHistory.PASTDUEINTEREST = item.pastDueInterest;
                    addLoanHistory.INTERESTONPASTDUEPRINCIPAL = item.interestOnPastDuePrincipal;
                    addLoanHistory.INTERESTONPASTDUEINTEREST = item.interesrtOnPastDueInterest;
                    addLoanHistory.PENALCHARGEAMOUNT = item.penalChargeAmount;
                    addLoanHistory.PRINCIPALADDITIONCOUNT = item.principalAdditionCount;
                    addLoanHistory.PRINCIPALREDUCTIONCOUNT = item.principalReductionCount;
                    addLoanHistory.FIXEDPRINCIPAL = item.fixedPrincipal;
                    addLoanHistory.PROFILELOAN = item.profileLoan;
                    addLoanHistory.DISCHARGELETTER = item.dischargeLetter;
                    addLoanHistory.SUSPENDINTEREST = item.suspendInterest;
                    addLoanHistory.ISSCHEDULEDPREPAYMENT = item.isScheduledPrepayment;
                    addLoanHistory.ALLOWFORCEDEBITREPAYMENT = item.allowForceDebitRepayment;
                    addLoanHistory.SCHEDULEDPREPAYMENTAMOUNT = item.scheduledPrepaymentAmount;
                    addLoanHistory.SCHEDULEDPREPAYMENTDATE = item.scheduledPrepaymentDate;
                    addLoanHistory.SCH_PREPAYMENT_FREQUENCY_TYPID = item.principalFrequencyTypeId;//scheduledPrepaymentFrequencyTypeId;
                    addLoanHistory.INT_PRUDENT_GUIDELINE_STATUSID = (int)item.internalPrudentialGuidelineStatusId;
                    addLoanHistory.EXT_PRUDENT_GUIDELINE_STATUSID = (int)item.externalPrudentialGuidelineStatusId;
                    addLoanHistory.USER_PRUDENTIAL_GUIDE_STATUSID = (int)item.userPrudentialGuidelineStatusId;
                    addLoanHistory.NPLDATE = item.nplDate;
                    addLoanHistory.CREATEDBY = item.createdBy;
                    addLoanHistory.DATETIMECREATED = item.dateTimeCreated;

                    loanHistory.Add(addLoanHistory);
                }
                //tbl_Loan
                this.context.TBL_LOAN_ARCHIVE.AddRange(loanHistory);

                var result = context.SaveChanges() > 0;
                //return model;
                if (result)
                {
                    //trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                return null;

            }

        }

        public IEnumerable<RevolvingLoanViewModel> OverDraftHistory()
        {
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
                var model = (from a in context.TBL_LOAN_REVOLVING
                             where a.LOANSTATUSID == (short)LoanStatusEnum.Active
                             select new RevolvingLoanViewModel()
                             {
                                 loanId = a.REVOLVINGLOANID,
                                 loanSystemTypeId = (short)a.LOANSYSTEMTYPEID,
                                 customerId = a.CUSTOMERID,
                                 productId = a.PRODUCTID,
                                 companyId = a.COMPANYID,
                                 casaAccountId = a.CASAACCOUNTID,
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
                                 userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                                 nplDate = a.NPLDATE,
                                 createdBy = a.CREATEDBY,
                                 dateTimeCreated = a.DATETIMECREATED,

                             }).ToList();

                List<TBL_LOAN_REVOLVING_ARCHIVE> overDraftHistory = new List<TBL_LOAN_REVOLVING_ARCHIVE>();


                foreach (var item in model)
                {

                    TBL_LOAN_REVOLVING_ARCHIVE addOverDraftHistory = new TBL_LOAN_REVOLVING_ARCHIVE();


                    addOverDraftHistory.ARCHIVEDATE = systemDate;
                    addOverDraftHistory.REVOLVINGLOANID = item.loanId;
                    addOverDraftHistory.CUSTOMERID = item.customerId;
                    addOverDraftHistory.LOANSYSTEMTYPEID = item.loanSystemTypeId;
                    addOverDraftHistory.PRODUCTID = item.productId;
                    addOverDraftHistory.COMPANYID = item.companyId;
                    addOverDraftHistory.CASAACCOUNTID = item.casaAccountId;
                    addOverDraftHistory.BRANCHID = item.branchId;
                    addOverDraftHistory.CURRENCYID = item.currencyId;
                    addOverDraftHistory.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
                    addOverDraftHistory.EXCHANGERATE = item.exchangeRate;
                    addOverDraftHistory.LOANREFERENCENUMBER = item.loanReferenceNumber;
                    addOverDraftHistory.RELATED_LOAN_REFERENCE_NUMBER = item.relatedLoanReferenceNumber;
                    addOverDraftHistory.SUBSECTORID = item.subSectorId;
                    addOverDraftHistory.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
                    addOverDraftHistory.RELATIONSHIPMANAGERID = item.relationshipManagerId;
                    addOverDraftHistory.MISCODE = item.misCode;
                    addOverDraftHistory.TEAMMISCODE = item.teamMiscode;
                    addOverDraftHistory.INTERESTRATE = item.interestRate;
                    addOverDraftHistory.EFFECTIVEDATE = item.effectiveDate;
                    addOverDraftHistory.MATURITYDATE = item.maturityDate;
                    addOverDraftHistory.BOOKINGDATE = item.bookingDate;
                    addOverDraftHistory.OVERDRAFTLIMIT = item.overdraftLimit;
                    addOverDraftHistory.APPROVALSTATUSID = item.approvalStatusId;
                    addOverDraftHistory.APPROVEDBY = item.approvedBy;
                    addOverDraftHistory.APPROVERCOMMENT = item.approverComment;
                    addOverDraftHistory.DATEAPPROVED = item.dateApproved;
                    addOverDraftHistory.LOANSTATUSID = item.loanStatusId;
                    addOverDraftHistory.ISDISBURSED = item.isDisbursed;
                    addOverDraftHistory.DISBURSEDBY = item.disbursedBy;
                    addOverDraftHistory.DISBURSERCOMMENT = item.disburserComment;
                    addOverDraftHistory.DISBURSEDATE = item.disburseDate;
                    addOverDraftHistory.OPERATIONID = item.operationId;
                    addOverDraftHistory.CREATEDBY = item.createdBy;
                    addOverDraftHistory.DATETIMECREATED = item.dateTimeCreated;
                    addOverDraftHistory.DISCHARGELETTER = item.dischargeLetter;
                    addOverDraftHistory.SUSPENDINTEREST = item.suspendInterest;
                    addOverDraftHistory.DAYCOUNTCONVENTIONID = (short)item.dayCountConventionId;
                    addOverDraftHistory.INT_PRUDENT_GUIDELINE_STATUSID = (int)item.internalPrudentialGuidelineStatusId;
                    addOverDraftHistory.EXT_PRUDENT_GUIDELINE_STATUSID = (int)item.externalPrudentialGuidelineStatusId;
                    addOverDraftHistory.USER_PRUDENTIAL_GUIDE_STATUSID = (int)item.userPrudentialGuidelineStatusId;
                    addOverDraftHistory.NPLDATE = item.nplDate;
                    addOverDraftHistory.CREATEDBY = item.createdBy;
                    addOverDraftHistory.DATETIMECREATED = item.dateTimeCreated;

                    overDraftHistory.Add(addOverDraftHistory);
                }

                this.context.TBL_LOAN_REVOLVING_ARCHIVE.AddRange(overDraftHistory);

                var result = context.SaveChanges() > 0;
                // return model;
                if (result)
                {
                    // trans.Commit();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                // trans.Rollback();
                return null;

            }

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

        [OperationBehavior(TransactionScopeRequired = true)]////Review This Method
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

            result.PRINCIPALFREQUENCYTYPEID = PrincipalFrequency;
            result.INTERESTFREQUENCYTYPEID = InterestFrequency;
            context.SaveChanges();
        }

        public bool UpdateLoanPrepaymentSchedule(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {

            //using (var trans = context.Database.BeginTransaction())
            //{
            bool output = false;
            double penalAmount = 0;
            try

            {
                var systemDate = generalSetup.GetApplicationDate();
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var penalCharge = context.TBL_CHARGE_FEE.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.Prepayment);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if(accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (principalOutStandingBalance + pastDue + accruedInterest);
                if (penalCharge == null)
                {
                    penalAmount = 0;
                }
                //else
                //{

                //    penalAmount = loanInput.principalAmount * (double)(penalCharge.RATE / 100);
                //}

                decimal partPayment = (decimal)loanInput.payAmount - (pastDue + accruedInterest);

                int value = product.INTERESTRECEIVABLEPAYABLEGL.Value;

                //finacle.GetGlAccountCode(product.INTERESTRECEIVABLEPAYABLEGL.Value, item.currencyId, item.branchId)

                //var penalGL = penalCharge.CHARGEFEEID;

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                if (loanInput.payAmount >= (double)(totalamount + (decimal)penalAmount))
                {
                    //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentFeePosting(loanInput, (decimal)penalAmount, penalCharge.CHARGEFEEID, "Penal Charge", (int)OperationsEnum.Prepayment));///change to charge GL

                    //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, pastDue, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Past Due", (int)OperationsEnum.InterestPastDueLoanRepayment));

                    inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, twoFactorAuth, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest", (int)OperationsEnum.InterestLoanRepayment));

                    inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, twoFactorAuth, principalOutStandingBalance, product.PRINCIPALBALANCEGL.Value, "Principal Amount", (int)OperationsEnum.PrincipalLoanRepayment));

                    //financeTransaction.PostTransaction(inputTransactions);
                    updateloanTableStatus(loanInput.loanId);
                }
                else
                {
                    // inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentFeePosting(loanInput, (decimal)penalAmount, penalCharge.CHARGEFEEID, "Penal Charge", (int)OperationsEnum.Prepayment));///change to charge GL

                    //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, pastDue, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Past Due", (int)OperationsEnum.InterestPastDueLoanRepayment));

                    inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, twoFactorAuth, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest", (int)OperationsEnum.InterestLoanRepayment));

                    inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, twoFactorAuth, partPayment, product.PRINCIPALBALANCEGL.Value, "Principal Amount", (int)OperationsEnum.PrincipalLoanRepayment));


                    if (LoanExist(loanId) > 0)
                    {
                        var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                        DeleteLoanExist(loanId, systemDate);
                        ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                        ArchivePeriodicSchedule(loanId, archiveBatchCode);
                        ArchiveDailySchedule(loanId, archiveBatchCode);

                        loanInput.principalAmount = ((double)totalamount - (loanInput.payAmount + penalAmount));
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
                                                                                                  //context.SaveChanges();

                        MergePeriodicSchedule(loanId, applicationDate);
                        MergeDailySchedule(loanId, applicationDate);

                        var outstInterest = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                                            where d.LOANID == loanId
                                            let sumPrincipalAmount = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId).Sum(a => a.PERIODINTERESTAMOUNT)
                                            select sumPrincipalAmount;
                        var outstandingInterest = outstInterest.FirstOrDefault();
                        //----------update loan details -----------------------------------
                        //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                        loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                        loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                        loan.INTERESTNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                        loan.OUTSTANDINGPRINCIPAL = loan.OUTSTANDINGPRINCIPAL - (decimal)loanInput.payAmount;
                        loan.OUTSTANDINGINTEREST = outstandingInterest;
                        loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                        //loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                        //loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                        //loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                        //loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                        //loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                        //-------------------------------------------------




                        //List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                        //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)loanInput.newAmount, penalCharge.GLAccountId, "Penal Charge"));
                        //financeTransaction.PostTransaction(inputTransactions);

                        //-------------------------------------------------------
                    }

                }
                var result = context.SaveChanges() > 0;

                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = true;
            }
            catch (BadLogicException be)
            {
                throw new BadLogicException(be.Message);
            }
            catch (ConditionNotMetException ce)
            {
                throw new ConditionNotMetException(ce.Message);
            }
            catch (APIErrorException ae)
            {
                throw new APIErrorException(ae.Message);
            }
            catch (TwoFactorAuthenticationException fa)
            {
                throw new TwoFactorAuthenticationException(fa.Message);
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;

        }

        public bool PaymentFrequencyChange(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();

                if (LoanExist(loanId) > 0)

                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);


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

                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);
                    //----------update loan details -----------------------------------
                    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;

                    if (loanInput.operationId == (short)OperationsEnum.InterestandPrincipalFrequencyChange)
                    {
                        loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                        loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                    }
                    else if (loanInput.operationId == (short)OperationsEnum.InterestFrequencyChange)
                    {
                        loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                    }
                    else
                    {
                        loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                    }
                    //loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                    //loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                    //loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                    //loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    //loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                    //-------------------------------------------------

                    //-------------------------------------------------------
                }

                var result = context.SaveChanges() > 0;

                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }

            return output;
        }

        public bool PaymentDateChange(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                var systemDate = generalSetup.GetApplicationDate();
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();
                if (LoanExist(loanId) > 0)
                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);


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


                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);
                    //----------update loan details -----------------------------------
                    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                    loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                    //-------------------------------------------------


                    //-------------------------------------------------------
                }
                var result = context.SaveChanges() > 0;

                if (result)
                {
                    // trans.Commit();
                    output = true;
                }
                //output = true;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }

            return output;
        }

        public bool LoanReversal(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            bool result = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                decimal pastDueRate = 0;//change to pastdueRate
                var systemDate = generalSetup.GetApplicationDate();
                loanInput.date = applicationDate;
                //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                //var loanScheDaily = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate);
                //var loanSchePeriodic = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(x => x.TBL_LOAN.TERMLOANID == loanId
                //&& x.PAYMENTDATE >= applicationDate && x.PAYMENTDATE <= systemDate && x.PAYMENTNUMBER != 0).OrderBy(x => x.PERIODICSCHEDULEID);
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                //decimal accruedInterest = loanScheDaily.FirstOrDefault().ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                //int Count = loanSchePeriodic.Count();
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                ArchivePeriodicSchedule(loanId, archiveBatchCode);
                ArchiveDailySchedule(loanId, archiveBatchCode);

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
                result = LoanBackDateFunction(loanId, applicationDate, systemDate, pastDueRate, loanInput);

                //var data1 = from d in context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV
                //            where d.LOANID == loanId
                //            let dailyAccruedInterest = context.TBL_LOAN_SCHEDULE_DAILY_ARCHIV.Where(a => a.LOANID == loanId
                //            && a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                //            ).Sum(a => (double?)a.DAILYINTERESTAMOUNT ?? 0)
                //            select dailyAccruedInterest;

                //decimal previousAccruedInterest = (decimal?)data1.FirstOrDefault() ?? 0;
                //decimal.Round(previousAccruedInterest, 2, MidpointRounding.AwayFromZero);

                //var data2 = from d in context.TBL_LOAN_SCHEDULE_DAILY_TEMP
                //            where d.LOANID == loanId
                //            let dailyAccruedInterest = context.TBL_LOAN_SCHEDULE_DAILY_TEMP.Where(a => a.LOANID == loanId
                //            && a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                //            ).Sum(a => (double?)a.DAILYINTERESTAMOUNT ?? 0)
                //            select dailyAccruedInterest;

                //decimal currentAccruedInterest = (decimal?)data2.FirstOrDefault() ?? 0;
                //decimal.Round(currentAccruedInterest, 2, MidpointRounding.AwayFromZero);

                //var data3 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                //            where d.LOANID == loanId
                //            let periodicInterest = context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.Where(a => a.LOANID == loanId
                //            && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                //            ).Sum(a => (double?)a.PERIODINTERESTAMOUNT ?? 0)
                //            select periodicInterest;

                //decimal previousPeriodicInterest = (decimal?)data3.FirstOrDefault() ?? 0;
                //decimal.Round(previousPeriodicInterest, 2, MidpointRounding.AwayFromZero);

                //var data4 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                //            where d.LOANID == loanId
                //            let periodicInterest = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId
                //            && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                //            ).Sum(a => (double?)a.PERIODINTERESTAMOUNT ?? 0)
                //            select periodicInterest;

                //decimal currentPeriodicInterest = (decimal?)data4.FirstOrDefault() ?? 0;
                //decimal.Round(currentPeriodicInterest, 2, MidpointRounding.AwayFromZero);


                //var data5 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                //            where d.LOANID == loanId
                //            let periodicPrincipal = context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.Where(a => a.LOANID == loanId
                //            && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                //            ).Sum(a => (double?)a.PERIODPRINCIPALAMOUNT ?? 0)
                //            select periodicPrincipal;

                //decimal previousPeriodicPrincipal = (decimal?)data5.FirstOrDefault() ?? 0;
                //decimal.Round(previousPeriodicPrincipal, 2, MidpointRounding.AwayFromZero);

                //var data6 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                //            where d.LOANID == loanId
                //            let periodicPrincipal = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId
                //            && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                //            ).Sum(a => (double?)a.PERIODPRINCIPALAMOUNT ?? 0)
                //            select periodicPrincipal;

                //decimal currentPeriodicPrincipal = (decimal?)data6.FirstOrDefault() ?? 0;
                //decimal.Round(currentPeriodicPrincipal, 2, MidpointRounding.AwayFromZero);

                //var data = (from a in context.TBL_LOAN_PAST_DUE
                //            join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.LOANID equals b.LOANID
                //            where a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                //            && a.TRANSACTIONTYPEID == (int)LoanTransactionTypeEnum.Interest && a.DATE == b.PAYMENTDATE

                //            select new PastDueOnPastDueViewModel()
                //            {
                //                date = a.DATE,
                //                amount = b.PERIODINTERESTAMOUNT

                //            }).ToList();

                //List<PastDueOnPastDueViewModel1> abc = new List<PastDueOnPastDueViewModel1>();


                //foreach (var item in data)
                //{
                //    PastDueOnPastDueViewModel1 xyz = new PastDueOnPastDueViewModel1();

                //    xyz.date = item.date;
                //    xyz.amount = item.amount;
                //    xyz.count = (int)(systemDate - item.date).TotalDays;
                //    xyz.interestOnAmount = ((pastDueRate / 100) * ((int)(systemDate - item.date).TotalDays) * item.amount);

                //    abc.Add(xyz);

                //}

                //var data7 = from d in abc
                //            select d.interestOnAmount;
                //decimal currentPastDueInterest = (decimal?)data7.FirstOrDefault() ?? 0;



                //var model = (from a in context.TBL_LOAN_PAST_DUE
                //             join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.LOANID equals b.LOANID
                //             where a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                //             && a.TRANSACTIONTYPEID == (int)LoanTransactionTypeEnum.Principal && a.DATE == b.PAYMENTDATE

                //             select new PastDueOnPastDueViewModel()
                //             {
                //                 date = a.DATE,
                //                 amount = b.PERIODINTERESTAMOUNT

                //             }).ToList();

                //List<PastDueOnPastDueViewModel1> abcPrincipal = new List<PastDueOnPastDueViewModel1>();


                //foreach (var item in model)
                //{
                //    PastDueOnPastDueViewModel1 xyzPrincipal = new PastDueOnPastDueViewModel1();

                //    xyzPrincipal.date = item.date;
                //    xyzPrincipal.amount = item.amount;
                //    xyzPrincipal.count = (int)(systemDate - item.date).TotalDays;
                //    xyzPrincipal.interestOnAmount = ((pastDueRate / 100) * ((int)(systemDate - item.date).TotalDays) * item.amount);


                //    abcPrincipal.Add(xyzPrincipal);

                //}

                //var data8 = from d in abcPrincipal
                //            select d.interestOnAmount;
                //decimal currentPastDuePrincipal = (decimal?)data8.FirstOrDefault() ?? 0;


                //decimal accruedInterestDiff = previousAccruedInterest - currentAccruedInterest;
                //decimal periodicInterestDiff = previousPeriodicInterest - currentPeriodicInterest;
                //decimal periodicPrincipalDiff = previousPeriodicPrincipal - currentPeriodicPrincipal;
                //decimal pastDueInterestDiff = loan.INTERESTONPASTDUEINTEREST - currentPastDueInterest;
                //decimal pastDuePrincipalDiff = loan.INTERESTONPASTDUEPRINCIPAL - currentPastDuePrincipal;

                //if (Count < 1 && applicationDate == systemDate)
                //{
                //}
                //else if (Count < 1 && applicationDate <= systemDate)
                //{

                //    inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterestDiff, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest Reversal", 1)); //change later));

                //    result = financeTransaction.PostTransaction(inputTransactions);
                //}
                //else
                //{
                //    inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterestDiff, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest Reversal", 2));

                //    inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, periodicInterestDiff, product.PRINCIPALBALANCEGL.Value, "Interest Reversal", 3));

                //    inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, periodicPrincipalDiff, product.PRINCIPALBALANCEGL.Value, "Principal Reversal", 4));

                //    inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, pastDueInterestDiff, product.PRINCIPALBALANCEGL.Value, "PastDue Interest Reversal", 5));

                //    inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, pastDuePrincipalDiff, product.PRINCIPALBALANCEGL.Value, "PastDue Principal Reversal", 6));

                //    result = financeTransaction.PostTransaction(inputTransactions);
                //}
                //var result = context.SaveChanges()>0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                // output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }

            return output;
        }

        public bool LoanBackDateFunction(int loanId, DateTime applicationDate, DateTime systemDate, decimal pastDueRate, LoanPaymentRestructureScheduleInputViewModel loanInput)
        {
            bool output = false;
            var result = "";
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
            var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
            var loanSchePeriodic = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(x => x.TBL_LOAN.TERMLOANID == loanId
                   && x.PAYMENTDATE >= applicationDate && x.PAYMENTDATE <= systemDate && x.PAYMENTNUMBER != 0).OrderBy(x => x.PERIODICSCHEDULEID);
            int Count = loanSchePeriodic.Count();
            if (Count == 0)
            {
                throw new SecureException("Application Date not found in Payment Schedule");
            }

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

            decimal previousPeriodicInterest = (decimal?)data3.FirstOrDefault() ?? 0;
            decimal.Round(previousPeriodicInterest, 2, MidpointRounding.AwayFromZero);

            var data4 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                        where d.LOANID == loanId
                        let periodicInterest = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId
                        && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.PERIODINTERESTAMOUNT ?? 0)
                        select periodicInterest;

            decimal currentPeriodicInterest = (decimal?)data4.FirstOrDefault() ?? 0;
            decimal.Round(currentPeriodicInterest, 2, MidpointRounding.AwayFromZero);


            var data5 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                        where d.LOANID == loanId
                        let periodicPrincipal = context.TBL_LOAN_SCHEDULE_PERIODIC_ARC.Where(a => a.LOANID == loanId
                        && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.PERIODPRINCIPALAMOUNT ?? 0)
                        select periodicPrincipal;

            decimal previousPeriodicPrincipal = (decimal?)data5.FirstOrDefault() ?? 0;
            decimal.Round(previousPeriodicPrincipal, 2, MidpointRounding.AwayFromZero);

            var data6 = from d in context.TBL_LOAN_SCHEDULE_PERIODIC_TMP
                        where d.LOANID == loanId
                        let periodicPrincipal = context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.Where(a => a.LOANID == loanId
                        && a.PAYMENTDATE >= DbFunctions.TruncateTime(applicationDate) && a.PAYMENTDATE <= DbFunctions.TruncateTime(systemDate)
                        ).Sum(a => (double?)a.PERIODPRINCIPALAMOUNT ?? 0)
                        select periodicPrincipal;

            decimal currentPeriodicPrincipal = (decimal?)data6.FirstOrDefault() ?? 0;
            decimal.Round(currentPeriodicPrincipal, 2, MidpointRounding.AwayFromZero);

            var data = (from a in context.TBL_LOAN_PAST_DUE
                        join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.LOANID equals b.LOANID
                        where a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                        && a.TRANSACTIONTYPEID == (int)LoanTransactionTypeEnum.Interest && a.DATE == b.PAYMENTDATE

                        select new PastDueOnPastDueViewModel()
                        {
                            date = a.DATE,
                            amount = b.PERIODINTERESTAMOUNT

                        }).ToList();

            List<PastDueOnPastDueViewModel1> abc = new List<PastDueOnPastDueViewModel1>();


            foreach (var item in data)
            {
                PastDueOnPastDueViewModel1 xyz = new PastDueOnPastDueViewModel1();

                xyz.date = item.date;
                xyz.amount = item.amount;
                xyz.count = (int)(systemDate - item.date).TotalDays;
                xyz.interestOnAmount = ((pastDueRate / 100) * ((int)(systemDate - item.date).TotalDays) * item.amount);

                abc.Add(xyz);

            }

            var data7 = from d in abc
                        select d.interestOnAmount;
            decimal currentPastDueInterest = (decimal?)data7.FirstOrDefault() ?? 0;



            var model = (from a in context.TBL_LOAN_PAST_DUE
                         join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.LOANID equals b.LOANID
                         where a.DATE >= DbFunctions.TruncateTime(applicationDate) && a.DATE <= DbFunctions.TruncateTime(systemDate)
                         && a.TRANSACTIONTYPEID == (int)LoanTransactionTypeEnum.Principal && a.DATE == b.PAYMENTDATE

                         select new PastDueOnPastDueViewModel()
                         {
                             date = a.DATE,
                             amount = b.PERIODINTERESTAMOUNT

                         }).ToList();

            List<PastDueOnPastDueViewModel1> abcPrincipal = new List<PastDueOnPastDueViewModel1>();


            foreach (var item in model)
            {
                PastDueOnPastDueViewModel1 xyzPrincipal = new PastDueOnPastDueViewModel1();

                xyzPrincipal.date = item.date;
                xyzPrincipal.amount = item.amount;
                xyzPrincipal.count = (int)(systemDate - item.date).TotalDays;
                xyzPrincipal.interestOnAmount = ((pastDueRate / 100) * ((int)(systemDate - item.date).TotalDays) * item.amount);


                abcPrincipal.Add(xyzPrincipal);

            }

            var data8 = from d in abcPrincipal
                        select d.interestOnAmount;
            decimal currentPastDuePrincipal = (decimal?)data8.FirstOrDefault() ?? 0;


            decimal accruedInterestDiff = previousAccruedInterest - currentAccruedInterest;
            decimal periodicInterestDiff = previousPeriodicInterest - currentPeriodicInterest;
            decimal periodicPrincipalDiff = previousPeriodicPrincipal - currentPeriodicPrincipal;
            decimal pastDueInterestDiff = loan.INTERESTONPASTDUEINTEREST - currentPastDueInterest;
            decimal pastDuePrincipalDiff = loan.INTERESTONPASTDUEPRINCIPAL - currentPastDuePrincipal;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            if (Count < 1 && applicationDate >= systemDate)
            {
            }
            else if (Count < 1 && applicationDate <= systemDate)
            {

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterestDiff, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest Reversal", loanInput.operationId)); //change later));

                result = financeTransaction.PostTransaction(inputTransactions);
            }
            else
            {
                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterestDiff, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest Reversal", loanInput.operationId));

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, periodicInterestDiff, product.PRINCIPALBALANCEGL.Value, "Interest Reversal", loanInput.operationId));

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, periodicPrincipalDiff, product.PRINCIPALBALANCEGL.Value, "Principal Reversal", loanInput.operationId));

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, pastDueInterestDiff, product.PRINCIPALBALANCEGL.Value, "PastDue Interest Reversal", loanInput.operationId));

                inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, pastDuePrincipalDiff, product.PRINCIPALBALANCEGL.Value, "PastDue Principal Reversal", loanInput.operationId));

                result = financeTransaction.PostTransaction(inputTransactions);
            }
            return output;
        }

        public bool RegenerateSchedule(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

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
                loan.INTERESTNUMBEROFINSTALLMENT = periodicSchedule.Count() - 1;
                loan.OUTSTANDINGPRINCIPAL = (decimal)loanInput.principalAmount;
                loan.OUTSTANDINGINTEREST = (decimal)(periodicSchedule.Select(x => x.periodInterestAmount)).Sum();
                //------------------------------------------------

                var result = context.SaveChanges() > 0;

                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;

        }

        public bool TerminateAndRebookLoanSchedule(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId)
        {
            bool output = false;

            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                loanInput.date = applicationDate;

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, principalOutStandingBalance, product.PRINCIPALBALANCEGL.Value, "Loan Outstanding Principal Balance"));

                //financeTransaction.PostTransaction(inputTransactions, false, twoFactorAuth);

                TBL_LOAN results = (from p in context.TBL_LOAN
                                    where p.TERMLOANID == loanId
                                    select p).SingleOrDefault();

                results.LOANSTATUSID = (short)LoanStatusEnum.Terminated;



               CreateLoan(loanId);

               results.LOANSTATUSID = (short)LoanStatusEnum.Terminated;
                var result = context.SaveChanges() > 0;

                if (result)
                {
                    // trans.Commit();
                    output = true;
                }
                // output = false;
            }
            catch (Exception ex)
            {
                // trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }

            return output;

        }

        public LoanViewModel CreateLoan(int loanId)
        {
            
            
            var data  = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();
            var refNo = loanGenerate.GenerateLoanReferenceNumber((int)data.BRANCHID, (int)data.PRODUCTID, (int)LoanSystemTypeEnum.TermDisbursedFacility);
            //var refNo = CommonHelpers.GenerateRandomDigitCode(10);
            var model = (from b in context.TBL_LOAN_REVIEW_OPERATION
                         join a in context.TBL_LOAN on b.LOANID equals a.TERMLOANID
                         where  b.LOANID == loanId && b.OPERATIONCOMPLETED == false
                        // a.LOANSTATUSID == (short)LoanStatusEnum.Terminated &&
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
                             // lastRestructureDate = (DateTime)a.LASTRESTRUCTUREDATE,
                             principalAmount = (decimal)b.PREPAYMENT,
                             loanSystemTypeId = a.LOANSYSTEMTYPEID,
                             principalInstallmentLeft = 0,
                             interestInstallmentLeft = 0,
                             approvalStatusId = a.APPROVALSTATUSID,
                             approvedBy = a.APPROVEDBY,
                             approverComment = a.APPROVERCOMMENT,
                             dateApproved = DateTime.Today,
                             loanStatusId = (short)LoanStatusEnum.Active,
                             scheduleTypeId = (short)b.SCHEDULETYPEID,//1,///add column in table b
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
                             interestOnPastDuePrincipal = 0,
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
                             internalPrudentialGuidelineStatusId = 1,
                             externalPrudentialGuidelineStatusId = 1,
                             userPrudentialGuidelineStatusId = 1,
                             nplDate = null,
                             createdBy = (int)SystemStaff.System,
                             dateTimeCreated = DateTime.Today,

                         }).FirstOrDefault();

            List<TBL_LOAN> loan = new List<TBL_LOAN>();

            TBL_LOAN addLoan = new TBL_LOAN();

            addLoan.PRODUCTPRICEINDEXRATE = model.productPriceIndexRate;
            addLoan.CUSTOMERRISKRATINGID = model.customerRiskRatingId;
            addLoan.LOANSYSTEMTYPEID = model.loanSystemTypeId;
            addLoan.CUSTOMERID = model.customerId;
            addLoan.PRODUCTID = model.productId;
            addLoan.COMPANYID = model.companyId;
            addLoan.CASAACCOUNTID = model.casaAccountId;
            addLoan.CASAACCOUNTID2 = model.casaAccountId2;
            addLoan.BRANCHID = model.branchId;
            addLoan.CURRENCYID = (short)model.currencyId;
            addLoan.EXCHANGERATE = model.exchangeRate;
            addLoan.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            addLoan.LOANREFERENCENUMBER = model.loanReferenceNumber;
            addLoan.RELATED_LOAN_REFERENCE_NUMBER = model.RelatedloanReferenceNumber;
            addLoan.SUBSECTORID = model.subSectorId;
            addLoan.PRINCIPALFREQUENCYTYPEID = model.principalFrequencyTypeId;
            addLoan.INTERESTFREQUENCYTYPEID = model.interestFrequencyTypeId;
            addLoan.PRINCIPALNUMBEROFINSTALLMENT = model.principalNumberOfInstallment;
            addLoan.INTERESTNUMBEROFINSTALLMENT = model.interestNumberOfInstallment;
            addLoan.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
            addLoan.RELATIONSHIPMANAGERID = model.relationshipManagerId;
            addLoan.MISCODE = model.misCode;
            addLoan.TEAMMISCODE = model.teamMiscode;
            addLoan.INTERESTRATE = model.interestRate;
            addLoan.EFFECTIVEDATE = model.effectiveDate;
            addLoan.MATURITYDATE = model.maturityDate;
            addLoan.BOOKINGDATE = model.bookingDate;
            addLoan.PRINCIPALAMOUNT = model.principalAmount;
            addLoan.PRINCIPALINSTALLMENTLEFT = model.principalInstallmentLeft;
            addLoan.INTERESTINSTALLMENTLEFT = model.interestInstallmentLeft;
            addLoan.APPROVALSTATUSID = model.approvalStatusId;
            addLoan.APPROVEDBY = model.approvedBy;
            addLoan.APPROVERCOMMENT = model.approverComment;
            addLoan.DATEAPPROVED = model.dateApproved;
            addLoan.LOANSTATUSID = model.loanStatusId;
            addLoan.SCHEDULETYPEID = model.scheduleTypeId;
            addLoan.SCHEDULEDAYCOUNTCONVENTIONID = model.scheduleDayCountConventionId;
            addLoan.SCHEDULEDAYINTERESTTYPEID = model.scheduleDayInterestTypeId;
            addLoan.SHOULD_DISBURSE = model.shouldDisbursed;
            addLoan.ISDISBURSED = model.isDisbursed;
            addLoan.DISBURSEDBY = model.disbursedBy;
            addLoan.DISBURSERCOMMENT = model.disburserComment;
            addLoan.DISBURSEDATE = model.disburseDate;
            addLoan.OPERATIONID = model.operationId;
            addLoan.EQUITYCONTRIBUTION = model.equityContribution;
            addLoan.FIRSTPRINCIPALPAYMENTDATE = model.firstPrincipalPaymentDate;
            addLoan.FIRSTINTERESTPAYMENTDATE = model.firstInterestPaymentDate;
            addLoan.OUTSTANDINGPRINCIPAL = model.outstandingPrincipal;
            addLoan.OUTSTANDINGINTEREST = model.outstandingInterest;
            addLoan.PASTDUEPRINCIPAL = model.pastDuePrincipal;
            addLoan.PASTDUEINTEREST = model.pastDueInterest;
            addLoan.INTERESTONPASTDUEPRINCIPAL = model.interestOnPastDuePrincipal;
            addLoan.INTERESTONPASTDUEINTEREST = model.interesrtOnPastDueInterest;
            addLoan.PENALCHARGEAMOUNT = model.penalChargeAmount;
            addLoan.PRINCIPALADDITIONCOUNT = model.principalAdditionCount;
            addLoan.PRINCIPALREDUCTIONCOUNT = model.principalReductionCount;
            addLoan.FIXEDPRINCIPAL = model.fixedPrincipal;
            addLoan.PROFILELOAN = model.profileLoan;
            addLoan.DISCHARGELETTER = model.dischargeLetter;
            addLoan.SUSPENDINTEREST = model.suspendInterest;
            addLoan.ISSCHEDULEDPREPAYMENT = model.isScheduledPrepayment;
            addLoan.ALLOWFORCEDEBITREPAYMENT = model.allowForceDebitRepayment;
            addLoan.SCHEDULEDPREPAYMENTAMOUNT = model.scheduledPrepaymentAmount;
            addLoan.SCHEDULEDPREPAYMENTDATE = model.scheduledPrepaymentDate;
            addLoan.LOANSYSTEMTYPEID = model.loanSystemTypeId;
            addLoan.SCH_PREPAYMENT_FREQUENCY_TYPID = model.scheduledPrepaymentFrequencyTypeId;
            addLoan.INT_PRUDENT_GUIDELINE_STATUSID = (int)model.internalPrudentialGuidelineStatusId;
            addLoan.EXT_PRUDENT_GUIDELINE_STATUSID = (int)model.externalPrudentialGuidelineStatusId;
            addLoan.USER_PRUDENTIAL_GUIDE_STATUSID = (int)model.userPrudentialGuidelineStatusId;
            addLoan.NPLDATE = model.nplDate;
            addLoan.CREATEDBY = model.createdBy;
            addLoan.DATETIMECREATED = model.dateTimeCreated;

            loan.Add(addLoan);
            this.context.TBL_LOAN.AddRange(loan);

            context.SaveChanges();
            return model;
        }

        public bool CompleteWriteOff(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                var systemDate = generalSetup.GetApplicationDate();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

                var sllp = 27;////Get SLLP GL

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, sllp, "Interest Write off"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, principalOutStandingBalance, sllp, "principal Write off"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, pastDue, sllp, "past due Write off"));

                financeTransaction.PostTransaction(inputTransactions, false, twoFactorAuth);

                TBL_LOAN results = (from p in context.TBL_LOAN
                                    where p.TERMLOANID == loanId
                                    select p).SingleOrDefault();

                results.LOANSTATUSID = (short)LoanStatusEnum.WriteOff;
                context.SaveChanges();

                TBL_LOAN_CAMSOL loanCamsol = new TBL_LOAN_CAMSOL();

                loanCamsol.LOANID = loanId;
                loanCamsol.COMPANYID = loanInput.companyId;
                loanCamsol.BALANCE = totalamount;
                loanCamsol.DATE = applicationDate;
                //loanCamsol.TYPE = "Loan Complete Write Off";
                loanCamsol.CUSTOMERCODE = (context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == loanInput.customerId)).CUSTOMERCODE;

                this.context.TBL_LOAN_CAMSOL.Add(loanCamsol); ////change to Temp table
                                                              //context.SaveChanges();
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

                var lienReference = casaLien.PlaceLien(lien, twoFactorAuth);



                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;

        }

        public bool LoanCancellation(int loanId, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                var systemDate = generalSetup.GetApplicationDate();

                TBL_LOAN results = (from p in context.TBL_LOAN
                                    where p.TERMLOANID == loanId
                                    select p).SingleOrDefault();

                results.LOANSTATUSID = (short)LoanStatusEnum.Cancelled;

                var result = context.SaveChanges() > 0;

                if (result)
                {
                    // trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool LoanWorkOut(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            int installmentNo = 0;
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);
                loanInput.principalAmount = (double)totalamount - loanInput.payAmount;
                var sllp = 27;////Get SLLP GL

                if (loanInput.scheduleMethodId == (short)LoanScheduleTypeEnum.BulletPayment)
                {
                    installmentNo = 3;
                }
                else if (loanInput.scheduleMethodId == (short)LoanScheduleTypeEnum.BallonPayment)
                {
                    installmentNo = 7;
                }

                if (LoanExist(loanId) > 0)
                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);

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

                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);
                    //----------update loan details -----------------------------------
                    //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.PASTDUEPRINCIPAL = 0;
                    loan.PASTDUEINTEREST = 0;
                    loan.INTERESTONPASTDUEPRINCIPAL = 0;
                    loan.INTERESTONPASTDUEINTEREST = 0;
                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                    loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                    loan.INTERESTFREQUENCYTYPEID = (short)(reviewData.INTERESTFREQUENCYTYPEID == 0 ? installmentNo : reviewData.INTERESTFREQUENCYTYPEID);
                    loan.PRINCIPALFREQUENCYTYPEID = (short)(reviewData.PRINCIPALFREQUENCYTYPEID == 0 ? installmentNo : reviewData.PRINCIPALFREQUENCYTYPEID);
                    loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                    //-------------------------------------------------



                    //-------------------------------------------------------
                }
                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = true;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = true;

            }
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

        public bool LoanSales(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                var systemDate = generalSetup.GetApplicationDate();

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

                decimal writeOffAmount = totalamount - (decimal)loanInput.payAmount;

                var sllp = 11;////Get SLLP GL
                var GLAccount = this.context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanInput.loanId && x.OPERATIONTYPEID == loanInput.operationId
                && x.OPERATIONCOMPLETED == false).FirstOrDefault();

                int casaGLAccount = (int)GLAccount.CASA_ACCOUNTID;

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, writeOffAmount, sllp, "Loan Write-Off Amount"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, (decimal)loanInput.payAmount, casaGLAccount, "Loan Sales Amount"));

                financeTransaction.PostTransaction(inputTransactions, false, twoFactorAuth);

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

                var result = context.SaveChanges() > 0;

                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool TenorExtension(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                var systemDate = generalSetup.GetApplicationDate();
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var penalCharge = context.TBL_CHARGE_FEE.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.TenorChange);
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();

                if (LoanExist(loanId) > 0)
                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);


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
                    //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    //context.SaveChanges();
                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);

                    //----------update loan details -----------------------------------
                    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;
                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                    //loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                    //loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                    //loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                    //loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    //loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                    //-------------------------------------------------

                    //MergePeriodicSchedule(loanId, applicationDate);
                }
                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }



            //-------------------------------------------------------
            return output;
        }

        public bool Restructured(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            int installmentNo = 0;
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();
                //var penalCharge = context.TBL_CHARGE_FEE.FirstOrDefault(x => x.OPERATIONID == (int)OperationsEnum.Restructured);
                if (loanInput.scheduleMethodId == (short)LoanScheduleTypeEnum.BulletPayment)
                {
                    installmentNo = 3;
                }
                else if (loanInput.scheduleMethodId == (short)LoanScheduleTypeEnum.BallonPayment)
                {
                    installmentNo = 7;
                }
                if (LoanExist(loanId) > 0)
                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);


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
                    //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.TBL_LOAN_SCHEDULE_PERIODIC_TMP.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.TBL_LOAN_SCHEDULE_DAILY_TEMP.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    context.SaveChanges();

                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);

                    //----------update loan details -----------------------------------
                    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                    loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                    loan.INTERESTFREQUENCYTYPEID = (short)(reviewData.INTERESTFREQUENCYTYPEID == 0 ? installmentNo : reviewData.INTERESTFREQUENCYTYPEID);
                    loan.PRINCIPALFREQUENCYTYPEID = (short)(reviewData.PRINCIPALFREQUENCYTYPEID == 0 ? installmentNo : reviewData.PRINCIPALFREQUENCYTYPEID);
                    loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;

                    //-------------------------------------------------
                    //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, penalCharge.GLAccountId, "Penal Charge"));///change to charge GL
                    //(short)(item.productCode != "" ? 8 : 8);
                    //-------------------------------------------------------
                }

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool LoanTermination(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {

                var systemDate = generalSetup.GetApplicationDate();
                loanInput.date = applicationDate;

                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                // decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, product.INTERESTRECEIVABLEPAYABLEGL.Value, "Accrued Interest"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, principalOutStandingBalance, product.PRINCIPALBALANCEGL.Value, "Loan Outstanding Principal Balance"));

                financeTransaction.PostTransaction(inputTransactions);

                TBL_LOAN results = (from p in context.TBL_LOAN
                                    where p.TERMLOANID == loanId
                                    select p).SingleOrDefault();

                results.LOANSTATUSID = (short)LoanStatusEnum.Terminated;

                var result = context.SaveChanges() > 0;

                ///call disturbs loan method and posting
                if (result)
                {
                    // trans.Commit();
                    output = true;
                }
                // output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback()
                throw new SecureException(ex.Message);
                output = false;

            }
            return output;
        }

        public bool LoanRecovery(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            try
            {
                var systemDate = generalSetup.GetApplicationDate();
                var reviewData = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == loanId && x.OPERATIONTYPEID == loanInput.operationId && x.OPERATIONCOMPLETED == false).FirstOrDefault();
                var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanInput.productId);
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanInput.loanId).FirstOrDefault();
                decimal principalOutStandingBalance = loan.OUTSTANDINGPRINCIPAL;
                var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID);
                //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                decimal accruedInterest = 0;
                var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                               where a.TBL_LOAN.TERMLOANID == loanId && a.DATE == applicationDate
                               select a).FirstOrDefault();

                if (accrued != null)
                {
                    accruedInterest = accrued.ACCRUEDINTEREST;
                }
                else
                {
                    throw new SecureException("Application Date not found in Payment Schedule");
                }
                accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
                principalOutStandingBalance = decimal.Round(principalOutStandingBalance, 2, MidpointRounding.AwayFromZero);
                decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
                decimal totalamount = (accruedInterest + principalOutStandingBalance + pastDue);

                loanInput.principalAmount = (double)totalamount - loanInput.payAmount;
                var sllp = 27;////Get SLLP GL

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, (decimal)loanInput.payAmount, sllp, "Loan Write-Off Amount"));

                financeTransaction.PostTransaction(inputTransactions, false, twoFactorAuth);

                if (LoanExist(loanId) > 0)
                {
                    var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                    DeleteLoanExist(loanId, systemDate);
                    ArchiveLoan(loanId, loanInput.operationId, archiveBatchCode);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId, archiveBatchCode);
                    ArchiveDailySchedule(loanId, archiveBatchCode);

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

                    MergeDailySchedule(loanId, applicationDate);
                    MergePeriodicSchedule(loanId, applicationDate);
                    //----------update loan details -----------------------------------
                    //var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                    loan.MATURITYDATE = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PRINCIPALNUMBEROFINSTALLMENT = periodicScheduleTemp.Count() - 1;
                    loan.INTERESTNUMBEROFINSTALLMENT = loan.PRINCIPALNUMBEROFINSTALLMENT;

                    loan.EFFECTIVEDATE = reviewData.EFFECTIVEDATE;
                    //loan.INTERESTRATE = (double)reviewData.INTERATERATE;
                    //loan.INTERESTFREQUENCYTYPEID = (short)reviewData.INTERESTFREQUENCYTYPEID;
                    //loan.PRINCIPALFREQUENCYTYPEID = (short)reviewData.PRINCIPALFREQUENCYTYPEID;
                    //loan.FIRSTINTERESTPAYMENTDATE = reviewData.INTERESTFIRSTPAYMENTDATE;
                    //loan.FIRSTPRINCIPALPAYMENTDATE = reviewData.PRINCIPALFIRSTPAYMENTDATE;
                    //-------------------------------------------------
                    //-------------------------------------------------------
                }

                var result = context.SaveChanges() > 0;
                if (result)
                {
                    //trans.Commit();
                    output = true;
                }
                //output = false;
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                throw new SecureException(ex.Message);
                output = false;

            }

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
            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.LOANID == data.TERMLOANID && x.DATE == applicationDate).ACCRUEDINTEREST;
            //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == refNo && x.DATE == applicationDate).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.LOANID == data.TERMLOANID && x.PAYMENTDATE > applicationDate).PAYMENTDATE;
            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == refNo && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
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
                                   relationshipManagerId = l.RELATIONSHIPMANAGERID,
                                   relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                                   productTypeId = l.TBL_PRODUCT.PRODUCTTYPEID,
                                   systemCurrentDate = applicationDate
                               }).FirstOrDefault();

            return runningLoan;
        }

        public LoanViewModel GetRunningFXLoans(int companyId, string refNo)
        {
            var applicationDate = generalSetup.GetApplicationDate();
            var data = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == refNo && x.COMPANYID == companyId);
            DateTime maturityDate = data.MATURITYDATE;
            DateTime effectiveDate = data.EFFECTIVEDATE;
            decimal outStandingBalance = data.OUTSTANDINGPRINCIPAL;
            TimeSpan difference = maturityDate - applicationDate;
            int days = (int)difference.TotalDays;
            //decimal accruedInterest =  context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.LOANID == data.TERMLOANID && x.DATE == applicationDate).ACCRUEDINTEREST;
            //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == refNo && x.DATE == applicationDate).ACCRUEDINTEREST;
            //accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.LOANID == data.TERMLOANID && x.PAYMENTDATE > applicationDate).PAYMENTDATE;
            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == refNo && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
            // outStandingBalance = decimal.Round(outStandingBalance, 2, MidpointRounding.AwayFromZero);

            //decimal pastDue = decimal.Round((data.PASTDUEINTEREST + data.INTERESTONPASTDUEINTEREST + data.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            //decimal totalamount = (accruedInterest + outStandingBalance + pastDue);


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
                                   //teno = (int)(l.MATURITYDATE - l.EFFECTIVEDATE).Days,
                                   newtenor = 0,
                                   // accrualedAmount = accruedInterest,
                                   // totalAmount = totalamount,
                                   //firstPrincipalPaymentDate = nextPaymentDate,
                                   // firstInterestPaymentDate = nextPaymentDate,
                                   principalFrequencyTypeId = l.PRINCIPALFREQUENCYTYPEID,
                                   interestFrequencyTypeId = l.INTERESTFREQUENCYTYPEID,
                                   //  pastDueTotal = pastDue,
                                   relationshipManagerId = l.RELATIONSHIPMANAGERID,
                                   relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                                   productTypeId = l.TBL_PRODUCT.PRODUCTTYPEID,
                                   systemCurrentDate = applicationDate
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
                    && data.ISDISABLED == false
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

        public bool DoesChargeFeeExist(int loanId, int operationTypeId, int chargeFeeId)
        {
            var data = from a in context.TBL_LOAN_REVIEW_OPERATION
                       where a.LOANID == loanId && a.OPERATIONTYPEID == operationTypeId
                      && a.INTERESTFREQUENCYTYPEID == chargeFeeId && a.OPERATIONCOMPLETED == false
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
                SCHEDULETYPEID = model.scheduleTypeId,
                SCHEDULEDAYINTERESTTYPEID = model.interestTypeId,
                SCHEDULEDAYCOUNTCONVENTIONID = model.scheduleDayCountId,
                OPERATIONCOMPLETED = false,
                CREATEDBY = model.createdBy,
                DATECREATED = DateTime.Now,
                TBL_LOAN_REVIEW_OPRATN_IREG_SC = irregularSchedules
            };
            // Audit Section ---------------------------


            var operationPerformed = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANREVIEWAPPLICATIONID == model.lmsApplicationDetailId).FirstOrDefault();
            if(operationPerformed!= null)
            {
                operationPerformed.OPERATIONPERFORMED = true;
            }

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
                    int status = 0;
                    if ((int)OperationsEnum.Prepayment != model.operationTypeId)
                    {
                        status = (int)ApprovalStatusEnum.Pending;
                    }
                    else
                    {
                        status = (int)ApprovalStatusEnum.Pending;
                    }
                    workFlow.StaffId = model.createdBy;
                    workFlow.CompanyId = model.companyId;
                    workFlow.StatusId = status;//(int)ApprovalStatusEnum.Pending;
                    workFlow.TargetId = model.loanId;
                    workFlow.Comment = "Initiation";
                    workFlow.OperationId = model.operationTypeId;
                    workFlow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    workFlow.ExternalInitialization = true;
                    //if ((int)OperationsEnum.Prepayment != model.operationTypeId)
                    //{
                    var response = workFlow.LogActivity();
                    //}

                    try
                    {
                        output = context.SaveChanges() > 0;
                    }
                    catch (DbEntityValidationException ex)
                    {

                        string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }

                    trans.Commit();

                    //if ((int)OperationsEnum.Prepayment == model.operationTypeId)
                    //{
                    //    int loanReviewOperationsId = this.context.TBL_LOAN_REVIEW_OPERATION.FirstOrDefault(x => x.LOANID == model.loanId).LOANREVIEWOPERATIONID;
                    //    LoanRephasementProcess((short)loanReviewOperationsId, model.loanId, model.createdBy);
                    //}

                    return output;


                }

                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }



        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId)
        {
            //var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.ContractualInterestRateChange);

            // var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId);

            // var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ChecklistOperation).ToList();

            var levelResult = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFID == staffId).FirstOrDefault();
            int staffApprovalLevelId = 0;
            if (levelResult != null) staffApprovalLevelId = levelResult.APPROVALLEVELID;

            var dataLoan = (from ln in context.TBL_LOAN
                            join op in context.TBL_LOAN_REVIEW_OPERATION on ln.TERMLOANID equals op.LOANID
                            join tt in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals tt.OPERATIONID
                            join atrail in context.TBL_APPROVAL_TRAIL on op.LOANID equals atrail.TARGETID
                            join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                            join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                            join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                            join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                            join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                            join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                            join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                            join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                            join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                            where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                            //|| atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                            && atrail.OPERATIONID == op.OPERATIONTYPEID
                            && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                            && atrail.RESPONSESTAFFID == null && op.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                            && op.OPERATIONCOMPLETED == false
                            orderby op.LOANID descending
                            select new LoanReviewOperationApprovalViewModel
                            {
                                loanId = ln.TERMLOANID,
                                loanReviewOperationsId = op.LOANREVIEWOPERATIONID,
                                customerId = ln.CUSTOMERID,
                                productId = ln.PRODUCTID,
                                casaAccountId = ln.CASAACCOUNTID,
                                branchId = ln.BRANCHID,
                                loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
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
                                customerGroupId = lp.CUSTOMERGROUPID,
                                operationId = ln.OPERATIONID,
                                loanTypeId = lp.LOANAPPLICATIONTYPEID,
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
                                customerCode = cu.CUSTOMERCODE,
                                productAccountNumber = ch.ACCOUNTCODE,
                                productAccountName = ch.ACCOUNTNAME,
                                loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                currencyId = ln.CURRENCYID,
                                branchName = br.BRANCHNAME,
                                relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                productName = pr.PRODUCTNAME,
                                comment = "",
                                operationTypeId = op.OPERATIONTYPEID,
                                operationTypeName = context.TBL_OPERATIONS.FirstOrDefault(d => d.OPERATIONID == op.OPERATIONTYPEID).OPERATIONNAME,
                                newEffectiveDate = op.EFFECTIVEDATE,
                                reviewDetails = op.REVIEWDETAILS,
                                approvedAmount = ld.APPROVEDAMOUNT,
                                creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var dataRevolvingLoan = (from ln in context.TBL_LOAN_REVOLVING
                                     join op in context.TBL_LOAN_REVIEW_OPERATION on ln.REVOLVINGLOANID equals op.LOANID
                                     join tt in context.TBL_OPERATIONS on op.OPERATIONTYPEID equals tt.OPERATIONID
                                     join atrail in context.TBL_APPROVAL_TRAIL on op.LOANID equals atrail.TARGETID
                                     join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                     join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                     join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                     join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                     join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     //join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                     where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     //|| atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                     && atrail.OPERATIONID == op.OPERATIONTYPEID
                                     && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                                     && atrail.RESPONSESTAFFID == null && op.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                     && op.OPERATIONCOMPLETED == false
                                     orderby op.LOANID descending
                                     select new LoanReviewOperationApprovalViewModel
                                     {
                                         loanId = ln.REVOLVINGLOANID,
                                         loanReviewOperationsId = op.LOANREVIEWOPERATIONID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                         relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                         misCode = ln.MISCODE,
                                         teamMiscode = ln.TEAMMISCODE,
                                         interestRate = ln.INTERESTRATE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         approvalStatusId = op.APPROVALSTATUSID,
                                         approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == op.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                         approvedBy = (int)ln.APPROVEDBY,
                                         approverComment = ln.APPROVERCOMMENT,
                                         dateApproved = ln.DATEAPPROVED,
                                         loanStatusId = ln.LOANSTATUSID,
                                         isDisbursed = ln.ISDISBURSED,
                                         disburserComment = ln.DISBURSERCOMMENT,
                                         disburseDate = ln.DISBURSEDATE,
                                         customerGroupId = lp.CUSTOMERGROUPID,
                                         operationId = ln.OPERATIONID,
                                         loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         dischargeLetter = ln.DISCHARGELETTER,
                                         suspendInterest = ln.SUSPENDINTEREST,
                                         customerCode = cu.CUSTOMERCODE,
                                         //productAccountNumber = ch.ACCOUNTCODE,
                                         //productAccountName = ch.ACCOUNTNAME,
                                         loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         currencyId = ln.CURRENCYID,
                                         branchName = br.BRANCHNAME,
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         comment = "",
                                         operationTypeId = op.OPERATIONTYPEID,
                                         operationTypeName = tt.OPERATIONNAME, //context.TBL_OPERATIONS.FirstOrDefault(d => d.OPERATIONID == op.OPERATIONTYPEID).OPERATIONNAME,
                                         newEffectiveDate = op.EFFECTIVEDATE,
                                         reviewDetails = op.REVIEWDETAILS,
                                     }).ToList();

            var termLoanData = dataLoan.GroupBy(x => x.loanReviewOperationsId).Select(y => y.FirstOrDefault());
            var revolvingLoanData = dataRevolvingLoan.GroupBy(x => x.loanReviewOperationsId).Select(y => y.FirstOrDefault());
            var data = termLoanData.Union(revolvingLoanData);
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
            //var termLoanData = dataLoan.GroupBy(x => x.loanReviewOperationsId).Select(y => y.FirstOrDefault());
            //var revolvingLoanData = dataRevolving.GroupBy(x => x.loanReviewOperationsId).Select(y => y.FirstOrDefault());
            //var data = termLoanData.Union(revolvingLoanData);
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

        public int GoForApproval(ApprovalViewModel entity)
        {
            var twoFactorAuth = new TwoFactorAutheticationViewModel
            {
                passcode = entity.passCode,
                username = entity.userName
            };
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.StaffId = entity.staffId;
                    workFlow.CompanyId = entity.companyId;
                    workFlow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workFlow.TargetId = entity.targetId;
                    workFlow.Comment = entity.comment;
                    workFlow.OperationId = entity.operationId;
                    //workFlow.DeferredExecution = false;

                    workFlow.LogActivity();


                    if (workFlow.Saved)
                    {
                        bool output = false;
                        bool result = false;
                        int data = 0;
                        var reviewRecord = (from s in context.TBL_LOAN_REVIEW_OPERATION
                                            where s.LOANID == entity.targetId && s.OPERATIONTYPEID == entity.operationId
                                           && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                            && s.OPERATIONCOMPLETED == false
                                            select s).FirstOrDefault();
                        if (workFlow.NewState != (int)ApprovalState.Ended)
                        {
                            reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                            output = context.SaveChanges() > 0;
                            //if (output == true)
                            //{
                            trans.Commit();
                            data = 1;

                            //}
                        }
                        else if (workFlow.NewState == (int)ApprovalState.Ended)
                        {
                            result = LoanRephasementProcess(twoFactorAuth, (short)reviewRecord.LOANREVIEWOPERATIONID, reviewRecord.LOANID, entity.staffId);
                            if (result == true)
                            {
                                reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                                output = context.SaveChanges() > 0;
                            }
                            if (output == true && result == true)
                            {
                                trans.Commit();
                                data = 2;
                            }

                        }
                        return data;
                    }

                }
                catch (ConditionNotMetException ce)
                {
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (BadLogicException be)
                {
                    throw new BadLogicException(be.Message);
                }
                catch (APIErrorException e)
                {
                    throw new APIErrorException(e.Message);
                }
                catch (TwoFactorAuthenticationException e)
                {
                    throw new TwoFactorAuthenticationException(e.Message);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }

            return 0;

        }

        private int ApproveLoanReview(int loanId, ApprovalViewModel user)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    bool output = false;
                    bool result = false;
                    int data = 0;
                    var reviewRecord = (from s in context.TBL_LOAN_REVIEW_OPERATION
                                        where s.LOANID == loanId && s.OPERATIONTYPEID == user.operationId
                                       && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                        && s.OPERATIONCOMPLETED == false
                                        select s).FirstOrDefault();
                    if (workFlow.NewState != (int)ApprovalState.Ended)
                    {
                        reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                        output = context.SaveChanges() > 0;
                        //if (output == true)
                        //{
                        trans.Commit();
                        data = 1;

                        //}
                    }
                    else if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        //  result = LoanRephasementProcess((short)reviewRecord.LOANREVIEWOPERATIONID, reviewRecord.LOANID, user.staffId);
                        //if (result == true)
                        //{
                        //    reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        //    output = context.SaveChanges() > 0;
                        //}


                        if (output == true && result == true)
                        {
                            trans.Commit();
                            data = 2;
                        }

                    }
                    return data;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }


        }

        //private int ApproveLoanReview(int loanId, ApprovalViewModel user)
        //{
        //    using (var trans = context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            bool output = false;
        //            bool result = false;
        //            int data = 0;
        //            var reviewRecord = (from s in context.TBL_LOAN_REVIEW_OPERATION
        //                                where s.LOANID == loanId && s.OPERATIONTYPEID == user.operationId
        //                               && s.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
        //                                && s.OPERATIONCOMPLETED == false
        //                                select s).FirstOrDefault();
        //            if (workFlow.NewState != (int)ApprovalState.Ended)
        //            {
        //                reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
        //                output = context.SaveChanges() > 0;
        //                //if(output == true)
        //                //{
        //                    trans.Commit();
        //                    data = 1;

        //                //}
        //            }
        //            else if (workFlow.NewState == (int)ApprovalState.Ended)
        //            {
        //                 result = LoanRephasementProcess((short)reviewRecord.LOANREVIEWOPERATIONID, reviewRecord.LOANID, user.staffId);
        //                if (result == true)
        //                {
        //                    reviewRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
        //                    output = context.SaveChanges() > 0;
        //                }


        //                if (output == true && result == true)
        //                {
        //                    trans.Commit();
        //                    data = 2;
        //                }

        //            }
        //            return data;
        //        }
        //        catch (Exception ex)
        //        {
        //            trans.Rollback();
        //            throw new SecureException(ex.Message);
        //        }
        //    }

        //}

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanRephasementProcess(TwoFactorAutheticationViewModel twoFactorAuth, short loanReviewOperationsId, int loanId, int staffId)
        {
            try
            {
                bool output = false;
                bool result = false;
                //var systemDate = generalSetup.GetApplicationDate();

                var checkForOverDraft = this.context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == loanId);
                if (checkForOverDraft != null)
                {
                    var model = (
                                 from a in context.TBL_LOAN_REVIEW_OPERATION
                                 join b in context.TBL_LOAN_REVOLVING on a.LOANID equals b.REVOLVINGLOANID
                                 where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false
                                 && a.LOANREVIEWOPERATIONID == loanReviewOperationsId

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
                                     productId = b.PRODUCTID,

                                 }).FirstOrDefault();

                    //foreach (var item in model)
                    //{
                    string appDate = model.newEffectiveDate.ToString(@"yyyy-MM-dd");
                    var applicationDate = Convert.ToDateTime(appDate);
                    if ((int)OperationsEnum.OverdraftTopup == model.operationId)
                    {

                        result = OverdraftTopUp(twoFactorAuth, loanId, (decimal)model.newAmount);
                        if (result == true)
                        {
                            output = true;
                        }
                        else
                        {
                            output = false;
                        }
                    }
                    else if ((int)OperationsEnum.OverdraftRenewal == model.operationId)
                    {
                        result = OverdraftRenewal(twoFactorAuth, loanId, (decimal)model.newAmount);
                        if (result == true)
                        {
                            output = true;
                        }
                        else
                        {
                            output = false;
                        }
                    }
                    else if ((int)OperationsEnum.OverdraftTenorExtension == model.operationId)
                    {
                        result = OverdraftExtension(twoFactorAuth, loanId, (decimal)model.newAmount);
                        if (result == true)
                        {
                            output = true;
                        }
                        else
                        {
                            output = false;
                        }
                    }

                    else if ((int)OperationsEnum.OverdraftSubAllocation == model.operationId)
                    {
                        result = SubAllocation(twoFactorAuth, loanId, (decimal)model.newAmount, applicationDate, staffId);
                        if (result == true)
                        {
                            output = true;
                        }
                        else
                        {
                            output = false;
                        }
                    }
                    //}
                }
                else
                {
                    var scheduleMethod = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId).SCHEDULETYPEID;
                    var operationType = this.context.TBL_LOAN_REVIEW_OPERATION.FirstOrDefault(x => x.LOANREVIEWOPERATIONID == loanReviewOperationsId).OPERATIONTYPEID;

                    if (scheduleMethod == (short)LoanScheduleTypeEnum.IrregularSchedule)
                    {
                        var model = (
                                 from a in context.TBL_LOAN_REVIEW_OPERATION
                                 join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                 //join c in context.tbl_Loan_Review_Operation_Irregular_Schedule on a.LoanReviewOperationId equals c.LoanReviewOperationId
                                 where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false
                                     && a.LOANREVIEWOPERATIONID == loanReviewOperationsId

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
                                     productId = b.PRODUCTID,
                                 }).FirstOrDefault();

                        //foreach (var model in model)
                        //{
                        List<IrregularLoanScheduleInputViewModel> irregularSchedule = new List<IrregularLoanScheduleInputViewModel>();
                        {
                            var scheduleInput = context.TBL_LOAN_REVIEW_OPRATN_IREG_SC.Where(x => x.LOANREVIEWOPERATIONID == loanReviewOperationsId);

                            foreach (var model2 in scheduleInput)
                            {
                                irregularSchedule.Add(new IrregularLoanScheduleInputViewModel { paymentAmount = (double)model2.PAYMENTAMOUNT, paymentDate = model2.PAYMENTDATE });
                            }

                            model.irregularPaymentSchedule = irregularSchedule;

                        }
                        var unEarnedFee = from d in context.TBL_LOAN_SCHEDULE_DAILY
                                          where d.LOANID == model.loanId
                                          let sumUnEarnedFee = context.TBL_LOAN_SCHEDULE_DAILY.Where(a => a.LOANID == model.loanId
                                          && a.DATE >= DbFunctions.TruncateTime(model.newEffectiveDate)).Sum(a => (double?)a.UNEARNEDFEE ?? 0)
                                          select sumUnEarnedFee;
                        model.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;
                        string appDate = model.newEffectiveDate.ToString(@"yyyy-MM-dd");
                        var applicationDate = Convert.ToDateTime(appDate);

                        DateTime nextPaymentDate = DateTime.Now;
                        var paymentDate = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                                       where a.TBL_LOAN.TERMLOANID == loanId && a.PAYMENTDATE >= applicationDate
                                       select a).FirstOrDefault();

                        if (paymentDate != null)
                        {
                            nextPaymentDate = paymentDate.PAYMENTDATE;
                        }
                        else
                        {
                            throw new SecureException("Application Date not found in Payment Schedule");
                        }

                        if ((int)OperationsEnum.ContractualInterestRateChange == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            model.interestRate = model.newInterest;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor;
                            model.principalFirstpaymentDate = nextPaymentDate;
                            model.interestFirstpaymentDate = nextPaymentDate;
                            result = InterestRateReview(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }


                        }
                        else if ((int)OperationsEnum.Prepayment == model.operationId)
                        {
                            //decimal accruedAmount = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                            decimal accruedAmount = 0;
                            var accrued = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                                           where a.TBL_LOAN.TERMLOANID == loanId && a.DATE >= applicationDate
                                           select a).FirstOrDefault();

                            if (accrued != null)
                            {
                                accruedAmount = accrued.ACCRUEDINTEREST;
                            }
                            else
                            {
                                throw new SecureException("Application Date not found in Payment Schedule");
                            }
                            decimal accruedInterest = decimal.Round(accruedAmount, 2, MidpointRounding.AwayFromZero);
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            if (model.isManagementInterestRate == true)
                            {

                                model.maturityDate = (DateTime)model.newMaturityDate;
                                //model.newAmount = ((model.principalAmount + (double)accruedInterest) - model.payAmount);
                                model.effectiveDate = model.newEffectiveDate;
                                model.tenor = model.newTenorPrepayment;
                                model.interestFirstpaymentDate = nextPaymentDate;
                                model.principalFirstpaymentDate = nextPaymentDate;
                            }
                            else
                            {
                                //model.newAmount = ((model.principalAmount + (double)accruedInterest) - model.payAmount);
                                model.effectiveDate = model.newEffectiveDate;
                                model.tenor = model.newTenorPrepayment;
                                model.effectiveDate = model.newEffectiveDate;
                                model.interestFirstpaymentDate = nextPaymentDate;
                                model.principalFirstpaymentDate = nextPaymentDate;
                            }

                            result = UpdateLoanPrepaymentSchedule(twoFactorAuth, loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.PaymentDateChange == model.operationId)
                        {
                            model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;
                            model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;

                            result = PaymentDateChange(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                updateLoanPrincipalInterestPaymentDate(model.interestFirstpaymentDate, model.principalFirstpaymentDate, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }


                        }
                        else if ((int)OperationsEnum.PrincipalFrequencyChange == model.operationId || (int)OperationsEnum.InterestFrequencyChange == model.operationId
                            || (int)OperationsEnum.InterestandPrincipalFrequencyChange == model.operationId)
                        {
                            if ((int)OperationsEnum.PrincipalFrequencyChange == model.operationId)
                            {
                                model.principalFrequency = (short)model.newPrincipalFrequency;
                                model.interestFrequency = (short)model.interestFrequency;
                            }
                            if ((int)OperationsEnum.InterestFrequencyChange == model.operationId)
                            {
                                model.interestFrequency = (short)model.newInterestFrequency;
                                model.principalFrequency = (short)model.principalFrequency;
                            }
                            if ((int)OperationsEnum.InterestandPrincipalFrequencyChange == model.operationId)
                            {
                                model.interestFrequency = (short)model.newInterestFrequency;
                                model.principalFrequency = (short)model.newPrincipalFrequency;
                            }
                            result = PaymentFrequencyChange(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                updateLoanFrequency((short)model.principalFrequency, (short)model.interestFrequency, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.CompleteWriteOff == model.operationId)
                        {
                            result = CompleteWriteOff(loanId, model, twoFactorAuth, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.CancelUndisbursedLoan == model.operationId)
                        {
                            result = LoanCancellation(loanId, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.Fee_chargeChange == model.operationId)
                        {
                            result = ProcessChargeReversal(twoFactorAuth, loanId, model.operationId, staffId);
                            if (result == true)
                            {
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.TenorChange == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor; ;
                            model.principalFirstpaymentDate = nextPaymentDate;
                            model.interestFirstpaymentDate = nextPaymentDate;
                            result = TenorExtension(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }

                        else if ((int)OperationsEnum.Restructured == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            model.interestRate = model.newInterest;
                            model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;//nextPaymentDate;
                            model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor;
                            model.interestFrequency = (short)model.newInterestFrequency;
                            model.principalFrequency = (short)model.newPrincipalFrequency;
                            result = Restructured(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.LoanSales == model.operationId)
                        {
                            result = LoanSales(loanId, model, twoFactorAuth, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.LoanWorkOut == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            model.interestRate = model.newInterest;
                            model.effectiveDate = model.newEffectiveDate;
                            model.scheduleMethodId = model.scheduleMethodId;
                            result = LoanWorkOut(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }

                        else if ((int)OperationsEnum.LoanRecovery == model.operationId)
                        {
                            //model.interestRate = model.newInterest;
                            //model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;//nextPaymentDate;
                            //model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            //model.maturityDate = (DateTime)model.newMaturityDate;
                            //model.effectiveDate = model.newEffectiveDate;
                            //model.tenor = model.newTenor;
                            //model.interestFrequency = (short)model.newInterestFrequency;
                            //model.principalFrequency = (short)model.newPrincipalFrequency;

                            model.interestRate = model.newInterest;
                            model.effectiveDate = model.newEffectiveDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.scheduleMethodId = (short)LoanScheduleTypeEnum.IrregularSchedule;
                            result = LoanRecovery(loanId, model,twoFactorAuth, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        //}
                    }
                    else
                    {
                        var model = (
                                 from a in context.TBL_LOAN_REVIEW_OPERATION
                                 join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                 where b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId && a.OPERATIONCOMPLETED == false
                                 && a.LOANREVIEWOPERATIONID == loanReviewOperationsId

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
                                     productId = b.PRODUCTID,
                                     oldCasaAccountId = b.CASAACCOUNTID,
                                     newCasaAccountId = a.CASA_ACCOUNTID,


                                 }).FirstOrDefault();

                        //foreach (var model in model)
                        //{
                        var unEarnedFee = from d in context.TBL_LOAN_SCHEDULE_DAILY
                                          where d.LOANID == model.loanId
                                          let sumUnEarnedFee = context.TBL_LOAN_SCHEDULE_DAILY.Where(a => a.LOANID == model.loanId
                                          && a.DATE >= DbFunctions.TruncateTime(model.newEffectiveDate)).Sum(a => (double?)a.UNEARNEDFEE ?? 0)
                                          select sumUnEarnedFee;
                        model.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;
                        string appDate = model.newEffectiveDate.ToString(@"yyyy-MM-dd");
                        var applicationDate = Convert.ToDateTime(appDate);

                        DateTime nextPaymentDate = DateTime.Now;
                        var paymentDate = (from a in context.TBL_LOAN_SCHEDULE_PERIODIC
                                           where a.TBL_LOAN.TERMLOANID == loanId && a.PAYMENTDATE >= applicationDate
                                           select a).FirstOrDefault();

                        if (paymentDate != null)
                        {
                            nextPaymentDate = paymentDate.PAYMENTDATE;
                        }
                        else
                        {
                            throw new SecureException("Application Date not found in Payment Schedule");
                        }
                        if ((int)OperationsEnum.ContractualInterestRateChange == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            model.interestRate = model.newInterest;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor;
                            model.principalFirstpaymentDate = nextPaymentDate;
                            model.interestFirstpaymentDate = nextPaymentDate;
                            if (model.interestFirstpaymentDate <= model.effectiveDate)
                            {
                                throw new ConditionNotMetException("First Payment Date must be greater than Effective Date");
                            }
                            result = InterestRateReview(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }


                        }
                        else if ((int)OperationsEnum.Prepayment == model.operationId)
                        {
                            //decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.DATE == applicationDate).ACCRUEDINTEREST;
                            // DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE > applicationDate).PAYMENTDATE;
                            if (model.isManagementInterestRate == true)
                            {
                                model.maturityDate = (DateTime)model.newMaturityDate;
                                //model.newAmount = ((model.principalAmount + (double)accruedInterest) - model.payAmount);
                                model.effectiveDate = model.newEffectiveDate;
                                model.tenor = model.newTenorPrepayment;
                                model.interestFirstpaymentDate = nextPaymentDate;
                                model.principalFirstpaymentDate = nextPaymentDate;
                            }
                            else
                            {
                                model.maturityDate = (DateTime)model.newMaturityDate;
                                //model.newAmount = ((model.principalAmount + (double)accruedInterest) - model.payAmount);
                                model.effectiveDate = model.newEffectiveDate;
                                model.tenor = model.newTenorPrepayment;
                                model.interestFirstpaymentDate = nextPaymentDate;
                                model.principalFirstpaymentDate = nextPaymentDate;
                            }

                            result = UpdateLoanPrepaymentSchedule(twoFactorAuth, loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.PaymentDateChange == model.operationId)
                        {
                            model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;
                            model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;

                            result = PaymentDateChange(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                updateLoanPrincipalInterestPaymentDate(model.interestFirstpaymentDate, model.principalFirstpaymentDate, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }


                        }
                        else if ((int)OperationsEnum.PrincipalFrequencyChange == model.operationId || (int)OperationsEnum.InterestFrequencyChange == model.operationId
                            || (int)OperationsEnum.InterestandPrincipalFrequencyChange == model.operationId)
                        {
                            if ((int)OperationsEnum.PrincipalFrequencyChange == model.operationId)
                            {
                                model.principalFrequency = (short)model.newPrincipalFrequency;
                                model.interestFrequency = (short)model.interestFrequency;
                            }
                            if ((int)OperationsEnum.InterestFrequencyChange == model.operationId)
                            {
                                model.interestFrequency = (short)model.newInterestFrequency;
                                model.principalFrequency = (short)model.principalFrequency;
                            }
                            if ((int)OperationsEnum.InterestandPrincipalFrequencyChange == model.operationId)
                            {
                                model.interestFrequency = (short)model.newInterestFrequency;
                                model.principalFrequency = (short)model.newPrincipalFrequency;
                            }
                            result = PaymentFrequencyChange(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                updateLoanFrequency((short)model.principalFrequency, (short)model.interestFrequency, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.CompleteWriteOff == model.operationId)
                        {
                            model.interestRate = model.newInterest;
                            model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;//nextPaymentDate;
                            model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor;
                            model.interestFrequency = (short)model.newInterestFrequency;
                            model.principalFrequency = (short)model.newPrincipalFrequency;
                            result = CompleteWriteOff(loanId, model, twoFactorAuth, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.TerminateAndRebook == model.operationId)
                        {
                            //LoanCancellation(loanId, applicationDate, staffId);
                            TerminateAndRebookLoanSchedule(loanId, model, twoFactorAuth, applicationDate, staffId);
                            var loan = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                            var data = (
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

                                        }).FirstOrDefault();

                            //foreach (var model1 in data)
                            //{
                            result = RegenerateSchedule(data.loanId, data, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, data.loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                            //}

                        }
                        else if ((int)OperationsEnum.Fee_chargeChange == model.operationId)
                        {
                            result = ProcessChargeReversal(twoFactorAuth, loanId, model.operationId, staffId);
                            if (result == true)
                            {
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.TenorChange == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= applicationDate).PAYMENTDATE;
                            model.maturityDate = model.maturityDate;
                            model.effectiveDate = model.newEffectiveDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.tenor = model.newTenor; ;
                            model.principalFirstpaymentDate = nextPaymentDate;
                            model.interestFirstpaymentDate = nextPaymentDate;
                            result = TenorExtension(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }

                        else if ((int)OperationsEnum.LoanSales == model.operationId)
                        {
                            result = LoanSales(loanId, model, twoFactorAuth, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }

                        else if ((int)OperationsEnum.Restructured == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            model.interestRate = model.newInterest;
                            model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;//nextPaymentDate;
                            model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor;
                            model.interestFrequency = (short)model.newInterestFrequency;
                            model.principalFrequency = (short)model.newPrincipalFrequency;
                            result = Restructured(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.LoanWorkOut == model.operationId)
                        {
                            //DateTime nextPaymentDate = context.TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.TBL_LOAN.TERMLOANID == model.loanId && x.PAYMENTDATE >= systemDate).PAYMENTDATE;
                            model.interestRate = model.newInterest;
                            model.effectiveDate = model.newEffectiveDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.scheduleMethodId = model.scheduleMethodId;
                            result = LoanWorkOut(loanId, model, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.LoanRecovery == model.operationId)
                        {
                            model.interestRate = model.newInterest;
                            model.interestFirstpaymentDate = (DateTime)model.newInterestFirstpaymentDate;//nextPaymentDate;
                            model.principalFirstpaymentDate = (DateTime)model.newPrincipalFirstpaymentDate;//nextPaymentDate;
                            model.maturityDate = (DateTime)model.newMaturityDate;
                            model.effectiveDate = model.newEffectiveDate;
                            model.tenor = model.newTenor;
                            model.interestFrequency = (short)model.newInterestFrequency;
                            model.principalFrequency = (short)model.newPrincipalFrequency;
                            result = LoanRecovery(loanId, model, twoFactorAuth, applicationDate, staffId);
                            if (result == true)
                            {
                                updateLoanReviewOperation(loanReviewOperationsId, loanId);
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        else if ((int)OperationsEnum.CASAAccountChange == model.operationId)
                        {
                            result = ChangeOperativeAccount(model.oldCasaAccountId, (int)model.newCasaAccountId, loanId);
                            if (result == true)
                            {
                                output = true;
                            }
                            else
                            {
                                output = false;
                            }

                        }
                        //}
                    }

                }


                context.SaveChanges();
                //-------------------------------------------------------
                //output = true;

                return output;
            }
            catch (ConditionNotMetException ce)
            {
                throw new ConditionNotMetException(ce.Message);
            }
            catch (BadLogicException be)
            {
                throw new BadLogicException(be.Message);
            }
            catch (APIErrorException e)
            {
                throw new APIErrorException(e.Message);
            }
            catch (Exception e)
            {
                throw new ConditionNotMetException(e.Message);
            }


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

                inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.INTERESTRECEIVABLEPAYABLEGL.Value, "interest repayment Debt as a result of Defer Document", (int)OperationsEnum.InterestLoanRepayment));

                inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PRINCIPALBALANCEGL.Value, "principal repayment Debt as a result of Defer Document", (int)OperationsEnum.PrincipalLoanRepayment));

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

        //#region Commercial Paper  Operation

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool CommercialPaperRateReview(int aplicationId, double newRate, DateTime applicationDate, int staffId, int operationId)
        //{
        //    bool output = false;

        //    TBL_LOAN_APPLICATION result = (from p in context.TBL_LOAN_APPLICATION
        //                                   where p.LOANAPPLICATIONID == aplicationId
        //                                   select p).SingleOrDefault();

        //    result.INTERESTRATE = newRate;



        //    context.SaveChanges();
        //    output = true;

        //    return output;
        //}

        //public bool CommercialPaperTenorReview(int aplicationId, int newTenor, DateTime applicationDate, int staffId, int operationId)
        //{
        //    bool output = false;

        //    TBL_LOAN_APPLICATION result = (from p in context.TBL_LOAN_APPLICATION
        //                                   where p.LOANAPPLICATIONID == aplicationId
        //                                   select p).SingleOrDefault();

        //    result.APPLICATIONTENOR = newTenor;



        //    context.SaveChanges();
        //    output = true;

        //    return output;
        //}

        //public bool CommercialPaperTenorReviewDetails(int loanAplicationDetailId, int newTenor, DateTime applicationDate, int staffId, int operationId)
        //{
        //    bool output = false;

        //    TBL_LOAN_APPLICATION_DETAIL result = (from p in context.TBL_LOAN_APPLICATION_DETAIL
        //                                          where p.LOANAPPLICATIONDETAILID == loanAplicationDetailId
        //                                          select p).SingleOrDefault();

        //    result.PROPOSEDTENOR = newTenor;
        //    result.APPROVEDTENOR = newTenor;



        //    context.SaveChanges();
        //    output = true;

        //    return output;
        //}

        //public void CommercialPaperChangeOperativeAccount(int casaPayAccountId, int newCasaPayAccountId)
        //{ //TODO rework
        //    TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
        //                                 where //p.CASAACCOUNTID2 == casaPayAccountId &&
        //                         p.LOANSTATUSID == (short)LoanStatusEnum.Active
        //                                 select p).SingleOrDefault();
        //    var casa = this.context.TBL_CASA.Where(x => x.CASAACCOUNTID == newCasaPayAccountId && x.ACCOUNTSTATUSID == (short)CASAAccountStatusEnum.Active).FirstOrDefault().CASAACCOUNTID;

        //    // result.CASAACCOUNTID2 = casa;
        //    context.SaveChanges();
        //}

        //public IEnumerable<LoanApplicationViewModel> ArchiveLoanApplication(int aplicationId)
        //{
        //    var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
        //    var model = (from a in context.TBL_LOAN_APPLICATION
        //                 where a.APPROVALSTATUSID == (short)LoanStatusEnum.Active && a.LOANAPPLICATIONID == aplicationId

        //                 select new LoanApplicationViewModel()
        //                 {
        //                     loanApplicationId = a.LOANAPPLICATIONID,
        //                     applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
        //                     loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
        //                     customerId = a.CUSTOMERID,
        //                     companyId = a.COMPANYID,
        //                     branchId = a.BRANCHID,
        //                     customerGroupId = a.CUSTOMERGROUPID,
        //                     loanTypeId = a.LOANAPPLICATIONTYPEID,
        //                     relationshipOfficerId = a.RELATIONSHIPOFFICERID,
        //                     relationshipManagerId = a.RELATIONSHIPMANAGERID,
        //                     casaAccountId = a.CASAACCOUNTID,
        //                     newApplicationDate = a.APPLICATIONDATE,
        //                     interestRate = a.INTERESTRATE,
        //                     applicationTenor = a.APPLICATIONTENOR,
        //                     effectiveDate = a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().EFFECTIVEDATE,
        //                     expiryDate = a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().EXPIRYDATE,
        //                     operationId = a.OPERATIONID,
        //                     productClassId = (short)a.PRODUCTCLASSID,
        //                     applicationAmount = a.APPLICATIONAMOUNT,
        //                     approvedAmount = a.APPROVEDAMOUNT,
        //                     loanInformation = a.LOANINFORMATION,
        //                     misCode = a.MISCODE,
        //                     teamMisCode = a.TEAMMISCODE,
        //                     isInvestmentGrade = a.ISINVESTMENTGRADE,
        //                     isRelatedParty = a.ISRELATEDPARTY,
        //                     isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
        //                     createdBy = a.CREATEDBY,
        //                     dateTimeCreated = a.DATETIMECREATED,
        //                     lastUpdatedBy = (int)a.LASTUPDATEDBY,
        //                     dateTimeUpdated = a.DATETIMEUPDATED,
        //                     deleted = a.DELETED,
        //                     deletedBy = a.DELETEDBY,
        //                     dateTimeDeleted = a.DATETIMEDELETED,
        //                     //approvalStatusId = a.APPROVALSTATUSID,
        //                     applicationStatusId = a.APPLICATIONSTATUSID,
        //                     submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
        //                     customerInfoValidated = a.CUSTOMERINFOVALIDATED,
        //                     //notInNegativeCrms = a.NOTINNEGATIVECRMS,
        //                     //notInBlackbook = a.NOTINBLACKBOOK,
        //                     //notInCamsol = a.NOTINCAMSOL,
        //                     //notInXds = a.NOTINXDS,
        //                     //notInCrc = a.NOTINCRC,

        //                 }).ToList();

        //    List<TBL_LOAN_APPLICATION_ARCHIVE> LoanApplicationArchive = new List<TBL_LOAN_APPLICATION_ARCHIVE>();

        //    foreach (var item in model)
        //    {

        //        TBL_LOAN_APPLICATION_ARCHIVE addLoanApplicationArchive = new TBL_LOAN_APPLICATION_ARCHIVE();

        //        addLoanApplicationArchive.ARCHIVEDATE = DateTime.Today;
        //        addLoanApplicationArchive.LOANAPPLICATIONID = item.loanApplicationId;
        //        addLoanApplicationArchive.APPLICATIONREFERENCENUMBER = item.applicationReferenceNumber;
        //        addLoanApplicationArchive.LOANPRELIMINARYEVALUATIONID = item.loanPreliminaryEvaluationId;
        //        addLoanApplicationArchive.CUSTOMERID = item.customerId;
        //        addLoanApplicationArchive.COMPANYID = item.companyId;
        //        addLoanApplicationArchive.BRANCHID = (short)item.branchId;
        //        addLoanApplicationArchive.CUSTOMERGROUPID = item.customerGroupId;
        //        addLoanApplicationArchive.LOANAPPLICATIONTYPEID = item.loanTypeId;
        //        addLoanApplicationArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
        //        addLoanApplicationArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
        //        addLoanApplicationArchive.CASAACCOUNTID = item.casaAccountId;
        //        addLoanApplicationArchive.APPLICATIONDATE = item.newApplicationDate;
        //        addLoanApplicationArchive.INTERESTRATE = item.interestRate;
        //        addLoanApplicationArchive.APPLICATIONTENOR = (int)item.applicationTenor;
        //        addLoanApplicationArchive.EFFECTIVEDATE = item.effectiveDate;
        //        addLoanApplicationArchive.EXPIRYDATE = item.expiryDate;
        //        addLoanApplicationArchive.OPERATIONID = (int)item.operationId;
        //        addLoanApplicationArchive.PRODUCTCLASSID = item.productClassId;
        //        addLoanApplicationArchive.APPLICATIONAMOUNT = item.applicationAmount;
        //        addLoanApplicationArchive.APPROVEDAMOUNT = item.approvedAmount;
        //        addLoanApplicationArchive.LOANINFORMATION = item.loanInformation;
        //        addLoanApplicationArchive.MISCODE = item.misCode;
        //        addLoanApplicationArchive.TEAMMISCODE = item.teamMisCode;
        //        addLoanApplicationArchive.ISINVESTMENTGRADE = item.isInvestmentGrade;
        //        addLoanApplicationArchive.ISRELATEDPARTY = item.isRelatedParty;
        //        addLoanApplicationArchive.ISPOLITICALLYEXPOSED = item.isPoliticallyExposed;
        //        addLoanApplicationArchive.CREATEDBY = item.createdBy;
        //        addLoanApplicationArchive.DATETIMECREATED = item.dateTimeCreated;
        //        addLoanApplicationArchive.LASTUPDATEDBY = item.lastUpdatedBy;
        //        addLoanApplicationArchive.DATETIMEUPDATED = item.dateTimeUpdated;
        //        addLoanApplicationArchive.DELETED = item.deleted;
        //        addLoanApplicationArchive.DELETEDBY = item.deletedBy;
        //        addLoanApplicationArchive.DATETIMEDELETED = item.dateTimeDeleted;
        //        addLoanApplicationArchive.APPROVALSTATUSID = item.approvalStatusId;
        //        addLoanApplicationArchive.APPLICATIONSTATUSID = item.applicationStatusId;
        //        addLoanApplicationArchive.SUBMITTEDFORAPPRAISAL = item.submittedForAppraisal;
        //        addLoanApplicationArchive.CUSTOMERINFOVALIDATED = item.customerInfoValidated;
        //        addLoanApplicationArchive.NOTINNEGATIVECRMS = item.notInNegativeCrms;
        //        addLoanApplicationArchive.NOTINBLACKBOOK = item.notInNegativeCrms;
        //        addLoanApplicationArchive.NOTINBLACKBOOK = item.notInBlackbook;
        //        addLoanApplicationArchive.NOTINCAMSOL = item.notInCamsol;
        //        addLoanApplicationArchive.NOTINXDS = item.notInXds;
        //        addLoanApplicationArchive.NOTINCRC = item.notInCrc;
        //        addLoanApplicationArchive.OPERATIONID = (int)item.operationId;
        //        addLoanApplicationArchive.CUSTOMERGROUPID = item.customerGroupId;
        //        addLoanApplicationArchive.LOANAPPLICATIONTYPEID = item.loanTypeId;

        //        LoanApplicationArchive.Add(addLoanApplicationArchive);

        //    }

        //    this.context.TBL_LOAN_APPLICATION_ARCHIVE.AddRange(LoanApplicationArchive);

        //    context.SaveChanges();
        //    return model;
        //}

        //public IEnumerable<LoanApplicationDetailViewModel> ArchiveLoanApplicationDetails(int aplicationId)
        //{
        //    var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
        //    var model = (from a in context.TBL_LOAN_APPLICATION_DETAIL
        //                 where a.LOANAPPLICATIONID == aplicationId

        //                 select new LoanApplicationDetailViewModel()
        //                 {
        //                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
        //                     loanApplicationId = a.LOANAPPLICATIONID,
        //                     customerId = a.CUSTOMERID,
        //                     proposedProductId = a.PROPOSEDPRODUCTID,
        //                     proposedTenor = a.PROPOSEDTENOR,
        //                     proposedInterestRate = a.PROPOSEDINTERESTRATE,
        //                     proposedAmount = a.PROPOSEDAMOUNT,
        //                     approvedProductId = a.APPROVEDPRODUCTID,
        //                     approvedTenor = a.APPROVEDTENOR,
        //                     approvedInterestRate = a.APPROVEDINTERESTRATE,
        //                     approvedAmount = a.APPROVEDAMOUNT,
        //                     currencyId = a.CURRENCYID,
        //                     exchangeRate = a.EXCHANGERATE,
        //                     subSectorId = a.SUBSECTORID,
        //                     statusId = a.STATUSID,
        //                     loanPurpose = a.LOANPURPOSE,
        //                     createdBy = a.CREATEDBY,
        //                     dateTimeCreated = a.DATETIMECREATED,
        //                     lastUpdatedBy = (int)a.LASTUPDATEDBY,
        //                     dateTimeUpdated = a.DATETIMEUPDATED,
        //                     deleted = a.DELETED,
        //                     deletedBy = a.DELETEDBY,
        //                     dateTimeDeleted = a.DATETIMEDELETED,

        //                 }).ToList();

        //    List<TBL_LOAN_APPLICATION_DETL_ARCH> LoanApplicationDetailsArchive = new List<TBL_LOAN_APPLICATION_DETL_ARCH>();

        //    foreach (var item in model)
        //    {
        //        TBL_LOAN_APPLICATION_DETL_ARCH addLoanApplDetailsArchive = new TBL_LOAN_APPLICATION_DETL_ARCH();


        //        addLoanApplDetailsArchive.ARCHIVEDATE = DateTime.Today;
        //        addLoanApplDetailsArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
        //        addLoanApplDetailsArchive.LOANAPPLICATIONID = item.loanApplicationId;
        //        addLoanApplDetailsArchive.CUSTOMERID = item.customerId;
        //        addLoanApplDetailsArchive.PROPOSEDPRODUCTID = item.proposedProductId;
        //        addLoanApplDetailsArchive.PROPOSEDTENOR = item.proposedTenor;
        //        addLoanApplDetailsArchive.PROPOSEDINTERESTRATE = item.proposedInterestRate;
        //        addLoanApplDetailsArchive.PROPOSEDAMOUNT = item.proposedAmount;
        //        addLoanApplDetailsArchive.APPROVEDPRODUCTID = item.approvedProductId;
        //        addLoanApplDetailsArchive.APPROVEDTENOR = item.approvedTenor;
        //        addLoanApplDetailsArchive.APPROVEDINTERESTRATE = item.approvedInterestRate;
        //        addLoanApplDetailsArchive.APPROVEDAMOUNT = item.approvedAmount;
        //        addLoanApplDetailsArchive.CURRENCYID = item.currencyId;
        //        addLoanApplDetailsArchive.EXCHANGERATE = item.exchangeRate;
        //        addLoanApplDetailsArchive.SUBSECTORID = item.subSectorId;
        //        addLoanApplDetailsArchive.STATUSID = item.statusId;
        //        addLoanApplDetailsArchive.LOANPURPOSE = item.loanPurpose;
        //        addLoanApplDetailsArchive.CREATEDBY = item.createdBy;
        //        addLoanApplDetailsArchive.DATETIMECREATED = item.dateTimeCreated;
        //        addLoanApplDetailsArchive.LASTUPDATEDBY = item.lastUpdatedBy;
        //        addLoanApplDetailsArchive.DATETIMEUPDATED = item.dateTimeUpdated;
        //        addLoanApplDetailsArchive.DELETED = item.deleted;
        //        addLoanApplDetailsArchive.DELETEDBY = item.deletedBy;
        //        addLoanApplDetailsArchive.DATETIMEDELETED = item.dateTimeDeleted;

        //        LoanApplicationDetailsArchive.Add(addLoanApplDetailsArchive);

        //    }

        //    this.context.TBL_LOAN_APPLICATION_DETL_ARCH.AddRange(LoanApplicationDetailsArchive);

        //    context.SaveChanges();
        //    return model;
        //}

        //public IEnumerable<RevolvingLoanViewModel> ArchiveRevolvingLoan(int aplicationId)
        //{
        //    var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
        //    var model = (from a in context.TBL_LOAN_REVOLVING
        //                 where a.APPROVALSTATUSID == (short)LoanStatusEnum.Active && a.LOANAPPLICATIONDETAILID == aplicationId

        //                 select new RevolvingLoanViewModel()
        //                 {
        //                     loanId = a.REVOLVINGLOANID,
        //                     customerId = a.CUSTOMERID,
        //                     productId = a.PRODUCTID,
        //                     companyId = a.COMPANYID,
        //                     branchId = a.BRANCHID,
        //                     customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
        //                     loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
        //                     relationshipOfficerId = a.RELATIONSHIPOFFICERID,
        //                     relationshipManagerId = a.RELATIONSHIPMANAGERID,
        //                     casaAccountId = a.CASAACCOUNTID,
        //                     //casaAccountId2 = a.CASAACCOUNTID2,
        //                     currencyId = a.CURRENCYID,
        //                     loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
        //                     exchangeRate = a.EXCHANGERATE,
        //                     loanReferenceNumber = a.LOANREFERENCENUMBER,
        //                     relatedLoanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
        //                     subSectorId = a.SUBSECTORID,
        //                     maturityDate = a.MATURITYDATE,
        //                     bookingDate = a.BOOKINGDATE,
        //                     interestRate = a.INTERESTRATE,
        //                     effectiveDate = a.EFFECTIVEDATE,
        //                     operationId = a.OPERATIONID,
        //                     misCode = a.MISCODE,
        //                     teamMisCode = a.TEAMMISCODE,
        //                     overdraftLimit = a.OVERDRAFTLIMIT,
        //                     //disbursedAmount = a.DISBURSED_AMOUNT,
        //                     //interestAmount = a.INTEREST_AMOUNT,
        //                     approverComment = a.APPROVERCOMMENT,
        //                     approvedBy = (int)a.APPROVEDBY,
        //                     dateApproved = a.DATEAPPROVED,
        //                     loanStatusId = a.LOANSTATUSID,
        //                     isDisbursed = a.ISDISBURSED,
        //                     disbursedBy = a.DISBURSEDBY,
        //                     disburserComment = a.DISBURSERCOMMENT,
        //                     disburseDate = a.DISBURSEDATE,
        //                     // trancheBatchCode = a.TRANCHEBATCHCODE,
        //                     dischargeLetter = a.DISCHARGELETTER,
        //                     suspendInterest = a.SUSPENDINTEREST,
        //                     internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
        //                     externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
        //                     nplDate = a.NPLDATE,
        //                     createdBy = a.CREATEDBY,
        //                     dateTimeCreated = a.DATETIMECREATED,
        //                     approvalStatusId = a.APPROVALSTATUSID,


        //                 }).ToList();

        //    List<TBL_LOAN_REVOLVING_ARCHIVE> LoanRevolvingArchive = new List<TBL_LOAN_REVOLVING_ARCHIVE>();



        //    foreach (var item in model)
        //    {

        //        TBL_LOAN_REVOLVING_ARCHIVE addLoanRevolvingArchive = new TBL_LOAN_REVOLVING_ARCHIVE();
        //        addLoanRevolvingArchive.ARCHIVEDATE = DateTime.Today;
        //        addLoanRevolvingArchive.CUSTOMERID = item.customerId;
        //        addLoanRevolvingArchive.PRODUCTID = item.productId;
        //        addLoanRevolvingArchive.COMPANYID = item.companyId;
        //        addLoanRevolvingArchive.BRANCHID = item.branchId;
        //        addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID = item.customerGroupId;
        //        addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID = item.loanTypeId;
        //        addLoanRevolvingArchive.RELATIONSHIPOFFICERID = item.relationshipOfficerId;
        //        addLoanRevolvingArchive.RELATIONSHIPMANAGERID = item.relationshipManagerId;
        //        addLoanRevolvingArchive.CASAACCOUNTID = item.casaAccountId;
        //        // addLoanRevolvingArchive.CASAACCOUNTID2 = item.casaAccountId2;
        //        addLoanRevolvingArchive.CURRENCYID = (short)item.currencyId;
        //        addLoanRevolvingArchive.LOANAPPLICATIONDETAILID = item.loanApplicationDetailId;
        //        addLoanRevolvingArchive.EXCHANGERATE = item.exchangeRate;
        //        addLoanRevolvingArchive.LOANREFERENCENUMBER = item.loanReferenceNumber;
        //        addLoanRevolvingArchive.RELATED_LOAN_REFERENCE_NUMBER = item.relatedLoanReferenceNumber;
        //        addLoanRevolvingArchive.SUBSECTORID = item.subSectorId;
        //        addLoanRevolvingArchive.MATURITYDATE = item.maturityDate;
        //        addLoanRevolvingArchive.BOOKINGDATE = item.bookingDate;
        //        addLoanRevolvingArchive.INTERESTRATE = item.interestRate;
        //        addLoanRevolvingArchive.EFFECTIVEDATE = item.effectiveDate;
        //        addLoanRevolvingArchive.OPERATIONID = item.operationId;
        //        addLoanRevolvingArchive.MISCODE = item.misCode;
        //        addLoanRevolvingArchive.TEAMMISCODE = item.teamMiscode;
        //        addLoanRevolvingArchive.OVERDRAFTLIMIT = item.overdraftLimit;
        //        //addLoanRevolvingArchive.DISBURSED_AMOUNT = item.disbursedAmount;
        //        //addLoanRevolvingArchive.INTEREST_AMOUNT = item.interestAmount;
        //        addLoanRevolvingArchive.APPROVALSTATUSID = item.approvalStatusId;
        //        addLoanRevolvingArchive.APPROVEDBY = item.approvedBy;
        //        addLoanRevolvingArchive.APPROVERCOMMENT = item.approverComment;
        //        addLoanRevolvingArchive.DATEAPPROVED = item.dateApproved;
        //        addLoanRevolvingArchive.LOANSTATUSID = item.loanStatusId;
        //        addLoanRevolvingArchive.CREATEDBY = item.createdBy;
        //        addLoanRevolvingArchive.DATETIMECREATED = item.dateTimeCreated;
        //        addLoanRevolvingArchive.ISDISBURSED = item.isDisbursed;
        //        addLoanRevolvingArchive.DISBURSEDBY = item.disbursedBy;
        //        addLoanRevolvingArchive.DISBURSERCOMMENT = item.disburserComment;
        //        addLoanRevolvingArchive.DISBURSEDATE = item.disburseDate;
        //        addLoanRevolvingArchive.OPERATIONID = (int)item.operationId;
        //        addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID = item.customerGroupId;
        //        addLoanRevolvingArchive.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID = item.loanTypeId;
        //        // addLoanRevolvingArchive.TRANCHEBATCHCODE = item.trancheBatchCode;
        //        addLoanRevolvingArchive.DISCHARGELETTER = item.dischargeLetter;
        //        addLoanRevolvingArchive.SUSPENDINTEREST = item.suspendInterest;
        //        addLoanRevolvingArchive.INT_PRUDENT_GUIDELINE_STATUSID = 1; //item.internalPrudentialGuidelineStatusId;
        //        addLoanRevolvingArchive.EXT_PRUDENT_GUIDELINE_STATUSID = 1; //item.externalPrudentialGuidelineStatusId;
        //        addLoanRevolvingArchive.NPLDATE = item.nplDate;
        //        addLoanRevolvingArchive.CREATEDBY = item.createdBy;
        //        addLoanRevolvingArchive.DATETIMECREATED = item.dateTimeCreated;

        //        LoanRevolvingArchive.Add(addLoanRevolvingArchive);

        //    }

        //    this.context.TBL_LOAN_REVOLVING_ARCHIVE.AddRange(LoanRevolvingArchive);

        //    context.SaveChanges();
        //    return model;
        //}

        //public IEnumerable<LoanReviewOperationApprovalViewModel> GetRunningCommercialLoans(int companyId, string loanReferenceNumber)
        //{
        //    var data = (from ln in context.TBL_LOAN
        //                where ln.MATURITYDATE > DateTime.Now
        //                && ln.LOANSTATUSID == (short)LoanStatusEnum.Active
        //                && ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
        //                && ln.COMPANYID == companyId
        //                orderby ln.MATURITYDATE descending
        //                select new LoanReviewOperationApprovalViewModel
        //                {
        //                    loanId = ln.TERMLOANID,
        //                    customerId = ln.CUSTOMERID,
        //                    productId = ln.PRODUCTID,
        //                    casaAccountId = ln.CASAACCOUNTID,
        //                    branchId = ln.BRANCHID,
        //                    loanReferenceNumber = ln.LOANREFERENCENUMBER,
        //                    loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
        //                    companyId = ln.COMPANYID,
        //                    exchangeRate = ln.EXCHANGERATE,
        //                    principalAmount = ln.PRINCIPALAMOUNT,
        //                    interestRate = ln.INTERESTRATE,
        //                    outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
        //                    outstandingInterest = ln.OUTSTANDINGINTEREST,
        //                    maturityAmount = ln.OUTSTANDINGPRINCIPAL + ln.OUTSTANDINGINTEREST,
        //                    dateTimeCreated = ln.DATETIMECREATED,
        //                    createdByName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    dischargeLetter = ln.DISCHARGELETTER,

        //                    relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
        //                    relationshipManagerId = ln.RELATIONSHIPMANAGERID,
        //                    misCode = ln.MISCODE,
        //                    teamMiscode = ln.TEAMMISCODE,
        //                    effectiveDate = ln.EFFECTIVEDATE,
        //                    maturityDate = ln.MATURITYDATE,
        //                    bookingDate = ln.BOOKINGDATE,

        //                    approverComment = ln.APPROVERCOMMENT,
        //                    dateApproved = ln.DATEAPPROVED,

        //                    isDisbursed = ln.ISDISBURSED,
        //                    disburserComment = ln.DISBURSERCOMMENT,
        //                    disburseDate = ln.DISBURSEDATE,
        //                    newTenor = DbFunctions.DiffDays(ln.MATURITYDATE, ln.EFFECTIVEDATE).Value,
        //                    customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
        //                    operationId = ln.OPERATIONID,
        //                    loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
        //                    subSectorId = ln.SUBSECTORID,
        //                    subSectorName = ln.TBL_SUB_SECTOR.NAME,
        //                    sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

        //                    customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
        //                    productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
        //                    productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
        //                    loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                    customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
        //                    currencyId = ln.CURRENCYID,
        //                    currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
        //                    branchName = ln.TBL_BRANCH.BRANCHNAME,
        //                    relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    productName = ln.TBL_PRODUCT.PRODUCTNAME,
        //                    loanStatusId = ln.LOANSTATUSID,
        //                    comment = "",
        //                }).ToList();
        //    return data;
        //}

        ////public IEnumerable<MaturityIntructionTypeViewModel> GetMaturityInstructionType()
        ////{
        ////    var data = from a in context.TBL_LOAN_MATURITY_INSTRU_TYPE
        ////               select new MaturityIntructionTypeViewModel
        ////               {
        ////                  instructionTypeId = a.INSTRUCTIONTYPEID ,
        ////                    instructionTypeName = a.INSTRUCTIONTYPENAME,
        ////               };

        ////    return data.ToList();
        ////}

        //public List<LoanReviewOperationParentChildViewModel> GetMaturedCommercialLoansParent(int companyId)
        //{
        //    var data = from a in context.TBL_LOAN_APPLICATION_DETAIL
        //               join ln in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals ln.LOANAPPLICATIONDETAILID
        //               where ln.MATURITYDATE < DateTime.Now
        //               && ln.LOANSTATUSID == (short)LoanStatusEnum.Active || ln.LOANSTATUSID == (short)LoanStatusEnum.Completed
        //               && ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
        //               && ln.COMPANYID == companyId
        //               orderby ln.MATURITYDATE descending
        //               select new LoanReviewOperationParentChildViewModel
        //               {
        //                   applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
        //                   loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
        //                   firstName = a.TBL_CUSTOMER.FIRSTNAME,
        //                   middleName = a.TBL_CUSTOMER.MIDDLENAME,
        //                   lastName = a.TBL_CUSTOMER.LASTNAME,
        //                   approvedAmount = a.APPROVEDAMOUNT,
        //                   approvedInterestRate = a.APPROVEDINTERESTRATE,
        //                   approvedProductName = a.TBL_PRODUCT.PRODUCTNAME,
        //                   numberofTranchesBooked = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == ln.LOANAPPLICATIONDETAILID && x.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking).Count(),
        //                   numberofrunningTranches = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == ln.LOANAPPLICATIONDETAILID && x.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking && x.LOANSTATUSID == (short)LoanStatusEnum.Active).Count(),
        //                   // runningTranches = GetMaturedCommercialLoans(companyId,a.LOANAPPLICATIONDETAILID).ToList()
        //                   //effectiveDate = ln.TBL_LOAN_APPLICATION_DETAIL.EFFECTIVEDATE ?? null,
        //                   // maturityDate = ln.EFFECTIVEDATE.Value.AddDays((int)ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR),
        //               };

        //    return data.ToList();
        //}

        //public List<LoanReviewOperationApprovalViewModel> GetMaturedCommercialLoans(int companyId, int loanApplicationDetailID)
        //{
        //    var data = (from ln in context.TBL_LOAN
        //                where ln.LOANAPPLICATIONDETAILID == loanApplicationDetailID
        //                ////&& ln.MATURITYDATE < DateTime.Now
        //                ////&& ln.LOANSTATUSID == (short)LoanStatusEnum.Active || ln.LOANSTATUSID == (short)LoanStatusEnum.Completed
        //                ////&& ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
        //                ////&& ln.COMPANYID == companyId
        //                orderby ln.MATURITYDATE descending
        //                select new LoanReviewOperationApprovalViewModel
        //                {
        //                    loanId = ln.TERMLOANID,
        //                    customerId = ln.CUSTOMERID,
        //                    productId = ln.PRODUCTID,
        //                    casaAccountId = ln.CASAACCOUNTID,
        //                    branchId = ln.BRANCHID,
        //                    loanReferenceNumber = ln.LOANREFERENCENUMBER,
        //                    relatedReferenceNumber = ln.RELATED_LOAN_REFERENCE_NUMBER,
        //                    loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
        //                    companyId = ln.COMPANYID,
        //                    exchangeRate = ln.EXCHANGERATE,

        //                    principalAmount = ln.PRINCIPALAMOUNT,
        //                    interestRate = ln.INTERESTRATE,
        //                    interestAmount = ln.OUTSTANDINGINTEREST,
        //                    outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
        //                    outstandingInterest = ln.OUTSTANDINGINTEREST,
        //                    maturityAmount = ln.OUTSTANDINGPRINCIPAL + ln.OUTSTANDINGINTEREST,
        //                    dateTimeCreated = ln.DATETIMECREATED,
        //                    createdByName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    dischargeLetter = ln.DISCHARGELETTER,

        //                    relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
        //                    relationshipManagerId = ln.RELATIONSHIPMANAGERID,
        //                    misCode = ln.MISCODE,
        //                    teamMiscode = ln.TEAMMISCODE,
        //                    effectiveDate = ln.EFFECTIVEDATE,
        //                    maturityDate = ln.MATURITYDATE,
        //                    bookingDate = ln.BOOKINGDATE,

        //                    approverComment = ln.APPROVERCOMMENT,
        //                    dateApproved = ln.DATEAPPROVED,

        //                    isDisbursed = ln.ISDISBURSED,
        //                    disburserComment = ln.DISBURSERCOMMENT,
        //                    disburseDate = ln.DISBURSEDATE,
        //                    customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
        //                    operationId = ln.OPERATIONID,
        //                    loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
        //                    subSectorId = ln.SUBSECTORID,
        //                    subSectorName = ln.TBL_SUB_SECTOR.NAME,
        //                    sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

        //                    customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
        //                    productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
        //                    productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
        //                    loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                    customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
        //                    currencyId = ln.CURRENCYID,
        //                    currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
        //                    branchName = ln.TBL_BRANCH.BRANCHNAME,
        //                    relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    productName = ln.TBL_PRODUCT.PRODUCTNAME,
        //                    loanStatusId = ln.LOANSTATUSID,
        //                    comment = "",
        //                });
        //    return data.ToList();
        //}

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool CommercialPaperCancellation(int aplicationId, DateTime applicationDate, int staffId)
        //{
        //    bool output = false;
        //    var systemDate = generalSetup.GetApplicationDate();

        //    TBL_LOAN_APPLICATION result = (from p in context.TBL_LOAN_APPLICATION
        //                                   where p.LOANAPPLICATIONID == aplicationId
        //                                   select p).SingleOrDefault();

        //    result.APPLICATIONSTATUSID = (short)LoanStatusEnum.Cancelled;

        //    context.SaveChanges();

        //    output = true;

        //    return output;

        //}

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool CommercialPaperDetailsCancellation(string refNo, DateTime applicationDate, int staffId)
        //{
        //    bool output = false;
        //    var systemDate = generalSetup.GetApplicationDate();

        //    TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
        //                                 where p.LOANREFERENCENUMBER == refNo
        //                                 select p).SingleOrDefault();

        //    result.LOANSTATUSID = (short)LoanStatusEnum.Cancelled;

        //    context.SaveChanges();

        //    output = true;

        //    return output;

        //}

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool CommercialPaperPrepayment(string refNo, decimal prepaymentAmount, DateTime applicationDate, int staffId)
        //{
        //    bool output = false;
        //    var systemDate = generalSetup.GetApplicationDate();



        //    TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
        //                                 where p.LOANREFERENCENUMBER == refNo
        //                                 select p).SingleOrDefault();


        //    if (result.OVERDRAFTLIMIT == prepaymentAmount)
        //    {
        //        result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT - prepaymentAmount;
        //        result.LOANSTATUSID = (short)LoanStatusEnum.Completed;
        //    }
        //    else
        //    {
        //        result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT - prepaymentAmount;
        //    }


        //    context.SaveChanges();

        //    output = true;

        //    return output;

        //}

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public bool CommercialPaperRollOver(string refNo, decimal prepaymentAmount, DateTime applicationDate, int staffId)
        //{
        //    bool output = false;
        //    var systemDate = generalSetup.GetApplicationDate();

        //    TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
        //                                 where p.LOANREFERENCENUMBER == refNo
        //                                 select p).SingleOrDefault();

        //    if (result.OVERDRAFTLIMIT == prepaymentAmount)
        //    {
        //        result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT;
        //        result.LOANSTATUSID = (short)LoanStatusEnum.Active;
        //    }
        //    else
        //    {
        //        result.OVERDRAFTLIMIT = result.OVERDRAFTLIMIT + prepaymentAmount;
        //        result.LOANSTATUSID = (short)LoanStatusEnum.Active;
        //    }

        //    context.SaveChanges();

        //    output = true;

        //    return output;

        //}

        //public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyCommercialPaperInterestAccrual(DateTime applicationDate)

        //{
        //    var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


        //    var data = (from a in context.TBL_LOAN_REVOLVING
        //                where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.PRODUCTID == (short)ProductClassEnum.Commercial


        //                select new DailyInterestAccrualViewModel()
        //                {
        //                    referenceNumber = a.LOANREFERENCENUMBER,
        //                    productId = a.PRODUCTID,
        //                    branchId = a.BRANCHID,
        //                    companyId = a.COMPANYID,
        //                    currencyId = a.CURRENCYID,
        //                    exchangeRate = a.EXCHANGERATE,
        //                    interestRate = a.INTERESTRATE,
        //                    date = applicationDate,
        //                    dailyAccuralAmount = a.INTERESTRATE,
        //                    mainAmount = a.OVERDRAFTLIMIT,
        //                    categoryId = (short)DailyAccrualCategory.AuthorisedOverdraft,
        //                    //availableBalance = b.AVAILABLEBALANCE,
        //                    transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
        //                    //baseReferenceNumber = null,
        //                    //dayCountConventionId = c.DAYCOUNTCONVENTIONID,
        //                    //daysInAYear = c.DAYSINAYEAR,

        //                });

        //    List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


        //    foreach (var item in data)
        //    {
        //        TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

        //        dailyAccrual.REFERENCENUMBER = item.referenceNumber;
        //        dailyAccrual.PRODUCTID = item.productId;
        //        dailyAccrual.BRANCHID = item.branchId;
        //        dailyAccrual.EXCHANGERATE = item.exchangeRate;
        //        dailyAccrual.CURRENCYID = item.currencyId;
        //        dailyAccrual.INTERESTRATE = item.interestRate;
        //        dailyAccrual.DATE = item.date;
        //        dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
        //        dailyAccrual.MAINAMOUNT = item.mainAmount;
        //        dailyAccrual.CATEGORYID = item.categoryId;
        //        dailyAccrual.COMPANYID = item.companyId;
        //        dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
        //        dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
        //        dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


        //        transAccrual.Add(dailyAccrual);
        //    }
        //    this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

        //    context.SaveChanges();

        //    var model = (from a in context.TBL_DAILY_ACCRUAL
        //                 where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.AuthorisedOverdraft
        //                 group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
        //                 select new DailyInterestAccrualViewModel()
        //                 {
        //                     productId = groupedQ.Key.PRODUCTID,
        //                     branchId = groupedQ.Key.BRANCHID,
        //                     companyId = groupedQ.Key.COMPANYID,
        //                     currencyId = groupedQ.Key.CURRENCYID,
        //                     exchangeRate = groupedQ.Key.EXCHANGERATE,
        //                     dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
        //                 });

        //    foreach (var item in model)
        //    {
        //        financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
        //    }
        //    return data;
        //}



        //public IEnumerable<LoanReviewOperationApprovalViewModel> GetBandGReadyForRenewal()
        //{

        //    var data = (from ln in context.TBL_LOAN_CONTINGENT
        //                where ln.ISTENORED == true && DbFunctions.DiffDays(ln.EFFECTIVEDATE, DateTime.Now) > 365
        //                orderby ln.EFFECTIVEDATE descending
        //                select new LoanReviewOperationApprovalViewModel
        //                {
        //                    loanId = ln.CONTINGENTLOANID,
        //                    customerId = ln.CUSTOMERID,
        //                    productId = ln.PRODUCTID,
        //                    casaAccountId = ln.CASAACCOUNTID,
        //                    branchId = ln.BRANCHID,
        //                    loanReferenceNumber = ln.LOANREFERENCENUMBER,
        //                    loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
        //                    isBankFormat = ln.ISBANKFORMAT,
        //                    companyId = ln.COMPANYID,
        //                    exchangeRate = ln.EXCHANGERATE,
        //                    approvedAmount = ln.CONTINGENTAMOUNT,
        //                    dateTimeCreated = ln.DATETIMECREATED,
        //                    createdByName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    dischargeLetter = ln.DISCHARGELETTER,

        //                    relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
        //                    relationshipManagerId = ln.RELATIONSHIPMANAGERID,
        //                    misCode = ln.MISCODE,
        //                    teamMiscode = ln.TEAMMISCODE,
        //                    effectiveDate = ln.EFFECTIVEDATE,
        //                    maturityDate = ln.MATURITYDATE,
        //                    bookingDate = ln.BOOKINGDATE,

        //                    approverComment = ln.APPROVERCOMMENT,
        //                    dateApproved = ln.DATEAPPROVED,

        //                    isDisbursed = ln.ISDISBURSED,
        //                    disburserComment = ln.DISBURSERCOMMENT,
        //                    disburseDate = ln.DISBURSEDATE,

        //                    customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
        //                    operationId = ln.OPERATIONID,
        //                    loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
        //                    subSectorId = ln.SUBSECTORID,
        //                    subSectorName = ln.TBL_SUB_SECTOR.NAME,
        //                    sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

        //                    customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
        //                    productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
        //                    productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
        //                    loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                    customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
        //                    currencyId = ln.CURRENCYID,
        //                    currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
        //                    branchName = ln.TBL_BRANCH.BRANCHNAME,
        //                    relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    relationshipManagerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
        //                    productName = ln.TBL_PRODUCT.PRODUCTNAME,
        //                    loanStatusId = ln.LOANSTATUSID,
        //                    comment = "",
        //                }).ToList();
        //    return data;
        //}
        //#endregion

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AutomaticInterestRepricing(DateTime applicationDate)
        {
            bool output = false;
            //var systemDate = generalSetup.GetApplicationDate();


            //var dailyRepricing = (from d in context.TBL_PRODUCT_PRICE_INDEX_DAILY
            //                      where d.PRICEDATE >= DbFunctions.TruncateTime(applicationDate.AddDays(-90)) && d.PRICEDATE <= DbFunctions.TruncateTime(applicationDate)
            //                      select new
            //                      {
            //                          d.PRICEINDEXRATE
            //                      }).ToList();
            //var sumdailyRepricing = dailyRepricing.Select(c => c.PRICEINDEXRATE).Sum();


            ///// update the price index rate
            ////context.SaveChanges();

            //output = true;

            return output;

        }

        public IEnumerable<LoanFeeOperationViewModel> GetLoanChargeFeeByLoanId(int loanId)
        {
            var chargeFee = (from a in context.TBL_LOAN_FEE
                             where a.LOANID == loanId
                             select new LoanFeeOperationViewModel()
                             {
                                 loanId = a.LOANID,
                                 loanFeeId = a.LOANCHARGEFEEID,
                                 chargeFeeId = a.CHARGEFEEID,
                                 chargeFeeName = a.TBL_CHARGE_FEE.CHARGEFEENAME,
                                 feeAmount = a.FEEAMOUNT,
                                 feeRateValue = a.FEERATEVALUE,
                                 feeEarnedAmount = a.EARNEDFEEAMOUNT,
                                 feeUnearnedAmount = (a.FEEAMOUNT - a.EARNEDFEEAMOUNT)
                             }).ToList();
            return chargeFee;
        }

        public bool WriteBulkPostingToStaging(DateTime applicationDate, string TransactionType, string batchCode)
        {
            bool output = false;
            //using (var trans = context.Database.BeginTransaction())
            //{
            //    try
            //    {
            var data = (from a in context.TBL_CUSTOM_TRANSACTION_BULK
                        where a.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && a.BATCHID == batchCode
                        select new FinanceTransactionStagingViewModel()
                        {
                            batchId = a.BATCHID,
                            batchRefId = a.BATCHREFID,
                            transType = a.TRANSACTIONTYPE,
                            flowType = a.FLOWTYPE,
                            amount = a.AMOUNT,
                            debitGlAccount = a.DEBITACCOUNT,
                            creditGlAccount = a.CREDITACCOUNT,
                            currencyCode = a.CURRENCYCODE,
                            currencyRate = a.CURRENCYRATE,
                            currencyRateCode = a.CURRENCYRATECODE,
                            description = a.DESCRIPTION,
                            amountCollected = 0,
                            bankId = a.BANKID,
                            branchId = (short)a.DESTINATIONBRANCHID,
                            sourceReferenceNumber = a.SOURCEREFERENCENUMBER,

                            //sid = a.SID,


                        }).ToList();

            List<FINTRAK_TRAN_PROC_DETAILS> staging = new List<FINTRAK_TRAN_PROC_DETAILS>();


            foreach (var item in data)
            {
                FINTRAK_TRAN_PROC_DETAILS addStaging = new FINTRAK_TRAN_PROC_DETAILS();

                addStaging.BATCH_ID = item.batchId;
                addStaging.BATCH_REF_ID = item.batchRefId;
                addStaging.TRAN_TYPE = item.transType;
                addStaging.FLOW_TYPE = item.flowType;
                addStaging.AMT = item.amount;
                addStaging.CR_ACCT = item.creditGlAccount;
                addStaging.DR_ACCT = item.debitGlAccount;
                addStaging.RATE_CODE = item.currencyRateCode;
                addStaging.REF_CRNCY_CODE = item.currencyCode;
                addStaging.RATE = (decimal)item.currencyRate;
                addStaging.NARRATION = item.description;
                addStaging.AMT_COLLECTED = item.amountCollected;
                addStaging.BANK_ID = item.bankId;
                addStaging.TOD_FLG = "N";
                addStaging.LOAN_ACCT = item.sourceReferenceNumber;
                addStaging.STATUS = "NEW";
                addStaging.RCRE_DATE = applicationDate;
                addStaging.PSTD_FLG = "N";
                addStaging.PSTD_DATE = applicationDate;
                addStaging.DEL_FLG = "N";
                addStaging.FAIL_FLG = "N";

                staging.Add(addStaging);

            }
            this.stagingContext.FINTRAK_TRAN_PROC_DETAILS.AddRange(staging);
            stagingContext.SaveChanges();

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.WriteToStagingTable,
            //    STAFFID = (int)SystemStaff.System,//model.createdBy,
            //    BRANCHID = data.FirstOrDefault().branchId,
            //    DETAIL = $"Write to Staging: {data.FirstOrDefault().sourceReferenceNumber}",
            //    IPADDRESS = data.FirstOrDefault().userIPAddress,
            //    URL = data.FirstOrDefault().applicationUrl,
            //    APPLICATIONDATE = applicationDate,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);


            var model = (from a in context.TBL_CUSTOM_TRANSACTION_BULK
                         where a.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && a.BATCHID == batchCode
                         group a by new { a.BATCHID } into groupedQ
                         select new FinanceTransactionStagingViewModel()
                         {
                             batchId = groupedQ.Key.BATCHID,
                             amount = groupedQ.Sum(i => i.AMOUNT),
                         }).ToList();

            List<FINTRAK_TRAN_PROC_MAIN> main = new List<FINTRAK_TRAN_PROC_MAIN>();
            var recordCount = this.context.TBL_CUSTOM_TRANSACTION_BULK.Where(x => x.VALUEDATE == DbFunctions.TruncateTime(applicationDate) && x.BATCHID == batchCode).Count();
            foreach (var item in model)
            {
                FINTRAK_TRAN_PROC_MAIN addMain = new FINTRAK_TRAN_PROC_MAIN();

                addMain.BATCH_ID = item.batchId;
                addMain.RCRE_DATE = applicationDate;
                addMain.TRAN_TYPE = TransactionType;
                addMain.RCRE_USER = "SYSTEM";
                addMain.TOTAL_AMT = item.amount;
                addMain.STATUS = "NEW";
                addMain.REC_COUNT = recordCount;
                addMain.BANK_ID = "01";
                addMain.IS_SELECTED = "N";
                addMain.PSTD_DATE = applicationDate;
                addMain.PSTD_FLG = "N";
                addMain.DEL_FLG = "N";

                //addMain.SID = 1;


                main.Add(addMain);

            }
            this.stagingContext.FINTRAK_TRAN_PROC_MAIN.AddRange(main);

            var result = stagingContext.SaveChanges() > 0;
            if (result)
            {
                //trans.Commit();
                output = true;
            }
            //output = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        trans.Rollback();
            //        output = false;

            //    }
            //} 
            return output;
        }

        #region Commercial Paper  Operation
        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperSubAllocation(List<subAllocationViewModel> models)
        {
            foreach (var model in models)
            {
                var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);
                var loanRecord = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == model.loanReferenceNumber);
                if (loanRecord.Any())
                {
                    var loan = loanRecord.FirstOrDefault();
                    ArchiveLoan(loan.TERMLOANID, (short)OperationsEnum.CommercialPaperLoanBooking, archiveBatchCode);
                    loan.PRINCIPALAMOUNT = model.newPrincipalAmount;
                    loan.OUTSTANDINGPRINCIPAL = model.newPrincipalAmount;
                    loan.LASTRESTRUCTUREDATE = generalSetup.GetApplicationDate();

                }
            }
            return context.SaveChanges() > 0;
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

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperRateReview(InterestReviewViewModel userModel)
        {
            var result = (from p in context.TBL_LOAN_APPLICATION_DETAIL
                          where p.LOANAPPLICATIONDETAILID == userModel.aplicationDetailId
                          select p).SingleOrDefault();


            ArchiveLoanApplicationDetails(result.LOANAPPLICATIONDETAILID);
            result.APPROVEDINTERESTRATE = userModel.newRate;

            var loans = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == result.LOANAPPLICATIONDETAILID);
            foreach (var loan in loans)
            {
                loan.INTERESTRATE = userModel.newRate;

                //Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanInterestRateChange,
                    STAFFID = userModel.createdBy,
                    BRANCHID = (short)userModel.userBranchId,
                    DETAIL = $"Interest rate changed on loan with reference number: {loan.LOANREFERENCENUMBER} to new rate {userModel.newRate}",
                    IPADDRESS = userModel.userIPAddress,
                    URL = userModel.applicationUrl,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                context.TBL_AUDIT.Add(audit);
                //end of Audit section -------------------------------
            };

            return context.SaveChanges() > 0;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CommercialPaperTenorReview(TenorExtionViewModel userModel)
        {
            var refNo = string.Empty;
            var auditDetail = string.Empty;
            userModel.isParent = false;
            var archiveBatchCode = CommonHelpers.GenerateRandomDigitCode(7);

            TBL_LOAN loan = new TBL_LOAN();
            TBL_LOAN_APPLICATION_DETAIL loanApp = new TBL_LOAN_APPLICATION_DETAIL();

            if (userModel.newTenor == 0)
                throw new BadLogicException("You cannot extend tenor with a zero value");

            if (userModel.loanRef != null)
            {
                loan = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == userModel.loanRef).FirstOrDefault();
                userModel.id = loan.LOANAPPLICATIONDETAILID;
                userModel.isParent = false;

                if (loan != null && loan.MATURITYDATE < loan.MATURITYDATE.AddDays(userModel.newTenor))
                {
                    ArchiveLoan(loan.TERMLOANID, (int)loan.OPERATIONID, archiveBatchCode);
                    loan.MATURITYDATE = loan.MATURITYDATE.AddDays(userModel.newTenor);
                }
                else throw new SecureException("New tenor has no positive value");

                auditDetail = $"Extended loan tenor with reference number: {loan.LOANREFERENCENUMBER} with {userModel.newTenor} extra";
            }
            else if (!userModel.isParent)
            {
                if (userModel.appRef == null)
                    throw new ConditionNotMetException("Line tenor extention require application detail");

                loanApp = (TBL_LOAN_APPLICATION_DETAIL)context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == userModel.id);
                userModel.id = loanApp.LOANAPPLICATIONDETAILID;
                userModel.isParent = true;

                auditDetail = $"Extended loan application detail tenor with reference number: {userModel.appRef} with {userModel.newTenor} extra";
            }

            CommercialPaperTenorReviewDetails(userModel.id, userModel.newTenor);
            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanTenorExtended,
                STAFFID = userModel.createdBy,
                BRANCHID = (short)userModel.userBranchId,
                DETAIL = auditDetail,
                IPADDRESS = userModel.userIPAddress,
                URL = userModel.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_AUDIT.Add(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        public bool CommercialPaperTenorReviewDetails(int loanAplicationDetailId, int newTenor)
        {
            TBL_LOAN_APPLICATION_DETAIL result = (from p in context.TBL_LOAN_APPLICATION_DETAIL
                                                  where p.LOANAPPLICATIONDETAILID == loanAplicationDetailId
                                                  select p).SingleOrDefault();


            result.APPROVEDTENOR = result.APPROVEDTENOR + newTenor;
            if (result.EXPIRYDATE != null)
            {
                var expiryDate = (DateTime)result.EXPIRYDATE;
                result.EXPIRYDATE = expiryDate.AddDays(newTenor);

                //List<TBL_LOAN> loans = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == loanAplicationDetailId).ToList();
                //if(loans.Count > 0)
                //{
                //    foreach(var loan in loans)
                //    {
                //        var newMaturitydate = loan.MATURITYDATE;

                //        if(newMaturitydate.AddDays(newTenor) <= result.EXPIRYDATE)
                //        {
                //            loan.MATURITYDATE = newMaturitydate.AddDays(newTenor);
                //        }
                //    }
                //}
            }

            return context.SaveChanges() > 0;
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

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetRunningCommercialLoans(int companyId, string loanReferenceNumber)
        {
            var data = (from ln in context.TBL_LOAN
                        where ln.MATURITYDATE > DateTime.Now
                        && ln.LOANSTATUSID == (short)LoanStatusEnum.Active
                        && ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
                        && ln.COMPANYID == companyId
                        orderby ln.MATURITYDATE descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TERMLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
                            companyId = ln.COMPANYID,
                            exchangeRate = ln.EXCHANGERATE,
                            principalAmount = ln.PRINCIPALAMOUNT,
                            interestRate = ln.INTERESTRATE,
                            outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            outstandingInterest = ln.OUTSTANDINGINTEREST,
                            maturityAmount = ln.OUTSTANDINGPRINCIPAL + ln.OUTSTANDINGINTEREST,
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
                            newTenor = DbFunctions.DiffDays(ln.MATURITYDATE, ln.EFFECTIVEDATE).Value,
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

        public IEnumerable<MaturityIntructionViewModel> GetLoanMaturityInstructions()
        {
            var data = (from a in context.TBL_LOAN_MATURITY_INSTRUCTION
                        where a.ISUSED == false
                        select new MaturityIntructionViewModel
                        {
                            instructionTypeId = a.INSTRUCTIONTYPEID,
                            instructionTypeName = a.TBL_LOAN_MATURITY_INSTRU_TYPE.INSTRUCTIONTYPENAME,
                            newTenor = a.TENOR,
                            loanId = a.LOANID,
                            maturityInstructionId = a.MATURITYINSTRUCTIONID,
                            loanSystemTypeId = a.LOANSYSTEMTYPEID,
                            loanReferenceNumber = context.TBL_LOAN.Where(x => x.TERMLOANID == a.LOANID).FirstOrDefault().LOANREFERENCENUMBER,
                            oldTenor = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONDETAILID == context.TBL_LOAN.Where(n => n.LOANAPPLICATIONDETAILID == c.LOANAPPLICATIONDETAILID).FirstOrDefault().LOANAPPLICATIONDETAILID).FirstOrDefault().APPROVEDTENOR,

                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY,
                        }).ToList();
            foreach (var i in data)
            {
                var outstandingPrincipal = (decimal)context.TBL_LOAN.Where(x => x.TERMLOANID == i.loanId).FirstOrDefault().OUTSTANDINGPRINCIPAL;
                var outstandingInterest = (decimal)context.TBL_LOAN.Where(x => x.TERMLOANID == i.loanId).FirstOrDefault().OUTSTANDINGINTEREST;
                var interestRate = (double)context.TBL_LOAN.Where(x => x.TERMLOANID == i.loanId).FirstOrDefault().INTERESTRATE;
                var customer = (from b in context.TBL_CUSTOMER join k in context.TBL_LOAN on b.CUSTOMERID equals k.CUSTOMERID where k.TERMLOANID == i.loanId select b).FirstOrDefault();

                i.outstandingPrincipal = outstandingPrincipal;
                i.outstandingInterest = outstandingInterest;
                i.interestRate = interestRate;

                i.customerName = customer.FIRSTNAME + " " + customer.MIDDLENAME + " " + customer.LASTNAME;

            }
            var g = data.ToList();
            return data;
        }

        public bool addMaturityInstruction(MaturityIntructionViewModel model)
        {
            var systemDate = generalSetup.GetApplicationDate();

            if (context.TBL_LOAN_MATURITY_INSTRUCTION.Where(x => x.LOANID == model.loanId && x.ISUSED == false).Any())
                throw new BadImageFormatException("There is already an an active maturity instruction on this loan");

            TBL_LOAN_MATURITY_INSTRUCTION maturity = new TBL_LOAN_MATURITY_INSTRUCTION();

            maturity.LOANID = model.loanId;
            maturity.LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility;
            maturity.INSTRUCTIONTYPEID = model.instructionTypeId;
            maturity.TENOR = model.tenor;
            maturity.CREATEDBY = model.createdBy;
            maturity.DATETIMECREATED = DateTime.Now;
            maturity.ISUSED = false;
            this.context.TBL_LOAN_MATURITY_INSTRUCTION.Add(maturity);

            return context.SaveChanges() > 0;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool ProcessCommercialPaperManualRollOver(MaturityIntructionViewModel model, string refNo)
        {

            var systemDate = generalSetup.GetApplicationDate();

            TBL_LOAN result;
            decimal newPrincipal = 0;
            decimal newInterest = 0;

            if (model.loanReferenceNumber != null)
            {
                result = (from p in context.TBL_LOAN where p.LOANREFERENCENUMBER == refNo select p).SingleOrDefault();
            }
            else result = context.TBL_LOAN.Find(model.loanId);

            if (model.valueDate == null) model.valueDate = result.MATURITYDATE;

            if (model.valueDate < systemDate)
            {
                var pastDays = ((DateTime)model.valueDate - (DateTime)systemDate).TotalDays;
                int ctr = 0;
                while (ctr != pastDays)
                {
                    //TODO: Call daily accrual posting method 
                }
            }

            var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(result.CUSTOMERID, result.PRODUCTID, result.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);
            var newEffectivedate = result.MATURITYDATE;
            switch (model.instructionTypeId)
            {
                case (short)MaturityInstructionTypeEnum.RolloverPrincipal:
                    newPrincipal = result.PRINCIPALAMOUNT;
                    break;

                case (short)MaturityInstructionTypeEnum.RolloverInterstAndPrincipal:
                    newPrincipal = result.PRINCIPALAMOUNT;
                    newInterest = result.OUTSTANDINGINTEREST;
                    break;
            };

            result.LOANSTATUSID = (short)LoanStatusEnum.Completed;

            TBL_LOAN newCommercialLoanLineEntry = new TBL_LOAN();
            newCommercialLoanLineEntry.PRODUCTPRICEINDEXRATE = result.PRODUCTPRICEINDEXRATE;
            newCommercialLoanLineEntry.CUSTOMERRISKRATINGID = result.CUSTOMERRISKRATINGID;
            newCommercialLoanLineEntry.LOANSYSTEMTYPEID = model.loanSystemTypeId;
            newCommercialLoanLineEntry.CUSTOMERID = result.CUSTOMERID;
            newCommercialLoanLineEntry.PRODUCTID = result.PRODUCTID;
            newCommercialLoanLineEntry.COMPANYID = result.COMPANYID;
            newCommercialLoanLineEntry.LOANAPPLICATIONDETAILID = result.LOANAPPLICATIONDETAILID;
            newCommercialLoanLineEntry.CASAACCOUNTID = result.CASAACCOUNTID;
            newCommercialLoanLineEntry.CASAACCOUNTID2 = result.CASAACCOUNTID2;
            newCommercialLoanLineEntry.LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility;
            newCommercialLoanLineEntry.BRANCHID = result.BRANCHID;
            newCommercialLoanLineEntry.SUBSECTORID = result.SUBSECTORID;
            newCommercialLoanLineEntry.CURRENCYID = result.CURRENCYID;
            newCommercialLoanLineEntry.EXCHANGERATE = result.EXCHANGERATE;
            newCommercialLoanLineEntry.LOANREFERENCENUMBER = loanReferenceNumber;
            newCommercialLoanLineEntry.RELATED_LOAN_REFERENCE_NUMBER = result.LOANREFERENCENUMBER;
            newCommercialLoanLineEntry.PRINCIPALNUMBEROFINSTALLMENT = result.PRINCIPALNUMBEROFINSTALLMENT;
            newCommercialLoanLineEntry.INTERESTNUMBEROFINSTALLMENT = result.INTERESTINSTALLMENTLEFT;
            newCommercialLoanLineEntry.RELATIONSHIPOFFICERID = result.RELATIONSHIPOFFICERID;
            newCommercialLoanLineEntry.RELATIONSHIPMANAGERID = result.RELATIONSHIPMANAGERID;
            newCommercialLoanLineEntry.MISCODE = result.MISCODE;
            newCommercialLoanLineEntry.TEAMMISCODE = result.TEAMMISCODE;
            newCommercialLoanLineEntry.INTERESTRATE = result.INTERESTRATE;
            newCommercialLoanLineEntry.EFFECTIVEDATE = newEffectivedate;
            newCommercialLoanLineEntry.MATURITYDATE = newEffectivedate.AddDays(model.tenor);
            newCommercialLoanLineEntry.BOOKINGDATE = result.BOOKINGDATE;
            newCommercialLoanLineEntry.PRINCIPALAMOUNT = newPrincipal;
            newCommercialLoanLineEntry.PRINCIPALINSTALLMENTLEFT = 0;
            newCommercialLoanLineEntry.INTERESTINSTALLMENTLEFT = 0;
            newCommercialLoanLineEntry.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
            newCommercialLoanLineEntry.APPROVERCOMMENT = null;
            newCommercialLoanLineEntry.LOANSTATUSID = (short)LoanStatusEnum.Active;
            newCommercialLoanLineEntry.SCHEDULETYPEID = result.SCHEDULETYPEID;
            newCommercialLoanLineEntry.SCHEDULEDAYCOUNTCONVENTIONID = result.SCHEDULEDAYCOUNTCONVENTIONID;
            newCommercialLoanLineEntry.SCHEDULEDAYINTERESTTYPEID = result.SCHEDULEDAYINTERESTTYPEID;
            newCommercialLoanLineEntry.SHOULD_DISBURSE = true;
            newCommercialLoanLineEntry.ISDISBURSED = true;
            newCommercialLoanLineEntry.EQUITYCONTRIBUTION = result.EQUITYCONTRIBUTION;
            newCommercialLoanLineEntry.OUTSTANDINGPRINCIPAL = newPrincipal;
            newCommercialLoanLineEntry.OUTSTANDINGINTEREST = newInterest;
            newCommercialLoanLineEntry.PASTDUEPRINCIPAL = 0;
            newCommercialLoanLineEntry.PASTDUEINTEREST = 0;
            newCommercialLoanLineEntry.INTERESTONPASTDUEPRINCIPAL = 0;
            newCommercialLoanLineEntry.INTERESTONPASTDUEINTEREST = 0;
            newCommercialLoanLineEntry.PENALCHARGEAMOUNT = 0;
            newCommercialLoanLineEntry.FIXEDPRINCIPAL = false;
            newCommercialLoanLineEntry.PROFILELOAN = false;
            newCommercialLoanLineEntry.DISCHARGELETTER = false;
            newCommercialLoanLineEntry.SUSPENDINTEREST = false;
            newCommercialLoanLineEntry.ALLOWFORCEDEBITREPAYMENT = true;

            newCommercialLoanLineEntry.INT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing;
            newCommercialLoanLineEntry.EXT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing;
            newCommercialLoanLineEntry.USER_PRUDENTIAL_GUIDE_STATUSID = (short)LoanPrudentialStatusEnum.Performing;
            newCommercialLoanLineEntry.NPLDATE = result.NPLDATE;
            newCommercialLoanLineEntry.CREATEDBY = model.staffId;
            newCommercialLoanLineEntry.DATETIMECREATED = DateTime.Now;
            context.TBL_LOAN.Add(newCommercialLoanLineEntry);

            var instruction = context.TBL_LOAN_MATURITY_INSTRUCTION.Where(x => x.LOANID == model.loanId);
            if (instruction.Any())
            {
                instruction.FirstOrDefault().ISUSED = true;
            }

            var loanModel = new LoanViewModel
            {
                productPriceIndexRate = result.PRODUCTPRICEINDEXRATE,
                customerId = result.CUSTOMERID,
                productId = result.PRODUCTID,
                companyId = result.COMPANYID,
                loanApplicationDetailId = result.LOANAPPLICATIONDETAILID,
                casaAccountId = result.CASAACCOUNTID,
                casaAccountId2 = result.CASAACCOUNTID2,
                loanId = model.loanId,
                loanSystemTypeId = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                branchId = result.BRANCHID,
                subSectorId = result.SUBSECTORID,
                currencyId = result.CURRENCYID,
                exchangeRate = result.EXCHANGERATE,
                loanReferenceNumber = loanReferenceNumber,
                RelatedloanReferenceNumber = result.LOANREFERENCENUMBER,
                principalNumberOfInstallment = result.PRINCIPALNUMBEROFINSTALLMENT,
                interestNumberOfInstallment = result.INTERESTINSTALLMENTLEFT,
                interestRate = result.INTERESTRATE,
                effectiveDate = newEffectivedate,
                maturityDate = newEffectivedate.AddDays(model.tenor),
                bookingDate = result.BOOKINGDATE,
                principalAmount = newPrincipal,
                principalInstallmentLeft = 0,
                interestInstallmentLeft = 0,
                scheduleTypeId = result.SCHEDULETYPEID,
                allowForceDebitRepayment = true,

                outstandingPrincipal = newPrincipal,
                outstandingInterest = newInterest,

                dateTimeCreated = DateTime.Now,
            };
            loanGenerate.PostLoanFees(loanModel);
            var loanScheduleModel = loanGenerate.BuildScheduleModel(model.loanId, model.createdBy);
            var loanDisbursementModel = loanGenerate.BuildDisbursementModel(model.loanId, loanScheduleModel, model.createdBy);
            loanGenerate.DisburseLoan(loanDisbursementModel);

            return context.SaveChanges() > 0;
        }

        public List<LoanReviewOperationParentChildViewModel> GetRunningCommercialLoanLines(int companyId)
        {
            var data = from a in context.TBL_LOAN_APPLICATION_DETAIL
                       join ln in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals ln.LOANAPPLICATIONDETAILID
                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                       where ln.LOANSTATUSID == (short)LoanStatusEnum.Active
                       // && ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
                       && ln.COMPANYID == companyId
                       orderby ln.MATURITYDATE descending
                       select new LoanReviewOperationParentChildViewModel
                       {
                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                           loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                           firstName = a.TBL_CUSTOMER.FIRSTNAME,
                           middleName = a.TBL_CUSTOMER.MIDDLENAME,
                           lastName = a.TBL_CUSTOMER.LASTNAME,
                           approvedAmount = a.APPROVEDAMOUNT,
                           approvedInterestRate = a.APPROVEDINTERESTRATE,
                           approvedProductName = a.TBL_PRODUCT.PRODUCTNAME,
                           customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                           customerCode = c.CUSTOMERCODE,
                           approvedTenor = a.APPROVEDTENOR,
                           //lineEffectiveDate =  (DateTime)a.EXPIRYDATE.Value.AddDays(- (int)a.APPROVEDTENOR) ,
                           expiryDate = (DateTime)a.EXPIRYDATE,
                           //tenorLeft = a.APPROVEDTENOR - (int)a.EXPIRYDATE.Value.AddDays(-(int)a.APPROVEDTENOR).Day

                       };
            var v = data;
            return data.ToList();
        }

        public bool reBookCommercialLoan(int loanId, int additionalTenor, int staffId, short instructionTypeId)
        {
            var systemDate = generalSetup.GetApplicationDate();

            decimal newPrincipal = 0;
            decimal newInterest = 0;

            var existingData = context.TBL_LOAN.Find(loanId);

            var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(existingData.CUSTOMERID, existingData.PRODUCTID, existingData.LOANSYSTEMTYPEID);

            TBL_LOAN newCommercialLoanEntry = new TBL_LOAN();
            newCommercialLoanEntry.PRODUCTPRICEINDEXRATE = existingData.PRODUCTPRICEINDEXRATE;
            newCommercialLoanEntry.CUSTOMERRISKRATINGID = existingData.CUSTOMERRISKRATINGID;
            newCommercialLoanEntry.LOANSYSTEMTYPEID = existingData.LOANSYSTEMTYPEID;
            newCommercialLoanEntry.CUSTOMERID = existingData.CUSTOMERID;
            newCommercialLoanEntry.PRODUCTID = existingData.PRODUCTID;
            newCommercialLoanEntry.COMPANYID = existingData.COMPANYID;
            newCommercialLoanEntry.LOANAPPLICATIONDETAILID = existingData.LOANAPPLICATIONDETAILID;
            newCommercialLoanEntry.CASAACCOUNTID = existingData.CASAACCOUNTID;
            newCommercialLoanEntry.CASAACCOUNTID2 = existingData.CASAACCOUNTID2;
            newCommercialLoanEntry.LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility;
            newCommercialLoanEntry.BRANCHID = existingData.BRANCHID;
            newCommercialLoanEntry.SUBSECTORID = existingData.SUBSECTORID;
            newCommercialLoanEntry.CURRENCYID = existingData.CURRENCYID;
            newCommercialLoanEntry.EXCHANGERATE = existingData.EXCHANGERATE;
            newCommercialLoanEntry.LOANREFERENCENUMBER = loanReferenceNumber;
            newCommercialLoanEntry.RELATED_LOAN_REFERENCE_NUMBER = loanReferenceNumber;
            newCommercialLoanEntry.PRINCIPALNUMBEROFINSTALLMENT = existingData.PRINCIPALNUMBEROFINSTALLMENT;
            newCommercialLoanEntry.INTERESTNUMBEROFINSTALLMENT = existingData.INTERESTINSTALLMENTLEFT;
            newCommercialLoanEntry.RELATIONSHIPOFFICERID = existingData.RELATIONSHIPOFFICERID;
            newCommercialLoanEntry.RELATIONSHIPMANAGERID = existingData.RELATIONSHIPMANAGERID;
            newCommercialLoanEntry.MISCODE = existingData.MISCODE;
            newCommercialLoanEntry.TEAMMISCODE = existingData.TEAMMISCODE;
            newCommercialLoanEntry.INTERESTRATE = existingData.INTERESTRATE;
            newCommercialLoanEntry.EFFECTIVEDATE = existingData.MATURITYDATE;
            newCommercialLoanEntry.MATURITYDATE = existingData.MATURITYDATE.AddDays(additionalTenor);
            newCommercialLoanEntry.BOOKINGDATE = existingData.BOOKINGDATE;
            newCommercialLoanEntry.PRINCIPALINSTALLMENTLEFT = 0;
            newCommercialLoanEntry.INTERESTINSTALLMENTLEFT = 0;
            newCommercialLoanEntry.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
            newCommercialLoanEntry.APPROVERCOMMENT = null;
            newCommercialLoanEntry.LOANSTATUSID = (short)LoanStatusEnum.Active;
            newCommercialLoanEntry.SCHEDULETYPEID = existingData.SCHEDULETYPEID;
            newCommercialLoanEntry.SCHEDULEDAYCOUNTCONVENTIONID = existingData.SCHEDULEDAYCOUNTCONVENTIONID;
            newCommercialLoanEntry.SCHEDULEDAYINTERESTTYPEID = existingData.SCHEDULEDAYINTERESTTYPEID;
            newCommercialLoanEntry.SHOULD_DISBURSE = true;
            newCommercialLoanEntry.ISDISBURSED = true;
            newCommercialLoanEntry.EQUITYCONTRIBUTION = existingData.EQUITYCONTRIBUTION;
            newCommercialLoanEntry.PASTDUEPRINCIPAL = 0;
            newCommercialLoanEntry.PASTDUEINTEREST = 0;
            newCommercialLoanEntry.INTERESTONPASTDUEPRINCIPAL = 0;
            newCommercialLoanEntry.INTERESTONPASTDUEINTEREST = 0;
            newCommercialLoanEntry.PENALCHARGEAMOUNT = 0;
            newCommercialLoanEntry.FIXEDPRINCIPAL = false;
            newCommercialLoanEntry.PROFILELOAN = false;
            newCommercialLoanEntry.DISCHARGELETTER = false;
            newCommercialLoanEntry.SUSPENDINTEREST = false;
            newCommercialLoanEntry.ALLOWFORCEDEBITREPAYMENT = true;

            newCommercialLoanEntry.INT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing;
            newCommercialLoanEntry.EXT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing;
            newCommercialLoanEntry.USER_PRUDENTIAL_GUIDE_STATUSID = (short)LoanPrudentialStatusEnum.Performing;
            newCommercialLoanEntry.NPLDATE = existingData.NPLDATE;
            newCommercialLoanEntry.CREATEDBY = staffId;
            newCommercialLoanEntry.DATETIMECREATED = DateTime.Now;

            newCommercialLoanEntry.PRINCIPALAMOUNT = newPrincipal;
            newCommercialLoanEntry.OUTSTANDINGPRINCIPAL = newPrincipal;
            newCommercialLoanEntry.OUTSTANDINGINTEREST = newInterest;

            var product = context.TBL_PRODUCT.Find(existingData.PRODUCTID);
            int interestDaysPeriod = loanGenerate.getDaysInLoanPeriod(existingData.MATURITYDATE, existingData.MATURITYDATE.AddDays(additionalTenor)) - 1;
            var totalInterest = loanGenerate.getTotalInterest(existingData.PRINCIPALAMOUNT, existingData.INTERESTRATE, interestDaysPeriod);

            switch (instructionTypeId)
            {
                case (short)MaturityInstructionTypeEnum.RolloverPrincipal:
                    if (product.DEALTYPEID == (short)DealTypeEnum.Upfront)
                    {
                        newCommercialLoanEntry.PRINCIPALAMOUNT = newPrincipal;
                        newCommercialLoanEntry.PRINCIPALAMOUNT = existingData.PRINCIPALAMOUNT;
                        newCommercialLoanEntry.OUTSTANDINGPRINCIPAL = existingData.PRINCIPALAMOUNT - totalInterest;
                        newCommercialLoanEntry.OUTSTANDINGINTEREST = totalInterest;
                    }
                    else
                    {
                        newCommercialLoanEntry.PRINCIPALAMOUNT = newPrincipal;
                        newCommercialLoanEntry.PRINCIPALAMOUNT = existingData.PRINCIPALAMOUNT;
                        newCommercialLoanEntry.OUTSTANDINGPRINCIPAL = existingData.PRINCIPALAMOUNT;
                        newCommercialLoanEntry.OUTSTANDINGINTEREST = totalInterest;
                    }
                    break;
            }
            context.TBL_LOAN.Add(newCommercialLoanEntry);

            existingData.LOANSTATUSID = (short)LoanStatusEnum.Completed;

            return context.SaveChanges() > 0;
        }

        public IEnumerable<MaturityIntructionViewModel> GetMaturityInstructionType()
        {
            var data = from a in context.TBL_LOAN_MATURITY_INSTRU_TYPE
                       select new MaturityIntructionViewModel
                       {
                           instructionTypeId = a.INSTRUCTIONTYPEID,
                           instructionTypeName = a.INSTRUCTIONTYPENAME,
                       };

            return data.ToList();
        }

        public List<LoanReviewOperationParentChildViewModel> GetCommercialLoansLines(int companyId)
        {
            var data = from a in context.TBL_LOAN_APPLICATION_DETAIL
                       join ln in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals ln.LOANAPPLICATIONDETAILID
                       where //ln.MATURITYDATE < DateTime.Now
                             //ln.LOANSTATUSID == (short)LoanStatusEnum.Completed || ln.LOANSTATUSID == (short)LoanStatusEnum.Active
                        ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
                       && ln.COMPANYID == companyId
                       orderby ln.MATURITYDATE descending
                       select new LoanReviewOperationParentChildViewModel
                       {
                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                           loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                           firstName = a.TBL_CUSTOMER.FIRSTNAME,
                           middleName = a.TBL_CUSTOMER.MIDDLENAME,
                           lastName = a.TBL_CUSTOMER.LASTNAME,
                           approvedAmount = a.APPROVEDAMOUNT,
                           approvedInterestRate = a.APPROVEDINTERESTRATE,
                           approvedProductName = a.TBL_PRODUCT.PRODUCTNAME,
                       };
            var c = data.ToList();
            //foreach(var i in data)
            //{
            //    var startDate = i.effectiveDate;
            //    var endDate = i.expiryDate;
            //    if(i.expiryDate != null && i.effectiveDate != null)
            //    {
            //        i.interestAtMuturity = loan.getTotalInterest((decimal)i.approvedAmount, (double)i.approvedInterestRate, loan.getDaysInLoanPeriod(startDate,endDate.Value));
            //    }

            //}

            return data.ToList();
        }

        public List<LoanReviewOperationApprovalViewModel> GetDueCommercialLoansByApplicationDetailId(int companyId, int loanApplicationDetailID)
        {
            var data = (from ln in context.TBL_LOAN
                        where ln.LOANAPPLICATIONDETAILID == loanApplicationDetailID
                        ////&& ln.MATURITYDATE < DateTime.Now
                        ////&& ln.LOANSTATUSID == (short)LoanStatusEnum.Active || ln.LOANSTATUSID == (short)LoanStatusEnum.Completed
                        && ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
                        && ln.COMPANYID == companyId
                        orderby ln.MATURITYDATE descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TERMLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            relatedReferenceNumber = ln.RELATED_LOAN_REFERENCE_NUMBER,
                            loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
                            companyId = ln.COMPANYID,
                            exchangeRate = ln.EXCHANGERATE,

                            principalAmount = ln.PRINCIPALAMOUNT,
                            interestRate = ln.INTERESTRATE,
                            interestAmount = ln.OUTSTANDINGINTEREST,
                            outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            outstandingInterest = ln.OUTSTANDINGINTEREST,

                            maturityAmount = ln.OUTSTANDINGPRINCIPAL + ln.OUTSTANDINGINTEREST,
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
                        });
            return data.ToList();
        }

        public List<LoanReviewOperationApprovalViewModel> GetDueCommercialLoans(int companyId)
        {
            var date = DateTime.Now.Date;
            var data = (from ln in context.TBL_LOAN
                        where //ln.MATURITYDATE < DateTime.Now &&
                         ln.OPERATIONID == (int)OperationsEnum.CommercialPaperLoanBooking
                        && ln.COMPANYID == companyId
                        orderby ln.MATURITYDATE descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TERMLOANID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            relatedReferenceNumber = ln.RELATED_LOAN_REFERENCE_NUMBER,
                            loanApplicationDetailId = ln.LOANAPPLICATIONDETAILID,
                            companyId = ln.COMPANYID,
                            exchangeRate = ln.EXCHANGERATE,

                            principalAmount = ln.PRINCIPALAMOUNT,
                            interestRate = ln.INTERESTRATE,
                            interestAmount = ln.OUTSTANDINGINTEREST,
                            outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                            outstandingInterest = ln.OUTSTANDINGINTEREST,

                            maturityAmount = ln.OUTSTANDINGPRINCIPAL + ln.OUTSTANDINGINTEREST,
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
                        });
            return data.ToList();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public loanPrepaymentViewModel CommercialPaperPrepayment(string refNo, loanPrepaymentViewModel model)
        {
            var systemDate = generalSetup.GetApplicationDate();

            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            TBL_LOAN loanRecord = (from p in context.TBL_LOAN where p.LOANREFERENCENUMBER == refNo select p).FirstOrDefault();
            TBL_LOAN newLoanRecord = new TBL_LOAN();

            var baseReferenceNumber = refNo;
            if (loanRecord.RELATED_LOAN_REFERENCE_NUMBER != null)
                baseReferenceNumber = loanRecord.RELATED_LOAN_REFERENCE_NUMBER;

            var accrual = context.TBL_DAILY_ACCRUAL.Where(p => p.BASEREFERENCENUMBER == baseReferenceNumber
                                         || p.REFERENCENUMBER == baseReferenceNumber);

            var accruedLoanDaysInYear = loanGenerate.getDaysInLoanPeriod(loanRecord.EFFECTIVEDATE, systemDate.Subtract(TimeSpan.FromDays(1)));
            var loanDaysInYear = loanGenerate.getDaysInLoanPeriod(loanRecord.EFFECTIVEDATE, loanRecord.MATURITYDATE.Subtract(TimeSpan.FromDays(1)));
            var dailyInterestAmount = loanGenerate.getDailyInterest(loanRecord.OUTSTANDINGPRINCIPAL, loanRecord.INTERESTRATE, loanDaysInYear);
            if (model.effectiveDate < systemDate)
            {
                // Back-dated payment. Do interest reversal
                if (model.effectiveDate < loanRecord.EFFECTIVEDATE)
                    throw new ConditionNotMetException("Effective date cannot be lesser than the loan effective date");

                var datediff = (systemDate - model.effectiveDate).Days;
                var remainingDaysInYear = loanGenerate.getDaysInLoanPeriod(model.effectiveDate, loanRecord.MATURITYDATE.Subtract(TimeSpan.FromDays(1)));

                var interestToDate = dailyInterestAmount * accruedLoanDaysInYear; //accrual.Sum(x => x.DAILYACCURALAMOUNT);
                var interestToLastDate = interestToDate - (dailyInterestAmount * datediff);

                newLoanRecord.OUTSTANDINGPRINCIPAL = loanRecord.OUTSTANDINGPRINCIPAL - model.amount;
                var remainingInterestAmount = loanGenerate.getTotalInterest(newLoanRecord.OUTSTANDINGPRINCIPAL, loanRecord.INTERESTRATE, remainingDaysInYear);

                newLoanRecord.OUTSTANDINGINTEREST = interestToLastDate + remainingInterestAmount;

            }
            else
            {
                var interestToDate = dailyInterestAmount * accruedLoanDaysInYear;
                newLoanRecord = loanRecord;
                newLoanRecord.OUTSTANDINGPRINCIPAL = newLoanRecord.OUTSTANDINGPRINCIPAL - loanRecord.OUTSTANDINGPRINCIPAL;
            }

            //ArchiveLoan(loanRecord.TERMLOANID, (short)OperationsEnum.CommercialPaperLoanBooking, batchCode);
            ReBookLoan(newLoanRecord);

            bool response;
            var responseModel = new loanPrepaymentViewModel();
            if (!model.isPreSubmission)
            {
                response = context.SaveChanges() > 0;
                if (response)
                {
                    responseModel.saveStatus = "saved";
                }
            }

            responseModel.interestToDate = accrual.Sum(x => x.DAILYACCURALAMOUNT);
            responseModel.newPrincipal = newLoanRecord.OUTSTANDINGPRINCIPAL;
            responseModel.InterestAtMaturity = newLoanRecord.OUTSTANDINGINTEREST;
            responseModel.newMaturityAmount = newLoanRecord.OUTSTANDINGPRINCIPAL + newLoanRecord.OUTSTANDINGINTEREST;

            return responseModel;
        }

        public bool ReBookLoan(TBL_LOAN newLoan)
        {
            var loanReferenceNumber = loanGenerate.GenerateLoanReferenceNumber(newLoan.CUSTOMERID, newLoan.PRODUCTID, newLoan.LOANSYSTEMTYPEID);

            TBL_LOAN newLoanEntry = new TBL_LOAN();
            newLoanEntry.PRODUCTPRICEINDEXRATE = newLoan.PRODUCTPRICEINDEXRATE;
            newLoanEntry.CUSTOMERRISKRATINGID = newLoan.CUSTOMERRISKRATINGID;
            newLoanEntry.LOANSYSTEMTYPEID = newLoan.LOANSYSTEMTYPEID;
            newLoanEntry.CUSTOMERID = newLoan.CUSTOMERID;
            newLoanEntry.PRODUCTID = newLoan.PRODUCTID;
            newLoanEntry.COMPANYID = newLoan.COMPANYID;
            newLoanEntry.LOANAPPLICATIONDETAILID = newLoan.LOANAPPLICATIONDETAILID;
            newLoanEntry.CASAACCOUNTID = newLoan.CASAACCOUNTID;
            newLoanEntry.CASAACCOUNTID2 = newLoan.CASAACCOUNTID2;
            newLoanEntry.LOANSYSTEMTYPEID = newLoan.LOANSYSTEMTYPEID;
            newLoanEntry.BRANCHID = newLoan.BRANCHID;
            newLoanEntry.SUBSECTORID = newLoan.SUBSECTORID;
            newLoanEntry.CURRENCYID = newLoan.CURRENCYID;
            newLoanEntry.EXCHANGERATE = newLoan.EXCHANGERATE;
            newLoanEntry.LOANREFERENCENUMBER = loanReferenceNumber;
            newLoanEntry.RELATED_LOAN_REFERENCE_NUMBER = newLoan.LOANREFERENCENUMBER;
            newLoanEntry.PRINCIPALNUMBEROFINSTALLMENT = newLoan.PRINCIPALNUMBEROFINSTALLMENT;
            newLoanEntry.INTERESTNUMBEROFINSTALLMENT = newLoan.INTERESTINSTALLMENTLEFT;
            newLoanEntry.RELATIONSHIPOFFICERID = newLoan.RELATIONSHIPOFFICERID;
            newLoanEntry.RELATIONSHIPMANAGERID = newLoan.RELATIONSHIPMANAGERID;
            newLoanEntry.MISCODE = newLoan.MISCODE;
            newLoanEntry.TEAMMISCODE = newLoan.TEAMMISCODE;
            newLoanEntry.INTERESTRATE = newLoan.INTERESTRATE;
            newLoanEntry.EFFECTIVEDATE = newLoan.MATURITYDATE;
            newLoanEntry.MATURITYDATE = newLoan.MATURITYDATE;
            newLoanEntry.BOOKINGDATE = newLoan.BOOKINGDATE;
            newLoanEntry.PRINCIPALINSTALLMENTLEFT = newLoan.PRINCIPALINSTALLMENTLEFT;
            newLoanEntry.INTERESTINSTALLMENTLEFT = newLoan.INTERESTINSTALLMENTLEFT;
            newLoanEntry.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
            newLoanEntry.APPROVERCOMMENT = newLoan.APPROVERCOMMENT;
            newLoanEntry.LOANSTATUSID = (short)LoanStatusEnum.Active;
            newLoanEntry.SCHEDULETYPEID = newLoan.SCHEDULETYPEID;
            newLoanEntry.SCHEDULEDAYCOUNTCONVENTIONID = newLoan.SCHEDULEDAYCOUNTCONVENTIONID;
            newLoanEntry.SCHEDULEDAYINTERESTTYPEID = newLoan.SCHEDULEDAYINTERESTTYPEID;
            newLoanEntry.SHOULD_DISBURSE = newLoan.SHOULD_DISBURSE;
            newLoanEntry.ISDISBURSED = newLoan.ISDISBURSED;
            newLoanEntry.EQUITYCONTRIBUTION = newLoan.EQUITYCONTRIBUTION;
            newLoanEntry.PASTDUEPRINCIPAL = newLoan.PASTDUEPRINCIPAL;
            newLoanEntry.PASTDUEINTEREST = newLoan.PASTDUEINTEREST;
            newLoanEntry.INTERESTONPASTDUEPRINCIPAL = newLoan.INTERESTONPASTDUEPRINCIPAL;
            newLoanEntry.INTERESTONPASTDUEINTEREST = newLoan.INTERESTONPASTDUEINTEREST;
            newLoanEntry.PENALCHARGEAMOUNT = newLoan.PENALCHARGEAMOUNT;
            newLoanEntry.FIXEDPRINCIPAL = newLoan.FIXEDPRINCIPAL;
            newLoanEntry.PROFILELOAN = newLoan.PROFILELOAN;
            newLoanEntry.DISCHARGELETTER = newLoan.DISCHARGELETTER;
            newLoanEntry.SUSPENDINTEREST = newLoan.SUSPENDINTEREST;
            newLoanEntry.ALLOWFORCEDEBITREPAYMENT = newLoan.ALLOWFORCEDEBITREPAYMENT;

            newLoanEntry.INT_PRUDENT_GUIDELINE_STATUSID = newLoan.INT_PRUDENT_GUIDELINE_STATUSID;
            newLoanEntry.EXT_PRUDENT_GUIDELINE_STATUSID = newLoan.EXT_PRUDENT_GUIDELINE_STATUSID;
            newLoanEntry.USER_PRUDENTIAL_GUIDE_STATUSID = newLoan.USER_PRUDENTIAL_GUIDE_STATUSID;
            newLoanEntry.NPLDATE = newLoan.NPLDATE;
            newLoanEntry.CREATEDBY = newLoan.CREATEDBY;
            newLoanEntry.DATETIMECREATED = DateTime.Now;

            newLoanEntry.PRINCIPALAMOUNT = newLoan.PRINCIPALAMOUNT;
            newLoanEntry.OUTSTANDINGPRINCIPAL = newLoan.OUTSTANDINGPRINCIPAL;
            newLoanEntry.OUTSTANDINGINTEREST = newLoan.OUTSTANDINGINTEREST;

            context.TBL_LOAN.Add(newLoanEntry);

            return context.SaveChanges() > 0;
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

        //public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyCommercialPaperInterestAccrual(DateTime applicationDate)
        //{
        //    var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


        //    var data = (from a in context.TBL_LOAN
        //                where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.PRODUCTID == (short)ProductClassEnum.Commercial


        //                select new DailyInterestAccrualViewModel()
        //                {
        //                    referenceNumber = a.LOANREFERENCENUMBER,
        //                    productId = a.PRODUCTID,
        //                    branchId = a.BRANCHID,
        //                    companyId = a.COMPANYID,
        //                    currencyId = a.CURRENCYID,
        //                    exchangeRate = a.EXCHANGERATE,
        //                    interestRate = a.INTERESTRATE,
        //                    date = applicationDate,
        //                    dailyAccuralAmount = a.INTERESTRATE,
        //                    mainAmount = a.PRINCIPALAMOUNT,
        //                    categoryId = (short)DailyAccrualCategory.CommercialLoan,
        //                    availableBalance = a.PRINCIPALAMOUNT,
        //                    transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
        //                    //baseReferenceNumber = null,
        //                    //dayCountConventionId = c.DAYCOUNTCONVENTIONID,
        //                    //daysInAYear = c.DAYSINAYEAR,

        //                });

        //    List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


        //    foreach (var item in data)
        //    {
        //        TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

        //        dailyAccrual.REFERENCENUMBER = item.referenceNumber;
        //        dailyAccrual.PRODUCTID = item.productId;
        //        dailyAccrual.BRANCHID = item.branchId;
        //        dailyAccrual.EXCHANGERATE = item.exchangeRate;
        //        dailyAccrual.CURRENCYID = item.currencyId;
        //        dailyAccrual.INTERESTRATE = item.interestRate;
        //        dailyAccrual.DATE = item.date;
        //        dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
        //        dailyAccrual.MAINAMOUNT = item.mainAmount;
        //        dailyAccrual.CATEGORYID = item.categoryId;
        //        dailyAccrual.COMPANYID = item.companyId;
        //        dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
        //        dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
        //        dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


        //        transAccrual.Add(dailyAccrual);
        //    }
        //    this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);

        //    context.SaveChanges();

        //    var model = (from a in context.TBL_DAILY_ACCRUAL
        //                 where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.CommercialLoan
        //                 group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
        //                 select new DailyInterestAccrualViewModel()
        //                 {
        //                     productId = groupedQ.Key.PRODUCTID,
        //                     branchId = groupedQ.Key.BRANCHID,
        //                     companyId = groupedQ.Key.COMPANYID,
        //                     currencyId = groupedQ.Key.CURRENCYID,
        //                     exchangeRate = groupedQ.Key.EXCHANGERATE,
        //                     dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
        //                 });

        //    foreach (var item in model)
        //    {
        //        financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
        //    }
        //    return data;
        //}

        //[OperationBehavior(TransactionScopeRequired = true)]
        //public void CommercialPaperManualRollOver(DateTime applicationDate)
        //{
        //    var maturedAutomatedCommercialLoans = (from l in context.TBL_LOAN
        //                                           join m in context.TBL_LOAN_MATURITY_INSTRUCTION on l.TERMLOANID equals m.LOANID
        //                                           where l.LOANSTATUSID == (short)LoanStatusEnum.Active
        //                                           && l.MATURITYDATE.Date == applicationDate.Date
        //                                           && l.OPERATIONID == (short)OperationsEnum.CommercialPaperLoanBooking
        //                                           select new MaturityIntructionViewModel
        //                                           {
        //                                               loanId = l.TERMLOANID,
        //                                               tenor = m.TENOR,
        //                                               instructionTypeId = m.INSTRUCTIONTYPEID,
        //                                               staffId = m.CREATEDBY,
        //                                               loanReferenceNumber = l.LOANREFERENCENUMBER
        //                                           }
        //                                           ).ToList();

        //    foreach (var loan in maturedAutomatedCommercialLoans)
        //    {
        //        ProcessCommercialPaperManualRollOver(loan, loan.loanReferenceNumber);
        //    }
        //}
        #endregion END OF COMMERCIAL PAPER



        #region Flow Type For Custom Facility Repayment Report
        public List<ItemValue> FlowTypes()
        {
            BulkTransactionPosting flow = new BulkTransactionPosting();
            return flow.GetFlowTypes();
        }
        #endregion
    }
}