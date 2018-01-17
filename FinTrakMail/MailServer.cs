using System;
using System.ServiceProcess;
using System.Timers;


namespace FinTrakMail
{
    partial class MailServer : ServiceBase
    {
        BusLogic mailsender = new BusLogic();
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
            try
            {
               // Esetup = mailsender.GetEmailSetup();
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Interval = 10000;
                timer.Enabled = true;
                timer.AutoReset = true;
                timer.Start();
            }
            catch (Exception ex)
            {
                AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Service" + " " + ex.Message + " - "  + DateTime.Now.ToString());
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.IsBusy)
            {
                SendMail();
            }
        }


        private void  SendMail()
        {
            this.IsBusy = true;
            try
            {
               // timer.Stop();

                AuditTrail.LogFileManager.LogToFile("Email Service Started Successfully" + DateTime.Now.ToString());

              bool  sent = mailsender.SendMail();

                //this.IsBusy = false;

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
