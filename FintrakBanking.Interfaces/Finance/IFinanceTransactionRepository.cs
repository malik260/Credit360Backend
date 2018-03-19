using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Finance
{
    public interface IFinanceTransactionRepository
    {
        //bool AddCollateralSearchLien(CasaLienViewModel model);

        List<FinanceTransactionViewModel> PostCollateralSearch(CasaLienViewModel model);

        string PostTransaction(List<FinanceTransactionViewModel> transaction);

        CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId);

        CasaBalanceViewModel GetCASABalance(int casaAccountId);

        FinanceTransactionViewModel PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description);

        CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId);

        decimal GetLienBalance(string productAccountNumber);

        FinanceTransactionViewModel PostBuildLoanChargeFeesPosting(LoanViewModel model);

        FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel BuildChargeReversalPosting(LoanChargeFeeViewModel model);

        FinanceTransactionViewModel PostBuildLoanReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel BuildCustomerApplicationChargeOrChargeReversalPosting(string postType, int loanId, GeneralEntity model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId);
    }
}