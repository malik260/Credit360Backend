using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
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
    public class ProductFeeController : ApiControllerBase
    {
        private IProductFeeRepository repo;

        public ProductFeeController(IProductFeeRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("product-fee/all/{productId}")]
        public HttpResponseMessage GetFeeByProduct(HttpRequestMessage request, int productId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetFeeByProduct(productId);
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
        [Route("product-fee/unmapped/{productId}")]
        public HttpResponseMessage GetUnmappedFeeToProduct(HttpRequestMessage request, int productId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetUnmappedFeeToProduct(productId);
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
        [Route("product-fee/{productFeeId}")]
        public HttpResponseMessage GetProductFeeViewModel(HttpRequestMessage request, int productFeeId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetProductFeeViewModel(productFeeId);
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

        // POST api/values
        [HttpPost]
        [Route("product-fee")]
        public HttpResponseMessage AddProductFee(HttpRequestMessage request, [FromBody] ProductFeeViewModel model)
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

                    var recordId = repo.AddProductFee(model);
                    if (recordId >= 1)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                        Ok(new { success = true, result = recordId, message = "product fee has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                                        Ok(new { success = false, message = "product fee not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPost]
        [Route("product-fee/multiple")]
        public HttpResponseMessage AddMultipleProductFee(HttpRequestMessage request, [FromBody] List<ProductFeeViewModel> model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var recordId = repo.AddMultipleProductFee(model);
                    if (recordId >= 1)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                            Ok(new { success = true, result = recordId, message = "product fee(s) has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                                            Ok(new { success = false, message = "product fee not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPut]
        [Route("product-fee/{productFeeId}")]
        public HttpResponseMessage UpdateProductFee(HttpRequestMessage request, int productFeeId, [FromBody] ProductFeeViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (model == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                                            Ok(new { success = false, message = "No record found" }));
                }

                var data = repo.GetProductFeeViewModel(productFeeId);
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

                    repo.UpdateProductFee(productFeeId, model);

                    response = request.CreateResponse(HttpStatusCode.OK,
                                            Ok(new { success = true, result = productFeeId, message = "product fee has been updated successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpDelete]
        [Route("product-fee/{productFeeId}")]
        public HttpResponseMessage DeleteProductFee(HttpRequestMessage request, int productFeeId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                var account = repo.GetProductFeeViewModel(productFeeId);
                if (account == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                                                Ok(new { success = false, message = "No record found" }));
                }

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
                    repo.DeleteProductFee(productFeeId, user);

                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new
                    {
                        success = true,
                        result = productFeeId,
                        message = "product fee has been deleted successfully"
                    }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpDelete]
        [Route("product-fee/multiple/{productFeeIds}")]
        public HttpResponseMessage DeleteMultipleProductFee(HttpRequestMessage request, List<int> productFeeIds)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (productFeeIds.Count <= 0)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                                                    Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    repo.DeleteMultipleProductFee(productFeeIds);

                    response = request.CreateResponse(HttpStatusCode.OK,
                                                    Ok(new { success = true, result = 1, message = "product fee(s) has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
    }
}