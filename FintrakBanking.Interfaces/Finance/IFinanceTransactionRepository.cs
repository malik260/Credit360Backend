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

        List<FinanceTransactionViewModel> BuildRecapitalisationAccuredInterestReceivablePosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);
        CasaBalanceViewModel GetCASABalance(int casaAccountId);

        bool PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description,int operationId);

        List<FinanceTransactionViewModel> BuildContingentPrincipalPostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId);

        FinanceTransactionViewModel PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description, int operationId);

        FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId);

        decimal GetLienBalance(string productAccountNumber);

        bool PostBuildLoanChargeFeesPosting(LoanViewModel model);
        // FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId, TwoFactorAutheticationViewModel twoFactorAuth);
        List<FinanceTransactionViewModel> BuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth, decimal postedAmount, int creditGL, string description, int operationId);
        
        // FinanceTransactionViewModel PostBuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);
        FinanceTransactionViewModel BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth);

        //FinanceTransactionViewModel BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model);

        List<FinanceTransactionViewModel> BuildContingentUnEarnedFeePostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId);

        List<FinanceTransactionViewModel> BuildContingentChargeFeePosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId);

        List<FinanceTransactionViewModel> BuildContingentPrincipalPosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId);

        FinanceTransactionViewModel PostBuildLoanPositiveReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        FinanceTransactionViewModel PostBuildLoanNegativeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, TwoFactorAutheticationViewModel twoFactorAuth);

        FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId);

        void UpdateCustomTransactions(string batchCode);

        bool BulkIntegrationPosting(FinanceTransactionStagingViewModel model);

        string GetCustomerAccountType(string accountNumber);

    }
}