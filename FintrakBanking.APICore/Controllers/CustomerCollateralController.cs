using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class CustomerCollateralController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
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
        [Route("customer-collateral")]
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
        [Route("customer-collateral/{collateralCustomerId}")]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("customer-collateral/{collateralCustomerId}")]
        public async Task<HttpResponseMessage> DeleteCollateralCustomer(int collateralCustomerId)
        {
            try
            {
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
        [Route("customer-collateral/customer/{customerId}")]
        public HttpResponseMessage GetCollateralCustomer(int customerId)
        {
            try
            {
                var response = repo.GetCollateralCustomer(customerId, token.GetCompanyId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Collatera Types
        [HttpGet]
        [Route("collateral-type")]
        public HttpResponseMessage GetCollateralType()
        {
            try
            {
                var response = repo_type.GetCollateralTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-sub-type")]
        public HttpResponseMessage GetCollateralSubTypes()
        {
            try
            {
                var response = repo_type.GetCollateralSubTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-sub-type/collateral-type/{collateralTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            try
            {
                var response = repo_type.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("collateral-sub-type")]
        public async Task<HttpResponseMessage> AddCollateralSubType([FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("collateral-type/{collateralTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralType(short collateralTypeId, [FromBody] CollateralTypeViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;

                var response = await repo_type.UpdateCollateralTypes(collateralTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralSubType(short collateralSubTypeId, [FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;

                var response = await repo_type.UpdateCollateralSubTypes(collateralSubTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public async Task<HttpResponseMessage> DeleteCollateralSubType(int collateralSubTypeId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Seniority Of Claims


        [HttpGet]
        [Route("collateral-valuer")]
        public HttpResponseMessage GetCollateralValuers()
        {
            try
            {
                var response = repo.GetCollateralValuer(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-valuer-type")]
        public HttpResponseMessage GetCollateralValuerType()
        {
            try
            {
                var response = repo.GetCollateralValuerType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-value-base-type")]
        public HttpResponseMessage GetCollateralValueBaseType()
        {
            try
            {
                var response = repo.GetCollateralValueBaseType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}