using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IAppraisalMemorandumRepository
    {
        AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId);

        IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId);

        AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model);

        Task<bool> ForwardAppraisalMemorandum(ForwardViewModel model);

        //IEnumerable<AppraisalMemorandumViewModel> GetAllAppraisalMemorandum();

        bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId);

        IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId);

        ApprovedLoanDetailViewModel GetApprovedLoanDetail(int applicationId);

        PrivilegeViewModel GetUserPrivilege(int staffId,int applicationId);
    }
}