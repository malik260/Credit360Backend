using System;
namespace FintrakBanking.ViewModels
{
    public class LookupViewModel
    {
        public short lookupId { get; set; }
        public string lookupName { get; set; }
        public short lookupTypeId { get; set; }
        public string lookupTypeName { get; set; }

    }
    public class CurrencyRateCodeViewModel
    {
        public short rateCodeId { get; set; }
        public string rateCode { get; set; }
        public string rateCodeDescription { get; set; }

    }
}