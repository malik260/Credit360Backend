using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using FintrakBanking.Entities.Models;
using DataExtractionService.Interface;
using DataExtractionService.Services_Logic;
using FintrakBanking.Entities.StagingModels;
//using TopShelfWindowsService.Services;
namespace DataExtractionService
{
   public  class ModuleNinject
    {
        public class IocModule: NinjectModule
        {
            public override void Load()
            {
                Bind<FinTrakBankingContext>().To<FinTrakBankingContext>().InSingletonScope();
                Bind<ITreasuaryRateExtraction>().To<TreasuaryRateExtraction>().InSingletonScope();
                Bind<FinTrakBankingStagingContext>().To<FinTrakBankingStagingContext>().InSingletonScope();
            }
        }
    }
}
