using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace FintrakBanking.APICore.Filters
{
    public class SecureExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();

        // public override async Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        public override void OnException(HttpActionExecutedContext context)
        { 
            var ctx = context;

            if (context.Exception is SecureException)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = context.Exception.Message });
                return;
            }

            // LogUnhandledException(context.Exception);
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
            var errorMessage = ex.Message;
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
                ERRORSOURCE = ex.Source.ToString(),
                ERRORMESSAGE = errorMessage,
                ERRORTYPE = ex.GetType().Name,
                STATUSCODE = 500,
                ALLXML = errorMessage + " " + ex.StackTrace,
                TIMEUTC = time,
            };
            context.TBL_ERRORLOG.Add(log);

            var message = new TBL_MESSAGE_LOG
            {
                TOADDRESS = "ft", //
                MESSAGESUBJECT = "CREDIT 360 UNHANDLED EXCEPTION",
                MESSAGEBODY = "ERROR MESSAGE: " + errorMessage + " STACKTRACE: " + ex.StackTrace + " TIME: " + time, //
                MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                MESSAGETYPEID = (short)MessageTypeEnum.Email,
                FROMADDRESS = "this.support", //
                DATETIMERECEIVED = DateTime.Now,
                SENDONDATETIME = DateTime.Now,
                TARGETID = null,
                OPERATIONID = null
            };
            context.TBL_MESSAGE_LOG.Add(message);

            await context.SaveChangesAsync();
        }
    }
}
/*
        private void LogUnhandledExceptions(Exception ex)
        {
            //    var errorLogger = new ErrorLogRepository();
            //    await errorLogger.LogErrorAsync(ex, "fake ip", "fake name");
            //}
            var errorMsg = ex.Message;
            if (ex.InnerException != null)
            {
                errorMsg += " " + ex.InnerException.Message;
            }

            var errorDetails = new TBL_ERRORLOG()
            {
                USERNAME = "test",
                ERRORPATH = "error type",
                ERRORSOURCE = ex.Source,
                ERRORMESSAGE = errorMsg,
                APIENDPOINT = "/end",
                ERRORTYPE = ex.GetType().Name,
                STATUSCODE = 401,
                ALLXML = errorMsg + " " + ex.StackTrace,
                TIMEUTC = DateTime.Now,
            };
            context.TBL_ERRORLOG.Add(errorDetails);
            int row = context.SaveChanges();

            if (row > 0) { Console.WriteLine("good"); }


        }
*/