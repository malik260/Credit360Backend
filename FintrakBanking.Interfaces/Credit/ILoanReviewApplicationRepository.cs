using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanReviewApplicationRepository
    {
        IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? productClassId);

        SelectListViewModel GetAllSelectList();
        bool validateCustomer(int loanApplicationDetailId, int customerId);

        string SubmitLoanReviewApplication(LoanReviewApplicationViewModel entity);

        //List<LoanViewModel> LoanSearch(int getCompanyId, SearchViewModel search);

        //int SaveCam(CamViewModel cam);

        //CamViewModel GetCamDocument(int documentationId);

        //CamViewModel GetCamDocumentByApprovalLevel(int applicationId, int staffId);

        //List<CamViewModel> GetCamDocuments(int applicationId);

        WorkflowResponse ForwardApplication(ForwardReviewViewModel model);

        LoanApplicationDetailViewModel GetLoanApplicationDetail(int loanId, int loanTypeId);

        IQueryable<LoanReviewApplicationViewModel> GetRegionalLoanApplications(int getStaffId);

        IEnumerable<LoanApplicationViewModel> Search(string searchString);
        bool AppraisalReviewReferBack(ForwardViewModel entity);
        bool UpdateManagementPosition(ManagementPositionViewModel entity);
        ManagementPositionViewModel GetManagementPosition(int detailId);
    }
}
