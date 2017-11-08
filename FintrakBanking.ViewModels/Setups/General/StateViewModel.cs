namespace FintrakBanking.ViewModels.Setups.General
{
    public class StateViewModel : GeneralEntity
    {
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public decimal CollateralSearchChargeAmount { get; set; }
        public decimal ChartingAmount { get; set; }
        public decimal VerificationAmount { get; set; }

    }
}