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

        // string PostTransaction(List<FinanceTransactionViewModel> transaction, bool isBulkPosting = false);
        string PostTransaction(List<FinanceTransactionViewModel> inputTransactions, bool isBulkPosting = false, TwoFactorAutheticationViewModel twoFADetails = null);

        CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId);

        CasaBalanceViewModel GetCASABalance(int casaAccountId);

        bool PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description,int operationId);

        FinanceTransactionViewModel PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description, int operationId);

        FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId);

        decimal GetLienBalance(string productAccountNumber);

        bool PostBuildLoanChargeFeesPosting(LoanViewModel model);
        // FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId, TwoFactorAutheticationViewModel twoFactorAuth);
        FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth, decimal postedAmount, int creditGL, string description, int operationId);
       // FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        FinanceTransactionViewModel BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model);

        FinanceTransactionViewModel PostBuildLoanReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId);

        void UpdateCustomTransactions(string batchCode);

        bool BulkIntegrationPosting(FinanceTransactionStagingViewModel model);

    }
}