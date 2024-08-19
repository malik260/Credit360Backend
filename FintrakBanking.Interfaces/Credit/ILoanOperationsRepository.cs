using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.General;
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
        IEnumerable<LoanReviewOperationApprovalViewModel> GetBulkUnassignmentRetailRecoveryFromAgentAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetBulkRetailRecoveryToAgentAwaitingApproval(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllUnassignLoansRecoveryAnalysisByAgent(int staffId, int companyId, int accreditedConsultantId, int referenceId);
        WorkflowResponse GoForBulkUnassignLoansFromAgentApproval(List<BulkRecoveryApprovalViewModel> entity, UserInfo user, int approvalStatusId, string comment);
        WorkflowResponse GoForUnassignLoansFromAgentApproval(ApprovalViewModel entity);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetBulkUnassignmentRecoveryFromAgentAwaitingApproval(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllUnassignedRecoveryOperationByAgent(string source, int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationByLoanReferenceAwaitingApproval(int staffId, int companyId, string searchString);
        IEnumerable<RecoveryCollectionsViewModel> GetAllRecoveryCustomersAssignedToAgent(int recoveryAgent);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansRecoveryAnalysisByAgentRemedial(int staffId, int companyId, int accreditedConsultantId, string referenceId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansForRecoveryAnalysisBySingleAgent(string source, int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansForExternalRecoveryAnalysisByAgent(string source, int staffId, int companyId, DateTime month);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansForRecoveryAnalysisByAgent(string source, int staffId, int companyId, DateTime month);
        IEnumerable<RetailLoanRecoveryCommissionViewModel> getAllInternalRecoveryCommissonByAgents(int staffId, int companyId, DateTime month);
        IEnumerable<AccreditedConsultantsViewModel> GetAllInternalRecoveryAgents(int staffId, int companyId, DateTime month);
        IEnumerable<AccreditedConsultantsViewModel> GetAllRecoveryAgents(int staffId, int companyId);
        IEnumerable<RetailLoanRecoveryCommissionViewModel> getAllRecoveryReportCollectionByAgents(int staffId, int companyId, DateTime month);
        IEnumerable<LoanReviewOperationApprovalViewModel> getAllPendingEmailAlert(string source, int staffId, int companyId);
        bool generateRecoveryMailToAgents(string source, int staffId, int companyId);
        WorkflowResponse GoForBulkAssignLoansToAgentApproval(List<BulkRecoveryApprovalViewModel> entity, UserInfo user, int approvalStatusId, string comment);
        IEnumerable<RetailLoanRecoveryCommissionViewModel> getAllRecoveryCommissonByAgents(int staffId, int companyId, DateTime month);
        IEnumerable<MultipleInsuranceOutputApprovalViewModel> GetBulkInsuranceUploadRejectedApproval(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> GetAllLoansRecoveredByAgents(int staffId, int companyId);
        //bool GoForDocumentationFillingApproval(ApprovalViewModel entity);
        int GoForDocumentationFillingApproval(ApprovalViewModel entity);
        WorkflowResponse GoForBulkInsuranceUploadApproval(ApprovalViewModel entity);
        WorkflowResponse GoForMultipleBulkInsuranceUploadApproval(List<MultipleInsuranceOutputViewModel> entity, UserInfo user, int approvalStatusId, string comment);
        IEnumerable<CamProcessedLoanViewModel> GetLoanOperationDocumentationLosApproval(int staffId, int companyId);
        IEnumerable<MultipleInsuranceOutputApprovalViewModel> GetBulkInsuranceUploadAwaitingApproval(int staffId, int companyId);
        WorkflowResponse GoForRecoveryCommissionApproval(ApprovalViewModel entity);
        IEnumerable<LoanRecoveryCommissionApprovalViewModel> GetBulkRecoveryCommissionAwaitingApproval(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansRecoveryCommissionByReference(int staffId, int companyId, string referenceId);
        IEnumerable<LoanRecoveryCommissionApprovalViewModel> GetBulkRecoveryCommissionAwaitingApprovalList(int staffId, int companyId);
        IEnumerable<LoanRecoveryCommissionApprovalViewModel> BulkRecoveryCommissionApplicationList(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllPaymentRecoveredCommissionByAgent(int staffId, int companyId);
        IEnumerable<LoanRecoveryReportApprovalViewModel> GetBulkRecoveryReportingAwaitingApproval(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> GetAllLoansRecoveredByAgentForReporting(int staffId, int companyId);
        IEnumerable<LoanRecoveryReportApprovalViewModel> GetBulkRecoveryReportingAwaitingApprovalList(int staffId, int companyId);
        WorkflowResponse GoForRecoveryReportingApproval(ApprovalViewModel entity);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansRecoveryReportingByReference(int staffId, int companyId, string referenceId);
        IEnumerable<LoanRecoveryReportApprovalViewModel> BulkRecoveryReportingApplicationList(int staffId, int companyId);
        bool CreditDocumentationFillingLos(CreditDocumentationViewModel model);
        IEnumerable<CamProcessedLoanViewModel> GetLoanOperationDocumentationLos(int staffId, int companyId);
        IEnumerable<CamProcessedLoanViewModel> GetLoanOperationDocumentationLosSearch(int staffId, int companyId, string searchString);
        IEnumerable<CamProcessedLoanViewModel> GetAllCompletedLoanOperationDocumentationLos(int staffId, int companyId, DateTime startDate, DateTime endDate);
        IEnumerable<LoanReviewOperationApprovalViewModel> BulkRecoveryToAgentAwaitingApprovalList(string source, int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetBulkRecoveryToAgentAwaitingApprovalList(string source, int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansRecoveryAnalysisByAgent(int staffId, int companyId, int accreditedConsultantId, string referenceId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetBulkRecoveryToAgentAwaitingApproval(int staffId, int companyId);
        WorkflowResponse GoForAssignLoansToAgentApproval(ApprovalViewModel entity);
        int GoForLienRemovalApproval(ApprovalViewModel entity);
        IEnumerable<RemoveLienViewModel> GetLienRemovalDocuments(int lienRemovalId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLienRemovalAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLienSearchData(string searchString);
        int AddRequestUnLienODAccount(RemoveLienViewModel model, byte[] buffer);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetAllLoansOperationWriteOffAnalysis(int staffId, int companyId);

        List<LoanViewModel> GetCurrentPrepayment(int companyId);

        List<LoanViewModel> GetRunningPrepaymentLoans(int companyId, int loanId);

        List<LoanViewModel> AddBulkPrepaymentReversal(LoanReviewOperationViewModel model, int companyId);
        bool AddBulkPrepaymentReversalData(LoanViewModel data, int batchCode, DateTime applicationDate);
        string GetTransactionReferenceNo();
        CollectionsRetailComputationVariableSetupViewModel getAllRecoveryComputationVariables(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> GetAllLoansRecoveredByAgent(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> getAllLoansOperationRecoveryAnalysisByAgent(string source, int staffId, int companyId, DateTime month);
        IEnumerable<GlobalExposureApplicationViewModel> GetLoanOperationRecoveryAnalysis(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> GetDelinquentAccounts(int staffId, int companyId);
        IEnumerable<GlobalExposureApplicationViewModel> GetDelinquentDigitalAccounts(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationDocumentation(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationDocumentationSearch(int staffId, int companyId, string searchString);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetCompletedLoanOperationDocumentation(int staffId, int companyId, DateTime startDate, DateTime endDate);
        bool CreditDocumentationFilling(CreditDocumentationViewModel model);
        bool UpdateLoanClassification(DateTime applicationDate, int companyId);
        bool DoesChargeFeeExist(int loanId, int operationTypeId, int chargeFeeId);
        bool DoesOperationExist(int loanId, int operationTypeId, int loanSystemTypeId);
        WorkflowResponse GoForApproval(ApprovalViewModel entity);
        bool AddCollateralSearchLien(CasaLienViewModel model);
        string GetCollateralLoanNewRefernceNumber(ApprovalViewModel model);

        LoanViewModel getPrincipalAndInterestDate(int companyId, string refNo, DateTime effectiveDate);

        bool DailyWrittenOffFacilityAccrual(DateTime applicationDate, int companyId, int staffId);

        TBL_LOAN GetLoanInformation(int loanid);

        bool GetRepaymentDate(int loanId);

        bool FullAndFinalCompleteWriteOff(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId);

        decimal GetCollateralSearchChargeAmount(int stateId);
        bool AddOperationReview(LoanReviewOperationViewModel model);
        IEnumerable<LoanOperationTypeViewModel> GetOperationType(bool isFinalOperation);

        bool UpdateLoanClassification(DateTime applicationDate, int companyId, int staffId);

        bool ProcessGlobalInterestRepricing(DateTime effectiveDate, int productPriceIndexID, short staffId);

        bool ProcessReleaseLien(DateTime applicationDate, int companyId, int staffId);

        bool ProcessContingentLiabilityTerminationAtMaturity(DateTime date);

        void ProcessAutomaticInterestRepricing(DateTime applicationDate, int staffId, int companyId);

        bool LoanRecoveryCompletion(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId);

        LoanViewModel GetRunningLoanOpeningBalance(int companyId, string refNo, DateTime effectiveDate);

        bool ContingentLiabilityTenorExtension(TwoFactorAutheticationViewModel twoFactorAuth, LoanPaymentRestructureScheduleInputViewModel model, string approvalComment);

        bool ContingentLiabilityAmountReduction(TwoFactorAutheticationViewModel twoFactorAuth, LoanPaymentRestructureScheduleInputViewModel model, string approvalComment);

        bool LoanRecoveryPayment(int loanId, LoanPaymentRestructureScheduleInputViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId);

        List<LoanPaymentSchedulePeriodicViewModel> GeneratePrepaymentSchedule(LoanPaymentScheduleInputViewModel loanInput);

        List<DailyInterestAccrualViewModel> ProcessDailyTermLoansInterestAccrual(DateTime applicationDate, int companyId, int staffId, FinTrakBankingContext context);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyInterestOnPastDueInterestAccrual(DateTime applicationDate, int companyId, int staffId);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyInterestOnPastDuePrincipalAccrual(DateTime applicationDate, int companyId, int staffId);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTaxAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyFeeAccrual(DateTime applicationDate, int companyId, int staffId);

        IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId);
        IEnumerable<LoanOperationTypeViewModel> GetReviewApprovalOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId);
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
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate, int companyId, int staffId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue(DateTime applicationDate, int companyId, int staffId);
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

        void ProcessGlobalInterestRepricing(DateTime effectiveDate, int productPriceIndexID, short staffId, bool isMarketInduced, int productPriceIndexGlobalId);

        bool AddOperationReviewContingent(LoanReviewOperationViewModel model);
        bool AddOperationReviewContingentWithImage(LoanReviewOperationViewModel model, byte[] buffer);
        bool SaveMainDocument(LoanReviewOperationViewModel model, int loanId, byte[] file, int loanreviewoperationId);
        bool SaveDocument(LoanReviewOperationViewModel model, byte[] file);
        bool DeleteLoanExistingOnDailyAndPeriodicSchedule(int loanId);

        bool SendEmailToRecoveryAgent(int companyId, int staffId, short branchId, int accreditedConsultantId);

        LoanViewModel GetWriteOffLoans(int companyId, string refNo);



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

        IEnumerable<LoanRepaymentViewModel> ProcessLoanDisbursmentRollOver(DateTime applicationDate, int companyId);

        void ProcessAutomaticCommercialLoanRollover(DateTime applicationDate, int companyId, int staffId);

        #endregion

        #region
        List<ItemValue> FlowTypes();
        #endregion
    }
}