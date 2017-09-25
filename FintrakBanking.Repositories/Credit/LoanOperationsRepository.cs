using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
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

        public LoanOperationsRepository(

        FinTrakBankingContext _context, IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail)
        {

            this.context = _context;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
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

    }
}
