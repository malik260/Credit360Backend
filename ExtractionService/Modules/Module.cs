 

namespace ExtractionService.Modules
{
    using ExtractionService.Contracts;
    using ExtractionService.Services;
    using FintrakBanking.Entities.Models;
    using FintrakBanking.Entities.StagingModels;
    //  using Contracts;
    using Ninject.Modules;
    using Ninject.Web.Common;
    using Topshelf;

    //  using Services;

    namespace Modules
    {
        public class Module : NinjectModule
        {
            public override void Load()
            {
              //  Bind<HostControl>().To<HostControl>().InSingletonScope();
                Bind<FinTrakBankingContext>().To<FinTrakBankingContext>().InSingletonScope();
                Bind<FinTrakBankingStagingContext>().To<FinTrakBankingStagingContext>().InSingletonScope();
                Bind<IExtraction>().To<Extraction>().InSingletonScope();

            }
        }
    }
}
