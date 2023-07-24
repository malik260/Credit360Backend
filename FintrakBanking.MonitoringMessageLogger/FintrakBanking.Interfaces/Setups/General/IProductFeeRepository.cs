using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using FintrakBanking.ViewModels.Setups.Finance;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IProductFeeRepository
    {
        IEnumerable<ProductFeeViewModel> GetAllMappedFeeByProduct(int productId);

        IEnumerable<ProductFeeViewModel> GetAllMappedFeeByTempProduct(int productId);

        IEnumerable<ChargeFeeViewModel> GetUnmappedFeeToProduct(int productId);

        ProductFeeViewModel GetProductFee(int productFeeId);
        List<ProductFeeViewModel> GetTempProductFee(int productFeeId);
        List<ProductFeeViewModel> GetProductFeeAwaitingApprovals(int tempProductId);

        int AddProductFee(ProductFeeViewModel productFee);

        int AddTempProductFee(ProductFeeViewModel productFee);

        void ApproveProductFee(int productId, UserInfo user);

        int AddMultipleProductFee(List<ProductFeeViewModel> productFees);

        bool UpdateProductFee(int productFeeId, ProductFeeViewModel productFee);

        bool DeleteProductFee(int productFeeId, UserInfo user);

        bool DeleteMultipleProductFee(List<int> productFeeIds);

        bool DoesProductFeeExist(int productFeeId);

        IEnumerable<dynamic> GetFeesByProductId(int productId);
        IEnumerable<dynamic> GetSavedFee(int loanApplicationDetailId, bool forModifyFacility);
    }
}