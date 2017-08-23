using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IProductRepository
    {
        IEnumerable<ProductCategoryViewModel> GetAllProductCategory();

        IEnumerable<LookupViewModel> GetAllProductClass();

        #region product
        IEnumerable<ApprovalStatusViewModel> GetApprovalStatus();
        ProductViewModel GetProductDetail(string productCode, int companyId);
        IEnumerable<ProductViewModel> GetProductAwaitingApprovals(int staffId, int companyId);
        ProductViewModel GetTempProductDetail(int productId);
        IEnumerable<ProductViewModel> GetAllProduct();

        ProductViewModel GetProductById(int productId);

        IEnumerable<ProductViewModel> GetProductByGroupAndCategory(short productGroupId, short productCategoryId);

        IEnumerable<ProductViewModel> GetProductByTypeAndCategory(short productTypeId, short productCategoryId);
        bool IsProductCodeAlreadyExist(string productCode);
        bool IsProductExist(string productCode);
        Task<bool> GoForApproval(ApprovalViewModel entity);
        ProductViewModel AddTempProduct(ProductViewModel product);

        bool UpdateProduct(int productId, ProductViewModel product);

        //bool DeleteProduct(int productId);

        #endregion product

        #region product Price Index
        IEnumerable<ProductPriceIndexViewModel> GetProductPriceIndex(int companyId);

        ProductPriceIndexViewModel GetProductPriceIndexById(int productPriceIndexId, int companyId);

        ProductPriceIndexViewModel AddProductPriceIndex(ProductPriceIndexViewModel prodPriceIndex);

        bool UpdateProductPriceIndex(int productPriceIndexId, ProductPriceIndexViewModel prodPriceIndex);

        bool DeleteProductPriceIndex(int productPriceIndexId, UserInfo user);

        #endregion product Price Index



        #region product group
        IEnumerable<ProductGroupViewModel> GetAllProductGroup();

        ProductGroupViewModel GetProductGroupById(short productGroupId);

        bool UpdateProductGroup(int productGroupId, ProductGroupViewModel productGroup);
        # endregion product group


        //-----------------product type---------------------
        IEnumerable<ProductTypeViewModel> GetAllProductType();

        ProductTypeViewModel GetProductTypeById(short productTypeId);

        IEnumerable<ProductTypeViewModel> GetProductTypeByProductGroup(short productGroupId);

        short AddProductType(ProductTypeViewModel productType);

        bool UpdateProductType(int productTypeId, ProductTypeViewModel productType);
        //-----------------end product type---------------------
    }
}