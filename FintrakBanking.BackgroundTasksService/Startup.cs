using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Oracle.Core;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(FintrakBanking.BackgroundTasksService.Startup))]

namespace FintrakBanking.BackgroundTasksService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["FinTrakBankingContext"].ToString();
            GlobalConfiguration.Configuration.UseStorage(
                new OracleStorage(
                    connectionString,
                    new OracleStorageOptions
                    {
                        TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = false,
                        DashboardJobListLimit = 50000,
                        TransactionTimeout = TimeSpan.FromMinutes(1),
                        SchemaName = "HANGFIRE"
                    }));
            app.UseHangfireDashboard();
            app.UseHangfireServer();

        }
    }
}
