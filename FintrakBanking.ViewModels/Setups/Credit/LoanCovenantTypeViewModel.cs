namespace FintrakBanking.ViewModels.Setups
{
    public class LoanCovenantTypeViewModel : GeneralEntity
    {
        public bool requireCasaAccount;

        public short covenantTypeId { get; set; }
        public string covenantTypeName { get; set; }
        public bool requireAmount { get; set; }
        public bool requireFrequency { get; set; }

    }
}