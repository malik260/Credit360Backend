using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;


namespace FinTrakMail
{
    partial class MailServer : ServiceBase
    {
        EmailSender mailsender = new EmailSender();

        Timer timer = new Timer();

        private bool IsBusy = false;

        public MailServer()
        {
            InitializeComponent();
        }

        public void OnDebug()
        { OnStart(null); }

        protected override void OnStart(string[] args)
        {
            string Congifinterval = ConfigurationManager.AppSettings["EmailServiceInterval"];
            int interval = 0;

            
            try
            {
                if (!String.IsNullOrEmpty(Congifinterval))
                {
                    interval = Convert.ToInt32(Congifinterval);

                    timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                    timer.Interval = (interval * 1000);
                    timer.Enabled = true;
                    timer.AutoReset = true;

                    if (timer.Interval > 0) { timer.Start(); } else { timer.Stop(); }
                }
            }
            catch (Exception ex)
            {
                AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Service" + " " + ex.Message + " - "  + DateTime.Now.ToString());
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.IsBusy == false)
            {
                SendMail();
            }
        }


        private void  SendMail()
        {
            this.IsBusy = true;
            try
            {
              //  timer.Stop();

                AuditTrail.LogFileManager.LogToFile("Email Service Started Successfully" + DateTime.Now.ToString());

                bool  sent = mailsender.SendMail();

                this.IsBusy = false;

                if (sent)
                {
                    AuditTrail.LogFileManager.LogToFile("Email Service Ended Successfully" + DateTime.Now.ToString());
                    return;
                }
                else

                AuditTrail.LogFileManager.LogToFile("Email Service Failed" + DateTime.Now.ToString());


            }
            catch (Exception ex)
            {
                AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Service" + " " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        protected override void OnStop()
        {
            AuditTrail.LogFileManager.LogToFile("Email Service stopped"+ DateTime.Now.ToString());
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
        }
    }
}
