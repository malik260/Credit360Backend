using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Web.Http;
using Hangfire;
using Hangfire.Oracle.Core;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(FintrakBanking.BackgroundTasksService.App_Start.Startup))]

namespace FintrakBanking.BackgroundTasksService.App_Start
{
    public class Startup
    {
        string connectionString = "";
        public void Configuration(IAppBuilder app)
        {
            Hangfire.GlobalConfiguration.Configuration.UseStorage(
    new OracleStorage(
        connectionString,
        new OracleStorageOptions
        {
            TransactionIsolationLevel = IsolationLevel.ReadCommitted,
            QueuePollInterval = TimeSpan.FromSeconds(15),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true,
            DashboardJobListLimit = 50000,
            TransactionTimeout = TimeSpan.FromMinutes(1),
            SchemaName = "HANGFIRE"
        }));
        }
    }
}
