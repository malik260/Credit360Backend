using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICollateralCustomerRepository
    {
        #region Collateral
        bool IsCollateralDocExists(string docName);
        Task<bool> AddCollateralCustomer(CollateralCustomerViewModel entity);
        Task<bool> DeleteCollateralCustomer(int collateralCustomerId, UserInfo user);
        Task<bool> UpdateCollateralCustomer(int collateralCustomerId, CollateralCustomerViewModel entity);
        IEnumerable<CollateralCustomerViewModel> GetCollateralCustomer(int customerId, int companyId);
        #endregion Collateral

        #region Miscellaneous Notes
        Task<bool> DeleteCollateralMiscellaneousNotes(int miscNoteId, UserInfo user);
        Task<bool> UpdateCollateralMiscellaneousNotes(int miscNoteId, CollateralMiscellaneousNotesViewModel entity);
        #endregion Miscellaneous Notes

        #region Seniority Of Claims
        Task<bool> AddCollateralSeniorityOfClaims(CollateralSeniorityOfClaimsViewModel entity);
        Task<bool> DeleteCollateralSeniorityOfClaims(int seniorityOfClaimId, UserInfo user);
        Task<bool> UpdateCollateralSeniorityOfClaims(int seniorityOfClaimId, CollateralSeniorityOfClaimsViewModel entity);
        IEnumerable<CollateralSeniorityOfClaimsViewModel> GetCollateralSeniorityOfClaims();
        #endregion Seniority Of Claims

        #region Listing Functions
        IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType();

        //IEnumerable<CollateralValuersViewModel> GetCollateralValuers(int companyId);

        IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType();

        //CollateralCustomerPolicyViewModel GetCollateralCustomerPolicyByCollateralCustomerId(short collateralCustomerId);

        CollateralSubTypeViewModel GetCollateralSubTypeById(short collateralSubTypeId);

        IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId);
        #endregion End Of Listing Functions

    }
}
