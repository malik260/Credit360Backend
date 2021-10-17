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

        //private readonly FinTrakBankingContext _context = new FinTrakBankingContext();
        public ClaimsAuthorization()
        {

        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //public FinTrakBankingContext _bankingContext { get; private set; }
        public string publicClientId { get; private set; }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string Username = actionContext.RequestContext.Principal.Identity.Name;

            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (!principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }

            //var _context = new FinTrakBankingContext();
            //////if (!(principal.HasClaim(x => x.Type == "logincode" && x.Value == user.LOGINCODE)))
            /////ify
            //var user = _context.TBL_PROFILE_USER.Where(p => p.USERNAME == Username).FirstOrDefault();
            //var claim = principal.Claims.FirstOrDefault(x => x.Type.ToLower() == "logincode").Value;
            //if (!(claim == user.LOGINCODE))
            //{
            //    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            //    return Task.FromResult<object>(null);
            //}

            //User is Authorized, complete execution
            return Task.FromResult<object>(null);
        }
    }
}
