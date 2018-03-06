using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerViewModels : GeneralEntity
    {
        public string branchCode { get; set; }
        public int creditBureauCount { get; set; }
        public bool isCreditBureauUploadCompleted { get; set; }
        public double numberOfShares { get; set; }
        public short companyDirectorTypeId { get; set; }
        public int? companyDirectorId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }

        public CustomerViewModels()
        {
            CustomerAddresses = new List<CustomerAddressViewModels>();
            //CustomerBvn = new List<CustomerBvnViewModels>();
            CustomerEditHistory = new List<CustomerEditHistoryViewModels>();
            CustomerCompanyInfomation = new List<CustomerCompanyInfomationViewModels>();
            CustomerEmploymentHistory = new List<CustomerEmploymentHistoryViewModels>();
            CustomerIdentification = new List<CustomerIdentificationViewModels>();
            CustomerPhoneContact = new List<CustomerPhoneContactViewModels>();
            CustomerCompanyDirectors = new List<CustomerCompanyDirectorsViewModels>();
            CustomerClientOrSupplier = new List<CustomerClientOrSupplierViewModels>();
            CustomerChildren = new List<CustomerChildrenViewModel>();
            CustomerCompanyAccountSignatory = new List<CustomerCompanyAccountSignatoryViewModels>();
        }

        public int customerId { get; set; }
        public string customerCode { get; set; }
        public short branchId { get; set; }
        public int companyMainId { get; set; }
        public string branchName { get; set; }
        //public string customerAccountNo { get; set; }
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
        public string relationshipOfficerName { get; set; }
        public bool isPoliticallyExposed { get; set; }
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
        public bool isInvestmentGrade { get; set; }
        public bool isRealatedParty { get; set; }
        public string customerBVN { get; set; }
        public short? riskRatingId { get; set; }
        public string riskRatingName { get; set; }
        public List<CustomerAddressViewModels> CustomerAddresses { get; set; }
        //public List<CustomerBvnViewModels> CustomerBvn { get; set; }
        public List<CustomerCompanyInfomationViewModels> CustomerCompanyInfomation { get; set; }
        public List<CustomerEditHistoryViewModels> CustomerEditHistory { get; set; }
        public List<CustomerEmploymentHistoryViewModels> CustomerEmploymentHistory { get; set; }
        public List<CustomerIdentificationViewModels> CustomerIdentification { get; set; }
        public List<CustomerPhoneContactViewModels> CustomerPhoneContact { get; set; }
        public List<CustomerCompanyDirectorsViewModels> CustomerCompanyDirectors { get; set; }
        public List<CustomerCompanyShareholderViewModels> CustomerCompanyShareholder { get; set; }
        public List<CustomerCompanyAccountSignatoryViewModels> CustomerCompanyAccountSignatory { get; set; }
        public List<CustomerClientOrSupplierViewModels> CustomerClientOrSupplier { get; set; }
        public List<CustomerSupplierViewModels> CustomerSupplier { get; set; }
        public List<CollateralViewModel> CustomerCollateral { get; set; }
        public List<CustomerChildrenViewModel> CustomerChildren { get; set; }

    }
    public class CustomerInformationStagingViewModels
    {
        public string customerCode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.lastName; } }
        public int customerTypeId { get; set; }
        public DateTime dateOfBirth { get; set; }
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
    public class CustomerChildrenViewModel : GeneralEntity
    {
        public int customerChildrenId { get; set; }
        public int customerId { get; set; }
        public string childName { get; set; }
        public DateTime childDateOfBirth { get; set; }

    }
    public class CustomerSectorViewModel
    {
        public short subSectorId { get; set; }
        public short sectorId { get; set; }
        public string sectorName { get; set; }
        public string sectorCode { get; set; }

    }
    public class CustomerInformationApprovalViemModel
    {
        public int customerId { get; set; }
        public int customerModificationId { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public int targetId { get; set; }
        public int modificationTyepId { get; set; }
        public string modificationType { get; set; }
        public string approvalStatus { get; set; }
        public DateTime dateUpdated { set; get; }
        public string createdBy { get; set; }
        public string customerBranch { get; set; }
        public int approvalStatusId { get; set; }
        public string comment { get; set; }
        public int operationId { get; set; }
    }

    public class ChangeTrackingViewModel
    {
        public int customerId { get; set; }
        public string propertyName { get; set; }
        public string propertyOriginalValue { get; set; }
        public string propertyCurrentValue { get; set; }
    }
}