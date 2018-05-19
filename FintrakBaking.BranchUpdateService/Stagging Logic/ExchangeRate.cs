using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBaking.BranchUpdateService.Stagging_Logic
{
    public class ExchangeRate
    {
        FinTrakBankingContext coreContext = new FinTrakBankingContext();
        FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext();
        private IGeneralSetupRepository genSetup;
        public bool MigrateExchangeRate()
        {
            //var data = stagingContext.CWG_TREASURY_RATE_TBL.Where(d => d.DATE == genSetup.GetApplicationDate())
            //    .Select(d => new STG_PRICE_INDEX_RATE
            //    {
            //        BID_RATE = d.BID_RATE,
            //        CURRENCY = d.CURRENCY,
            //        OFFER_RATE = d.OFFER_RATE,
            //        PRICEDATE = d.DATE,
            //        PRICEINDEX = d.PRODUCT
            //    });

            //stagingContext.STG_PRICE_INDEX_RATE.AddRange(data);

            return true;// stagingContext.SaveChanges() > 0;

        }

    }
}
