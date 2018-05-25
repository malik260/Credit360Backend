using FinTrakMail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EmailMessageLogger
{
    public partial class LoggerService : ServiceBase
    {
        EmailMessageLogic emailMessageLogic = new EmailMessageLogic();

        Timer timer = new Timer();

        private bool IsBusy = false;

        public LoggerService()
        {
            InitializeComponent();
        }

        public void OnDebug()
        { OnStart(null); }

        protected override void OnStart(string[] args)
        {
            try
            {
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Interval = 10000;
                timer.Enabled = true;
                timer.AutoReset = true;
                timer.Start();
            }
            catch (Exception ex)
            {
              //  AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Message Logger Service" + " " + ex.Message + " - " + DateTime.Now.ToString());
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.IsBusy)
            {
                SendMail();
            }
        }
        private void SendMail()
        {
            this.IsBusy = true;
            try
            {
                //  timer.Stop();

             //   AuditTrail.LogFileManager.LogToFile("Email Message Logger Service Started Successfully" + DateTime.Now.ToString());

               string respose =  emailMessageLogic.Start();

              //  AuditTrail.LogFileManager.LogToFile("Email Message Logger Service ends with these responses : " + respose + " " + DateTime.Now.ToString());

                this.IsBusy = false;

                if (respose != "")
                {
                    //   AuditTrail.LogFileManager.LogToFile("Email Message Logger Service Ended Successfully" + DateTime.Now.ToString());
                    return;
                }
                else { }

                  //  AuditTrail.LogFileManager.LogToFile("Email Message Logger Service Failed" + DateTime.Now.ToString());


            }
            catch (Exception ex)
            {
              //  AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Message Logger Service" + " " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        protected override void OnStop()
        {
         //   AuditTrail.LogFileManager.LogToFile("Email Message Logger Service stopped" + DateTime.Now.ToString());
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
        }
    }
}
