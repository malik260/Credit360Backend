using FintrakBanking.Common.AlertMonitoring;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;
using Topshelf.Logging;
using Timer = System.Timers.Timer;

namespace FintrakBanking.LoanRepaymentBackgroundService
{
    public class ServiceLayer : ServiceControl
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        private string interval = ConfigurationManager.AppSettings["emailServiceInterval"];
        private string slaEscalationIntervalInHours = ConfigurationManager.AppSettings["SLAEscalationIntervalInHours"];
        private string alertMessageLoggertime = ConfigurationManager.AppSettings["alertMessageLoggingTime"];
        private static readonly LogWriter _log = HostLogger.Get<ServiceLayer>();
        private FinTrakBankingContext context;
        ILoanOperationsRepository loanOperationsRepository;
        public ServiceLayer(
            FinTrakBankingContext context,
            ILoanOperationsRepository loanOperationsRepository
            )
        {
            this.context = context;
            this.loanOperationsRepository = loanOperationsRepository;
        }
        public bool Start(HostControl hostControl)
        {
            if (interval != null)
            {
                try
                {

                    _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat("Loan repayment has started successfully at : " + DateTime.Now);

                    int timeInterval = Convert.ToInt32(interval);
                    _syncTimer = new Timer();
                    _syncTimer.Interval = timeInterval * 5000;
                    _syncTimer.Enabled = true;
                    _syncTimer.Elapsed += RunJob;

                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("Faled Error Log");
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat(ex.Message + "  ------    " + DateTime.Now);
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat(ex.InnerException + "  ------    " + DateTime.Now);
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat(ex.StackTrace + "  ------    " + DateTime.Now);
                    _log.ErrorFormat("==================================================================");
                }
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
            //var now = DateTime.Now;
            //// int[] taskIds = { (int)SchedulerItemEnum.LoanRepaymentFromStagging };

            //var tasks = context.TBL_SCHEDULER.Where(x => x.ENABLED == true && x.SCHEDULERITEMID == (int)SchedulerItemEnum.LoanRepaymentFromStagging)
            //      .Select(x => x).ToList();

            //if (tasks.Count() < 1) return;

            //foreach (var task in tasks)
            //{
            //    if (task.FREQUENCYTYPEID == (int)SchudulerFrequencyEnum.Daily)
            //    {
            //        var nextRun = task.NEXTRUNDATETIME.Date;
            //        var currenDate = now.Date;

            //        if (task.NEXTRUNDATETIME.Date == now.Date)
            //        {
            //            if (now.TimeOfDay >= task.NEXTRUNDATETIME.TimeOfDay)
            //            {
            //                RunRepayments(task.SCHEDULERITEMID);
            //            }
            //        }
            //    }

            //    if (task.FREQUENCYTYPEID == (int)SchudulerFrequencyEnum.Monthly)
            //    {
            //    }

            //    if (task.FREQUENCYTYPEID == (int)SchudulerFrequencyEnum.Yearly)
            //    {
            //    }


            //}

            //_log.ErrorFormat("");
            //_log.ErrorFormat("==================================================================");
            //_log.ErrorFormat("Loan repayment has ended successfully at : " + DateTime.Now);
        }

        private void RunRepayments(int? monitoringAlertId)
        {

            // log alart
            //var result = alertMessagesEngine.Start(monitoringAlertId);
            //var result = loanOperationsRepository.GetRepaymentFromStaging();
            //if (result == true)
            //{
            //    var updateNextRun = context.TBL_SCHEDULER.Where(o => o.FREQUENCYTYPEID == monitoringAlertId).Select(o => o).FirstOrDefault();
            //    if (updateNextRun == null)
            //    {
            //        updateNextRun.LASTRUNDATETIME = updateNextRun.NEXTRUNDATETIME;
            //        updateNextRun.NEXTRUNDATETIME = updateNextRun.NEXTRUNDATETIME.AddDays(1);
            //        context.SaveChanges();
            //    }
            //}
        }
        private void StopJob(object state, ElapsedEventArgs elapsedEventArgs)
        {

        }
    }
}
