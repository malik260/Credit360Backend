namespace FintrakBanking.ViewModels.CreditLimitValidations
{
    public class CreditLimitValidationsModel
    {
        public double totalExposure { get; set; }
        public double outstandingBalance { get; set; }
        public double limit { get; set; }
        public double difference { get; set; }
        public short? riskRatingId { get; set; }
        public double ratio { get; set; }

    }
}