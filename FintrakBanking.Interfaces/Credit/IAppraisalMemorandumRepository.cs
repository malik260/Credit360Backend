using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IAppraisalMemorandumRepository
    {
        AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId);

        IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId);

        AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model);

        WorkflowResponse ForwardAppraisalMemorandum(ForwardViewModel model);

        bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId);

        IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId, int operationId);

        // IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId);
        LoanApplicationDetailsViewModel GetLoanApplicationDetail(int applicationId);
        IEnumerable<LookupViewModel> GetAllCRMSSecuredCollateralType(int companyid);
        IEnumerable<LookupViewModel> GetAllCRMSAllCollateralType(int companyid);

        IEnumerable<LookupViewModel> GetAllCRMSUnsecuredCollateralType(int companyid);
        Task<bool> UpdateLoadDetails(int applicationId, ApprovedLoanDetailViewModel model);

        IEnumerable<LoanDetailsFeeViewModel> GetLoanDetailsFee(int applicationId);

        IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId);

        //bool Confirmation(int type, int applicationId);

        IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int applicationId, int countryId, int branchId, int staffId, int? classId);

        IEnumerable<CurrentCommitteeViewModel> GetCurrentCommittee(int loanApplicationId);

        bool SecretariatForwardAppraisalMemorandum(ForwardCommitteeCamViewModel entity);

        IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId);

        List<PendingProductProgramViewModel> GetPendingProductProgram(UserInfo user);

        bool GetUntenoredStatus(int applicationId);

        PrivilegeViewModel GetUserPrivilege(AuthoritySignatureViewModel entity);

        IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggers(int applicationId);

        IEnumerable<MonitoringTriggersViewModel> SaveApplicationMonitoringTriggers(int applicationId, List<MonitoringTriggersViewModel> entity, int staffId);

        bool WorkflowTest();

        List<RepaymentScheduleTermsViewModel> SaveRepaymentScheduleAndTerms(RepaymentScheduleTermsViewModel entity);
        List<ProductLimitValidationViewModel> SaveProductLimitValidation(ProductLimitValidationViewModel entity);
        List<ProductLimitValidationViewModel> GetProductLimitValidation(int applicationId, int classId);

        List<RecommendedCollateralViewModel> GetRecommendedCollateral(int applicationId);
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
        LoanApplicationDetailsViewModel GetLMSLoanApplicationDetail(int applicationId);
    }
}