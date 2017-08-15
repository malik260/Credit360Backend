namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductCollateralTypeViewModel : GeneralEntity
    {
        public int productCollateralId { get; set; }
        public short productId { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }
        //public int companyId { get; set; }
        //public int createdBy { get; set; }
        //public DateTime dateTimeCreated { get; set; }
        //public int? lastUpdatedBy { get; set; }
        //public DateTime? dateTimeUpdated { get; set; }
        //public bool deleted { get; set; }
        //public int? deletedBy { get; set; }
        //public DateTime? dateTimeDeleted { get; set; }
    }
}