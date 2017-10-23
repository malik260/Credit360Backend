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

    }
}