using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Timers;


namespace FinTrakMail
{
    partial class MailServer : ServiceBase
    {
        EmailSender mailsender = new EmailSender();

        Timer timer = new Timer();

        private bool IsBusy = false;
        private string Congifinterval = "";
        private int interval = 0;
        System.Timers.Timer timeDelay;
        int count;
        public MailServer()
        {
            InitializeComponent();
             Congifinterval = ConfigurationManager.AppSettings["EmailServiceInterval"];

            timeDelay = new System.Timers.Timer();
            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

        }
        
        public void OnDebug()
        { OnStart(null); }

        protected override void OnStart(string[] args)
        {
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
              //  AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Service" + " " + ex.Message + " - " + DateTime.Now.ToString());
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

               // AuditTrail.LogFileManager.LogToFile("Email Service Started Successfully at : " + DateTime.Now.ToString());

                bool  sent = mailsender.SendMail();

                this.IsBusy = false;

                if (sent)
                {
                    //  AuditTrail.LogFileManager.LogToFile("Email Service Ended Successfully at :  " + DateTime.Now.ToString());
                    return;
                }
                else { }

              //  AuditTrail.LogFileManager.LogToFile("Email Service Failed at :  " + DateTime.Now.ToString());


            }
            catch (Exception ex)
            {
              //  AuditTrail.LogFileManager.LogToFile("Error Occurred: Email Service : " + " " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
            }
            finally
            {
                this.IsBusy = false;
            }
            this.IsBusy = false;
        }

        protected override void OnStop()
        {
           // AuditTrail.LogFileManager.LogToFile("Email Service stopped at : "+ DateTime.Now.ToString());
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
        }

        public static void Log(string str)
        {

            StreamWriter fileWritter = File.AppendText(@"d:\Log.txt");
            fileWritter.WriteLine(DateTime.Now.ToString() + " " + str);
            fileWritter.Close();
        }
    }
}
