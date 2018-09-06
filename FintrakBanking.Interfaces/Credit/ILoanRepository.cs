using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.Approval;
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
    public interface ILoanRepository
    {
        IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationDetailCovenantById(int applicationDetailId);
        IEnumerable<TransactionDynamicsViewModel> GetLoanTransactionDynamics(int loanApplicationDetailId);
        decimal getDailyInterest(decimal principal, double interestRate, int interestDaysPeriod);

        CurrencyExchangeRateViewModel GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode);

        decimal getTotalInterest(decimal principal, double interestRate, int interestDaysPeriod);

        int getDaysInLoanPeriod(DateTime startDate, DateTime endDate);

        IEnumerable<LookupViewModel> GetLoanApplicationTypes();

        IQueryable<LoanViewModel> SearchForLoan(string searchQuery);

        IQueryable<LoanViewModel> SearchForFXRevolvingLoan(string searchQuery);

        IEnumerable<LoanViewModel> GetApprovedLoanReview();

        IEnumerable<LoanViewModel> GetApprovedLoanReviewRemedial();

        LoanViewModel GetDisbursedLoanByLoanId(int loanId);

        LoanViewModel GetGroupLoanByLoanId(int loanId);

        IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetLoanApplicationDetails(int loanApplicationDetailId, int companyId);

        List<ApprovalLevelStaffViewModel> GetLoanOperationApprovers(int operation, int companyId);

        string AddLoanBooking(LoanViewModel entity);

        //  bool AddLoanGuarantor(LoanGuarantorViewModel guarantorModel, short productTypeId, int loanApplicationId);

        IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId);

        LoanViewModel GetLoan(int loanId);
        List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTrigger();

        IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId);

        IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsReadyForBooking(int companyId, int staffId);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationDetailById(int staffId, int companyId, int applicationDetailId,int loanBookingRequestId);

        bool AddLoanBookingRequest(int applicationStatusId, LoanBookingRequestViewModel entity);

        IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId);

        IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId);

        IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId);
        IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId);

        IEnumerable<LoanViewModel> GetLoanBookingAwaitingApproval(int staffId, int companyId);

        IEnumerable<RevolvingLoanViewModel> GetRevolvingFacilityBookingAwaitingApproval(int staffId, int companyId);

        IEnumerable<ContingentLoanViewModel> GetContingentFacilityBookingAwaitingApproval(int staffId, int companyId);
        //IEnumerable<LoanChargeFeeViewModel> GetDeferredTermLoanFeeAwaitingApproval(int staffId, int companyId);
        //IEnumerable<LoanChargeFeeViewModel> GetDeferredRevolvingLoanFeeAwaitingApproval(int staffId, int companyId);
        //IEnumerable<LoanChargeFeeViewModel> GetDeferredContingentLoanFeeAwaitingApproval(int staffId, int companyId);

        int GoForApproval(ApprovalViewModel entity, int loanBookingRequestId);

        bool GoForFeeOverrideApproval(ApprovalViewModel entity);

        void PostLoanFees(LoanViewModel entity);

        AppraisalMemorandumLoanDetailViewModel GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId);

        IQueryable<CustomerSearchItemViewModels> SearchCustomerCollateral(int companyId, string searchQuery);

        IQueryable<CustomerViewModels> SearchForCustomerCollateral(int companyId, string searchQuery);

        List<loanApplicationColateralViewModel> GetLoanApplicationCollateralsByApplicationId(int loanApplicationId);

        List<CasaViewModel> GetLoanCustomerAccounts(int customerId, int loanApplicationDetailId);

        //  List<loanApplicationColateralViewModel> GetLoanApplicationCollateralsByApplicationId(int loanApplicationId);

        List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTriggerByLoanApplicationDetailId(int loanApplicationDetailId);

        List<LoanChargeFeeViewModel> GetLoanChargeFee(int loanId);
        // decimal GetCustomerLoanAvailableBalance(int loanAplicationDetailId);

        List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId);

        List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int companyId);

        IEnumerable<LoanPaymentSchedulePeriodicViewModel> GetLoanScheduleByLoanId(int loanId);
        IEnumerable<LoanViewModel> GetBookedLoanDetailsWithParameters(int companyId, string param);

        Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId);

        //void AddLoanTestFees(List<LoanChargeFeeViewModel> feeModel, int staffId, int loanId, short productTypeId, int companyId, bool feeOverride);

        IEnumerable<LoanViewModel> SearchForLoanAndRevolvingLoan(int performanceTypeId, int productTypeId, string searchQuery);

        string GenerateLoanReferenceNumber(int customerId, int productId, int productTypeId);

        IEnumerable<LoanViewModel> GetLoanReviewApplicationOverDraft();

        LoanViewModel GetOverdraftDetailsByLoanId(int revolvingLoanId);

        IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId, ReportSearchParamViewModel param);

        IQueryable<LoanViewModel> SearchRunningCommercialAndFXLoans(string searchQuery);

        IEnumerable<RevolvingLoanViewModel> GetRevolvingLoanTypes();

        IEnumerable<RevolvingLoanViewModel> GetTemporaryOverdrafts();

        IEnumerable<LoanViewModel> GetLoanStatus(int companyId);

        IEnumerable<CustomerCompanyInfomationViewModels> getLoanCustomerCompanyInformation(int customerId);

        #region Loan Disbursement 
        IEnumerable<LoanDisbursementViewModel> GetAllLoanDisbursement(int loanId);
       // bool AddUpdateLoanDisbursement(LoanDisbursementViewModel entity);
        #endregion

        IEnumerable<LookupViewModel> GetAllFrequencyType();

        List<ProductViewModel> GetLoanCommercialLoans(int companyId);

        LoanPaymentScheduleInputViewModel BuildScheduleModel(int targetId, int createdBy);

        LoanViewModel BuildDisbursementModel(int loanId, LoanPaymentScheduleInputViewModel loanInputModel, int staffId);

        void DisburseLoan(LoanViewModel entity, TwoFactorAutheticationViewModel twoFactorAuthDetails = null);

        IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId);

        int GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId);

        IEnumerable<LoanViewModel> GetApprovedCommercialLoanReview();

        IEnumerable<LoanViewModel> GetApprovedFXRevolvingLoanReview();
       // IEnumerable<LookupViewModel> GetAllCRMSRepaymentAgreementType();

        List<LoanViewModel> GetLoanApplicationExistingLoans(int applicationId);

        List<CurrentCustomerExposure> GetApplicationFacilitySummary(int applicationId);


        IEnumerable<CamProcessedLoanViewModel> GetApprovedLineReview();
    }
}