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
using FintrakBanking.Interfaces.Setups.Finance;
using System.Web.Http.Cors;
using FintrakBanking.Interfaces.ErrorLogger;
using System.Linq;
using FintrakBanking.Interfaces.Setups.Credit;

namespace FintrakBanking.APICore.Controllers
{
    // GET: CustomerCollateral
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]

    [System.Web.Http.RoutePrefix("api/v1/credit")]
    public class CustomerCollateralController : ApiControllerBase
    {
        TokenDecryptionHelper token = null;
        private ICustomerCollateralRepository repo;
        private ICollateralTypeRepository repo_type;
        IErrorLogRepository errorLogger;

        public CustomerCollateralController(ICustomerCollateralRepository _repo, ICollateralTypeRepository _repo_type,
                                            IErrorLogRepository _errorLogger)
        {
            repo = _repo;
            this.repo_type = _repo_type;
            this.errorLogger = _errorLogger;
        }

        #region Collateral 
        [HttpPost]
        [Route("collateral-customer")]
        public async Task<HttpResponseMessage> AddCollateral([FromBody] CollateralCustomerViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
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
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("collateral-customer/{collateralCustomerId}")]
        public async Task<HttpResponseMessage> UpdateCustomCollateral(int collateralCustomerId, [FromBody] CollateralCustomerViewModel entity)
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = await repo.UpdateCollateralCustomer(collateralCustomerId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("collateral-customer/{collateralCustomerId}")]
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
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
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
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-customer/customer/{customerId}")]
        public HttpResponseMessage GetCollateralCustomer(int customerId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralCustomer(customerId, token.GetCompanyId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Collatera Types
        [HttpGet]
        [Route("collateral-type")]
        public HttpResponseMessage GetCollateralType(short collateralTypeId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo_type.GetCollateralTypes();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeById(short collateralSubTypeId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo_type.GetCollateralSubTypeById(collateralSubTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-sub-type/collateral-type/{collateralTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo_type.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("collateral-sub-type")]
        public async Task<HttpResponseMessage> AddCollateralSubType([FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.companyId = token.GetCompanyId;

                var response = await repo_type.AddCollateralSubTypes(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("collateral-type/{collateralTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralType(short collateralTypeId, [FromBody] CollateralTypeViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = await repo_type.UpdateCollateralTypes(collateralTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralSubType(short collateralSubTypeId, [FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = await repo_type.UpdateCollateralSubTypes(collateralSubTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public async Task<HttpResponseMessage> DeleteCollateralSubType(int collateralSubTypeId)
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
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var response = await repo.DeleteCollateralCustomer(collateralSubTypeId, user);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion  End of Collateral Types


        #region Seniority Of Claims
        [HttpGet]
        [Route("collateral-seniority-of-claims")]
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
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Seniority Of Claims


        #region Listing Functions
        [HttpGet]
        [Route("collateral-value-base-type")]
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
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-valuers")]
        public HttpResponseMessage GetCollateralValuers()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralValuer(token.GetCompanyId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-valuer-type")]
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
            catch (Exception ex)
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

      

        #endregion End of Listing Functions

    }
}