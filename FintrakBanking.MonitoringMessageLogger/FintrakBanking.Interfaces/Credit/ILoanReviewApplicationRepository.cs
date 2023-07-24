using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
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
        IEnumerable<LoanApplicationDetailViewModel> ExceptionalSearch(string searchString);
        IEnumerable<LoanReviewOperationViewModel> ContingentSearch(string searchString);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetAllLienRemovalApplications(int staffId, int companyId);
        
        IEnumerable<LoanReviewOperationApprovalViewModel> SearchLien(string searchString);
        IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? productClassId);
        List<applicationDetails> GetApplicationsById(UserInfo user, int lmsApplicationId);
        List<LoanReviewApplicationViewModel> CalculateSLA(List<LoanReviewApplicationViewModel> apps);
        IQueryable<LoanReviewApplicationViewModel> GetLoanReviewAvailmentAwaitingApproval(UserInfo user, int operationId, int? classId);
        IQueryable<LoanReviewApplicationViewModel> GetLoanReviewForCRMS(UserInfo user, int operationId, int? classId);
        IQueryable<LoanReviewApplicationViewModel> GetLoanReviewDrawdownApproval(UserInfo user, int? classId);
        SelectListViewModel GetAllSelectList();
        SelectListViewModel GetAllLMSApprovalOperationList();
        SelectListViewModel GetAllLMSApprovalOperationListByProductTypeId(int productTypeId);
        LoanChargeFeeViewModel GetChargeFeeDetails(int id);

        bool ValidateSubAllocationOperation(int loanApplicationDetailId, int customerId);

        string SubmitLoanReviewApplication(LoanReviewApplicationViewModel entity);

        List<LoanReviewOperationViewModel> GetMaturityInstruction(int loanId, short loansystemTypeId);

        //List<LoanViewModel> LoanSearch(int getCompanyId, SearchViewModel search);

        //int SaveCam(CamViewModel cam);

        //CamViewModel GetCamDocument(int documentationId);

        //CamViewModel GetCamDocumentByApprovalLevel(int applicationId, int staffId);

        //List<CamViewModel> GetCamDocuments(int applicationId);

        WorkflowResponse ForwardApplication(ForwardReviewViewModel model);
        WorkflowResponse ForwardApplicationAppraisal(ForwardReviewViewModel model);

        LoanApplicationDetailViewModel GetLoanApplicationDetail(int loanId, int loanTypeId);

        IQueryable<LoanReviewApplicationViewModel> GetRegionalLoanApplications(int getStaffId);

        IEnumerable<LoanApplicationViewModel> Search(string searchString);
        bool AppraisalReviewReferBack(ForwardViewModel entity);
        bool UpdateManagementPosition(ManagementPositionViewModel entity);
        ManagementPositionViewModel GetManagementPosition(int detailId);

        bool ValidateNewSubAllocationOperation(int loanApplicationDetailId, int customerId, int loanSystemTypeId);

        List<LoanReviewOperationViewModel> GetLMSOperation(int loanId, short loansystemTypeId);
        decimal? GetWrittenOffAccrualAmount(int loanId, short loanSystemTypeId);
        decimal GetMaximumApplicationOutstandingBalance(int applicationId);
        ContingentLoansViewModel GetContingentTotoalUsed(int contingetLoanId);
    }
}
