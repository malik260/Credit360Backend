using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private FinTrakBankingContext repo;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if( publicClientId==null) throw new ArgumentNullException("publicClientId");
            this.repo = new FinTrakBankingContext();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var origin = context.OwinContext.Request.Headers["Origin"];

            var exipredHr = int.Parse(ConfigurationManager.AppSettings["tokenExpiryHour"]);
            var userVM = new UserViewModel
            {
                password = context.Password.EncryptSha512(StaticHelpers.EncryptionKey),
                username = context.UserName
            };

            var _authRepo = new AuthenticationRepository(repo);

            var isUserAccountValid = Task.FromResult(_authRepo.IsUserAccountValid(userVM.username)).Result;
            if (isUserAccountValid)
            {
                var user = Task.FromResult(_authRepo.FindUserByUserNameAndPassword(userVM.username, userVM.password))
                    .Result;
                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

                var currIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
                var currUser = user;

                currIdentity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                currIdentity.AddClaim(new Claim("username", currUser.username));
                currIdentity.AddClaim(new Claim("companyId", currUser.companyId.ToString()));
                currIdentity.AddClaim(new Claim("staffId", currUser.staffId.ToString()));
                currIdentity.AddClaim(new Claim("branchId", currUser.branchId.ToString()));
                currIdentity.AddClaim(new Claim("countryId", currUser.countryId.ToString()));
                currIdentity.AddClaim(new Claim("userId", currUser.user_id.ToString()));

                var today = DateTime.Now;
                TimeSpan duration = new TimeSpan(exipredHr, 0, 0);

                var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                        "expiry_date", today.Add(duration).ToString()
                    }
                });

                var ticket = new AuthenticationTicket(currIdentity, props);

                context.Validated(ticket);

                context.Request.Context.Authentication.SignIn(currIdentity);
            }
            else
            {
                context.SetError("unauthorized_access", "Access denied: Please contact your administrator");
                return;
            }


            await Task.CompletedTask;
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }
}