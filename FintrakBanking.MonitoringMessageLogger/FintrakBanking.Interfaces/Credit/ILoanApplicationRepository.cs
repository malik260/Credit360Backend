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
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.Finance;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanApplicationRepository
    {
        bool AddApprovalFromSubsidiary(HeadOfficeFacilityApprovalViewModel model);
        
        LoanApplicationViewModel GetFacilityByApplicationDetailIdLos(int loanApplicationDetailId);
        LoanApplicationViewModel GetFacilityByApplicationDetailIdLms(int loanApplicationDetailId);
        IEnumerable<RetailRecoveryCustomerTransactionsViewModels> GetRetailRecoveryReporting(DateTime startDate, DateTime endDate, int accreditedConsultantId, string customer);
        bool UpdateLoanApplicationTagsLMS(LoanApplicationTagsLMSViewModel model, int id, UserInfo user);
        LoanApplicationTagsLMSViewModel GetLoanApplicationTagsLMS(int id);
        IQueryable<LoanReviewApplicationViewModel> GetRejectedReviewLoanApplications(UserInfo user);
        IQueryable<LoanApplicationViewModel> GetRejectedLoanApplicationsArch(UserInfo user);
        List<LoanApplicationDetailViewModel> GetLoanApplicationDetailsById(int loanApplicationId, int companyId);
        List<LoanApplicationDetailViewModel> GetLmsLoanApplicationDetailsById(int loanApplicationId, int companyId);

        IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailById(int loanApplicationDetailId, int companyId);

        IEnumerable<LoanApplicationDetailViewModel> GetAllLoanApplicationsDetailsById(int loanApplicationId, int companyId);

        IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailByLoanApplicationId(int loanApplicationId, int companyId);

        bool CheckExistingCertificateOfOwnership(string certificateOfOwnership, int companyId);

        IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId);
        CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId);

        IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId);

        IEnumerable<ProductClassViewModel> GetProductClass();
        IEnumerable<CustomerViewModels> GetCustomerByApplicationId(int applicationId, string processtype);
        CustomerApplicationTransactionsViewModels GetCustomerTransactions(int customerId, int applicationId, bool islms);

        // LoanApplicationUpdateMessage UpdateApprovalStatusForApplication(int applicationId);
        LoanApplicationUpdateMessage UpdateApprovalStatusForApplication(int applicationId, int staffId);

        IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId);
        bool SendApplicationToEdit(int loanApplicationId, int operationId, int accountOfficerId);

        IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId);

        Task<bool> UpdateApprovalStatus(ApprovalViewModel entity);

        IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationsDetails(int loanApplicationId, int companyId);

        IQueryable<LoanApplicationDetailViewModel> GetLoanApplicationsAwaitingCheckList(int companyId);

        //IEnumerable<LoanApplicationViewModel> Search(string searchString);

        List<LoanApplicationViewModel> Search(string searchString, int staffId = 0);
        List<LoanApplicationViewModel> SearchDrawDown(string searchString, int staffId = 0);
        List<WorkflowTrackerViewModel> SearchBookedLoans(int loanApplicationDetailId);



        LoanApplicationViewModel AddLoanApplication( LoanApplicationViewModel loan, bool isExceptionalLoan = false);
        bool ValidateDuplicateLoanApplication( LoanApplicationViewModel loan);
        string GetRefrenceNumber();

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

        void LoadCustomerTurnover(int applicationId, int staffId);

        void GetCustomerRatiosFromBasel(int applicationId, int staffId);

        void GetCustomerGroupRatiosFromBasel(int applicationId, int staffId);

        void GetCorporateCustomerRatingFromBasel(int applicationId, int staffId);

        void GetFacilityRatingFromBasel(int applicationId, int staffId);

        void LoadCustomerTurnoverLms(int applicationId, int staffId);

        IEnumerable<ProductFeesViewModel> GetLoanApplicationFees(int loanDetailId);

        List<FacilityRatingViewModel> GetFacilityRating(int applicationDetailId);

        short SubmitLoanApplicationForCam(int applicationId, int staffId, int checkListIndex);
        int? GetFirstAdhocReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false);
        bool ArchiveLoanApplication(int loanAppliactionId, int operationId, short applicationStatus, int archivedBy);
        void ArchiveLoanApplicationDetails(int loanApplicationDetailId);
        int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, int? productId, int? exclusiveFlowChangeId, bool next = false);
        int? GetFirstLevelStaffId(int levelId, int userBranch);
        List<ProductFeeViewModel> GetLoanApplicationProductFees(int loanApplicationDeatilId);

        bool ProductFeesConcession(ProductFeesViewModel fees, UserInfo user);
        // decimal GetCustomerTotalOutstandingBalance(int customerId);

        dynamic GetLoanAppById(int loanApplicationDetailId, int companyId);

        IEnumerable<LoanApplicationViewModel> SearchForLoan(string searchString);

        bool DeleteLoanApplicationDetail(int loanApplicationDetailId);
        bool DeleteLoanApplication(int loanApplicationId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationDedubeCheck(int customerId, int companyId);

        IEnumerable<CreditApplicationViewModel> CommitteeCreditApplications(int applicationType, int staffId);

        bool ValidateInvoiceDetails(ValidateNumberViewModel data);

        List<LoanApplicationViewModel> GetLoanApplication(string searchQuery);

        WorkflowResponse RerouteWorkflowTarget(ForwardViewModel model);

        WorkflowResponse RouteWorkflowTarget(ForwardViewModel model);

        List<LoanApplicationViewModel> GetAllRequestsForLoanCancellation(int staffId);
        List<LoanReviewApplicationViewModel> GetAllLmsRequestsForLoanCancellation(int staffId);

        int SaveCancelledApplcation(LoanApplicationViewModel data);
        int SaveLMSCancelledApplcation(LoanReviewApplicationViewModel data);

        LoanApplicationViewModel ViewLaonApplicationCancellationDetails(LoanApplicationViewModel data);
        LoanReviewApplicationViewModel ViewLmsLaonApplicationCancellationDetails(LoanReviewApplicationViewModel values);
        bool GoForLoanApplicationCancellationApproval(LoanApplicationViewModel data);
        bool GoForLmsLoanApplicationCancellationApproval(LoanReviewApplicationViewModel data);

        List<TransactionDynamicsViewModel> GetTrnasactionDynamics(int loanApplicationId);

        List<ConditionPrecedentViewModel> GetConditionPrecidents(int loanApplicationId);

        LoanApplicationViewModel GetSingleLoanApplicationById(int loanApplicationId, int companyId);

        bool updateSuggestionsLoanApplicationdetail(LoanApplicationDetailViewModel model);

        LoanApplicationDetailViewModel GetSingleLoanApplicationsDetails(int loanApplicationDetailId, int companyId);

        IEnumerable<LookupViewModel> GetAllCRMSRepaymentSource();

        IEnumerable<LookupViewModel> GetAllCRMSFundingSource();

        IEnumerable<LookupViewModel> GetAllCRMSRepaymentAgreementType();

        List<ConditionPrecedentViewModel> GetLMSConditionPrecidents(int loanApplicationId);

        IEnumerable<LookupViewModel> GetAllSyndicationType();

        IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationDetailsByReference(string reference, int companyId);

        IEnumerable<LoanApplicationDetailViewModel> SearchApprovedLoanApplicationDetails(string reference, int companyId);

        CurrentCustomerExposure GetCurrentCompanyExposure();

        CurrentCustomerExposure GetCurrentCustomerExposure(int customerId);

        String ResponseMessage(WorkflowResponse response, string itemHeading);
        IEnumerable<LoanApplicationDetailViewModel> GetExceptionalLoansForApproval(int staffId);

        WorkflowResponse GoForApprovalExceptionalLoan(ExceptionalLoanViewModel model);

        LoanApplicationDetailViewModel GetLoanApplicationDetailFields(int detailId);

        LoanApplicationTagsViewModel GetLoanApplicationTags(int id);
        bool UpdateLoanApplicationTags(LoanApplicationTagsViewModel model, int id, UserInfo user);

        IEnumerable<RevisedProcessFlowModel> getFacilityApplicationRevisedProcessFlow();
        IEnumerable<RevisedProcessFlowModel> getFacilityApplicationRevisedProcessFlowByProductClassId(short productClassId, short productId, short productTypeId);
        RevisedProcessFlowModel getCashCollaterizedProcessFlowBy();

        IEnumerable<LoanApplicationViewModel> GetFacilityByApplicationId(int loanApplicationId);

        bool LoanApplicationFlowChange(int loanApplicationId);

        bool DeleteLoanApplicationThatFailedRAC(int loanApplicationDetailId, int deletedBy);

        LoanApplicationFlowChangeViewModel GetLoanAppicationFlowChange(int id);

        IEnumerable<LoanApplicationFlowChangeViewModel> GetLoanApplicationFlowChange();
 
        bool AddLoanApplicationFlowChange(LoanApplicationFlowChangeViewModel model);

        bool UpdateLoanApplicationFlowChange(LoanApplicationFlowChangeViewModel model, int id, UserInfo user);

        bool DeleteLoanApplicationFlowChange(int id, UserInfo user);
        List<RatingAndRatioViewModel> GetCustomerRatios(int customerId, int applicationId, bool isLms = false);
        CustomerApplicationTransactionsViewModels GetCustomerTransactionsByFilterLogic(int customerId, int applicationId, int froma, int to, int fYear, int tYear, bool v);
        List<InvoiceDetailViewModel> GetBulkLoanInvoice(byte[] file, UserInfo user);
        IEnumerable<LoanApplicationViewModel> LoanSearch(string searchString);
        List<accountsViewModels> GetCustomerTransactionsAccounts(int customerId, int applicationId, bool isLms = false);
        CustomerApplicationTransactionsViewModels GetCustomerTransactionsFiltered(int customerId, int applicationId, string accountnumber, int? fromYear, int fromMonth, int? toYear, int toMonth, bool isLms = false);

        IEnumerable<LoanApplicationLienViewModel> GetLienByApplicationDetailId(int applicationDetailId);
        IEnumerable<LoanApplicationLienViewModel> GetLienByCollateralId(int collateralId);
        LoanApplicationLienViewModel GetApplicationDetailLien(int id);
        IEnumerable<LoanApplicationLienViewModel> GetApplicationDetailLienByAccountNo(string accountNo);
        IEnumerable<LoanApplicationLienViewModel> GetApplicationDetailLienByIsReleased(bool isReleased);
        IEnumerable<LoanApplicationLienViewModel> GetApplicationDetailLien();
        bool AddLoanApplicationDetailLien(LoanApplicationLienViewModel model);
        bool UpdateLoanApplicationDetailLien(LoanApplicationLienViewModel model, int id, UserInfo user);
        bool DeleteLoanApplicationDetailLien(int id, UserInfo user);
        RacReturnInfoViewModel SaveRac(RacInformationViewModel rac, int? operationId, int productId, int? productClassId, int targetId, int staffId, int applicationId);
        CurrentCustomerExposure GetTotalBankExposure();
        bool ModifyFacility(FacilityModificationViewModel model, int loanApplicationDetailId);
        IEnumerable<LoanDetailReviewTypeViewModel> GetAllLoanDetailReviewTypes();
        IEnumerable<ApprovedTradeCycleViewModel> GetAllApprovedTradeCycles();
        List<CurrentCustomerExposure> GetExposures(TBL_LOAN_APPLICATION loanApplication);
        //IEnumerable<LoanApplicationLienViewModel> GetRacDetails();
    }
}