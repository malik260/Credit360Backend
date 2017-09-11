namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerPhoneContactViewModels : GeneralEntity
    {
        public int phoneContactId { get; set; }
        public string phone { get; set; }
        public string phoneNumber { get; set; }
        public int customerId { get; set; }
        public bool active { get; set; }
    }


}