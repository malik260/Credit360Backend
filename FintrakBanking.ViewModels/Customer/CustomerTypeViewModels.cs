namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerTypeViewModels : GeneralEntity
    {
        public short customerTypeId { get; set; }
        public string name { get; set; }
        public bool isHybrid { get; set; }
    }

    public class CustomerAddressTypeViewModels
    {
        public short addressTypeId { get; set; }
        public string addressTypeName { get; set; }
    }

    public class CustomerRiskRatingViewModels
    {
        public short riskRatingId { get; set; }
        public string riskRating { get; set; }
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
    public class CorporateCustomerTypeViewModels
    {
        public int corporateCustomerTypeId { get; set; }
        public string corporateCustomerTypeName { get; set; }
    }
}