namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerIdentificationViewModels : GeneralEntity
    {
        public int identificationId { get; set; }
        public int customerId { get; set; }
        public string identificationNo { get; set; }
        public int identificationModeId { get; set; }
        public string identificationMode { get; set; }
        public string issuePlace { get; set; }
        public string issueAuthority { get; set; }
    }


}