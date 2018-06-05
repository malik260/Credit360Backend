using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Repositories.AlertMonitoring;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.MonitoringMessagesSender
{
    public class NinjectBinding : NinjectModule
    {
        public override void Load()
        {
            Bind<IEmailSender>().To<EmailSender>().InSingletonScope();
            Bind<IAlertMessagesEngine>().To<AlertMessagesEngine>().InSingletonScope();
            Bind<IAlertMessageLogger>().To<AlertMessageLogger>().InSingletonScope();
            Bind<ISLANotification>().To<SLANotification>().InSingletonScope();
        }
    }
}
