using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/alert/test")]
    public class AlertTestingController : ApiControllerBase
    {
        private readonly IExternalAlertRepository _repo;
        private readonly TokenDecryptionHelper _token = new TokenDecryptionHelper();

        public AlertTestingController(IExternalAlertRepository repo)
        {
            this._repo = repo;
        }

        [HttpGet]
        [Route("alert-imminent-maturit")]
        public HttpResponseMessage GetAlertImminentMaturity()
        {
            try
            {
                var alertViewModels = _repo.GetScheduleOfDirectorsAccounts(); 
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
