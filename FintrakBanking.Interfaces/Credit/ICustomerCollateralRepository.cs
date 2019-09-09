using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
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
        #region Collateral

        int AddCollateral(CollateralViewModel entity, byte[] file);
        //   bool AddCollateral(CollateralViewModel entity);
        int AddReleaseDocument(CollateralViewModel model, byte[] file);
        IEnumerable<CollateralViewModel> GetCollateralReleaseDocument(int releaseId);
        CollateralViewModel GetReleaseSupportingDocument(int documentId);

        bool ReleaseCollateral(CollateralViewModel entity);
        bool ReleaseCollateralJobRequest(CollateralViewModel entity);
        ApprovalResponse ReleaseCollateralGoForApproval(ApprovalViewModel entity);

        IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingApproval(int companyId, int staffId);

        IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingJobRequest(int companyId, int branchId);

        Task<bool> UpdateCollateral(CollateralViewModel entity, int collateralId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int? applicationId, int companyId);
        IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateral( int? applicationDetailId, int currencyId, int companyId);
        IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByCustomerId(int customerId);
        CollateralViewModel GetCustomerCollateralInformation(int collateralCustomerId, int companyId);
        List<CollateralViewModel> GetCustomerPropertyCollaterals(int? customerId, int companyId);

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
        List<InsurancePolicies> GetCollateralInsurancePolicy(int collateralId);
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

        List<InsurancePolicies> GetCollateralInsurancePolicies(int collateralId);

        void AddTempItemInsurancePolicy(int collateralId, CollateralViewModel entity);

        bool AddNewItemInsurancePolicy(InsurancePolicies entity);

        int GoForApproval(ApprovalViewModel model);

        List<InsurancePolicies> GetTempCollateralInsurancePoliciesWaitingForApproval(int staffId);

        int GoForPolicyApproval(ApprovalViewModel model);

        List<CollateralDocumentViewModel> GetPropertyVistation(int collateralId);
        List<CollateralDocumentViewModel> GetTempPropertyVistation(int collateralId);
        List<InsurancePolicies> GetTempCollateralInsurancePolicy(int collateralId);

        CasaLienViewModel GetAccountLienDetail(string AccountNumber);

        IEnumerable<CollateralViewModel> GetCustomerCollateralByCollateralId(int companyId, int collaterId);

        IEnumerable<CollateralViewModel> GetCollateralStampToCoverValues(int customerId);

        TDAccountRecordViewModel GetFixedDepositAccountDetail(string AccpuntNumber);
        IEnumerable<CollateralViewModel> GetCustomerCollateralReport(string searchParam, int companyId);

        bool ProposeCollateralForUsage(CollateralCoverageViewModel model);
        bool RejectProposedCollateralForUsage(int collateralCustomerId);

        IEnumerable<CollateralUsageStatus> GetCollateralUsageStatus();

        IEnumerable<InsurancePolicies> GetInsuranceCompany();

        IEnumerable<InsurancePolicies> GetInsuranceType();

        bool AddInsurancePolicy(InsurancePolicies entity);

        List<InsurancePolicies> GetCollateralInsurancePoliciesWaitingForApproval(int staffId);

        WorkflowResponse GoForInsurancePolicyApproval(ApprovalViewModel model);
        IEnumerable<CollateralCoverageViewModel> GetCollateralCoverage(int collateralSubTypeId);
        bool AddCollateralCoverage(CollateralCoverageViewModel model);
        bool DeleteCollateralCoverage(int collateralCoverageId, int createdById);
        IEnumerable<CollateralCoverageViewModel> CalculateCoverateOfCollateral(CollateralCoverageViewModel model);

        bool DeleteProposedCollateral(CollateralCoverageViewModel model);

        #region collateralInsuranceRequest

        bool AddInsurancePolicyRequest(CollateralInsuranceRequestViewModel model);
        string GetReferenceNumber();
        IEnumerable<CollateralViewModel> GetInsuranceRequests(int staffId);
        bool InsuranceRequestGoForApproval(CollateralViewModel model);
        InsurancePolicies GetInsurancePolicy(int collateralId);
        bool DeleteInsuranceRequest(int insuranceRequestId);
        bool checkInsurancePolicy(InsurancePolicies model);
        bool UpdateInsurancePolicyRequest(CollateralInsuranceRequestViewModel model, int id);
        #endregion
    }
}
