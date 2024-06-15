using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICustomerCollateralRepository
    {
        IEnumerable<CollateralCashReleaseViewModel> GetCustomerCashCollateralApplications(int id);
        IEnumerable<CollateralViewModel> GetCustomerCashCollateral(int customerId, int? applicationId, int companyId, bool isLMS = false);
        bool DeleteAddedValuer(int valuerId, int createdById);
        CollateralHistory getCollateralHistoryUsage(int collateralId);
        IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByCustomerIdLMS(int customerId, bool getAll = false);
        bool ProposeCollateralForUsageLMS(CollateralCoverageViewModel model);
        #region Collateral

        //int AddCollateral(CollateralViewModel entity, byte[] file);
        IEnumerable<CollateralCoverageViewModel> CalculateCoverateOfCollateralLMS(CollateralCoverageViewModel model);
        bool DeleteDuplicatedCollateral(CollateralViewModel model);
        int AddCollateral(CollateralViewModel entity);
        int AddReleaseDocument(CollateralViewModel model, byte[] file);
        IEnumerable<CollateralViewModel> GetCollateralReleaseDocument(int releaseId);
        CollateralViewModel GetReleaseSupportingDocument(int documentId);

        bool ReleaseCollateral(CollateralViewModel entity);
        bool ReleaseCollateralJobRequest(CollateralViewModel entity);
        ApprovalResponse ReleaseCollateralGoForApproval(ApprovalViewModel entity);

        IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingApproval(int companyId, int staffId);

        IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingJobRequest(int companyId, int branchId);

        bool UpdateCollateral(CollateralViewModel entity, int collateralId);
        IEnumerable<OriginalDocumentSubmissionByFacilityViewModel> GetCustomerFacility(int customerId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int? applicationId, int companyId, bool isLMS = false);
        IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateral( int? applicationDetailId, int currencyId, int companyId);
        IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByCustomerId(int customerId, bool getAll);
        IEnumerable<CollateralCoverageViewModel> GetProposedFacilitiesToCollateralByCollateralId(int collateralId);
        IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByLoanApplicationDetailId(int loanApplicationDetailId);
        CollateralViewModel GetCustomerCollateralInformation(int collateralCustomerId, int companyId);
        List<CollateralViewModel> GetCustomerPropertyCollaterals(int? customerId, int companyId);

        CollateralViewModel GetCustomerCollateralByCustomerCollateralId(int customerCollateralId);

        IEnumerable<CollateralViewModel> GetTempCustomerCollateralForApproval(int companyId, int staffId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int companyId);
        CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId);
        CollateralViewModel GetTempCollateralTypeByCollateralId(int collateralId, int typeId);
        IEnumerable<CollateralViewModel> GetCollateralByCollateralTypeIdByCustomerId(int companyId, short collateralTypeId, int customerId, int thirdpartyCustomerId);
        IEnumerable<ActiveCustomerCollateralViewModel> GetActiveCustomerCollateral(int customerId);
        IEnumerable<ActiveCustomerCollateralViewModel> GetLoanCollateral(int loanId, int productTypeId);

        Task<bool> AddCollateralValuer(CollateralValuersViewModel entity);
        Task<bool> UpdateCollateralValuer(CollateralValuersViewModel entity, int id);

        bool ReleaseCollateral(int collateralMappingId, int staffId, GeneralEntity model);
        bool ApproveCollateralRelease(ApprovalViewModel entity, int staffId, GeneralEntity model);
        IEnumerable<ActiveCustomerCollateralViewModel> GetPendingCustomerCollateralRelease(int staffId);
        List<InsurancePolicy> GetCollateralInsurancePolicy(int collateralId);
        string GetInsurancePolicyCollateralReport(int trackingId);
        List<InsurancePolicy> GetCollateralInsurancePolicyReport(DateTime? startDate, DateTime? endDate, string searchString, int? businessUnitId);
        IQueryable<CollateralSearchViewModel> SearchCollateral(string searchString, int companyId);
        //bool AssignCollateral(ActiveCustomerCollateralViewModel entity);

        decimal GetAccountLeinAmountForFD(string accountNumber);

        decimal GetAccountLeinAmountForCASA(string accountNumber);

        CollateralHistory getCollateralHistory(int collateralId);


        #endregion Collateral

        #region Collateral Type
        IEnumerable<CollateralTypeViewModel> GetCollateralType();
        #endregion End of Collateral Type 

        #region Seniority Of Claims
        Task<bool> AddCollateralSeniorityOfClaims(CollateralSeniorityOfClaimsViewModel entity);
        Task<bool> DeleteCollateralSeniorityOfClaims(int seniorityOfClaimId, UserInfo user);
        Task<bool> UpdateCollateralSeniorityOfClaims(int seniorityOfClaimId, CollateralSeniorityOfClaimsViewModel entity);
        IEnumerable<CollateralSeniorityOfClaimsViewModel> GetCollateralSeniorityOfClaims();
        #endregion Seniority Of Claims

        #region Listing Functions
        IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType(short collateralType);
        IEnumerable<CollateralValuersViewModel> GetCollateralValuer(int companyId);
        IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType();
        IEnumerable<CollateralPerfectionStatusViewModel> GetCollateralPerfectionStatus();
        #endregion End Of Listing Functions

        IEnumerable<LoanApplicationCollateralViewModel> MapApplicationCollateral(ApplicationCollateralMapping entity);
        bool IsCollateralMapped(ApplicationCollateralMapping entity);
        IEnumerable<LoanApplicationCollateralViewModel> UnmapApplicationCollateral(ApplicationCollateralMapping entity);

        //  IEnumerable<CollateralLoanApplication> GetAllUnmappedCustomerCollateral(int customerId, int loanApplicationId, int companyId);
        //  IEnumerable<CollateralLoanApplication> GetAllMappedCustomerCollateral(int customerId, int loanApplicationId, int companyId);
        //  bool DeleteCollateralApplicationMapped(IEnumerable<CollateralLoanApplication> mappings, int companyId);

        #region Collateral Information View
        IEnumerable<AllCollateralViewModel> GetCollateralInformationById(int customercollateralId);


        #endregion


        int AddPropertyVistation(CollateralDocumentViewModel entity);

        IEnumerable<StockCompanyViewModel> getStockPrice();

        bool CheckForExpiredItemPolicies(DateTime currentDate);

        List<CollateralViewModel> AddGuaranteeJoinCollateral(CollateralViewModel entity, byte[] bufer);

        List<InsurancePolicy> GetCollateralInsurancePolicies(int collateralId);

        void AddTempItemInsurancePolicy(int collateralId, CollateralViewModel entity);

        bool AddNewItemInsurancePolicy(InsurancePolicy entity);

        int GoForApproval(ApprovalViewModel model);

        List<InsurancePolicy> GetTempCollateralInsurancePoliciesWaitingForApproval(int staffId);

        int GoForPolicyApproval(ApprovalViewModel model);

        List<CollateralDocumentViewModel> GetPropertyVistation(int collateralId);
        List<CollateralDocumentViewModel> GetTempPropertyVistation(int collateralId);
        List<InsurancePolicy> GetTempCollateralInsurancePolicy(int collateralId);

        CasaLienViewModel GetAccountLienDetail(string AccountNumber);
        //CasaLienViewModel GetAccountLienDetailForFD(string AccountNumber);//not implemented
        bool AddCollateralInsuranceTrackingForm(int accountOfficer, CollateralInsuranceTrackingViewModel model);
        IEnumerable<CollateralViewModel> GetCustomerCollateralByCollateralId(int companyId, int collaterId);

        IEnumerable<CollateralViewModel> GetCollateralStampToCoverValues(int customerId);

        TDAccountRecordViewModel GetFixedDepositAccountDetail(string AccpuntNumber);
        IEnumerable<CollateralViewModel> GetCustomerCollateralReport(string searchParam, int companyId);
        IEnumerable<CollateralViewModel> GetCustomerFixedDepositCollateral(string searchParam, int companyId);

        bool ProposeCollateralForUsage(CollateralCoverageViewModel model);
        bool RejectProposedCollateralForUsage(int collateralCustomerId);

        IEnumerable<CollateralUsageStatus> GetCollateralUsageStatus();

        IEnumerable<InsurancePolicy> GetInsuranceCompany();

        IEnumerable<InsurancePolicy> GetInsuranceType();

        bool AddInsurancePolicy(CollateralInsurancePolicyViewModel entity);

        List<InsurancePolicy> GetCollateralInsurancePoliciesWaitingForApproval(int staffId);
        IEnumerable<InsurancePolicy> Explore(string searchString);

        WorkflowResponse GoForInsurancePolicyApproval(ApprovalViewModel model);
        IEnumerable<CollateralCoverageViewModel> GetCollateralCoverage(int collateralSubTypeId);
        bool AddCollateralCoverage(CollateralCoverageViewModel model);
        bool DeleteCollateralCoverage(int collateralCoverageId, int createdById);
        IEnumerable<CollateralCoverageViewModel> CalculateCoverateOfCollateral(CollateralCoverageViewModel model);

        bool DeleteProposedCollateral(CollateralCoverageViewModel model);

        InsuranceCompanyViewModel GetInsuranceCompany(int id);
        IEnumerable<InsuranceCompanyViewModel> GetInsuranceCompanies();
        bool AddInsuranceCompany(InsuranceCompanyViewModel model);
        bool DeleteInsuranceCompany(int id, UserInfo user);
        bool UpdateInsuranceCompany(InsuranceCompanyViewModel model, int id, UserInfo user);
        bool DeleteInsurancePolicyType(int id, UserInfo user);
        bool AddInsurancePolicyType(InsurancePolicyTypeViewModel model);
        InsuranceTypeViewModel GetInsuranceType(int id);
        IEnumerable<InsuranceTypeViewModel> GetInsuranceTypes();
        IEnumerable<CollateralTypeViewModel> GetCollateralTypes();
        IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes(int collateralTypeId);
        IEnumerable<InsuranceStatusViewModel> GetInsuranceStatus();
        IEnumerable<InsurancePolicyTypeViewModel> GetInsurancePolicyTypes();
        
        bool AddInsuranceType(InsuranceTypeViewModel model);
        bool DeleteInsuranceType(int id, UserInfo user);
        bool UpdateInsuranceType(InsuranceTypeViewModel model, int id, UserInfo user);
        bool UpdateInsurancePolicyType(InsurancePolicyTypeViewModel model, int id, UserInfo user);

        bool AddInsurancePolicyFile(InsurancePolicy model);
        bool DeleteInsurancePolicy(int id, UserInfo user);
        bool UpdateInsurancePolicy(int id, CollateralInsurancePolicyViewModel model);

        IEnumerable<CollateralSwapViewModel> GetAllCollateralSwaps(int staffId);
        IEnumerable<CollateralSwapViewModel> GetCollateralSwapsForApproval(int staffId);
        IEnumerable<CollateralSwapViewModel> SearchCollateralSwap(string searchString);
        CollateralSwapViewModel GetCollateralSwap(int collateralSwapId);
        CollateralSwapViewModel AddCollateralSwap(CollateralSwapViewModel model);
        bool UpdateCollateralSwap(CollateralSwapViewModel model, int id, UserInfo user);
        bool DeleteCollateralSwap(int collateralSwapId, UserInfo user);
        IEnumerable<LoanApplicationDetailViewModel> GetCollateralMappingDetails(int id);
        CollateralInsurancePolicyViewModel GetAddedInsuranceById(int id);
        IEnumerable<FacilityStampDutyViewModel> GetFacilityStampDuty(int loanApplicationId);
        IEnumerable<FacilityStampDutyViewModel> GetFacilityStampDutyId(int loanApplicationId);
        IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDuty();
        IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyFixed();
        IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyFixedFiltered(DateRange dateRange);
        FacilityStampDutyViewModel GetFacilityStampDutyById(int loanApplicationId);
        IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyReport(DateRange param);
        IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyFiltered(DateRange param);
        bool AddFacilityStampDutySharing(FacilityStampDutyViewModel model);

        bool AddStampSetup(StampDutyConditionViewModel entity);
        IEnumerable<StampDutyConditionViewModel> GetStampSetup();
        bool UpdateStampSetup(int conditionId, StampDutyConditionViewModel entity);
        bool DeleteStampSetup(int conditionId, UserInfo user);
        WorkflowResponse CollateralSwapMemorandum(CollateralSwapViewModel model);

        String ResponseMessage(WorkflowResponse response, string itemHeading);
        int GetNextLevelForCollateralSwap(int collateralSwapId, int createdBy, int companyId);


        #region collateralInsuranceRequest

        bool AddInsurancePolicyRequest(CollateralInsuranceRequestViewModel model, int? id);
        string GetReferenceNumber();
        IEnumerable<CollateralViewModel> GetInsuranceRequests(int staffId);
        bool InsuranceRequestGoForApproval(CollateralViewModel model);
        InsurancePolicy GetInsurancePolicy(int collateralId);
        bool DeleteInsuranceRequest(int insuranceRequestId);
        bool checkInsurancePolicy(InsurancePolicy model);
        bool UpdateInsurancePolicyRequest(CollateralInsuranceRequestViewModel model, int id);
        string GetLastComment(int targetId, int operationId);
        bool UpdateCollateralInsuranceTrackingForm(int getStaffId, int id, CollateralInsuranceTrackingViewModel model);
        bool GetCustomerCollateralInsuranceDetailsConfirmation(int getStaffId, int id);
        bool DeleteCustomerCollateralInsuranceDetails(int getStaffId, int id);
        #endregion
    }
}
