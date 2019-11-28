using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Repositories.AlertMonitoring;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.MonitoringMessageLogger
{
    public class NinjectBindings : NinjectModule
    {
        public override void Load()
        {
       //     Bind<IAlertMessagesEngine>().To<AlertMessagesEngine>().InSingletonScope();
            Bind<IAlertMessageLogger>().To<AlertMessageLogger>().InSingletonScope();
        }
    }
}
