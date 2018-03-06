using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Web.Services.Description;
using System.Net.Http;
using System.Net;

[assembly: OwinStartup(typeof(FintrakBanking.APICore.Startup))]

namespace FintrakBanking.APICore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        //public void ConfigureServices(ServiceCollection services)
        //{
        //    Uri endPointA = new Uri("http://localhost:58919/"); // this is the endpoint HttpClient will hit
        //    HttpClient httpClient = new HttpClient()
        //    {
        //        BaseAddress = endPointA,
        //    };

        //    ServicePointManager.FindServicePoint(endPointA).ConnectionLeaseTimeout = 60000; // sixty seconds

        //    services.AddSingleton<HttpClient>(httpClient); // note the singleton
        //}
    }
}
