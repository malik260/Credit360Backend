using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;
using System;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class ProductController : ApiControllerBase
    {
        private IProductRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ProductController(IProductRepository _repo)
        {
            this.repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/group")]
        public HttpResponseMessage GetProductByProductGroup()
        {
            try
            {
                var data = repo.GetProductByProductGroup(new TokenDecryptionHelper().GetCompanyId).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-behaviour-types")]
        public HttpResponseMessage GetAllProductBehaviourTypes()
        {
            try
            {
                var data = repo.GetAllProductBehaviourTypes().ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("product-behaviour/{productId}")]
        public HttpResponseMessage GetAllProductBehaviour(int productId)
        {
            try
            {
                var data = repo.GetProductBehaviour(productId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet] [ClaimsAuthorization]  
        [Route("currency-by-product/{id}")]
        public HttpResponseMessage GetProductCurrency(int id)
        {
            try
            {
                var data = repo.GetProductCurrency(id).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #region Product Group

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-group")]
        public HttpResponseMessage GetAllProductGroup()
        {
            try
            {
                var data = repo.GetAllProductGroup();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-group/{productGroupId}")]
        public HttpResponseMessage GetProductGroupById(short productGroupId)
        {
            try
            {
                var data = repo.GetProductGroupById(productGroupId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("product-group")]
        public HttpResponseMessage AddProductGroup([FromBody] ProductGroupViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddProductGroup(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "product group has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "product group not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("product-group/{productGroupId}")]
        public HttpResponseMessage UpdateProductGroup(short productGroupId, [FromBody] ProductGroupViewModel model)
        {
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, Ok());
            }

            var data = repo.GetProductGroupById(productGroupId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "No record found" });
            }

            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                repo.UpdateProductGroup(productGroupId, model);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = model.productGroupId, message = "product group has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-group/{productGroupId}")]
        public HttpResponseMessage DeleteProductGroup(short productGroupId)
        {
            try
            {
                var account = repo.GetProductGroupById(productGroupId);

                if (account == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };
                var response = repo.DeleteProductGroup(productGroupId, user);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = productGroupId, message = "Product group has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Product group has not been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-category")]
        public HttpResponseMessage GetAllProductCategory()
        {
            try
            {
                var data = repo.GetAllProductCategory();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-class-by-cusstomertype/{id}")]
        public HttpResponseMessage GetAllProductClassByCustomerTypeId(int id)
        {
            try
            {
                var data = repo.GetAllProductClassByCustomerTypeId(id).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-class/customertype/{customertypeid}/process/{processId}")]
        public HttpResponseMessage GetAllProductClass(int customertypeid, int processId)
        {
            try
            {
                var data = repo.GetAllProductClass(customertypeid, processId).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //1137
        //@B@cus7#12
      [HttpGet] [ClaimsAuthorization]  
        [Route("product-class")]
        public HttpResponseMessage GetAllProductClass()
        {
            try
            {
                var data = repo.GetAllProductClass().ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-class/product-processid/{id}")]
        public HttpResponseMessage GetProductClassByProcessId(int id)
        {
            try
            {
                var data = repo.GetProductClassByProcessId(id);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        #endregion Product Group

        #region Product Type

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-type")]
        public HttpResponseMessage GetAllProductType()
        {
            try
            {
                var data = repo.GetAllProductType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("revolving-type")]
        public HttpResponseMessage GetRevolvingType()
        {
            try
            {
                var data = repo.GetRevolvingTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("product-type/{productTypeId}")]
        public HttpResponseMessage GetProductTypeById(short productTypeId)
        {
            try
            {
                var data = repo.GetProductTypeById(productTypeId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-type-by-group/{productGroupId}")]
        public HttpResponseMessage GetProductTypeByProductGroup(short productGroupId)
        {
            try
            {
                var data = repo.GetProductTypeByProductGroup(productGroupId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // POST api/values
         [HttpPost] [ClaimsAuthorization]
        [Route("product-type")]
        public HttpResponseMessage AddProductType([FromBody] ProductTypeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var recordId = repo.AddProductType(model);
                if (recordId >= 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = recordId, message = "product type has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "product type not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("product-type/{productTypeId}")]
        public HttpResponseMessage UpdateProductType(short productTypeId, [FromBody] ProductTypeViewModel model)
        {
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, Ok());
            }

            var account = repo.GetProductTypeById(productTypeId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "No record found" });
            }

            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                repo.UpdateProductType(productTypeId, model);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = productTypeId, message = "product type has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-type/{productTypeId}")]
        public HttpResponseMessage DeleteProductType(short productTypeId)
        {
            try
            {
                var account = repo.GetProductTypeById(productTypeId);

                if (account == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };
                var response = repo.DeleteProductType(productTypeId, user);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = productTypeId, message = "Product type has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, result = productTypeId, message = "Product type has not been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Product Type

        #region Product Region

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-by-productclass/{id}/customerType/{cid}")]
        public HttpResponseMessage GetAllProduct(int id, int cid)
        {
            try
            {

                var data = repo.GetAllProductByProductClass(id, cid).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product")]
        public HttpResponseMessage GetAllProduct()
        {
            try
            {
                var data = repo.GetAllProduct().ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-lite")]
        public HttpResponseMessage GetAllProductLite()
        {
            try
            {
                var data = repo.GetAllProductLite();
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-product")]
        public HttpResponseMessage GetAllLoanProduct()
        {
            try
            {
                var data = repo.GetAllLoanProduct(token.GetCompanyId).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/group/{productGroupId}/category/{productCategoryId}")]
        public HttpResponseMessage GetProductByGroupAndCategory(short productGroupId, short productCategoryId)
        {
            try
            {
                var data = repo.GetProductByGroupAndCategory(productGroupId, productCategoryId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/type/{productTypeId}/category/{productCategoryId}")]
        public HttpResponseMessage GetProductByTypeAndCategory(short productTypeId, short productCategoryId)
        {
            try
            {
                var data = repo.GetProductByTypeAndCategory(productTypeId, productCategoryId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-status")]
        public HttpResponseMessage GetApprovalStatus()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var productinfo = repo.GetApprovalStatus();

                if (productinfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = productinfo.ToList() });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/approvals/temp")]
        public HttpResponseMessage GetProductAwaitingApproval()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var productinfo = repo.GetProductAwaitingApprovals(token.GetStaffId, token.GetCompanyId);

                if (productinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = productinfo.ToList() });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, HttpContext.Current.Request.UserHostAddress, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/approvals/temp/{productId}")]
        public HttpResponseMessage GetTempProductDetailsById(int productId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var productInfo = repo.GetTempProductDetail(productId);

                if (productInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = productInfo });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, HttpContext.Current.Request.UserHostAddress, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/{productId}")]
        public HttpResponseMessage GetProductById(int productId)
        {
            try
            {
                var data = repo.GetProductById(productId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product/approvals/{productCode}")]
        public HttpResponseMessage GetProductDetailsProductCode(string productCode)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var productinfo = repo.GetProductDetail(productCode, token.GetCompanyId);

                if (productinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = productinfo });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, HttpContext.Current.Request.UserHostAddress, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // [HttpPost] [ClaimsAuthorization]
        //[Route("product")]
        //public HttpResponseMessage AddProduct([FromBody] ProductViewModel model)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper();
        //        model.userBranchId = (short)token.GetBranchId;
        //        model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //        model.applicationUrl = HttpContext.Current.Request.Path;
        //        model.createdBy = token.GetStaffId;
        //        model.companyId = token.GetCompanyId;

        //        var record = repo.AddProduct(model);
        //        if (record != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.Created,

        //                new { success = true, result = record, message = "product has been created successfully" });
        //        }
        //        else
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = false, message = "product not created" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

         [HttpPost] [ClaimsAuthorization]
        [Route("product")]
        public async Task<HttpResponseMessage> AddTempProduct([FromBody] ProductViewModel model)
        {
            try
            {
                if (repo.IsProductCodeAlreadyExist(model.productCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"A product with {model.productCode} already exist" });
                }
                if (repo.IsProductExist(model.productCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"A product with {model.productCode} already exist waiting for approval" });
                }

                TokenDecryptionHelper token = new TokenDecryptionHelper();

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Common.CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                foreach (var item in model.currencies)
                {
                    item.createdBy = token.GetStaffId;
                }

                foreach (var item in model.fees)
                {
                    item.createdBy = token.GetStaffId;
                }

                foreach (var item in model.collaterals)
                {
                    item.createdBy = token.GetStaffId;
                }

                var product = await repo.AddTempProduct(model);

                if (product != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = product, message = "Product has been created successfully, now waiting for approval" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Product not created" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPut]
        //[Route("product/{productId}")]
        //public HttpResponseMessage UpdateProduct(int productId, [FromBody] ProductViewModel model)
        //{
        //    if (model == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, Ok());
        //    }

        //    var account = repo.GetProductById(productId);
        //    if (account == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = false, message = "No record found" });
        //    }

        //    try
        //    {
        //        var token = new TokenDecryptionHelper();
        //        model.userBranchId = (short)token.GetBranchId;
        //        model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //        model.applicationUrl = HttpContext.Current.Request.Path;
        //        model.createdBy = token.GetStaffId;
        //        model.companyId = token.GetCompanyId;

        //        repo.UpdateProduct(productId, model);

        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, result = productId, message = "product has been updated successfully" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }

        //}

       [HttpPut] [ClaimsAuthorization]
        [Route("product/{productId}")]
        public async Task<HttpResponseMessage> UpdateProduct(int productId, [FromBody] ProductViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path; 
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                foreach (var item in model.currencies)
                {
                    item.createdBy = token.GetStaffId;
                }

                foreach (var item in model.fees)
                {
                    item.createdBy = token.GetStaffId;
                }

                foreach (var item in model.collaterals)
                {
                    item.createdBy = token.GetStaffId;
                }

                var staff = await repo.UpdateProduct(productId, model);

                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "Product has been updated successfully, now awaiting approval" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Product not created" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("product/approval")]
        public HttpResponseMessage GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);

                if (data == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Product record has been approved successfully." });
                }
                else if (data == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Product details has been disapproved." });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office." });
                }
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $" {ex.Message}" });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: An unhandled error occured." });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-type")]
        public HttpResponseMessage GetAllCRMSType()
        {
            try
            {
                var data = repo.GetAllCRMSType();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Product Region

        #region Product Price Index

        [HttpGet] [ClaimsAuthorization]  
        [Route("product-price-index")]
        public HttpResponseMessage GetAllProductPriceIndex()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetProductPriceIndex(token.GetCompanyId).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-price-index/{productPriceIndexId}")]
        public HttpResponseMessage GetProductPriceIndexById(int productPriceIndexId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetProductPriceIndexById(productPriceIndexId, token.GetBranchId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("product-price-index")]
        public HttpResponseMessage AddProductPriceIndex([FromBody] ProductPriceIndexViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var record = repo.AddProductPriceIndex(model);
                if (record != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = record, message = "product has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "product not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("product-price-index/{productPriceIndexId}")]
        public HttpResponseMessage UpdateProductPriceIndex(int productPriceIndexId, [FromBody] ProductPriceIndexViewModel model)
        {
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "product price index not found" });
            }

            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                repo.UpdateProductPriceIndex(productPriceIndexId, model);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = productPriceIndexId, message = "product Price Index has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-price-index/{productPriceIndexId}")]
        public HttpResponseMessage DeleteProductPriceIndex(int productPriceIndexId)
        {
            try
            {
                var token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };
                repo.DeleteProductPriceIndex(productPriceIndexId, user);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = productPriceIndexId, message = "product Price Index has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Product Price Index

        #region Product Class Process

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-process")]
        public HttpResponseMessage GetAllProductClassProcess()
        {
            try
            {
                var data = repo.GetAllProductClassProcesses();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }

            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("product-process")]
        public HttpResponseMessage AddProductClassProcess([FromBody] ProductClassProcessViewModel model)
        {
            try
            {
                var data = repo.AddProductClassProcess(model);
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Product process not added successfully!" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, message = "Product process added successfully!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("product-process/{productProcessId}")]
        public HttpResponseMessage UpdateProductClassProcess(int productProcessId, [FromBody] ProductClassProcessViewModel model)
        {
            try
            {
                var data = repo.UpdateProductClassProcess(productProcessId, model);
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Product process not updated successfully!" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, message = "Product process updated successfully!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route("product-process/{id}")]
        //public HttpResponseMessage GetProductProductProcessById(int id)
        //{
        //    try
        //    {
        //        var data = repo.GetProductClassProcess(id);
        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        #endregion Product Class Process

        #region Product Classification

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-class-type")]
        public HttpResponseMessage GetAllProductClassTypes()
        {
            try
            {
                var data = repo.GetAllProductClassType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }

            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("product-classification")]
        public HttpResponseMessage GetAllProductClassification()
        {
            try
            {
                var data = repo.GetAllProductClassification();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }

            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("product-classification")]
        public HttpResponseMessage AddProductClassification([FromBody] ProductClassificationViewModel model)
        {
            try
            {
                string createUpdate = "";
                if (model.productClassId != 0 || model.productClassId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateProductClassification(model.productClassName))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                               new { success = false, message = "Product Class with same name already exist." });
                    }
                }
                model.userBranchId = (short)token.GetBranchId;
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = (short)token.GetCompanyId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;

                var data = repo.AddUpdateProductClassification(model);
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"Product class not {createUpdate} successfully!" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, message = $"Product class {createUpdate} successfully!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        #endregion Product Classification
    }
}