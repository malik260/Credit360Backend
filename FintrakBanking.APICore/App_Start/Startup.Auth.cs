using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using FintrakBanking.APICore.Providers;
using System.Configuration;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;

namespace FintrakBanking.APICore
{
    public partial class Startup
    {
        public Startup()
        {

        }
        private readonly IGeneralSetupRepository _setup;
        private readonly IAuditTrailRepository _auditTrail;

        public Startup(IGeneralSetupRepository setup, IAuditTrailRepository auditTrail)
        {
            _setup = setup;
            _auditTrail = auditTrail;
        }
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

     

        public void ConfigureAuth(IAppBuilder app)
        {
            
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);


            app.Use(async (context, next) =>
            {
                IOwinRequest req = context.Request;
                IOwinResponse res = context.Response;

                if (req.Path.StartsWithSegments(new PathString("/Token")))
                {
                    var origin = req.Headers.Get("Origin");

                    if (!string.IsNullOrEmpty(origin))
                    {
                        res.Headers.Set("Access-Control-Allow-Origin", "*");
                        res.Headers.Set("Access-Control-Allow-Origin", origin);

                    }

                    if (req.Method == "OPTIONS")
                    {
                        res.StatusCode = 200;
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Methods", "GET", "POST");
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Headers", "authorization", "content-type", "X-COUNTRYCODE", "X-LANG");

                        return;

                    }

                }

                await next();

            });


            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
             
            var exipredMin = int.Parse(ConfigurationManager.AppSettings["tokenExpiryMinute"]);
          
            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                // AccessTokenExpireTimeSpan = TimeSpan.FromHours(exipredHr),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(exipredMin),
                AllowInsecureHttp = true,
                RefreshTokenProvider = new SimpleRefreshTokenProvider()
            };


            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }

    }
}
