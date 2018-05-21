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
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace FintrakBanking.APICore.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private FinTrakBankingContext _bankingContext;
        private TBL_SETUP_GLOBAL appSetup;
        private const string HttpContext = "MS_HttpContext";



        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null) throw new ArgumentNullException("publicClientId");
            this._bankingContext = new FinTrakBankingContext();
        }

       
        public string GetIpAddress(HttpRequestMessage request)
        {
            if (!request.Properties.ContainsKey(HttpContext)) return null;
            dynamic context = request.Properties[HttpContext];
            return context != null ? (string)context.Request.UserHostAddress : null;
        }

       // string IPAddress = 
        public string GetIpAddress()
        {
            string ipAddress = string.Empty;
            IPHostEntry Host = default(IPHostEntry);
            string hostname = null;
            hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(hostname);
            foreach (IPAddress ip in Host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = Convert.ToString(ip);
                }
            }
            return ipAddress;
        }
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var origin = context.OwinContext.Request.Headers["Origin"];
            try
            {

              string ipAddress = GetIpAddress(); 
            UserViewModel user = null;

            var exipredHr = int.Parse(ConfigurationManager.AppSettings["tokenExpiryHour"]);
            var exipredMin = int.Parse(ConfigurationManager.AppSettings["tokenExpiryMinute"]);
            var exipredSec  = int.Parse(ConfigurationManager.AppSettings["tokenExpirySecond"]);

            var userVM = new UserViewModel
            {
                password = context.Password.EncryptSha512(StaticHelpers.EncryptionKey),
                username = context.UserName
            };

            var authRepo = new AuthenticationRepository(_bankingContext);

            appSetup = _bankingContext.TBL_SETUP_GLOBAL.SingleOrDefault();

            ClaimsIdentity identity;

            if (appSetup != null && appSetup.USE_ACTIVE_DIRECTORY)
            {
                    var _user = _bankingContext.TBL_PROFILE_USER.Where(x => x.USERNAME.ToLower() == context.UserName.ToLower());

                    if(!_user .Any())
                    {
                        context.SetError("invalid_grant", "This user is not profiled on this application. Contact the system administrator");
                        return;
                    }


                    if (Task.FromResult(ValidateActiveDirectoryCredentials(context.UserName, context.Password, out identity)).Result)
                {
                    user = Task.FromResult(authRepo.FindUserByUserName(userVM.username)).Result;                   
                }
                else
                {

                    context.SetError("invalid_grant", "The user name or password is incorrect");
                    return;
                }
            }
            else
            {
                user = Task.FromResult(authRepo.FindUserByUserNameAndPassword(userVM.username, userVM.password, true ))
                   .Result;
                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }

            bool isUserAccountValid;

            if (Task.FromResult(authRepo.IsUserAccountValid(userVM.username)).Result)
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

                    currIdentity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                    currIdentity.AddClaim(new Claim("username", user.username));
                    currIdentity.AddClaim(new Claim("companyId", user.companyId.ToString()));
                    currIdentity.AddClaim(new Claim("staffId", user.staffId.ToString()));
                    currIdentity.AddClaim(new Claim("branchId", user.branchId.ToString()));
                    currIdentity.AddClaim(new Claim("countryId", user.countryId.ToString()));
                    currIdentity.AddClaim(new Claim("userId", user.user_id.ToString()));
                    currIdentity.AddClaim(new Claim("logincode", user.logincode == null ? Guid.NewGuid().ToString() : user.logincode));
                    var today = DateTime.Now;
                    TimeSpan duration = new TimeSpan(exipredHr, exipredMin, exipredSec);//(exipredHr, 0, 0);

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
                    context.SetError("unauthorized_access", "The user name or password is incorrect");
                    return;
                }


            await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                string innerMessage = "";

                if (ex.InnerException != null)
                    innerMessage = ex.InnerException.Message;

                context.SetError("invalid_grant", ex.Message + " - inner exception - " + innerMessage );

                //  context.SetError("invalid_grant", "The user name or password is incorrect.");
                //if (CommonHelpers.IsNumeric(CommonHelpers.Left(ex.Message, 4)))
                //{
                //    context.SetError("invalid_grant", ex.Message.Replace("1001", ""));
                //}

                //if (ex.Message.Contains("network-related"))
                //{
                //    context.SetError("invalid_grant", "Server error: Contact System Administrator");
                //}else
                //{
                //    context.SetError("invalid_grant", ex.Message);
                //    //  context.SetError("invalid_grant", "Server error: Contact System Administrator");
                //}
            }
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