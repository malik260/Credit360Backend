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
    public class ProductCollateralTypeController : ApiControllerBase
    {
        private IProductCollateralTypeRepository repo;

        public ProductCollateralTypeController(IProductCollateralTypeRepository _repo)
        {
            this.repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-collateral-type/all/{productId}")]
        public HttpResponseMessage GetCollateralTypeByProduct(int productId)
        {
            try
            {
                var data = repo.GetCollateralTypeByProduct(productId);
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
        [Route("product-collateral-type/all/mapped/{productId}")]
        public HttpResponseMessage GetAllMappedCollateralToProduct(int productId)
        {
            try
            {
                var data = repo.GetMappedCollateralTypeByProduct(productId);
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
        [Route("product-collateral-type/unmapped/{productId}")]
        public HttpResponseMessage GetUnmappedCollateralToProduct(int productId)
        {
            try
            {
                var data = repo.GetUnmappedCollateralToProduct(productId);
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
        [Route("product-collateral-type/{productCollateralTypeId}")]
        public HttpResponseMessage GetProductCollateralTypeViewModel(int productCollateralTypeId)
        {
            try
            {
                var data = repo.GetProductCollateralTypeViewModel(productCollateralTypeId);
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

        // POST api/values
         [HttpPost] [ClaimsAuthorization]
        [Route("product-collateral-type")]
        public HttpResponseMessage AddProductCollateralType([FromBody] ProductCollateralTypeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var recordId = repo.AddTempProductCollateralType(model);
                if (recordId >= 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                    new
                                    {
                                        success = true,
                                        result = recordId,
                                        message = "product collateral type has been created successfully"
                                    });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                                    new { success = false, message = "product collateral type not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("product-collateral-type/multiple")]
        public HttpResponseMessage AddMultipleProductCollateralType([FromBody] List<ProductCollateralTypeViewModel> model)
        {
            try
            {
                var token = new TokenDecryptionHelper();

                var recordId = repo.AddMultipleProductCollateralType(model);
                if (recordId >= 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                        new
                                        {
                                            success = true,
                                            result = recordId,
                                            message = "product collateral type(s) has been created successfully"
                                        });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = false, message = "product collateral type not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-collateral-type/{productCollateralTypeId}")]
        public HttpResponseMessage DeleteProductCollateralType(int productCollateralTypeId)
        { //if (!repo.DoesProductCollateralExist(productCollateralTypeId))            
          //{
          //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
          //}

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
                var response = repo.DeleteProductCollateralType(productCollateralTypeId, user);

                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new
                        {
                            success = false,
                            message = "product collateral type has not been deleted successfully"
                        });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                                        new
                                        {
                                            success = true,
                                            result = productCollateralTypeId,
                                            message = "product collateral type has been deleted successfully"
                                        });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("product-collateral-type/multiple/{productCollateralTypeIds}")]
        public HttpResponseMessage DeleteMultipleProductCollateralType(List<int> productCollateralTypeIds)
        {
            if (productCollateralTypeIds.Count <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = "No record found"
                });
            }

            try
            {
                var token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.companyId = token.GetCompanyId;
                user.staffId = token.GetStaffId;
                user.applicationUrl = HttpContext.Current.Request.Path;
                user.userIPAddress = Request.RequestUri.Host;


                repo.DeleteMultipleProductCollateralType(productCollateralTypeIds, user);

                return Request.CreateResponse(HttpStatusCode.OK,
                                            new
                                            {
                                                success = true,
                                                result = 1,
                                                message = "product collateral type(s) has been deleted successfully"
                                            });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = false, message = ex.Message });
            }
        }
    }
}