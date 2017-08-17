using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IProductCollateralTypeRepository
    {
        IEnumerable<ProductCollateralTypeViewModel> GetCollateralTypeByProduct(int productId);

        IEnumerable<ProductCollateralTypeViewModel> GetMappedCollateralTypeByProduct(int productId);

        IEnumerable<CollateralTypeViewModel> GetUnmappedCollateralToProduct(int productId);

        ProductCollateralTypeViewModel GetProductCollateralTypeViewModel(int productCollateralTypeId);

        int AddProductCollateralType(ProductCollateralTypeViewModel collateralType);

        int AddMultipleProductCollateralType(List<ProductCollateralTypeViewModel> collateralTypes);
        void ApproveProductCollateral(int productId, UserInfo user);
        int AddTempProductCollateralType(ProductCollateralTypeViewModel productCollateral);

        bool DeleteProductCollateralType(int productCollateralTypeId, UserInfo user);

        bool DeleteMultipleProductCollateralType(List<int> productCollateralTypeIds, UserInfo user);

        bool DoesProductCollateralExist(int productCollateralTypeId);
    }
}