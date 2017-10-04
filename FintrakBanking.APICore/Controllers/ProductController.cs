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

        public ProductController(IProductRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("product-category")]
        public HttpResponseMessage GetAllProductCategory()
        {
            try
            {
                var data = repo.GetAllProductCategory().ToList();
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
                return Request.CreateResponse(HttpStatusCode.OK,
         new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


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
                return Request.CreateResponse(HttpStatusCode.OK,
         new { success = true, result = data });  //Ok(accounts);
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
                var data = repo.GetAllProductGroup().ToList();
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
        //
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

        #endregion Product Type

        #region Product Region

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
                    item.createdBy = model.createdBy;
                    item.companyId = model.companyId;
                }

                foreach (var item in model.collaterals)
                {
                    item.createdBy = model.createdBy;
                    item.companyId = model.companyId;
                }

                foreach (var item in model.fees)
                {
                    item.createdBy = model.createdBy;
                    item.companyId = model.companyId;
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
        public async Task<HttpResponseMessage> GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = await repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Product record has been approved successfully" });
                }
                else
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

        #endregion Product Price index
    }
}