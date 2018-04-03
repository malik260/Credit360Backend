using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
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

        Task<bool> AddCollateral(CollateralViewModel entity);
        Task<bool> UpdateCollateral(CollateralViewModel entity, int collateralId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int? applicationId, int companyId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int companyId);
        CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId);

        IEnumerable<CollateralViewModel> GetCollateralByCollateralTypeIdByCustomerId(int companyId, short collateralTypeId, int customerId, int thirdpartyCustomerId);
        IEnumerable<ActiveCustomerCollateralViewModel> GetActiveCustomerCollateral(int customerId);
        IEnumerable<ActiveCustomerCollateralViewModel> GetLoanCollateral(int loanId);

        Task<bool> AddCollateralValuer(CollateralValuersViewModel entity);
        Task<bool> UpdateCollateralValuer(CollateralValuersViewModel entity, int id);

        bool ReleaseCollateral(int collateralMappingId, int staffId, GeneralEntity model);
        bool ApproveCollateralRelease(ApprovalViewModel entity, int staffId, GeneralEntity model);
        IEnumerable<ActiveCustomerCollateralViewModel> GetPendingCustomerCollateralRelease();

        IQueryable<CollateralSearchViewModel> SearchCollateral(string searchString, int companyId);
        //bool AssignCollateral(ActiveCustomerCollateralViewModel entity);

        decimal GetAccountLeinAmountForFD(string accountNumber);

        decimal GetAccountLeinAmountForCASA(string accountNumber);

        IEnumerable<CollateralHistory> getCollateralHistory(short collateralID);
        

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
        IEnumerable<LoanApplicationCollateralViewModel> UnmapApplicationCollateral(ApplicationCollateralMapping entity);

        //  IEnumerable<CollateralLoanApplication> GetAllUnmappedCustomerCollateral(int customerId, int loanApplicationId, int companyId);
        //  IEnumerable<CollateralLoanApplication> GetAllMappedCustomerCollateral(int customerId, int loanApplicationId, int companyId);
        //  bool DeleteCollateralApplicationMapped(IEnumerable<CollateralLoanApplication> mappings, int companyId);

        #region Collateral Information View
        IEnumerable<AllCollateralViewModel> GetCollateralInformationById(int customercollateralId);


        #endregion


        int AddPropertyVistation(CollateralDocumentViewModel entity);

        List<CollateralDocumentViewModel> GetPropertyVistation(int collateralVisitationId);

        IEnumerable<StockCompanyViewModel> getStockPrice();
    }
}
