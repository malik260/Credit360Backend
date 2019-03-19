using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace FintrakBanking.APICore.Filters
{
    public class JWTAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(actionContext);
            }
            else
            {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            }
        }
    }

    //public class TokenValidation : AuthorizeAttribute
    //{

    //}
}