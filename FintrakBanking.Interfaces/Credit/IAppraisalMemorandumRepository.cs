using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.Entities.Models;
using System;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IAppraisalMemorandumRepository
    {
        ProjectRiskRatingViewModel AddProjectRiskRating(ProjectRiskRatingViewModel projectRiskRating);
        bool AddContractorTiering(ContractorTieringViewModel contractorCriteria);
        bool AddIBLCheclistDetail(IBLChecklistViewModel iblChecklistDetail);
        //IEnumerable<ApprovalTrailCallMemoViewModel> GetAppraisalMemorandumTrailCallMemo(int operationId);
        IEnumerable<ApprovalTrailViewModel> GetGlobalInterestRateChangeTrail(int applicationId, int operationid);
        IEnumerable<ApprovalTrailViewModel> GetCallmemoApprovalTrail(int applicationId, int operationId);
        AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId);

        IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId);

        AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model);

        WorkflowResponse ForwardAppraisalMemorandum(ForwardViewModel model);

        WorkflowResponse AdhocAppraisalMemorandum(ForwardViewModel model);

        WorkflowResponse LcAppraisalMemorandum(LcForwardViewModel model);

        WorkflowResponse LcReleaseMemorandum(LcForwardViewModel model);
        WorkflowResponse LcCancelationMemorandum(LcForwardViewModel model);
        WorkflowResponse LcEnhancementMemorandum(LcForwardViewModel model);
        WorkflowResponse LcIssuanceExtensionMemorandum(LcForwardViewModel model);
        WorkflowResponse LcUsanceExtensionMemorandum(LcForwardViewModel model);
        WorkflowResponse LcUssanceMemorandum(LcForwardViewModel model);

        WorkflowResponse LetterGenerationRequestMemorandum(LetterGenerationRequestViewModel model);

        String ResponseMessage(WorkflowResponse response, string itemHeading);
        //WorkflowResponse CollateralSwapMemorandum(CollateralSwapViewModel model);

        bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId);

        IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId, int operationId, bool all);
        IEnumerable<ApprovalTrailViewModel> GetTrailForReferBack(int applicationId, int operationId, int currentLevelId, bool all, bool isClassified, bool isLMSCrossWorkflow = false);

        // IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId);
        LoanApplicationDetailsViewModel GetLoanApplicationDetail(int applicationId);
        LoanApplicationDetailsViewModel GetApprovedTrancheDetail(int bookingRequestId);
        IEnumerable<LookupViewModel> GetAllCRMSSecuredCollateralType(int companyid);
        IEnumerable<LookupViewModel> GetAllCRMSAllCollateralType(int companyid);

        IEnumerable<LookupViewModel> GetAllCRMSUnsecuredCollateralType(int companyid);
        IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId);

        IEnumerable<LoanDetailsFeeViewModel> GetLoanDetailsFee(int applicationId);


        //bool Confirmation(int type, int applicationId);

        IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int applicationId, int countryId, int branchId, int staffId, int? classId, bool isSpecific = false);
        IQueryable<SubsidiaryViewModel> GetSubsidiaryPendingLoanApplications(int applicationId, int countryId, int branchId, int staffId, int? classId, string staffRoleCode, bool isSpecific = false);
        Task<IEnumerable<SubsidiaryViewModel>> GetSubsidiaries();
        List<LoanApplicationViewModel> CalculateSLA(List<LoanApplicationViewModel> apps);
        IQueryable<LoanApplicationViewModel> GetPoolApplications(int operationId, int companyId, int branchId, int staffId, int? classId);
        bool AssignApplication(int approvalTrailId, int staffId, GeneralEntity entity);
        bool ChangeApplicationOwner(int loanApplicationId, int staffId, GeneralEntity entity);

        bool SelfAssignMultpleApplication(List<ForwardViewModel> models, GeneralEntity userEntity);
        bool ReassignMultipleRequests(List<int> models, GeneralEntity userEntity, int staffId);

        bool ReturnAssignApplicationToPool(int approvalTrailId, GeneralEntity model);

        IQueryable<LoanApplicationViewModel> GetPendingAdhocApplications(int applicationId, int countryId, int branchId, int staffId, int? classId);

        IEnumerable<CurrentCommitteeViewModel> GetCurrentCommittee(int loanApplicationId);

        bool SecretariatForwardAppraisalMemorandum(ForwardCommitteeCamViewModel entity);

        IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId);

        List<PendingProductProgramViewModel> GetPendingProductProgram(UserInfo user);
        
        bool GetUntenoredStatus(int applicationId);

        PrivilegeViewModel GetUserPrivilege(AuthoritySignatureViewModel entity);
        PrivilegeViewModel GetUserPrivilegeByCode(AuthoritySignatureViewModel entity);

        IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggers(int applicationId);

        IEnumerable<MonitoringTriggersViewModel> SaveApplicationMonitoringTriggers(int applicationId, List<MonitoringTriggersViewModel> entity, int staffId);

        bool WorkflowTest();
        string GetAllOldApplicationReference(string data);
        ApprovalTrailViewModel GetapprovalTrailByTrailId(int approvalTrailId);

        IEnumerable<RepaymentScheduleTermsViewModel> SaveRepaymentScheduleAndTerms(RepaymentScheduleTermsViewModel entity);
        IEnumerable<RepaymentScheduleTermSetupViewModel> GetAllSetupRepaymentTerms();
        List<ProductLimitValidationViewModel> SaveProductLimitValidation(ProductLimitValidationViewModel entity);
        List<ProductLimitValidationViewModel> GetProductLimitValidation(int applicationId, int classId);

        List<RecommendedCollateralViewModel> GetRecommendedCollateral(int applicationId, int staffId);
        List<RecommendedCollateralViewModel> AddRecommendedCollateral(RecommendedCollateralViewModel entity);
        List<RecommendedCollateralViewModel> UpdateRecommendedCollateral(RecommendedCollateralViewModel entity);
        IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggersLms(int applicationId);
        IEnumerable<MonitoringTriggersViewModel> SaveApplicationMonitoringTriggersLms(int applicationId, List<MonitoringTriggersViewModel> entity, int getStaffId);
        List<RepaymentScheduleTermsViewModel> SaveRepaymentScheduleAndTermsLms(RepaymentScheduleTermsViewModel entity);
        List<RecommendedCollateralViewModel> UpdateRecommendedCollateralLms(RecommendedCollateralViewModel entity);
        List<RecommendedCollateralViewModel> AddRecommendedCollateralLms(RecommendedCollateralViewModel entity);
        List<RecommendedCollateralViewModel> GetRecommendedCollateralLms(int applicationId);
        bool saveTranchDisbursmentApprovalLevel(TranchDisbursmentViewModel entity);
        List<RecommendedCollateralViewModel> GetRecommendedCollateralHistory(int applicationId);
        List<RecommendedCollateralViewModel> GetRecommendedCollateralHistoryLms(int applicationId);
        LoanApplicationDetailsViewModel GetSingleLoanApplicationDetail(int detailId);
        LoanApplicationDetailsViewModel GetLMSLoanApplicationDetail(int applicationId);

        WorkflowResponse GetWorkflowNextStatus(ForwardViewModel model);

        WorkflowResponse GetWorkflowNextStatusLms(ForwardReviewViewModel model);

        IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggersByOperationId(int operationId, int applicationDetailId);
        LoanApplicationDetailsViewModel GetLoanApplicationDetailByRefNo(string applicationReferenceNumber);

        void LoanStatusChangeThroughAPI(TBL_LOAN_APPLICATION loanApplication, string comment, int staffId, string statusCode);

        bool UpdateSubsidiaryBasicTransaction(int id,ForwardViewModel entity);

        SubsidiaryViewModel GetSubsidiaryBasicApprovalLevel(int id);

    }
}