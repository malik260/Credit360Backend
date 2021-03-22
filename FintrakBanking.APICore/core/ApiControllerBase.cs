using FintrakBanking.APICore.App_Start;
using FintrakBanking.APICore.Filters;
using FintrakBanking.APICore.Providers;
using Serilog;
using System.Security;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Filters;

namespace FintrakBanking.APICore.core
{
    [JWTAuthorize]
    //[EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    //[SimpleRefreshTokenProvider]
    [AdministratorLockoutFilter]
    [ClaimsAuthorization]
    public class ApiControllerBase : ApiController
    {
        protected void ValidateAuthorizedUser(string userRequested)
        { 
            string userLoggedIn = User.Identity.Name;
            if (userLoggedIn != userRequested)
                throw new SecurityException("Attempting to access data for another user.");
        }
    }

    //public class UnhandledExceptionFilter : ExceptionFilterAttribute
    //{
    //    public override void OnException(HttpActionExecutedContext context)
    //    {
    //        //Log.Error(context.Exception,"OOps");
    //    }
    //}
}