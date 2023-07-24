using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FintrakBanking.Interfaces.Credit
{
    public interface ICreditDrawdownRepository
    {
        IEnumerable<TransactionDynamicsViewModel> GetLoanTransactionDynamics(int loanApplicationDetailId);
        bool LogApproval(ForwardViewModel model, int operationId, bool externalInitialization, int ApprovalStatusId);
        WorkflowResponse LogApprovalForMessage(ForwardViewModel model, bool externalInitialization, bool saveChanges = false);
        int GetNextLevelForBookingRequest(int applicationStatusId, List<LoanBookingRequestViewModel> entity);
        CurrentCustomerExposure GetCurrentCompanyExposure();
        WorkflowResponse GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId);
        IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId, bool isInitiation = false);
        int GetDrawdownOperationId(int applicationDetailId);
        IEnumerable<CamProcessedLoanViewModel> GetGlobalEmployerLoansDueForInitiateBooking(int companyId, string searchString);
        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId, int customerId = 0, bool getAll = false);
        IEnumerable<CamProcessedLoanViewModel> getApplicationsToBeAdhocApprovedForInitiateBooking(int companyId, int staffId, int branchId);
        WorkflowResponse AddLoanBookingRequest(int applicationStatusId, List<LoanBookingRequestViewModel> models);
        bool setLineFacilityLegalDocumentStatus(RecommendedCollateralViewModel entity, int loanBookingRequestId, bool value);

      //  Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId);
    }
}
