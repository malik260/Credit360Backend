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
        CurrentCustomerExposure GetCurrentCompanyExposure();
        int GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId);
        IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId, bool isInitiation = false);
        int GetDrawdownOperationId(int applicationDetailId);
        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId);
        IEnumerable<CamProcessedLoanViewModel> getApplicationsToBeAdhocApprovedForInitiateBooking(int companyId, int staffId, int branchId);
        bool AddLoanBookingRequest(int applicationStatusId, List<LoanBookingRequestViewModel> models);
      //  Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId);
    }
}
