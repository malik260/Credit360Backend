using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Topshelf;
using Topshelf.Ninject; 

namespace ExtractionService
{
    using ExtractionService.Modules.Modules;
    using Modules;
    using Services;
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run
           (
               c =>
               {
                    // load the DI bindings from the Module class
                    // which is a Ninject module; can take multiple
                    // modules
                    c.UseNinject(new Module());
                   
                   c.Service<Service>
                   (
                       sc =>
                       {
                            // inject the objects bound from
                            // our Ninject module(s) above
                            // into our Service class
                            sc.ConstructUsingNinject();

                           sc.WhenStarted
                           (
                               (service, hostControl) =>
                               service.Start(hostControl)
                           );
                           sc.WhenStopped
                           (
                               (service, hostControl) =>
                               service.Stop(hostControl)
                           );
                       }
                   );

                    c.SetServiceName("ExtractionProcessorSvc");
                   c.SetDisplayName("Extraction Processor");
                    c.SetDescription("Extraction running background");

                  // c.EnablePauseAndContinue();
                  //// c.EnableShutdown();

                  // c.StartAutomaticallyDelayed();
                  // c.RunAsLocalSystem();

                  // c.DependsOnEventLog();
                  // c.DependsOnMsSql();
                  // c.DependsOnIis();
               }
           );
           var exit = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());   
           Environment.ExitCode = exit;
            //return (int)exitCode;
        }
    }
}
