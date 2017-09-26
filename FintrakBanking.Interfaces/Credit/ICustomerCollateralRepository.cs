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
        Task<bool> AddCollateral(CollateralViewModel entity);
        Task<bool> UpdateCollateral(CollateralViewModel entity, int collateralId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int companyId);
        IEnumerable<CollateralViewModel> GetCustomerCollateral(int companyId);
        CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId);

        IEnumerable<CollateralViewModel> GetCollateralByCollateralTypeIdByCustomerId(int companyId, short collateralTypeId, int customerId, int thirdpartyCustomerId);

        //#region Collateral
        //Task<bool> AddCollateralCustomer(CollateralCustomerViewModel entity);
        //Task<bool> DeleteCollateralCustomer(int collateralCustomerId, UserInfo user);
        //Task<bool> UpdateCollateralCustomer(int collateralCustomerId, CollateralCustomerViewModel entity);
        //IEnumerable<CollateralCustomerViewModel> GetCollateralCustomer(int customerId, int companyId);
        //bool IsCollateralDocExists(string docName);
        Task<bool> AddCollateralValuer(CollateralValuersViewModel entity);
        Task<bool> UpdateCollateralValuer(CollateralValuersViewModel entity, int id);
        //#endregion Collateral

        #region Collateral Type
        IEnumerable<CollateralTypeViewModel> GetCollateralType();
        #endregion End of Collateral Type 

        //#region Miscellaneous Notes
        //Task<bool> DeleteCollateralMiscellaneousNotes(int miscNoteId, UserInfo user);
        //Task<bool> UpdateCollateralMiscellaneousNotes(int miscNoteId, CollateralMiscellaneousNotesViewModel entity);
        //#endregion Miscellaneous Notes

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

        //CollateralCustomerPolicyViewModel GetCollateralCustomerPolicyByCollateralCustomerId(short collateralCustomerId);

        // IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes();

        //IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId);
        #endregion End Of Listing Functions

    }
}
