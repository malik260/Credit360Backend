using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories.AlertMonitoring;
using Ninject;
using System;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Reflection;
using System.Threading;
using System.Timers;
using Topshelf;
using Topshelf.Logging;
using Timer = System.Timers.Timer;


namespace FintrakBanking.MonitoringMessageLogger
{
    public class WindowService : ServiceControl
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        EmailSender emailSender = new EmailSender();
        private string interval = ConfigurationManager.AppSettings["emailServiceInterval"];
        private string slaEscalationIntervalInHours = ConfigurationManager.AppSettings["SLAEscalationIntervalInHours"];
       
        private string alertMessageLoggertime = ConfigurationManager.AppSettings["alertMessageLoggingTime"];
        private static readonly LogWriter _log = HostLogger.Get<WindowService>();
        AlertMessageLogger logger = new AlertMessageLogger();
        
        public WindowService()
        {
        }
        public bool Start(HostControl hostControl)
        {
            if (interval != null)
            {
                _log.ErrorFormat("");
                _log.ErrorFormat("==================================================================");
                _log.ErrorFormat("Message Logger has started successfully at : " + DateTime.Now);

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
                    var kernel = new StandardKernel();
                    kernel.Load(Assembly.GetExecutingAssembly());
                    var alert = kernel.Get<IAlertRepository>();
                    bool response = alert.validateAlertCheck();

                    if (response == true)
                        {
                            _log.Info("");
                            _log.Info("==================================================================");
                            _log.Info("Emails has been logged successfully and ends at : " + DateTime.Now);
                        }
                        else
                        {
                            _log.Info("");
                            _log.Info("==================================================================");
                            _log.Info("No email has been logged as at : " + DateTime.Now);
                        }

                    }
                catch (DbEntityValidationException ee)
                {
                    foreach (var error in ee.EntityValidationErrors)
                    {
                        foreach (var thisError in error.ValidationErrors)
                        {
                            Console.WriteLine("DbEntityValidationException   :   " + thisError.ErrorMessage);

                            _log.ErrorFormat("DbEntityValidationException   :    " + thisError.ErrorMessage);
                            _log.ErrorFormat("");
                            _log.ErrorFormat("==================================================================");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    _log.ErrorFormat("Message Logger has failed with error : " + ex.ToString() + " at : "  + DateTime.Now);
                    _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    if (ex.InnerException!=null)
                    {
                        _log.ErrorFormat("InnerException  :  " + ex.InnerException);
                        _log.ErrorFormat("");
                        _log.ErrorFormat("==================================================================");

                        Console.WriteLine(ex.InnerException);

                    }
                    _log.ErrorFormat("ex.Message   :    " + ex.Message);
                    _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    Console.WriteLine(ex.Message);

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
                   //emailSender.SendEmailCompleted();
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
