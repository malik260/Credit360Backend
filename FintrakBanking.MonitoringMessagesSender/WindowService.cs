using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.AlertMonitoring;
using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Threading;
using System.Timers;
using Topshelf;
using Topshelf.Logging;
using System.Linq;

using Timer = System.Timers.Timer;
using FintrakBanking.Common.AlertMonitoring;
using System.Collections.Generic;

namespace FintrakBanking.MonitoringMessagesSender
{
    public class WindowService : ServiceControl
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        private IEmailSender emailSender;
        private string interval = ConfigurationManager.AppSettings["emailServiceInterval"];
        private string slaEscalationIntervalInHours = ConfigurationManager.AppSettings["SLAEscalationIntervalInHours"];
        private string alertMessageLoggertime = ConfigurationManager.AppSettings["alertMessageLoggingTime"];
        private static readonly LogWriter _log = HostLogger.Get<WindowService>();
        private IAlertMessageLogger logger;
        private FinTrakBankingContext context;
        private IAlertMessagesEngine alertMessagesEngine;
        public WindowService(IEmailSender _emailSender, IAlertMessageLogger _logger, FinTrakBankingContext context, IAlertMessagesEngine alertMessagesEngine)
        {
            emailSender = _emailSender;
            logger = _logger;
            this.context = context;
            this.alertMessagesEngine = alertMessagesEngine;
        }
        public bool Start(HostControl hostControl)
        {
            if (interval != null)
            {
                _log.ErrorFormat("");
                _log.ErrorFormat("==================================================================");
                _log.ErrorFormat("Email Sender has started successfully at : " + DateTime.Now);

                int timeInterval = Convert.ToInt32(interval);
                _syncTimer = new Timer();
                _syncTimer.Interval = (timeInterval * 5000);
                _syncTimer.Enabled = true;
                _syncTimer.Elapsed += RunJob;
            }
            else
                return false;
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _syncTimer.Elapsed += StopJob;
            _syncTimer.Enabled = false;
            return true;
        }
        private void RunJob(object state, ElapsedEventArgs elapsedEventArgs)
        {
            var now = DateTime.Now;

            var tasks = context.TBL_SCHEDULER.Where(x => x.ENABLED == true)
                  .Select(x => x).ToList();

            if (tasks.Count() < 1) return;

            foreach (var task in tasks)
            {
                if (task.FREQUENCYTYPEID == (int)SchudulerFrequencyEnum.Daily)
                {
                    var nextRun = task.NEXTRUNDATETIME.Date;
                    var currenDate = now.Date;

                    if (task.NEXTRUNDATETIME.Date == now.Date)
                    {
                        if (task.NEXTRUNDATETIME.TimeOfDay >= now.TimeOfDay)
                        {
                            var newTask = context.TBL_MONITORING_ALERT_SETUP.Where(o => o.MONITORING_ITEMID == task.MONITORING_ITEMID).Select(o => o).ToList();

                            if (newTask.Count() < 1) continue;
                                LogEmail(newTask);
                        }
                    }
                }

                if (task.FREQUENCYTYPEID == (int)SchudulerFrequencyEnum.Monthly)
                {
                }

                if (task.FREQUENCYTYPEID == (int)SchudulerFrequencyEnum.Yearly)
                {
                }


            }
        }

        private void LogEmail(List<TBL_MONITORING_ALERT_SETUP> task)
        {

            //log alart
            foreach (var alart in task)
            {
                alertMessagesEngine.Start(alart.MONITORING_ITEMID);

                var updateNextRun = context.TBL_SCHEDULER.Where(o => o.MONITORING_ITEMID == alart.MONITORING_ITEMID).Select(o => o).FirstOrDefault();
                if (updateNextRun == null) continue;

                updateNextRun.LASTRUNDATETIME = updateNextRun.NEXTRUNDATETIME;
                updateNextRun.NEXTRUNDATETIME = updateNextRun.NEXTRUNDATETIME.AddDays(1);

            }

            context.SaveChanges();
        }

        private void StopJob(object state, ElapsedEventArgs elapsedEventArgs)
        {
            //Prevents the job firing until it finishes its job
            if (Monitor.TryEnter(s_lock))
            {
                try
                {
                  //  emailSender.SendEmailCompleted();
                }
                finally
                {
                    //unlock the job
                    Monitor.Exit(s_lock);
                }
            }
        }
    }
}
