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
    public class ExchangeRate 
    {
        FinTrakBankingContext coreContext = new FinTrakBankingContext();
        FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext();
        //private IGeneralSetupRepository genSetup;


        public bool MigrateExchangeRate()
        {
            try
            {
                DateTime date =(from o in  stagingContext.STG_TREASURY_RATE_TBL orderby o.DATE descending select o.DATE  ).FirstOrDefault();
                DateTime? lastRefreshDate = (from o in coreContext.TBL_PRODUCT_PRICE_INDEX orderby o.LASTREFRESHDATE descending select o.LASTREFRESHDATE).FirstOrDefault();

                if (lastRefreshDate != null && lastRefreshDate >= date)
                    return false;


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


                if (indexRate == null)
                    return false;


                    foreach (var rate in indexRate)
                    {
                        int currencyId = coreContext.TBL_CURRENCY.Where(o => o.CURRENCYCODE == rate.currency).Select(o => o.CURRENCYID).FirstOrDefault();
                        var change = coreContext.TBL_PRODUCT_PRICE_INDEX.Where(o => o.PRICEINDEXNAME == rate.product && o.CURRENCYID == currencyId).FirstOrDefault();

                        if (change != null)
                        {
                            change.PRICEINDEXRATE = (double)rate.bidRate;
                            change.LASTREFRESHDATE = date;
                        }
                    }

                    return coreContext.SaveChanges() > 0;
                
            }catch(Exception ex)
            {

            }
            return false;
        }

    }
}
