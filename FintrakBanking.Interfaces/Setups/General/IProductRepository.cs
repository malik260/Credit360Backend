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

        #region Product
        IEnumerable<ApprovalStatusViewModel> GetApprovalStatus();
        ProductViewModel GetProductDetail(string productCode, int companyId);
        IEnumerable<ProductViewModel> GetProductAwaitingApprovals(int staffId, int companyId);
        ProductViewModel GetTempProductDetail(int productId);
        IEnumerable<ProductViewModel> GetAllProduct();
        IEnumerable<ProductViewModel> GetProductByProductGroup(int companyId);
        ProductViewModel GetProductById(int productId);
        IEnumerable<ProductViewModel> GetProductByGroupAndCategory(short productGroupId, short productCategoryId);
        IEnumerable<ProductViewModel> GetProductByTypeAndCategory(short productTypeId, short productCategoryId);
        bool IsProductCodeAlreadyExist(string productCode);
        bool IsProductExist(string productCode);
        bool GoForApproval(ApprovalViewModel entity);
        Task<ProductViewModel> AddTempProduct(ProductViewModel product);
        Task<bool> UpdateProduct(int productId, ProductViewModel product);

        //bool DeleteProduct(int productId);

        #endregion Product

        #region Product Price Index
        IEnumerable<ProductPriceIndexViewModel> GetProductPriceIndex(int companyId);

        ProductPriceIndexViewModel GetProductPriceIndexById(int productPriceIndexId, int companyId);

        ProductPriceIndexViewModel AddProductPriceIndex(ProductPriceIndexViewModel prodPriceIndex);

        bool UpdateProductPriceIndex(int productPriceIndexId, ProductPriceIndexViewModel prodPriceIndex);

        bool DeleteProductPriceIndex(int productPriceIndexId, UserInfo user);

        #endregion Product Price Index

        #region Product Group
        IEnumerable<ProductGroupViewModel> GetAllProductGroup();

        ProductGroupViewModel GetProductGroupById(short productGroupId);

        bool UpdateProductGroup(int productGroupId, ProductGroupViewModel productGroup);

        bool AddProductGroup(ProductGroupViewModel productTypeModel);

        bool DeleteProductGroup(int productGroupId, UserInfo user);
        #endregion Product Group

        #region Product Type
        IEnumerable<ProductTypeViewModel> GetAllProductType();

        ProductTypeViewModel GetProductTypeById(short productTypeId);

        IEnumerable<ProductTypeViewModel> GetProductTypeByProductGroup(short productGroupId);

        short AddProductType(ProductTypeViewModel productType);

        bool UpdateProductType(int productTypeId, ProductTypeViewModel productType);

        bool DeleteProductType(int productTypeId, UserInfo user);

        #endregion
    }
}