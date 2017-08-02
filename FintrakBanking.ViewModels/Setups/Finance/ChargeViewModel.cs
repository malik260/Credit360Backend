
namespace FintrakBanking.ViewModels.Setups.Finance
{
    public class ChargeViewModel : GenaralEntity
    {
        public int chargeId { get; set; }
        public string chargeName { get; set; }
        public int operationId { get; set; }
        public decimal setValue { get; set; }
        public int gLAccountId { get; set; }
        public int frequency { get; set; }
        public bool applyVAT { get; set; }
        public bool applyWHT { get; set; }
    }

    public class ChargeRangeViewModel : GenaralEntity
    {
        public int rangeId { get; set; }
        public double rate { get; set; }
        public decimal minimumAmount { get; set; }
        public decimal maximumAmount { get; set; }
        public int chargeId { get; set; }
    }

    public class TaxViewModel : GenaralEntity
    {
        public int rangeId { get; set; }
        public double rate { get; set; }
        public decimal minimumAmount { get; set; }
        public decimal maximumAmount { get; set; }
        public int chargeId { get; set; }
    }
}
