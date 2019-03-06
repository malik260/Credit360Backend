using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Web;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using Hangfire;
using Hangfire.Oracle.Core;
using Microsoft.Owin;
using Ninject;
using Oracle.ManagedDataAccess.Client;
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
                       // SchemaName = "HANGFIRE"
                    }));
           // var options = new DashboardOptions { AppPath = VirtualPathUtility.ToAbsolute("/url")}
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

          //  RecurringJob.AddOrUpdate(() => Console.WriteLine("Recuring Job"), Cron.Minutely);


        }
    }
}
