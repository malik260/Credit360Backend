using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Repositories.AlertMonitoring;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Ninject;

namespace FintrakBanking.MonitoringMessagesSender
{
    class Program
    {
        static void Main(string[] args)
        {

            HostFactory.Run(serviceConfig =>
            {
               serviceConfig.UseNinject(new NinjectBindings());

                serviceConfig.Service<WindowService>(serviceInstance =>
                {
                    serviceInstance.ConstructUsingNinject();
                    serviceInstance.WhenStarted((service, hostControl) => service.Start(hostControl));
                    serviceInstance.WhenStopped((service, hostControl) => service.Stop(hostControl));
                });


                serviceConfig.RunAsLocalSystem();                                     

                serviceConfig.SetDescription("Fintrak Credit 360 General Email Alert Monitoring Sender");                 
                serviceConfig.SetDisplayName("Fintrak Credit 360 Email Sender");                                
                serviceConfig.SetServiceName("FintrakCredit360EmailSender");                                
            });
        }                                                                                         
    }
}
