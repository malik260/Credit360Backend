using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace FintrakBanking.APICore.Filters
{
    public class MiddlewareAuthorizeAttribute : ActionFilterAttribute
    {
        const string token = "VACGFSUQIWIWY2873891HHS63781SKIQ102983";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var accessToken = actionContext.Request.Headers.SingleOrDefault(x => x.Key == "authToken").Value?.FirstOrDefault();


            if (accessToken == string.Empty || accessToken == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Authorization failed, kindly confirm authToken.");
                return;
            }


            if (accessToken == token)
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Access token not valid or token is deactivated.");
                return;
            }
        }


    }
}