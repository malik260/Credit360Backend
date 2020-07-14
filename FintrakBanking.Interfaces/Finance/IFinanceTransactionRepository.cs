using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
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

        bool PostDailyWriteoffLoansInterestAccrual(DailyInterestAccrualViewModel model);

        FinanceTransactionViewModel PostTerminateAndRebookDoubleEntries(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, int creditGL, string description, TwoFactorAutheticationViewModel twoFactorAuth);

        FinanceTransactionViewModel PostTerminateAndRebookEntries(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, int creditGL, string description, TwoFactorAutheticationViewModel twoFactorAuth);
        CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId);

        List<FinanceTransactionViewModel> BuildLoanContingentFeesReversal(int loanApplicationDetailId, int staffId);

        List<FinanceTransactionViewModel> BuildRecapitalisationAccuredInterestReceivablePosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);
        CasaBalanceViewModel GetCASABalance(int casaAccountId);

        CasaBalanceViewModel GetCASABalance(string accountNumber, int companyId);

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
        List<FinanceTransactionViewModel> BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth, string postType);

        List<FinanceTransactionViewModel> BuildLoanChargeFeesPostingReversal(LoanViewModel loanDetails, LoanChargeFeeViewModel feeModel );

        List<FinanceTransactionViewModel> BuildContingentUnEarnedFeePostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId);

        List<FinanceTransactionViewModel> BuildContingentChargeFeePosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId);

        List<FinanceTransactionViewModel> BuildContingentPrincipalPosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId);
        List<FinanceTransactionViewModel> BuildContingentPrincipalPostingReduction(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId);

        FinanceTransactionViewModel PostLoanPositiveReversalCasaEntries(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);
        FinanceTransactionViewModel PostLoanPositiveReversalEntries(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        FinanceTransactionViewModel PostBuildLoanNegativeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId);

        FinanceTransactionViewModel TerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        List<FinanceTransactionViewModel> BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        //FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description);

        FinanceTransactionViewModel PostTerminateAndRebookEntries(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, string description, TwoFactorAutheticationViewModel twoFactorAuth);

        FinanceTransactionViewModel PostLoanGLEntries(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, int creditGL, string description, int operationId);

        FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId);

        void UpdateCustomTransactions(string batchCode);

        bool BulkIntegrationPosting(FinanceTransactionStagingViewModel model);

        string GetCustomerAccountType(string accountNumber);
        FinanceTransactionViewModel PostEarnUnEarnedFeeOperationEntries(DailyInterestAccrualViewModel model, decimal postedAmount, string description, int operationId, int loanSystemTypeId);
        List<TrialBalanceViewModel> GetTrialBalanceSummary(ReportSearchEntity entity, int companyId);

        TrialBalanceViewModel GetExportedTrialBalanceSummary(ReportSearchEntity entity, int companyId);
        List<TrialBalanceViewModel> GetGLandAccountName();

    }
}