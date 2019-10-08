namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerAddressViewModels : GeneralEntity
    {
        public int addressId { get; set; }
        public string address { get; set; }
        public int stateId { get; set; }
        public int cityId { get; set; }
        public string homeTown { get; set; }
        public string nearestLandmark { get; set; }
        public string electricMeterNumber { get; set; }
        public string pobox { get; set; }
        public int customerId { get; set; }
        public short addressTypeId { get; set; }
        public bool active { get; set; }
        public string mailingAddress { get; set; }
        public int? localGovernmentId { get; set; }
        public string localGovernmentName { get; set; }
        public string stateName { get; set; }
        public string city { get; set; }
    }


}