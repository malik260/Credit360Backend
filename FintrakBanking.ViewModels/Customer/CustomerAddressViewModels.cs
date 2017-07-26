namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerAddressViewModels : GenaralEntity
    {
        public int addressId { get; set; }
        public string address { get; set; }
        public int stateId { get; set; }
        public int cityId { get; set; }
        public string homeTown { get; set; }
        public string pobox { get; set; }
        public int customerId { get; set; }
        public int addressTypeId { get; set; }
        public bool active { get; set; }
    }


}