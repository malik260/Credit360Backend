using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanApplicationRepository
    {
        IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailById(int loanApplicationDetailId, int companyId);

        IEnumerable<LoanApplicationDetailViewModel> GetAllLoanApplicationsDetails(int loanApplicationId, int companyId);

        IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailByLoanApplicationId(int loanApplicationId, int companyId);

        bool CheckExistingCertificateOfOwnership(string certificateOfOwnership, int companyId);

        IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId);

        IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId);

        IEnumerable<ProductClassViewModel> GetProductClass();

        // LoanApplicationUpdateMessage UpdateApprovalStatusForApplication(int applicationId);
        LoanApplicationUpdateMessage UpdateApprovalStatusForApplication(int applicationId, int staffId);

        IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId);

        IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId);

        Task<bool> UpdateApprovalStatus(ApprovalViewModel entity);

        IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationsDetails(int loanApplicationId, int companyId);

        IQueryable<LoanApplicationDetailViewModel> GetLoanApplicationsAwaitingCheckList(int companyId);

        IEnumerable<LoanApplicationViewModel> Search(string searchString);

        LoanApplicationViewModel AddLoanApplication( LoanApplicationViewModel loan);

        bool AddLoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity);

        IEnumerable<LoanApplicationCollateralViewModel> GetLoanApplicationCollateral(int loanApplicatioinCollateralId);

        dynamic GetLoanApplicationDetailsProductProgram(int loanApplicationDetailId);

        ValidateDataViewModel ValidateDocumentDate(ValidateDataViewModel data);

        ValidateNumberViewModel ValidateDocumentNumber(ValidateNumberViewModel data);

        IQueryable<LoanApplicationViewModel> GetLoanApplicationsByOperation(int operationId, int? classId, int branchId, int staffId);

        IQueryable<LoanApplicationViewModel> GetRejectedLoanApplications(UserInfo user);

        string ReviewRequest(ForwardViewModel model);

        dynamic GetCollateralRequirements(int applicationID, int? collateralCurrencyId, int companyId);

        bool UpdateLoanApplicationDetails(LoanApplicationDatailViewModel entity, UserInfo user);

        IEnumerable<ProductFeesViewModel> GetLoanApplicationFees(int loanDetailId);

        bool SubmitLoanApplicationForCam(int applicationId, int staffId, int checkListIndex);

        List<ProductFeeViewModel> GetLoanApplicationProductFees(int loanApplicationDeatilId);

        bool ProductFeesConcession(ProductFeesViewModel fees, UserInfo user);
        // decimal GetCustomerTotalOutstandingBalance(int customerId);

        dynamic GetLoanAppById(int loanApplicationDetailId, int companyId);

        IEnumerable<LoanApplicationViewModel> SearchForLoan(string searchString);

        bool DeleteLoanApplicationDetail(int loanApplicationDetailId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationDedubeCheck(int customerId, int companyId);

        IEnumerable<CreditApplicationViewModel> CommitteeCreditApplications(int applicationType, int staffId);

        bool ValidateInvoiceDetails(ValidateNumberViewModel data);

        WorkflowResponse RerouteWorkflowTarget(ForwardViewModel data);
        
    }
}