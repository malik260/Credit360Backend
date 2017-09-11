namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerTypeViewModels : GeneralEntity
    {
        public short customerTypeId { get; set; }
        public string name { get; set; }
    }

    public class CustomerSupplierTypeViewModels
    { 
        public short client_SupplierTypeId { get; set; }
        public string name { get; set; }
    }
    public class CustomerIdentificationModeTypeViewModels
    {
        public int identificationModeId { get; set; }
        public string name { get; set; }
    }
    public class CompanyDirectorTypeViewModels
    {
        public int companyDirectorTypeId { get; set; }
        public string name { get; set; }
    }
}