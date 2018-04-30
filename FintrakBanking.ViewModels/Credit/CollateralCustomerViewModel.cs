using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralViewModel : GeneralEntity 
    {
        public bool requireInsurancePolicy;
        public string collateralDetail { get; set; }
        public decimal? collateralSearchAmount { get; set; }
        public decimal? chartingAmount { get; set; }
        public decimal? verificationAmount { get; set; }
        public bool? legalFeeTaken { get; set; }
        public int notificationDuration { get; set; }
        public int collateralId { get; set; }
        public int detailId { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }
        public short collateralSubTypeId { get; set; }
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public string currency { get; set; }
        public string collateralCode { get; set; }
        public string camRefNumber { get; set; }
        public bool allowSharing { get; set; }
        public bool isLocationBased { get; set; }
        public int? valuationCycle { get; set; }
        public double haircut { get; set; }
        public bool hasInsurance { get; set; }
        public string collateralSubTypeName { get; set; }
        public string fundName { get; set; }
        public string insuranceType { get; set; }
        public string metalType { get; set; }
        public string machineType { get; set; }
        public double exchangeRate { get; set; }

        // presentation
        public int approvalStatus { get; set; }

        // insurance
        public string referenceNumber { get; set; }
        public decimal sumInsured { get; set; }
        public string insuranceCompany { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public InsurancePolicies insurancePolicies { get; set; }
        public int policyId { get; set; }

        public List<InsurancePolicies> insurancePolicy { get; set; }

        public List<CollateralDocumentViewModel> collateralVisitation { get; set; }
        // deposit
        public int collateralDepositId { get; set; }

        public string dealReferenceNumber { get; set; }
        public string accountNumber { get; set; }
        public decimal existingLienAmount { get; set; }
        public decimal lienAmount { get; set; }
        public decimal availableBalance { get; set; }
        public decimal securityValue { get; set; }
        public DateTime maturityDate { get; set; }
        public decimal maturityAmount { get; set; }
        public string remark { get; set; }

        // equipment
        public string machineName { get; set; }

        public string description { get; set; }
        public string machineNumber { get; set; }
        public string manufacturerName { get; set; }
        public string yearOfManufacture { get; set; }
        public string yearOfPurchase { get; set; }
        public int valueBaseTypeId { get; set; }
        public string valueBaseTypeName { get; set; }
        public string machineCondition { get; set; }
        public string machineryLocation { get; set; }
        public decimal replacementValue { get; set; }
        public string equipmentSize { get; set; }
        public string intendedUse { get; set; }

        // miscellaneous
        public string securityName { get; set; }

        public List<MiscellaneousNote> notes { get; set; }

        // guarantee

        public List<crossGarantee> crossGarantee { get; set; }
        public int collateralGauranteeId { get; set; }

        public int collateralCustomerId { get; set; }
        public string institutionName { get; set; }
        public string guarantorAddress { get; set; }
        //  public string guarantorReferenceNumber { get; set; }
        public decimal guaranteeValue { get; set; }
        public DateTime? endDate { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string bvn { get; set; }
        public string rcNumber { get; set; }
        public string phoneNumber1 { get; set; }
        public string phoneNumber2 { get; set; }
        public string emailAddress { get; set; }
        public string relationship { get; set; }
        public string relationshipDuration { get; set; }
        public DateTime cStartDate { get; set; }
        public string taxNumber { get; set; }

        // casa
        public int collateralCasaId { get; set; }

        public bool isOwnedByCustomer { get; set; }

        // immovableProperty
        public int collateralPropertyId { get; set; }

        public string propertyName { get; set; }
        public int cityId { get; set; }
        public short countryId { get; set; }
        public DateTime? constructionDate { get; set; }
        public string propertyAddress { get; set; }
        public DateTime dateOfAcquisition { get; set; }
        public DateTime lastValuationDate { get; set; }
        public short? valuerId { get; set; }
        public string valuerReferenceNumber { get; set; }
        public short propertyValueBaseTypeId { get; set; }
        public decimal openMarketValue { get; set; }
        public decimal? collateralValue { get; set; }
        public decimal? forcedSaleValue { get; set; }
        public decimal stampToCoverAmount { get; set; }
        public string valuationSource { get; set; }
        public decimal originalValue { get; set; }
        public decimal availableValue { get; set; }
        public decimal? collateralUsableAmount { get; set; }
        public string nearestLandMark { get; set; }
        public string nearestBusStop { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public byte? perfectionStatusId { get; set; }
        public string perfectionStatusReason { get; set; }

        // marketableSecurities
        public int collateralMarketableSecurityId { get; set; }

        public string securityType { get; set; }
        public DateTime effectiveDate { get; set; }
        public decimal dealAmount { get; set; }
        public decimal lienUsableAmount { get; set; }
        public string issuerName { get; set; }
        public string issuerReferenceNumber { get; set; }
        public decimal unitValue { get; set; }
        public int numberOfUnits { get; set; }
        public short rating { get; set; }
        public short percentageInterest { get; set; }
        public short? interestPaymentFrequency { get; set; }

        // policy
        public int collateralInsurancePolicyId { get; set; }

        public string insurancePolicyNumber { get; set; }
        public decimal premiumAmount { get; set; }
        public decimal policyAmount { get; set; }
        public string insuranceCompanyName { get; set; }
        public string insurerAddress { get; set; }
        public DateTime policyStartDate { get; set; }
        public DateTime assignDate { get; set; }
        public short? renewalFrequencyTypeId { get; set; }
        public string insurerDetails { get; set; }
        public DateTime policyRenewalDate { get; set; }

        // preciousMetal
        public int collateralPreciousMetalId { get; set; }

        public string preciousMetalName { get; set; }
        public string weightInGrammes { get; set; }
        public decimal? metalValuationAmount { get; set; }
        public double? metalUnitRate { get; set; }
        public string preciousMetalFrm { get; set; }
        public decimal? valuationAmount { get; set; }

        // stock
        public int? collateralStockId { get; set; }

        //public string companyName { get; set; }
        public int shareQuantity { get; set; }

        public decimal marketPrice { get; set; }
        public decimal amount { get; set; }
        public decimal sharesSecurityValue { get; set; }
        public decimal shareValueAmountToUse { get; set; }

        // vehicle
        public int collateralVehicleId { get; set; }

        public string vehicleType { get; set; }
        public string vehicleStatus { get; set; }
        public string vehicleMake { get; set; }
        public string modelName { get; set; }
        public string manufacturedDate { get; set; }
        public string registrationNumber { get; set; }
        public string serialNumber { get; set; }
        public string chasisNumber { get; set; }
        public string engineNumber { get; set; }
        public string nameOfOwner { get; set; }
        public string registrationCompany { get; set; }
        public decimal? resaleValue { get; set; }
        public DateTime? valuationDate { get; set; }
        public decimal? lastValuationAmount { get; set; }
        public decimal invoiceValue { get; set; }

        // customer
        //public int collateralCustomerId { get; set; }
        //public int collateralId { get; set; }
        //public int collateralTypeId { get; set; }
        public string collateralType { get; set; }

        //public string collateralCode { get; set; }
        //public short currencyId { get; set; }
        //public string currency { get; set; }
        //public bool allowSharing { get; set; }
        //public bool isLocationBased { get; set; }
        //public int? valuationCycle { get; set; }
        public double hairCut { get; set; }

        //public int customerId { get; set; }
        //public string customerName { get; set; }
        //public string camRefNumber { get; set; }
        //public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }

        public int? actedOnBy { get; set; }
        public string collateralSubType { get; set; }
        public string customerName { get; set; }
        public int relationshipManagerId { get; set; }
        public string relationshipManager { get; set; }
        public string relationshipManagerEmail { get; set; }


        //collateral value calculation ERROR PRONE
        public double securityCollateralValue
        {
            get
            {
                return (collateralValue != null) ? (double)collateralValue - ((double)collateralValue * (float)(haircut * 0.01)) : 0;
            }
        }

        public bool canMappedToApplication { get; set; }
        public bool allowApplicationMapping { get; set; }
        public string bank { get; set; }
        public int stateId { get; set; }

        //File Upload
        public string documentTitle { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] file { get; set; }
        public int? TargetId { get; set; }

        public bool isPrimaryDocument { get; set; }
        public string formData { get; set; }
        public string collateralPrimaryDocumentTitle { get; set; }

        public DateTime nextVisitationDate { get { return lastVisitationDate.AddDays((double)(visitationCycle)); } set { } }
        public DateTime lastVisitationDate { get; set; }
        public int visitationCycle { get; set; }
        public bool requireVisitation { get; set; }
    }

    public class crossGarantee
    {

        public string institutionName { get; set; }
        public string guarantorAddress { get; set; }
        //  public string guarantorReferenceNumber { get; set; }
        public decimal guaranteeValue { get; set; }
        public DateTime? endDate { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string bvn { get; set; }
        public string rcNumber { get; set; }
        public string phoneNumber1 { get; set; }
        public string phoneNumber2 { get; set; }
        public string emailAddress { get; set; }
        public string relationship { get; set; }
        public string relationshipDuration { get; set; }
        public DateTime cStartDate { get; set; }
        public string taxNumber { get; set; }
        public string remark { get; set; }

    }
    public class MiscellaneousNote
    {
        public string labelName { get; set; }
        public string labelValue { get; set; }
        public string controlName { get; set; }
    }

    //public class CollateralCustomerViewModel : GeneralEntity
    //{
    //    public int collateralCustomerId { get; set; }
    //    public int collateralId { get; set; }
    //    public int collateralTypeId { get; set; }
    //    public string collateralType { get; set; }
    //    public string collateralCode { get; set; }
    //    public short currencyId { get; set; }
    //    public string currency { get; set; }
    //    public bool allowSharing { get; set; }
    //    public bool isLocationBased { get; set; }
    //    public int? valuationCycle { get; set; }
    //    public double hairCut { get; set; }
    //    public int customerId { get; set; }
    //    public string customerName { get; set; }
    //    public string camRefNumber { get; set; }
    //    public int approvalStatus { get; set; }
    //    public DateTime? dateActedOn { get; set; }
    //    public int? actedOnBy { get; set; }
    //    public CollateralDepositViewModel collateralDeposit { get; set; }
    //    public CollateralCasaViewModel collateralCasa { get; set; }
    //    public CollateralPlantsAndEquipmentViewModel collateralMachineDetail { get; set; }
    //    public CollateralMarketableSecurityViewModel collateralMarketableSecurity { get; set; }
    //    public CollateralPropertyViewModel collateralProperty { get; set; }
    //    public CollateralSecurityViewModel collateralSecurity { get; set; }
    //    public CollateralPreciousMetalViewModel collateralPreciousMetal { get; set; }
    //    public CollateralInsurancePolicyViewModel collateralInsurancePolicy { get; set; }
    //    public CollateralGauranteeViewModel collateralGaurantee { get; set; }
    //    public CollateralVehicleViewModel collateralVehicle { get; set; }
    //    public CollateralMiscellaneousViewModel collateralMiscellaneous { get; set; }
    //    public CollateralCustomerPolicyViewModel collateralCustomerPolicy { get; set; }
    //}

    public class InsurancePolicies : GeneralEntity
        {
        public string referenceNumber { get; set; }
        public decimal sumInsured { get; set; }
        public string insuranceCompany { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string insuranceType { get; set; }
        public bool hasExpired { get; set; }
        public int collateraalId { get; set; }
        public string collateralCode { get; set; }
        public string collateralType { get; set; }
        public string collateralSubType { get; set; }
        public decimal collateralValue { get; set; }
        public int policyId { get; set; }
        public int collateralTypeId { get; set; }
        public short collateralSubTypeId { get; set; }
        public string customerName { get; set; }
    }

   
    public class AllCollateralViewModel : CollateralViewModel
    {

        public CollateralStockViewModel collateralStock { get; set; }

        public CollateralVehicleViewModel collateralVehicle { get; set; }
        public CollateralDepositViewModel collateralDeposit { get; set; }
        public CollateralCasaViewModel collateralCasa { get; set; }
        public CollateralPlantsAndEquipmentViewModel collateralEquipment { get; set; }
        public CollateralMarketableSecurityViewModel collateralMarketableSecurity { get; set; }
        public CollateralPropertyViewModel collateralProperty { get; set; }
        public CollateralSecurityViewModel collateralSecurity { get; set; }

        public CollateralPreciousMetalViewModel collateralPreciousMetal { get; set; }
        public CollateralInsurancePolicyViewModel collateralInsurancePolicy { get; set; }
        public CollateralGauranteeViewModel collateralGaurantee { get; set; }
        public MiscellaneousNote collateralMiscellaneous { get; set; }
        public List<CollateralCustomerPolicyViewModel> collateralItemPolicy { get; set; }

    }

    public class CollateralDepositViewModel
    {
        public string collateralSubTypeName;

        public int collateralDepositId { get; set; }
        public int collateralCustomerId { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string dealReferenceNumber { get; set; }
        public string accountType { get; set; }
        public string accountNumber { get; set; }
        public decimal existingLienAmount { get; set; }
        public decimal lienAmount { get; set; }
        public decimal availableBalance { get; set; }
        public decimal securityValue { get; set; }
        public DateTime maturityDate { get; set; }
        public decimal maturityAmount { get; set; }
        public string remark { get; set; }
    }

    public class CollateralCasaViewModel
    {
        public string collateralSubTypeName;

        public int collateralCasaId { get; set; }
        public int collateralCustomerId { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string accountNumber { get; set; }
        public bool isOwnedByCustomer { get; set; }
        public short? cashTypeId { get; set; }
        public decimal availableBalance { get; set; }
        public decimal existingLienAmount { get; set; }
        public decimal lienAmount { get; set; }
        public decimal securityValue { get; set; }
        public string remark { get; set; }
    }

    public class CollateralPlantsAndEquipmentViewModel
    {
        public string collateralSubTypeName;
        public string valueBaseTypeName { get; set; }
        public int collateralMachineDetailId { get; set; }
        public int collateralCustomerId { get; set; }
        public short collateralSubTypeId { get; set; }
        public string machineName { get; set; }
        public string description { get; set; }
        public string machineNumber { get; set; }
        public string manufacturerName { get; set; }
        public string yearOfManufacture { get; set; }
        public string yearOfPurchase { get; set; }
        public short valueBaseTypeId { get; set; }
        public string machineCondition { get; set; }
        public string machineryLocation { get; set; }
        public decimal replacementValue { get; set; }
        public string equipmentSize { get; set; }
        public string intendedUse { get; set; }
    }

    public class CollateralMarketableSecurityViewModel
    {
        public string collateralSubTypeName;

        public int collateralMarketableSecurityId { get; set; }
        public int collateralCustomerId { get; set; }
        public short collateralSubTypeId { get; set; }
        public string securityType { get; set; }
        public string dealReferenceNumber { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public decimal dealAmount { get; set; }
        public decimal securityValue { get; set; }
        public decimal lienUsableAmount { get; set; }
        public string issuerName { get; set; }
        public string issuerReferenceNumber { get; set; }
        public decimal unitValue { get; set; }
        public int numberOfUnits { get; set; }
        public short rating { get; set; }
        public short percentageInterest { get; set; }
        public short? interestPaymentFrequency { get; set; }
        public string remark { get; set; }
    }

    public class CollateralPropertyViewModel
    {
        public string collateralSubTypeName;
        public string cityName;
        public string valuerName;

        public int collateralPropertyId { get; set; }
        public int collateralCustomerId { get; set; }
        public short collateralSubTypeId { get; set; }
        public string propertyName { get; set; }
        public int cityId { get; set; }
        public short countryId { get; set; }
        public DateTime? constructionDate { get; set; }
        public string propertyAddress { get; set; }
        public DateTime dateOfAcquisition { get; set; }
        public DateTime lastValuationDate { get; set; }
        public short? valuerId { get; set; }
        public string valuerReferenceNumber { get; set; }
        public short propertyValueBaseTypeId { get; set; }
        public decimal? openMarketValue { get; set; }
        public decimal collateralValue { get; set; }
        public decimal? forcedSaleValue { get; set; }
        public string stampToCover { get; set; }
        public string valuationSource { get; set; }
        public decimal originalValue { get; set; }
        public decimal availableValue { get; set; }
        public decimal securityValue { get; set; }
        public decimal? collateralUsableAmount { get; set; }
        public string remark { get; set; }
        public decimal valuationAmount { get; set; }
    }

    public class CollateralSecurityViewModel
    {
        public int collateralsecurityId { get; set; }
        public int collateralCustomerId { get; set; }
        public string securityType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string securityCode { get; set; }
        public string description { get; set; }
        public string issuerName { get; set; }
        public string issuerRefNo { get; set; }
        public decimal? unitValue { get; set; }
        public int? noofUnits { get; set; }
        public string remark { get; set; }
    }

    public class CollateralPreciousMetalViewModel
    {
        public string collateralSubTypeName;

        public int collateralPreciousMetalId { get; set; }
        public int collateralCustomerId { get; set; }
        public short collateralSubTypeId { get; set; }
        public bool isOwnedByCustomer { get; set; }
        public string preciousMetalName { get; set; }
        public string metalType { get; set; }
        public string weightInGrammes { get; set; }
        public decimal? valuationAmount { get; set; }
        public double? unitRate { get; set; }
        public string preciousMetalForm { get; set; }
        public string remark { get; set; }
    }

    public class CollateralInsurancePolicyViewModel
    {
        public string collateralSubTypeName;
        public string renewalFrequency;

        public int collateralInsurancePolicyId { get; set; }
        public int collateralCustomerId { get; set; }
        public short collateralSubTypeId { get; set; }
        public bool isOwnedByCustomer { get; set; }
        public string insurancePolicyNumber { get; set; }
        public decimal premiumAmount { get; set; }
        public decimal policyAmount { get; set; }
        public string insuranceCompanyName { get; set; }
        public string insurerAddress { get; set; }
        public DateTime policyStartDate { get; set; }
        public DateTime assignDate { get; set; }
        public short? renewalFrequencyTypeId { get; set; }
        public string insurerDetails { get; set; }
        public DateTime policyRenewalDate { get; set; }
        public string remark { get; set; }
    }

    public class CollateralGauranteeViewModel
    {
        public string collateralSubTypeName;

        public int collateralGauranteeId { get; set; }
        public int collateralCustomerId { get; set; }
        public short collateralSubTypeId { get; set; }
        public bool? isOwnedByCustomer { get; set; }
        public string institutionName { get; set; }
        public string guarantorAddress { get; set; }
        public string guarantorReferenceNumber { get; set; }
        public string guaranteeType { get; set; }
        public decimal guaranteeValue { get; set; }
        public DateTime startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string remark { get; set; }
    }

    public class CollateralVehicleViewModel
    {
        public string collateralSubTypeName;

        public int collateralVehicleId { get; set; }
        public int collateralCustomerId { get; set; }
        public string vehicleType { get; set; }
        public short collateralSubTypeId { get; set; }
        public string vehicleStatus { get; set; }
        public string vehicleMake { get; set; }
        public string modelName { get; set; }
        public string manufacturedDate { get; set; }
        public string registrationNumber { get; set; }
        public string serialNumber { get; set; }
        public string chasisNumber { get; set; }
        public string engineNumber { get; set; }
        public string nameOfOwner { get; set; }
        public string registrationCompany { get; set; }
        public decimal? resaleValue { get; set; }
        public DateTime? valuationDate { get; set; }
        public decimal? lastValuationAmount { get; set; }
        public decimal invoiceValue { get; set; }
        public string remark { get; set; }
    }

    public class CollateralMiscellaneousViewModel
    {
        public int collateralMiscellaneousId { get; set; }
        public int collateralCustomerId { get; set; }
        public bool isOwnedByCustomer { get; set; }
        public string nameOfSecurity { get; set; }
        public decimal securityValue { get; set; }
        public string note { get; set; }
        public List<CollateralMiscellaneousNotesViewModel> collateralMiscellaneousNotes { get; set; }
    }

    public class CollateralMiscellaneousNotesViewModel
    {
        public int miscellaneousNoteId { get; set; }
        public int? miscellaneousId { get; set; }
        public string columnName { get; set; }
        public string columnValue { get; set; }
    }

    public class CollateralStockViewModel
    {
        public short collateralSubTypeId;
        public string collateralSubTypeName;

        public int collateralStockId { get; set; }

        public int collateralCustomerId { get; set; }

        public string companyName { get; set; }

        public int shareQuantity { get; set; }

        public decimal marketPrice { get; set; }

        public decimal amount { get; set; }

        public decimal shareSecurityValue { get; set; }

        public decimal shareValueAmountToUse { get; set; }

    }
    //public class CollateralDocumentViewModel
    //{
    //    public long documentId { get; set; }
    //    public int collateralCustomerId { get; set; }
    //    public string documentCategory { get; set; }
    //    public string documentRef { get; set; }
    //    public string documentCode { get; set; }
    //    public string documentType { get; set; }
    //    public bool isMandatory { get; set; }
    //    public string remark { get; set; }
    //}

    public class CollateralValueBaseTypeViewModel : GeneralEntity
    {
        public short collateralValueBaseTypeId { get; set; }
        public string valueBaseTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public string remark { get; set; }
    }

    public class CollateralValuersViewModel : GeneralEntity
    {
        public short collateralValuerId { get; set; }
        public string valuerLicenceNumber { get; set; }
        public string name { get; set; }
        public short? valuerTypeId { get; set; }
        public short? cityId { get; set; }
        public string cityName { get; set; }
        public short? countryId { get; set; }
        public string countryName { get; set; }
        public string accountNumber { get; set; }
        public string valuerBVN { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
    }

    public class CollateralPerfectionStatusViewModel : GeneralEntity
    {
        public byte perfectionStatusId { get; set; }
        public string perfectionStatusName { get; set; }
    }
    public class CollateralValuerTypeViewModel : GeneralEntity
    {
        public short valuerTypeId { get; set; }
        public string valuerTypeName { get; set; }
    }

    public class CollateralCustomerPolicyViewModel
    {
        public int policyId { get; set; }
        public int collateralCustomerId { get; set; }
        public string policyReferenceNumber { get; set; }
        public string insuranceCompanyName { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }

    public class CollateralSubTypeViewModel : GeneralEntity
    {
        public string collateralTypeName { get; set; }

        public short collateralSubTypeId { get; set; }
        public string collateralSubTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public double haircut { get; set; }
        public int revaluationDuration { get; set; }
        public bool isLocationBased { get; set; }
        public bool allowSharing { get; set; }
    }

    public class CustomerCollateralSearch
    {
        public int customerId { get; set; }
        public string firstName { get; set; }
        public string customerCode { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string accountNumber { get; set; }
        public string currency { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string customerName { get { return $"{this.firstName} {this.lastName}"; } }
        public CollateralViewModel customerCollateral { get; set; }
    }

    public class CollateralSearchViewModel
    {
        public int collateralId { get; set; }
        public int customerId { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public int currencyId { get; set; }
        public string currencyCode { get; set; }
        public string collateralCode { get; set; }
        public bool allowSharing { get; set; }
        public bool isLocationBased { get; set; }
        public int? valuationCycle { get; set; }
        public double haircut { get; set; }

    }

    public class ActiveCustomerCollateralViewModel : GeneralEntity
    {
        public int? customerId { get; set; }
        public int collateralCustomerId { get; set; }
        public int collateralTypeId { get; set; }
        public short collateralSubTypeId { get; set; }
        public short currencyId { get; set; }
        public short productId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public int relationshipManagerId { get; set; }
        public int loanCollateralMappingId { get; set; }
        public int loanId { get; set; }
        public int loanApplicationId { get; set; }
        public short productTypeId { get; set; }
        public string customerCode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string occupation { get; set; }
        public string collateralCode { get; set; }
        public bool allowSharing { get; set; }
        public bool isLocationBased { get; set; }
        public int? valuationCycle { get; set; }
        public double hairCut { get; set; }
        public string applicationReferenceNumber { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal principalAmount { get; set; }
        public double interestRate { get; set; }
        public double exchangeRate { get; set; }
        public int tenor { get; set; }
        public string loanInformation { get; set; }
        public bool isInvestmentGrade { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public bool isReleased { get; set; }
        public short releaseApprovalStatusId { get; set; }
        public decimal collateralValue { get; set; }
    }

    public class CollateralLoanApplication
    {
        public int loanApplicationCollateralId { get; set; }
        public double haircut { get; set; }
        public string collateralCode { get; set; }
        public double collateralValue { get; set; }
        public int collateralId { get; set; }
        public string collateralType { get; set; }
        public double securityValue { get { return collateralValue - (haircut * 0.01 * collateralValue); } }
    }

    public class ApplicationCollateralMapping
    {
        public int? collateralId { get; set; }
        public int applicationId { get; set; }
        public int? applicationDetailId { get; set; }
        public int staffId { get; set; }
        public string collateralCode { get; set; }
    }

    public class CollateralHistoryList
    {
        public string customerName { get; set; }
        public string usedBy { get; set; }
        public string loanRef { get; set; }
        public DateTime expirationDate { get; set; }
        public decimal collateralValue { get; set; }
        public decimal amountInUse { get; set; }
        public decimal collateralBalance { get; set; }
        public DateTime dateUsed { get; set; }
        public double haircut { get; set; }
        public double exchangeRate { get; set; }
        public decimal approvedLoanAmount { get; set; }
        public decimal haircutValue { get; set; }
        public decimal runningPrincipal { get; set; }
        public decimal principalAmount { get; set; }
        public decimal outstandingPrincipal { get; set; }
    }

    public class CollateralHistory
    {
        public IEnumerable<CollateralHistoryList> usage { get; set; }

        public decimal collateralValue { get; set; }
        public decimal totalAmountUsedByOutstanding { get; set; }
        public decimal totalAmountUsedByPrincipal { get; set; }
        public decimal availableValueByOutstanding { get; set; }
        public decimal availableValueByPrincipal { get; set; }
    }

    public class StockCompanyViewModel
    {
        public int stockId { get; set; }
        public string stockCode { get; set; }
        public string stockName { get; set; }
        public int MyProperty { get; set; }
        public int stockPriceId { get; set; }
        public decimal stockPrice { get; set; }
    }

    public class loanApplicationColateralViewModel
    {
        public int loanApplicationCollateralId { get; set; }
        public int collateralCustomerId { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public bool legalFeeTaken { get; set; }
        public decimal legalFeeAmount { get; set; }
        public DateTime legalFeeDate { get; set; }
        public string collateralTypeName { get; set; }
        public decimal collateralValue { get; set; }
        public double hairCut { get; set; }
        public decimal valuationCycle { get; set; }
        public int currencyId { get; set; }
        public string currencyCode { get; set; }
        public string currency { get; set; }
    }
}
