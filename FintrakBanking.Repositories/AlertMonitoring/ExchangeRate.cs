using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.AlertMonitoring
{
    public class ExchangeRate : ICurrencyAndRateUpdate
    {
        FinTrakBankingContext coreContext = new FinTrakBankingContext();
        FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext();
        private IGeneralSetupRepository genSetup;


        public bool MigrateExchangeRate()
        {
            try
            {
                DateTime date = stagingContext.STG_TREASURY_RATE_TBL.Select(o => o.DATE).OrderByDescending(o => o.Date).FirstOrDefault();
                var indexRate = (from x in stagingContext.STG_TREASURY_RATE_TBL
                                 where x.DATE == date 
                                 select new IndexRateChangeViewModel
                                 {
                                     product = x.PRODUCT,
                                     currency = x.CURRENCY,
                                     offerRate = x.OFFER_RATE,
                                     bidRate = x.BID_RATE,
                                     date = x.DATE

                                 }).ToList();

                if (indexRate != null)
                {
                    foreach (var rate in indexRate)
                    {
                        int currencyId = coreContext.TBL_CURRENCY.Where(o => o.CURRENCYCODE == rate.currency).Select(o => o.CURRENCYID).FirstOrDefault();
                        var change = coreContext.TBL_PRODUCT_PRICE_INDEX.Where(o => o.PRICEINDEXNAME == rate.product && o.CURRENCYID == currencyId).FirstOrDefault();

                        if (change != null)
                        {
                            change.PRICEINDEXRATE = (double)rate.bidRate;
                            change.LASTREFRESHDATE = DateTime.Now;
                        }
                    }

                    return coreContext.SaveChanges() > 0;
                }
            }catch(Exception ex)
            {

            }
            return false;
        }

    }
}
