using Serilog;
using SerilogWeb.Classic.Enrichers;
using SerilogWeb.Classic.WebApi.Enrichers;

namespace FintrakBanking.APICore.ExceptionLogger
{
    namespace SeriLogger
    {
        public static class SeriLogger
        {
            public static void LogSetup()
            {

                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(@"C:\Logs\logfile-{Date}.txt")
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
