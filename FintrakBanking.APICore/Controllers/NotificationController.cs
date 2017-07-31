using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/notifications")]
    public class NotificationController : ApiControllerBase
    {
        INotificationRepository repo;
        public NotificationController(INotificationRepository _repo)
        {
            repo = _repo;
        }
        [HttpGet]
        [Route("all")]
        public HttpResponseMessage GetNotification()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetNotification(token.GetStaffId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
