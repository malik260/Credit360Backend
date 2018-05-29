using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Finance
{
    public interface IFinanceTransactionRepository
    {
        //bool AddCollateralSearchLien(CasaLienViewModel model);

        //List<FinanceTransactionViewModel> PostCollateralSearch(CasaLienViewModel model);

        string PostTransaction(List<FinanceTransactionViewModel> transaction);

        CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId);

        CasaBalanceViewModel GetCASABalance(int casaAccountId);

        bool PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description);

        FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description);

        CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId);

        decimal GetLienBalance(string productAccountNumber);

        FinanceTransactionViewModel PostBuildLoanChargeFeesPosting(LoanViewModel model);

        FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model);

        FinanceTransactionViewModel PostBuildLoanReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId);

        void UpdateCustomTransactions(string batchCode);

    }
}