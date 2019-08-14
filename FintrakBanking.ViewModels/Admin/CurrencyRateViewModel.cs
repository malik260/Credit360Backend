using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Admin
{
    public class CurrencyRateViewModel : GeneralEntity
    {
        public short currencyRateId { get; set; }
        public short currencyId { get; set; }
        public DateTime date { get; set; }

        public short rateCodeId { get; set; }

        public string rateCodeName { get; set; }

        public double buyingRate { get; set; }
        public short baseCurrencyId { get; set; }
        public double sellingRate { get; set; }
        public String currency { get; set; }
        public String baseCurrency { get; set; }

        public double exchangeRate { get; set; }
        public string rateCode { get; set; }
    }

    public class CurrencyViewModel 
    {
        public short lookupId { get; set; }
        public string lookupName { get; set; }

        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public string currencyName { get; set; }
        public bool isUse { get; set; }
        public string currencyCodeName { get { return $"{this.currencyName} {this.currencyCode}"; } }
    }
}
