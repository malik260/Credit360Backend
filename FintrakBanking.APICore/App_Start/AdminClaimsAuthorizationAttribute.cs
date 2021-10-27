using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Entities.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FintrakBanking.APICore
{
    public class AdminClaimsAuthorizationAttribute : AuthorizationFilterAttribute
    {

        //private readonly FinTrakBankingContext _context = new FinTrakBankingContext();
        public AdminClaimsAuthorizationAttribute()
        {

        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //public FinTrakBankingContext _bankingContext { get; private set; }
        public string publicClientId { get; private set; }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string Username = actionContext.RequestContext.Principal.Identity.Name;

            if (
                actionContext.Request.RequestUri.AbsolutePath.Contains("auth/token"))
                {
                return Task.FromResult<object>(null);
                }
              
                var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (!principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }
            var tokens = actionContext.Request.Headers.Authorization.Parameter;
            var _context = new FinTrakBankingContext();

            var user = _context.TBL_PROFILE_USER.Where(p => p.USERNAME == Username).FirstOrDefault();
            var claim = principal.Claims.FirstOrDefault(x => x.Type.ToLower() == "logincode").Value;
            var claim2 = token.GetUserActivities.ToLower();//principal.Claims.FirstOrDefault(x => x.Type.ToLower() == "useractivities").Value;

            if (!(claim == user.LOGINCODE))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }

            if (!claim2.Contains("admin"))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }

            /*var tokenIsValid   =  _context.TBL_USER_CLAIMS.Where(x => x.TOKEN == tokens && x.ISACTIVE == true).FirstOrDefault();
            if ( tokenIsValid == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }*/

            //User is Authorized, complete execution
            return Task.FromResult<object>(null);
        }
    }
}
