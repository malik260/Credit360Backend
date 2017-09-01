using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/credit")]
    public class LoanCovenantController : ApiControllerBase
    {
        private ILoanCovenantRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        IErrorLogRepository errorLogger;
        public LoanCovenantController(ILoanCovenantRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        [HttpPost][Route("covenant-detail")]
        public async Task<HttpResponseMessage> AddLoanCovenantDetail([FromBody] LoanCovenantDetailViewModel entity)
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.AddLoanCovenantDetail(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete][Route("covenant-detail")]
        public async Task<HttpResponseMessage> DeleteLoanCovenantDetail(int loanCovenantDetailId)
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    //applicationUrl = Request.Path.Value,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = await repo.DeleteLoanCovenantDetail(loanCovenantDetailId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut][Route("covenant-detail/{id}")]
        public async Task<HttpResponseMessage> UpdateLoanCovenantDetail([FromBody] LoanCovenantDetailViewModel entity, int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.UpdateLoanCovenantDetail(id, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

        [HttpGet][Route("covenant-detail/covenant-type/{id}")]
        public HttpResponseMessage GetLoanCovenantDetailByCovenantType(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetLoanCovenantDetailByCovenantType(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("covenant-detail/{id}")]
        public HttpResponseMessage GetLoanCovenantDetailById(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetLoanCovenantDetailById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("covenant-detail/loan/{id}")]
        public HttpResponseMessage GetLoanCovenantDetailByloanId(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetLoanCovenantDetailByloanId(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpPost][Route("covenant-type")]
        public async Task<HttpResponseMessage> AddLoanCovenantType([FromBody] LoanCovenantTypeViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.AddLoanCovenantType(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpGet][Route("covenant-type")]
        public HttpResponseMessage GetLoanCovenantType()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetLoanCovenantType(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut][Route("covenant-type/{id}")]
        public async Task<HttpResponseMessage> UpdateLoanCovenantType([FromBody] LoanCovenantTypeViewModel entity, short id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.UpdateLoanCovenantType(id, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
    }
}