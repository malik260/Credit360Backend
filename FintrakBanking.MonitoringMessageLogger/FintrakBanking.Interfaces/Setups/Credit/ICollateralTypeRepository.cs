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
        Task<bool> UpdateCollateralTypes(int typeId, CollateralTypeViewModel entity);
        IEnumerable<CollateralDocumentTypeViewModel> GetCollateralDocumentTypes(int id);
        bool AddCollateralDocumentType(CollateralDocumentTypeViewModel entity);

        IEnumerable<CollateralTypeViewModel> GetCollateralTypes();
        CollateralTypeViewModel GetCollateralTypesById(int typeId);
        IEnumerable<CollateralTypeViewModel> CollateralTypesByLoanApplication(int? id);

        #endregion End of Collateral Types


        #region Collateral SubTypes
        IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes();
        CollateralSubTypeViewModel CollateralSubType(int Id);
        IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypeByCollateralTypeId(short collateralTypeId);
        Task<bool> AddCollateralSubTypes(CollateralSubTypeViewModel entity);
        Task<bool> DeleteCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity, UserInfo user);
        bool UpdateCollateralSubTypes(int subTypeId, CollateralSubTypeViewModel entity);
        #endregion End of Collateral SubTypes


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