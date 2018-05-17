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

namespace FintrakBanking.APICore
{
    public class ClaimsAuthorization : AuthorizationFilterAttribute
    {

        public ClaimsAuthorization()
        {
         //   if (publicClientId == null) throw new ArgumentNullException("publicClientId");
            this._bankingContext = new FinTrakBankingContext();
        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public FinTrakBankingContext _bankingContext { get; private set; }
        public object publicClientId { get; private set; }

        //public string ClaimType { get; set; }
        //public   string ClaimValue { get; set; }
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // string Username = actionContext.RequestContext.Principal.Identity.Name;

            //var user = _bankingContext.TBL_PROFILE_USER.Where(p => p.USERNAME == Username).FirstOrDefault();
             var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (!principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }

            // if (!(principal.HasClaim(x => x.Type == "logincode" && x.Value == user.LOGINCODE)))
            // {
            //     actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            //     return Task.FromResult<object>(null);
            // }

            //User is Authorized, complete execution
            return Task.FromResult<object>(null);
        }
    }
}        
     