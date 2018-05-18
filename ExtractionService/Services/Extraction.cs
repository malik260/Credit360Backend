using ExtractionService.Contracts;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractionService
{
  public   class Extraction : IExtraction
    {

        public bool islocked = false;
        FinTrakBankingContext context;
        FinTrakBankingStagingContext finaco;


        public Extraction(FinTrakBankingContext context, FinTrakBankingStagingContext finaco)
        {
            this.context = context;
            this.finaco = finaco;
        }

        private async Task UpdateCurrency(List<string> dCurrency)
        {
            List<TBL_CURRENCY> CurrencyData = new List<TBL_CURRENCY>();

            var currencytbl = context.TBL_CURRENCY.ToList();

            foreach (var item in dCurrency)
            {

                var dat = currencytbl.FirstOrDefault(d => d.CURRENCYCODE == item);
                if (dat == null)
                {
                    var cdata = new TBL_CURRENCY
                    {
                        CREATEDBY = -1,
                        CURRENCYCODE = item,
                        CURRENCYNAME = item,
                        DATETIMECREATED = DateTime.Now.Date,
                        INUSE = false,
                    };
                    CurrencyData.Add(cdata);
                }
            }
            if (CurrencyData.Any())
            {
                context.TBL_CURRENCY.AddRange(CurrencyData);
            await context.SaveChangesAsync();
            }
        }


        private async Task UpdateRateCode(List<string> dRateCode)
        {
            List<TBL_CURRENCY_RATECODE> RATECODELst = new List<TBL_CURRENCY_RATECODE>();



            var RATECODE = context.TBL_CURRENCY_RATECODE.ToList();

            foreach (var item in dRateCode)
            {
                var ratecode = RATECODE.FirstOrDefault(c => c.RATECODE == item);

                if (ratecode == null)
                {
                    var rateData = new TBL_CURRENCY_RATECODE
                    {
                        RATECODE = item,
                        RATECODEDESCRIPTION = item
                    };
                    RATECODELst.Add(rateData);
                }
            }
            if (RATECODELst.Any())
            {
                context.TBL_CURRENCY_RATECODE.AddRange(RATECODELst);
                await context.SaveChangesAsync();
            }


        }

        public async Task CurrencyExchangeRateExtraction()
        {
            int i = 0;
            Console.WriteLine($"testeing ->  {i++ }" );

            var applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var checkRate = context.TBL_CURRENCY_EXCHANGERATE.Where(c => c.DATE.Date == applicationDate.Date);

            //terminate if record exist for FINANCE CURRENT DATE
            if (checkRate.Any())
            {
                context.TBL_CURRENCY_EXCHANGERATE.RemoveRange(checkRate);
                context.SaveChanges();
                return;
            }


            List<TBL_CURRENCY_EXCHANGERATE> lstExchangeRate = new List<TBL_CURRENCY_EXCHANGERATE>();



            var RATECODE = context.TBL_CURRENCY_RATECODE.ToList();

            int currencyId = 0; int rateCodeId = 0;

            var STGEXCHANGERATE = finaco.STG_CURRENCY_EXCHANGERATE.ToList();

            var CURRENCY = context.TBL_CURRENCY.ToList();


            var dCurrency = STGEXCHANGERATE.Select(c => c.CURRENCYCODE).Distinct().ToList();
            var dRateCode = STGEXCHANGERATE.Select(c => c.RATECODE).ToList().Distinct().ToList();

              UpdateRateCode(dRateCode).Wait(500);

              UpdateCurrency(dCurrency).Wait(500);

            foreach (var item in STGEXCHANGERATE)
            {
                var ratecode = RATECODE.FirstOrDefault(c => c.RATECODE == item.RATECODE);

                if (ratecode != null)
                {
                    rateCodeId = ratecode.RATECODEID;
                }

                var currency = CURRENCY.FirstOrDefault(c => c.CURRENCYCODE == item.CURRENCYCODE);

                if (currency != null)
                {
                    currencyId = currency.CURRENCYID;
                }



                if (rateCodeId > 0)
                {
                    var data = new TBL_CURRENCY_EXCHANGERATE
                    {
                        DATETIMECREATED = DateTime.Now.Date,
                        CREATEDBY = -1,
                        DATE = (DateTime)item.DATE,
                        BASECURRENCYID = CURRENCY.FirstOrDefault(c => c.CURRENCYCODE == item.BASECURRENCY).CURRENCYID,
                        CURRENCYID = (short)currencyId,
                        RATECODEID = (short)rateCodeId,
                        EXCHANGERATE = item.EXCHANGERATE,
                    };

                    lstExchangeRate.Add(data);
                }
            }
            if (lstExchangeRate.Any())
            {
                context.TBL_CURRENCY_EXCHANGERATE.AddRange(lstExchangeRate);
                await context.SaveChangesAsync();
            }
            
        }

        public async Task ProductPricingExtraction()
        {
            var applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            int currencyId = 0; int priceIndexId = 0;
            var PRICE_INDEX_DAILY = context.TBL_PRODUCT_PRICE_INDEX_DAILY.Where(c => c.PRICEDATE.Date ==  applicationDate.Date.AddDays(- 1));

            //terminate if record exist for FINANCE CURRENT DATE
            if (PRICE_INDEX_DAILY.Any())
            {
                context.TBL_PRODUCT_PRICE_INDEX_DAILY.RemoveRange(PRICE_INDEX_DAILY);
                context.SaveChangesAsync().Wait(500);
                return;

            }

            List<TBL_PRODUCT_PRICE_INDEX_DAILY> lstPriceIndex = new List<TBL_PRODUCT_PRICE_INDEX_DAILY>();

            var PRICE_INDEX = context.TBL_PRODUCT_PRICE_INDEX.ToList();

            var CURRENCY = context.TBL_CURRENCY.ToList();

            var STG_PRICE_INDEX_DAILY = finaco.STG_PRICE_INDEX_RATE;//.Where(c => c.PRICEDATE == applicationDate.Date.AddDays(-1));

            // UpdateCurrency(CURRENCY);

            foreach (var item in STG_PRICE_INDEX_DAILY)
            {

                currencyId = CURRENCY.FirstOrDefault(c => c.CURRENCYCODE == item.CURRENCY).CURRENCYID;
                priceIndexId = PRICE_INDEX.FirstOrDefault(p => p.PRICEINDEXNAME == item.PRICEINDEX).PRODUCTPRICEINDEXID;


                var data = new TBL_PRODUCT_PRICE_INDEX_DAILY
                {
                    CREATEDBY = -1,
                    PRICEDATE = (DateTime)item.PRICEDATE,
                    DATETIMECREATED = DateTime.Now.Date,
                    PRODUCTPRICEINDEXID = (short)priceIndexId,
                    BID_RATE = (double)item.BID_RATE,
                    OFFER_RATE = (double)item.OFFER_RATE,
                    CURRENCYID = currencyId
                };
                lstPriceIndex.Add(data);
            }

            if (lstPriceIndex.Any())
            {
                context.TBL_PRODUCT_PRICE_INDEX_DAILY.AddRange(lstPriceIndex);
             await  context.SaveChangesAsync();
            }




        }

        public  async Task CustomerAccountExtraction()
        {
            var applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            var data = finaco.STG_CASA_DAILY_BALANCE_INPUT.Where(c => c.BALANCEDATE == applicationDate);

            if (data.Any())
            {
                finaco.STG_CASA_DAILY_BALANCE_INPUT.RemoveRange(data);
                finaco.SaveChangesAsync().Wait(500);
            }

            var loan = context.TBL_LOAN_REVOLVING.Where(l => l.LOANSTATUSID == 1).Select((l) => new STG_CASA_DAILY_BALANCE_INPUT
            {
                ACCOUNTNUMBER = l.TBL_CASA.PRODUCTACCOUNTNUMBER,
                BALANCEDATE = applicationDate
            });


            var revolving = context.TBL_LOAN_REVOLVING.Where(r => r.LOANSTATUSID == 1).Select(r => new STG_CASA_DAILY_BALANCE_INPUT
            {
                ACCOUNTNUMBER = r.TBL_CASA.PRODUCTACCOUNTNUMBER,
                BALANCEDATE = applicationDate
            });

            loan = loan.Union(revolving);

            if (loan.Any())
            {
                finaco.STG_CASA_DAILY_BALANCE_INPUT.AddRange(loan);
               await   finaco.SaveChangesAsync();
            }
        }

        //public async Task   CustomerAccountBalances()
        //{
        //    var stgData = finaco.STG_CASA_DAILY_BALANCE.ToList();

        //    if (stgData.Any())
        //    {
        //        var casData = stgData.Select(s => new TBL_CUSTOM_CASADAILYBALANCE()
        //        {
        //            ACCOUNTBALANCE = s.ACCOUNTBALANCE,
        //            ACCOUNTNAME = s.ACCOUNTNAME,
        //            ACCOUNTNUMBER = s.ACCOUNTNUMBER,
        //            BALANCEDATE = s.BALANCEDATE,
        //            CASADAILYBALANCEID = s.CASADAILYBALANCEID,
        //            CURRENCY = s.CURRENCY,
        //            CUSTOMERCODE = s.CUSTOMERCODE,
        //            SCHEME_CODE = s.SCHEME_CODE,
        //            TOTALINFLOW = s.TOTALINFLOW,
        //            TOTALOUTFLOW = s.TOTALOUTFLOW
        //        });
        //    }
        //}
    }
}
