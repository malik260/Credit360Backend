using FintrakBanking.Interfaces.AlertMonitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace FintrakBanking.MonitoringMessageLogger
{
    partial class ServiceEngine : ServiceBase
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        //private IEmailSender emailSender;
        private string interval = ConfigurationManager.AppSettings["emailServiceInterval"];
        private string slaEscalationIntervalInHours = ConfigurationManager.AppSettings["SLAEscalationIntervalInHours"];
        private string alertMessageLoggertime = ConfigurationManager.AppSettings["alertMessageLoggingTime"];
        private static readonly LogWriter _log = HostLogger.Get<WindowService>();
        private IAlertMessageLogger logger;
        private ICurrencyAndRateUpdate currencyAndRateUpdate;
        public ServiceEngine(IEmailSender _emailSender, IAlertMessageLogger _logger, ICurrencyAndRateUpdate _currencyAndRateUpdate)
        {
            //emailSender = _emailSender;
            logger = _logger;
            currencyAndRateUpdate = _currencyAndRateUpdate;

            InitializeComponent();
        }

        public ServiceEngine()
        {

        }


        protected override void OnStart(string[] args)
        {
            System.Diagnostics.Debugger.Launch();

            if (Monitor.TryEnter(s_lock))
            {
                try
                {


                    // SEND EMAILS
                    bool response = true; // emailSender.SendEmails();
                    if (response == true)
                    {
                        _log.Info("");
                        _log.Info("==================================================================");
                        _log.Info("Emails has been sent successfully and ends at : " + DateTime.Now);
                    }
                    else
                    {
                        _log.Info("");
                        _log.Info("==================================================================");
                        _log.Info("No Message Logger has been sent as at : " + DateTime.Now);
                    }


                    //LOG SLA APPROVAL NOTIFICATIONS
                    if (slaEscalationIntervalInHours != null)
                    {
                        DateTime currentDate = DateTime.Now;
                        TimeSpan escalationTime = currentDate.AddHours(Convert.ToInt32(slaEscalationIntervalInHours)).TimeOfDay;
                        TimeSpan endOfescalationTime = DateTime.Now.AddMinutes(5).TimeOfDay;
                        TimeSpan timeAtTheMoment = DateTime.Now.TimeOfDay;

                        if (escalationTime >= timeAtTheMoment && escalationTime <= endOfescalationTime)
                        {
                            _log.Info("");
                            _log.Info("==================================================================");
                            _log.Info("SLA notification has started successfully at : " + DateTime.Now);

                            logger.LogSLAApprovalNotification();

                            _log.Info("");
                            _log.Info("==================================================================");
                            _log.Info("SLA notification has ends at : " + DateTime.Now);
                        }

                    }

                    // LOG MONITORING ALERTS
                    TimeSpan currentTime = DateTime.Now.TimeOfDay;
                    TimeSpan LoggeingTimeFromConfig = Convert.ToDateTime(alertMessageLoggertime).TimeOfDay;

                    TimeSpan alertLoggerMaxRuntime = TimeSpan.FromMinutes(30);
                    TimeSpan LoggeingTimeFromConfigExtended = LoggeingTimeFromConfig.Add(alertLoggerMaxRuntime);


                    if (currentTime >= LoggeingTimeFromConfig && currentTime <= LoggeingTimeFromConfigExtended)
                    {
                        //  _log.Info("##############   started at " + currentTime + "     ##################### ");
                        _log.Info("==================================================================");
                        _log.Info("Monitoring alert has started successfully");

                       // emailSender.LogMonitorringAlert();
                        currencyAndRateUpdate.MigrateExchangeRate();

                        _log.Info("");
                        _log.Info("==================================================================");
                        _log.Info("Monitoring alert has finished logging successfully ");
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
                    _log.ErrorFormat("Message Logger has failed with error : " + ex + " at : " + DateTime.Now);
                    _log.ErrorFormat("");
                    _log.ErrorFormat("==================================================================");
                    if (ex.InnerException != null)
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

                    //emailSender.SendEmailOfException(ex.ToString());
                }

                finally
                {
                    //unlock the job
                    Monitor.Exit(s_lock);
                }
            }
        }

        protected override void OnStop()
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
