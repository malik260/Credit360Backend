using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IProductFeeRepository
    {
        IEnumerable<ProductFeeViewModel> GetFeeByProduct(int productId);

        IEnumerable<FeeViewModel> GetUnmappedFeeToProduct(int productId);

        ProductFeeViewModel GetProductFeeViewModel(int productFeeId);

        int AddProductFee(ProductFeeViewModel productFee);

        int AddMultipleProductFee(List<ProductFeeViewModel> productFees);

        bool UpdateProductFee(int productFeeId, ProductFeeViewModel productFee);

        bool DeleteProductFee(int productFeeId, UserInfo user);

        bool DeleteMultipleProductFee(List<int> productFeeIds);

        bool DoesProductFeeExist(int productFeeId);
    }
}