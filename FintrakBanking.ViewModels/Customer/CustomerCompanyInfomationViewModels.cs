using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerCompanyInfomationViewModels : GeneralEntity
    {
        public int companyInfomationId { get; set; }
        public int customerId { get; set; }
        public string registrationNumber { get; set; }
        public string companyWebsite { get; set; }
        public string companyEmail { get; set; }
        public string registeredOffice { get; set; }
        public string annualTurnOver { get; set; }
        public string corporateBusinessCategory { get; set; }
        public string creditRating { get; set; }
        public string previousCreditRating { get; set; }
        public int? paidUpCapital { get; set; }
        public int? authorizedCapital { get; set; }
        public List<CustomerCompanyDirectorsViewModels> companyDiretcors { get; set; }
        public List<CustomerCompanyShareholdersViewModels> companyShareholders { get; set; }
        public List<CustomerCompanyAccountSignatoryViewModels> companyAccountSignatories { get; set; }

    }

    public class CustomerCompanyDirectorsViewModels : GeneralEntity
    {
        public int companyDirectorId { get; set; }
        public int customerId { get; set; }
        public short customerTypeId { get; set; }
        public string customerName { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string fullname { get; set; }
        public string bankVerificationNumber { get; set; }
        public short companyDirectorTypeId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public string rcNumber { get; set; }
        public string taxNumber { get; set; }
        public int numberOfShares { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
    }
    public class CustomerCompanyShareholderViewModels
    {
        public int companyDirectorId { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string fullname { get; set; }
        public string bankVerificationNumber { get; set; }
        public short companyDirectorTypeId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public int numberOfShares { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
    }
    public class CustomerCompanyAccountSignatoryViewModels
    {
        public int companyDirectorId { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string fullname { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string bankVerificationNumber { get; set; }
        public short companyDirectorTypeId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public int numberOfShares { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
    }
    public class CustomerClientOrSupplierViewModels : GeneralEntity
    {
        public int client_SupplierId { get; set; }
        public int customerId { get; set; }
        public short customerTypeId { get; set; }
        public string customerTypeName { get; set; }
        public string clientOrSupplierName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string rcNumber { get; set; }
        public string taxNumber { get; set; }
        public string contactPerson { get; set; }
        public bool? hasCASAAccount { get; set; }
        public string casaAccountNumber { get; set; }
        public string client_SupplierAddress { get; set; } 
        public string client_SupplierPhoneNumber { get; set; }
        public string client_SupplierEmail { get; set; }
        public short client_SupplierTypeId { get; set; }
        public string client_SupplierTypeName { get; set; }
    }
    public class CustomerSupplierViewModels
    {
        public int client_SupplierId { get; set; }
        public int customerId { get; set; }
        public short customerTypeId { get; set; }
        public string clientOrSupplierName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string client_SupplierAddress { get; set; }
        public string client_SupplierPhoneNumber { get; set; }
        public string client_SupplierEmail { get; set; }
        public short client_SupplierTypeId { get; set; }
        public string client_SupplierTypeName { get; set; }
    }

    public class CustomerCompanyShareholdersViewModels: CustomerCompanyDirectorsViewModels { }
}