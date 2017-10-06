using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
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
        private IWorkFlowRepository workFlow;
        public LoanOperationsRepository(

        FinTrakBankingContext _context, IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail,
            ILoanScheduleRepository _loanSchedule, IWorkFlowRepository _workFlow)
        {

            this.context = _context;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.workFlow = _workFlow;
        }


        public decimal GetCollateralSearchChargeAmount(int stateId)
        {
            var collateralSearchChargeAmount = this.context.tbl_State.FirstOrDefault(x => x.StateId == stateId).CollateralSearchChargeAmount;


            return collateralSearchChargeAmount;
        }

        public IEnumerable<DailyInterestAccrualViewModel> GetDailyTeamLoansInterestAccrual(DateTime applicationDate)


        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = (from a in context.tbl_Loan_Schedule_Daily
                        join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                        join c in context.tbl_Loan_Schedule_Periodic on b.TermLoanId equals c.LoanId
                        join d in context.tbl_Day_Count_Convention on b.ScheduleDayCountConventionId equals d.DayCountConventionId
                        where a.Date == applicationDate && b.LoanStatusId == (short)LoanStatusEnum.Active
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
                            dailyAccuralAmount = a.DailyInterestAmount,
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
                dailyAccrual.DailyAccuralAmount = Math.Abs(item.dailyAccuralAmount);
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
                         where a.Date == applicationDate && a.CategoryId == (short)DailyAccrualCategory.TermLoan
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyLoansInterestAccrual(item);
            }

            return data;
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

        public IEnumerable<DailyInterestAccrualViewModel> GetDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan_Revolving
                        join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                        join c in context.tbl_Day_Count_Convention on a.DayCountConventionId equals c.DayCountConventionId
                        where a.EffectiveDate == applicationDate && a.LoanStatusId == (short)LoanStatusEnum.Active
                        && b.AvailableBalance < 0


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
                            dailyAccuralAmount = (decimal)a.InterestRate,
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
                dailyAccrual.DailyAccuralAmount = Math.Abs((item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
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
                         where a.Date == applicationDate && a.CategoryId == (short)DailyAccrualCategory.AuthorisedOverdraft
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyAuthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> GetDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan
                        join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                        join c in context.tbl_Day_Count_Convention on a.ScheduleDayCountConventionId equals c.DayCountConventionId
                        join d in context.tbl_Setup_Global on a.CompanyId equals d.CompanyId
                        where a.EffectiveDate == applicationDate && a.LoanStatusId == (short)LoanStatusEnum.Active
                        && b.AvailableBalance < 0 && a.AllowForceDebitRepayment == true


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
                            dailyAccuralAmount = (decimal)d.UnauthorisedOverdraft_InterestRate,
                            mainAmount = b.AvailableBalance,
                            categoryId = (short)DailyAccrualCategory.UnauthorisedOverdraft,
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
                dailyAccrual.DailyAccuralAmount = Math.Abs((item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
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
                         where a.Date == applicationDate && a.CategoryId == (short)DailyAccrualCategory.UnauthorisedOverdraft
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyUnauthorisedOverdraftInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> GetDailyPastDueInterestAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan
                        join b in context.tbl_Loan_Past_Due on a.TermLoanId equals b.LoanId
                        join c in context.tbl_Day_Count_Convention on a.ScheduleDayCountConventionId equals c.DayCountConventionId
                        join d in context.tbl_Setup_Global on a.CompanyId equals d.CompanyId
                        where a.EffectiveDate == applicationDate && a.LoanStatusId == (short)LoanStatusEnum.Active
                        && (b.DebitAmount - b.CreditAmount) < 0 && a.AllowForceDebitRepayment == false
                        && b.TransactionTypeId == (byte)LoanTransactionTypeEnum.Interest


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
                            dailyAccuralAmount = (decimal)d.PastDueInDefault_InterestRate,
                            mainAmount = (b.DebitAmount - b.CreditAmount),
                            categoryId = (short)DailyAccrualCategory.UnauthorisedOverdraft,
                            availableBalance = (b.DebitAmount - b.CreditAmount),
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
                dailyAccrual.DailyAccuralAmount = Math.Abs((item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
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
                         where a.Date == applicationDate && a.CategoryId == (short)DailyAccrualCategory.PastDueObligation
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyPastDueInterestAccrual(item);
            }
            return data;
        }

        public IEnumerable<DailyInterestAccrualViewModel> GetDailyPastDuePrincipalAccrual(DateTime applicationDate)

        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);


            var data = (from a in context.tbl_Loan
                        join b in context.tbl_Loan_Past_Due on a.TermLoanId equals b.LoanId
                        join c in context.tbl_Day_Count_Convention on a.ScheduleDayCountConventionId equals c.DayCountConventionId
                        join d in context.tbl_Setup_Global on a.CompanyId equals d.CompanyId
                        where a.EffectiveDate == applicationDate && a.LoanStatusId == (short)LoanStatusEnum.Active
                        && (b.DebitAmount - b.CreditAmount) < 0 && a.AllowForceDebitRepayment == false
                        && b.TransactionTypeId == (byte)LoanTransactionTypeEnum.Principal


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
                            dailyAccuralAmount = (decimal)d.PastDueInDefault_InterestRate,
                            mainAmount = (b.DebitAmount - b.CreditAmount),
                            categoryId = (short)DailyAccrualCategory.UnauthorisedOverdraft,
                            availableBalance = (b.DebitAmount - b.CreditAmount),
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
                dailyAccrual.DailyAccuralAmount = Math.Abs((item.dailyAccuralAmount / item.daysInAYear) * item.availableBalance);
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
                         where a.Date == applicationDate && a.CategoryId == (short)DailyAccrualCategory.PastDueObligation
                         group a by new { a.ProductId, a.BranchId, a.CompanyId, a.CurrencyId, a.ExchangeRate } into groupedQ
                         select new DailyInterestAccrualViewModel()
                         {
                             productId = groupedQ.Key.ProductId,
                             branchId = groupedQ.Key.BranchId,
                             companyId = groupedQ.Key.CompanyId,
                             currencyId = groupedQ.Key.CurrencyId,
                             exchangeRate = groupedQ.Key.ExchangeRate,
                             dailyAccuralAmount = groupedQ.Sum(i => i.DailyAccuralAmount),
                         });

            foreach (var item in model)
            {
                financeTransaction.PostDailyPastDuePrincipalAccrual(item);
            }
            return data;
        }

        public void updateloanTable(LoanRepaymentViewModel item)
        {
            tbl_Loan result = (from p in context.tbl_Loan
                               where p.TermLoanId == item.loanId
                               select p).SingleOrDefault();

            result.OutstandingPrincipal = result.OutstandingPrincipal - item.periodPrincipalAmount;
            result.OutstandingInterest = result.OutstandingInterest - item.periodInterestAmount;


            context.SaveChanges();
        }

        public IEnumerable<LoanRepaymentViewModel> BuildLoanRepaymentPostingForceDebit(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where a.PaymentDate == applicationDate && b.LoanStatusId == (short)LoanStatusEnum.Active
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

                         });

            List<tbl_Loan_Force_Debit> transForceDebit = new List<tbl_Loan_Force_Debit>();


            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);

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

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodPrincipalAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                    financeTransaction.PostTransaction(inputTransactions);

                    updateloanTable(item);
                }
                else if (casabalance < 0)
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

        public IEnumerable<LoanRepaymentViewModel> BuildLoanRepaymentPostingPastDue(DateTime applicationDate)

        {
            var model = (from a in context.tbl_Loan_Schedule_Periodic
                         join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                         where a.PaymentDate == applicationDate && b.LoanStatusId == (short)LoanStatusEnum.Active
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
                         });

            List<tbl_Loan_Past_Due> transForceDebit = new List<tbl_Loan_Past_Due>();

            foreach (var item in model)
            {
                var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == item.productId);

                var forceDebitCode = CommonHelpers.GenerateRandomDigitCode(10);
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
                    tbl_Loan_Past_Due forceDebit = new tbl_Loan_Past_Due();


                    forceDebit.LoanId = item.loanId;
                    forceDebit.PastDueCode = forceDebitCode;
                    forceDebit.CreditAmount = 0;
                    forceDebit.Description = "Force Debit as a result of Account not funded";
                    forceDebit.DebitAmount = Math.Abs(casabalance - item.totalAmount);
                    forceDebit.Date = item.paymentDate;
                    forceDebit.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebit.Parent_PastDueCode = item.loanRefNo;

                    transForceDebit.Add(forceDebit);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, item.periodInterestAmount, product.InterestReceivablePayableGL.Value, "interest repayment"));
                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, forceDebit.DebitAmount, product.PrincipalBalanceGL.Value, "partial principal repayment"));
                    // place lien on the customer account on partial principal
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = forceDebit.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = forceDebit.DebitAmount,
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
                    tbl_Loan_Past_Due forceDebitInterest = new tbl_Loan_Past_Due();

                    forceDebitInterest.LoanId = item.loanId;
                    forceDebitInterest.PastDueCode = forceDebitCode;
                    forceDebitInterest.CreditAmount = 0;
                    forceDebitInterest.Description = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DebitAmount = Math.Abs(casabalance - item.periodInterestAmount);
                    forceDebitInterest.Date = item.paymentDate;
                    forceDebitInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.Parent_PastDueCode = item.loanRefNo;

                    transForceDebit.Add(forceDebitInterest);

                    tbl_Loan_Past_Due forceDebitPrincipal = new tbl_Loan_Past_Due();

                    forceDebitPrincipal.LoanId = item.loanId;
                    forceDebitPrincipal.PastDueCode = forceDebitCode;
                    forceDebitPrincipal.CreditAmount = 0;
                    forceDebitPrincipal.Description = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DebitAmount = item.periodPrincipalAmount;
                    forceDebitPrincipal.Date = item.paymentDate;
                    forceDebitPrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.Parent_PastDueCode = item.loanRefNo;

                    transForceDebit.Add(forceDebitPrincipal);

                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    inputTransactions.Add(financeTransaction.PostBuildLoanRepaymentPosting(item, forceDebitInterest.DebitAmount, product.InterestReceivablePayableGL.Value, "partial interest repayment"));
                    financeTransaction.PostTransaction(inputTransactions);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = forceDebitInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = forceDebitInterest.DebitAmount,
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
                        SourceReferenceNumber = forceDebitPrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = forceDebitPrincipal.DebitAmount,
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
                else if (casabalance < 0)
                {
                    tbl_Loan_Past_Due forceDebitInterest = new tbl_Loan_Past_Due();

                    forceDebitInterest.LoanId = item.loanId;
                    forceDebitInterest.PastDueCode = forceDebitCode;
                    forceDebitInterest.CreditAmount = 0;
                    forceDebitInterest.Description = "Force Debit as a result of Account not funded";
                    forceDebitInterest.DebitAmount = Math.Abs(item.periodInterestAmount);
                    forceDebitInterest.Date = item.paymentDate;
                    forceDebitInterest.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                    forceDebitInterest.Parent_PastDueCode = item.loanRefNo;

                    transForceDebit.Add(forceDebitInterest);

                    tbl_Loan_Past_Due forceDebitPrincipal = new tbl_Loan_Past_Due();

                    forceDebitPrincipal.LoanId = item.loanId;
                    forceDebitPrincipal.PastDueCode = forceDebitCode;
                    forceDebitPrincipal.CreditAmount = 0;
                    forceDebitPrincipal.Description = "Force Debit as a result of Account not funded";
                    forceDebitPrincipal.DebitAmount = Math.Abs(item.periodPrincipalAmount);
                    forceDebitPrincipal.Date = item.paymentDate;
                    forceDebitPrincipal.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                    forceDebitPrincipal.Parent_PastDueCode = item.loanRefNo;

                    transForceDebit.Add(forceDebitPrincipal);

                    var data = new tbl_CASA_Lien
                    {
                        ProductAccountNumber = casa.ProductAccountNumber,
                        LienReferenceNumber = CommonHelpers.GenerateRandomDigitCode(10),
                        SourceReferenceNumber = forceDebitInterest.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = forceDebitInterest.DebitAmount,
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
                        SourceReferenceNumber = forceDebitPrincipal.Parent_PastDueCode,
                        BranchId = item.branchId,
                        CompanyId = item.companyId,
                        LienCreditAmount = forceDebitPrincipal.DebitAmount,
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

            this.context.tbl_Loan_Past_Due.AddRange(transForceDebit);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanRepaymentViewModel> BuildAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Daily_Accrual on a.LoanReferenceNumber equals c.ReferenceNumber
                         where firstDayOfMonth <= applicationDate && lastDayOfMonth <= applicationDate
                         && a.LoanStatusId == (short)LoanStatusEnum.Active
                         && c.CategoryId == (short)DailyAccrualCategory.AuthorisedOverdraft
                         // && b.AvailableBalance < 0
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


                         });

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

        public IEnumerable<LoanRepaymentViewModel> BuildUnauthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate)
        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var model = (from a in context.tbl_Loan
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Daily_Accrual on a.LoanReferenceNumber equals c.ReferenceNumber
                         where firstDayOfMonth <= applicationDate && lastDayOfMonth <= applicationDate
                         && a.LoanStatusId == (short)LoanStatusEnum.Active
                         && c.CategoryId == (short)DailyAccrualCategory.UnauthorisedOverdraft
                         //&& b.AvailableBalance < 0
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


                         });

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

        public IEnumerable<LoanPastDueViewModel> BuildUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate)

        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var model = (from a in context.tbl_Loan_Past_Due
                         where firstDayOfMonth <= applicationDate && lastDayOfMonth <= applicationDate
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
                         });

            List<tbl_Loan_Past_Due> loanPastDue = new List<tbl_Loan_Past_Due>();



            foreach (var item in model)
            {
                tbl_Loan_Past_Due forceDebit = new tbl_Loan_Past_Due();


                forceDebit.LoanId = item.loanId;
                forceDebit.PastDueCode = item.pastDueCode;
                forceDebit.CreditAmount = 0;
                forceDebit.Description = "Interest Accrual as a result of Past Due";
                forceDebit.DebitAmount = Math.Abs(item.totalAmount);
                forceDebit.Date = applicationDate;
                forceDebit.TransactionTypeId = (byte)LoanTransactionTypeEnum.Interest;
                forceDebit.Parent_PastDueCode = item.parent_PastDueCode;

                loanPastDue.Add(forceDebit);

            }

            this.context.tbl_Loan_Past_Due.AddRange(loanPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanPastDueViewModel> BuildUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(DateTime applicationDate)

        {

            var firstDayOfMonth = new DateTime(applicationDate.Year, applicationDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var model = (from a in context.tbl_Loan_Past_Due
                         where firstDayOfMonth <= applicationDate && lastDayOfMonth <= applicationDate
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
                         });

            List<tbl_Loan_Past_Due> loanPastDue = new List<tbl_Loan_Past_Due>();



            foreach (var item in model)
            {
                tbl_Loan_Past_Due forceDebit = new tbl_Loan_Past_Due();


                forceDebit.LoanId = item.loanId;
                forceDebit.PastDueCode = item.pastDueCode;
                forceDebit.CreditAmount = 0;
                forceDebit.Description = "Principal Accrual as a result of Past Due";
                forceDebit.DebitAmount = Math.Abs(item.totalAmount);
                forceDebit.Date = applicationDate;
                forceDebit.TransactionTypeId = (byte)LoanTransactionTypeEnum.Principal;
                forceDebit.Parent_PastDueCode = item.parent_PastDueCode;

                loanPastDue.Add(forceDebit);

            }

            this.context.tbl_Loan_Past_Due.AddRange(loanPastDue);

            context.SaveChanges();
            return model;
        }

        public IEnumerable<LoanViewModel> BuildIntervalFeeandCommissionPosting(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Fee
                         join b in context.tbl_Product_Type on a.ProductTypeId equals b.ProductTypeId
                         join c in context.tbl_Loan_Fee_Schedule on a.LoanChargeFeeId equals c.LoanChargeFeeId
                         join d in context.tbl_Loan on a.LoanId equals d.TermLoanId
                         join e in context.tbl_CASA on d.CasaAccountId equals e.CasaAccountId
                         where c.FeeDate == applicationDate && a.IsRecurring == true
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


                         });

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

        public IEnumerable<LimitSuspensionViewModel> NPLByBranchSuspension()
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

                         });


            foreach (var item in model)
            {
                if (item.amount >= item.limitAmount)
                {
                    tbl_Branch result = (from p in context.tbl_Branch
                                         where p.BranchId == item.branchId
                                         select p).SingleOrDefault();

                    result.NPL_LimitExceeded = true;


                    context.SaveChanges();
                }


            }
            return model;

        }

        public IEnumerable<LimitSuspensionViewModel> NPLByRMSuspension()
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

                         });


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

        public IEnumerable<LoanCovenantDetailViewModel> OverdraftBalanceSuspensionBaseOnCleanUp(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Loan_Covenant_Detail on a.RevolvingLoanId equals c.LoanId
                         join d in context.tbl_Loan_Covenant_Type on c.CovenantTypeId equals d.CovenantTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.RevolvingLoanId == c.LoanId
                         && c.CovenantTypeId == d.CovenantTypeId && c.NextCovenantDate == applicationDate
                          && d.CovenantTypeId == (short)LoanCovenantTypeEnum.Cleanup
                         select new LoanCovenantDetailViewModel()
                         {
                             loanCovenantDetailId = c.LoanCovenantDetailId,
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             frequencyTypeId = c.FrequencyTypeId,

                         });


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

        public IEnumerable<LoanCovenantDetailViewModel> OverdraftBalanceSuspensionBaseOnCovenant(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join c in context.tbl_Loan_Covenant_Detail on a.RevolvingLoanId equals c.LoanId
                         join d in context.tbl_Loan_Covenant_Type on c.CovenantTypeId equals d.CovenantTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.RevolvingLoanId == c.LoanId
                         && c.CovenantTypeId == d.CovenantTypeId && c.NextCovenantDate == applicationDate
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

                         });


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

        public IEnumerable<LoanCovenantDetailViewModel> LPOExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join e in context.tbl_Product on a.ProductId equals e.ProductId
                         join f in context.tbl_Product_Type on e.ProductTypeId equals f.ProductTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.ProductId == e.ProductId
                         && e.ProductTypeId == f.ProductTypeId && a.EffectiveDate <= applicationDate
                         && e.ProductTypeId == (short)LoanProductTypeEnum.LPO
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             effectiveDate = a.EffectiveDate,
                             maximumDrawDownDuration = (int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         });


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

        public IEnumerable<LoanCovenantDetailViewModel> CFFExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join e in context.tbl_Product on a.ProductId equals e.ProductId
                         join f in context.tbl_Product_Type on e.ProductTypeId equals f.ProductTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.ProductId == e.ProductId
                         && e.ProductTypeId == f.ProductTypeId && a.EffectiveDate <= applicationDate
                         && e.ProductTypeId == (short)LoanProductTypeEnum.CFF
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             effectiveDate = a.EffectiveDate,
                             maximumDrawDownDuration = (int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         });


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

        public IEnumerable<LoanCovenantDetailViewModel> IDFExpiryAndlocking(DateTime applicationDate)
        {
            var model = (from a in context.tbl_Loan_Revolving
                         join b in context.tbl_CASA on a.CasaAccountId equals b.CasaAccountId
                         join e in context.tbl_Product on a.ProductId equals e.ProductId
                         join f in context.tbl_Product_Type on e.ProductTypeId equals f.ProductTypeId
                         where a.CasaAccountId == b.CasaAccountId && a.ProductId == e.ProductId
                         && e.ProductTypeId == f.ProductTypeId && a.EffectiveDate <= applicationDate
                         && e.ProductTypeId == (short)LoanProductTypeEnum.IDF
                         select new LoanCovenantDetailViewModel()
                         {
                             loanId = a.RevolvingLoanId,
                             loanRef = a.LoanReferenceNumber,
                             casaId = a.CasaAccountId,
                             effectiveDate = a.EffectiveDate,
                             maximumDrawDownDuration = (int)e.ExpiryPeriod, // change to MaximumDrawDownDuration after scarfolding
                         });


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

        public IEnumerable<LoanViewModel> ArchiveLoanSchedule(int loanId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan
                         where a.TermLoanId == loanId && a.LoanStatusId == (short)LoanStatusEnum.Active
                         select new LoanViewModel()
                         {
                             loanId = a.TermLoanId,
                             productPriceIndexRate = (decimal)a.ProductPriceIndexRate,
                             customerRiskRatingId = (int)a.CustomerRiskRatingId,
                             customerId = a.CustomerId,
                             productId = a.ProductId,
                             companyId = a.CompanyId,
                             casaAccountId = a.CasaAccountId,
                             branchId = a.BranchId,
                             currencyId = a.CurrencyId,
                             exchangeRate = a.ExchangeRate,
                             loanApplicationDetailId = a.tbl_Loan_Application_Detail.LoanApplicationDetailId,
                             loanReferenceNumber = a.LoanReferenceNumber,
                             subSectorId = a.SubSectorId,
                             principalFrequencyTypeId = (short)a.PrincipalFrequencyTypeId,
                             interestFrequencyTypeId = (short)a.InterestFrequencyTypeId,
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
                             approvedAmount = a.ApprovedAmount,
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
                             isScheduledPrepayment = (bool)a.IsScheduledPrepayment,
                             allowForceDebitRepayment = (bool)a.AllowForceDebitRepayment,
                             scheduledPrepaymentAmount = (decimal)a.ScheduledPrepaymentAmount,
                             scheduledPrepaymentDate = (DateTime)a.ScheduledPrepaymentDate,
                             scheduledPrepaymentFrequencyTypeId = (short)a.ScheduledPrepaymentFrequencyTypeId,
                             customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                             internalPrudentialGuidelineStatusId = (int)a.InternalPrudentialGuidelineStatusId,
                             externalPrudentialGuidelineStatusId = (int)a.ExternalPrudentialGuidelineStatusId,
                             nplDate = (DateTime)a.NPLDate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,

                         });

            List<tbl_Loan_Archive> loanScheduleArchive = new List<tbl_Loan_Archive>();



            foreach (var item in model)
            {

                tbl_Loan_Archive addLoanScheduleArchive = new tbl_Loan_Archive();


                addLoanScheduleArchive.LoanId = item.loanId;
                addLoanScheduleArchive.ProductPriceIndexRate = (double)item.productPriceIndexRate;
                addLoanScheduleArchive.CustomerRiskRatingId = item.customerRiskRatingId;
                addLoanScheduleArchive.CustomerId = item.customerId;
                addLoanScheduleArchive.ProductId = item.productId;
                addLoanScheduleArchive.CompanyId = item.companyId;
                addLoanScheduleArchive.CasaAccountId = item.casaAccountId;
                addLoanScheduleArchive.BranchId = item.branchId;
                addLoanScheduleArchive.CurrencyId = (short)item.currencyId;
                addLoanScheduleArchive.ExchangeRate = item.exchangeRate;
                addLoanScheduleArchive.LoanApplicationId = item.loanApplicationDetailId;
                addLoanScheduleArchive.LoanReferenceNumber = item.loanReferenceNumber;
                addLoanScheduleArchive.SubSectorId = item.subSectorId;
                addLoanScheduleArchive.PrincipalFrequencyTypeId = item.principalFrequencyTypeId;
                addLoanScheduleArchive.InterestFrequencyTypeId = item.interestFrequencyTypeId;
                addLoanScheduleArchive.PrincipalNumberOfInstallment = item.principalNumberOfInstallment;
                addLoanScheduleArchive.InterestNumberOfInstallment = item.interestNumberOfInstallment;
                addLoanScheduleArchive.RelationshipOfficerId = item.relationshipOfficerId;
                addLoanScheduleArchive.RelationshipManagerId = item.relationshipManagerId;
                addLoanScheduleArchive.MISCode = item.misCode;
                addLoanScheduleArchive.TeamMISCode = item.teamMiscode;
                addLoanScheduleArchive.InterestRate = item.interestRate;
                addLoanScheduleArchive.EffectiveDate = item.effectiveDate;
                addLoanScheduleArchive.MaturityDate = item.maturityDate;
                addLoanScheduleArchive.BookingDate = item.bookingDate;
                addLoanScheduleArchive.PrincipalAmount = item.principalAmount;
                addLoanScheduleArchive.ApprovedAmount = (decimal)item.approvedAmount;
                addLoanScheduleArchive.PrincipalInstallmentLeft = item.principalInstallmentLeft;
                addLoanScheduleArchive.InterestInstallmentLeft = item.interestInstallmentLeft;
                addLoanScheduleArchive.ApprovalStatusId = item.approvalStatusId;
                addLoanScheduleArchive.ApprovedBy = item.approvedBy;
                addLoanScheduleArchive.ApproverComment = item.approverComment;
                addLoanScheduleArchive.DateApproved = item.dateApproved;
                addLoanScheduleArchive.LoanStatusId = item.loanStatusId;
                addLoanScheduleArchive.CreatedBy = item.createdBy;
                addLoanScheduleArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanScheduleArchive.ScheduleTypeId = item.scheduleTypeId;
                addLoanScheduleArchive.ScheduleDayCountConventionId = item.scheduleDayCountConventionId;
                addLoanScheduleArchive.ScheduleDayInterestTypeId = item.scheduleDayInterestTypeId;
                addLoanScheduleArchive.IsDisbursed = item.isDisbursed;
                addLoanScheduleArchive.DisbursedBy = item.disbursedBy;
                addLoanScheduleArchive.DisburserComment = item.disburserComment;
                addLoanScheduleArchive.DisburseDate = item.disburseDate;
                addLoanScheduleArchive.OperationId = (int)item.operationId;
                addLoanScheduleArchive.CustomerGroupId = item.customerGroupId;
                addLoanScheduleArchive.LoanTypeId = item.loanTypeId;
                //addLoanScheduleArchive.TrancheBatchCode = item.trancheBatchCode;
                addLoanScheduleArchive.EquityContribution = item.equityContribution;
                addLoanScheduleArchive.FirstPrincipalPaymentDate = item.firstPrincipalPaymentDate;
                addLoanScheduleArchive.FirstInterestPaymentDate = item.firstInterestPaymentDate;
                addLoanScheduleArchive.OutstandingPrincipal = item.outstandingPrincipal;
                addLoanScheduleArchive.OutstandingInterest = item.outstandingInterest;
                addLoanScheduleArchive.PrincipalAdditionCount = item.principalAdditionCount;
                addLoanScheduleArchive.PrincipalReductionCount = item.principalReductionCount;
                addLoanScheduleArchive.FixedPrincipal = item.fixedPrincipal;
                addLoanScheduleArchive.ProfileLoan = item.profileLoan;
                addLoanScheduleArchive.DischargeLetter = item.dischargeLetter;
                addLoanScheduleArchive.SuspendInterest = item.suspendInterest;
                addLoanScheduleArchive.IsScheduledPrepayment = item.isScheduledPrepayment;
                addLoanScheduleArchive.AllowForceDebitRepayment = item.allowForceDebitRepayment;
                addLoanScheduleArchive.ScheduledPrepaymentAmount = item.scheduledPrepaymentAmount;
                addLoanScheduleArchive.ScheduledPrepaymentDate = item.scheduledPrepaymentDate;
                addLoanScheduleArchive.ScheduledPrepaymentFrequencyTypeId = item.scheduledPrepaymentFrequencyTypeId;
                addLoanScheduleArchive.CustomerSensitivityLevelId = item.customerSensitivityLevelId;
                addLoanScheduleArchive.InternalPrudentialGuidelineStatusId = item.internalPrudentialGuidelineStatusId;
                addLoanScheduleArchive.ExternalPrudentialGuidelineStatusId = item.externalPrudentialGuidelineStatusId;
                addLoanScheduleArchive.NPLDate = item.nplDate;
                addLoanScheduleArchive.CreatedBy = item.createdBy;
                addLoanScheduleArchive.DateTimeCreated = item.dateTimeCreated;

                loanScheduleArchive.Add(addLoanScheduleArchive);

            }

            this.context.tbl_Loan_Archive.AddRange(loanScheduleArchive);

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

                         });

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


                         });

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

        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> UpdatePeriodicSchedule(int loanId, DateTime reviewEffectiveDate)
        {


            var number = (from a in context.tbl_Loan_Schedule_Periodic_Archive
                          where a.LoanId == loanId && a.PaymentDate <= reviewEffectiveDate
                          select a);

            int no = number.Count() - 1;

            var model = (from a in context.tbl_Loan_Schedule_Periodic_Archive
                         where a.LoanId == loanId && a.PaymentDate <= reviewEffectiveDate
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
                          where a.LoanId == loanId && a.PaymentDate >= reviewEffectiveDate
                          && a.PaymentNumber != 0
                          orderby a.PaymentNumber ascending
                          select new LoanPaymentSchedulePeriodicViewModel()
                          {
                              loanId = a.LoanId,
                              paymentNumber = a.PaymentNumber + no,
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
                          });

            List<tbl_Loan_Schedule_Periodic> loanSchedulePeriodic = new List<tbl_Loan_Schedule_Periodic>();



            foreach (var item in model)
            {
                tbl_Loan_Schedule_Periodic addLoanSchedulePeriodic = new tbl_Loan_Schedule_Periodic();


                addLoanSchedulePeriodic.LoanId = item.loanId;
                addLoanSchedulePeriodic.PaymentNumber = item.paymentNumber;
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

            var itemToRemove = context.tbl_Loan_Schedule_Periodic.SingleOrDefault(x => x.LoanId == loanId);//  confirm if this code will delete all data with loanId

            if (itemToRemove != null)
            {
                context.tbl_Loan_Schedule_Periodic.Remove(itemToRemove);
                context.SaveChanges();
            }

            this.context.tbl_Loan_Schedule_Periodic.AddRange(loanSchedulePeriodic);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool UpdateLoanSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            if (LoanExist(loanId) > 0)
            {
                ArchiveLoanSchedule(loanId);
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

                //----------update loan details -----------------------------------
                var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                //-------------------------------------------------

                UpdatePeriodicSchedule(loanId, applicationDate);

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

                         });

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


                         });

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

        public IEnumerable<LoanViewModel> BulkArchiveLoanSchedule(int priceindexId)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(5);
            var model = (from a in context.tbl_Loan
                         where a.LoanStatusId == (short)LoanStatusEnum.Active
                          && !context.tbl_Loan_PriceIndex_Exception.Any(d => d.LoanId == a.TermLoanId)

                         select new LoanViewModel()
                         {
                             loanId = a.TermLoanId,
                             productPriceIndexRate = (decimal)a.ProductPriceIndexRate,
                             customerRiskRatingId = (int)a.CustomerRiskRatingId,
                             customerId = a.CustomerId,
                             productId = a.ProductId,
                             companyId = a.CompanyId,
                             casaAccountId = a.CasaAccountId,
                             branchId = a.BranchId,
                             currencyId = a.CurrencyId,
                             exchangeRate = a.ExchangeRate,
                             loanApplicationDetailId = a.tbl_Loan_Application_Detail.LoanApplicationId,
                             loanReferenceNumber = a.LoanReferenceNumber,
                             subSectorId = a.SubSectorId,
                             principalFrequencyTypeId = (short)a.PrincipalFrequencyTypeId,
                             interestFrequencyTypeId = (short)a.InterestFrequencyTypeId,
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
                             approvedAmount = a.ApprovedAmount,
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
                             //trancheBatchCode = a.TrancheBatchCode,
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
                             isScheduledPrepayment = (bool)a.IsScheduledPrepayment,
                             allowForceDebitRepayment = (bool)a.AllowForceDebitRepayment,
                             scheduledPrepaymentAmount = (decimal)a.ScheduledPrepaymentAmount,
                             scheduledPrepaymentDate = (DateTime)a.ScheduledPrepaymentDate,
                             scheduledPrepaymentFrequencyTypeId = (short)a.ScheduledPrepaymentFrequencyTypeId,
                             customerSensitivityLevelId = a.CustomerSensitivityLevelId,
                             internalPrudentialGuidelineStatusId = (int)a.InternalPrudentialGuidelineStatusId,
                             externalPrudentialGuidelineStatusId = (int)a.ExternalPrudentialGuidelineStatusId,
                             nplDate = (DateTime)a.NPLDate,
                             createdBy = a.CreatedBy,
                             dateTimeCreated = a.DateTimeCreated,

                         });

            List<tbl_Loan_Archive> loanScheduleArchive = new List<tbl_Loan_Archive>();



            foreach (var item in model)
            {

                tbl_Loan_Archive addLoanScheduleArchive = new tbl_Loan_Archive();


                addLoanScheduleArchive.LoanId = item.loanId;
                addLoanScheduleArchive.ProductPriceIndexRate = (double)item.productPriceIndexRate;
                addLoanScheduleArchive.CustomerRiskRatingId = item.customerRiskRatingId;
                addLoanScheduleArchive.CustomerId = item.customerId;
                addLoanScheduleArchive.ProductId = item.productId;
                addLoanScheduleArchive.CompanyId = item.companyId;
                addLoanScheduleArchive.CasaAccountId = item.casaAccountId;
                addLoanScheduleArchive.BranchId = item.branchId;
                addLoanScheduleArchive.CurrencyId = (short)item.currencyId;
                addLoanScheduleArchive.ExchangeRate = item.exchangeRate;
                addLoanScheduleArchive.LoanApplicationId = item.loanApplicationDetailId;
                addLoanScheduleArchive.LoanReferenceNumber = item.loanReferenceNumber;
                addLoanScheduleArchive.SubSectorId = item.subSectorId;
                addLoanScheduleArchive.PrincipalFrequencyTypeId = item.principalFrequencyTypeId;
                addLoanScheduleArchive.InterestFrequencyTypeId = item.interestFrequencyTypeId;
                addLoanScheduleArchive.PrincipalNumberOfInstallment = item.principalNumberOfInstallment;
                addLoanScheduleArchive.InterestNumberOfInstallment = item.interestNumberOfInstallment;
                addLoanScheduleArchive.RelationshipOfficerId = item.relationshipOfficerId;
                addLoanScheduleArchive.RelationshipManagerId = item.relationshipManagerId;
                addLoanScheduleArchive.MISCode = item.misCode;
                addLoanScheduleArchive.TeamMISCode = item.teamMiscode;
                addLoanScheduleArchive.InterestRate = item.interestRate;
                addLoanScheduleArchive.EffectiveDate = item.effectiveDate;
                addLoanScheduleArchive.MaturityDate = item.maturityDate;
                addLoanScheduleArchive.BookingDate = item.bookingDate;
                addLoanScheduleArchive.PrincipalAmount = item.principalAmount;
                addLoanScheduleArchive.ApprovedAmount = (decimal)item.approvedAmount;
                addLoanScheduleArchive.PrincipalInstallmentLeft = item.principalInstallmentLeft;
                addLoanScheduleArchive.InterestInstallmentLeft = item.interestInstallmentLeft;
                addLoanScheduleArchive.ApprovalStatusId = item.approvalStatusId;
                addLoanScheduleArchive.ApprovedBy = item.approvedBy;
                addLoanScheduleArchive.ApproverComment = item.approverComment;
                addLoanScheduleArchive.DateApproved = item.dateApproved;
                addLoanScheduleArchive.LoanStatusId = item.loanStatusId;
                addLoanScheduleArchive.CreatedBy = item.createdBy;
                addLoanScheduleArchive.DateTimeCreated = item.dateTimeCreated;
                addLoanScheduleArchive.ScheduleTypeId = item.scheduleTypeId;
                addLoanScheduleArchive.ScheduleDayCountConventionId = item.scheduleDayCountConventionId;
                addLoanScheduleArchive.ScheduleDayInterestTypeId = item.scheduleDayInterestTypeId;
                addLoanScheduleArchive.IsDisbursed = item.isDisbursed;
                addLoanScheduleArchive.DisbursedBy = item.disbursedBy;
                addLoanScheduleArchive.DisburserComment = item.disburserComment;
                addLoanScheduleArchive.DisburseDate = item.disburseDate;
                addLoanScheduleArchive.OperationId = (int)item.operationId;
                addLoanScheduleArchive.CustomerGroupId = item.customerGroupId;
                addLoanScheduleArchive.LoanTypeId = item.loanTypeId;
                //addLoanScheduleArchive.TrancheBatchCode = item.trancheBatchCode;
                addLoanScheduleArchive.EquityContribution = item.equityContribution;
                addLoanScheduleArchive.FirstPrincipalPaymentDate = item.firstPrincipalPaymentDate;
                addLoanScheduleArchive.FirstInterestPaymentDate = item.firstInterestPaymentDate;
                addLoanScheduleArchive.OutstandingPrincipal = item.outstandingPrincipal;
                addLoanScheduleArchive.OutstandingInterest = item.outstandingInterest;
                addLoanScheduleArchive.PrincipalAdditionCount = item.principalAdditionCount;
                addLoanScheduleArchive.PrincipalReductionCount = item.principalReductionCount;
                addLoanScheduleArchive.FixedPrincipal = item.fixedPrincipal;
                addLoanScheduleArchive.ProfileLoan = item.profileLoan;
                addLoanScheduleArchive.DischargeLetter = item.dischargeLetter;
                addLoanScheduleArchive.SuspendInterest = item.suspendInterest;
                addLoanScheduleArchive.IsScheduledPrepayment = item.isScheduledPrepayment;
                addLoanScheduleArchive.AllowForceDebitRepayment = item.allowForceDebitRepayment;
                addLoanScheduleArchive.ScheduledPrepaymentAmount = item.scheduledPrepaymentAmount;
                addLoanScheduleArchive.ScheduledPrepaymentDate = item.scheduledPrepaymentDate;
                addLoanScheduleArchive.ScheduledPrepaymentFrequencyTypeId = item.scheduledPrepaymentFrequencyTypeId;
                addLoanScheduleArchive.CustomerSensitivityLevelId = item.customerSensitivityLevelId;
                addLoanScheduleArchive.InternalPrudentialGuidelineStatusId = item.internalPrudentialGuidelineStatusId;
                addLoanScheduleArchive.ExternalPrudentialGuidelineStatusId = item.externalPrudentialGuidelineStatusId;
                addLoanScheduleArchive.NPLDate = item.nplDate;
                addLoanScheduleArchive.CreatedBy = item.createdBy;
                addLoanScheduleArchive.DateTimeCreated = item.dateTimeCreated;

                loanScheduleArchive.Add(addLoanScheduleArchive);

            }

            this.context.tbl_Loan_Archive.AddRange(loanScheduleArchive);

            context.SaveChanges();
            return model;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool AddLoanScheduleByBulkRate(short priceindexId, double newRate, DateTime applicationDate, int staffId)
        {
            bool output = false;
            // var applicationDate = generalSetup.GetApplicationDate();

            BulkArchiveLoanSchedule(priceindexId);
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
                         });

            foreach (var item in model)
            {
                var unEarnedFee = from d in context.tbl_Loan_Schedule_Daily
                                  where d.LoanId == item.loanId
                                  let sumUnEarnedFee = context.tbl_Loan_Schedule_Daily.Where(a => a.LoanId == item.loanId
                                  && a.Date >= applicationDate).Sum(a => a.UnEarnedFee)
                                  select sumUnEarnedFee;
                item.integralFeeAmount = (double)unEarnedFee.FirstOrDefault();

                UpdateLoanSchedule(item.loanId, item, applicationDate, staffId);
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

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool UpdateLoanPrepaymentSchedule(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();
            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);
            var penalCharge = context.tbl_Charge_Fee.FirstOrDefault(x => x.OperationId == (int)OperationsEnum.LoanPrepayment);
            if (loanInput.principalAmount == loanInput.payAmount)
            {
                var refNo = this.context.tbl_Loan.Where(x => x.TermLoanId == loanInput.loanId).FirstOrDefault().LoanReferenceNumber;
                var interest = from d in context.tbl_Daily_Accrual
                               where d.ReferenceNumber == refNo
                               let sumDailyAccuralAmount = context.tbl_Daily_Accrual.Where(a => a.ReferenceNumber == refNo
                              && a.Date <= applicationDate && a.RepaymentPostedStatus == false).Sum(a => a.DailyAccuralAmount)/// add repaymentpostedstatus = false after scaffording
                               select sumDailyAccuralAmount;
                var accruedInterest = interest.FirstOrDefault();

                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "Accrued Interest"));

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, (decimal)loanInput.payAmount, product.PrincipalBalanceGL.Value, "principal repayment"));

                inputTransactions.Add(financeTransaction.PostBuildLoanPrepaymentPosting(loanInput, accruedInterest, penalCharge.GLAccountId, "Penal Charge"));///change to charge GL

                financeTransaction.PostTransaction(inputTransactions);
                updateloanTableStatus(loanInput.loanId);
            }
            else
            {

                if (LoanExist(loanId) > 0)
                {
                    ArchiveLoanSchedule(loanId);
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

                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    UpdatePeriodicSchedule(loanId, applicationDate);

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

            if (loanInput.newPrincipalFrequency != loanInput.principalFrequency || loanInput.newInterestFrequency != loanInput.interestFrequency)
            {

                if (LoanExist(loanId) > 0)
                {
                    ArchiveLoanSchedule(loanId);
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

                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    UpdatePeriodicSchedule(loanId, applicationDate);

                    context.SaveChanges();
                    //-------------------------------------------------------
                }
            }

            output = true;

            return output;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PaymentDateChange(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            if (loanInput.newPrincipalFirstpaymentDate != loanInput.principalFirstpaymentDate || loanInput.newInterestFirstpaymentDate != loanInput.interestFirstpaymentDate)
            {

                if (LoanExist(loanId) > 0)
                {
                    ArchiveLoanSchedule(loanId);
                    ArchivePeriodicSchedule(loanId);
                    ArchiveDailySchedule(loanId);


                    //----------generate and save periodic loan schedule -----------------------------------

                    loanInput.principalFirstpaymentDate = loanInput.newPrincipalFirstpaymentDate;
                    loanInput.interestFirstpaymentDate = loanInput.newInterestFirstpaymentDate;

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

                    //----------update loan details -----------------------------------
                    var loan = this.context.tbl_Loan.FirstOrDefault(x => x.TermLoanId == loanId);
                    loan.MaturityDate = periodicScheduleTemp.Max(x => x.paymentDate);
                    loan.PrincipalNumberOfInstallment = periodicScheduleTemp.Count() - 1;
                    loan.InterestNumberOfInstallment = loan.PrincipalNumberOfInstallment;
                    //-------------------------------------------------

                    UpdatePeriodicSchedule(loanId, applicationDate);

                    context.SaveChanges();
                    //-------------------------------------------------------
                }
            }

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

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool ChargeReversal(LoanChargeFeeViewModel feeInput, DateTime applicationDate, int staffId)
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

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool LoanReversal(int loanId, LoanPaymentScheduleInputViewModel loanInput, DateTime applicationDate, int staffId)
        {
            bool output = false;
            var systemDate = generalSetup.GetApplicationDate();

            var product = context.tbl_Product.FirstOrDefault(x => x.ProductId == loanInput.productId);

            var interestAmount = (from p in context.tbl_Loan_Schedule_Daily
                                  where p.Date <= applicationDate
                                  select p).SingleOrDefault();

            var interest = from d in context.tbl_Loan_Schedule_Daily
                           where d.Date <= applicationDate
                           let sumDailyAccuralAmount = context.tbl_Loan_Schedule_Daily.Where(a => a.Date <= applicationDate).Sum(a => a.DailyInterestAmount)/// add repaymentpostedstatus = false after scaffording
                           select sumDailyAccuralAmount;
            var accruedInterest = interest.FirstOrDefault();

            var principal = from d in context.tbl_Loan_Schedule_Periodic
                            where d.PaymentDate <= applicationDate
                            let sumPrincipalAmount = context.tbl_Loan_Schedule_Periodic.Where(a => a.PaymentDate <= applicationDate).Sum(a => a.PeriodPrincipalAmount)/// add repaymentpostedstatus = false after scaffording
                            select sumPrincipalAmount;
            var accruedPrincipal = principal.FirstOrDefault();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedInterest, product.InterestReceivablePayableGL.Value, "interest repayment"));

            inputTransactions.Add(financeTransaction.PostBuildLoanReversalPosting(loanInput, accruedPrincipal, product.PrincipalBalanceGL.Value, "principal repayment"));

            financeTransaction.PostTransaction(inputTransactions);



            ArchiveLoanSchedule(loanId);
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
        public IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(int scheduleId)
        {
            if (scheduleId == (int)LoanScheduleTypeEnum.IrregularSchedule)
            {
                return (from data in context.tbl_Operations
                        where data.OperationTypeId == (int)OperationTypeEnum.LoanManagement &&
                        data.OperationId != (int)OperationsEnum.InterestandPrincipalFrequencyChange &&
                         data.OperationId != (int)OperationsEnum.InterestFrequencyChange &&
                           data.OperationId != (int)OperationsEnum.PrincipalFrequencyChange
                        select new LoanOperationTypeViewModel()
                        {
                            operationTypeId = data.OperationId,
                            operationTypeName = data.OperationName
                        });
            }
            else if (scheduleId == (int)LoanScheduleTypeEnum.BallonPayment)
            {
                return (from data in context.tbl_Operations
                        where data.OperationTypeId == (int)OperationTypeEnum.LoanManagement &&
                        data.OperationId != (int)OperationsEnum.InterestandPrincipalFrequencyChange &&
                         data.OperationId != (int)OperationsEnum.InterestFrequencyChange &&
                           data.OperationId != (int)OperationsEnum.PrincipalFrequencyChange
                        select new LoanOperationTypeViewModel()
                        {
                            operationTypeId = data.OperationId,
                            operationTypeName = data.OperationName
                        });
            }
            return (from data in context.tbl_Operations
                    where data.OperationTypeId == (int)OperationTypeEnum.LoanManagement
                    select new LoanOperationTypeViewModel()
                    {
                        operationTypeId = data.OperationId,
                        operationTypeName = data.OperationName
                    });

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
                InterateRate = model.interateRate,
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
                CreatedBy = model.createdBy,
                DateCreated = DateTime.Now
            };
            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDocumentAdded,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Added tbl_Loan_Review_Operation '{ data.LoanReviewOperationsId }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            
            //end of Audit section -----------------------
            if (workFlow.CheckRouteForOperation((int)OperationsEnum.ContractualInterestRateChange, model.companyId))
            {
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
                            operationId = (int)OperationsEnum.RevolvingLoanBooking,
                            BranchId = model.userBranchId
                        };
                        var response =  workFlow.LogForApproval(approvalModel);
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
            return false;
        }
    }
}
