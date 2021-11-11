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
using FintrakBanking.ViewModels.Setups;
using System.Collections.Generic;
using FintrakBanking.Common.CustomException;

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

         [HttpPost] [ClaimsAuthorization][Route("covenant-detail")]
        public HttpResponseMessage AddLoanCovenantDetail([FromBody] LoanCovenantDetailViewModel entity)
        {

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddLoanCovenantDetail(entity);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
        
        }

        [HttpDelete] [ClaimsAuthorization][Route("covenant-detail")]
        public async Task<HttpResponseMessage> DeleteLoanCovenantDetail(int loanCovenantDetailId)
        {

            try
            {
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
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization][Route("covenant-detail/{id}")]
        public async Task<HttpResponseMessage> UpdateLoanCovenantDetail([FromBody] LoanCovenantDetailViewModel entity, int id)
        {
            try
            {
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
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("covenant-detail/covenant-type/{id}")]
        public HttpResponseMessage GetLoanCovenantDetailByCovenantType(int id)
        {
            try
            {
                var data = repo.GetLoanCovenantDetailByCovenantType(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("covenant-detail/{id}")]
        public HttpResponseMessage GetLoanCovenantDetailById(int id)
        {
            try
            {
                var data = repo.GetLoanCovenantDetailById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("covenant-detail/loan/{id}")]
        public HttpResponseMessage GetLoanCovenantDetailByloanId(int id)
        {
            try
            {
                var data = repo.GetLoanCovenantDetailByloanId(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization][Route("covenant-type")]
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
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("covenant-type")]
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
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization][Route("covenant-type/{id}")]
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
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        // application


        [HttpGet]
        [ClaimsAuthorization]
        [Route("covenant/loan-application/{id}")]
        public HttpResponseMessage GetLoanApplicationCovenant(int id)
        {
            
                var data = repo.GetLoanApplicationCovenant(id);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else { 
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
                }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("covenant/loan-application-detail/{id}")]
        public HttpResponseMessage GetLoanApplicationDetailCovenant(int id)
        {
           
                var data = repo.GetLoanApplicationDetailCovenant(id);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
                }
        }

        
         [HttpPost] [ClaimsAuthorization]
        [Route("loan-application-covenant")]
        public HttpResponseMessage AddLoanApplicationCovenant([FromBody] LoanCovenantDetailViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                var data = repo.AddLoanApplicationCovenant(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("loan-application-covenant/{id}")]
        public HttpResponseMessage DeleteLoanApplicationCovenant(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                };
                var data = repo.DeleteLoanApplicationCovenant(id, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // lms approval

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-covenant/loan-application/{id}")]
        public HttpResponseMessage GetLoanApplicationCovenantLms(int id)
        {
            try
            {
                IEnumerable<LoanCovenantDetailViewModel> data = repo.GetLoanApplicationCovenantLms(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-loan-application-covenant")]
        public HttpResponseMessage AddLoanApplicationCovenantLms([FromBody] LoanCovenantDetailViewModel entity)
        {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                bool data = repo.AddLoanApplicationCovenantLms(entity);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lms-loan-application-covenant/{id}")]
        public HttpResponseMessage DeleteLoanApplicationCovenantLms(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                };
                bool data = repo.DeleteLoanApplicationCovenantLms(id, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("get-customer-account-balances")]
        //public HttpResponseMessage GetCustomerAccountBalanceFor()
        //{

        //    var data = repo.GetCustomerAccountBalance("1431919138");
        //    if (data != null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    else
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        //    }
        //}

    }
}