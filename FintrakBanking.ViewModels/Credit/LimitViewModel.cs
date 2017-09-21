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
}
