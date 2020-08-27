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
using FintrakBanking.Common.CustomException;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class ProductFeeController : ApiControllerBase
    {
        private IProductFeeRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ProductFeeController(IProductFeeRepository _repo)
        {
            this.repo = _repo;
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("fee/product/{id}")]
        public HttpResponseMessage GetFee(int id)
        {
            try
            {
                var data = repo.GetFeesByProductId(id);
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

        [HttpGet]
        [ClaimsAuthorization]
        [Route("fee/saved-Facility/{loanApplicationDetailId}/{forModifyFacility}")]
        public HttpResponseMessage GetSavedFee(int loanApplicationDetailId, bool forModifyFacility)
        {
            try
            {
                var data = repo.GetSavedFee(loanApplicationDetailId, forModifyFacility);
                if (data.Count() == 0)
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
        [Route("product-fee/all/{productId}")]
        public HttpResponseMessage GetFeeByProduct(int productId)
        {
            try
            {
                var data = repo.GetAllMappedFeeByProduct(productId);
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
        [Route("product-fee/all/mapped/{productId}")]
        public HttpResponseMessage GetAllMappedFeeByProduct(int productId)
        {
            try
            {
                var data = repo.GetAllMappedFeeByProduct(productId);
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
        [Route("product-fee/unmapped/{productId}")]
        public HttpResponseMessage GetUnmappedFeeToProduct(int productId)
        {
            try
            {
                var data = repo.GetUnmappedFeeToProduct(productId);
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
        [Route("product-fee/{productFeeId}")]
        public HttpResponseMessage GetProductFee(int productFeeId)
        {
            try
            {
                var data = repo.GetProductFee(productFeeId);
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
        [Route("product-fee/temp/{productFeeId}")]
        public HttpResponseMessage GetTempProductFee(int productFeeId)
        {
            try
            {
                var data = repo.GetTempProductFee(productFeeId);
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
        [Route("product-fee/approvals/temp{tempProductId}")]
        public HttpResponseMessage GetProductFeeAwaitingApproval(int tempProductId)
        {
            try
            {
                var productFeeinfo = repo.GetProductFeeAwaitingApprovals(tempProductId);

                if (productFeeinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = productFeeinfo.ToList() });
            }
            catch (SecureException ex)
            {
                // errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // POST api/values
         [HttpPost] [ClaimsAuthorization]
        [Route("product-fee")]
        public HttpResponseMessage AddTempProductFee([FromBody] ProductFeeViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var recordId = repo.AddProductFee(model);
                if (recordId >= 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                    new
                                    {
                                        success = true,
                                        result = recordId,
                                        message = "product fee has been created successfully"
                                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "product fee not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("product-fee/multiple")]
        public HttpResponseMessage AddMultipleProductFee([FromBody] List<ProductFeeViewModel> model)
        {
            try
            {
                var recordId = repo.AddMultipleProductFee(model);
                if (recordId >= 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, result = recordId, message = "product fee(s) has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "product fee not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("product-fee/{productFeeId}")]
        public HttpResponseMessage UpdateProductFee(int productFeeId, [FromBody] ProductFeeViewModel model)
        {
            if (model == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = false, message = "No record found" });
            }

            var data = repo.GetProductFee(productFeeId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = false, message = "No record found" });
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

                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, result = productFeeId, message = "product fee has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-fee/{productFeeId}")]
        public HttpResponseMessage DeleteProductFee(int productFeeId)
        {
            var account = repo.GetProductFee(productFeeId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = false, message = "No record found" });
            }

            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host
                };
                var response = repo.DeleteProductFee(productFeeId, user);

                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = false,
                        message = "product fee has not been deleted successfully"
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    result = productFeeId,
                    message = "product fee has been deleted successfully"
                });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-fee/multiple/{productFeeIds}")]
        public HttpResponseMessage DeleteMultipleProductFee(List<int> productFeeIds)
        {
            if (productFeeIds.Count <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                                new { success = false, message = "No record found" });
            }

            try
            {
                repo.DeleteMultipleProductFee(productFeeIds);

                return Request.CreateResponse(HttpStatusCode.OK,
                                                new { success = true, result = 1, message = "product fee(s) has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}