using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Repositories.AlertMonitoring;
using Hangfire;
using Hangfire.Oracle.Core;
using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;
using System.Data;

[assembly: OwinStartup(typeof(FintrakBanking.BackgroundTasksService.Startup))]

namespace FintrakBanking.BackgroundTasksService
{
    public class Startup
    {
        SLANotification sLANotification = new SLANotification();
        AlertMessageLogger alertMessageLogger = new AlertMessageLogger();
        EmailSender emailSender = new EmailSender();
        ExchangeRate exchangeRate = new ExchangeRate();
        string title = string.Empty;
        string body = string.Empty;

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
                        JobExpirationCheckInterval = TimeSpan.FromHours(12),
                        CountersAggregateInterval = TimeSpan.FromMinutes(10),
                        PrepareSchemaIfNecessary = false,
                        //DashboardJobListLimit = 50000,
                        TransactionTimeout = TimeSpan.FromMinutes(10),
                        // SchemaName = "HANGFIRE"
                    }));
            // var options = new DashboardOptions { AppPath = VirtualPathUtility.ToAbsolute("/url")}
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

            //   RecurringJob.AddOrUpdate(() => Console.WriteLine("Recuring Job"), Cron.Minutely);
            //BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));
            //  BackgroundJob.Schedule(() => Console.WriteLine("Hello, world"), TimeSpan.FromDays(1));


            RecurringJob.AddOrUpdate(() => emailSender.SendEmails(), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => exchangeRate.MigrateExchangeRate(), Cron.Minutely);

            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsForCovenantsApproachingDueDate(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsForCovenantsOverDue(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsForExpiredBG(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertForExpiredInsurance(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertOnAccountWithExeption_Overdrawn(title, body), Cron.Minutely);
            // RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertOnAccountWithExeption_Watchist(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertOnAccountWithExeption_Unauthorized(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertOnInsuranceApprochingExpiration(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertOnPastDueObligationAccounts(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertOnTurnoverCovenant(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsForCollateralPropertyApproachingRevaluation(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsForCollateralPropertyDueForVisitation(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsOnExpiredActiveBondAndGuarantee(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsOnInActiveBondAndGuarantee(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsOnLoanCASAwithPND(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsOnOverDraftLoansAlmostDue(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsOnSelfLiquidatingLoanExpiry(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertsForLoanRepayment(title, body), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => alertMessageLogger.SendAlertToCustomerForLoanRepaymentApproachingDueDate(title, body), Cron.Minutely);

        }
    }
}
