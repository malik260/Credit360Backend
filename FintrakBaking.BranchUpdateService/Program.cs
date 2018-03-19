using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBaking.BranchUpdateService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            NewBranchService checkForNewBranch = new NewBranchService();
            checkForNewBranch.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new NewBranchService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
