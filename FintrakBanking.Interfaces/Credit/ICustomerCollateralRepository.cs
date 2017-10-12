using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
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
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int companyId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int companyId);
        CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId);

        IEnumerable<CollateralViewModel> GetCollateralByCollateralTypeIdByCustomerId(int companyId, short collateralTypeId, int customerId, int thirdpartyCustomerId);
        IEnumerable<ActiveCustomerCollateralViewModel> GetActiveCustomerCollateral(int customerId);

        Task<bool> AddCollateralValuer(CollateralValuersViewModel entity);
        Task<bool> UpdateCollateralValuer(CollateralValuersViewModel entity, int id);

        bool ReleaseCollateral(int collateralMappingId, int staffId, GeneralEntity model);
        bool ApproveCollateralRelease(int collateralMappingId, int staffId, GeneralEntity model);

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
        IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType();
        IEnumerable<CollateralValuersViewModel> GetCollateralValuer(int companyId);
        IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType();
        #endregion End Of Listing Functions

    }
}
