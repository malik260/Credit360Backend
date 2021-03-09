using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Media;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.Interfaces.Media
{
    public interface IOriginalDocumentApprovalRepository
    {
        IEnumerable<OriginalDocumentApprovalViewModel> GetOriginalDocumentSearch(string searchString);
        OriginalDocumentApprovalViewModel GetOriginalDocumentApproval(int id);

        IEnumerable<OriginalDocumentApprovalViewModel> GetOriginalDocumentApprovals(int staffId);

        int AddOriginalDocumentApproval(OriginalDocumentApprovalViewModel model);

        bool UpdateOriginalDocumentApproval(OriginalDocumentApprovalViewModel model, int id, UserInfo user);

        bool DeleteOriginalDocumentApproval(int id, UserInfo user);

        IEnumerable<LoanApplicationViewModel> Search(string parameter);

        List<OriginalDocumentApprovalViewModel> GetOriginalDocumentByCollateralCustomerId(int id);
        List<OriginalDocumentApprovalViewModel> GetApprovedOriginalDocumentByCollateralCustomerId(int id);

        WorkflowResponse GoForApproval(OriginalDocumentApprovalViewModel model, short? approvalStatusId);

        //bool SubmitApproval(OriginalDocumentApprovalViewModel entity);
        WorkflowResponse SubmitApproval(OriginalDocumentApprovalViewModel entity);

        IEnumerable<OriginalDocumentApprovalViewModel> SearchForApprovedOriginalDocument(string searchString);

        List<OriginalDocumentApprovalViewModel> GetOriginalDocument(int id);

        List<OriginalDocumentApprovalViewModel> GetReleaseDocumentByCollateralCustomerId(int id);
        List<OriginalDocumentApprovalViewModel> GetDocumentUploadList(int staffId);
        List<OriginalDocumentApprovalViewModel> GetReleaseDocumentByCustomerFacilityId(int id);
    }
}
