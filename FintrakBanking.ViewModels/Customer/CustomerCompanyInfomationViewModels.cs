using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerCompanyInfomationViewModels : GeneralEntity
    {
        public int companyInfomationId { get; set; }
        public int customerId { get; set; }
        public string registrationNumber { get; set; }
        public string customerCode { get; set; }
        public string companyWebsite { get; set; }
        public string companyEmail { get; set; }
        public string registeredOffice { get; set; }
        public string annualTurnOver { get; set; }
        public string corporateBusinessCategory { get; set; }
        public string creditRating { get; set; }
        public string previousCreditRating { get; set; }
        public decimal? paidUpCapital { get; set; }
        public decimal? authorizedCapital { get; set; }
        public decimal? shareholderFund { get; set; }
        public List<CustomerCompanyDirectorsViewModels> companyDirectors { get; set; }
        public List<CustomerCompanyShareholdersViewModels> companyShareholders { get; set; }
        public List<CustomerCompanyAccountSignatoryViewModels> companyAccountSignatories { get; set; }
        public int? numberOfEmployees { get; set; }
        public int? countryOfParentCompanyId { get; set; }
        public string companyStructure { get; set; }
        public int noOfFemaleEmployees { get; set; }
        public bool isStartUp { get; set; }
        public bool isFirstTimeCredit { get; set; }
        public string fullName { get; set; }
        public DateTime? birthDate { get; set; }
        public string gender { get; set; }
        public string address { get; set; }
        public string state { get; set; }
        public string phoneNo { get; set; }
        public string email { get; set; }
        public DateTime? interestRepayStartDate { get; set; }
        public DateTime? principalRepayStartDate { get; set; }
        public string interestRepayFreq { get; set; }
        public int tenor { get; set; }
        public string startUp { get; set; }
        public string msmeAnnualTurnover { get; set; }
        public string moratorium { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal amountGranted { get; set; }
        public string tenorType { get; set; }
        public double rate { get; set; }
        public string scheduleType { get; set; }
        public string sector { get; set; }
        public string natureOfBusiness { get; set; }
        public DateTime effectiveDate { get; set; }
        public string principalRepayFreq { get; set; }
        public string firstTimeAccessToCredit { get; set; }
        public string facilityType { get; set; }
        public int? noOfEmployees { get; set; }
        public string wpower { get; set; }
        public string esRating { get; set; }
        public string bvn { get; set; }
        public int? baseYear { get; set; }
        public string refNo { get; set; }
        public int? totalAssets { get; set; }
        public int? corporateCustomerTypeId { get; set; }
        public int? totalAsset { get; set; }
        public string field1 { get; set; }
    }

  
    public class CustomerCompanyShareholderViewModels
    {
        public int companyDirectorId { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string customerNIN { get; set; }
        public string fullname { get; set; }
        public string bankVerificationNumber { get; set; }
        public short companyDirectorTypeId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public double numberOfShares { get; set; }
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
        public string customerNIN { get; set; }
        public string middlename { get; set; }
        public string bankVerificationNumber { get; set; }
        public short companyDirectorTypeId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public double numberOfShares { get; set; }
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
        public string bankName { get; set; }
        public string casaAccountNumber { get; set; }
        public string natureOfBusiness { get; set; }
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
        public string rcNumber { get; set; }
        public string taxNumber { get; set; }
        public string contactPerson { get; set; }
        public string natureOfBusiness { get; set; }
        public string bankName { get; set; }
        public bool? hasCASAAccount { get; set; }
        public string casaAccountNumber { get; set; }
        public string client_SupplierAddress { get; set; }
        public string client_SupplierPhoneNumber { get; set; }
        public string client_SupplierEmail { get; set; }
        public short client_SupplierTypeId { get; set; }
        public string client_SupplierTypeName { get; set; }
    }

    public class CustomerCompanyShareholdersViewModels: CustomerCompanyDirectorsViewModels { }
}