using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Interfaces.ErrorLogger;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/notifications")]
    public class NotificationController : ApiControllerBase
    {
        INotificationRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        IErrorLogRepository errorLogger;

        public NotificationController(INotificationRepository _repo, IErrorLogRepository _errorLogger)
        {
            repo = _repo;
            errorLogger = _errorLogger;
        }

        [HttpGet]
        [Route("all")]
        public HttpResponseMessage GetNotification()
        {
            try
            {
                var data = repo.GetNotification(token.GetStaffId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("completed-approval/all")]
        public HttpResponseMessage GetNotificationForFinalState()
        {
            try
            {
                var data = repo.GetNotificationForFinalState(token.GetStaffId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
