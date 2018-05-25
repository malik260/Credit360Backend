namespace FintrakBanking.ExtractionService
{
    using Ninject.Modules;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public class NinjectBindings : NinjectModule
    {
        public override void Load()
        {
            //   Bind<IEmailSender>().To<EmailSender>().InSingletonScope();
        }
    }
}
