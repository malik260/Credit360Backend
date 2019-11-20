using System;
namespace FintrakBanking.ViewModels.Finance
{
 
    public partial class CurrencyExchangeRateViewModel
    {        
 
        public short currencyId { get; set; }

        public DateTime date { get; set; }

        public double buyingRate { get; set; }

        public double sellingRate { get; set; }

        public short baseCurrencyId { get; set; }

        public bool isBaseCurrency { get; set; }

        public string webRequestStatus { get; set; }
        public int companyId { get; set; }
        public string fromCurrencyCode { get; set; }
        public string toCurrencyCode { get; set; }
        public double exchangeRate { get; set; }
    }

    public partial class ExchangeRateViewModel
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public CurrencyExchangeRateViewModel data { get; set; }
    }
}