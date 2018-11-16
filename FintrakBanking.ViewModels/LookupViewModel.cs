using System;
namespace FintrakBanking.ViewModels
{
    public class LookupViewModel
    {
        public short approvalStatusId { get; set; }

        public short lookupId { get; set; }
        public string lookupName { get; set; }
        public int? lookupcustomerId { get; set; }
        public short lookupTypeId { get; set; }
        public string lookupTypeName { get; set; }
        public string mode { get; set; }
        public double value { get; set; }
        public string description { get; set; }
        public bool? isVisible { get; set; }

    }
    public class CurrencyRateCodeViewModel
    {
        public short rateCodeId { get; set; }
        public string rateCode { get; set; }
        public string rateCodeDescription { get; set; }

    }
}