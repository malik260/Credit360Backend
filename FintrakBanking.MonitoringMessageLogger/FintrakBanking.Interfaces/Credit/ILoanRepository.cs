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
    public interface ILoanRepository
    {
        CloseMannualBookingResponseViewModel GetLoanBookingDetailsFromFlexcube(CloseMannualBookingViewModel model);
        WorkflowResponse saveMultipleRetailLoanUnAssignmentToAgent(List<GlobalExposureApplicationViewModel> model, UserInfo user);
        WorkflowResponse saveRetailBulkLoanUnAssignmentToAgent(LoanRecoveryAssignmentViewModel model, UserInfo user);
        bool saveBulkLoanReAssignmentToAgentRem(LoanRecoveryAssignmentViewModel model, UserInfo user);
        bool RetailLoanRecoveryCommissionInternal(RetailLoanRecoveryCommissionViewModel models, UserInfo user);
        bool RetailLoanRecoveryReportCollection(RetailLoanRecoveryCommissionViewModel models, UserInfo user);
        WorkflowResponse saveMultipleLoanUnAssignmentToAgent(List<GlobalExposureApplicationViewModel> model, UserInfo user);
        WorkflowResponse saveBulkLoanUnAssignmentToAgent(LoanRecoveryAssignmentViewModel model, UserInfo user);
        bool saveBulkLoanAssignmentToAgentRem(List<GlobalExposureApplicationViewModel> models, int accreditedConsultant, DateTime? expCompletionDate, string source, string assignmentType, UserInfo user);
        bool RetailLoanRecoveryCommission(RetailLoanRecoveryCommissionViewModel models, UserInfo user);
        WorkflowResponse saveBulkLoanReAssignmentToAgent(GlobalExposureApplicationViewModel model, UserInfo user);
        WorkflowResponse saveMultipleLoanReAssignmentToAgent(List<GlobalExposureApplicationViewModel> model, UserInfo user, DateTime expCompletionDate, int accreditedConsultant, string source);
        WorkflowResponse saveMultipleRetailLoanReAssignmentToAgent(List<GlobalExposureApplicationViewModel> model, UserInfo user, DateTime expCompletionDate, int accreditedConsultant, string source);
        IEnumerable<FacilityModificationViewModel> GetLMSFacilityModificationsForApproval(int staffId);
        WorkflowResponse ApproveLMSFacilityModification(ForwardViewModel model);
        FacilityModificationViewModel GetLMSFacilityModification(int facilityModificationId);
        WorkflowResponse AddFacilityModification(FacilityModificationViewModel model);
        bool saveBulkLoanRecoveryCommission(List<LoanRecoveryCommissionBatchViewModel> models, UserInfo user);
        WorkflowResponse bulkLoanRecoveryCommissionGoForApproval(LoanRecoveryCommissionApprovalViewModel models, UserInfo user);
        WorkflowResponse bulkLoanRecoveryReportingGoForApproval(LoanRecoveryReportApprovalViewModel models, UserInfo user);
        bool saveBulkLoanRecoveryReporting(List<LoanRecoveryReportBatchViewModel> models, UserInfo user);
        IEnumerable<LoanViewModel> GetProcessLoanReviewData(int companyId, int staffId, string searchString);
        IEnumerable<CamProcessedLoanViewModel> GetBookedLoanApplicationForBookingVerificationParam(int staffId, int companyId, string searchString);
        IEnumerable<CamProcessedLoanViewModel> getLoanFacilitiesAwaitingApprovalByParam(int companyId, int staffId, string searchString);
        int AddCollateralLiquidationRecoveryWithoutFile(CollateralLiquidationRecoveryViewModel model);
        WorkflowResponse bulkLoanAssignmentToAgentGoForApproval(GlobalExposureApplicationViewModel models, UserInfo user);
        RemoveLienViewModel GetLienRemovalLetter(int lienRemovalId);
        CollateralLiquidationRecoveryViewModel GetLiquidationReceipt(int liquidationRecoveryReceiptId);
        int AddCollateralLiquidationRecovery(CollateralLiquidationRecoveryViewModel model, byte[] buffer);
        bool saveBulkLoanAssignmentToAgent(List<GlobalExposureApplicationViewModel> models, int accreditedConsultant, DateTime? expCompletionDate, string source, string assignmentType, UserInfo user);
        void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMehtod);
        IQueryable<LoanViewModel> LoanPrepaymentApprovalList();
        CasaBalanceViewModel GetCASABalanceById(int casaAccountId, int companyId);
        IQueryable<LoanViewModel> SearchForLoanPrepaymentReversal(string searchQuery);
        List<OverrideItemVeiwModel> getBookingOverride(string customerCode);
        LoanViewModel GetReferedBookingFacilityRecordsById(CamProcessedLoanViewModel model);
        WorkflowResponse ReferBackBooking(ApprovalViewModel model);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationDetailCovenantById(int applicationDetailId);
        IEnumerable<TransactionDynamicsViewModel> GetLoanTransactionDynamics(int loanApplicationDetailId);
        decimal getDailyInterest(decimal principal, double interestRate, int daysInAYear);

        List<ProductFeeViewModel> GetLoanProductFees(int loanBookingRequestId);
        

        CurrencyExchangeRateViewModel GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode);

        decimal getTotalInterest(decimal principal, double interestRate, int interestDaysPeriod, DayCountConventionEnum dayCountConventionId);

        //int getDaysInLoanPeriod(DateTime startDate, DateTime endDate);

        IEnumerable<LookupViewModel> GetLoanApplicationTypes();

        IQueryable<LoanViewModel> SearchForLoan(string searchQuery);
        IQueryable<LoanViewModel> SearchForLoanPrepayment(string searchQuery);
        IQueryable<LoanViewModel> SearchForLoanContingent(string searchQuery);
        IQueryable<LoanViewModel> SearchForLoanInactiveContingent(string searchQuery);

        IQueryable<LoanViewModel> SearchForFXRevolvingLoan(string searchQuery);

        IEnumerable<LoanViewModel> GetApprovedLoanReview(int companyId, int staffId);

        IEnumerable<LoanViewModel> GetApprovedLoanReviewRemedial(int userId, int companyId);
        LoanViewModel GetUnDisbursedLoanByLoanId(int loanId, int loanType);

        LoanViewModel GetDisbursedLoanByLoanId(int loanId, int loanType);

        IQueryable<LoanViewModel> SearchForFullAndFinalLoan(string searchQuery);

        bool CancelFullAndFinal(int loanId);
        bool AddExistingLoan(LoanViewModel entity);
        
        List<LoanViewModel> getDisbursedCommercialLoanTrancheDetailsById(int loanId);

        LoanViewModel GetDisbursedLoanByLoanId(int loanId);

        LoanViewModel GetGroupLoanByLoanId(int loanId);

        IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetLoanApplicationDetails(int loanApplicationDetailId, int companyId);

        List<ApprovalLevelStaffViewModel> GetLoanOperationApprovers(int operation, int companyId);

        string AddLoanBooking(LoanViewModel entity);

        bool UpdateFacilityLineStatus(LoanViewModel entity);

        //  bool AddLoanGuarantor(LoanGuarantorViewModel guarantorModel, short productTypeId, int loanApplicationId);

        IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId);

        LoanViewModel GetLoan(int loanId);
        List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTrigger();

        IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId);

        IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel);

        IEnumerable<CamProcessedLoanViewModel> ApprovedLoansForIFF(int companyId, int staffId, int branchId);
        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId);
        IEnumerable<CamProcessedLoanViewModel> getApplicationsToBeAdhocApprovedForInitiateBooking(int companyId, int staffId, int branchId);
        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsReadyForCrmsCode(int companyId, int staffId);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsReadyForBooking(int companyId, int staffId);
        IEnumerable<CamProcessedLoanViewModel> GetAvailedContingentFacilityBooking(int companyId, int staffId);

        IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationDetailById(int staffId, int companyId, int applicationDetailId, int loanBookingRequestId);


        IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId);

        IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId);

        IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId);

        IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId);
        IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId);

        IEnumerable<LoanViewModel> GetLoanFacilityBookingAwaitingApproval(int staffId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetBookedLoanApplicationForBookingVerification(int staffId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetFacilityLineAwaitingMaintenanceApproval(int staffId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetdisbursedLoansApplicationDetails(int staffId, int companyId);

        IEnumerable<RevolvingLoanViewModel> GetRevolvingFacilityBookingAwaitingApproval(int staffId, int companyId);

        IEnumerable<ContingentLoanViewModel> GetContingentFacilityBookingAwaitingApproval(int staffId, int companyId);
        //IEnumerable<LoanChargeFeeViewModel> GetDeferredTermLoanFeeAwaitingApproval(int staffId, int companyId);
        //IEnumerable<LoanChargeFeeViewModel> GetDeferredRevolvingLoanFeeAwaitingApproval(int staffId, int companyId);
        //IEnumerable<LoanChargeFeeViewModel> GetDeferredContingentLoanFeeAwaitingApproval(int staffId, int companyId);

        int GoForApproval(ApprovalViewModel entity, int loanBookingRequestId, bool isManual = false);


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
        List<LoanChargeFeeViewModel> GetLoanChargeFee(int loanId, int loanType);
        // decimal GetCustomerLoanAvailableBalance(int loanAplicationDetailId);

        List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId, int loanType);
        List<CollateralLoanApplication> GetLoanCollateral(int loanId, int loanType);
        List<LoanDisbursementViewModel> GetForeignLoanBeneficiaryNaration(int loanId);
        List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTriggers(int loanId, int loanSystemTypeId);
        bool VerifyLegalContingentCode(string legalContingentCode, int loanApplicationDetailId);

        List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int loanTypeId, int companyId);
        List<LoanCAMSOLViewModel> GetCurrentCamsolByCustomer(List<CustomerExposure> customer, int loanTypeId, int companyId);

        IEnumerable<LoanPaymentSchedulePeriodicViewModel> GetLoanScheduleByLoanId(int loanId);
        IEnumerable<LoanViewModel> GetBookedLoanDetailsWithParameters(int companyId, string param);

        Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId);

        //void AddLoanTestFees(List<LoanChargeFeeViewModel> feeModel, int staffId, int loanId, short productTypeId, int companyId, bool feeOverride);

        //IEnumerable<LoanViewModel> SearchForLoanAndRevolvingLoan(string searchQuery);
        IEnumerable<LoanViewModel> SearchForLoanAndRevolvingLoan(string searchQuery, int statusId = 0);
        //IEnumerable<LoanViewModel> SearchForLoanAndRevolvingLoanReviewFeeCharge(int loanSystemTypeId, string searchQuery);
        IEnumerable<LoanViewModel> SearchForLoanAndRevolvingLoanFeeCharge(int loanSystemTypeId, string searchQuery);

        string GenerateLoanReferenceNumber(int customerId, int productId, int productTypeId);

        IEnumerable<LoanViewModel> GetLoanReviewApplicationOverDraft(int staffId, int companyId);

        LoanViewModel GetOverdraftDetailsByLoanId(int revolvingLoanId);

        IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId, ReportSearchParamViewModel param);

        IQueryable<LoanViewModel> SearchRunningCommercialAndFXLoans(string searchQuery);

        IEnumerable<RevolvingLoanViewModel> GetRevolvingLoanTypes();

        IEnumerable<RevolvingLoanViewModel> GetTemporaryOverdrafts();

        IEnumerable<LoanViewModel> GetLoanStatus(int companyId);

        IEnumerable<CustomerCompanyInfomationViewModels> getLoanCustomerCompanyInformation(int customerId);

        #region Loan Disbursement 
        IEnumerable<LoanDisbursementViewModel> GetAllLoanDisbursement(int loanId);
        IEnumerable<CamProcessedLoanViewModel> GetEmployerRelatedData(int staffId, int companyId, DateRange dateRange);
        // bool AddUpdateLoanDisbursement(LoanDisbursementViewModel entity);
        #endregion

        IEnumerable<LookupViewModel> GetAllFrequencyType();

        List<ProductViewModel> GetLoanCommercialLoans(int companyId);

        LoanPaymentScheduleInputViewModel BuildScheduleModel(int targetId, int createdBy);

        LoanViewModel BuildDisbursementModel(int loanId, LoanPaymentScheduleInputViewModel loanInputModel, int staffId);

        void DisburseLoan(LoanViewModel entity, TwoFactorAutheticationViewModel twoFactorAuthDetails = null);

        IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId, bool isInitiation);

        int GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId);
        //WorkflowResponse GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId);

        IEnumerable<LoanViewModel> GetApprovedNonTermLoansForReview(int staffId, int companyId);
        IEnumerable<LoanViewModel> GetApprovedNonTermLoansForReviewAwaitingApproval(int staffId, int companyId);

        // IEnumerable<LoanViewModel> GetApprovedFXRevolvingLoanReview();
        // IEnumerable<LookupViewModel> GetAllCRMSRepaymentAgreementType();

        List<LoanViewModel> GetLoanApplicationExistingLoans(int applicationId);

        List<CurrentCustomerExposure> GetApplicationFacilitySummary(int applicationId);


        IEnumerable<CamProcessedLoanViewModel> GetApprovedLineReview(int staffId, int companyId);

        IEnumerable<DailyInterestAccrualViewModel> ProcessBackDatedTeamLoansInterestAccrual(DateTime effectiveDate, int loanId);
        IEnumerable<LoanViewModel> GetContingentApprovedExpiredApplication(int staffId, int companyId);

        IEnumerable<LoanViewModel> GetContingentApprovedApplication(int staffId, int companyId);

        LoanViewModel GetContingentByLoanId(int revolvingLoanId);
        // IEnumerable<LookupViewModel> GetAllCRMSRepaymentAgreementType();
        IEnumerable<LoanViewModel> GetCommercialLoanByApplicationDetailId(int loanApplicationDetailId);
        IEnumerable<LoanViewModel> GetLoanByApplicationDetailId(int loanApplicationDetailId);
        IEnumerable<LoanViewModel> GetLoanHistoryByLoanAccountNumber(string loanReferenceNumber);
        IEnumerable<LoanBookingRequestViewModel> GetLoanRequestsByApplicationDetailId(int loanApplicationDetailId);
        IEnumerable<CamProcessedLoanViewModel> GetCustomerLines(int customerId);

        decimal getLoanInterestRateAmount(decimal principal, double interestRate, DateTime startDate, DateTime endDate, DayCountConventionEnum dayCountConventionId);
        List<LookupViewModel> GetLoanRepricingModes();

        List<LoanViewModel> GetCompletedLoans();
        List<LoanViewModel> GetCompletedLoan(string searchValue);
        bool GetChangeLoanStatusOfACompletedLoan(int loanId);
        IEnumerable<LookupViewModel> GetAllLoanStatus();

        IQueryable<LoanViewModel> SearchAllOverdraft(string searchQuery);

        AccountBalanceViewModel GetLoanBalances(int loanId, int companyId);

        Tuple<List<multipleDisbursementOutputViewModel>, bool> preBulkLoanDisbursement(byte[] file, UserInfo user, bool isFinal);
        Tuple<List<MultipleInsuranceOutputViewModel>, bool> preBulkInsurance(byte[] file, UserInfo user, bool isFinal);
        IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId);
       // IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId);

        List<multipleDisbursementOutputViewModel> startBulkLoanDisbursement(List<multipleDisbursementOutputViewModel> models, UserInfo user);
        bool saveBulkLoanDisbursementEntries(List<multipleDisbursementOutputViewModel> models, UserInfo user);
        IEnumerable<multipleDisbursementOutputViewModel> GetpendingMultipleDisbursement();
        List<LoanViewModel> GetApprovedLoanReviewAwaitingRoute(int staffId, int companyId);
        List<LoanViewModel> GetLoanReviewApplicationOverDraftRouteAndOperations(int staffId, int companyId);
        IEnumerable<LookupViewModel> GetFullAndFinalStatus();
        bool CancelFullAndFinal(int loanId, int statusId);
        List<LoanViewModel> GetApprovedLoanReviewAwaitingOperation(int staffId, int companyId);
        WorkflowResponse saveBulkInsurancePolicyEntries(List<MultipleInsuranceOutputViewModel> models, UserInfo user);
      //  IEnumerable<CamProcessedLoanViewModel> GetApprovedLineReviewSearch(int staffId, int companyId, string searchString);
       // IEnumerable<LoanViewModel> GetLoanReviewApplicationOverDraftSearch(int staffId, int companyId, string searchString);
       // IEnumerable<LoanViewModel> GetApprovedLoanReviewSearch(int companyId, int staffId, string searchString);
       // IEnumerable<LoanViewModel> GetContingentApprovedApplicationSearch(int staffId, int companyId, string searchString);
    }
}