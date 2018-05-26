namespace FintrakBanking.ExtractionService
{
    using System;
using System.Threading;
using System.Timers;
using Topshelf;
using Timer = System.Timers.Timer;



    public class WindowService : ServiceControl
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        //private IEmailSender emailSender;

        //public WindowService(IEmailSender _emailSender)
        //{
        //    emailSender = _emailSender;
        //}
        public bool Start(HostControl hostControl)
        {
            _syncTimer = new Timer();
            _syncTimer.Interval = 5000;
            _syncTimer.Enabled = true;
            _syncTimer.Elapsed += RunJob;

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
                  //  emailSender.SendMail();
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
                   // emailSender.SendEmailCompleted();
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
