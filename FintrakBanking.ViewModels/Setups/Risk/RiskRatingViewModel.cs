namespace FintrakBanking.ViewModels.Setups
{
    public class RiskRatingViewModel : GeneralEntity
    {
        public int riskRatingId { get; set; }
        public string rates { get; set; }
        public decimal? maxRange { get; set; }
        public decimal? minRange { get; set; }
        public decimal? advicedRate { get; set; }
        public string ratesDescription { get; set; }
        public short productId { get; set; }
    }
}