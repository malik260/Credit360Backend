using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Repositories.AlertMonitoring;
using Ninject.Modules;

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
