using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/monitoring")]
    public class EmailAndAlertsController : ApiControllerBase
    {
        private IEmailAndAlertsRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public EmailAndAlertsController(IEmailAndAlertsRepository _repo)
        {
            repo = _repo;
        }

        [HttpGet]
        [Route("send-alerts/covenants-close-to-due-date")]
        public HttpResponseMessage SendAlertsForCovenantsApproachingDueDate()
        {
            try
            {
                repo.SendAlertsForCovenantsApproachingDueDate();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("send-alerts/covenants-overdue")]
        public HttpResponseMessage SendAlertsForCovenantsOverDue()
        {
            try
            {
                repo.SendAlertsForCovenantsOverDue();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There were errors: {ex.Message}" });
            }
        }
    }
}