using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerViewModels : GeneralEntity
    {
        public CustomerViewModels()
        {
            CustomerAddresses = new List<CustomerAddressViewModels>();
            CustomerBvn = new List<CustomerBvnViewModels>();
            CustomerEditHistory = new List<CustomerEditHistoryViewModels>();
            CustomerCompanyInfomation = new List<CustomerCompanyInfomationViewModels>();
            CustomerEmploymentHistory = new List<CustomerEmploymentHistoryViewModels>();
            CustomerIdentification = new List<CustomerIdentificationViewModels>();
            CustomerPhoneContact = new List<CustomerPhoneContactViewModels>();
            CustomerCompanyDirectors = new List<CustomerCompanyDirectorsViewModels>();
            CustomerClientOrSupplier = new List<CustomerClientOrSupplierViewModels>();
        }

        public int customerId { get; set; }
        public string customerCode { get; set; }
        public short branchId { get; set; }
        public int companyMainId { get; set; }
        public string branchName { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string customerTypeName { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.lastName; } }
         public string fullName { get; set; }
        public string searchItem { get { return this.firstName + " " + this.middleName + " " + this.lastName + " " + this.customerCode; } }
        public string lastName { get; set; }
        public string gender { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string placeOfBirth { get; set; }
        public string nationality { get; set; }
        public int maritalStatus { get; set; }
        public string emailAddress { get; set; }
        public string maidenName { get; set; }
        public string spouse { get; set; }
        public string firstChildName { get; set; }
        public DateTime childDateOfBirth { get; set; }
        public string occupation { get; set; }
        public short customerTypeId { get; set; }
        public int relationshipOfficerId { get; set; }
        public bool politicallyExposedPerson { get; set; }
        public string misCode { get; set; }
        public string misStaff { get; set; }
        public int approvalStatus { get; set; }
        public DateTime dateActedOn { get; set; }
        public string actedOnBy { get; set; }
        public bool accountCreationComplete { get; set; }
        public bool creationMailSent { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public short subSectorId { get; set; }
        public string subSectorName { get; set; }
        public short sectorId { get; set; }
        public string sectorName { get; set; }
        public string taxNumber { get; set; }

        public List<CustomerAddressViewModels> CustomerAddresses { get; set; }
        public List<CustomerBvnViewModels> CustomerBvn { get; set; }
        public List<CustomerCompanyInfomationViewModels> CustomerCompanyInfomation { get; set; }
        public List<CustomerEditHistoryViewModels> CustomerEditHistory { get; set; }
        public List<CustomerEmploymentHistoryViewModels> CustomerEmploymentHistory { get; set; }
        public List<CustomerIdentificationViewModels> CustomerIdentification { get; set; }
        public List<CustomerPhoneContactViewModels> CustomerPhoneContact { get; set; }
        public List<CustomerCompanyDirectorsViewModels> CustomerCompanyDirectors { get; set; }
        public List<CustomerCompanyShareholderViewModels> CustomerCompanyShareholder { get; set; }
        public List<CustomerClientOrSupplierViewModels> CustomerClientOrSupplier { get; set; }
        public List<CustomerSupplierViewModels> CustomerSupplier { get; set; }
        
    }

    public class CustomerSearchItemViewModels
    {
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string phoneNumber { get; set; }
        public int? customerTypeId { get; set; }
        public string customerTypeName { get; set; }
        public string customerCode { get; set; }
        public int relationshipOfficerId { get; set; }
        public string relationshipOfficerName { get; set; }
        public int? branchId { get; set; }
        public string branchName { get; set; }
        public int customerSectorId { get; set; }
        public string customerSectorName { get; set; }
        public short subSectorId { get; set; }
        public string subSectorName { get; set; }
    }

    public class CustomerSectorViewModel
    {
        public short subSectorId { get; set; }
        public short sectorId { get; set; }
        public string sectorName { get; set; }
        public string sectorCode { get; set; }

    }
}