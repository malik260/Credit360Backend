using FintrakBanking.ViewModels.CreditLimitValidations;

namespace FintrakBanking.ViewModels.Credit
{
    public class LimitViewModel : GeneralEntity
    {
        public int limitId { get; set; }
        public string limitName { get; set; }
        public int limitValueTypeId { get; set; }
        public int limitMetricId { get; set; }
        public string limitValueType { get; set; }
        public string limitMetric { get; set; }
    }

    public class LimitDetailViewModel : GeneralEntity
    {
        public int limitDetailId { get; set; }
        public int limitTypeId { get; set; }
        public int limitId { get; set; }
        public int targetId { get; set; }
        public string targetName { get; set; }
        public short limitFrequencyTypeId { get; set; }
        public string limitFrequencyTypeName { get; set; }
        public decimal minimumValue { get; set; }
        public decimal maximumValue { get; set; }
        public string limitName { get; set; }
        public string limitTypeName { get; set; }
        public bool allowOverride { get; set; }
    }

    public class LimitMetricViewModel
    {
        public int limitMetricId { get; set; }
        public string limitMetricName { get; set; }
    }

    public class LimitTypeViewModel
    {
        public int limitTypeId { get; set; }
        public string limitTypeName { get; set; }
    }

    public class TargetViewModel
    {
        public int targetId { get; set; }
        public string targetName { get; set; }
    }
    public class LimitValueTypeViewModel 
    {
        public int limitValueTypeId { get; set; }
        public string limitValueTypeName { get; set; }
    }

    public class FrequencyTypeViewModel
    {
        public short frequencyTypeId { get; set; }
        public string mode { get; set; }
        public double value { get; set; }
        public string description { get; set; }
        public bool? isVisible { get; set; }
    }


    public class LimitSuspensionViewModel : GeneralEntity
    {
        public int limitId { get; set; }
        //public string limitName { get; set; }
        //public int staffId { get; set; }
        public int branchId { get; set; }
        public decimal limitAmount { get; set; }
        public decimal amount { get; set; }
    }

    public class CurrencyLimitViewModel : GeneralEntity
    {
        public int currencyLimitId { get; set; }
        public string currencyLimitName { get; set; }
        public decimal currencyLimitValue { get; set; }
        public string description { get; set; }
        public string currencyName { get; set; }
        public string currencyCode { get; set; }
    }

    public class GroupLimitViewModel : GeneralEntity
    {
        public int groupLimitId { get; set; }
        public int? limitNumber { get; set; }
        public string groupName { get; set; }
        public decimal groupLimitValue { get; set; }
        public string description { get; set; }
    }

    public class ObligorLimitViewModel : GeneralEntity
    {
        public int scenerio { get; set; } // 1 appl, 2 lmsa
        public int applicationId { get; set; }
        public int? customerId { get; set; }
        public int? customerGroupId { get; set; }
        public int riskRatingId { get; set; }
        public string riskRating { get; set; }
        public string description { get; set; }
        public bool isInvestmentGrade { get; set; }
        public double maxShareholderPercentage { get; set; }
    }

    //public class LimitValidationViewModel : CreditLimitValidationsModel
    //{
    //    public decimal maximumAllowedLimit { get; set; }
    //    public decimal obligorExposure { get; set; }
    //    public bool validated { get; set; }
    //}
}
