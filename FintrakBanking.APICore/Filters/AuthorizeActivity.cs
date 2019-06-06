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

namespace FintrakBanking.APICore.Filters
{
    public class AuthorizeActivity : AuthorizeAttribute
    {
        private readonly string[] allowedActivities;
        private FinTrakBankingContext context = new FinTrakBankingContext();

        public AuthorizeActivity(params string[] activities)
        {
            this.allowedActivities = activities;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool authorize = false;
            int userId = Int32.Parse(new ClaimsIdentity(HttpContext.Current.User.Identity).Claims.First(x => x.Type == "userId").Value);
            var admin = new AuthenticationRepository(context, null,null);
            List<String> activities = admin.GetUserActivitiesByUser(userId);
            authorize = allowedActivities.Any(x => activities.Contains(x));
            return authorize;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext filterContext)
        {
            filterContext.Response = new HttpResponseMessage();
            filterContext.Response.StatusCode = HttpStatusCode.Forbidden;
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}