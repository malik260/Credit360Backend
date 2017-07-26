using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

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
        public HttpResponseMessage GetAllProductCategory(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllProductCategory();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }

        [HttpGet]
        [Route("product-class")]
        public HttpResponseMessage GetAllProductClass(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllProductClass();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        #region Product Group

        [HttpGet]
        [Route("product-group")]
        public HttpResponseMessage GetAllProductGroup(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllProductGroup();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("product-group/{productGroupId}")]
        public HttpResponseMessage GetProductGroupById(HttpRequestMessage request, short productGroupId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductGroupById(productGroupId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPut]
        [Route("product-group/{productGroupId}")]
        public HttpResponseMessage UpdateProductGroup(HttpRequestMessage request, short productGroupId, [FromBody] ProductGroupViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (model == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok());
                }


                var data = repo.GetProductGroupById(productGroupId);
                if (data == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    repo.UpdateProductGroup(productGroupId, model);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = model.productGroupId, message = "product group has been updated successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        #endregion Product Group

        #region Product Type

        [HttpGet]
        [Route("product-type")]
        public HttpResponseMessage GetAllProductType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllProductType();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("product-type/{productTypeId}")]
        public HttpResponseMessage GetProductTypeById(HttpRequestMessage request, short productTypeId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductTypeById(productTypeId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("product-type-by-group/{productGroupId}")]
        public HttpResponseMessage GetProductTypeByProductGroup(HttpRequestMessage request, short productGroupId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductTypeByProductGroup(productGroupId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        // POST api/values
        [HttpPost]
        [Route("product-type")]
        public HttpResponseMessage AddProductType(HttpRequestMessage request, [FromBody] ProductTypeViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var recordId = repo.AddProductType(model);
                    if (recordId >= 1)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = recordId, message = "product type has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "product type not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPut]
        [Route("product-type/{productTypeId}")]
        public HttpResponseMessage UpdateProductType(HttpRequestMessage request, short productTypeId, [FromBody] ProductTypeViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (model == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok());
                }

                var account = repo.GetProductTypeById(productTypeId);
                if (account == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    repo.UpdateProductType(productTypeId, model);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = productTypeId, message = "product type has been updated successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }

        #endregion Product Type

        #region Product Region

        [HttpGet]
        [Route("product")]
        public HttpResponseMessage GetAllProduct(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllProduct();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("product/group/{productGroupId}/category/{productCategoryId}")]
        public HttpResponseMessage GetProductByGroupAndCategory(HttpRequestMessage request, short productGroupId, short productCategoryId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductByGroupAndCategory(productGroupId, productCategoryId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }

        [HttpGet]
        [Route("product/type/{productTypeId}/category/{productCategoryId}")]
        public HttpResponseMessage GetProductByTypeAndCategory(HttpRequestMessage request, short productTypeId, short productCategoryId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductByTypeAndCategory(productTypeId, productCategoryId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("product/{productId}")]
        public HttpResponseMessage GetProductById(HttpRequestMessage request, int productId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductById(productId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }

        [HttpPost]
        [Route("product")]
        public HttpResponseMessage AddProduct(HttpRequestMessage request, [FromBody] ProductViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var record = repo.AddProduct(model);
                    if (record != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = record, message = "product has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "product not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPut]
        [Route("product/{productId}")]
        public HttpResponseMessage UpdateProduct(HttpRequestMessage request, int productId, [FromBody] ProductViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (model == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok());
                }

                var account = repo.GetProductById(productId);
                if (account == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    repo.UpdateProduct(productId, model);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = productId, message = "product has been updated successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }

        #endregion Product Region

        #region Product Price Index

        [HttpGet]
        [Route("product-price-index")]
        public HttpResponseMessage GetAllProductPriceIndex(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var data = repo.GetProductPriceIndex(token.GetCompanyId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("product-price-index/{productPriceIndexId}")]
        public HttpResponseMessage GetProductPriceIndexById(HttpRequestMessage request, int productPriceIndexId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var data = repo.GetProductPriceIndexById(productPriceIndexId, token.GetBranchId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }


        [HttpPost]
        [Route("product-price-index")]
        public HttpResponseMessage AddProductPriceIndex(HttpRequestMessage request, [FromBody] ProductPriceIndexViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var record = repo.AddProductPriceIndex(model);
                    if (record != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = record, message = "product has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "product not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPut]
        [Route("product-price-index/{productPriceIndexId}")]
        public HttpResponseMessage UpdateProductPriceIndex(HttpRequestMessage request, int productPriceIndexId, [FromBody] ProductPriceIndexViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (model == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "product price index not found" }));
                }

                try
                {
                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    repo.UpdateProductPriceIndex(productPriceIndexId, model);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = productPriceIndexId, message = "product Price Index has been updated successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }


        [HttpDelete]
        [Route("product-price-index/{productPriceIndexId}")]
        public HttpResponseMessage DeleteProductPriceIndex(HttpRequestMessage request, int productPriceIndexId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        userIPAddress = Request.RequestUri.Host
                    };
                    repo.DeleteProductPriceIndex(productPriceIndexId, user);

                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = true, result = productPriceIndexId, message = "product Price Index has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        #endregion Product Price index
    }
}