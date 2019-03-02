using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using FintrakBanking.ViewModels.Operations;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanOperationsRepository
    {
        bool DoesChargeFeeExist(int loanId, int operationTypeId, int chargeFeeId);
        bool DoesOperationExist(int loanId, int operationTypeId, short loanSystemTypeId);
        int GoForApproval(ApprovalViewModel entity);
        bool AddCollateralSearchLien(CasaLienViewModel model);
        string GetCollateralLoanNewRefernceNumber(ApprovalViewModel model);

        bool DailyWrittenOffFacilityAccrual(DateTime applicationDate);

        decimal GetCollateralSearchChargeAmount(int stateId);
        bool AddOperationReview(LoanReviewOperationViewModel model);
        IEnumerable<LoanOperationTypeViewModel> GetOperationType();

        bool UpdateLoanClassification(DateTime applicationDate);

        void ProcessGlobalInterestRepricing(DateTime effectiveDate, int productPriceIndexID, short staffId);

        bool ProcessReleaseLien(DateTime applicationDate);
            
        bool ProcessContingentLiabilityTerminationAtMaturity(DateTime date);

        void ProcessAutomaticInterestRepricing(DateTime applicationDate, int staffId);

        LoanViewModel GetRunningLoanOpeningBalance(int companyId, string refNo,DateTime effectiveDate);

        bool ContingentLiabilityTenorExtension(TwoFactorAutheticationViewModel twoFactorAuth, LoanPaymentRestructureScheduleInputViewModel model, string approvalComment);

        bool ContingentLiabilityAmountReduction(TwoFactorAutheticationViewModel twoFactorAuth, LoanPaymentRestructureScheduleInputViewModel model, string approvalComment);

        List<LoanPaymentSchedulePeriodicViewModel> GeneratePrepaymentSchedule(LoanPaymentScheduleInputViewModel loanInput);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTermLoansInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyInterestOnPastDueInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyInterestOnPastDuePrincipalAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTaxAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyFeeAccrual(DateTime applicationDate);

        IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetApprovedLoanOperationReview();
        IEnumerable<ApprovalTrailDetailsViewModel> GetApprovalDetails(int loanId, int OperationId);
        IEnumerable<LoanCovenantDetailViewModel> ProcessIDFExpiryAndlocking(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCovenant(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCleanUp(DateTime applicationDate);
        IEnumerable<LoanViewModel> ProcessIntervalFeeandCommissionPosting(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessCFFExpiryAndlocking(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessLPOExpiryAndlocking(DateTime applicationDate);
        IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(DateTime applicationDate);
        bool LoanCancellation(int loanId, DateTime applicationDate, int staffId);
        bool OverdraftTopUp(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, decimal amount);
        //bool OverdraftTopUp(int loanId, decimal amount);
        IEnumerable<LimitSuspensionViewModel> ProcessNPLByBranchSuspension();
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId);
        LoanViewModel ArchiveLoan(int loanId, int operationId, string archiveBatchCode, string changeReason);
        IEnumerable<LoanViewModel> BulkArchiveLoan();
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId, string archiveBatchCode);
        IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId, string archiveBatchCode);
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule(int loanId, DateTime applicationDate);
        bool LoanRephasementProcess(TwoFactorAutheticationViewModel twoFactorAuth, int loanReviewOperationsId, int loanId, int staffId, LoanSystemTypeEnum facilityType, [Optional] string approvalComment);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview(DateTime applicationDate, int loanId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate);
        bool BulkRateReview(short priceindexId, double newRate, DateTime applicationDate, int staffId, int operationId);
        IEnumerable<LoanViewModel> GetLoanRateCustomerExcemptions(int companyId);
        bool addBulkRateLoanExcemptions(LoanViewModel model);
        bool addInterestRateChange(LoanBulkInterestReviewViewModel model);
        IEnumerable<LoanBulkInterestReviewViewModel> GetNewInterestRateReviews(int companyId);
        IEnumerable<LoanClassificationViewModel> CalculateLoanClassification(DateTime applicationDate);
        LoanViewModel GetRunningLoans(int companyId, string refNo);
        LoanViewModel GetRunningFXLoans(int companyId, string refNo);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByOD();
        IEnumerable<LoanOperationTypeViewModel> GetRemedialOperationType();
        IEnumerable<LoanFeeOperationViewModel> GetLoanChargeFeeByLoanId(int loanId);

        IEnumerable<LoanClassificationViewModel> CalculateOverdraftClassification(DateTime applicationDate);
        IEnumerable<LoanViewModel> LoanHistory();
        IEnumerable<RevolvingLoanViewModel> OverDraftHistory();

        IEnumerable<LoanReviewIrregularScheduleViewModel> GetLoanReviewOperationIrregularSchedule(int loanReviewOperationId);

        void ProcessGlobalInterestRepricing(DateTime effectiveDate, int productPriceIndexID, short staffId, int isMarketInduced, int productPriceIndexGlobalId);

        bool AddOperationReviewContingent(LoanReviewOperationViewModel model);
        bool AddOperationReviewContingentWithImage(LoanReviewOperationViewModel model, byte[] buffer);

        bool DeleteLoanExistingOnDailyAndPeriodicSchedule(int loanId);

        bool SendEmailToRecoveryAgent(int companyId, int staffId, short branchId, int accreditedConsultantId);

        #region COMMERCIAL PAPER LOANS
        bool SubAllocateCommercialLoanPrincipal(subAllocationViewModel models);
        IEnumerable<MaturityIntructionViewModel> GetMaturityInstructionType();
        bool ApproveMaturityInstructionRequest(MaturityIntructionViewModel model);
        bool addMaturityInstruction(MaturityIntructionViewModel model);
        IEnumerable<MaturityIntructionViewModel> GetLoanMaturityInstructions();
        bool ApproveCommercialPaperManualRollOverRequest(MaturityIntructionViewModel model, string refNo);
        bool RolloverCommercialLoanByManualProcess(MaturityIntructionViewModel model, string refNo);
        //void CommercialPaperManualRollOver(DateTime applicationDate);        
        int addApplicationGoForApproval(ApprovalViewModel userModel);

        bool ApproveNonTermLoanTenorReviewRequest(LoanReviewViewModel userModel);
        bool ReviewNonTermLoanTenor(LoanReviewViewModel userModel);
        List<LoanReviewOperationParentChildViewModel> GetRunningCommercialLoanLines(int companyId);
        bool AproveApplicationLineRateChangeRequest(LoanReviewViewModel userModel);
        bool ReviewApplicationLineRate(LoanReviewViewModel userModel);
        List<LoanReviewOperationParentChildViewModel> GetCommercialLoansLines(int companyId);
        List<LoanReviewOperationApprovalViewModel> GetDueCommercialLoans(int companyId);
        List<LoanReviewOperationApprovalViewModel> GetDueCommercialLoansByApplicationDetailId(int companyId, int loanApplicationDetailID);
        //IEnumerable<DailyInterestAccrualViewModel> ProcessDailyCommercialPaperInterestAccrual(DateTime applicationDate);
        void CommercialPaperChangeOperativeAccount(int casaPayAccountId, int newCasaPayAccountId);
        bool CommercialPaperDetailsCancellation(string refNo, DateTime applicationDate, int staffId);
        //loanPrepaymentViewModel addCommercialLoanPrepayment(string refNo, loanPrepaymentViewModel model);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetRunningCommercialLoans(int companyId, string loanReferenceNumber);
        int LineOperationGoForApproval(ApprovalViewModel userModel);
        bool AproveApplicationLineTenorChangeRequest(LoanReviewViewModel userModel);
        IEnumerable<CamProcessedLoanViewModel> GetApplicationLineTenorChangeAwaitingApproval(int staffId, int companyId);
        bool ApproveNonTermLoanLoanRateChangeRequest(LoanReviewViewModel userModel);

        bool ReviewNonTermLoanLoanRate(LoanReviewViewModel userModel);
        bool ApproveApplicationLineAmountChangeRequest(LoanReviewViewModel userModel);

        bool changeApplicationLineAmount(LoanReviewViewModel userModel);
        bool GetRepaymentFromStaging();

        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByContingent();

        IEnumerable<LoanRepaymentViewModel> ProcessLoanDisbursmentRollOver(DateTime applicationDate);

        void ProcessAutomaticCommercialLoanRollover(DateTime applicationDate);

        #endregion

        #region
        List<ItemValue> FlowTypes();
        #endregion
    }
}