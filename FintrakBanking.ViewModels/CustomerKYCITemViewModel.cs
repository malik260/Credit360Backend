namespace FintrakBanking.ViewModels
{
    public class KYCItemViewModel : GeneralEntity
    {
        public short kYCItemId { get; set; }

        public short productId { get; set; }

        public string productName { get; set; }

        public int displayOrder { get; set; }

        public bool isMandatory { get; set; }

        public string item { get; set; }
    }

    public class CustomerKYCViewModel : GeneralEntity
    {
        public int customerAccountKYCItemId { get; set; }

        public short? kYCItemId { get; set; }

        public int customerId { get; set; }
        
        public string accountNumber { get; set; }

        public bool? provided { get; set; }

        public bool? deferred { get; set; }

        public bool? waived { get; set; }

        public bool? disapproved { get; set; }

        public bool? approved { get; set; }
    }
}