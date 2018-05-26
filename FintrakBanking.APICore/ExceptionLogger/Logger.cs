using Serilog;
using SerilogWeb.Classic.Enrichers;
using SerilogWeb.Classic.WebApi.Enrichers;
using System;
using System.Net.Http;

namespace FintrakBanking.APICore.ExceptionLogger
{
    namespace SeriLogger
    {
        public static class SeriLogger
        {
            public static void LogSetup()
            {

                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(@"C:\Logs\logfile-lastlog}.txt")
                    .Enrich.With<WebApiRouteTemplateEnricher>()
                    .Enrich.With<WebApiControllerNameEnricher>()
                    .Enrich.With<WebApiActionNameEnricher>()
                    .Enrich.With<HttpRequestIdEnricher>()
                    
                    .Enrich.With<HttpRequestClientHostNameEnricher>()
                    .CreateLogger();
            }


        }
    }


}
