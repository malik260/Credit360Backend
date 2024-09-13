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
using System.Linq;

namespace FintrakBanking.APICore.Filters
{
    public class SecureExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();
        private readonly string support = ConfigurationManager.AppSettings["SupportEmailAddr"];
        private string innerException = String.Empty;

        public override void OnException(HttpActionExecutedContext context)
        {
            context.Exception.Data["validation_error_message"] = String.Empty;
            if (context.Exception.InnerException != null) innerException = context.Exception.InnerException.Message;

            //var x = context.Exception.InnerException.GetType();

            if (context.Exception is SecureException || context.Exception is AggregateException)
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

            //Task.Run(() => LogUnhandledExceptionAsync(context));
            LogUnhandledException(context);

            context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occurred. Try again or contact the system administrator." });
            //context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = context.Exception.Message + " inner exception " + innerException });

            base.OnException(context);
        }

        private void LogUnhandledException(HttpActionExecutedContext httpContext)
        {
            //using(var trans = context.Database.BeginTransaction())
            //{
                var ex = httpContext.Exception;
                var test = ex.ToString();
                var endPoint = httpContext.Request.RequestUri;
                var userName = httpContext.ActionContext.RequestContext.Principal.Identity.Name;

                var errorMessage = ex.Message;
                if (innerException != null) errorMessage = errorMessage + ", INNER_EXCETION: " + innerException;
                var time = DateTime.Now;
                if (String.IsNullOrEmpty(ex.Data["validation_error_message"].ToString())) errorMessage = errorMessage + ", ENTITY_VALIDATION_ERROR: " + ex.Data["validation_error_message"];

                var log = new TBL_ERRORLOG()
                {
                    USERNAME = userName == null ? "SYSTEM" : userName,
                    APIENDPOINT = endPoint.ToString(),
                    ERRORPATH = ex.TargetSite.ToString(),
                    ERRORSOURCE = ex.Source,
                    ERRORMESSAGE = errorMessage,
                    ERRORTYPE = ex.GetType().Name,
                    STATUSCODE = 500,
                    ALLXML = errorMessage + " " + ex.StackTrace,
                    //ALLXML = test,
                    TIMEUTC = time,
                };

                context.TBL_ERRORLOG.Add(log);
                context.SaveChanges();

            string recipients = "augustine.nwaka@fintraksoftware.com;benjamin.gbaaikye@fintraksoftware.com;chisonm.okafor@fintraksoftware.com;";
            string recipients2 = context.TBL_STAFF.Where(s=>s.STAFFCODE.ToLower() == "supportstaff").Select(s=>s.EMAIL).FirstOrDefault();

                var message = new TBL_MESSAGE_LOG
                {
                    FROMADDRESS = support,
                    TOADDRESS = recipients,
                    MESSAGESUBJECT = "UNHANDLED EXCEPTION",
                    MESSAGEBODY = "<p><b>TIME:</b> " + time + "</p>< p><b>USERNAME:</b> " + (userName == null ? "SYSTEM" : userName) + "</p> <p><b>ENDPOINT:</b> " + endPoint + "</p> <p><b>ERROR MESSAGE:</b> " + errorMessage + "</p> <p><b>STACKTRACE:</b> " + ex.StackTrace + "</p> ", // MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                    MESSAGETYPEID = (short)MessageTypeEnum.Email,
                    DATETIMERECEIVED = time,
                    SENDONDATETIME = time,
                    TARGETID = null,
                    OPERATIONID = null,
                    MESSAGESTATUSID = 1
                };

                context.TBL_MESSAGE_LOG.Add(message);

                var message2 = new TBL_MESSAGE_LOG
                {
                    FROMADDRESS = support,
                    TOADDRESS = recipients2,
                    MESSAGESUBJECT = "UNHANDLED EXCEPTION",
                    MESSAGEBODY = "<p><b>TIME:</b> " + time + "</p>< p><b>USERNAME:</b> " + (userName == null ? "SYSTEM" : userName) + "</p> <p><b>ENDPOINT:</b> " + endPoint + "</p> <p><b>ERROR MESSAGE:</b> " + errorMessage + "</p> <p><b>STACKTRACE:</b> " + ex.StackTrace + "</p> ", // MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                    MESSAGETYPEID = (short)MessageTypeEnum.Email,
                    DATETIMERECEIVED = time,
                    SENDONDATETIME = time,
                    TARGETID = null,
                    OPERATIONID = null,
                    MESSAGESTATUSID = 1
                };

                context.TBL_MESSAGE_LOG.Add(message2);

            context.SaveChanges();
            //  trans.Commit();
            //}
        }

        private async Task LogUnhandledExceptionAsyncOld(HttpActionExecutedContext httpContext)
        {
            var ex = httpContext.Exception;
            var endPoint = httpContext.Request.RequestUri;
            var userName = httpContext.ActionContext.RequestContext.Principal.Identity.Name;
            var errorMessage = ex.Message;
            if (innerException != null) errorMessage = errorMessage + ", INNER_EXCETION: " + innerException;
            var time = DateTime.Now;
            if (String.IsNullOrEmpty(ex.Data["validation_error_message"].ToString())) errorMessage = errorMessage + ", ENTITY_VALIDATION_ERROR: " + ex.Data["validation_error_message"];

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

            string recipients = "augustine.nwaka@fintraksoftware.com;benjamin.gbaaikye@fintraksoftware.com;";

            var message = new TBL_MESSAGE_LOG
            {
                FROMADDRESS = support,
                TOADDRESS = recipients,
                MESSAGESUBJECT = "UNHANDLED EXCEPTION",
                MESSAGEBODY = "<p><b>TIME:</b> " + time + "</p>< p><b>USERNAME:</b> " + userName + "</p> <p><b>ENDPOINT:</b> " + endPoint + "</p> <p><b>ERROR MESSAGE:</b> " + errorMessage + "</p> <p><b>STACKTRACE:</b> " + ex.StackTrace + "</p> ", // MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                MESSAGETYPEID = (short)MessageTypeEnum.Email,
                DATETIMERECEIVED = time,
                SENDONDATETIME = time,
                TARGETID = null,
                OPERATIONID = null,
                MESSAGESTATUSID = 1
            };
            context.TBL_MESSAGE_LOG.Add(message);

            await context.SaveChangesAsync();
        }
    }
}
