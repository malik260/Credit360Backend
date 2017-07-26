using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups;
using FintrakBanking.ViewModels.Setups.Credit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Credit
{
    public interface ICollateralTypeRepository
    {
        #region Collateral Types
        IEnumerable<CollateralTypeViewModel> GetCollateralTypes();

        CollateralTypeViewModel GetCollateralTypesById(int typeId);

        Task<bool> AddCollateralTypes(CollateralTypeViewModel entity);

        Task<bool> UpdateCollateralTypes(int typeId, CollateralTypeViewModel entity);

        Task<bool> DeleteCollateralTypes(int typeId, CollateralTypeViewModel entity, UserInfo user);

       // CollateralTypeViewModel GetCollateralTypeByCategoryId(int categoryId);

        #endregion Collateral Types

        #region Collateral Category
        //IEnumerable<CollateralCategoryViewModel> GetCollateralCategory();

        //CollateralCategoryViewModel GetCollateralCategoryById(int categoryId);

        //Task<bool> AddCollateralCategory(CollateralCategoryViewModel entity);

        //Task<bool> UpdateCollateralCategory(int categoryId, CollateralCategoryViewModel entity);

        //Task<bool> DeleteCollateralCategory(int categoryId, CollateralCategoryViewModel entity, UserInfo user);

        //IEnumerable<CollateralCategoryViewModel> GetCollateralCategoryByProductGroupId(int ProductGroupId);

        //IEnumerable<CollateralLocationsViewModel> GetCollateralLocations(int companyId);
        #endregion Collateral Category
              
      

       // #region Collateral Custom Fields
        //Task<bool> AddCollateralCustomFields(CollateralCustomFieldsViewModel entity);

        //Task<bool> UpdateCollateralCustomFields(int collateralCustomFieldsId, CollateralCustomFieldsViewModel entity);

        //Task<bool> DeleteCollateralCustomFields(int collateralCustomFieldsId, UserInfo user);

        //CollateralCustomFieldsViewModel CollateralCustomFieldsByCollateralCustomFieldsId(int collateralCustomFieldId, int companyId);

        //IEnumerable<CollateralCustomFieldsViewModel> CollateralCustomFieldsByCollateralTypeId(int collateralTypeId, int companyId);

        //IEnumerable<CollateralCustomFieldsViewModel> GetCollateralCustomFields(int companyId);
        //#endregion Collateral Custom Fields

    }
}