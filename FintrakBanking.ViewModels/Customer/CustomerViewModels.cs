using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerViewModels : GeneralEntity
    {
        public short? customerIssueTypeId;

        public string customerRating { get; set; }

        public string branchCode { get; set; }
        public string rcNumber { get; set; }
        public string customerAccountNo { get; set; }
        public string sectorCode { get; set; }
        public string subSectorCode { get; set; }

        public int? crmsLegalStatusId { get; set; }
        public int? crmsCompanySizeId { get; set; }
        public int? crmsRelationshipTypeId { get; set; }
        public string crmsLegalStatusName { get; set; }
        public string crmsCompanySizeName { get; set; }
        public string crmsRelationshipTypeName { get; set; }

        public int creditBureauCount { get; set; }
        public bool isCreditBureauUploadCompleted { get; set; }
        public double numberOfShares { get; set; }
        public string company_name { get; set; }
        public short companyDirectorTypeId { get; set; }
        public int? companyDirectorId { get; set; }
        public string companyDirectorTypeName { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public int? countryOfResidentId { get; set; }
        public int? numberOfDependents { get; set; }
        public int? numberOfLoansTaken { get; set; }
        public decimal? loanMonthlyRepaymentFromOtherBanks { get; set; }
        public DateTime? dateOfRelationshipWithBank { get; set; }
        public int? relationshipTypeId { get; set; }
        public string teamLDP { get; set; }
        public string teamNPL { get; set; }
        public string corr { get; set; }
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
        public string customerType { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.lastName; } }
        public string fullName { get; set; }
        public string searchItem { get { return this.firstName + " " + this.middleName + " " + this.lastName + " " + this.customerCode; } }
        public string lastName { get; set; }
        public string gender { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string placeOfBirth { get; set; }
        public int? nationalityId { get; set; }
        public string maritalStatus { get; set; }
        public string emailAddress { get; set; }
        public string maidenName { get; set; }
        public string spouse { get; set; }
        public string firstChildName { get; set; }
        public DateTime? childDateOfBirth { get; set; }
        public string occupation { get; set; }
        public short? customerTypeId { get; set; }
        public int? relationshipOfficerId { get; set; }
        public string relationshipOfficerCode { get; set; }
        public string relationshipOfficerName { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public short politicallyExposedPerson { get; set; }

        public string misCode { get; set; }
        public string misStaff { get; set; }
        public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }
        public DateTime? dateValidated { get; set; }
        public string actedOnBy { get; set; }
        public bool accountCreationComplete { get; set; }
        public bool creationMailSent { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public short? subSectorId { get; set; }
        public string subSectorName { get; set; }
        public short? sectorId { get; set; }
        public string sectorName { get; set; }
        public string taxNumber { get; set; }
        public string businessTaxIdNumber { get; set; }
        public bool isInvestmentGrade { get; set; }
        public bool isRealatedParty { get; set; }
        public string customerBVN { get; set; }
        public string bankVerificationNumber { get; set; }
        public short? riskRatingId { get; set; }
        public string riskRatingName { get; set; }
        public string prospectCustomerCode { get; set; }
        public bool isProspect { get; set; }


        public string telephone { get; set; }
        public string accountNumber { get; set; }
        public string contactAddress { get; set; }
        public string lastContactAddress { get; set; }
        public string staffCode { get; set; }
        public string customerSensitivityLevel { get; set; }
        public string fsCaptionGroupCode { get; set; }
        public string electricMeterNumber { get; set; }
        public string officeAddress { get; set; }
        public string nearestLandmark { get; set; }
        public string dateofIncorporation { get; set; }
        public string paidUpCapital { get; set; }
        public string authorizedCapital { get; set; }
        public string employerDetails { get; set; }
        public string directorName { get; set; }
        //public string companyName { get; set; }
        public string directorAddress { get; set; }
        public string directorEmail { get; set; }
        public string directorBvn { get; set; }
        public string directorShareholderPercentage { get; set; }
        public string nameofSignatories { get; set; }
        public string addressofSignatories { get; set; }
        public string phoneNumberofSignatories { get; set; }
        public string emailofSignatories { get; set; }
        public string bvnNumberofSignatories { get; set; }
        public string shareholderName { get; set; }
        public string shareholderBvn { get; set; }
        public string top10SuppliersName { get; set; }
        public string top10SuppliersAddress { get; set; }
        public string top10SuppliersPhoneNumber { get; set; }
        public string top10SuppliersEmailAddress { get; set; }
        public string top10CustomerName { get; set; }
        public string top10CustomerAddress { get; set; }
        public string top10CustomerPhoneNumber { get; set; }
        public string top10CustomerEmailAddress { get; set; }


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
        public int? businessUnitId { get; set; }
        public decimal? pastDueObligations { get; set; }
        public string ownership { get; set; }
        public string businessUnitName { get; set; }
        public string approvalStatusName { get; set; }
        public bool isCurrent { get; set; }
        public string lastUpdatedByName { get; set; }
    }

    public class CustomerViewModels2 : GeneralEntity
    {
        public string branchCode { get; set; }
        public string rcNumber { get; set; }
        public string customerAccountNo { get; set; }

        public new string companyName { get; set; }
        //public short companyDirectorTypeId { get; set; }
        //public int? companyDirectorId { get; set; }
        //public string companyDirectorTypeName { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }


        public int customerId { get; set; }
        public string customerCode { get; set; }
        public string branchName { get; set; }
        //public string customerAccountNo { get; set; }
        public string title { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string customerTypeName { get; set; }
        public string customerType { get; set; }
        public string customerName { get { return this.firstName + " " + this.middleName + " " + this.lastName; } }
        public string fullName { get; set; }
        public string searchItem { get { return this.firstName + " " + this.middleName + " " + this.lastName + " " + this.customerCode; } }
        public string lastName { get; set; }
        public string gender { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string placeOfBirth { get; set; }
        public string nationality { get; set; }
        public string maritalStatus { get; set; }
        public string emailAddress { get; set; }
        public string maidenName { get; set; }
        public string spouse { get; set; }
        //public string firstChildName { get; set; }
        //public DateTime childDateOfBirth { get; set; }
        public string occupation { get; set; }
        public short? customerTypeId { get; set; }
        //public int? relationshipOfficerId { get; set; }
        //public string relationshipOfficerCode { get; set; }
        //public string relationshipOfficerName { get; set; }
        //public bool isPoliticallyExposed { get; set; }

        //public string misCode { get; set; }
        //public string misStaff { get; set; }
        //public int approvalStatus { get; set; }
        //public DateTime dateActedOn { get; set; }
        //public string actedOnBy { get; set; }
        //public bool accountCreationComplete { get; set; }
        //public bool creationMailSent { get; set; }
        //public short customerSensitivityLevelId { get; set; }
        //public short subSectorId { get; set; }
        //public string subSectorName { get; set; }
        //public short sectorId { get; set; }
        //public string sectorName { get; set; }
        //public string taxNumber { get; set; }
        //public string businessTaxIdNumber { get; set; }
        //public bool isInvestmentGrade { get; set; }
        //public bool isRealatedParty { get; set; }
        //public string customerBVN { get; set; }
        //public string bankVerificationNumber { get; set; }
        //public short? riskRatingId { get; set; }
        //public string riskRatingName { get; set; }
        //public string prospectCustomerCode { get; set; }
        //public bool isProspect { get; set; }

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


    public class CustomerEligibilityViewModels
    {
        public string request_id { get; set; }
        public string phone_number { get; set; }
        public string account_number { get; set; }
        public string response_descr { get; set; }
        public string response_code { get; set; }
        public string full_description { get; set; }
        public string tenor { get; set; }
        public string amount { get; set; }
        public string response_id { get; set; }
        public string sal_account { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public double interest { get; set; }
        public double maintenance { get; set; }
        public string module { get; set; }
        public bool isFixedTenor { get; set; }
        public bool IsEligible { get; set; }
        public int customerId { get; set; }
        public string accountNumber { get; set; }
        public string phoneNumber { get; set; }
        public string customerName { get; set; }
        public bool isIblRequest { get; set; }
        public int eliigibilityId { get; set; }
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
        public int? customerSectorId { get; set; }
        public string customerSectorName { get; set; }
        public short? subSectorId { get; set; }
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
    public class CustomerRelatedDirectorViewModel
    {
        public int customerId { get; set; }
        public string customerName { get; set; }
        public short? customerTypeId { get; set; }
        public string customerTypeName { get; set; }
        public short? directorTypeId { get; set; }
        public string directorTypeName { get; set; }
        public string customerCode { get; set; }
        //  public int customerTypeId { get; set; }
        //  public string customerTypeName { get; set; }
    }



    // REST API VIEW MODELS FOR CLIENT INPUT
    //=================================================================================================================================================================
    //=================================================================================================================================================================
    public class IncomingCustomerViewModels
    {
        public short? sectorId { get; set; }
        public short? subsectorId { get; set; }
        public int? createdBy { get; set; }
        public int? staffId { get; set; }
        public int? companyId { get; set; }
        public short? branchId { get; set; }

        public string accountOfficerStaffCode { get; set; }

        public string customerType { get; set; }
        public string request_Id { get; set; }
        public ApiCustomerBusinessDetailsViewModel corporateCustomerInformation { get; set; }
        public ApiIndividualCustomerDetails individualCustomerInformation { get; set; }
        public List<ApiAddressesViewModel> customerAddresses { get; set; }
        public List<ApicontactViewModel> contacts { get; set; }
        public List<ApicontactViewModel> customerContacts { get; set; }

        public List<ApiDirectorViewModel> companyDirectors { get; set; }
        public List<ApiCompanySignatoriesViewModel> companySignatories { get; set; }
        public List<ApiIndividualSupplierViewModel> individualSuppliers { get; set; }
        public List<ApiIndividualClientViewModel> individualClients { get; set; }
        public List<ApiCorporateSupplierViewModel> corporateSuppliers { get; set; }
        public List<ApiCorporateClientViewModel> corporateClients { get; set; }
        public List<ApiShareholderViewModel> individualshareholders { get; set; }
        public List<ApiInsiderRelatedViewModel> insiderRelatedParties { get; set; }
        public List<ApiEmploymentHistoryViewModel> individualEmploymentHistory { get; set; }
        public ApiNextOfkinViewModel individualNextOfKin { get; set; }
        public List<ApiCreditBureauViewModel> creditBureauReport { get; set; }
        public List<ApiSupportingdocumentsViewModel> customerSupportingDocuments { get; set; }
    }

    public class ApiCustomerBusinessDetailsViewModel
    {
        public string customerType { get; set; }
        public string customerCode { get; set; }
        public string crmsLegalStatus { get; set; }
        public string crmsRelationship { get; set; }
        public string crmsCompanySize { get; set; }
        public string accountOfficer { get; set; }
        public string corporateName { get; set; }
        public string dateOfIncorporation { get; set; }
        public string emailAddress { get; set; }
        public string tin { get; set; }
        public string businessUnit { get; set; }
        public int businessUnitId { get; set; }

        public string relationshipTypeWithBank { get; set; }
        public string teamLdr { get; set; }
        public string teamNpl { get; set; }
        public string sector { get; set; }
        public string subSector { get; set; }
        public string politicallyExposed { get; set; }
        public string corporateOrSmeInformation { get; set; }
        public string registrationNumber { get; set; }
        public string companyWebsite { get; set; }
        public string companyEmail { get; set; }
        public string registeredOffice { get; set; }
        public string annualTurnOver { get; set; }
        public string paidUpCapital { get; set; }
        public string authorizedCapital { get; set; }
        public string numberOfEmployees { get; set; }
        public string countryOfParentCompany { get; set; }
        public string companyStructure { get; set; }
        public string businessSector { get; set; }
        public string shareholdersFund { get; set; }
    }

    public class ApiIndividualCustomerDetails
    {
        public string customerType { get; set; }
        public string customerCode { get; set; }
        public string crmsLegalStatus { get; set; }
        public string crmsRelationship { get; set; }
        public string crmsCompanySize { get; set; }
        public string accountOfficer { get; set; }
        public string corporateName { get; set; }
        public string dateOfIncorporation { get; set; }
        public string emailAddress { get; set; }
        public string tin { get; set; }
        public string businessUnit { get; set; }
        public string relationshipTypeWithBank { get; set; }
        public string teamLdr { get; set; }
        public string teamNpl { get; set; }
        public string sector { get; set; }
        public string subSector { get; set; }
        public string politicallyExposed { get; set; }
        public string corporateOrSmeInformation { get; set; }
        public string registrationNumber { get; set; }
        public string companyWebsite { get; set; }
        public string companyEmail { get; set; }
        public string registeredOffice { get; set; }
        public string annualTurnOver { get; set; }
        public string paidUpCapital { get; set; }
        public string authorizedCapital { get; set; }
        public string numberOfEmployees { get; set; }
        public string countryOfParentCompany { get; set; }
        public string companyStructure { get; set; }
        public string businessSector { get; set; }
        public string shareholdersFund { get; set; }
        public int businessUnitId { get; set; }

        public string relationshipManagerCode { get; set; }
        public string title { get; set; }
        public string gender { get; set; }
        public string firstName { get; set; }
        public string dateOfBirth { get; set; }
        public string middleName { get; set; }
        public string customerBvn { get; set; }
        public string lastName { get; set; }
        public string PlaceOfBirth { get; set; }
        public string maritalStatus { get; set; }
        public string occupation { get; set; }
        public string spouse { get; set; }
        public List<ApiChildrenViewmodel> children { get; set; }
        public string countryOfOrigin { get; set; }
        public string countryOfResidence { get; set; }
        public string pastDueObligation { get; set; }
        public string numberOfDependents { get; set; }
        public string numberOfLoansTaken { get; set; }
        public string loanMonthlyRepaymentFromOtherBanks { get; set; }

    }

    public class ApiChildrenViewmodel
    {
        public string childName { get; set; }
        public string childDateOfBirth { get; set; }
    }

    public class ApiAddressesViewModel
    {
        public string addressType { get; set; }  //contactAddress/mailingAddress
        public string address { get; set; }
        public string state { get; set; }
        public string lga { get; set; }
        public string nearestLandmark { get; set; }
        public string city { get; set; }
        public string utilityBillNumber { get; set; }
    }

    public class ApicontactViewModel
    {
        public string officeLandNumber { get; set; }
        public string officeMobileNumber { get; set; }
    }

    public class ApiDirectorViewModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string otherNames { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string maritalStatus { get; set; }
        public string bvn { get; set; }
        public string nin { get; set; }
        public string phoneNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string address { get; set; }
        public string politicallyExposed { get; set; }
    }

    public class ApiCompanySignatoriesViewModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string otherNames { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string maritalStatus { get; set; }
        public string bvn { get; set; }
        public string nin { get; set; }
        public string phoneNumber { get; set; }
        public string dateOfBirth { get; set; }
        public string address { get; set; }
        public string politicallyexposed { get; set; }
    }

    public class ApiIndividualSupplierViewModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string otherNames { get; set; }
        public string isBankCustomer { get; set; }
        public string BankName { get; set; }
        public string accountNumber { get; set; }
        public string tin { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string natureOfBusiness { get; set; }
        public string address { get; set; }
    }

    public class ApiCorporateSupplierViewModel
    {
        public string corporateName { get; set; }
        public string rcNumber { get; set; }
        public string contactPerson { get; set; }
        public string isBankCustomer { get; set; }
        public string BankName { get; set; }
        public string accountNumber { get; set; }
        public string tin { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string natureOfBusiness { get; set; }
        public string address { get; set; }
    }

    public class ApiIndividualClientViewModel
    {
        public string firstName { get; set; }
        public string customerCode { get; set; }
        public string lastName { get; set; }
        public string otherNames { get; set; }
        public string isBankCustomer { get; set; }
        public string BankName { get; set; }
        public string accountNumber { get; set; }
        public string tin { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string natureOfBusiness { get; set; }
        public string address { get; set; }
    }

    public class ApiCorporateClientViewModel
    {
        public string corporateName { get; set; }
        public string rcNumber { get; set; }
        public string contactPerson { get; set; }
        public string isBankCustomer { get; set; }
        public string BankName { get; set; }
        public string accountNumber { get; set; }
        public string tin { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string natureOfBusiness { get; set; }
        public string address { get; set; }
    }

    public class ApiShareholderViewModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string otherNames { get; set; }
        public string nin { get; set; }
        public string bvn { get; set; }
        public string percentageShareholding { get; set; }
        public string tin { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string politicallyExposed { get; set; }
    }

    public class ApiUltimateBeneficialOwner
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string otherNames { get; set; }
        public string nin { get; set; }
        public string bvn { get; set; }
        public string percentageShareholding { get; set; }
        public string tin { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string politicallyExposed { get; set; }
    }

    public class ApiInsiderRelatedViewModel
    {
        public string companyDirectorFirstName { get; set; }
        public string companyDirectorLastName { get; set; }
        public string companyDirectorOtherNames { get; set; }
        public string companyDirectorType { get; set; }  //(Board Member,Shareholder,Board Member/Shareholder,Account Signatory)
        public string RelationshipType { get; set; }
    }

    public class ApiSupportingdocumentsViewModel
    {
        public string uploadType { get; set; }   //E.g. Passport, ID Card
        public string uploadTitle { get; set; }
        public string fileNumber { get; set; }
        public string physicalLocation { get; set; }
        public string fileData { get; set; }
        public string fileExtension { get; set; }
    }

    public class ApiEmploymentHistoryViewModel
    {
        public string EmployerType { get; set; } //E.g. Self Employed, Other Employer
        public string employerName { get; set; }
        public string employerAddress { get; set; }
        public string employerCountry { get; set; }
        public string employerState { get; set; }
        public string officePhone { get; set; }
        public string employmentDate { get; set; }
        public string previousEmployer { get; set; }
        public string yearsInEmployment { get; set; }
        public string totalWorkingExperience { get; set; }
        public string yearsOfCurrentEmployment { get; set; }
        public string terminalBenefits { get; set; }
        public string annualIncome { get; set; }
        public string monthlyIncome { get; set; }
        public string expenditure { get; set; }
    }

    public class ApiNextOfkinViewModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string dateOfBirth { get; set; }
        public string relationship { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string state { get; set; }
        public string lga { get; set; }
        public string cityORTown { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string nearestLandmark { get; set; }
        public List<ApiInsiderRelatedViewModel> insiderRelatedList { get; set; }
    }

    public class ApiCreditBureauViewModel
    {
        public string creditBureauType { get; set; }
        public string reportFileDateinPDF { get; set; }
        public string reportStatus { get; set; }                        //(positive = 1, Negative =0)
        public string documentTypeId { get; set; }
    }

    public class CflLoanApplication : GeneralEntity
    {
        public string applicationReferenceNumber { get; set; }
        public string customerCode { get; set; }

        public string requestId { get; set; } 
        public string callStatusCode { get; set; }
        public string applicationDate { get; set; }

        public string accountOfficerStaffCode { get; set; }

        public string relationshipManagerStaffCode { get; set; }

        public string productCode { get; set; }

        public string tenor { get; set; }

        public string loanAmount { get; set; }

        public string interestRate { get; set; }

        public string currencyCode { get; set; }

        public string priceIndex { get; set; }

        public string fundingSource { get; set; }

        public string repaymentSource { get; set; }

        public string purpose { get; set; }

        public string settlementAccount { get; set; }

        public string sectorCode { get; set; }

        public string subSectorCode { get; set; }

        public string exchangeRate { get; set; }

        public string repaymentTerm { get; set; }
        public List<applicationDocument> loanApplicationFiles  { get; set; }
        public List<ApiCreditBureauViewModel> creditBureauReport { get; set; }


    }

    
    public class applicationDocument
    {
        public string fileData { get; set; }
        public string fileExtension { get; set; }
        public string caption { get; set; }
        public string contentDescription { get; set; }
        public string documentTypeId { get; set; }
    }

    public class APIResponse
    {
        public string StatusCode { get; set; }   
        public string Message { get; set; }
        public string requestId { get; set; }
    	public string applicationReferenceNumber { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
    }

    public class Attachment
    {
        public string FileLink { get; set; }
        public string FileType { get; set; }
    }

    public class OfferLetterResponse 
    {
        public OfferLetterResponse()
        {
            this.Attachment = new Attachment();
        }

        public string StatusCode { get; set; }
        public string RequestId { get; set; }
        public string WorkflowStage { get; set; }
        public string ReasonForRejection { get; set; }
        public string ActionByName { get; set; }
        public string Comment { get; set; }
        public Attachment Attachment { get; set; }
        public string Message { get; set; }

    }

    public class LoanStatusResponse
    {
      
        public string statusCode { get; set; }
        public string requestId { get; set; }
        public string workflowStage { get; set; }
        public string reasonForRejection { get; set; }
        public string actionByName { get; set; }
        public string comment { get; set; }
        public string message { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string responseTime { get; set; }
    }

    public class CFLTokenModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string token { get; set; }
        public string message { get; set; }
        public string errors { get; set; }
        public bool hasError { get; set; }
        public string result { get; set; }
    }
}