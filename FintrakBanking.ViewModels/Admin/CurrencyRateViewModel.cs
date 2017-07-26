using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Admin
{
    public class CurrencyRateViewModel : GenaralEntity
    {
        public short currencyRateId { get; set; }
        public short currencyId { get; set; }
        public DateTime date { get; set; }
        public double buyingRate { get; set; }
        public short baseCurrencyId { get; set; }
        public double sellingRate { get; set; }
        public String currency { get; set; }
        public String baseCurrency { get; set; }
    }

    public class CurrencyViewModel 
    {
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public string currencyName { get; set; }
        public string currencyCodeName { get { return $"{this.currencyName} {this.currencyCode}"; } }
    }
}
