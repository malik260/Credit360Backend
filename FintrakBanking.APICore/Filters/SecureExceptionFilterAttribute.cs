using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using FintrakBanking.Common.CustomException;
using System.Web.Http.ExceptionHandling;
using System.Threading;
using System.Threading.Tasks;
using WebGrease;

namespace FintrakBanking.APICore.Filters
{
    public class SecureExceptionFilterAttribute : ExceptionFilterAttribute
    {
        // public override async Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is SecureException)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = context.Exception.Message });
                return;
            }

            if (context.Exception is Exception)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occurred. Try again or contact the system administrator." });
            }

            base.OnException(context);
        }
        
    }
}
