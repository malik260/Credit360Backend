using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EmailMessageLogger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            LoggerService myservice = new LoggerService();
            myservice.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new LoggerService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
