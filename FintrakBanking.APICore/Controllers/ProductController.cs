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

        [HttpGet]
        [Route("product-process")]
        public HttpResponseMessage GetAllProductClassProcess()
        {
            try
            {
                var data = repo.GetAllProductClassProcess();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }

            catch (System.Exception ex)
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
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}




        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #region Product Group

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //1137
        //@B@cus7#12
        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("product-class/product-processid/{id}")]
        public HttpResponseMessage GetProductClassByProcessId(int id)
        {
            try
            {
                var data = repo.GetProductClassByProcessId(id) ;
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);

            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        #endregion Product Group

        #region Product Type

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // POST api/values
        [HttpPost]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Product Type

        #region Product Region

        [HttpGet]
        [Route("product-by-productclass/{id}")]
        public HttpResponseMessage GetAllProduct(int id)
        {
            try
            {
                 
                var data = repo.GetAllProductByProductClass(id).ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("product")]
        public HttpResponseMessage GetAllProduct()
        {
            try
            {
                var data = repo.GetAllProduct().ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-product")]
        public HttpResponseMessage GetAllLoanProduct()
        {
            try
            {
                var data = repo.GetAllLoanProduct().ToList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                //errorLogger.LogError(ex, HttpContext.Current.Request.UserHostAddress, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                //errorLogger.LogError(ex, HttpContext.Current.Request.UserHostAddress, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                //errorLogger.LogError(ex, HttpContext.Current.Request.UserHostAddress, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
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
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
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
            catch (System.Exception ex)
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
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }

        //}

        [HttpPut]
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
            catch (System.Exception ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Product record has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Product Region

        #region Product Price Index

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Product Price Index

        #region Product Class Process

        [HttpGet]
        [Route("product-class-process")]
        public HttpResponseMessage GetAllProductClassProcesses()
        {
            try
            {
                var data = repo.GetAllProductClassProcesses();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("product-class-process")]
        public HttpResponseMessage AddProductClassProcess([FromBody] ProductClassProcessViewModel model)
        {
            try
            {
                var data = repo.AddProductClassProcess(model);
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Product process added successfully!" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, message = "Product process not added successfully!" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("product-class-process/{productClassProcessId}")]
        public HttpResponseMessage UpdateProductClassProcess(int productClassProcessId, [FromBody] ProductClassProcessViewModel model)
        {
            try
            {
                var data = repo.UpdateProductClassProcess(productClassProcessId, model);
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Product process updated successfully!" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,

                    new { success = true, message = "Product process not updated successfully!" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        #endregion Product Class Process

    }
}