namespace FintrakBanking.ViewModels.Setups
{
    public class LoanCovenantTypeViewModel : GeneralEntity
    {
        public bool requireCasaAccount;
        public bool isFinancial { get; set; }
        public short covenantTypeId { get; set; }
        public string covenantTypeName { get; set; }
        public bool requireAmount { get; set; }
        public bool requireFrequency { get; set; }

    }
}