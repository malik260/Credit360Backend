using DataExtractionService.Interface;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExtractionService.Services_Logic
{
    public class TreasuaryRateExtraction : ITreasuaryRateExtraction
    {
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext stagingcontext;
        private IGeneralSetupRepository genSetup;
        public TreasuaryRateExtraction(FinTrakBankingContext context, FinTrakBankingStagingContext stagingcontext, IGeneralSetupRepository genSetup)
        {
            this.context = context;
            this.stagingcontext = stagingcontext;
            this.genSetup = genSetup;
        }


        public bool MigrateExchangeRate()
        {
            
            var data = stagingcontext.CWG_TREASURY_RATE_TBL.Select(d => new STG_PRICE_INDEX_RATE
            {
                BID_RATE = d.BID_RATE,
                CURRENCY = d.CURRENCY,
                OFFER_RATE = d.OFFER_RATE,
                PRICEDATE = d.DATE,
                PRICEINDEX = d.PRODUCT
            });

            stagingcontext.STG_PRICE_INDEX_RATE.AddRange(data);

            return stagingcontext.SaveChanges() > 0;

        }
    }
}
