using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private FinTrakBankingContext _bankingContext;
        private TBL_SETUP_GLOBAL appSetup;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null) throw new ArgumentNullException("publicClientId");
            this._bankingContext = new FinTrakBankingContext();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var origin = context.OwinContext.Request.Headers["Origin"];

            UserViewModel user = null;

            var exipredHr = int.Parse(ConfigurationManager.AppSettings["tokenExpiryHour"]);
            var userVM = new UserViewModel
            {
                password = context.Password.EncryptSha512(StaticHelpers.EncryptionKey),
                username = context.UserName
            };

            ClaimsIdentity identity;
            var _authRepo = new AuthenticationRepository(_bankingContext);

            appSetup = _bankingContext.TBL_SETUP_GLOBAL.SingleOrDefault();

            if (appSetup.USE_ACTIVE_DIRECTORY)
            {
                if (Task.FromResult(ValidateActiveDirectoryCredentials(context.UserName, context.Password, out identity)).Result)
                {
                    user = Task.FromResult(_authRepo.FindUserByUserName(userVM.username)).Result;                   
                }
                else
                {
                    context.SetError("invalid_grant", "The user name is not registered in the application. Contact the system administrator.");
                    return;
                }
            }
            else
            {
                user = Task.FromResult(_authRepo.FindUserByUserNameAndPassword(userVM.username, userVM.password))
                   .Result;
                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }

            bool isUserAccountValid;

            if (Task.FromResult(_authRepo.IsUserAccountValid(userVM.username)).Result)
            {
                isUserAccountValid = true;
            }
            else
            {
                isUserAccountValid = false;
            }

            if (isUserAccountValid)
            {
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
                        "expiry_date", today.Add(duration).ToString("ddd MMM dd yyyy HH':'mm':'ss 'GMT'K")
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


        public bool ValidateActiveDirectoryCredentials(string userName, string password, out ClaimsIdentity identity)
        {
            appSetup = _bankingContext.TBL_SETUP_GLOBAL.FirstOrDefault();

            if (appSetup.REQUIRE_ADUSER == true)
            {
                using (var pc = new PrincipalContext(ContextType.Domain, appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, appSetup.ACTIVE_DIRECTORY_USERNAME, appSetup.ACTIVE_DIRECTORY_PASSWORD))
                {
                    bool isValid = pc.ValidateCredentials(userName, password);
                    if (isValid)
                    {
                        identity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
                        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

                    }
                    else
                    {
                        identity = null;
                    }

                    return isValid;
                }
            }
            else
            {
                using (var pc = new PrincipalContext(ContextType.Domain, appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME))
                {
                    bool isValid = pc.ValidateCredentials(userName, password);
                    if (isValid)
                    {
                        identity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
                        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

                    }
                    else
                    {
                        identity = null;
                    }

                    return isValid;
                }
            }
        }


    }
}