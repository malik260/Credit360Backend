using System;
using System.Threading;
using System.Timers;
using System.Text;
//using Topshelf;
//using Topshelf.ServiceConfigurators;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using DataExtractionService.Interface;

namespace DataExtractionService
{
    public class TopShelfWindowsService 
    {
        private Timer _syncTimer;
        private static object s_lock = new object();
        private ITreasuaryRateExtraction rate;

        public TopShelfWindowsService( )
        {
           // this.rate = rate;
        }


        /// <summary>
        /// Starts the windows service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns></returns>
        public bool Start(HostControl hostControl)
        {
            _syncTimer = new Timer();
            _syncTimer.Interval = 5000;
            _syncTimer.Enabled = true;
            _syncTimer.Elapsed += RunJob;

            return true;
        }

        /// <summary>
        /// Stops the windows service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns></returns>
        public bool Stop(HostControl hostControl)
        {
            _syncTimer.Enabled = false;
            return true;
        }

        /// <summary>
        /// Job runner event, with lock if the job still running
        /// </summary>
        /// <param name="state"></param>
        /// <param name="elapsedEventArgs"></param>
        private void RunJob(object state, ElapsedEventArgs elapsedEventArgs)
        {
            //Prevents the job firing until it finishes its job
            if (Monitor.TryEnter(s_lock))
            {
                try
                {
                    rate.MigrateExchangeRate();
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
