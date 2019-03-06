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

            //   RecurringJob.AddOrUpdate(() => Console.WriteLine("Recuring Job"), Cron.Minutely);
            //BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));
            //  BackgroundJob.Schedule(() => Console.WriteLine("Hello, world"), TimeSpan.FromDays(1));

            var alertSetups = alertMessageLogger.getAlertMessageSetting();


            RecurringJob.AddOrUpdate(() => emailSender.SendEmails(), Cron.Minutely);


            BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsForCovenantsApproachingDueDate(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsForCovenantsOverDue(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsForExpiredBG(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertForExpiredInsurance(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertOnAccountWithExeption_Overdrawn(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertOnAccountWithExeption_Watchist(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertOnAccountWithExeption_Unauthorized(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertOnInsuranceApprochingExpiration(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertOnPastDueObligationAccounts(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertOnTurnoverCovenant(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsForCollateralPropertyApproachingRevaluation(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsForCollateralPropertyDueForVisitation(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsOnExpiredActiveBondAndGuarantee(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsOnInActiveBondAndGuarantee(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsOnLoanCASAwithPND(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsOnOverDraftLoansAlmostDue(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsOnSelfLiquidatingLoanExpiry(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertsForLoanRepayment(title, body, alertSetups), TimeSpan.FromDays(1));
           BackgroundJob.Schedule(() => alertMessageLogger.SendAlertToCustomerForLoanRepaymentApproachingDueDate(title, body, alertSetups), TimeSpan.FromDays(1));

        }
    }
}
