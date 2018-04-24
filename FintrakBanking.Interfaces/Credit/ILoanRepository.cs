using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanRepository
    {
        IEnumerable<LookupViewModel> GetLoanApplicationTypes();

        IQueryable<LoanViewModel> SearchForLoan(string searchQuery);

        IEnumerable<LoanViewModel> GetApprovedLoanReview();

        LoanViewModel GetDisbursedLoanByLoanId(int loanId);

        IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetLoanApplicationDetails(int loanApplicationDetailId, int companyId);

        List<ApprovalLevelStaffViewModel> GetLoanOperationApprovers(int operation, int companyId);

        string AddLoanBooking(LoanViewModel entity);

        bool AddLoanGuarantor(LoanGuarantorViewModel guarantorModel, short productTypeId, int loanApplicationId);

        IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId);

        LoanViewModel GetLoan(int loanId);
        List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTrigger();

        IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId);

        IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsReadyForBooking(int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationDetailById(int companyId, int applicationDetailId);

        bool AddLoanBookingRequest(int applicationStatusId, LoanBookingRequestViewModel entity);

        IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId);

        IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId);

        IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId);

        IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId);

        IEnumerable<LoanViewModel> GetTermLoanBookingAwaitingApproval(int staffId, int companyId);

        IEnumerable<RevolvingLoanViewModel> GetRevolvingLoanBookingAwaitingApproval(int staffId, int companyId);

        IEnumerable<ContingentLoanViewModel> GetContingentLoanBookingAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanChargeFeeViewModel> GetDeferredTermLoanFeeAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanChargeFeeViewModel> GetDeferredRevolvingLoanFeeAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanChargeFeeViewModel> GetDeferredContingentLoanFeeAwaitingApproval(int staffId, int companyId);

        int GoForApproval(ApprovalViewModel entity, int loanBookingRequestId);
        bool GoForFeeOverrideApproval(ApprovalViewModel entity);

        void PostLoanFees(LoanViewModel entity);

        AppraisalMemorandumLoanDetailViewModel GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId);

        IQueryable<CustomerSearchItemViewModels> SearchCustomerCollateral(int companyId, string searchQuery);

        IQueryable<CustomerViewModels> SearchForCustomerCollateral(int companyId, string searchQuery);

        List<LoanGuarantorViewModel> GetLoanGuarantors(int loanApplicationId);

        List<CasaViewModel> GetLoanCustomerAccounts(int customerId, int loanApplicationDetailId);

        List<loanApplicationColateralViewModel> GetLoanApplicationCollateralsByApplicationId(int loanApplicationId);

        List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTriggerByLoanApplicationDetailId(int loanApplicationDetailId);

        List<LoanChargeFeeViewModel> GetLoanChargeFee(int loanId);
       // decimal GetCustomerLoanAvailableBalance(int loanAplicationDetailId);

        List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId);

        List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int companyId);

        IEnumerable<LoanPaymentSchedulePeriodicViewModel> GetLoanScheduleByLoanId(int loanId);
         IEnumerable<LoanViewModel> GetBookedLoanDetailsWithParameters(int companyId, string param);

        Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId);

        void AddLoanTestFees(List<LoanChargeFeeViewModel> feeModel, int staffId, int loanId, short productTypeId, int companyId, bool feeOverride);

        IQueryable<LoanViewModel> SearchForOverdraft(string searchQuery);

        string GenerateLoanReferenceNumber(int customerId, int productId, int productTypeId);


    }
}