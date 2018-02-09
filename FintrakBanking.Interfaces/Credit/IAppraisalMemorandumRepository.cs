using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IAppraisalMemorandumRepository
    {
        AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId);

        IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId);

        AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model);

        int ForwardAppraisalMemorandum(ForwardViewModel model);

        bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId);

        IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId, int operationId);

        IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId);

        IEnumerable<LoanDetailsFeeViewModel> GetLoanDetailsFee(int applicationId);

        IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId);

        bool Confirmation(int type, int applicationId);

        IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int countryId, int branchId, int staffId, int? classId);

        IEnumerable<CurrentCommitteeViewModel> GetCurrentCommittee(int loanApplicationId);

        bool SecretariatForwardAppraisalMemorandum(ForwardCommitteeCamViewModel entity);

        IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId);

        List<PendingProductProgramViewModel> GetPendingProductProgram(UserInfo user);

        bool GetUntenoredStatus(int applicationId);

        PrivilegeViewModel GetUserPrivilege(AuthoritySignatureViewModel entity);

        IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggers(int applicationId);

        IEnumerable<MonitoringTriggersViewModel> SaveApplicationMonitoringTriggers(int applicationId, List<MonitoringTriggersViewModel> entity, int staffId);
    }
}