using FintrakBanking.Interfaces.Setups.General;
using Ninject;
using NLog;
using System;
using System.Reflection;
using Topshelf;
using Topshelf.Logging;
using Topshelf.Ninject;

namespace FintrakBanking.MessagingAlertSender
{
    class Program
    {
        static void Main(string[] args)
        {
            //Logger _log = LogManager.GetCurrentClassLogger();
            //var kernel = new StandardKernel();
            //kernel.Load(Assembly.GetExecutingAssembly());

            //_log.Info("");
            //_log.Info("==================================================================");
            //_log.Info("About starting email sender : " + DateTime.Now);

            //EmailSender emailSender = new EmailSender();
            //emailSender.SendEmails();

            //_log.Info("");
            //_log.Info("==================================================================");
            //_log.Info("Started email sender : " + DateTime.Now);


            HostFactory.Run(serviceConfig =>
                {
                    serviceConfig.UseNinject(new NinjectBinding());
                    // serviceConfig.UseNLog();

                    serviceConfig.Service<WindowService>(serviceInstance =>
                    {
                        serviceInstance.ConstructUsingNinject();
                        serviceInstance.WhenStarted((service, hostControl) => service.Start(hostControl));
                        serviceInstance.WhenStopped((service, hostControl) => service.Stop(hostControl));
                    });


                    serviceConfig.RunAsLocalSystem();

                    serviceConfig.SetDescription("Fintrak Credit 360 General Email Alert Sender");
                    serviceConfig.SetDisplayName("Fintrak Credit 360 Email Sender");
                    serviceConfig.SetServiceName("FintrakCredit360EmailSender");
                });

        }
    }
}
