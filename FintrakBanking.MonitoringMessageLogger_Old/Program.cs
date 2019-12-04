using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Ninject;

namespace FintrakBanking.MonitoringMessageLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.UseNinject(new NinjectBindings());
                serviceConfig.UseNLog();

                serviceConfig.Service<WindowService>(serviceInstance =>
                {
                    serviceInstance.ConstructUsingNinject();
                    serviceInstance.WhenStarted((service, hostControl) => service.Start(hostControl));
                    serviceInstance.WhenStopped((service, hostControl) => service.Stop(hostControl));
                });


                serviceConfig.RunAsLocalSystem();

                serviceConfig.SetDescription("Fintrak Credit 360 Email Alert Monitoring Message Logger");
                serviceConfig.SetDisplayName("Fintrak Credit 360 Alert Message Logger");
                serviceConfig.SetServiceName("FintrakCredit360AlertMessageLogger");
            });
        }
    }
}
