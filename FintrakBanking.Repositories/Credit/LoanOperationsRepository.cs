using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
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
        public LoanOperationsRepository(

        FinTrakBankingContext _context, IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail,
            ILoanScheduleRepository _loanSchedule, IWorkflow _workFlow, IApprovalLevelStaffRepository _level)
        {

            this.context = _context;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.workFlow = _workFlow;
            this.level = _level;
        }

        public decimal GetCollateralSearchChargeAmount(int stateId)
        {
            var collateralSearchChargeAmount = this.context.tbl_State.FirstOrDefault(x => x.StateId == stateId).CollateralSearchChargeAmount;


            return collateralSearchChargeAmount;
        }
     
        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddCollateralSearchLien(CasaLienViewModel model)
        {

            var data = new tbl_CASA_Lien
            {
                ProductAccountNumber = model.productAccountNumber,
                LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                SourceReferenceNumber = model.sourceReferenceNumber,
                BranchId = model.userBranchId,
                CompanyId = model.companyId,
                LienCreditAmount = GetCollateralSearchChargeAmount(model.stateId),
                LienDebitAmount = 0,
                LienTypeId = (short)LienTypeEnum.CollateralSearch,
                CreatedBy = model.createdBy,
                Description = "lien placed due to loan application collateral search", // model.description,
                DateCreated = generalSetup.GetApplicationDate()

            };

            context.tbl_CASA_Lien.Add(data);

            // Audit Section ---------------------------            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LienAdded,
                StaffId = model.createdBy,
                BranchId = model.branchId,
                Detail = $"Applied for lien with reference number: { model.sourceReferenceNumber}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;

        }


        #region Daily Operation

        [OperationBehavior(TransactionScopeRequired = true)]
        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTeamLoansInterestAccrual (DateTime applicationDate)


        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in context.tbl_Loan_Schedule_Daily
                        join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                        join c in context.tbl_Loan_Schedule_Periodic on b.TermLoanId equals c.LoanId
                        join d in context.tbl_Day_Count_Convention on b.ScheduleDayCountConventionId equals d.DayCountConventionId
                        where a.Date == DbFunctions.TruncateTime(applicationDate) && b.LoanStatusId == (short)LoanStatusEnum.Active
                        && a.PaymentDate == c.PaymentDate

                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LoanReferenceNumber,
                            productId = b.ProductId,
                            branchId = b.BranchId,
                            companyId = b.CompanyId,
                            currencyId = b.CurrencyId,
                            exchangeRate = b.ExchangeRate,
                            interestRate = a.InterestRate,
                            date = applicationDate,
                            dailyAccuralAmount = (double)a.DailyInterestAmount,
                            mainAmount = c.PeriodInterestAmount,
                            categoryId = (short)DailyAccrualCategory.TermLoan,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DayCountConventionId,

                        }).ToList();

            List<tbl_Daily_Accrual> transAccrual = new List<tbl_Daily_Accrual>();


            foreach (var item in data)
            {
                tbl_Daily_Accrual dailyAccrual = new tbl_Daily_Accrual();

                dailyAccrual.ReferenceNumber = item.referenceNumber;
                dailyAccrual.ProductId = item.productId;
                dailyAccrual.BranchId = item.branchId;
                dailyAccrual.ExchangeRate = item.exchangeRate;
                dailyAccrual.CurrencyId = item.currencyId;
                dailyAccrual.InterestRate = item.interestRate;
                dailyAccrual.Date = item.date;
                dailyAccrual.DailyAccuralAmount = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MainAmount = item.mainAmount;
                dailyAccrual.CategoryId = item.categoryId;
                dailyAccrual.CompanyId = item.companyId;
                dailyAccrual.DayCountConventionId = item.dayCountConventionId;
                dailyAccrual.BaseReferenceNumber = item.baseReferenceNumber;
                dailyAccrual.TransactionTypeId = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);

            }
            this.context.tbl_Daily_Accrual.AddRange(transAccrual);
            context.SaveChanges();


            var model = (from a in context.tbl_Daily_Accrual
                         where a.Date == DbFunctions.TruncateTime(applicationDate) && a.CategoryId == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DailyAccuralAmount),
                         }).ToList();



            foreach (var item in model)
            {
                financeTransaction.PostDailyLoansInterestAccrual(item);
            }
            context.SaveChanges();
            return model;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan_Revolving
                        join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                        join c in context.tbl_Day_Count_Convention on a.DayCountConventionId equals c.DayCountConventionId
                        //where b.ActionDate == DbFunctions.TruncateTime(applicationDate) && a.LoanStatusId == (short)LoanStatusEnum.Active
                        where a.LoanStatusId == (short)LoanStatusEnum.Active
                       && b.AvailableBalance < 0 && a.SuspendInterest == false


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LoanReferenceNumber,
                            productId = a.ProductId,
                            branchId = a.BranchId,
                            companyId = a.CompanyId,
                            currencyId = a.CurrencyId,
                            exchangeRate = a.ExchangeRate,
                            interestRate = a.InterestRate,
                            date = applicationDate,
                            dailyAccuralAmount = a.InterestRate,
                            mainAmount = a.OverdraftLimit,
                            categoryId = (short)DailyAccrualCategory.AuthorisedOverdraft,
                            availableBalance = b.AvailableBalance,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DayCountConventionId,
                            daysInAYear = c.DaysInAYear,

                        });

            List<tbl_Daily_Accrual> transAccrual = new List<tbl_Daily_Accrual>();


            foreach (var item in data)
            {
                tbl_Daily_Accrual dailyAccrual = new tbl_Daily_Accrual();

                dailyAccrual.ReferenceNumber = item.referenceNumber;
                dailyAccrual.ProductId = item.productId;
                dailyAccrual.BranchId = item.branchId;
                dailyAccrual.ExchangeRate = item.exchangeRate;
                dailyAccrual.CurrencyId = item.currencyId;
                dailyAccrual.InterestRate = item.interestRate;
                dailyAccrual.Date = item.date;
                dailyAccrual.DailyAccuralAmount = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MainAmount = item.mainAmount;
                dailyAccrual.CategoryId = item.categoryId;
                dailyAccrual.CompanyId = item.companyId;
                dailyAccrual.DayCountConventionId = item.dayCountConventionId;
                dailyAccrual.BaseReferenceNumber = item.baseReferenceNumber;
                dailyAccrual.TransactionTypeId = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.tbl_Daily_Accrual.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.tbl_Daily_Accrual
                         where a.Date == DbFunctions.TruncateTime(applicationDate) && a.CategoryId == (short)DailyAccrualCategory.AuthorisedOverdraft
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan
                        join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                        join c in context.tbl_Day_Count_Convention on a.ScheduleDayCountConventionId equals c.DayCountConventionId
                        join d in context.tbl_Setup_Global on a.CompanyId equals d.CompanyId
                        //where b.ActionDate == DbFunctions.TruncateTime(applicationDate) && a.LoanStatusId == (short)LoanStatusEnum.Active
                        where a.LoanStatusId == (short)LoanStatusEnum.Active
                        && b.AvailableBalance < 0 && a.AllowForceDebitRepayment == false && a.SuspendInterest == false


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LoanReferenceNumber,
                            productId = a.ProductId,
                            branchId = a.BranchId,
                            companyId = a.CompanyId,
                            currencyId = a.CurrencyId,
                            exchangeRate = a.ExchangeRate,
                            interestRate = a.InterestRate,
                            date = applicationDate,
                            dailyAccuralAmount = d.UnauthorisedOverdraft_InterestRate,
                            mainAmount = b.AvailableBalance,
                            categoryId = (short)DailyAccrualCategory.UnauthorisedOverdraft,
                            availableBalance = b.AvailableBalance,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DayCountConventionId,
                            daysInAYear = c.DaysInAYear,


                        }).ToList();

            List<tbl_Daily_Accrual> transAccrual = new List<tbl_Daily_Accrual>();


            foreach (var item in data)
            {
                tbl_Daily_Accrual dailyAccrual = new tbl_Daily_Accrual();

                dailyAccrual.ReferenceNumber = item.referenceNumber;
                dailyAccrual.ProductId = item.productId;
                dailyAccrual.BranchId = item.branchId;
                dailyAccrual.ExchangeRate = item.exchangeRate;
                dailyAccrual.CurrencyId = item.currencyId;
                dailyAccrual.InterestRate = item.interestRate;
                dailyAccrual.Date = item.date;
                dailyAccrual.DailyAccuralAmount = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MainAmount = item.mainAmount;
                dailyAccrual.CategoryId = item.categoryId;
                dailyAccrual.CompanyId = item.companyId;
                dailyAccrual.DayCountConventionId = item.dayCountConventionId;
                dailyAccrual.BaseReferenceNumber = item.baseReferenceNumber;
                dailyAccrual.TransactionTypeId = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.tbl_Daily_Accrual.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.tbl_Daily_Accrual
                         where a.Date == DbFunctions.TruncateTime(applicationDate) && a.CategoryId == (short)DailyAccrualCategory.UnauthorisedOverdraft
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DailyAccuralAmount),
                         }).ToList();

            foreach (var item in model)
            {
                financeTransaction.PostDailyUnauthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDueInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan
                        join b in context.tbl_Loan_Past_Due on a.TermLoanId equals b.LoanId
                        join c in context.tbl_Day_Count_Convention on a.ScheduleDayCountConventionId equals c.DayCountConventionId
                        join d in context.tbl_Setup_Global on a.CompanyId equals d.CompanyId
                        where b.Date == DbFunctions.TruncateTime(applicationDate) && a.LoanStatusId == (short)LoanStatusEnum.Active
                       && (b.CreditAmount - b.DebitAmount) < 0 && a.AllowForceDebitRepayment == false
                        && b.TransactionTypeId == (byte)LoanTransactionTypeEnum.Interest && a.SuspendInterest == false


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LoanReferenceNumber,
                            productId = a.ProductId,
                            branchId = a.BranchId,
                            companyId = a.CompanyId,
                            currencyId = a.CurrencyId,
                            exchangeRate = a.ExchangeRate,
                            interestRate = a.InterestRate,
                            date = applicationDate,
                            dailyAccuralAmount = d.PastDueInDefault_InterestRate / 100,
                            mainAmount = (b.DebitAmount - b.CreditAmount),
                            categoryId = (short)DailyAccrualCategory.PastDueObligation,
                            availableBalance = (b.DebitAmount - b.CreditAmount),
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DayCountConventionId,
                            daysInAYear = c.DaysInAYear,


                        }).ToList();



            List<tbl_Daily_Accrual> transAccrual = new List<tbl_Daily_Accrual>();
            var count = data.Count();

            foreach (var item in data)
            {
                tbl_Daily_Accrual dailyAccrual = new tbl_Daily_Accrual();

                dailyAccrual.ReferenceNumber = item.referenceNumber;
                dailyAccrual.ProductId = item.productId;
                dailyAccrual.BranchId = item.branchId;
                dailyAccrual.ExchangeRate = item.exchangeRate;
                dailyAccrual.CurrencyId = item.currencyId;
                dailyAccrual.InterestRate = item.interestRate;
                dailyAccrual.Date = item.date;
                dailyAccrual.DailyAccuralAmount = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MainAmount = item.mainAmount;
                dailyAccrual.CategoryId = item.categoryId;
                dailyAccrual.CompanyId = item.companyId;
                dailyAccrual.DayCountConventionId = item.dayCountConventionId;
                dailyAccrual.BaseReferenceNumber = item.baseReferenceNumber;
                dailyAccrual.TransactionTypeId = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.tbl_Daily_Accrual.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.tbl_Daily_Accrual
                         where a.Date == DbFunctions.TruncateTime(applicationDate) && a.CategoryId == (short)DailyAccrualCategory.PastDueObligation
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DailyAccuralAmount),
                         }).ToList();

            foreach (var item in model)
            {
                financeTransaction.PostDailyPastDueInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDuePrincipalAccrual (DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan
                        join b in context.tbl_Loan_Past_Due on a.TermLoanId equals b.LoanId
                        join c in context.tbl_Day_Count_Convention on a.ScheduleDayCountConventionId equals c.DayCountConventionId
                        join d in context.tbl_Setup_Global on a.CompanyId equals d.CompanyId
                        where b.Date == DbFunctions.TruncateTime(applicationDate) && a.LoanStatusId == (short)LoanStatusEnum.Active
                       && (b.CreditAmount - b.DebitAmount) < 0 && a.AllowForceDebitRepayment == false
                        && b.TransactionTypeId == (byte)LoanTransactionTypeEnum.Principal && a.SuspendInterest == false


                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = a.LoanReferenceNumber,
                            productId = a.ProductId,
                            branchId = a.BranchId,
                            companyId = a.CompanyId,
                            currencyId = a.CurrencyId,
                            exchangeRate = a.ExchangeRate,
                            interestRate = a.InterestRate,
                            date = applicationDate,
                            dailyAccuralAmount = d.PastDueInDefault_InterestRate,
                            mainAmount = (b.DebitAmount - b.CreditAmount),
                            categoryId = (short)DailyAccrualCategory.UnauthorisedOverdraft,
                            availableBalance = (b.DebitAmount - b.CreditAmount),
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = c.DayCountConventionId,
                            daysInAYear = c.DaysInAYear,


                        }).ToList();

            List<tbl_Daily_Accrual> transAccrual = new List<tbl_Daily_Accrual>();


            foreach (var item in data)
            {
                tbl_Daily_Accrual dailyAccrual = new tbl_Daily_Accrual();

                dailyAccrual.ReferenceNumber = item.referenceNumber;
                dailyAccrual.ProductId = item.productId;
                dailyAccrual.BranchId = item.branchId;
                dailyAccrual.ExchangeRate = item.exchangeRate;
                dailyAccrual.CurrencyId = item.currencyId;
                dailyAccrual.InterestRate = item.interestRate;
                dailyAccrual.Date = item.date;
                dailyAccrual.DailyAccuralAmount = (decimal)Math.Abs((decimal)(item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
                dailyAccrual.MainAmount = item.mainAmount;
                dailyAccrual.CategoryId = item.categoryId;
                dailyAccrual.CompanyId = item.companyId;
                dailyAccrual.DayCountConventionId = item.dayCountConventionId;
                dailyAccrual.BaseReferenceNumber = item.baseReferenceNumber;
                dailyAccrual.TransactionTypeId = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);
            }
            this.context.tbl_Daily_Accrual.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.tbl_Daily_Accrual
                         where a.Date == DbFunctions.TruncateTime(applicationDate) && a.CategoryId == (short)DailyAccrualCategory.PastDueObligation
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DailyAccuralAmount),
                         }).ToList();

            foreach (var item in model)
            {
                financeTransaction.PostDailyPastDuePrincipalAccrual(item);
            }
            return data;
        }

        #endregion

        #region Anniversary  Operation

        public void updateloanTable(LoanRepaymentViewModel item)
        {
            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == item.loanId
                               select p).SingleOrDefault();

            result.OutstandingPrincipal = result.OutstandingPrincipal - item.periodPrincipalAmount;
            result.OutstandingInterest = result.OutstandingInterest - item.periodInterestAmount;


            context.SaveChanges();
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where a.PaymentDate == DbFunctions.TruncateTime(applicationDate) && b.LoanStatusId == (short)LoanStatusEnum.Active
                         && b.AllowForceDebitRepayment == true
                         select new LoanRepaymentViewModel()
                         {
                             productId = b.ProductId,
                             branchId = b.BranchId,
                             companyId = b.CompanyId,
                             currencyId = b.CurrencyId,
                             exchangeRate = b.ExchangeRate,
                             periodInterestAmount = a.PeriodInterestAmount,
                             periodPrincipalAmount = a.PeriodPrincipalAmount,
                             interestRate = a.InterestRate,
                             paymentDate = applicationDate,
                             loanId = a.LoanId,
                             totalAmount = a.PeriodInterestAmount + a.PeriodPrincipalAmount,
                             casaAccountId = b.CasaAccountId,
                             loanRefNo = b.LoanReferenceNumber

                         }).ToList();
          
            List<tbl_Loan_Force_Debit> transForceDebit = new List<tbl_Loan_Force_Debit>();


            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);
                //var productTypeId = context.tbl_Product_Type.FirstOrDefault(x => x.ProductGroupId == item.productId).ProductTypeId;

                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId).AvailableBalance;
                if (casabalance >= item.totalAmount)
                {
                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);

                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    tbl_Loan_Force_Debit forceDebit = new tbl_Loan_Force_Debit();




                    forceDebit.LoanId = item.loanId;
                    forceDebit.ForceDebitCode = forceDebitCode;
                    forceDebit.CreditAmount = 0;
                    forceDebit.Description = "Force Debit as a result of Account not funded";
                    forceDebit.DebitAmount = Math.Abs(casabalance - item.totalAmount);
                    forceDebit.Date = item.paymentDate;
                    forceDebit.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebit.Parent_ForceDebitCode = item.loanRefNo;
                    //forceDebit.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebit);

                    //var remainingAmount = casabalance - item.periodInterestAmount;

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "partial principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);

                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);
                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    tbl_Loan_Force_Debit forceDebitInterest = new tbl_Loan_Force_Debit();

                    forceDebitInterest.LoanId = item.loanId;
                    forceDebitInterest.ForceDebitCode = forceDebitCode;
                    forceDebitInterest.CreditAmount = 0;
                    forceDebitInterest.Description = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DebitAmount = Math.Abs(casabalance - item.periodInterestAmount);
                    forceDebitInterest.Date = item.paymentDate;
                    forceDebitInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.Parent_ForceDebitCode = item.loanRefNo;
                    //forceDebitInterest.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitInterest);

                    tbl_Loan_Force_Debit forceDebitPrincipal = new tbl_Loan_Force_Debit();

                    forceDebitPrincipal.LoanId = item.loanId;
                    forceDebitPrincipal.ForceDebitCode = forceDebitCode;
                    forceDebitPrincipal.CreditAmount = 0;
                    forceDebitPrincipal.Description = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DebitAmount = item.periodPrincipalAmount;
                    forceDebitPrincipal.Date = item.paymentDate;
                    forceDebitPrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.Parent_ForceDebitCode = item.loanRefNo;
                    //forceDebitPrincipal.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);
                }
                else if (casabalance <= 0)
                {
                    tbl_Loan_Force_Debit forceDebitInterest = new tbl_Loan_Force_Debit();

                    forceDebitInterest.LoanId = item.loanId;
                    forceDebitInterest.ForceDebitCode = forceDebitCode;
                    forceDebitInterest.CreditAmount = 0;
                    forceDebitInterest.Description = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DebitAmount = Math.Abs(item.periodInterestAmount);
                    forceDebitInterest.Date = item.paymentDate;
                    forceDebitInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.Parent_ForceDebitCode = item.loanRefNo;
                    forceDebitInterest.ProductTypeId = (short)item.productId;

                    transForceDebit.Add(forceDebitInterest);

                    tbl_Loan_Force_Debit forceDebitPrincipal = new tbl_Loan_Force_Debit();

                    forceDebitPrincipal.LoanId = item.loanId;
                    forceDebitPrincipal.ForceDebitCode = forceDebitCode;
                    forceDebitPrincipal.CreditAmount = 0;
                    forceDebitPrincipal.Description = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DebitAmount = Math.Abs(item.periodPrincipalAmount);
                    forceDebitPrincipal.Date = item.paymentDate;
                    forceDebitPrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.Parent_ForceDebitCode = item.loanRefNo;
                    forceDebitPrincipal.ProductTypeId = (short)item.productId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);
                }
            }

            this.context.tbl_Loan_Force_Debit.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingpastDue(DateTime applicationDate)

        {
            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where a.PaymentDate == DbFunctions.TruncateTime(applicationDate) && b.LoanStatusId == (short)LoanStatusEnum.Active
                         && b.AllowForceDebitRepayment == false
                         select new LoanRepaymentViewModel()
                         {
                             productId = b.ProductId,
                             branchId = b.BranchId,
                             companyId = b.CompanyId,
                             currencyId = b.CurrencyId,
                             exchangeRate = b.ExchangeRate,
                             periodInterestAmount = a.PeriodInterestAmount,
                             periodPrincipalAmount = a.PeriodPrincipalAmount,
                             interestRate = a.InterestRate,
                             paymentDate = applicationDate,
                             loanId = a.LoanId,
                             totalAmount = a.PeriodInterestAmount + a.PeriodPrincipalAmount,
                             casaAccountId = b.CasaAccountId,
                             loanRefNo = b.LoanReferenceNumber
                         }).ToList();
  
            List<tbl_Loan_Past_Due> transPastDue = new List<tbl_Loan_Past_Due>();

            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);

                var PastDueCode  = CommonHelpers.GenerateRandomDigitCode(10);
                var casa = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId);
                var casabalance = casa.AvailableBalance;
                if (casabalance >= item.totalAmount)
                {

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);
                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    tbl_Loan_Past_Due pastDue  = new tbl_Loan_Past_Due();


                    pastDue.LoanId = item.loanId;
                    pastDue.PastDueCode = PastDueCode;
                    pastDue.CreditAmount = 0;
                    pastDue.Description = "Force Debit as a result of Account not funded";
                    pastDue.DebitAmount = Math.Abs(casabalance - item.totalAmount);
                    pastDue.Date = item.paymentDate;
                    pastDue.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDue.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDue);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DebitAmount, product.PrincipalBalanceGL.Value, "partial principal repayment"));
                    // place lien on the customer account on partial principal
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDue.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDue.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Anniversary Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    //end of Audit section -------------------------------

                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    tbl_Loan_Past_Due pastDueInterest = new tbl_Loan_Past_Due();

                    pastDueInterest.LoanId = item.loanId;
                    pastDueInterest.PastDueCode = PastDueCode;
                    pastDueInterest.CreditAmount = 0;
                    pastDueInterest.Description = "Force Debit as a result of Account not funded";
                    pastDueInterest.DebitAmount = Math.Abs(casabalance - item.periodInterestAmount);
                    pastDueInterest.Date = item.paymentDate;
                    pastDueInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDueInterest);

                    tbl_Loan_Past_Due pastDuePrincipal = new tbl_Loan_Past_Due();

                    pastDuePrincipal.LoanId = item.loanId;
                    pastDuePrincipal.PastDueCode = PastDueCode;
                    pastDuePrincipal.CreditAmount = 0;
                    pastDuePrincipal.Description = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DebitAmount = item.periodPrincipalAmount;
                    pastDuePrincipal.Date = item.paymentDate;
                    pastDuePrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDuePrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DebitAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDueInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDueInterest.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.InterestRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Anniversary Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var dataP = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDuePrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDuePrincipal.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Anniversary Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(dataP);

                    // Audit Section ---------------------------            

                    var auditP = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {dataP.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(auditP);


                }
                else if (casabalance <= 0)
                {
                    tbl_Loan_Past_Due pastDueInterest = new tbl_Loan_Past_Due();

                    pastDueInterest.LoanId = item.loanId;
                    pastDueInterest.PastDueCode = PastDueCode;
                    pastDueInterest.CreditAmount = 0;
                    pastDueInterest.Description = "Force Debit as a result of Account not funded";
                    pastDueInterest.DebitAmount = Math.Abs(item.periodInterestAmount);
                    pastDueInterest.Date = item.paymentDate;
                    pastDueInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.Parent_PastDueCode = item.loanRefNo;
                    pastDueInterest.ProductTypeId = item.productId;

                    transPastDue.Add(pastDueInterest);

                    tbl_Loan_Past_Due pastDuePrincipal = new tbl_Loan_Past_Due();

                    pastDuePrincipal.LoanId = item.loanId;
                    pastDuePrincipal.PastDueCode = PastDueCode;
                    pastDuePrincipal.CreditAmount = 0;
                    pastDuePrincipal.Description = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DebitAmount = Math.Abs(item.periodPrincipalAmount);
                    pastDuePrincipal.Date = item.paymentDate;
                    pastDuePrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.Parent_PastDueCode = item.loanRefNo;
                    pastDuePrincipal.ProductTypeId = item.productId;

                    transPastDue.Add(pastDuePrincipal);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDueInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDueInterest.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.InterestRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Anniversary Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var dataP = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDuePrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDuePrincipal.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Anniversary Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(dataP);

                    // Audit Section ---------------------------            

                    var auditP = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {dataP.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(auditP);

                }
            }

            this.context.tbl_Loan_Past_Due.AddRange(transPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Daily_Accrual on a.LoanReferenceNumber equals c.ReferenceNumber
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.LoanStatusId == (short)LoanStatusEnum.Active
                         && c.CategoryId == (short)DailyAccrualCategory.AuthorisedOverdraft
                         && b.AvailableBalance < 0
                         group c by new
                         {
                             a.ProductId,
                             a.BranchId,
                             a.CompanyId,
                             a.CurrencyId,
                             a.ExchangeRate,
                             a.LoanReferenceNumber,
                             c.InterestRate,
                             a.RevolvingLoanId,
                             b.CasaAccountId
                         } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             interestRate = groupedQ.Key.InterestRate,
                             paymentDate = applicationDate,
                             loanId = groupedQ.Key.RevolvingLoanId,
                             casaAccountId = groupedQ.Key.CasaAccountId,
                             loanRefNo = groupedQ.Key.LoanReferenceNumber,
                             periodInterestAmount = groupedQ.Sum(i => i.DailyAccuralAmount)


                         }).ToList();

            List<tbl_Loan_Force_Debit> transForceDebit = new List<tbl_Loan_Force_Debit>();


            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);
                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                //var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId).AvailableBalance;
                //if (casabalance < 0 )
                //{
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildAuthorisedOverdraftRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                financeTransaction.PostTransaction(inputTransactions);
                // }
            }

            this.context.tbl_Loan_Force_Debit.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessUnauthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var model = (from a in context.tbl_Loan
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Daily_Accrual on a.LoanReferenceNumber equals c.ReferenceNumber
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.LoanStatusId == (short)LoanStatusEnum.Active
                         && c.CategoryId == (short)DailyAccrualCategory.UnauthorisedOverdraft
                         && b.AvailableBalance < 0
                         group c by new
                         { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate, a.LoanReferenceNumber, c.InterestRate, a.TermLoanId, b.CasaAccountId } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             interestRate = groupedQ.Key.InterestRate,
                             paymentDate = applicationDate,
                             loanId = groupedQ.Key.TermLoanId,
                             casaAccountId = groupedQ.Key.CasaAccountId,
                             loanRefNo = groupedQ.Key.LoanReferenceNumber,
                             periodInterestAmount = groupedQ.Sum(i => i.DailyAccuralAmount)


                         }).ToList();

            List<tbl_Loan_Force_Debit> transForceDebit = new List<tbl_Loan_Force_Debit>();


            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);
                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                //var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId).AvailableBalance;
                //if (casabalance < 0)
                //{
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildAuthorisedOverdraftRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                financeTransaction.PostTransaction(inputTransactions);
                //}
            }

            this.context.tbl_Loan_Force_Debit.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate)

        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var model = (from a in context.tbl_Loan_Past_Due
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.TransactionTypeId == (byte)LoanTransactionTypeEnum.Interest
                         group a by new
                         { a.LoanId, a.TransactionTypeId, a.PastDueCode, a.Parent_PastDueCode } into groupedQ
                         select new LoanPastDueViewModel()
                         {
                             loanId = groupedQ.Key.LoanId,
                             transactionTypeId = groupedQ.Key.TransactionTypeId,
                             pastDueCode = groupedQ.Key.PastDueCode,
                             parent_PastDueCode = groupedQ.Key.Parent_PastDueCode,
                             totalAmount = groupedQ.Sum(i => (i.DebitAmount - i.CreditAmount)),
                         }).ToList();

            List<tbl_Loan_Past_Due> loanPastDue = new List<tbl_Loan_Past_Due>();



            foreach (var item in model)
            {
                tbl_Loan_Past_Due pastDue = new tbl_Loan_Past_Due();


                pastDue.LoanId = item.loanId;
                pastDue.PastDueCode = item.pastDueCode;
                pastDue.CreditAmount = 0;
                pastDue.Description = "Interest Accrual as a result of Past Due";
                pastDue.DebitAmount = Math.Abs(item.totalAmount);
                pastDue.Date = applicationDate;
                pastDue.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                pastDue.Parent_PastDueCode = item.parent_PastDueCode;

                loanPastDue.Add(pastDue);

            }

            this.context.tbl_Loan_Past_Due.AddRange(loanPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(DateTime applicationDate)

        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var model = (from a in context.tbl_Loan_Past_Due
                         where firstDayOfMonth <= DbFunctions.TruncateTime(applicationDate) && lastDayOfMonth <= DbFunctions.TruncateTime(applicationDate)
                         && a.TransactionTypeId == (byte)LoanTransactionTypeEnum.Principal
                         group a by new
                         { a.LoanId, a.TransactionTypeId, a.PastDueCode, a.Parent_PastDueCode } into groupedQ
                         select new LoanPastDueViewModel()
                         {
                             loanId = groupedQ.Key.LoanId,
                             transactionTypeId = groupedQ.Key.TransactionTypeId,
                             pastDueCode = groupedQ.Key.PastDueCode,
                             parent_PastDueCode = groupedQ.Key.Parent_PastDueCode,
                             totalAmount = groupedQ.Sum(i => (i.DebitAmount - i.CreditAmount)),
                         }).ToList();

            List<tbl_Loan_Past_Due> loanPastDue = new List<tbl_Loan_Past_Due>();



            foreach (var item in model)
            {
                tbl_Loan_Past_Due pastDue  = new tbl_Loan_Past_Due();


                pastDue.LoanId = item.loanId;
                pastDue.PastDueCode = item.pastDueCode;
                pastDue.CreditAmount = 0;
                pastDue.Description = "Principal Accrual as a result of Past Due";
                pastDue.DebitAmount = Math.Abs(item.totalAmount);
                pastDue.Date = applicationDate;
                pastDue.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                pastDue.Parent_PastDueCode = item.parent_PastDueCode;

                loanPastDue.Add(pastDue);

            }

            this.context.tbl_Loan_Past_Due.AddRange(loanPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebitForInterestReview(DateTime applicationDate, int loanId)
        {
            var systemDate = generalSetup.GetApplicationDate();

            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where  b.LoanStatusId == (short)LoanStatusEnum.Active
                         && b.AllowForceDebitRepayment == true && a.LoanId == loanId 
                         && a.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && a.PaymentDate <= DbFunctions.TruncateTime(systemDate)
                         select new LoanRepaymentViewModel()
                         {
                             productId = b.ProductId,
                             branchId = b.BranchId,
                             companyId = b.CompanyId,
                             currencyId = b.CurrencyId,
                             exchangeRate = b.ExchangeRate,
                             periodInterestAmount = a.PeriodInterestAmount,
                             periodPrincipalAmount = a.PeriodPrincipalAmount,
                             interestRate = a.InterestRate,
                             paymentDate = applicationDate,
                             loanId = a.LoanId,
                             totalAmount = a.PeriodInterestAmount + a.PeriodPrincipalAmount,
                             casaAccountId = b.CasaAccountId,
                             loanRefNo = b.LoanReferenceNumber

                         }).ToList();

            List<tbl_Loan_Force_Debit> transForceDebit = new List<tbl_Loan_Force_Debit>();


            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);
                //var productTypeId = context.tbl_Product_Type.FirstOrDefault(x => x.ProductGroupId == item.productId).ProductTypeId;

                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId).AvailableBalance;
                if (casabalance >= item.totalAmount)
                {
                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);

                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    tbl_Loan_Force_Debit forceDebit = new tbl_Loan_Force_Debit();




                    forceDebit.LoanId = item.loanId;
                    forceDebit.ForceDebitCode = forceDebitCode;
                    forceDebit.CreditAmount = 0;
                    forceDebit.Description = "Force Debit as a result of Account not funded";
                    forceDebit.DebitAmount = Math.Abs(casabalance - item.totalAmount);
                    forceDebit.Date = item.paymentDate;
                    forceDebit.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebit.Parent_ForceDebitCode = item.loanRefNo;
                    //forceDebit.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebit);

                    //var remainingAmount = casabalance - item.periodInterestAmount;

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "partial principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);

                    //financeTransaction.PostAnniversaryTeamLoansAllowForceDebit(item);
                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    tbl_Loan_Force_Debit forceDebitInterest = new tbl_Loan_Force_Debit();

                    forceDebitInterest.LoanId = item.loanId;
                    forceDebitInterest.ForceDebitCode = forceDebitCode;
                    forceDebitInterest.CreditAmount = 0;
                    forceDebitInterest.Description = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DebitAmount = Math.Abs(casabalance - item.periodInterestAmount);
                    forceDebitInterest.Date = item.paymentDate;
                    forceDebitInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.Parent_ForceDebitCode = item.loanRefNo;
                    //forceDebitInterest.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitInterest);

                    tbl_Loan_Force_Debit forceDebitPrincipal = new tbl_Loan_Force_Debit();

                    forceDebitPrincipal.LoanId = item.loanId;
                    forceDebitPrincipal.ForceDebitCode = forceDebitCode;
                    forceDebitPrincipal.CreditAmount = 0;
                    forceDebitPrincipal.Description = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DebitAmount = item.periodPrincipalAmount;
                    forceDebitPrincipal.Date = item.paymentDate;
                    forceDebitPrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.Parent_ForceDebitCode = item.loanRefNo;
                    //forceDebitPrincipal.ProductTypeId = (byte)productTypeId.ProductTypeId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);
                }
                else if (casabalance <= 0)
                {
                    tbl_Loan_Force_Debit forceDebitInterest = new tbl_Loan_Force_Debit();

                    forceDebitInterest.LoanId = item.loanId;
                    forceDebitInterest.ForceDebitCode = forceDebitCode;
                    forceDebitInterest.CreditAmount = 0;
                    forceDebitInterest.Description = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DebitAmount = Math.Abs(item.periodInterestAmount);
                    forceDebitInterest.Date = item.paymentDate;
                    forceDebitInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.Parent_ForceDebitCode = item.loanRefNo;
                    forceDebitInterest.ProductTypeId = (short)item.productId;

                    transForceDebit.Add(forceDebitInterest);

                    tbl_Loan_Force_Debit forceDebitPrincipal = new tbl_Loan_Force_Debit();

                    forceDebitPrincipal.LoanId = item.loanId;
                    forceDebitPrincipal.ForceDebitCode = forceDebitCode;
                    forceDebitPrincipal.CreditAmount = 0;
                    forceDebitPrincipal.Description = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DebitAmount = Math.Abs(item.periodPrincipalAmount);
                    forceDebitPrincipal.Date = item.paymentDate;
                    forceDebitPrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.Parent_ForceDebitCode = item.loanRefNo;
                    forceDebitPrincipal.ProductTypeId = (short)item.productId;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);
                }
            }

            this.context.tbl_Loan_Force_Debit.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview (DateTime applicationDate, int loanId)

        {
            var systemDate = generalSetup.GetApplicationDate();

            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where b.LoanStatusId == (short)LoanStatusEnum.Active && b.AllowForceDebitRepayment == false && a.LoanId == loanId
                          && a.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && a.PaymentDate <= DbFunctions.TruncateTime(systemDate)
                         group a by new { b.ProductId, b.BranchId, b.CompanyId, b.CurrencyId, b.ExchangeRate,
                                            a.InterestRate, a.LoanId, b.CasaAccountId, b.LoanReferenceNumber, applicationDate } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             interestRate = groupedQ.Key.InterestRate,
                             paymentDate = groupedQ.Key.applicationDate,
                             loanId = groupedQ.Key.LoanId,                        
                             casaAccountId = groupedQ.Key.CasaAccountId,
                             loanRefNo = groupedQ.Key.LoanReferenceNumber,
                             periodInterestAmount = groupedQ.Sum(i => i.PeriodInterestAmount),
                             periodPrincipalAmount = groupedQ.Sum(i => i.PeriodPrincipalAmount),
                             totalAmount = groupedQ.Sum(i => i.PeriodInterestAmount) + groupedQ.Sum(i => i.PeriodPrincipalAmount),
                         }).ToList();

            List<tbl_Loan_Past_Due> transPastDue = new List<tbl_Loan_Past_Due>();

            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);

                var pastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casa = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId);
                var casabalance = casa.AvailableBalance;
                if (casabalance >= item.totalAmount)
                {

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);
                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    tbl_Loan_Past_Due pastDue = new tbl_Loan_Past_Due();


                    pastDue.LoanId = item.loanId;
                    pastDue.PastDueCode = pastDueCode;
                    pastDue.CreditAmount = 0;
                    pastDue.Description = "Force Debit as a result of Account not funded";
                    pastDue.DebitAmount = Math.Abs(casabalance - item.totalAmount);
                    pastDue.Date = item.paymentDate;
                    pastDue.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDue.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDue);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DebitAmount, product.PrincipalBalanceGL.Value, "partial principal repayment"));
                    // place lien on the customer account on partial principal
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDue.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDue.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    //end of Audit section -------------------------------

                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    tbl_Loan_Past_Due pastDueInterest = new tbl_Loan_Past_Due();

                    pastDueInterest.LoanId = item.loanId;
                    pastDueInterest.PastDueCode = pastDueCode;
                    pastDueInterest.CreditAmount = 0;
                    pastDueInterest.Description = "Force Debit as a result of Account not funded";
                    pastDueInterest.DebitAmount = Math.Abs(casabalance - item.periodInterestAmount);
                    pastDueInterest.Date = item.paymentDate;
                    pastDueInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDueInterest);

                    tbl_Loan_Past_Due pastDuePrincipal = new tbl_Loan_Past_Due();

                    pastDuePrincipal.LoanId = item.loanId;
                    pastDuePrincipal.PastDueCode = pastDueCode;
                    pastDuePrincipal.CreditAmount = 0;
                    pastDuePrincipal.Description = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DebitAmount = item.periodPrincipalAmount;
                    pastDuePrincipal.Date = item.paymentDate;
                    pastDuePrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDuePrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DebitAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDueInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDueInterest.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.InterestRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var dataP = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDuePrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDuePrincipal.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(dataP);

                    // Audit Section ---------------------------            

                    var auditP = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {dataP.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(auditP);


                }
                else if (casabalance <= 0)
                {
                    tbl_Loan_Past_Due pastDueInterest = new tbl_Loan_Past_Due();

                    pastDueInterest.LoanId = item.loanId;
                    pastDueInterest.PastDueCode = pastDueCode;
                    pastDueInterest.CreditAmount = 0;
                    pastDueInterest.Description = "Force Debit as a result of Account not funded";
                    pastDueInterest.DebitAmount = Math.Abs(item.periodInterestAmount);
                    pastDueInterest.Date = item.paymentDate;
                    pastDueInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.Parent_PastDueCode = item.loanRefNo;
                    pastDueInterest.ProductTypeId = item.productId;

                    transPastDue.Add(pastDueInterest);

                    tbl_Loan_Past_Due pastDuePrincipal = new tbl_Loan_Past_Due();

                    pastDuePrincipal.LoanId = item.loanId;
                    pastDuePrincipal.PastDueCode = pastDueCode;
                    pastDuePrincipal.CreditAmount = 0;
                    pastDuePrincipal.Description = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DebitAmount = Math.Abs(item.periodPrincipalAmount);
                    pastDuePrincipal.Date = item.paymentDate;
                    pastDuePrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.Parent_PastDueCode = item.loanRefNo;
                    pastDuePrincipal.ProductTypeId = item.productId;

                    transPastDue.Add(pastDuePrincipal);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDueInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDueInterest.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.InterestRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var dataP = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDuePrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDuePrincipal.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(dataP);

                    // Audit Section ---------------------------            

                    var auditP = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {dataP.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(auditP);

                }
            }

            this.context.tbl_Loan_Past_Due.AddRange(transPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate)

        {
            var systemDate = generalSetup.GetApplicationDate();

            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where b.LoanStatusId == (short)LoanStatusEnum.Active && b.AllowForceDebitRepayment == false 
                          && a.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && a.PaymentDate <= DbFunctions.TruncateTime(systemDate)
                         group a by new
                         {
                             b.ProductId,
                             b.BranchId,
                             b.CompanyId,
                             b.CurrencyId,
                             b.ExchangeRate,
                             a.InterestRate,
                             a.LoanId,
                             b.CasaAccountId,
                             b.LoanReferenceNumber,
                             applicationDate
                         } into groupedQ
                         select new LoanRepaymentViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             interestRate = groupedQ.Key.InterestRate,
                             paymentDate = groupedQ.Key.applicationDate,
                             loanId = groupedQ.Key.LoanId,
                             casaAccountId = groupedQ.Key.CasaAccountId,
                             loanRefNo = groupedQ.Key.LoanReferenceNumber,
                             periodInterestAmount = groupedQ.Sum(i => i.PeriodInterestAmount),
                             periodPrincipalAmount = groupedQ.Sum(i => i.PeriodPrincipalAmount),
                             totalAmount = groupedQ.Sum(i => i.PeriodInterestAmount) + groupedQ.Sum(i => i.PeriodPrincipalAmount),
                         }).ToList();

            List<tbl_Loan_Past_Due> transPastDue = new List<tbl_Loan_Past_Due>();

            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);

                var pastDueCode = CommonHelpers.GenerateRandomDigitCode(10);
                var casa = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaAccountId);
                var casabalance = casa.AvailableBalance;
                if (casabalance >= item.totalAmount)
                {

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);
                }
                else if (casabalance > item.periodInterestAmount && casabalance < item.totalAmount)
                {
                    tbl_Loan_Past_Due pastDue = new tbl_Loan_Past_Due();


                    pastDue.LoanId = item.loanId;
                    pastDue.PastDueCode = pastDueCode;
                    pastDue.CreditAmount = 0;
                    pastDue.Description = "Force Debit as a result of Account not funded";
                    pastDue.DebitAmount = Math.Abs(casabalance - item.totalAmount);
                    pastDue.Date = item.paymentDate;
                    pastDue.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDue.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDue);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDue.DebitAmount, product.PrincipalBalanceGL.Value, "partial principal repayment"));
                    // place lien on the customer account on partial principal
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDue.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDue.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    //end of Audit section -------------------------------

                }
                else if (casabalance < item.periodInterestAmount && casabalance > 0)
                {
                    tbl_Loan_Past_Due pastDueInterest = new tbl_Loan_Past_Due();

                    pastDueInterest.LoanId = item.loanId;
                    pastDueInterest.PastDueCode = pastDueCode;
                    pastDueInterest.CreditAmount = 0;
                    pastDueInterest.Description = "Force Debit as a result of Account not funded";
                    pastDueInterest.DebitAmount = Math.Abs(casabalance - item.periodInterestAmount);
                    pastDueInterest.Date = item.paymentDate;
                    pastDueInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDueInterest);

                    tbl_Loan_Past_Due pastDuePrincipal = new tbl_Loan_Past_Due();

                    pastDuePrincipal.LoanId = item.loanId;
                    pastDuePrincipal.PastDueCode = pastDueCode;
                    pastDuePrincipal.CreditAmount = 0;
                    pastDuePrincipal.Description = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DebitAmount = item.periodPrincipalAmount;
                    pastDuePrincipal.Date = item.paymentDate;
                    pastDuePrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.Parent_PastDueCode = item.loanRefNo;

                    transPastDue.Add(pastDuePrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, pastDueInterest.DebitAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDueInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDueInterest.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.InterestRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var dataP = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDuePrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDuePrincipal.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(dataP);

                    // Audit Section ---------------------------            

                    var auditP = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {dataP.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(auditP);


                }
                else if (casabalance <= 0)
                {
                    tbl_Loan_Past_Due pastDueInterest = new tbl_Loan_Past_Due();

                    pastDueInterest.LoanId = item.loanId;
                    pastDueInterest.PastDueCode = pastDueCode;
                    pastDueInterest.CreditAmount = 0;
                    pastDueInterest.Description = "Force Debit as a result of Account not funded";
                    pastDueInterest.DebitAmount = Math.Abs(item.periodInterestAmount);
                    pastDueInterest.Date = item.paymentDate;
                    pastDueInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    pastDueInterest.Parent_PastDueCode = item.loanRefNo;
                    pastDueInterest.ProductTypeId = item.productId;

                    transPastDue.Add(pastDueInterest);

                    tbl_Loan_Past_Due pastDuePrincipal = new tbl_Loan_Past_Due();

                    pastDuePrincipal.LoanId = item.loanId;
                    pastDuePrincipal.PastDueCode = pastDueCode;
                    pastDuePrincipal.CreditAmount = 0;
                    pastDuePrincipal.Description = "Force Debit as a result of Account not funded";
                    pastDuePrincipal.DebitAmount = Math.Abs(item.periodPrincipalAmount);
                    pastDuePrincipal.Date = item.paymentDate;
                    pastDuePrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    pastDuePrincipal.Parent_PastDueCode = item.loanRefNo;
                    pastDuePrincipal.ProductTypeId = item.productId;

                    transPastDue.Add(pastDuePrincipal);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDueInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDueInterest.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.InterestRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(data);

                    // Audit Section ---------------------------            

                    var audit = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {data.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);

                    var dataP = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = pastDuePrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = pastDuePrincipal.DebitAmount,
                        LienDebitAmount = 0,
                        LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                        CreatedBy = (int)SystemStaff.System,
                        Description = "lien placed due to Account not funded at Restructure Date", // model.description,
                        DateCreated = generalSetup.GetApplicationDate()

                    };

                    context.tbl_CASA_Lien.Add(dataP);

                    // Audit Section ---------------------------            

                    var auditP = new tbl_Audit
                    {
                        AuditTypeId = (short)AuditTypeEnum.LienAdded,
                        StaffId = (int)SystemStaff.System,
                        BranchId = item.branchId,
                        Detail = $"Applied for lien with reference number: {dataP.SourceReferenceNumber}",
                        IPAddress = item.userIPAddress,
                        Url = item.applicationUrl,
                        ApplicationDate = generalSetup.GetApplicationDate(),
                        SystemDateTime = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(auditP);

                }
            }

            this.context.tbl_Loan_Past_Due.AddRange(transPastDue);

            context.SaveChanges();
            return model;
        }

        #endregion

        #region Periodic  Operation

        public IEnumerable<LoanViewModel> ProcessIntervalFeeandCommissionPosting(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Fee
                         join b in context.tbl_Product_Type on a.ProductTypeId equals b.ProductTypeId
                         join c in context.tbl_Loan_Fee_Schedule on a.LoanChargeFeeId equals c.LoanChargeFeeId
                         join d in context.tbl_Loan on a.LoanId equals d.TermLoanId
                         join e in context.tbl_CASA on d.CasaAccountId equals e.CasaAccountId
                         where c.FeeDate == DbFunctions.TruncateTime(applicationDate) && a.IsRecurring == true
                         && b.ProductTypeId == (short)LoanProductTypeEnum.TermLoan
                         select new LoanViewModel()
                         {
                             productId = d.ProductId,
                             branchId = d.BranchId,
                             companyId = d.CompanyId,
                             currencyId = d.CurrencyId,
                             exchangeRate = d.ExchangeRate,
                             interestRate = d.InterestRate,
                             paymentDate = applicationDate,
                             loanId = a.LoanId,
                             totalAmount = c.FeeAmount,
                             casaAccountId = e.CasaAccountId,
                             loanReferenceNumber = d.LoanReferenceNumber,
                             chargeFeeId = a.ChargeFeeId


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
            var model = (from a in context.tbl_Loan
                         join b in context.tbl_Branch on a.BranchId equals b.BranchId
                         join c in context.tbl_Limit_Detail on b.BranchId equals c.TargetId
                         join d in context.tbl_Limit on c.LimitId equals d.LimitId
                         where b.BranchId == c.TargetId && c.LimitId == d.LimitId
                         && d.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan
                         && c.LimitTypeId == (int)LimitType.Branch
                         group a by new
                         { a.BranchId, c.LimitId, c.MaximumValue } into groupedQ
                         select new LimitSuspensionViewModel()
                         {
                             limitId = groupedQ.Key.LimitId,
                             limitAmount = groupedQ.Key.MaximumValue,
                             branchId = groupedQ.Key.BranchId,
                             amount = groupedQ.Sum(i => (i.PrincipalAmount)),

                         }).ToList();


            foreach (var item in model)
            {
                if (item.amount <= item.limitAmount)
                {
                    tbl_Branch result = (from p in context.tbl_Branch
                                         where p.BranchId == item.branchId
                                         select p).SingleOrDefault();

                    result.NPL_LimitExceeded = true ;


                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LimitSuspensionViewModel> ProcessNPLByRMSuspension()
        {
            var model = (from a in context.tbl_Loan
                         join b in context.tbl_Staff on a.RelationshipManagerId equals b.StaffId
                         join c in context.tbl_Limit_Detail on b.StaffId equals c.TargetId
                         join d in context.tbl_Limit on c.LimitId equals d.LimitId
                         where b.StaffId == c.TargetId && c.LimitId == d.LimitId
                         && d.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan
                         && c.LimitTypeId == (int)LimitType.RelationshipManager
                         group a by new
                         { a.RelationshipManagerId, c.LimitId, c.MaximumValue } into groupedQ
                         select new LimitSuspensionViewModel()
                         {
                             limitId = groupedQ.Key.LimitId,
                             limitAmount = groupedQ.Key.MaximumValue,
                             staffId = groupedQ.Key.RelationshipManagerId,
                             amount = groupedQ.Sum(i => (i.PrincipalAmount)),

                         }).ToList();


            foreach (var item in model)
            {
                if (item.amount >= item.limitAmount)
                {
                    tbl_Staff result = (from p in context.tbl_Staff
                                        where p.StaffId == item.staffId
                                        select p).SingleOrDefault();

                    result.NPL_LimitExceeded = true;


                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCleanUp(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Loan_Covenant_Detail on a.RevolvingLoanId equals c.LoanId
                         join d in context.tbl_Loan_Covenant_Type on c.CovenantTypeId equals d.CovenantTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.RevolvingLoanId == c.LoanId
                         && c.CovenantTypeId == d.CovenantTypeId && c.NextCovenantDate == DbFunctions.TruncateTime(applicationDate)
                          && d.CovenantTypeId == (short)LoanCovenantTypeEnum.Cleanup
                         select new LoanCovenantDetailViewModel()
                         {
                             loanCovenantDetailId = c.LoanCovenantDetailId,
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             frequencyTypeId = c.FrequencyTypeId,

                         }).ToList();


            foreach (var item in model)
            {
                var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaId).AvailableBalance;
                if (casabalance >= 0)
                {
                    tbl_Loan_Covenant_Detail result = (from p in context.tbl_Loan_Covenant_Detail
                                                       where p.LoanCovenantDetailId == item.loanCovenantDetailId
                                                       select p).SingleOrDefault();

                    result.CovenantDate = applicationDate;
                    result.NextCovenantDate = applicationDate.AddMonths((short)item.frequencyTypeId);///check if is monthly otherwise pick from setup


                    context.SaveChanges();
                }
                else
                {
                    tbl_CASA result = (from p in context.tbl_CASA
                                       where p.CasaAccountId == item.casaId
                                       select p).SingleOrDefault();

                    result.PostNoStatusId = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCovenant (DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Loan_Covenant_Detail on a.RevolvingLoanId equals c.LoanId
                         join d in context.tbl_Loan_Covenant_Type on c.CovenantTypeId equals d.CovenantTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.RevolvingLoanId == c.LoanId
                         && c.CovenantTypeId == d.CovenantTypeId && c.NextCovenantDate == DbFunctions.TruncateTime(applicationDate)
                         && d.CovenantTypeId == (short)LoanCovenantTypeEnum.Turnover
                         select new LoanCovenantDetailViewModel()
                         {
                             loanCovenantDetailId = c.LoanCovenantDetailId,
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             covenantAmount = c.CovenantAmount,
                             covenantDate = c.CovenantDate,
                             frequencyTypeId = c.FrequencyTypeId,

                         }).ToList();


            foreach (var item in model)
            {
                var casabalance = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == item.casaId).AvailableBalance;///pick from transaction table in finnacle to get inflows pass Date Range
                if (casabalance >= item.covenantAmount)
                {
                    tbl_Loan_Covenant_Detail result = (from p in context.tbl_Loan_Covenant_Detail
                                                       where p.LoanCovenantDetailId == item.loanCovenantDetailId
                                                       select p).SingleOrDefault();

                    result.CovenantDate = applicationDate;
                    result.NextCovenantDate = applicationDate.AddMonths((short)item.frequencyTypeId);///check if is monthly otherwise pick from setup


                    context.SaveChanges();
                }
                else
                {
                    tbl_CASA result = (from p in context.tbl_CASA
                                       where p.CasaAccountId == item.casaId
                                       select p).SingleOrDefault();

                    result.PostNoStatusId = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessLPOExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join e in context.tbl_Product on a.ProductId equals e.ProductId
                         join f in context.tbl_Product_Type on e.ProductTypeId equals f.ProductTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.ProductId == e.ProductId
                         && e.ProductTypeId == f.ProductTypeId && a.EffectiveDate <= DbFunctions.TruncateTime(applicationDate)
                         && e.ProductTypeId == (short)LoanProductTypeEnum.LPO
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             effectiveDate = a.EffectiveDate,
                             maximumDrawDownDuration = (int)e.MaximumDrawDownDuration,//(int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         }).ToList();


            foreach (var item in model)
            {
                var maxDay = (applicationDate - item.effectiveDate).TotalDays;
                if (maxDay >= item.maximumDrawDownDuration)
                {
                    tbl_CASA result = (from p in context.tbl_CASA
                                       where p.CasaAccountId == item.casaId
                                       select p).SingleOrDefault();

                    result.PostNoStatusId = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessCFFExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join e in context.tbl_Product on a.ProductId equals e.ProductId
                         join f in context.tbl_Product_Type on e.ProductTypeId equals f.ProductTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.ProductId == e.ProductId
                         && e.ProductTypeId == f.ProductTypeId && a.EffectiveDate <= DbFunctions.TruncateTime(applicationDate)
                         && e.ProductTypeId == (short)LoanProductTypeEnum.CFF
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             effectiveDate = a.EffectiveDate,
                             maximumDrawDownDuration = (int)e.MaximumDrawDownDuration,//(int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         }).ToList();


            foreach (var item in model)
            {
                var maxDay = (applicationDate - item.effectiveDate).TotalDays;
                if (maxDay >= item.maximumDrawDownDuration)
                {
                    tbl_CASA result = (from p in context.tbl_CASA
                                       where p.CasaAccountId == item.casaId
                                       select p).SingleOrDefault();

                    result.PostNoStatusId = (short)CASAPostNoStatusEnum.PostNoDebit;

                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LoanCovenantDetailViewModel> ProcessIDFExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join e in context.tbl_Product on a.ProductId equals e.ProductId
                         join f in context.tbl_Product_Type on e.ProductTypeId equals f.ProductTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.ProductId == e.ProductId
                         && e.ProductTypeId == f.ProductTypeId && a.EffectiveDate <= DbFunctions.TruncateTime(applicationDate)
                         && e.ProductTypeId == (short)LoanProductTypeEnum.IDF
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             effectiveDate = a.EffectiveDate,
                             maximumDrawDownDuration = (int)e.MaximumDrawDownDuration,//(int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         }).ToList();


            foreach (var item in model)
            {
                var maxDay = (applicationDate - item.effectiveDate).TotalDays;
                if (maxDay >= item.maximumDrawDownDuration)
                {
                    tbl_CASA result = (from p in context.tbl_CASA
                                       where p.CasaAccountId == item.casaId
                                       select p).SingleOrDefault();

                    result.PostNoStatusId = (short)CASAPostNoStatusEnum.PostNoDebit;

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


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildChargeReversalPosting(feeInput));

            financeTransaction.PostTransaction(inputTransactions);
            AddChargeReversal(feeInput, applicationDate, staffId);


            output = true;

            return output;
        }

        public bool AddChargeReversal(LoanChargeFeeViewModel model, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var productType = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == model.productId && x.CompanyId == model.companyId).ProductTypeId;

            tbl_Loan_Fee loanFee = new tbl_Loan_Fee();

            loanFee.LoanId = model.loanId;
            loanFee.ProductTypeId = productType;
            loanFee.ChargeFeeId = model.chargeFeeId;
            loanFee.FeeRateValue = model.feeRateValue;
            loanFee.FeeDependentAmount = model.feeDependentAmount;
            loanFee.FeeAmount = model.feeAmountDiff;
            loanFee.IsIntegralFee = false;
            loanFee.IsRecurring = false;
            loanFee.RecurringPaymentDay = 0;
            loanFee.CreatedBy = staffId;
            loanFee.DateTimeCreated = applicationDate;
            loanFee.LastUpdatedBy = null;
            loanFee.DateTimeUpdated = systemDate;
            loanFee.Deleted = false;
            loanFee.DeletedBy = null;
            loanFee.DateTimeDeleted = systemDate;


            this.context.tbl_Loan_Fee.Add(loanFee); ////change to Temp table

            context.SaveChanges();




            output = true;

            return output;
        }

        public void OverdraftTopUp(int loanId, decimal amount)
        {
            tbl_Loan_Revolving result = (from p in context.tbl_Loan_Revolving
                                         where p.RevolvingLoanId == loanId
                                         && p.LoanStatusId == (short)LoanStatusEnum.Active
                                         select p).SingleOrDefault();

            result.OverdraftLimit = result.OverdraftLimit + amount;



            context.SaveChanges();
        }

        public void ChangeOperativeAccount(int casaAccountId, int newCasaAccountId)
        {
            tbl_Loan result = (from p in context.tbl_Loan
                               where p.CasaAccountId == casaAccountId
                                && p.LoanStatusId == (short)LoanStatusEnum.Active
                               select p).SingleOrDefault();
            var casa = this.context.tbl_CASA.Where(x => x.CasaAccountId == newCasaAccountId && x.AccountStatusId == (short)CASAAccountStatusEnum.Active).FirstOrDefault().CasaAccountId;

            result.CasaAccountId = casa;
            context.SaveChanges();
        }

        public void SubAllocation(List<SubAllocationViewModel> fromAccount, int toLoanId, DateTime applicationDate, int staffId)
        {
            foreach (var item in fromAccount)
            {
                tbl_Loan_Revolving result = (from p in context.tbl_Loan_Revolving
                                             where p.RevolvingLoanId == toLoanId
                                             && p.LoanStatusId == (short)LoanStatusEnum.Active
                                             select p).SingleOrDefault();

                result.OverdraftLimit = result.OverdraftLimit + item.fromAmount;
                context.SaveChanges();
            }
        }

        #endregion

        #region Re-phasement  Operation
        //---------------------------------- Begining of Loan re-phasement-------------------------------------

        //---------------------------------- Rate Revision-------------------------------------

        public int LoanExist(int loanId)
        {
            var loanRef = (from a in context.tbl_Loan_Schedule_Periodic
                           where a.LoanId == loanId
                           select a);
            int loanRefResults = loanRef.Count();

            return loanRefResults;
        }

        public bool DeleteLoanExist (int loanId)
        {
            bool output = false;

            var removeLoan_Archive  = (from p in context.tbl_Loan_Archive
                                       where p.LoanId == loanId
                                select p);
            var removeLoan_Schedule_Periodic_Archive  = (from p in context.tbl_Loan_Schedule_Periodic_Archive
                                                         where p.LoanId == loanId
                                      select p);
            var removeLoan_Schedule_Daily_Archive = (from p in context.tbl_Loan_Schedule_Daily_Archive
                                                     where p.LoanId == loanId
                                      select p);
            var removeLoan_Schedule_Daily_Temp = (from p in context.tbl_Loan_Schedule_Daily_Temp
                                                  where p.LoanId == loanId
                                      select p);
            var removeLoan_Schedule_Periodic_Temp = (from p in context.tbl_Loan_Schedule_Periodic_Temp
                                                     where p.LoanId == loanId
                                      select p);

            if (removeLoan_Archive != null)
            {
                context.tbl_Loan_Archive.RemoveRange(removeLoan_Archive);
            }
            if (removeLoan_Schedule_Periodic_Archive != null)
            {
                context.tbl_Loan_Schedule_Periodic_Archive.RemoveRange(removeLoan_Schedule_Periodic_Archive);

            }
            if (removeLoan_Schedule_Daily_Archive != null)
            {
                context.tbl_Loan_Schedule_Daily_Archive.RemoveRange(removeLoan_Schedule_Daily_Archive);

            }
            if (removeLoan_Schedule_Daily_Temp != null)
            {
                context.tbl_Loan_Schedule_Daily_Temp.RemoveRange(removeLoan_Schedule_Daily_Temp);

            }
            if (removeLoan_Schedule_Periodic_Temp != null)
            {
                context.tbl_Loan_Schedule_Periodic_Temp.RemoveRange(removeLoan_Schedule_Periodic_Temp);
               
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


        public IEnumerable<LoanViewModel> ArchiveLoan (int loanId,int operationId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan
                         where a.TermLoanId == loanId && a.LoanStatusId == (short)LoanStatusEnum.Active
                         select new LoanViewModel()
                         {
                             loanId = a.TermLoanId,
                             productPriceIndexRate = a.ProductPriceIndexRate,
                             customerRiskRatingId = a.CustomerRiskRatingId,
                             customerId = a.CustomerId,
                             productId = a.ProductId,
                             companyId = a.CompanyId,
                             casaAccountId = a.CasaAccountId,
                             branchId = a.BranchId,
                             currencyId = a.CurrencyId,
                             exchangeRate = a.ExchangeRate,
                             loanApplicationDetailId = a.LoanApplicationDetailId,
                             loanReferenceNumber = a.LoanReferenceNumber,
                             subSectorId = a.SubSectorId,
                             principalFrequencyTypeId = a.PrincipalFrequencyTypeId,
                             interestFrequencyTypeId = a.InterestFrequencyTypeId,
                             principalNumberOfInstallment = a.PrincipalNumberOfInstallment,
                             interestNumberOfInstallment = a.InterestNumberOfInstallment,
                             relationshipOfficerId = a.RelationshipOfficerId,
                             relationshipManagerId = a.RelationshipManagerId,
                             misCode = a.MISCode,
                             teamMiscode = a.TeamMISCode,
                             interestRate = a.InterestRate,
                             effectiveDate = a.EffectiveDate,
                             maturityDate = a.MaturityDate,
                             bookingDate = a.BookingDate,
                             principalAmount = a.PrincipalAmount,                           

                             principalInstallmentLeft = a.PrincipalInstallmentLeft,
                             interestInstallmentLeft = a.InterestInstallmentLeft,
                             approvalStatusId = a.ApprovalStatusId,
                             approvedBy = a.ApprovedBy,
                             approverComment = a.ApproverComment,
                             dateApproved = a.DateApproved,
                             loanStatusId = a.LoanStatusId,
                             scheduleTypeId = a.ScheduleTypeId,
                             scheduleDayCountConventionId = a.ScheduleDayCountConventionId,
                             scheduleDayInterestTypeId = a.ScheduleDayInterestTypeId,
                             isDisbursed = a.IsDisbursed,
                             disbursedBy = a.DisbursedBy,
                             disburserComment = a.DisburserComment,
                             disburseDate = a.DisburseDate,
                             operationId = a.OperationId,
                             customerGroupId = a.CustomerGroupId,
                             loanTypeId = a.LoanTypeId,
                             equityContribution = a.EquityContribution,
                             firstPrincipalPaymentDate = a.FirstPrincipalPaymentDate,
                             firstInterestPaymentDate = a.FirstInterestPaymentDate,
                             outstandingPrincipal = a.OutstandingPrincipal,
                             outstandingInterest = a.OutstandingInterest,
                             principalAdditionCount = a.PrincipalAdditionCount,
                             principalReductionCount = a.PrincipalReductionCount,
                             fixedPrincipal = a.FixedPrincipal,
                             profileLoan = a.ProfileLoan,
                             dischargeLetter = a.DischargeLetter,
                             suspendInterest = a.SuspendInterest,
                             isScheduledPrepayment = a.IsScheduledPrepayment,
                             allowForceDebitRepayment = a.AllowForceDebitRepayment,
                             scheduledPrepaymentAmount = a.ScheduledPrepaymentAmount,
                             scheduledPrepaymentDate = a.ScheduledPrepaymentDate,
                             scheduledPrepaymentFrequencyTypeId = a.ScheduledPrepaymentFrequencyTypeId,
                             customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                             internalPrudentialGuidelineStatusId = a.InternalPrudentialGuidelineStatusId,
                             externalPrudentialGuidelineStatusId = a.ExternalPrudentialGuidelineStatusId,
                             nplDate = a.NPLDate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,

                         }).ToList();

            List<tbl_Loan_Archive> loanArchive = new List<tbl_Loan_Archive>();



            foreach (var item in model)
            {

                tbl_Loan_Archive addLoanArchive   = new tbl_Loan_Archive();


                addLoanArchive.ChangeEffectiveDate = DateTime.Today;
                addLoanArchive.IsApplied = false;
                addLoanArchive.ChangeReason = "Rephasement";
                addLoanArchive.LoanId = item.loanId;
                addLoanArchive.ProductPriceIndexRate = item.productPriceIndexRate;
                addLoanArchive.CustomerRiskRatingId = item.customerRiskRatingId;
                addLoanArchive.CustomerId = item.customerId;
                addLoanArchive.ProductId = item.productId;
                addLoanArchive.CompanyId = item.companyId;
                addLoanArchive.CasaAccountId = item.casaAccountId;
                addLoanArchive.BranchId = item.branchId;
                addLoanArchive.CurrencyId = (short)item.currencyId;
                addLoanArchive.ExchangeRate = item.exchangeRate;
                addLoanArchive.LoanApplicationDetailId = item.loanApplicationDetailId;
                addLoanArchive.LoanReferenceNumber = item.loanReferenceNumber;
                addLoanArchive.SubSectorId = item.subSectorId;
                addLoanArchive.PrincipalFrequencyTypeId = item.principalFrequencyTypeId;
                addLoanArchive.InterestFrequencyTypeId = item.interestFrequencyTypeId;
                addLoanArchive.PrincipalNumberOfInstallment = item.principalNumberOfInstallment;
                addLoanArchive.InterestNumberOfInstallment = item.interestNumberOfInstallment;
                addLoanArchive.RelationshipOfficerId = item.relationshipOfficerId;
                addLoanArchive.RelationshipManagerId = item.relationshipManagerId;
                addLoanArchive.MISCode = item.misCode;
                addLoanArchive.TeamMISCode = item.teamMiscode;
                addLoanArchive.InterestRate = item.interestRate;
                addLoanArchive.EffectiveDate = item.effectiveDate;
                addLoanArchive.MaturityDate = item.maturityDate;
                addLoanArchive.BookingDate = item.bookingDate;
                addLoanArchive.PrincipalAmount = item.principalAmount;
                addLoanArchive.PrincipalInstallmentLeft = item.principalInstallmentLeft;
                addLoanArchive.InterestInstallmentLeft = item.interestInstallmentLeft;
                addLoanArchive.ApprovalStatusId = item.approvalStatusId;
                addLoanArchive.ApprovedBy = item.approvedBy;
                addLoanArchive.ApproverComment = item.approverComment;
                addLoanArchive.DateApproved = item.dateApproved;
                addLoanArchive.LoanStatusId = item.loanStatusId;
                addLoanArchive.CreatedBy = item.createdBy;
                addLoanArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanArchive.ScheduleTypeId = item.scheduleTypeId;
                addLoanArchive.ScheduleDayCountConventionId = item.scheduleDayCountConventionId;
                addLoanArchive.ScheduleDayInterestTypeId = item.scheduleDayInterestTypeId;
                addLoanArchive.IsDisbursed = item.isDisbursed;
                addLoanArchive.DisbursedBy = item.disbursedBy;
                addLoanArchive.DisburserComment = item.disburserComment;
                addLoanArchive.DisburseDate = item.disburseDate;
                addLoanArchive.OperationId = operationId;
                addLoanArchive.CustomerGroupId = item.customerGroupId;
                addLoanArchive.LoanTypeId = item.loanTypeId;
                //addLoanArchive.TrancheBatchCode = item.trancheBatchCode;
                addLoanArchive.EquityContribution = item.equityContribution;
                addLoanArchive.FirstPrincipalPaymentDate = item.firstPrincipalPaymentDate;
                addLoanArchive.FirstInterestPaymentDate = item.firstInterestPaymentDate;
                addLoanArchive.OutstandingPrincipal = item.outstandingPrincipal;
                addLoanArchive.OutstandingInterest = item.outstandingInterest;
                addLoanArchive.PrincipalAdditionCount = item.principalAdditionCount;
                addLoanArchive.PrincipalReductionCount = item.principalReductionCount;
                addLoanArchive.FixedPrincipal = item.fixedPrincipal;
                addLoanArchive.ProfileLoan = item.profileLoan;
                addLoanArchive.DischargeLetter = item.dischargeLetter;
                addLoanArchive.SuspendInterest = item.suspendInterest;
                addLoanArchive.IsScheduledPrepayment = item.isScheduledPrepayment;
                addLoanArchive.AllowForceDebitRepayment = item.allowForceDebitRepayment;
                addLoanArchive.ScheduledPrepaymentAmount = item.scheduledPrepaymentAmount;
                addLoanArchive.ScheduledPrepaymentDate = item.scheduledPrepaymentDate;
                addLoanArchive.ScheduledPrepaymentFrequencyTypeId = item.principalFrequencyTypeId;//scheduledPrepaymentFrequencyTypeId;
                addLoanArchive.CustomerSensitivityLevelId = item.customerSensitivityLevelId;
                addLoanArchive.InternalPrudentialGuidelineStatusId = 1; //item.internalPrudentialGuidelineStatusId;
                addLoanArchive.ExternalPrudentialGuidelineStatusId = 1; //item.externalPrudentialGuidelineStatusId;
                addLoanArchive.NPLDate = item.nplDate;
                addLoanArchive.CreatedBy = item.createdBy;
                addLoanArchive.DateTimeCreated = item.dateTimeCreated;

                 loanArchive.Add(addLoanArchive);
            }
            //tbl_Loan
           this.context.tbl_Loan_Archive.AddRange(loanArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         where a.LoanId == loanId
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LoanId,
                             paymentNumber = a.PaymentNumber,
                             paymentDate = a.PaymentDate,
                             startPrincipalAmount = (double)a.StartPrincipalAmount,
                             periodPaymentAmount = (double)a.PeriodPaymentAmount,
                             periodInterestAmount = (double)a.PeriodInterestAmount,
                             periodPrincipalAmount = (double)a.PeriodPrincipalAmount,
                             endPrincipalAmount = (double)a.EndPrincipalAmount,
                             interestRate = a.InterestRate,
                             amortisedStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                             amortisedPeriodPaymentAmount = (double)a.AmortisedPeriodPaymentAmount,
                             amortisedPeriodInterestAmount = (double)a.AmortisedPeriodInterestAmount,
                             amortisedPeriodPrincipalAmount = (double)a.AmortisedPeriodPrincipalAmount,
                             amortisedEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                             effectiveInterestRate = a.EffectiveInterestRate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,

                         }).ToList();

            List<tbl_Loan_Schedule_Periodic_Archive> loanSchedulePeriodicArchive = new List<tbl_Loan_Schedule_Periodic_Archive>();



            foreach (var item in model)
            {

                tbl_Loan_Schedule_Periodic_Archive addLoanSchedulePeriodicArchive = new tbl_Loan_Schedule_Periodic_Archive();


                addLoanSchedulePeriodicArchive.LoanId = item.loanId;
                addLoanSchedulePeriodicArchive.PaymentNumber = item.paymentNumber;
                addLoanSchedulePeriodicArchive.PaymentDate = item.paymentDate;
                addLoanSchedulePeriodicArchive.StartPrincipalAmount = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodicArchive.PeriodPaymentAmount = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodicArchive.PeriodInterestAmount = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodicArchive.PeriodPrincipalAmount = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodicArchive.EndPrincipalAmount = (decimal)item.endPrincipalAmount;


                addLoanSchedulePeriodicArchive.InterestRate = item.interestRate;
                addLoanSchedulePeriodicArchive.AmortisedStartPrincipalAmount = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodicArchive.AmortisedPeriodPaymentAmount = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodicArchive.AmortisedPeriodInterestAmount = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodicArchive.AmortisedPeriodPrincipalAmount = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodicArchive.AmortisedEndPrincipalAmount = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodicArchive.EffectiveInterestRate = item.effectiveInterestRate;
                addLoanSchedulePeriodicArchive.CreatedBy = item.createdBy;
                addLoanSchedulePeriodicArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanSchedulePeriodicArchive.ArchiveDate = generalSetup.GetApplicationDate();
                addLoanSchedulePeriodicArchive.ArchiveBatchCode = batchCode;

                loanSchedulePeriodicArchive.Add(addLoanSchedulePeriodicArchive);

            }

            this.context.tbl_Loan_Schedule_Periodic_Archive.AddRange(loanSchedulePeriodicArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan_Schedule_Daily
                         where a.LoanId == loanId
                         select new LoanPaymentScheduleDailyViewModel()
                         {
                             loanId = a.LoanId,
                             paymentNumber = a.PaymentNumber,
                             date = a.Date,
                             paymentDate = a.PaymentDate,
                             openingBalance = (double)a.OpeningBalance,
                             startPrincipalAmount = (double)a.StartPrincipalAmount,
                             dailyPaymentAmount = (double)a.DailyPaymentAmount,
                             dailyInterestAmount = (double)a.DailyInterestAmount,
                             dailyPrincipalAmount = (double)a.DailyPrincipalAmount,
                             closingBalance = (double)a.ClosingBalance,
                             endPrincipalAmount = (double)a.EndPrincipalAmount,
                             amortisedCost = (double)a.AmortisedCost,
                             accruedInterest = (double)a.AccruedInterest,
                             norminalInterestRate = a.InterestRate,

                             amOpeningBalance = (double)a.AmortisedOpeningBalance,
                             amStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                             amDailyPaymentAmount = (double)a.AmortisedDailyPaymentAmount,
                             amDailyInterestAmount = (double)a.AmortisedDailyInterestAmount,
                             amDailyPrincipalAmount = (double)a.AmortisedDailyPrincipalAmount,
                             amClosingBalance = (double)a.AmortisedClosingBalance,
                             amEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                             amAccruedInterest = (double)a.AmortisedAccruedInterest,
                             amAmortisedCost = (double)a.Amortised_AmortisedCost,
                             discountPremium = (double)a.DiscountPremium,
                             unEarnedFee = (double)a.UnEarnedFee,
                             earnedFee = (double)a.EarnedFee,
                             effectiveInterestRate = a.EffectiveInterestRate,
                             numberOfPeriods = a.NumberOfPeriods,
                             ballonAmount = (double)a.BallonAmount,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,


                         }).ToList();

            List<tbl_Loan_Schedule_Daily_Archive> loanScheduleDailyArchive = new List<tbl_Loan_Schedule_Daily_Archive>();



            foreach (var item in model)
            {
                tbl_Loan_Schedule_Daily_Archive addLoanScheduleDailyArchive = new tbl_Loan_Schedule_Daily_Archive();


                addLoanScheduleDailyArchive.LoanId = loanId;
                addLoanScheduleDailyArchive.PaymentNumber = item.paymentNumber;
                addLoanScheduleDailyArchive.Date = item.date;
                addLoanScheduleDailyArchive.PaymentDate = item.paymentDate;
                addLoanScheduleDailyArchive.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                addLoanScheduleDailyArchive.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                addLoanScheduleDailyArchive.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                addLoanScheduleDailyArchive.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                addLoanScheduleDailyArchive.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                addLoanScheduleDailyArchive.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                addLoanScheduleDailyArchive.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                addLoanScheduleDailyArchive.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                addLoanScheduleDailyArchive.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                addLoanScheduleDailyArchive.InterestRate = item.norminalInterestRate;

                addLoanScheduleDailyArchive.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                addLoanScheduleDailyArchive.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                addLoanScheduleDailyArchive.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                addLoanScheduleDailyArchive.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                addLoanScheduleDailyArchive.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                addLoanScheduleDailyArchive.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                addLoanScheduleDailyArchive.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                addLoanScheduleDailyArchive.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                addLoanScheduleDailyArchive.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                addLoanScheduleDailyArchive.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                addLoanScheduleDailyArchive.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                addLoanScheduleDailyArchive.EarnedFee = Convert.ToDecimal(item.earnedFee);
                addLoanScheduleDailyArchive.EffectiveInterestRate = item.effectiveInterestRate;
                addLoanScheduleDailyArchive.NumberOfPeriods = item.numberOfPeriods;
                addLoanScheduleDailyArchive.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                addLoanScheduleDailyArchive.CreatedBy = item.createdBy;
                addLoanScheduleDailyArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanScheduleDailyArchive.ArchiveDate = generalSetup.GetApplicationDate();
                addLoanScheduleDailyArchive.ArchiveBatchCode = batchCode;

                loanScheduleDailyArchive.Add(addLoanScheduleDailyArchive);

            }

            this.context.tbl_Loan_Schedule_Daily_Archive.AddRange(loanScheduleDailyArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule (int loanId, DateTime applicationDate)
        {


            int no = 0;//number.Count() - 1;

            var model = (from a in context.tbl_Loan_Schedule_Periodic_Archive
                         where a.LoanId == loanId && a.PaymentDate < DbFunctions.TruncateTime(applicationDate)
                         orderby a.PaymentNumber ascending
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LoanId,
                             paymentNumber = a.PaymentNumber,
                             paymentDate = a.PaymentDate,
                             startPrincipalAmount = (double)a.StartPrincipalAmount,
                             periodPaymentAmount = (double)a.PeriodPaymentAmount,
                             periodInterestAmount = (double)a.PeriodInterestAmount,
                             periodPrincipalAmount = (double)a.PeriodPrincipalAmount,
                             endPrincipalAmount = (double)a.EndPrincipalAmount,
                             interestRate = a.InterestRate,
                             amortisedStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                             amortisedPeriodPaymentAmount = (double)a.AmortisedPeriodPaymentAmount,
                             amortisedPeriodInterestAmount = (double)a.AmortisedPeriodInterestAmount,
                             amortisedPeriodPrincipalAmount = (double)a.AmortisedPeriodPrincipalAmount,
                             amortisedEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                             effectiveInterestRate = a.EffectiveInterestRate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,
                         }).Concat
                         (from a in context.tbl_Loan_Schedule_Periodic_Temp
                          where a.LoanId == loanId && a.PaymentDate >= DbFunctions.TruncateTime(applicationDate)
                          && a.PaymentNumber != 0
                          orderby a.PaymentNumber ascending
                          select new LoanPaymentSchedulePeriodicViewModel()
                          {
                              loanId = a.LoanId,
                              paymentNumber = a.PaymentNumber,//lastNo + 1,
                              paymentDate = a.PaymentDate,
                              startPrincipalAmount = (double)a.StartPrincipalAmount,
                              periodPaymentAmount = (double)a.PeriodPaymentAmount,
                              periodInterestAmount = (double)a.PeriodInterestAmount,
                              periodPrincipalAmount = (double)a.PeriodPrincipalAmount,
                              endPrincipalAmount = (double)a.EndPrincipalAmount,
                              interestRate = a.InterestRate,
                              amortisedStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                              amortisedPeriodPaymentAmount = (double)a.AmortisedPeriodPaymentAmount,
                              amortisedPeriodInterestAmount = (double)a.AmortisedPeriodInterestAmount,
                              amortisedPeriodPrincipalAmount = (double)a.AmortisedPeriodPrincipalAmount,
                              amortisedEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                              effectiveInterestRate = a.EffectiveInterestRate,
                              createdBy = a.CreatedBy,
                              dateTimeCreated = a.DateTimeCreated,
                          }).ToList();

            List<tbl_Loan_Schedule_Periodic> loanSchedulePeriodic = new List<tbl_Loan_Schedule_Periodic>();



            foreach (var item in model)
            {
                tbl_Loan_Schedule_Periodic addLoanSchedulePeriodic = new tbl_Loan_Schedule_Periodic();


                addLoanSchedulePeriodic.LoanId = item.loanId;
                addLoanSchedulePeriodic.PaymentNumber = ++no -1;//item.paymentNumber;
                addLoanSchedulePeriodic.PaymentDate = item.paymentDate;
                addLoanSchedulePeriodic.StartPrincipalAmount = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodic.PeriodPaymentAmount = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodic.PeriodInterestAmount = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodic.PeriodPrincipalAmount = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodic.EndPrincipalAmount = (decimal)item.endPrincipalAmount;

                addLoanSchedulePeriodic.InterestRate = item.interestRate;
                addLoanSchedulePeriodic.AmortisedStartPrincipalAmount = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodic.AmortisedPeriodPaymentAmount = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodic.AmortisedPeriodInterestAmount = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodic.AmortisedPeriodPrincipalAmount = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodic.AmortisedEndPrincipalAmount = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodic.EffectiveInterestRate = item.effectiveInterestRate;
                addLoanSchedulePeriodic.CreatedBy = item.createdBy;
                addLoanSchedulePeriodic.DateTimeCreated = item.dateTimeCreated;
              


                loanSchedulePeriodic.Add(addLoanSchedulePeriodic);

            }

            //var itemToRemove = context.tbl_Loan_Schedule_Periodic.SingleOrDefault(x => x.LoanId == loanId);//  confirm if this code will delete all data with loanId

            //if (itemToRemove != null)
            //{
            //    context.tbl_Loan_Schedule_Periodic.Remove(itemToRemove);
            //    context.SaveChanges();
            //}

            var itemToRemove = (from p in context.tbl_Loan_Schedule_Periodic
                                where p.LoanId == loanId
                                select p);

            //itemToRemove.Delete();

            if (itemToRemove != null)
            {
                context.tbl_Loan_Schedule_Periodic.RemoveRange(itemToRemove);
                context.SaveChanges();
            }

            this.context.tbl_Loan_Schedule_Periodic.AddRange(loanSchedulePeriodic);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicScheduleForInterest(int loanId, DateTime applicationDate)
        {


            int no = 0;//number.Count() - 1;

            var model = (from a in context.tbl_Loan_Schedule_Periodic_Archive
                         where a.LoanId == loanId && a.PaymentDate < DbFunctions.TruncateTime(applicationDate)
                         orderby a.PaymentNumber ascending
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LoanId,
                             paymentNumber = a.PaymentNumber,
                             paymentDate = a.PaymentDate,
                             startPrincipalAmount = (double)a.StartPrincipalAmount,
                             periodPaymentAmount = (double)a.PeriodPaymentAmount,
                             periodInterestAmount = (double)a.PeriodInterestAmount,
                             periodPrincipalAmount = (double)a.PeriodPrincipalAmount,
                             endPrincipalAmount = (double)a.EndPrincipalAmount,
                             interestRate = a.InterestRate,
                             amortisedStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                             amortisedPeriodPaymentAmount = (double)a.AmortisedPeriodPaymentAmount,
                             amortisedPeriodInterestAmount = (double)a.AmortisedPeriodInterestAmount,
                             amortisedPeriodPrincipalAmount = (double)a.AmortisedPeriodPrincipalAmount,
                             amortisedEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                             effectiveInterestRate = a.EffectiveInterestRate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,
                         }).Concat
                         (from a in context.tbl_Loan_Schedule_Periodic_Temp
                          join b in context.tbl_Loan_Schedule_Periodic_Archive on a.LoanId equals b.LoanId
                          where a.LoanId == loanId && a.PaymentDate == b.PaymentDate && a.PaymentDate >= DbFunctions.TruncateTime(applicationDate)
                          && a.PaymentNumber != 0
                          orderby a.PaymentNumber ascending
                          select new LoanPaymentSchedulePeriodicViewModel()
                          {
                              loanId = a.LoanId,
                              paymentNumber = a.PaymentNumber,//lastNo + 1,
                              paymentDate = a.PaymentDate,
                              startPrincipalAmount = (double)a.StartPrincipalAmount,
                              periodPaymentAmount = (double)a.PeriodPaymentAmount,
                              periodInterestAmount = (double)a.PeriodInterestAmount,
                              periodPrincipalAmount = (double)a.PeriodPrincipalAmount,
                              endPrincipalAmount = (double)a.EndPrincipalAmount,
                              interestRate = a.InterestRate,
                              amortisedStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                              amortisedPeriodPaymentAmount = (double)a.AmortisedPeriodPaymentAmount,
                              amortisedPeriodInterestAmount = (double)a.AmortisedPeriodInterestAmount,
                              amortisedPeriodPrincipalAmount = (double)a.AmortisedPeriodPrincipalAmount,
                              amortisedEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                              effectiveInterestRate = a.EffectiveInterestRate,
                              createdBy = a.CreatedBy,
                              dateTimeCreated = a.DateTimeCreated,
                          }).ToList();

            List<tbl_Loan_Schedule_Periodic> loanSchedulePeriodic = new List<tbl_Loan_Schedule_Periodic>();



            foreach (var item in model)
            {
                tbl_Loan_Schedule_Periodic addLoanSchedulePeriodic = new tbl_Loan_Schedule_Periodic();


                addLoanSchedulePeriodic.LoanId = item.loanId;
                addLoanSchedulePeriodic.PaymentNumber = ++no - 1;//item.paymentNumber;
                addLoanSchedulePeriodic.PaymentDate = item.paymentDate;
                addLoanSchedulePeriodic.StartPrincipalAmount = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodic.PeriodPaymentAmount = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodic.PeriodInterestAmount = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodic.PeriodPrincipalAmount = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodic.EndPrincipalAmount = (decimal)item.endPrincipalAmount;

                addLoanSchedulePeriodic.InterestRate = item.interestRate;
                addLoanSchedulePeriodic.AmortisedStartPrincipalAmount = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodic.AmortisedPeriodPaymentAmount = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodic.AmortisedPeriodInterestAmount = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodic.AmortisedPeriodPrincipalAmount = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodic.AmortisedEndPrincipalAmount = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodic.EffectiveInterestRate = item.effectiveInterestRate;
                addLoanSchedulePeriodic.CreatedBy = item.createdBy;
                addLoanSchedulePeriodic.DateTimeCreated = item.dateTimeCreated;



                loanSchedulePeriodic.Add(addLoanSchedulePeriodic);

            }

            //var itemToRemove = context.tbl_Loan_Schedule_Periodic.SingleOrDefault(x => x.LoanId == loanId);//  confirm if this code will delete all data with loanId

            //if (itemToRemove != null)
            //{
            //    context.tbl_Loan_Schedule_Periodic.Remove(itemToRemove);
            //    context.SaveChanges();
            //}

            var itemToRemove = (from p in context.tbl_Loan_Schedule_Periodic
                                where p.LoanId == loanId
                                select p);

            //itemToRemove.Delete();

            if (itemToRemove != null)
            {
                context.tbl_Loan_Schedule_Periodic.RemoveRange(itemToRemove);
                context.SaveChanges();
            }

            this.context.tbl_Loan_Schedule_Periodic.AddRange(loanSchedulePeriodic);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool UpdateLoanInterestSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
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

                List<tbl_Loan_Schedule_Periodic_Temp> tblPeriodicScheduleTemp = new List<tbl_Loan_Schedule_Periodic_Temp>();
                foreach (var item in periodicScheduleTemp)
                {
                    tbl_Loan_Schedule_Periodic_Temp scheduleTemp = new tbl_Loan_Schedule_Periodic_Temp();

                    scheduleTemp.LoanId = loanId;
                    scheduleTemp.PaymentNumber = item.paymentNumber;
                    scheduleTemp.PaymentDate = item.paymentDate;
                    scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.PeriodPaymentAmount = Convert.ToDecimal(item.periodPaymentAmount);
                    scheduleTemp.PeriodInterestAmount = Convert.ToDecimal(item.periodInterestAmount);
                    scheduleTemp.PeriodPrincipalAmount = Convert.ToDecimal(item.periodPrincipalAmount);
                    scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.InterestRate = loanInput.interestRate;

                    scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                    scheduleTemp.AmortisedPeriodPaymentAmount = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                    scheduleTemp.AmortisedPeriodInterestAmount = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                    scheduleTemp.AmortisedPeriodPrincipalAmount = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                    scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                    scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                    scheduleTemp.CreatedBy = staffId;
                    scheduleTemp.DateTimeCreated = systemDate;

                    tblPeriodicScheduleTemp.Add(scheduleTemp);
                }
                //-------------------------------------------------------------------------------------


                //----------generate and save daily loan schedule -----------------------------------
                List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                List<tbl_Loan_Schedule_Daily_Temp> tblDailyScheduleTemp = new List<tbl_Loan_Schedule_Daily_Temp>();

                foreach (var item in dailyScheduleTemp)
                {
                    tbl_Loan_Schedule_Daily_Temp scheduleTemp = new tbl_Loan_Schedule_Daily_Temp();

                    scheduleTemp.LoanId = loanId;
                    scheduleTemp.PaymentNumber = item.paymentNumber;
                    scheduleTemp.Date = item.date;
                    scheduleTemp.PaymentDate = item.paymentDate;
                    scheduleTemp.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                    scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                    scheduleTemp.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                    scheduleTemp.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                    scheduleTemp.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                    scheduleTemp.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                    scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                    scheduleTemp.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                    scheduleTemp.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                    scheduleTemp.InterestRate = item.norminalInterestRate;

                    scheduleTemp.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                    scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                    scheduleTemp.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                    scheduleTemp.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                    scheduleTemp.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                    scheduleTemp.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                    scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                    scheduleTemp.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                    scheduleTemp.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                    scheduleTemp.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                    scheduleTemp.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                    scheduleTemp.EarnedFee = Convert.ToDecimal(item.earnedFee);
                    scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                    scheduleTemp.NumberOfPeriods = item.numberOfPeriods;
                    scheduleTemp.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                    scheduleTemp.CreatedBy = staffId;
                    scheduleTemp.DateTimeCreated = systemDate;

                    tblDailyScheduleTemp.Add(scheduleTemp);
                }
                //----------------------------------------------------------------


                //------------adding records to the database--------------------------

                //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                this.context.tbl_Loan_Schedule_Periodic_Temp.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                this.context.tbl_Loan_Schedule_Daily_Temp.AddRange(tblDailyScheduleTemp); ////change to Temp table
                context.SaveChanges();
                //----------update loan details -----------------------------------
                var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
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
            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where b.tbl_Product.tbl_Product_Price_Index.ProductPriceIndexId == priceindexId
                         && b.LoanStatusId == (short)LoanStatusEnum.Active
                         && !context.tbl_Loan_PriceIndex_Exception.Any(d => d.LoanId == b.TermLoanId) // a.LoanId == loanId
                         select new LoanPaymentSchedulePeriodicViewModel()
                         {
                             loanId = a.LoanId,
                             paymentNumber = a.PaymentNumber,
                             paymentDate = a.PaymentDate,
                             startPrincipalAmount = (double)a.StartPrincipalAmount,
                             periodPaymentAmount = (double)a.PeriodPaymentAmount,
                             periodInterestAmount = (double)a.PeriodInterestAmount,
                             periodPrincipalAmount = (double)a.PeriodPrincipalAmount,
                             endPrincipalAmount = (double)a.EndPrincipalAmount,
                             interestRate = a.InterestRate,
                             amortisedStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                             amortisedPeriodPaymentAmount = (double)a.AmortisedPeriodPaymentAmount,
                             amortisedPeriodInterestAmount = (double)a.AmortisedPeriodInterestAmount,
                             amortisedPeriodPrincipalAmount = (double)a.AmortisedPeriodPrincipalAmount,
                             amortisedEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                             effectiveInterestRate = a.EffectiveInterestRate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,

                         }).ToList();

            List<tbl_Loan_Schedule_Periodic_Archive> loanSchedulePeriodicArchive = new List<tbl_Loan_Schedule_Periodic_Archive>();



            foreach (var item in model)
            {

                tbl_Loan_Schedule_Periodic_Archive addLoanSchedulePeriodicArchive = new tbl_Loan_Schedule_Periodic_Archive();


                addLoanSchedulePeriodicArchive.LoanId = item.loanId;
                addLoanSchedulePeriodicArchive.PaymentNumber = item.paymentNumber;
                addLoanSchedulePeriodicArchive.PaymentDate = item.paymentDate;
                addLoanSchedulePeriodicArchive.StartPrincipalAmount = (decimal)item.startPrincipalAmount;
                addLoanSchedulePeriodicArchive.PeriodPaymentAmount = (decimal)item.periodPaymentAmount;
                addLoanSchedulePeriodicArchive.PeriodInterestAmount = (decimal)item.periodInterestAmount;
                addLoanSchedulePeriodicArchive.PeriodPrincipalAmount = (decimal)item.periodPrincipalAmount;
                addLoanSchedulePeriodicArchive.EndPrincipalAmount = (decimal)item.endPrincipalAmount;


                addLoanSchedulePeriodicArchive.InterestRate = item.interestRate;
                addLoanSchedulePeriodicArchive.AmortisedStartPrincipalAmount = (decimal)item.amortisedStartPrincipalAmount;
                addLoanSchedulePeriodicArchive.AmortisedPeriodPaymentAmount = (decimal)item.amortisedPeriodPaymentAmount;
                addLoanSchedulePeriodicArchive.AmortisedPeriodInterestAmount = (decimal)item.amortisedPeriodInterestAmount;
                addLoanSchedulePeriodicArchive.AmortisedPeriodPrincipalAmount = (decimal)item.amortisedPeriodPrincipalAmount;
                addLoanSchedulePeriodicArchive.AmortisedEndPrincipalAmount = (decimal)item.amortisedEndPrincipalAmount;
                addLoanSchedulePeriodicArchive.EffectiveInterestRate = item.effectiveInterestRate;
                addLoanSchedulePeriodicArchive.CreatedBy = item.createdBy;
                addLoanSchedulePeriodicArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanSchedulePeriodicArchive.ArchiveDate = generalSetup.GetApplicationDate();
                addLoanSchedulePeriodicArchive.ArchiveBatchCode = batchCode;

                loanSchedulePeriodicArchive.Add(addLoanSchedulePeriodicArchive);

            }

            this.context.tbl_Loan_Schedule_Periodic_Archive.AddRange(loanSchedulePeriodicArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPaymentScheduleDailyViewModel> BulkArchiveDailySchedule(int priceindexId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan_Schedule_Daily
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where b.LoanStatusId == (short)LoanStatusEnum.Active
                         && !context.tbl_Loan_PriceIndex_Exception.Any(d => d.LoanId == b.TermLoanId)
                         select new LoanPaymentScheduleDailyViewModel()
                         {
                             loanId = a.LoanId,
                             paymentNumber = a.PaymentNumber,
                             date = a.Date,
                             paymentDate = a.PaymentDate,
                             openingBalance = (double)a.OpeningBalance,
                             startPrincipalAmount = (double)a.StartPrincipalAmount,
                             dailyPaymentAmount = (double)a.DailyPaymentAmount,
                             dailyInterestAmount = (double)a.DailyInterestAmount,
                             dailyPrincipalAmount = (double)a.DailyPrincipalAmount,
                             closingBalance = (double)a.ClosingBalance,
                             endPrincipalAmount = (double)a.EndPrincipalAmount,
                             amortisedCost = (double)a.AmortisedCost,
                             accruedInterest = (double)a.AccruedInterest,
                             norminalInterestRate = a.InterestRate,

                             amOpeningBalance = (double)a.AmortisedOpeningBalance,
                             amStartPrincipalAmount = (double)a.AmortisedStartPrincipalAmount,
                             amDailyPaymentAmount = (double)a.AmortisedDailyPaymentAmount,
                             amDailyInterestAmount = (double)a.AmortisedDailyInterestAmount,
                             amDailyPrincipalAmount = (double)a.AmortisedDailyPrincipalAmount,
                             amClosingBalance = (double)a.AmortisedClosingBalance,
                             amEndPrincipalAmount = (double)a.AmortisedEndPrincipalAmount,
                             amAccruedInterest = (double)a.AmortisedAccruedInterest,
                             amAmortisedCost = (double)a.Amortised_AmortisedCost,
                             discountPremium = (double)a.DiscountPremium,
                             unEarnedFee = (double)a.UnEarnedFee,
                             earnedFee = (double)a.EarnedFee,
                             effectiveInterestRate = a.EffectiveInterestRate,
                             numberOfPeriods = a.NumberOfPeriods,
                             ballonAmount = (double)a.BallonAmount,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,


                         }).ToList();

            List<tbl_Loan_Schedule_Daily_Archive> loanScheduleDailyArchive = new List<tbl_Loan_Schedule_Daily_Archive>();



            foreach (var item in model)
            {
                tbl_Loan_Schedule_Daily_Archive addLoanScheduleDailyArchive = new tbl_Loan_Schedule_Daily_Archive();


                addLoanScheduleDailyArchive.LoanId = item.loanId;
                addLoanScheduleDailyArchive.PaymentNumber = item.paymentNumber;
                addLoanScheduleDailyArchive.Date = item.date;
                addLoanScheduleDailyArchive.PaymentDate = item.paymentDate;
                addLoanScheduleDailyArchive.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                addLoanScheduleDailyArchive.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                addLoanScheduleDailyArchive.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                addLoanScheduleDailyArchive.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                addLoanScheduleDailyArchive.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                addLoanScheduleDailyArchive.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                addLoanScheduleDailyArchive.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                addLoanScheduleDailyArchive.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                addLoanScheduleDailyArchive.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                addLoanScheduleDailyArchive.InterestRate = item.norminalInterestRate;

                addLoanScheduleDailyArchive.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                addLoanScheduleDailyArchive.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                addLoanScheduleDailyArchive.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                addLoanScheduleDailyArchive.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                addLoanScheduleDailyArchive.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                addLoanScheduleDailyArchive.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                addLoanScheduleDailyArchive.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                addLoanScheduleDailyArchive.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                addLoanScheduleDailyArchive.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                addLoanScheduleDailyArchive.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                addLoanScheduleDailyArchive.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                addLoanScheduleDailyArchive.EarnedFee = Convert.ToDecimal(item.earnedFee);
                addLoanScheduleDailyArchive.EffectiveInterestRate = item.effectiveInterestRate;
                addLoanScheduleDailyArchive.NumberOfPeriods = item.numberOfPeriods;
                addLoanScheduleDailyArchive.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                addLoanScheduleDailyArchive.CreatedBy = item.createdBy;
                addLoanScheduleDailyArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanScheduleDailyArchive.ArchiveDate = generalSetup.GetApplicationDate();
                addLoanScheduleDailyArchive.ArchiveBatchCode = batchCode;

                loanScheduleDailyArchive.Add(addLoanScheduleDailyArchive);

            }

            this.context.tbl_Loan_Schedule_Daily_Archive.AddRange(loanScheduleDailyArchive);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanViewModel> BulkArchiveLoan(int priceindexId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan
                         where a.LoanStatusId == (short)LoanStatusEnum.Active
                          && !context.tbl_Loan_PriceIndex_Exception.Any(d => d.LoanId == a.TermLoanId)
                         //where !context.tbl_Loan_PriceIndex_Exception.Any(d => d.LoanId == a.TermLoanId)

                         select new LoanViewModel()
                         {
                             loanId = a.TermLoanId,
                             productPriceIndexRate = a.ProductPriceIndexRate,
                             customerRiskRatingId = a.CustomerRiskRatingId,
                             customerId = a.CustomerId,
                             productId = a.ProductId,
                             companyId = a.CompanyId,
                             casaAccountId = a.CasaAccountId,
                             branchId = a.BranchId,
                             currencyId = a.CurrencyId,
                             exchangeRate = a.ExchangeRate,
                             loanApplicationDetailId = a.LoanApplicationDetailId,
                             loanReferenceNumber = a.LoanReferenceNumber,
                             subSectorId = a.SubSectorId,
                             principalFrequencyTypeId = a.PrincipalFrequencyTypeId,
                             interestFrequencyTypeId = a.InterestFrequencyTypeId,
                             principalNumberOfInstallment = a.PrincipalNumberOfInstallment,
                             interestNumberOfInstallment = a.InterestNumberOfInstallment,
                             relationshipOfficerId = a.RelationshipOfficerId,
                             relationshipManagerId = a.RelationshipManagerId,
                             misCode = a.MISCode,
                             teamMiscode = a.TeamMISCode,
                             interestRate = a.InterestRate,
                             effectiveDate = a.EffectiveDate,
                             maturityDate = a.MaturityDate,
                             bookingDate = a.BookingDate,
                             principalAmount = a.PrincipalAmount,

                             principalInstallmentLeft = a.PrincipalInstallmentLeft,
                             interestInstallmentLeft = a.InterestInstallmentLeft,
                             approvalStatusId = a.ApprovalStatusId,
                             approvedBy = a.ApprovedBy,
                             approverComment = a.ApproverComment,
                             dateApproved = a.DateApproved,
                             loanStatusId = a.LoanStatusId,
                             scheduleTypeId = a.ScheduleTypeId,
                             scheduleDayCountConventionId = a.ScheduleDayCountConventionId,
                             scheduleDayInterestTypeId = a.ScheduleDayInterestTypeId,
                             isDisbursed = a.IsDisbursed,
                             disbursedBy = a.DisbursedBy,
                             disburserComment = a.DisburserComment,
                             disburseDate = a.DisburseDate,
                             operationId = a.OperationId,
                             customerGroupId = a.CustomerGroupId,
                             loanTypeId = a.LoanTypeId,
                             equityContribution = a.EquityContribution,
                             firstPrincipalPaymentDate = a.FirstPrincipalPaymentDate,
                             firstInterestPaymentDate = a.FirstInterestPaymentDate,
                             outstandingPrincipal = a.OutstandingPrincipal,
                             outstandingInterest = a.OutstandingInterest,
                             principalAdditionCount = a.PrincipalAdditionCount,
                             principalReductionCount = a.PrincipalReductionCount,
                             fixedPrincipal = a.FixedPrincipal,
                             profileLoan = a.ProfileLoan,
                             dischargeLetter = a.DischargeLetter,
                             suspendInterest = a.SuspendInterest,
                             isScheduledPrepayment = a.IsScheduledPrepayment,
                             allowForceDebitRepayment = a.AllowForceDebitRepayment,
                             scheduledPrepaymentAmount = a.ScheduledPrepaymentAmount,
                             scheduledPrepaymentDate = a.ScheduledPrepaymentDate,
                             scheduledPrepaymentFrequencyTypeId = a.ScheduledPrepaymentFrequencyTypeId,
                             customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                             internalPrudentialGuidelineStatusId = a.InternalPrudentialGuidelineStatusId,
                             externalPrudentialGuidelineStatusId = a.ExternalPrudentialGuidelineStatusId,
                             nplDate = a.NPLDate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,

                         }).ToList();

            List<tbl_Loan_Archive> loanArchive = new List<tbl_Loan_Archive>();



            foreach (var item in model)
            {

                tbl_Loan_Archive addLoanArchive = new tbl_Loan_Archive();


                addLoanArchive.ChangeEffectiveDate = DateTime.Today;
                addLoanArchive.IsApplied = false;
                addLoanArchive.ChangeReason = "Rephasement";
                addLoanArchive.LoanId = item.loanId;
                addLoanArchive.ProductPriceIndexRate = item.productPriceIndexRate;
                addLoanArchive.CustomerRiskRatingId = item.customerRiskRatingId;
                addLoanArchive.CustomerId = item.customerId;
                addLoanArchive.ProductId = item.productId;
                addLoanArchive.CompanyId = item.companyId;
                addLoanArchive.CasaAccountId = item.casaAccountId;
                addLoanArchive.BranchId = item.branchId;
                addLoanArchive.CurrencyId = (short)item.currencyId;
                addLoanArchive.ExchangeRate = item.exchangeRate;
                addLoanArchive.LoanApplicationDetailId = item.loanApplicationDetailId;
                addLoanArchive.LoanReferenceNumber = item.loanReferenceNumber;
                addLoanArchive.SubSectorId = item.subSectorId;
                addLoanArchive.PrincipalFrequencyTypeId = item.principalFrequencyTypeId;
                addLoanArchive.InterestFrequencyTypeId = item.interestFrequencyTypeId;
                addLoanArchive.PrincipalNumberOfInstallment = item.principalNumberOfInstallment;
                addLoanArchive.InterestNumberOfInstallment = item.interestNumberOfInstallment;
                addLoanArchive.RelationshipOfficerId = item.relationshipOfficerId;
                addLoanArchive.RelationshipManagerId = item.relationshipManagerId;
                addLoanArchive.MISCode = item.misCode;
                addLoanArchive.TeamMISCode = item.teamMiscode;
                addLoanArchive.InterestRate = item.interestRate;
                addLoanArchive.EffectiveDate = item.effectiveDate;
                addLoanArchive.MaturityDate = item.maturityDate;
                addLoanArchive.BookingDate = item.bookingDate;
                addLoanArchive.PrincipalAmount = item.principalAmount;
                addLoanArchive.PrincipalInstallmentLeft = item.principalInstallmentLeft;
                addLoanArchive.InterestInstallmentLeft = item.interestInstallmentLeft;
                addLoanArchive.ApprovalStatusId = item.approvalStatusId;
                addLoanArchive.ApprovedBy = item.approvedBy;
                addLoanArchive.ApproverComment = item.approverComment;
                addLoanArchive.DateApproved = item.dateApproved;
                addLoanArchive.LoanStatusId = item.loanStatusId;
                addLoanArchive.CreatedBy = item.createdBy;
                addLoanArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanArchive.ScheduleTypeId = item.scheduleTypeId;
                addLoanArchive.ScheduleDayCountConventionId = item.scheduleDayCountConventionId;
                addLoanArchive.ScheduleDayInterestTypeId = item.scheduleDayInterestTypeId;
                addLoanArchive.IsDisbursed = item.isDisbursed;
                addLoanArchive.DisbursedBy = item.disbursedBy;
                addLoanArchive.DisburserComment = item.disburserComment;
                addLoanArchive.DisburseDate = item.disburseDate;
                addLoanArchive.OperationId = (int)item.operationId;
                addLoanArchive.CustomerGroupId = item.customerGroupId;
                addLoanArchive.LoanTypeId = item.loanTypeId;
                //addLoanArchive.TrancheBatchCode = item.trancheBatchCode;
                addLoanArchive.EquityContribution = item.equityContribution;
                addLoanArchive.FirstPrincipalPaymentDate = item.firstPrincipalPaymentDate;
                addLoanArchive.FirstInterestPaymentDate = item.firstInterestPaymentDate;
                addLoanArchive.OutstandingPrincipal = item.outstandingPrincipal;
                addLoanArchive.OutstandingInterest = item.outstandingInterest;
                addLoanArchive.PrincipalAdditionCount = item.principalAdditionCount;
                addLoanArchive.PrincipalReductionCount = item.principalReductionCount;
                addLoanArchive.FixedPrincipal = item.fixedPrincipal;
                addLoanArchive.ProfileLoan = item.profileLoan;
                addLoanArchive.DischargeLetter = item.dischargeLetter;
                addLoanArchive.SuspendInterest = item.suspendInterest;
                addLoanArchive.IsScheduledPrepayment = item.isScheduledPrepayment;
                addLoanArchive.AllowForceDebitRepayment = item.allowForceDebitRepayment;
                addLoanArchive.ScheduledPrepaymentAmount = item.scheduledPrepaymentAmount;
                addLoanArchive.ScheduledPrepaymentDate = item.scheduledPrepaymentDate;
                addLoanArchive.ScheduledPrepaymentFrequencyTypeId = item.principalFrequencyTypeId;//scheduledPrepaymentFrequencyTypeId;
                addLoanArchive.CustomerSensitivityLevelId = item.customerSensitivityLevelId;
                addLoanArchive.InternalPrudentialGuidelineStatusId = 1; //item.internalPrudentialGuidelineStatusId;
                addLoanArchive.ExternalPrudentialGuidelineStatusId = 1; //item.externalPrudentialGuidelineStatusId;
                addLoanArchive.NPLDate = item.nplDate;
                addLoanArchive.CreatedBy = item.createdBy;
                addLoanArchive.DateTimeCreated = item.dateTimeCreated;

                loanArchive.Add(addLoanArchive);

            }

            this.context.tbl_Loan_Archive.AddRange(loanArchive);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddLoanScheduleByBulkRate(short priceindexId, double newRate, DateTime applicationDate, int staffId)
        {
            bool output = false;
            // var applicationDate = generalSetup.GetApplicationDate();

            BulkArchiveLoan(priceindexId);
            BulkArchivePeriodicSchedule(priceindexId);
            BulkArchiveDailySchedule(priceindexId);

            var currentRate = this.context.tbl_Product_Price_Index.Where(x => x.ProductPriceIndexId == priceindexId).FirstOrDefault().PriceIndexRate;

            var rateChange = newRate - currentRate;

            var model = (
                         from a in context.tbl_Loan
                         where a.tbl_Product.tbl_Product_Price_Index.ProductPriceIndexId == priceindexId
                         && a.LoanStatusId == (short)LoanStatusEnum.Active // a.LoanId == loanId

                         select new LoanPaymentScheduleInputViewModel()
                         {
                             loanId = a.TermLoanId,
                             scheduleMethodId = a.ScheduleTypeId,
                             principalAmount = (double)a.OutstandingPrincipal,
                             principalFrequency = (short)a.PrincipalFrequencyTypeId,
                             interestFrequency = (short)a.InterestFrequencyTypeId,
                             tenor = (int)(a.MaturityDate - applicationDate).TotalDays,
                             principalFirstpaymentDate = (DateTime)a.FirstPrincipalPaymentDate,
                             interestFirstpaymentDate = (DateTime)a.FirstInterestPaymentDate,
                             interestRate = a.InterestRate + rateChange,
                             effectiveDate = applicationDate,
                             maturityDate = a.MaturityDate,
                             accurialBasis = a.ScheduleDayCountConventionId,
                             firstDayType = a.ScheduleDayInterestTypeId,
                             integralFeeAmount = 0,
                         }).ToList();

            foreach (var item in model)
            {
                var unEarnedFee = from d in context.tbl_Loan_Schedule_Daily
                                  where d.LoanId == item.loanId
                                  let sumUnEarnedFee = context.tbl_Loan_Schedule_Daily.Where(a => a.LoanId == item.loanId
                                  && a.Date >= DbFunctions.TruncateTime(applicationDate)).Sum(a => a.UnEarnedFee)
                                  select sumUnEarnedFee;
                item.integralFeeAmount = (double)unEarnedFee.FirstOrDefault();

                UpdateLoanInterestSchedule(item.loanId, item, applicationDate, staffId);
            }

            context.SaveChanges();
            //-------------------------------------------------------
            output = true;

            return output;
        }
        //----------------------------------Bulk Rate Revision End-------------------------------------
        public void updateloanTableStatus(int loanId)
        {
            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.LoanStatusId = (short)LoanStatusEnum.Completed;



            context.SaveChanges();
        }

        public void updateLoanReviewOperation (short loanReviewOperationsId, int loanId)
        {
            tbl_Loan_Review_Operation result = (from p in context.tbl_Loan_Review_Operation
                               where p.LoanId == loanId && p.LoanReviewOperationId == loanReviewOperationsId
                                                select p).SingleOrDefault();

            result.OperationCompleted = true;



            context.SaveChanges();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool UpdateLoanPrepaymentSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var penalCharge = context.tbl_Charge_Fee.FirstOrDefault(x => x.OperationId == (int)OperationsEnum.Prepayment);
            if (loanInput.principalAmount == loanInput.payAmount)
            {
                var refNo = this.context.tbl_Loan.Where(x => x.TermLoanId == loanInput.loanId).FirstOrDefault().LoanReferenceNumber;
                var interest = from d in context.tbl_Daily_Accrual
                                      where d.ReferenceNumber == refNo
                                      let sumDailyAccuralAmount  = context.tbl_Daily_Accrual.Where(a => a.ReferenceNumber == refNo
                                      && a.Date <= DbFunctions.TruncateTime(applicationDate) && a.RepaymentPostedStatus == false).Sum(a => a.DailyAccuralAmount)/// add repaymentpostedstatus = false after scaffording
                                      select sumDailyAccuralAmount ;
                var accruedInterest = interest.FirstOrDefault();

                var penalAmount = loanInput.principalAmount * (penalCharge.Rate / 100);

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "Accrued Interest"));

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)loanInput.payAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)penalAmount, penalCharge.GLAccountId, "Penal Charge"));///change to charge GL

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


                    //----------generate and save periodic loan schedule -----------------------------------

                    loanInput.principalAmount = loanInput.newAmount;

                    List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Periodic_Temp> tblPeriodicScheduleTemp = new List<tbl_Loan_Schedule_Periodic_Temp>();
                    foreach (var item in periodicScheduleTemp)
                    {
                        tbl_Loan_Schedule_Periodic_Temp scheduleTemp = new tbl_Loan_Schedule_Periodic_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.PeriodPaymentAmount = Convert.ToDecimal(item.periodPaymentAmount);
                        scheduleTemp.PeriodInterestAmount = Convert.ToDecimal(item.periodInterestAmount);
                        scheduleTemp.PeriodPrincipalAmount = Convert.ToDecimal(item.periodPrincipalAmount);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.InterestRate = loanInput.interestRate;

                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                        scheduleTemp.AmortisedPeriodPaymentAmount = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                        scheduleTemp.AmortisedPeriodInterestAmount = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                        scheduleTemp.AmortisedPeriodPrincipalAmount = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblPeriodicScheduleTemp.Add(scheduleTemp);
                    }
                    //-------------------------------------------------------------------------------------


                    //----------generate and save daily loan schedule -----------------------------------

                    //loanInput.principalAmount = loanInput.newAmount;
                    List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Daily_Temp> tblDailyScheduleTemp = new List<tbl_Loan_Schedule_Daily_Temp>();

                    foreach (var item in dailyScheduleTemp)
                    {
                        tbl_Loan_Schedule_Daily_Temp scheduleTemp = new tbl_Loan_Schedule_Daily_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.Date = item.date;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                        scheduleTemp.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                        scheduleTemp.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                        scheduleTemp.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                        scheduleTemp.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                        scheduleTemp.InterestRate = item.norminalInterestRate;

                        scheduleTemp.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                        scheduleTemp.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                        scheduleTemp.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                        scheduleTemp.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                        scheduleTemp.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                        scheduleTemp.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                        scheduleTemp.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                        scheduleTemp.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                        scheduleTemp.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                        scheduleTemp.EarnedFee = Convert.ToDecimal(item.earnedFee);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.NumberOfPeriods = item.numberOfPeriods;
                        scheduleTemp.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblDailyScheduleTemp.Add(scheduleTemp);
                    }
                    //----------------------------------------------------------------


                    //------------adding records to the database--------------------------

                    //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                    //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.tbl_Loan_Schedule_Periodic_Temp.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.tbl_Loan_Schedule_Daily_Temp.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    context.SaveChanges();

                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    MergePeriodicSchedule(loanId, applicationDate);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)loanInput.newAmount, penalCharge.GLAccountId, "Penal Charge"));
                    financeTransaction.PostTransaction(inputTransactions);
                    context.SaveChanges();
                    //-------------------------------------------------------
                }

            }


            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PaymentFrequencyChange(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
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

                    loanInput.principalFrequency = loanInput.newPrincipalFrequency;
                    loanInput.interestFrequency = loanInput.newInterestFrequency;

                    List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Periodic_Temp> tblPeriodicScheduleTemp = new List<tbl_Loan_Schedule_Periodic_Temp>();
                    foreach (var item in periodicScheduleTemp)
                    {
                        tbl_Loan_Schedule_Periodic_Temp scheduleTemp = new tbl_Loan_Schedule_Periodic_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.PeriodPaymentAmount = Convert.ToDecimal(item.periodPaymentAmount);
                        scheduleTemp.PeriodInterestAmount = Convert.ToDecimal(item.periodInterestAmount);
                        scheduleTemp.PeriodPrincipalAmount = Convert.ToDecimal(item.periodPrincipalAmount);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.InterestRate = loanInput.interestRate;

                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                        scheduleTemp.AmortisedPeriodPaymentAmount = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                        scheduleTemp.AmortisedPeriodInterestAmount = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                        scheduleTemp.AmortisedPeriodPrincipalAmount = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblPeriodicScheduleTemp.Add(scheduleTemp);
                    }
                    //-------------------------------------------------------------------------------------


                    //----------generate and save daily loan schedule -----------------------------------

                    //loanInput.principalAmount = loanInput.newAmount;
                    List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Daily_Temp> tblDailyScheduleTemp = new List<tbl_Loan_Schedule_Daily_Temp>();

                    foreach (var item in dailyScheduleTemp)
                    {
                        tbl_Loan_Schedule_Daily_Temp scheduleTemp = new tbl_Loan_Schedule_Daily_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.Date = item.date;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                        scheduleTemp.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                        scheduleTemp.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                        scheduleTemp.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                        scheduleTemp.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                        scheduleTemp.InterestRate = item.norminalInterestRate;

                        scheduleTemp.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                        scheduleTemp.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                        scheduleTemp.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                        scheduleTemp.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                        scheduleTemp.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                        scheduleTemp.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                        scheduleTemp.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                        scheduleTemp.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                        scheduleTemp.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                        scheduleTemp.EarnedFee = Convert.ToDecimal(item.earnedFee);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.NumberOfPeriods = item.numberOfPeriods;
                        scheduleTemp.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblDailyScheduleTemp.Add(scheduleTemp);
                    }
                    //----------------------------------------------------------------


                    //------------adding records to the database--------------------------

                    //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                    //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.tbl_Loan_Schedule_Periodic_Temp.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.tbl_Loan_Schedule_Daily_Temp.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    context.SaveChanges();
                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    MergePeriodicSchedule(loanId, applicationDate);

                    context.SaveChanges();
                    //-------------------------------------------------------
                }

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PaymentDateChange(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
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

                    List<tbl_Loan_Schedule_Periodic_Temp> tblPeriodicScheduleTemp = new List<tbl_Loan_Schedule_Periodic_Temp>();
                    foreach (var item in periodicScheduleTemp)
                    {
                        tbl_Loan_Schedule_Periodic_Temp scheduleTemp = new tbl_Loan_Schedule_Periodic_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.PeriodPaymentAmount = Convert.ToDecimal(item.periodPaymentAmount);
                        scheduleTemp.PeriodInterestAmount = Convert.ToDecimal(item.periodInterestAmount);
                        scheduleTemp.PeriodPrincipalAmount = Convert.ToDecimal(item.periodPrincipalAmount);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.InterestRate = loanInput.interestRate;

                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                        scheduleTemp.AmortisedPeriodPaymentAmount = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                        scheduleTemp.AmortisedPeriodInterestAmount = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                        scheduleTemp.AmortisedPeriodPrincipalAmount = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblPeriodicScheduleTemp.Add(scheduleTemp);
                    }
                    //-------------------------------------------------------------------------------------


                    //----------generate and save daily loan schedule -----------------------------------

                    //loanInput.principalAmount = loanInput.newAmount;
                    List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Daily_Temp> tblDailyScheduleTemp = new List<tbl_Loan_Schedule_Daily_Temp>();

                    foreach (var item in dailyScheduleTemp)
                    {
                        tbl_Loan_Schedule_Daily_Temp scheduleTemp = new tbl_Loan_Schedule_Daily_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.Date = item.date;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                        scheduleTemp.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                        scheduleTemp.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                        scheduleTemp.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                        scheduleTemp.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                        scheduleTemp.InterestRate = item.norminalInterestRate;

                        scheduleTemp.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                        scheduleTemp.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                        scheduleTemp.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                        scheduleTemp.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                        scheduleTemp.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                        scheduleTemp.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                        scheduleTemp.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                        scheduleTemp.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                        scheduleTemp.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                        scheduleTemp.EarnedFee = Convert.ToDecimal(item.earnedFee);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.NumberOfPeriods = item.numberOfPeriods;
                        scheduleTemp.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblDailyScheduleTemp.Add(scheduleTemp);
                    }
                    //----------------------------------------------------------------


                    //------------adding records to the database--------------------------

                    //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                    //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.tbl_Loan_Schedule_Periodic_Temp.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.tbl_Loan_Schedule_Daily_Temp.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    context.SaveChanges();
                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    MergePeriodicSchedule(loanId, applicationDate);

                    context.SaveChanges();
                    //-------------------------------------------------------
                }

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanReversal(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);

            var interestAmount  = (from p in context.tbl_Loan_Schedule_Daily
                               where p.Date <= DbFunctions.TruncateTime(applicationDate)
                                   select p).SingleOrDefault();

            var interest = from d in context.tbl_Loan_Schedule_Daily
                           where d.Date <= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                           let sumDailyAccuralAmount = context.tbl_Loan_Schedule_Daily.Where(a => a.Date <= DbFunctions.TruncateTime(applicationDate)
                           && d.LoanId == loanId).Sum(a => a.DailyInterestAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.tbl_Loan_Schedule_Periodic
                           where d.PaymentDate <= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                            let sumPrincipalAmount = context.tbl_Loan_Schedule_Periodic.Where(a => a.PaymentDate <= DbFunctions.TruncateTime(applicationDate)
                            && d.LoanId == loanId).Sum(a => a.PeriodPrincipalAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "interest repayment"));

            inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedPrincipal, product.PrincipalBalanceGL.Value, "principal repayment"));

            financeTransaction.PostTransaction(inputTransactions);



            ArchiveLoan(loanId, loanId);/////loanId change this to OperationId
            ArchivePeriodicSchedule(loanId);
            ArchiveDailySchedule(loanId);

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.LoanStatusId = (short)LoanStatusEnum.Terminated;



            //----------generate and save periodic loan schedule -----------------------------------
            List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

            List<tbl_Loan_Schedule_Periodic> tblPeriodicSchedule = new List<tbl_Loan_Schedule_Periodic>();
            foreach (var item in periodicSchedule)
            {
                tbl_Loan_Schedule_Periodic schedule = new tbl_Loan_Schedule_Periodic();

                schedule.LoanId = loanId;
                schedule.PaymentNumber = item.paymentNumber;
                schedule.PaymentDate = item.paymentDate;
                schedule.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.PeriodPaymentAmount = Convert.ToDecimal(item.periodPaymentAmount);
                schedule.PeriodInterestAmount = Convert.ToDecimal(item.periodInterestAmount);
                schedule.PeriodPrincipalAmount = Convert.ToDecimal(item.periodPrincipalAmount);
                schedule.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.InterestRate = loanInput.interestRate;
                schedule.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                schedule.AmortisedPeriodPaymentAmount = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                schedule.AmortisedPeriodInterestAmount = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                schedule.AmortisedPeriodPrincipalAmount = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                schedule.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                schedule.EffectiveInterestRate = item.effectiveInterestRate;
                schedule.CreatedBy = staffId;
                schedule.DateTimeCreated = systemDate;

                tblPeriodicSchedule.Add(schedule);
            }
            //-------------------------------------------------------------------------------------


            //----------generate and save daily loan schedule -----------------------------------

            //loanInput.principalAmount = loanInput.newAmount;
            List<LoanPaymentScheduleDailyViewModel> dailySchedule = loanSchedule.GenerateDailyLoanSchedule(loanInput);

            List<tbl_Loan_Schedule_Daily> tblDailySchedule = new List<tbl_Loan_Schedule_Daily>();

            foreach (var item in dailySchedule)
            {
                tbl_Loan_Schedule_Daily schedule = new tbl_Loan_Schedule_Daily();

                schedule.LoanId = loanId;
                schedule.PaymentNumber = item.paymentNumber;
                schedule.Date = item.date;
                schedule.PaymentDate = item.paymentDate;
                schedule.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                schedule.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                schedule.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                schedule.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                schedule.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                schedule.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                schedule.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                schedule.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                schedule.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                schedule.InterestRate = item.norminalInterestRate;
                schedule.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                schedule.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                schedule.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                schedule.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                schedule.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                schedule.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                schedule.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                schedule.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                schedule.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                schedule.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                schedule.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                schedule.EarnedFee = Convert.ToDecimal(item.earnedFee);
                schedule.EffectiveInterestRate = item.effectiveInterestRate;
                schedule.NumberOfPeriods = item.numberOfPeriods;
                schedule.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                schedule.CreatedBy = staffId;
                schedule.DateTimeCreated = systemDate;

                tblDailySchedule.Add(schedule);
            }
            //----------------------------------------------------------------


            //------------adding records to the database--------------------------

            this.context.tbl_Loan_Schedule_Periodic.AddRange(tblPeriodicSchedule);////change to Temp table

            this.context.tbl_Loan_Schedule_Daily.AddRange(tblDailySchedule); ////change to Temp table
            context.SaveChanges();
            //----------update loan details -----------------------------------
            var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
            loan.MaturityDate = periodicSchedule.Max(x => x.paymentDate);
            loan.PrincipalNumberOfInstallment = periodicSchedule.Count() - 1;
            loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
            //-------------------------------------------------

                    context.SaveChanges();
                    //-------------------------------------------------------
                
            

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool TerminateAndRebookLoanSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var refNo = this.context.tbl_Loan.Where(x => x.TermLoanId == loanInput.loanId).FirstOrDefault().LoanReferenceNumber;


            var interest = from d in context.tbl_Loan_Schedule_Daily
                           where d.Date >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                           let sumDailyAccuralAmount = context.tbl_Loan_Schedule_Daily.Where(a => a.Date >= DbFunctions.TruncateTime(applicationDate)
                           && d.LoanId == loanId).Sum(a => a.DailyInterestAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.tbl_Loan_Schedule_Periodic
                            where d.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                            let sumPrincipalAmount = context.tbl_Loan_Schedule_Periodic.Where(a => a.PaymentDate >= DbFunctions.TruncateTime(applicationDate)
                            && d.LoanId == loanId).Sum(a => a.PeriodPrincipalAmount)/// add repaymentpostedstatus = false after scaffording
                            select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId,loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "Loan Outstanding Interest Balance"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId,loanInput, accruedPrincipal, product.PrincipalBalanceGL.Value, "Loan Outstanding principal Balance"));

                financeTransaction.PostTransaction(inputTransactions);

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.LoanStatusId = (short)LoanStatusEnum.Terminated;

            context.SaveChanges();

            ///call disturbs loan method and posting



            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool CompleteWriteOff (int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var loan = this.context.tbl_Loan.Where(x => x.TermLoanId == loanInput.loanId).FirstOrDefault();

            var casa = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == loan.CasaAccountId);

            var interest = from d in context.tbl_Loan_Schedule_Daily
                           where d.Date >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                           let sumDailyAccuralAmount = context.tbl_Loan_Schedule_Daily.Where(a => a.Date >= DbFunctions.TruncateTime(applicationDate)
                           && d.LoanId == loanId).Sum(a => a.DailyInterestAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.tbl_Loan_Schedule_Periodic
                            where d.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                            let sumPrincipalAmount = context.tbl_Loan_Schedule_Periodic.Where(a => a.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId).Sum(a => a.PeriodPrincipalAmount)/// add repaymentpostedstatus = false after scaffording
                            select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            var sllp = 11;////Get SLLP GL

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, sllp, "Interest Write off"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedPrincipal, sllp, "principal Write off"));

            financeTransaction.PostTransaction(inputTransactions);

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.LoanStatusId = (short)LoanStatusEnum.WriteOff;

            ///call disturbs loan method and posting

            tbl_Loan_Camsol loanCamsol = new tbl_Loan_Camsol();

            loanCamsol.LoanId = loanId;
            loanCamsol.CompanyId = loanInput.companyId;
            loanCamsol.AmountAffected = accruedInterest + accruedPrincipal;
            loanCamsol.Date = applicationDate;
            loanCamsol.Type = "Loan Complete Write Off"; 

            this.context.tbl_Loan_Camsol.Add(loanCamsol); ////change to Temp table

            /// Place a Lien on Customer Repayment Account

            var data = new tbl_CASA_Lien
            {
                ProductAccountNumber = casa.ProductAccountNumber,
                LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                SourceReferenceNumber = loan.LoanReferenceNumber,
                BranchId = loan.BranchId,
                CompanyId = loan.CompanyId,
                LienCreditAmount = accruedInterest + accruedPrincipal,
                LienDebitAmount = 0,
                LienTypeId = (short)LienTypeEnum.PrincipalRepayment,
                CreatedBy = (int)SystemStaff.System,
                Description = "lien placed due to Loan Write Off", // model.description,
                DateCreated = generalSetup.GetApplicationDate()

            };

            context.tbl_CASA_Lien.Add(data);

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
        public bool LoanCancellation (int loanId, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();
            
            result.LoanStatusId = (short)LoanStatusEnum.Cancelled;

            context.SaveChanges();

            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanWorkOut(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var refNo = this.context.tbl_Loan.Where(x => x.TermLoanId == loanInput.loanId).FirstOrDefault().LoanReferenceNumber;


            var interest = from d in context.tbl_Loan_Schedule_Daily
                           where d.Date >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                           let sumDailyAccuralAmount = context.tbl_Loan_Schedule_Daily.Where(a => a.Date >= DbFunctions.TruncateTime(applicationDate)
                           && d.LoanId == loanId).Sum(a => a.DailyInterestAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.tbl_Loan_Schedule_Periodic
                            where d.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                            let sumPrincipalAmount = context.tbl_Loan_Schedule_Periodic.Where(a => a.PaymentDate >= DbFunctions.TruncateTime(applicationDate)
                            && d.LoanId == loanId).Sum(a => a.PeriodPrincipalAmount)/// add repaymentpostedstatus = false after scaffording
                            select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            var writeOffPrincipal = loanInput.payAmount;
            var writeOffInterest = loanInput.payInterest;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            if (writeOffPrincipal != null || writeOffPrincipal != 0 && writeOffInterest != null || writeOffInterest != 0)
            {
                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "Loan Outstanding Interest Balance"));

                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedPrincipal, product.PrincipalBalanceGL.Value, "Loan Outstanding principal Balance"));
            }
            else if (writeOffPrincipal == null || writeOffPrincipal == 0 && writeOffInterest != null || writeOffInterest != 0)
            {
                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "Loan Outstanding Interest Balance"));
            }
            else if (writeOffPrincipal != null || writeOffPrincipal != 0 && writeOffInterest == null || writeOffInterest == 0)
            {
                inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedPrincipal, product.PrincipalBalanceGL.Value, "Loan Outstanding principal Balance"));
            }



            financeTransaction.PostTransaction(inputTransactions);

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.LoanStatusId = (short)LoanStatusEnum.Terminated;

            context.SaveChanges();

            ///call disturbs loan method and posting



            output = true;

            return output;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId)


        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.SuspendInterest = true;



            context.SaveChanges();


            var data = (from a in context.tbl_Loan_Schedule_Daily
                        join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                        join c in context.tbl_Loan_Schedule_Periodic on b.TermLoanId equals c.LoanId
                        join d in context.tbl_Day_Count_Convention on b.ScheduleDayCountConventionId equals d.DayCountConventionId
                        where a.Date == DbFunctions.TruncateTime(applicationDate) && b.LoanStatusId == (short)LoanStatusEnum.Active
                        && a.PaymentDate == c.PaymentDate && a.LoanId == loanId && b.SuspendInterest == true

                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LoanReferenceNumber,
                            productId = b.ProductId,
                            branchId = b.BranchId,
                            companyId = b.CompanyId,
                            currencyId = b.CurrencyId,
                            exchangeRate = b.ExchangeRate,
                            interestRate = a.InterestRate,
                            date = applicationDate,
                            dailyAccuralAmount = (double)a.DailyInterestAmount,
                            mainAmount = c.PeriodInterestAmount,
                            categoryId = (short)DailyAccrualCategory.TermLoan,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DayCountConventionId,

                        });

            List<tbl_Daily_Accrual> transAccrual = new List<tbl_Daily_Accrual>();




            foreach (var item in data)
            {
                tbl_Daily_Accrual dailyAccrual = new tbl_Daily_Accrual();

                dailyAccrual.ReferenceNumber = item.referenceNumber;
                dailyAccrual.ProductId = item.productId;
                dailyAccrual.BranchId = item.branchId;
                dailyAccrual.ExchangeRate = item.exchangeRate;
                dailyAccrual.CurrencyId = item.currencyId;
                dailyAccrual.InterestRate = item.interestRate;
                dailyAccrual.Date = item.date;
                dailyAccrual.DailyAccuralAmount = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MainAmount = item.mainAmount;
                dailyAccrual.CategoryId = item.categoryId;
                dailyAccrual.CompanyId = item.companyId;
                dailyAccrual.DayCountConventionId = item.dayCountConventionId;
                dailyAccrual.BaseReferenceNumber = item.baseReferenceNumber;
                dailyAccrual.TransactionTypeId = item.transactionTypeId;




                transAccrual.Add(dailyAccrual);

            }
            this.context.tbl_Daily_Accrual.AddRange(transAccrual);

            context.SaveChanges();

            var model = (from a in context.tbl_Daily_Accrual
                         where a.Date == DbFunctions.TruncateTime(applicationDate) && a.CategoryId == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = (double)groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyInterestSuspension(item, loanId, applicationDate, staffId);
            }

            return data;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanSales(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var loan = this.context.tbl_Loan.Where(x => x.TermLoanId == loanId).FirstOrDefault();

            var casa = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == loan.CasaAccountId);

            var interest = from d in context.tbl_Loan_Schedule_Daily
                           where d.Date >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                           let sumDailyAccuralAmount = context.tbl_Loan_Schedule_Daily.Where(a => a.Date >= DbFunctions.TruncateTime(applicationDate)
                           && d.LoanId == loanId).Sum(a => a.DailyInterestAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.tbl_Loan_Schedule_Periodic
                            where d.PaymentDate >= DbFunctions.TruncateTime(applicationDate) && d.LoanId == loanId
                            let sumPrincipalAmount = context.tbl_Loan_Schedule_Periodic.Where(a => a.PaymentDate >= DbFunctions.TruncateTime(applicationDate)
                            && d.LoanId == loanId).Sum(a => a.PeriodPrincipalAmount)/// add repaymentpostedstatus = false after scaffording
                            select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            var sllp = 11;////Get SLLP GL

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedInterest, sllp, "Interest Write off"));

            inputTransactions.Add(financeTransaction.BuildTerminateAndRebookPosting(loanId, loanInput, accruedPrincipal, sllp, "principal Write off"));

            financeTransaction.PostTransaction(inputTransactions);

            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == loanId
                               select p).SingleOrDefault();

            result.LoanStatusId = (short)LoanStatusEnum.WriteOff;

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
        public bool TenorExtension(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var penalCharge = context.tbl_Charge_Fee.FirstOrDefault(x => x.OperationId == (int)OperationsEnum.TenorChange);

                if (LoanExist(loanId) > 0)
                {
                    DeleteLoanExist(loanId);
                    ArchiveLoan(loanId, loanInput.operationId);/////loanId change this to OperationId
                    ArchivePeriodicSchedule(loanId);
                    ArchiveDailySchedule(loanId);


                    //----------generate and save periodic loan schedule -----------------------------------

                    loanInput.principalAmount = loanInput.newAmount;

                    List<LoanPaymentSchedulePeriodicViewModel> periodicScheduleTemp = loanSchedule.GeneratePeriodicLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Periodic_Temp> tblPeriodicScheduleTemp = new List<tbl_Loan_Schedule_Periodic_Temp>();
                    foreach (var item in periodicScheduleTemp)
                    {
                        tbl_Loan_Schedule_Periodic_Temp scheduleTemp = new tbl_Loan_Schedule_Periodic_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.PeriodPaymentAmount = Convert.ToDecimal(item.periodPaymentAmount);
                        scheduleTemp.PeriodInterestAmount = Convert.ToDecimal(item.periodInterestAmount);
                        scheduleTemp.PeriodPrincipalAmount = Convert.ToDecimal(item.periodPrincipalAmount);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.InterestRate = loanInput.interestRate;

                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amortisedStartPrincipalAmount);
                        scheduleTemp.AmortisedPeriodPaymentAmount = Convert.ToDecimal(item.amortisedPeriodPaymentAmount);
                        scheduleTemp.AmortisedPeriodInterestAmount = Convert.ToDecimal(item.amortisedPeriodInterestAmount);
                        scheduleTemp.AmortisedPeriodPrincipalAmount = Convert.ToDecimal(item.amortisedPeriodPrincipalAmount);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amortisedEndPrincipalAmount);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblPeriodicScheduleTemp.Add(scheduleTemp);
                    }
                    //-------------------------------------------------------------------------------------


                    //----------generate and save daily loan schedule -----------------------------------

                    //loanInput.principalAmount = loanInput.newAmount;
                    List<LoanPaymentScheduleDailyViewModel> dailyScheduleTemp = loanSchedule.GenerateDailyLoanSchedule(loanInput);

                    List<tbl_Loan_Schedule_Daily_Temp> tblDailyScheduleTemp = new List<tbl_Loan_Schedule_Daily_Temp>();

                    foreach (var item in dailyScheduleTemp)
                    {
                        tbl_Loan_Schedule_Daily_Temp scheduleTemp = new tbl_Loan_Schedule_Daily_Temp();

                        scheduleTemp.LoanId = loanId;
                        scheduleTemp.PaymentNumber = item.paymentNumber;
                        scheduleTemp.Date = item.date;
                        scheduleTemp.PaymentDate = item.paymentDate;
                        scheduleTemp.OpeningBalance = Convert.ToDecimal(item.openingBalance);
                        scheduleTemp.StartPrincipalAmount = Convert.ToDecimal(item.startPrincipalAmount);
                        scheduleTemp.DailyPaymentAmount = Convert.ToDecimal(item.dailyPaymentAmount);
                        scheduleTemp.DailyInterestAmount = Convert.ToDecimal(item.dailyInterestAmount);
                        scheduleTemp.DailyPrincipalAmount = Convert.ToDecimal(item.dailyPrincipalAmount);
                        scheduleTemp.ClosingBalance = Convert.ToDecimal(item.closingBalance);
                        scheduleTemp.EndPrincipalAmount = Convert.ToDecimal(item.endPrincipalAmount);
                        scheduleTemp.AccruedInterest = Convert.ToDecimal(item.accruedInterest);
                        scheduleTemp.AmortisedCost = Convert.ToDecimal(item.amortisedCost);
                        scheduleTemp.InterestRate = item.norminalInterestRate;

                        scheduleTemp.AmortisedOpeningBalance = Convert.ToDecimal(item.amOpeningBalance);
                        scheduleTemp.AmortisedStartPrincipalAmount = Convert.ToDecimal(item.amStartPrincipalAmount);
                        scheduleTemp.AmortisedDailyPaymentAmount = Convert.ToDecimal(item.amDailyPaymentAmount);
                        scheduleTemp.AmortisedDailyInterestAmount = Convert.ToDecimal(item.amDailyInterestAmount);
                        scheduleTemp.AmortisedDailyPrincipalAmount = Convert.ToDecimal(item.amDailyPrincipalAmount);
                        scheduleTemp.AmortisedClosingBalance = Convert.ToDecimal(item.amClosingBalance);
                        scheduleTemp.AmortisedEndPrincipalAmount = Convert.ToDecimal(item.amEndPrincipalAmount);
                        scheduleTemp.AmortisedAccruedInterest = Convert.ToDecimal(item.amAccruedInterest);
                        scheduleTemp.Amortised_AmortisedCost = Convert.ToDecimal(item.amAmortisedCost);
                        scheduleTemp.DiscountPremium = Convert.ToDecimal(item.discountPremium);
                        scheduleTemp.UnEarnedFee = Convert.ToDecimal(item.unEarnedFee);
                        scheduleTemp.EarnedFee = Convert.ToDecimal(item.earnedFee);
                        scheduleTemp.EffectiveInterestRate = item.effectiveInterestRate;
                        scheduleTemp.NumberOfPeriods = item.numberOfPeriods;
                        scheduleTemp.BallonAmount = Convert.ToDecimal(item.balloonAmt);
                        scheduleTemp.CreatedBy = staffId;
                        scheduleTemp.DateTimeCreated = systemDate;

                        tblDailyScheduleTemp.Add(scheduleTemp);
                    }
                    //----------------------------------------------------------------


                    //------------adding records to the database--------------------------

                    //if (scheduleMethod == LoanScheduleTypeEnum.IrregularSchedule)
                    //{ this.context.tbl_Loan_Schedule_Irregular_Input.AddRange(tblIrregularSchedule); }////change to Temp table


                    this.context.tbl_Loan_Schedule_Periodic_Temp.AddRange(tblPeriodicScheduleTemp);////change to Temp table

                    this.context.tbl_Loan_Schedule_Daily_Temp.AddRange(tblDailyScheduleTemp); ////change to Temp table
                    context.SaveChanges();

                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    MergePeriodicSchedule(loanId, applicationDate);

                    //inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, penalCharge.GLAccountId, "Penal Charge"));///change to charge GL
                context.SaveChanges();
                    //-------------------------------------------------------
                }



            output = true;

            return output;
        }

        #endregion

        public IEnumerable<LoanOperationTypeViewModel> GetOperationType()
        {
            return (from data in context.tbl_Operations
                    where data.OperationTypeId == (int)OperationTypeEnum.LoanManagement
                    select new LoanOperationTypeViewModel()
                    {
                        operationTypeId = data.OperationId,
                        operationTypeName = data.OperationName
                    });
        }
        public IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId)
        {
            var loanOperations = (from data in context.tbl_Operations
                                  where data.OperationTypeId == (int)OperationTypeEnum.LoanManagement
                                  select new LoanOperationTypeViewModel()
                                  {
                                      operationTypeId = data.OperationId,
                                      operationTypeName = data.OperationName
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
            var data = from a in context.tbl_Loan_Review_Operation where a.LoanId == loanId
                       && a.OperationTypeId == operationTypeId //&& a.OperationCompleted == false
                       select a;
            if (data.Any())
            {
                return true;
            }
            return false;
        }
        public bool AddOperationReview(LoanReviewOperationViewModel model)
        {
            var data = new tbl_Loan_Review_Operation
            {
                LoanId = model.loanId,
                ProductTypeId = model.productTypeId,
                OperationTypeId = model.operationTypeId,
                EffectiveDate = model.proposedEffectiveDate,
                ReviewDetails = model.reviewDetails,
                InterateRate = (double)model.interateRate,
                Prepayment = model.prepayment,
                PrincipalFrequencyTypeId = model.principalFrequencyTypeId,
                InterestFrequencyTypeId = model.interestFrequencyTypeId,
                PrincipalFirstPaymentDate = model.principalFirstPaymentDate,
                InterestFirstPaymentDate = model.interestFirstPaymentDate,
                Tenor = model.tenor,
                CASA_AccountId = model.cASA_AccountId,
                OverDraftTopup = model.overDraftTopup,
                Fee_Charges = model.fee_Charges,
                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                IsManagementInterestRate = model.isManagementRate,
                OperationCompleted = false,
                CreatedBy = model.createdBy,
                DateCreated = DateTime.Now
            };
            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDocumentAdded,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Added tbl_Loan_Review_Operation '{ data.LoanReviewOperationId}' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            //end of Audit section -----------------------
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.tbl_Loan_Review_Operation.Add(data);
                    auditTrail.AddAuditTrail(audit);
                    var output = context.SaveChanges() > 0;

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
                    var response = workFlow.LogForApproval(approvalModel);
                    trans.Commit();

                    return output;
                }

                catch (Exception ex)
                {
                    trans.Rollback();
                    return false;
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

            var data = (from ln in context.tbl_Loan
                        join op in context.tbl_Loan_Review_Operation on ln.TermLoanId equals op.LoanId
                        join tt in context.tbl_Operations on op.OperationTypeId equals tt.OperationId
                        join atrail in context.tbl_Approval_Trail on op.LoanId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending
                        && atrail.OperationId == op.OperationTypeId
                        && atrail.ToApprovalLevelId == staffApprovalLevelId
                        && atrail.ResponseStaffId == null
                        orderby op.LoanId descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TermLoanId,
                            customerId = ln.CustomerId,
                            productId = ln.ProductId,
                            casaAccountId = ln.CasaAccountId,
                          // loanApplicationDetailId = (int)ln.LoanApplicationDetailId,

                            branchId = ln.BranchId,
                            loanReferenceNumber = ln.LoanReferenceNumber,
                            applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,

                            ////tenor = (ln.MaturityDate - ln.EffectiveDate).Days,
                            principalFrequencyTypeId = ln.PrincipalFrequencyTypeId != null ? (short)ln.PrincipalFrequencyTypeId : (short)0,
                            pricipalFrequencyTypeName = ln.tbl_Frequency_Type.Description,
                            interestFrequencyTypeId = ln.InterestFrequencyTypeId != null ? (short)ln.InterestFrequencyTypeId : (short)0,
                            interestFrequencyTypeName = ln.tbl_Frequency_Type.Description,

                            principalNumberOfInstallment = ln.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = ln.InterestNumberOfInstallment,
                            relationshipOfficerId = ln.RelationshipOfficerId,
                            relationshipManagerId = ln.RelationshipManagerId,
                            misCode = ln.MISCode,
                            teamMiscode = ln.TeamMISCode,
                            interestRate = ln.InterestRate,
                            effectiveDate = ln.EffectiveDate,
                            maturityDate = ln.MaturityDate,
                            bookingDate = ln.BookingDate,
                            principalAmount = ln.OutstandingPrincipal, //\\\ln.PrincipalAmount,
                            principalInstallmentLeft = ln.PrincipalInstallmentLeft,
                            interestInstallmentLeft = ln.InterestInstallmentLeft,
                            approvalStatusId = op.ApprovalStatusId,
                            approvalStatusName = context.tbl_Approval_Status.FirstOrDefault(f => f.ApprovalStatusId == op.ApprovalStatusId).ApprovalStatusName,
                            approvedBy = ln.ApprovedBy,
                            approverComment = ln.ApproverComment,
                            dateApproved = ln.DateApproved,
                            //loanStatusId = ln.LoanStatusId,
                            scheduleTypeId = ln.ScheduleTypeId,
                            isDisbursed = ln.IsDisbursed,
                            disbursedBy = ln.DisbursedBy,
                            disburserComment = ln.DisburserComment,
                            disburseDate = ln.DisburseDate,

                            ////approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.CustomerGroupId,
                            operationId = ln.OperationId,
                            loanTypeId = ln.LoanTypeId,
                            equityContribution = ln.EquityContribution,
                            subSectorId = ln.SubSectorId,
                            subSectorName = ln.tbl_Sub_Sector.Name,
                            sectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,

                            firstPrincipalPaymentDate = ln.FirstInterestPaymentDate,
                            firstInterestPaymentDate = ln.FirstInterestPaymentDate,
                            outstandingPrincipal = ln.OutstandingPrincipal,
                            principalAdditionCount = ln.PrincipalAdditionCount,
                            principalReductionCount = ln.PrincipalReductionCount,
                            fixedPrincipal = ln.FixedPrincipal,
                            profileLoan = ln.ProfileLoan,
                            dischargeLetter = ln.DischargeLetter,
                            suspendInterest = ln.SuspendInterest,

                            scheduled = ln.IsScheduledPrepayment,
                            isScheduledPrepayment = ln.IsScheduledPrepayment,
                            scheduledPrepaymentAmount = ln.ScheduledPrepaymentAmount,
                            scheduledPrepaymentDate = ln.ScheduledPrepaymentDate,

                            customerSensitivityLevelId = ln.CustomerSensitivityLevelId,
                            customerSensitivityLevelName = ln.tbl_Customer_Sensitivity_Level.Description,
                            customerCode = ln.tbl_Customer.CustomerCode,
                            productAccountNumber = ln.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = ln.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            loanTypeName = ln.tbl_Loan_Type.LoanTypeName,
                            customerName = ln.tbl_Customer.LastName + " " + ln.tbl_Customer.FirstName + " " + ln.tbl_Customer.MiddleName,
                            currencyId = ln.CurrencyId,

                            branchName = ln.tbl_Branch.BranchName,
                            relationshipOfficerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            relationshipManagerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            productName = ln.tbl_Product.ProductName,
                            comment = "",

                            //Loan Review Operation
                            operationTypeId = op.OperationTypeId,
                            operationTypeName = context.tbl_Operations.FirstOrDefault(d => d.OperationId == op.OperationTypeId).OperationName,
                            newEffectiveDate = op.EffectiveDate,
                            reviewDetails = op.ReviewDetails,
                            newInterateRate = (decimal)op.InterateRate
                        }).ToList();
            return data;
        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> GetApprovedLoanOperationReview()
        {

            var data = (from ln in context.tbl_Loan
                        join op in context.tbl_Loan_Review_Operation on ln.TermLoanId equals op.LoanId
                        where op.ApprovalStatusId == (int)ApprovalStatusEnum.Approved && op.OperationCompleted == false
                        orderby op.OperationTypeId descending
                        select new LoanReviewOperationApprovalViewModel
                        {
                            loanId = ln.TermLoanId,
                            customerId = ln.CustomerId,
                            productId = ln.ProductId,
                            casaAccountId = ln.CasaAccountId,
                            branchId = ln.BranchId,
                            loanReferenceNumber = ln.LoanReferenceNumber,
                            //applicationReferenceNumber = ln.tbl_Loan_Application_Detail.tbl_Loan_Application.ApplicationReferenceNumber,
                            principalFrequencyTypeId = ln.PrincipalFrequencyTypeId != null ? (short)ln.PrincipalFrequencyTypeId : (short)0,
                            pricipalFrequencyTypeName = ln.tbl_Frequency_Type.Description,
                            interestFrequencyTypeId = ln.InterestFrequencyTypeId != null ? (short)ln.InterestFrequencyTypeId : (short)0,
                            interestFrequencyTypeName = ln.tbl_Frequency_Type.Description,

                            principalNumberOfInstallment = ln.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = ln.InterestNumberOfInstallment,
                            relationshipOfficerId = ln.RelationshipOfficerId,
                            relationshipManagerId = ln.RelationshipManagerId,
                            misCode = ln.MISCode,
                            teamMiscode = ln.TeamMISCode,
                            interestRate = ln.InterestRate,
                            effectiveDate = ln.EffectiveDate,
                            maturityDate = ln.MaturityDate,
                            bookingDate = ln.BookingDate,
                            principalAmount = ln.OutstandingPrincipal, //\\\ln.PrincipalAmount,
                            principalInstallmentLeft = ln.PrincipalInstallmentLeft,
                            interestInstallmentLeft = ln.InterestInstallmentLeft,
                            approvalStatusId = op.ApprovalStatusId,
                            approvalStatusName = context.tbl_Approval_Status.FirstOrDefault(f => f.ApprovalStatusId == op.ApprovalStatusId).ApprovalStatusName,
                            approvedBy = ln.ApprovedBy,
                            approverComment = ln.ApproverComment,
                            dateApproved = ln.DateApproved,
                            //loanStatusId = ln.LoanStatusId,
                            scheduleTypeId = ln.ScheduleTypeId,
                            isDisbursed = ln.IsDisbursed,
                            disbursedBy = ln.DisbursedBy,
                            disburserComment = ln.DisburserComment,
                            disburseDate = ln.DisburseDate,

                            ////approvedAmount = ln.tbl_Loan_Application_Detail.ApprovedAmount,

                            customerGroupId = ln.CustomerGroupId,
                            operationId = ln.OperationId,
                            loanTypeId = ln.LoanTypeId,
                            equityContribution = ln.EquityContribution,
                            subSectorId = ln.SubSectorId,
                            subSectorName = ln.tbl_Sub_Sector.Name,
                            sectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,

                            firstPrincipalPaymentDate = ln.FirstInterestPaymentDate,
                            firstInterestPaymentDate = ln.FirstInterestPaymentDate,
                            outstandingPrincipal = ln.OutstandingPrincipal,
                            principalAdditionCount = ln.PrincipalAdditionCount,
                            principalReductionCount = ln.PrincipalReductionCount,
                            fixedPrincipal = ln.FixedPrincipal,
                            profileLoan = ln.ProfileLoan,
                            dischargeLetter = ln.DischargeLetter,
                            suspendInterest = ln.SuspendInterest,

                            scheduled = ln.IsScheduledPrepayment,
                            isScheduledPrepayment = ln.IsScheduledPrepayment,
                            scheduledPrepaymentAmount = ln.ScheduledPrepaymentAmount,
                            scheduledPrepaymentDate = ln.ScheduledPrepaymentDate,

                            customerSensitivityLevelId = ln.CustomerSensitivityLevelId,
                            customerSensitivityLevelName = ln.tbl_Customer_Sensitivity_Level.Description,
                            customerCode = ln.tbl_Customer.CustomerCode,
                            productAccountNumber = ln.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = ln.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            loanTypeName = ln.tbl_Loan_Type.LoanTypeName,
                            customerName = ln.tbl_Customer.LastName + " " + ln.tbl_Customer.FirstName + " " + ln.tbl_Customer.MiddleName,
                            currencyId = ln.CurrencyId,

                            branchName = ln.tbl_Branch.BranchName,
                            relationshipOfficerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            relationshipManagerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            productName = ln.tbl_Product.ProductName,
                            comment = "",
                            //Loan Review Operation
                            loanReviewOperationsId = op.LoanReviewOperationId,
                            operationTypeId = op.OperationTypeId,
                            operationTypeName = context.tbl_Operations.FirstOrDefault(d => d.OperationId == op.OperationTypeId).OperationName,
                            newEffectiveDate = op.EffectiveDate,
                            reviewDetails = op.ReviewDetails,
                            newInterateRate = (decimal)op.InterateRate,
                            prepayment = op.Prepayment,
                            newPrincipalFrequencyTypeId = op.PrincipalFrequencyTypeId,
                            newInterestFrequencyTypeId = op.InterestFrequencyTypeId,
                            newPrincipalFirstPaymentDate = op.PrincipalFirstPaymentDate,
                            newInterestFirstPaymentDate = op.InterestFirstPaymentDate,
                            newTenor = op.Tenor,
                            cASA_AccountId = op.CASA_AccountId,
                            overDraftTopup = op.OverDraftTopup,
                            fee_Charges = op.Fee_Charges,
                        }).ToList();
            return data;
        }
        public IEnumerable<ApprovalTrailDetailsViewModel> GetApprovalDetails(int loanId, int OperationId)
        {

            var data = (from det in context.tbl_Approval_Trail
                        where det.TargetId == loanId && det.OperationId == OperationId
                        select new ApprovalTrailDetailsViewModel
                        {
                            comment = det.Comment,
                            approvalStatusName = det.tbl_Approval_Status.ApprovalStatusName,
                            staffName = det.tbl_Staff.FirstName +" "+ det.tbl_Staff.FirstName,
                            targetName = context.tbl_Loan.FirstOrDefault(l=>l.TermLoanId == det.TargetId).LoanReferenceNumber,
                            operationName = det.tbl_Operations.OperationName,
                            approvalLevelName = det.tbl_Approval_Level.LevelName
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
            var reviewRecord = (from s in context.tbl_Loan_Review_Operation
                                where s.LoanId == loanId && s.OperationTypeId == user.operationId
                               && s.ApprovalStatusId != (int)ApprovalStatusEnum.Approved
                                && s.OperationCompleted == false
                                select s).FirstOrDefault();
            if (workFlow.NewState != (int)ApprovalState.Ended)
            {
                reviewRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;
            }
           else if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                reviewRecord.ApprovalStatusId = (int)ApprovalStatusEnum.Approved;
            }

            output = context.SaveChanges() > 0;

            return output;
        }
       [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanRephasementProcess(short loanReviewOperationsId, int loanId, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var scheduleMethod = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId).ScheduleTypeId;

            if (scheduleMethod == (short)LoanScheduleTypeEnum.IrregularSchedule)
            {    
                var model = (
                         from a in context.tbl_Loan_Review_Operation
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         //join c in context.tbl_Loan_Review_Operation_Irregular_Schedule on a.LoanReviewOperationId equals c.LoanReviewOperationId
                         where b.LoanStatusId == (short)LoanStatusEnum.Active && a.LoanId == loanId

                         select new LoanPaymentScheduleInputViewModel()
                         {
                             loanId = b.TermLoanId,
                             scheduleMethodId = b.ScheduleTypeId,
                             principalAmount = (double)b.OutstandingPrincipal,
                             principalFrequency = b.PrincipalFrequencyTypeId,
                             interestFrequency = b.InterestFrequencyTypeId,
                             principalFirstpaymentDate = (DateTime)b.FirstPrincipalPaymentDate,
                             interestFirstpaymentDate = (DateTime)b.FirstInterestPaymentDate,
                             interestRate = b.InterestRate,
                             effectiveDate = a.EffectiveDate,
                             maturityDate = b.MaturityDate,
                             accurialBasis = b.ScheduleDayCountConventionId,
                             firstDayType = b.ScheduleDayInterestTypeId,
                             integralFeeAmount = 0,
                             newEffectiveDate = a.EffectiveDate,
                             newInterestFrequency = (short?)a.InterestFrequencyTypeId ?? (short)b.InterestFrequencyTypeId,
                             newPrincipalFrequency = (short?)a.PrincipalFrequencyTypeId ?? (short)b.PrincipalFrequencyTypeId,
                             newInterestFirstpaymentDate = (DateTime)a.InterestFirstPaymentDate,
                             newInterest = (double)a.InterateRate,
                             payAmount = (double?)a.Prepayment ?? 0,
                             operationId = a.OperationTypeId,
                             newPrincipalFirstpaymentDate = (DateTime)a.PrincipalFirstPaymentDate,
                             isManagementInterestRate = true,//a.IsManagementInterestRate,
                             proposedTenor = a.Tenor,
                             newMaturityDate = a.MaturityDate,// change to maturity date affter scarfolding                             
                         }).ToList();

                foreach (var item in model)
                {
                    List<IrregularLoanScheduleInputViewModel> irregularSchedule = new List<IrregularLoanScheduleInputViewModel>();

                    if ((int)OperationsEnum.TenorChange == item.operationId)
                    {
                        var scheduleInput = context.tbl_Loan_Review_Operation_Irregular_Schedule.Where(x => x.LoanReviewOperationId == loanReviewOperationsId);

                        foreach (var item2 in scheduleInput)
                        {
                            irregularSchedule.Add(new IrregularLoanScheduleInputViewModel { paymentAmount = (double)item2.PaymentAmount, paymentDate = item2.PaymentDate });
                        }

                        item.irregularPaymentSchedule = irregularSchedule;

                    }
                    else if ((int)OperationsEnum.Prepayment == item.operationId)
                    {
                        var scheduleInput = context.tbl_Loan_Review_Operation_Irregular_Schedule.Where(x => x.LoanReviewOperationId == loanReviewOperationsId);

                        foreach (var item2 in scheduleInput)
                        {
                            irregularSchedule.Add(new IrregularLoanScheduleInputViewModel { paymentAmount = (double)item2.PaymentAmount, paymentDate = item2.PaymentDate });
                        }

                        item.irregularPaymentSchedule = irregularSchedule;
                    }
                    else
                    {
                        var scheduleInput = context.tbl_Loan_Schedule_Irregular_Input.Where(x => x.LoanId == loanId);

                        foreach (var item2 in scheduleInput)
                        {
                            irregularSchedule.Add(new IrregularLoanScheduleInputViewModel { paymentAmount = (double)item2.PaymentAmount, paymentDate = item2.PaymentDate });
                        }

                        item.irregularPaymentSchedule = irregularSchedule;
                    }

                        var unEarnedFee = from d in context.tbl_Loan_Schedule_Daily
                                      where d.LoanId == item.loanId
                                      let sumUnEarnedFee = context.tbl_Loan_Schedule_Daily.Where(a => a.LoanId == item.loanId
                                      && a.Date >= DbFunctions.TruncateTime(item.newEffectiveDate)).Sum(a => (double?)a.UnEarnedFee ?? 0)
                                      select sumUnEarnedFee;
                    item.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;
                    string appDate = item.newEffectiveDate.ToString(@"yyyy-MM-dd");
                    var applicationDate = Convert.ToDateTime(appDate);
                    if ((int)OperationsEnum.ContractualInterestRateChange == item.operationId)
                    {
                        item.interestRate = item.newInterest;
                        UpdateLoanInterestSchedule(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        if (DbFunctions.TruncateTime(item.effectiveDate) < DbFunctions.TruncateTime(systemDate))
                        {
                            //ProcessLoanRepaymentPostingPastDueForInterestReview(applicationDate, loanId);
                        }
                    }
                    else if ((int)OperationsEnum.Prepayment == item.operationId)
                    {
                        if (item.isManagementInterestRate == true)
                        {
                            item.maturityDate = (DateTime)item.newMaturityDate;
                            item.newAmount = item.principalAmount - item.payAmount;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenorPrepayment;
                            item.interestFirstpaymentDate = item.newInterestFirstpaymentDate;
                            item.principalFirstpaymentDate = item.newPrincipalFirstpaymentDate;
                        }
                        else
                        {
                            item.newAmount = item.principalAmount - item.payAmount;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.interestFirstpaymentDate = item.newInterestFirstpaymentDate;
                            item.principalFirstpaymentDate = item.newPrincipalFirstpaymentDate;
                        }

                        UpdateLoanPrepaymentSchedule(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.PaymentDateChange == item.operationId)
                    {
                        item.interestFirstpaymentDate = item.newInterestFirstpaymentDate;
                        item.principalFirstpaymentDate = item.newPrincipalFirstpaymentDate;

                        PaymentDateChange(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId || (int)OperationsEnum.InterestFrequencyChange == item.operationId
                        || (int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                    {
                        if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId)
                        {
                            item.principalFrequency = item.newPrincipalFrequency;
                            item.interestFrequency = item.interestFrequency;
                        }
                        if ((int)OperationsEnum.InterestFrequencyChange == item.operationId)
                        {
                            item.interestFrequency = item.newInterestFrequency;
                            item.principalFrequency = item.principalFrequency;
                        }
                        if ((int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                        {
                            item.interestFrequency = item.newInterestFrequency;
                            item.principalFrequency = item.newPrincipalFrequency;
                        }
                        PaymentFrequencyChange(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.CompleteWriteOff == item.operationId)
                    {
                        CompleteWriteOff(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.TerminateAndRebook == item.operationId)
                    {
                        TerminateAndRebookLoanSchedule(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.InterestSuspension == item.operationId)
                    {
                        InterestSuspension(loanId, applicationDate, staffId);

                    }
                    else if ((int)OperationsEnum.OverdraftTopup == item.operationId)
                    {
                        OverdraftTopUp(loanId, (decimal)item.newAmount);
                    }
                    else if ((int)OperationsEnum.TenorChange == item.operationId)
                    {
                        item.maturityDate = item.maturityDate.AddMonths((int)item.proposedTenor);
                        item.effectiveDate = item.newEffectiveDate;
                        item.tenor = item.tenor + (int)item.proposedTenor;
                        TenorExtension(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                }
            }
            else
            {
                var model = (
                         from a in context.tbl_Loan_Review_Operation
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where b.LoanStatusId == (short)LoanStatusEnum.Active && a.LoanId == loanId

                         select new LoanPaymentScheduleInputViewModel()
                         {
                             loanId = b.TermLoanId,
                             scheduleMethodId = b.ScheduleTypeId,
                             principalAmount = (double)b.OutstandingPrincipal,
                             principalFrequency = b.PrincipalFrequencyTypeId,
                             interestFrequency = b.InterestFrequencyTypeId,
                             principalFirstpaymentDate = (DateTime)b.FirstPrincipalPaymentDate,
                             interestFirstpaymentDate = (DateTime)b.FirstInterestPaymentDate,
                             interestRate = b.InterestRate,
                             effectiveDate = a.EffectiveDate,
                             maturityDate = b.MaturityDate,
                             accurialBasis = b.ScheduleDayCountConventionId,
                             firstDayType = b.ScheduleDayInterestTypeId,
                             integralFeeAmount = 0,
                             newEffectiveDate = a.EffectiveDate,
                             newInterestFrequency = (short?)a.InterestFrequencyTypeId ?? (short)b.InterestFrequencyTypeId,
                             newPrincipalFrequency = (short?)a.PrincipalFrequencyTypeId ?? (short)b.PrincipalFrequencyTypeId,
                             newInterestFirstpaymentDate = (DateTime)a.InterestFirstPaymentDate,
                             newInterest = (double)a.InterateRate,
                             payAmount = (double?)a.Prepayment ?? 0,
                             operationId = a.OperationTypeId,
                             newPrincipalFirstpaymentDate = (DateTime)a.PrincipalFirstPaymentDate,
                             isManagementInterestRate = true,//a.IsManagementInterestRate,
                             proposedTenor = a.Tenor,
                             newMaturityDate = a.MaturityDate,// change to maturity date affter scarfolding

                         }).ToList();

                foreach (var item in model)
                {
                    var unEarnedFee = from d in context.tbl_Loan_Schedule_Daily
                                      where d.LoanId == item.loanId
                                      let sumUnEarnedFee = context.tbl_Loan_Schedule_Daily.Where(a => a.LoanId == item.loanId
                                      && a.Date >= DbFunctions.TruncateTime(item.newEffectiveDate)).Sum(a => (double?)a.UnEarnedFee ?? 0)
                                      select sumUnEarnedFee;
                    item.integralFeeAmount = (double?)unEarnedFee.FirstOrDefault() ?? 0;
                    string appDate = item.newEffectiveDate.ToString(@"yyyy-MM-dd");
                    var applicationDate = Convert.ToDateTime(appDate);
                    if ((int)OperationsEnum.ContractualInterestRateChange == item.operationId)
                    {
                        item.interestRate = item.newInterest;
                        item.effectiveDate = item.newEffectiveDate;
                        item.tenor = item.newTenor;
                        UpdateLoanInterestSchedule(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                        if (DbFunctions.TruncateTime(item.effectiveDate) < DbFunctions.TruncateTime(systemDate))
                        {
                            //ProcessLoanRepaymentPostingPastDueForInterestReview(applicationDate, loanId);
                        }


                    }
                    else if ((int)OperationsEnum.Prepayment == item.operationId)
                    {
                        if (item.isManagementInterestRate == true)
                        {
                            item.maturityDate = (DateTime)item.newMaturityDate;
                            item.newAmount = item.principalAmount - item.payAmount;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenorPrepayment;
                            item.interestFirstpaymentDate = item.newInterestFirstpaymentDate;
                            item.principalFirstpaymentDate = item.newPrincipalFirstpaymentDate;
                        }
                        else
                        {
                            item.newAmount = item.principalAmount - item.payAmount;
                            item.effectiveDate = item.newEffectiveDate;
                            item.tenor = item.newTenor;
                            item.interestFirstpaymentDate = item.newInterestFirstpaymentDate;
                            item.principalFirstpaymentDate = item.newPrincipalFirstpaymentDate;
                        }

                        UpdateLoanPrepaymentSchedule(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.PaymentDateChange == item.operationId)
                    {
                        item.interestFirstpaymentDate = item.newInterestFirstpaymentDate;
                        item.principalFirstpaymentDate = item.newPrincipalFirstpaymentDate;

                        PaymentDateChange(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId || (int)OperationsEnum.InterestFrequencyChange == item.operationId
                        || (int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                    {
                        if ((int)OperationsEnum.PrincipalFrequencyChange == item.operationId)
                        {
                            item.principalFrequency = item.newPrincipalFrequency;
                            item.interestFrequency = item.interestFrequency;
                        }
                        if ((int)OperationsEnum.InterestFrequencyChange == item.operationId)
                        {
                            item.interestFrequency = item.newInterestFrequency;
                            item.principalFrequency = item.principalFrequency;
                        }
                        if ((int)OperationsEnum.InterestandPrincipalFrequencyChange == item.operationId)
                        {
                            item.interestFrequency = item.newInterestFrequency;
                            item.principalFrequency = item.newPrincipalFrequency;
                        }
                        PaymentFrequencyChange(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.CompleteWriteOff == item.operationId)
                    {
                        CompleteWriteOff(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.TerminateAndRebook == item.operationId)
                    {
                        TerminateAndRebookLoanSchedule(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                    else if ((int)OperationsEnum.InterestSuspension == item.operationId)
                    {
                        InterestSuspension(loanId, applicationDate, staffId);

                    }
                    else if ((int)OperationsEnum.OverdraftTopup == item.operationId)
                    {
                        OverdraftTopUp(loanId, (decimal)item.newAmount);
                    }
                    else if ((int)OperationsEnum.TenorChange == item.operationId)
                    {
                        item.maturityDate = item.maturityDate.AddMonths((int)item.proposedTenor);
                        item.effectiveDate = item.newEffectiveDate;
                        item.tenor = item.tenor + (int)item.proposedTenor;
                        TenorExtension(loanId, item, applicationDate, staffId);
                        updateLoanReviewOperation(loanReviewOperationsId, loanId);
                    }
                }
            }
            context.SaveChanges();
            //-------------------------------------------------------
            output = true;

            return output;
        }

    }
}
