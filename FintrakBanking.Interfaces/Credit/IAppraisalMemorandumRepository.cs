using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IAppraisalMemorandumRepository
    {
        AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId);

        IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId);

        AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model);

        bool ForwardAppraisalMemorandum(ForwardViewModel model);

        bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId);

        IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId);

        IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId);

        IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId);

        PrivilegeViewModel GetUserPrivilege(int staffId, int applicationId, int operationId);

        bool Confirmation(int type, int applicationId);

        IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int countryId, int branchId, int staffId);

        IEnumerable<CurrentCommitteeViewModel> GetCurrentCommittee(int loanApplicationId);

        bool SecretariatForwardAppraisalMemorandum(ForwardCommitteeCamViewModel entity);

        IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId);

    }
}