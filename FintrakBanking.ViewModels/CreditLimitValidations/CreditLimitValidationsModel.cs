namespace FintrakBanking.ViewModels.CreditLimitValidations
{
    public class CreditLimitValidationsModel
    {
        public double totalExposure { get; set; }
        public double outstandingBalance { get; set; }
        public double sectorLimit { get; set; }
        public double exposureLimit { get; set; }
        public double outstandingSectorsBalance { get; set; }
        public double limit { get; set; }
        public double difference { get; set; }
        public short? riskRatingId { get; set; }
        public double ratio { get; set; }

        public decimal maximumAllowedLimit { get; set; }
        public double obligorExposure { get; set; }
        public bool validated { get; set; }
        public decimal initiated { get; set; }
        public decimal approved { get; set; }
        public string limitString { get; set; }
        public decimal nplExposure { get; set; }
    }
}