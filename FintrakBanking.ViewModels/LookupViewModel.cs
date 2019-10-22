using System;
namespace FintrakBanking.ViewModels
{
    public class LookupViewModel
    {
        public string middleName;
        public string firstName;
        public string lastName;

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
        public string businessUnitName { get; set; }
        public int? businessUnitId { get; set; }
    }
    public class CurrencyRateCodeViewModel
    {
        public short rateCodeId { get; set; }
        public string rateCode { get; set; }
        public string rateCodeDescription { get; set; }

    }
}