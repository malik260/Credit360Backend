using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace FintrakBanking.APICore.Filters
{

    public class AdministratorLockoutFilter : AuthorizeAttribute
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            int? roleId = Int32.Parse(new ClaimsIdentity(HttpContext.Current.User.Identity).Claims.First(x => x.Type == "roleId").Value);
            if (roleId == 2 || roleId == 0 || roleId == null) return true;
            var countryId = Int32.Parse(new ClaimsIdentity(HttpContext.Current.User.Identity).Claims.First(x => x.Type == "countryId").Value);
            var auth = new AuthenticationRepository(context, null,null);
            if (auth.GetRunningEndOfDayProcess(countryId)) return false;
            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext filterContext)
        {
            filterContext.Response = new HttpResponseMessage();
            filterContext.Response.StatusCode = HttpStatusCode.Forbidden;
        }
    }
}