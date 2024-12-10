using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.External.Product;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IProductRepository
    {
        IEnumerable<ProductCategoryViewModel> GetAllProductCategory();
        IEnumerable<RevolvingTypeViewModel> GetRevolvingTypes();
        IEnumerable<LookupViewModel> GetProductClassByProcessId(int processId);
        IEnumerable<LookupViewModel> GetAllProductClass();
        //IEnumerable<LookupViewModel> GetAllProductClass(int customerTypeId, int processId);
        IEnumerable<LookupViewModel> GetAllCRMSType(int companyId);
        IEnumerable<LookupViewModel> GetAllRiskRatingType(int companyId);

        ProductBehaviourViewModel GetProductBehaviour(int productId);

        // IEnumerable<ProductPriceIndexViewModel> GetAllProductPriceIndexByCurrencyId(int currencyId);
       ProductPriceIndexViewModel GetProductPriceIndexByProductId(int productId);
        //IEnumerable<LookupViewModel> GetAllProductClassByCustomerTypeId(int customerTypeId);
        #region Product
        IEnumerable<ApprovalStatusViewModel> GetApprovalStatus();
        ProductViewModel GetProductDetail(string productCode, int companyId);
        IEnumerable<ProductViewModel> GetProductAwaitingApprovals(int staffId, int companyId);
        ProductViewModel GetTempProductDetail(int productId);
        IEnumerable<ProductViewModel> GetAllProduct();
        IEnumerable<ProductViewModel> GetProductsByProductClassProcess(int productClassProcessId);
        IEnumerable<ProductViewModel> GetProductByProductGroup(int companyId);
        ProductViewModel GetProductById(int productId);
        IEnumerable<ProductViewModel> GetProductByGroupAndCategory(short productGroupId, short productCategoryId);
        IEnumerable<ProductViewModel> GetProductByTypeAndCategory(short productTypeId, short productCategoryId);
        bool IsProductCodeAlreadyExist(string productCode);
        bool IsProductExist(string productCode);
        WorkflowResponse GoForApprovalGlobalPriceIndex(ApprovalViewModel entity);

        int GoForApproval(ApprovalViewModel entity);
        Task<ProductViewModel> AddTempProduct(ProductViewModel product);
        Task<bool> UpdateProduct(int productId, ProductViewModel product);
        IEnumerable<LookupViewModel> GetAllProductBehaviourTypes();
        //bool DeleteProduct(int productId);
        IEnumerable<ProductSearchViewModel> GetAllLoanProduct(int companyId);
        //IEnumerable<ProductViewModel> GetAllProductByProductClass(int productClassId);
        IEnumerable<ProductViewModel> GetAllProductByProductClass(int productClassId, int customerTypeId);
        IEnumerable<ProductViewModel> GetAllProductByProductClassAndCustomerType(int productClassId, int customerTypeId);
        IEnumerable<ProductViewModel> GetAllProductsByProductClassIdAndCustomerTypeId(int productClassId, int customerTypeId);
        IEnumerable<LookupViewModel> GetProductCurrency(int productId);

        IEnumerable<ProductViewModel> Products();
        #endregion Product

        #region Product Price Index
        IEnumerable<ProductPriceIndexViewModel> GetProductPriceIndex(int companyId);
        IEnumerable<ProductPriceIndexGlobalViewModel> GetProductPriceIndexGlobal(int staffId);
        IEnumerable<ProductPriceIndexGlobalViewModel> GetProductPriceIndexGlobalAwaitingApproval(int staffId);
        ProductPriceIndexViewModel GetProductPriceIndexById(int productPriceIndexId, int companyId);
        ProductPriceIndexViewModel GetAllProductPriceIndicesById(int priceIndexId);
        ProductPriceIndexViewModel AddProductPriceIndex(ProductPriceIndexViewModel prodPriceIndex);
        WorkflowResponse AddProductPriceIndexGlobal(ProductPriceIndexGlobalViewModel prodPriceIndexGlobal);
        bool UpdateProductPriceIndexGlobal(int prodPriceIndexGlobalId, ProductPriceIndexGlobalViewModel prodPriceIndexGlobal);

        bool UpdateProductPriceIndex(int productPriceIndexId, ProductPriceIndexViewModel prodPriceIndex);

        bool DeleteProductPriceIndex(int productPriceIndexId, UserInfo user);
        IEnumerable<ProductPriceIndexCurrencyViewModel> GetProductPriceIndexCurrencyById(int productPriceIndexId);
        //ProductPriceIndexCurrencyViewModel AddProductPriceIndexCurrency(ProductPriceIndexCurrencyViewModel prodPriceIndexCurrency);

        //bool UpdateProductPriceIndexCurrency(int priceIndexCurrencyId, ProductPriceIndexCurrencyViewModel prodPriceIndexCurrency);

        //bool DeleteProductPriceIndexCurrency(int priceIndexCurrencyId, UserInfo user);
        List<ProductPriceIndexDailyViewModel> getProductPriceIndexHistory(DateTime startDate, DateTime endDate, int companyId);

        List<ProductPriceIndexViewModel> GetProductPriceIndexByCurrencyId(int currencyId);

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

        #region Product Class Process 
        IEnumerable<ProductClassProcessViewModel> GetAllProductClassProcesses();
        bool AddProductClassProcess(ProductClassProcessViewModel model);
        bool UpdateProductClassProcess(int productClassProcessId, ProductClassProcessViewModel model);
        ProductClassProcessViewModel GetProductProcessByProcessId(int proccessId);
        #endregion Product Class Process

        #region Product Classification 
        IEnumerable<LookupViewModel> GetAllProductClassType();
        IEnumerable<ProductClassificationViewModel> GetAllProductClassification();
        bool AddUpdateProductClassification(ProductClassificationViewModel model);
        bool ValidateProductClassification(string productClassName);
        IEnumerable<ProductLiteViewModel> GetAllProductLite();
        #endregion Product Classification 
        IEnumerable<DocumentDefinitionViewModel> GetAllDocumentDefinition();
        bool AddDocumentDefinition(DocumentDefinitionViewModel model);
        bool AddProductDocumentMapping(ProductDocumentMappingViewModel model);
        IEnumerable<ProductDocumentMappingViewModel> GetAllProductDocumentMapping();
        bool DeleteProductDocumentMapping(int id);  
        bool UpdateProductDocumentMapping(ProductDocumentMappingViewModel model);

        ProductDocumentMappingViewModel GetProductDocumenetMapping(int Id);
        Task<List<ProductForReturn>> GetAllExternalProductAsync();
        List<ProductForReturn> GetAllExternalProduct();
    }
}