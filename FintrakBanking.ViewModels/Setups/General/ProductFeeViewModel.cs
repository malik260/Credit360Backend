namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductFeeViewModel : GeneralEntity
    {
        public int productFeeId { get; set; }
        public short productId { get; set; }
        public string productName { get; set; }
        public int feeId { get; set; }
        public string feeName { get; set; }
        public string feeTargetName { get; set; }
        public string feeIntervalName { get; set; }
        public string feeTypeName { get; set; }
        public string glAccountCode { get; set; }
        public string glAccountName { get; set; }        

        public decimal rateValue { get; set; }
        public decimal? dependentAmount { get; set; }

        //public int companyId { get; set; }
        //public int createdBy { get; set; }
        //public DateTime dateTimeCreated { get; set; }
        //public int? lastUpdatedBy { get; set; }
        //public DateTime? dateTimeUpdated { get; set; }
        //public bool? deleted { get; set; }
        //public int? deletedBy { get; set; }
        //public DateTime? dateTimeDeleted { get; set; }
    }
}