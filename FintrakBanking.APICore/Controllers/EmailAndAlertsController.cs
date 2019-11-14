using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/monitoring")]
    public class EmailAndAlertsController : ApiControllerBase
    {
        private IEmailAndAlertsRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IErrorLogRepository errorLog;

        public EmailAndAlertsController(IEmailAndAlertsRepository _repo, IErrorLogRepository _errorLog)
        {
            repo = _repo;
            errorLog = _errorLog;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("send-alerts/covenants-close-to-due-date")]
        public HttpResponseMessage SendAlertsForCovenantsApproachingDueDate()
        {
            try
            {
              // repo.SendAlertsForCovenantsApproachingDueDate();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (SecureException ex)
            {
                errorLog.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("send-alerts/covenants-overdue")]
        public HttpResponseMessage SendAlertsForCovenantsOverDue()
        {
            try
            {
              //  repo.SendAlertsForCovenantsOverDue();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (SecureException ex)
            {
                errorLog.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("send-alerts/collateral-property-revaluation")]
        public HttpResponseMessage SendAlertsForCollateralPropertyRevaluation()
        {
            try
            {
              //  repo.SendAlertsForCollateralPropertyRevaluation();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (SecureException ex)
            {
                errorLog.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("send-alerts/npl-loans")]
        public HttpResponseMessage SendAlertsForLoanNplMonitoring()
        {
            try
            {
               // repo.SendAlertsForLoanNplMonitoring();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (SecureException ex)
            {
                errorLog.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("send-alerts/self-liquidating-loans-expiry")]
        public HttpResponseMessage SendAlertsOnSelfLiquidatingLoanExpiry()
        {
            try
            {
              //  repo.SendAlertsOnSelfLiquidatingLoanExpiry();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (SecureException ex)
            {
                errorLog.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("send-alerts/overdraft-loans-expiry")]
        public HttpResponseMessage SendAlertsOnOverdraftLoansExpiry()
        {
            try
            {
              //  repo.SendAlertsOnOverDraftLoansAlmostDue();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (SecureException ex)
            {
                errorLog.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }
    }
}