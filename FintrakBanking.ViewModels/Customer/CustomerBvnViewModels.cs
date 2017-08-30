namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerBvnViewModels : GeneralEntity
    {
        public int customerBvnid { get; set; }
        public int customerId { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string bankVerificationNumber { get; set; }
        public bool isValidBvn { get; set; }
        public bool isPoliticallyExposed { get; set; }
    }


}