using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using System.Configuration;
using System.Data.Entity.Validation;

namespace FintrakBanking.APICore.Filters
{
    public class SecureExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();
        private readonly string support = ConfigurationManager.AppSettings["SupportEmailAddr"];

        public override void OnException(HttpActionExecutedContext context)
        {
            context.Exception.Data["validation_error_message"] = String.Empty;

            if (context.Exception is SecureException)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = context.Exception.Message });
                return;
            }

            if (context.Exception is DbEntityValidationException)
            {
                var e = (DbEntityValidationException)context.Exception;
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        context.Exception.Data["validation_error_message"] = context.Exception.Data["validation_error_message"] + ve.ErrorMessage + ", ";
                    }
                }
            }

            Task.Run(() => LogUnhandledExceptionAsync(context));

            if (context.Exception is Exception)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occurred. Try again or contact the system administrator." });
            }

            base.OnException(context);
        }

        private async Task LogUnhandledExceptionAsync(HttpActionExecutedContext httpContext)
        {
            var ex = httpContext.Exception;
            var endPoint = httpContext.Request.RequestUri;
            var userName = httpContext.ActionContext.RequestContext.Principal.Identity.Name;
            var errorMessage = ex.Message + " " + ex.Data["validation_error_message"];
            var time = DateTime.Now;

            if (ex.InnerException != null)
            {
                errorMessage = errorMessage + " -- " + ex.InnerException.Message;
            }

            var log = new TBL_ERRORLOG()
            {
                USERNAME = userName,
                APIENDPOINT = endPoint.ToString(),
                ERRORPATH = ex.TargetSite.ToString(),
                ERRORSOURCE = ex.Source,
                ERRORMESSAGE = errorMessage,
                ERRORTYPE = ex.GetType().Name,
                STATUSCODE = 500,
                ALLXML = errorMessage + " " + ex.StackTrace,
                TIMEUTC = time,
            };
            context.TBL_ERRORLOG.Add(log);

            string recipients = "anu.omotayo@fintraksoftware.com; osemeke.anyirah@fintraksoftware.com";

            var message = new TBL_MESSAGE_LOG
            {
                FROMADDRESS = support,
                TOADDRESS = recipients,
                MESSAGESUBJECT = "CREDIT 360 UNHANDLED EXCEPTION",
                MESSAGEBODY = "USERNAME: " + userName + " ENDPOINT: " + endPoint + " ERROR MESSAGE: " + errorMessage + " STACKTRACE: " + ex.StackTrace + " TIME: " + time, //
                MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                MESSAGETYPEID = (short)MessageTypeEnum.Email,
                DATETIMERECEIVED = time,
                SENDONDATETIME = time,
                TARGETID = null,
                OPERATIONID = null
            };
            context.TBL_MESSAGE_LOG.Add(message);

            await context.SaveChangesAsync();
        }
    }
}
