using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{

    public class CollateralViewModel : GenaralEntity
    {
        public int collateralCustomerId { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralCode { get; set; }
        public short currencyId { get; set; }
        public bool allowSharing { get; set; }
        public bool isLocationBased { get; set; }
        public int? valuationCycle { get; set; }
        public double hairCut { get; set; }
        public int customerId { get; set; }
        public string camRefNumber { get; set; }
        public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }
        public int? actedOnBy { get; set; }
        public CollateralDepositViewModel collateralDeposit { get; set; }
        public CollateralCasaViewModel collateralCasa { get; set; }
        public CollateralPlantsAndEquipmentViewModel collateralMachineDetail { get; set; }
        public CollateralMarketableSecurityViewModel collateralMarketableSecurity  { get; set; }
        public CollateralPropertyViewModel collateralProperty { get; set; }
        public CollateralSecurityViewModel collateralSecurity  { get; set; }
        public CollateralPreciousMetalViewModel collateralPreciousMetal { get; set; }
        public CollateralInsurancePolicyViewModel collateralInsurancePolicy  { get; set; }
        public CollateralGauranteeViewModel collateralGaurantee  { get; set; }
        public CollateralVehicleViewModel collateralVehicle  { get; set; }
        public CollateralMiscellaneousViewModel collateralMiscellaneous  { get; set; }
        public CollateralCustomerPolicyViewModel collateralCustomerPolicy { get; set; }
    }

    public class CollateralDepositViewModel 
    {
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
        public decimal? securityValue { get; set; }
        public decimal? collateralUsableAmount { get; set; }
        public string remark { get; set; }
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

    public class CollateralDocumentViewModel
    {
        public long documentId { get; set; }
        public int collateralCustomerId { get; set; }
        public string documentCategory { get; set; }
        public string documentRef { get; set; }
        public int documentCode { get; set; }
        public string documentType { get; set; }
        public bool isMandatory { get; set; }
        public string remark { get; set; }
    }

    public class CollateralValueBaseTypeViewModel : GenaralEntity
    {
        public short collateralValueBaseTypeId { get; set; }
        public string valueBaseTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public string remark { get; set; }
    }

    public class CollateralValuersViewModel : GenaralEntity
    {
        public short collateralValuerId { get; set; }
        public string valuerLicenceNumber { get; set; }
        public string name { get; set; }
        public short? valuerTypeId { get; set; }
        public short? cityId { get; set; }
        public string cityName { get; set; }
        public short? countryId { get; set; }
        public string countryName { get; set; }
    }

    public class CollateralValuerTypeViewModel : GenaralEntity
    {
        public short collateralValuerTypeId { get; set; }
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

    public class CollateralSubTypeViewModel
    {
        public short collateralSubTypeId { get; set; }
        public string collateralSubTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public double haircut { get; set; }
        public int revaluationDuration { get; set; }
    }
}
