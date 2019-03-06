using FintrakBanking.Interfaces.AlertMonitoring;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.BackgroundTasksService.Controllers
{
    public class ValuesController : ApiController
    {
        private IEmailSender emailSender;
        private IAlertMessageLogger alertMessageLogger;
        private ISLANotification sLANotification;
        private IAlertMessagesEngine alertMessagesEngine;

        public ValuesController(IEmailSender emailSender, IAlertMessageLogger alertMessageLogger, ISLANotification sLANotification, IAlertMessagesEngine alertMessagesEngine)
        {
            this.alertMessageLogger = alertMessageLogger;
            this.alertMessagesEngine = alertMessagesEngine;
            this.sLANotification = sLANotification;
            this.emailSender = emailSender;
        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            RecurringJob.AddOrUpdate(() => alertMessageLogger.LogSLAApprovalNotification(), Cron.Minutely);
            RecurringJob.AddOrUpdate(() => emailSender.LogMonitorringAlert(), Cron.Minutely);

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
