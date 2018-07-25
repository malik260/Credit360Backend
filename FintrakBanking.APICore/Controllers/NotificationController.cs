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
using FintrakBanking.ViewModels.Notification;
using FintrakBanking.Common.CustomException;

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

      [HttpGet] [ClaimsAuthorization]  
        [Route("workflow/all")]
        public HttpResponseMessage GetNotification()
        {
            try
            {
                var data = repo.GetWorkflowNotifications(token.GetStaffId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("pending-action/all")]
        public HttpResponseMessage GetAllNotifications()
        {
            try
            {
                var data = repo.GetAllNotifications(token.GetStaffId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("pending-action/{notificationId}")]
        public HttpResponseMessage UpdateNotificationState(int notificationId)
        {
            try
            {
                var data = repo.UpdateNotificationState(notificationId);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Notification state updated!" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Notification state not updated" });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("pending-action")]
        public HttpResponseMessage AddNotificationState(NotificationViewModel model)
        {
            try
            {
                var data = repo.AddNotification(model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Notification added successfully!" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Notification not added successfully" });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
