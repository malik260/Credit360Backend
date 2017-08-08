using System;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using FintrakBanking.APICore.core;
using System.Net;


namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class CollateralCustomerController : ApiControllerBase
    {
        private ICollateralCustomerRepository repo;

        public CollateralCustomerController()
        {
        }
        public CollateralCustomerController(ICollateralCustomerRepository _repo)
        {
            this.repo = _repo;
        }

        #region Collateral 
        [HttpPost][Route("customer-collateral")]
        public async Task<HttpResponseMessage> AddCollateral([FromBody] CollateralCustomerViewModel entity)
        {
            try
            {
                 TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.AddCollateralCustomer(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut][Route("customer-collateral/{collateralCustomerId}")]
        public async Task<HttpResponseMessage> UpdateCustomCollateral(int collateralCustomerId, [FromBody] CollateralCustomerViewModel entity)
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;

                var response = await repo.UpdateCollateralCustomer(collateralCustomerId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete][Route("customer-collateral/{collateralCustomerId}")]
        public async Task<HttpResponseMessage> DeleteCollateralCustomer(int collateralCustomerId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };

                var response = await repo.DeleteCollateralCustomer(collateralCustomerId, user);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("customer-collateral/{customerId}")]
        public HttpResponseMessage GetCollateralCustomer(int customerId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralCustomerByCustomerId(2,1);// repo.GetCollateralCustomer(customerId, token.GetCompanyId);

                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
        #endregion Collateral 

        #region Seniority Of Claims
        [HttpGet][Route("collateral-seniority-of-claims")]
        public HttpResponseMessage GetCollateralSeciorityOfClaims()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralSeniorityOfClaims();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
               // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Seniority Of Claims


        #region Listing Functions
        [HttpGet][Route("collateral-value-base-type")]
        public HttpResponseMessage GetCollateralValueBaseType()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralValueBaseType();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                 //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet][Route("collateral-valuers")]
        //public HttpResponseMessage GetCollateralValuers()
        //{
        //    try
        //    {
        //         TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        var response = repo.GetCollateralValuers(token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet][Route("collateral-valuer-type")]
        public HttpResponseMessage GetCollateralValuerType()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralValuerType();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet("collateral-policy/customer/{collateralCustomerId}")]
        //public IActionResult GetCollateralCustomerPolicyByCollateralCustomerId(short collateralCustomerId)
        //{
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralCustomerPolicyByCollateralCustomerId(collateralCustomerId);
        //        if (response == null)
        //        {

        //            return new { success = false, message = "No record found" });

        //        }
        //        return new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet][Route("collateral-sub-type/{collateralSubTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeById(short collateralSubTypeId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralSubTypeById(collateralSubTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet][Route("collateral-sub-type/collateral-type/{collateralTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion End of Listing Functions
    }
}