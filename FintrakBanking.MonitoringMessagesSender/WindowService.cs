using FintrakBanking.Interfaces.AlertMonitoring;
using System;
using System.Configuration;
using System.Threading;
using System.Timers;
using Topshelf;
using Topshelf.Logging;
using Timer = System.Timers.Timer;


namespace FintrakBanking.MonitoringMessagesSender
{
    public class WindowService : ServiceControl
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        private IEmailSender emailSender;
        private string interval = ConfigurationManager.AppSettings["emailServiceInterval"];
        private static readonly LogWriter _log = HostLogger.Get<WindowService>();

        public WindowService(IEmailSender _emailSender)
        {
            emailSender = _emailSender;
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


            //Prevents the job firing until it finishes its job
            if (Monitor.TryEnter(s_lock))
            {
                try
                {
                    bool response = emailSender.SendMail();

                    if (response == true)
                    {
                        _log.ErrorFormat("");
                        _log.ErrorFormat("==================================================================");
                        _log.ErrorFormat("Emails has been sent successfully and ends at : " + DateTime.Now);
                    }
                    else
                        _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat("No email has been sent as at : " + DateTime.Now);

                }catch(Exception ex)
                {
                    _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat("Email Sender has failed with error : " + ex + " at : "  + DateTime.Now);

                    emailSender.SendEmailOfException(ex.ToString());
                }
                finally
                {
                    //unlock the job
                    Monitor.Exit(s_lock);
                }
            }

        }
        private void StopJob(object state, ElapsedEventArgs elapsedEventArgs)
        {
            //Prevents the job firing until it finishes its job
            if (Monitor.TryEnter(s_lock))
            {
                try
                {
                    emailSender.SendEmailCompleted();
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
