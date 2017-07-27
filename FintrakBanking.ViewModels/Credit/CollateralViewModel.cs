using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{

    public class CollateralViewModel : GenaralEntity
    {
        public int collateralCustomerId { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralTypeName { get; set; }
        public string collateralCode { get; set; }
        public decimal collateralValue { get; set; }
        public short? currencyId { get; set; }
        public decimal? limitContribution { get; set; }
        public DateTime collateralValueDate { get; set; }
        public int? graceDays { get; set; }
        public int? quantity { get; set; }
        public int? chargeTypeId { get; set; }
        public short? seniorityOfClaimId { get; set; }
        public decimal? lendableMargin { get; set; }
        public int customerId { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime? revisionDate { get; set; }
        public string camRefNumber { get; set; }
        public bool requireFieldInvestigation { get; set; }
        public bool requireValuation { get; set; }
        public bool requireCheck { get; set; }
        public bool allowShare { get; set; }
        public DateTime? revaluationDate { get; set; }
        public DateTime? lastValuationDate { get; set; }
        public string valuationSource { get; set; }
        public decimal? valuationAmount { get; set; }
        public double? haircut { get; set; }
        public bool releaseCollateral { get; set; }
        public DateTime? releaseDate { get; set; }
        public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }
        public int? actedOnBy { get; set; }
        public CollateralDepositViewModel collateralDeposit { get; set; }
        public CollateralCasaViewModel collateralCasa { get; set; }
        public CollateralMachineDetailViewModel collateralMachineDetail { get; set; }
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
        public string accountType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string accountNumber { get; set; }
        public decimal? accountBalance { get; set; }
        public string contribution { get; set; }
        public string remark { get; set; }
    }

    public class CollateralCasaViewModel
    {
        public int collateralCasaId { get; set; }
        public int collateralCustomerId { get; set; }
        public string accountType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string accountNumber { get; set; }
        public decimal? accountBalance { get; set; }
        public string contribution { get; set; }
        public DateTime? maturityDate { get; set; }
        public string remark { get; set; }
    }

    public class CollateralMachineDetailViewModel 
    {
        public int collateralMachineDetailId { get; set; }
        public int collateralCustomerId { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string machineName { get; set; }
        public string manufacturer { get; set; }
        public string manufacturedYear { get; set; }
        public string purchasedYear { get; set; }
        public short machineValueBaseTypeId { get; set; }
        public string machineryLocation { get; set; }
        public decimal? replacementValue { get; set; }
        public decimal? thirdPartyChargeAmount { get; set; }
        public string machineryCondition { get; set; }
        public string intendedUse { get; set; }
    }

    public class CollateralMarketableSecurityViewModel 
    {
        public int collateralMarketableSecurityId { get; set; }
        public int collateralCustomerId { get; set; }
        public string securityType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string securityCode { get; set; }
        public string description { get; set; }
        public string issuerName { get; set; }
        public string issuerReferenceNumber { get; set; }
        public decimal? unitValue { get; set; }
        public int? numberOfUnits { get; set; }
        public string remark { get; set; }
    }

    public class CollateralPropertyViewModel 
    {
        public int collateralPropertyId { get; set; }
        public int collateralCustomerId { get; set; }
        public string propertyType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string collateralSubTypeName { get; set; }
        public int revaluationDuration { get; set; }
        public int cityId { get; set; }
        public short countryId { get; set; }
        public string propertyAddress { get; set; }
        public DateTime? constructionDate { get; set; }
        public DateTime? purchaseDate { get; set; }
        public string zoneClassification { get; set; }
        public short? propertyValueBaseTypeId { get; set; }
        public DateTime? lastValuationDate { get; set; }
        public string valuationSource { get; set; }
        public decimal? valuationAmount { get; set; }
        public decimal? otherLendersChargeAmount { get; set; }
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
        public string preciousMetal { get; set; }
        public string metalType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string weightInGrammes { get; set; }
        public decimal? valuationAmount { get; set; }
        public double? unitRate { get; set; }
        public string preciousMetalForm { get; set; }
        public string notes { get; set; }
        public string remark { get; set; }
    }

    public class CollateralInsurancePolicyViewModel 
    {
        public int collateralInsurancePolicyId { get; set; }
        public int collateralCustomerId { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string policyNumber { get; set; }
        public decimal insuranceAmount { get; set; }
        public DateTime startDate { get; set; }
        public decimal premiumAmount { get; set; }
        public DateTime? assignmentDate { get; set; }
        public string insurerAddress { get; set; }
        public string insurerDetails { get; set; }
        public short? renewalFrequencyTypeId { get; set; }
        public DateTime? nextRenewalDate { get; set; }
        public string remark { get; set; }
    }

    public class CollateralGauranteeViewModel 
    {
        public int collateralGauranteeId { get; set; }
        public int collateralCustomerId { get; set; }
        public string guaranteeType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public decimal guaranteeAmount { get; set; }
        public string guarantorCifnumber { get; set; }
        public string guarantorName { get; set; }
        public string guarantorAddress { get; set; }
        public DateTime agreementDate { get; set; }
        public string continuingGuarantee { get; set; }
        public string guarantorOwnExposure { get; set; }
        public decimal totalGuaranteeAmount { get; set; }
        public bool revokeable { get; set; }
        public DateTime? revokeDate { get; set; }
        public byte? rating { get; set; }
        public string remark { get; set; }
        
    }

    public class CollateralVehicleViewModel 
    {
        public int collateralVehicleId { get; set; }
        public int collateralCustomerId { get; set; }
        public string vehicleType { get; set; }
        public short? collateralSubTypeId { get; set; }
        public string newOrUsed { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string year { get; set; }
        public string registrationNumber { get; set; }
        public string chasisNumber { get; set; }
        public string engineNumber { get; set; }
        public string owner { get; set; }
        public string regAuthority { get; set; }
        public decimal? resaleValue { get; set; }
        public DateTime? valuationDate { get; set; }
        public decimal? valuationAmount { get; set; }
        public decimal invoiceValue { get; set; }
        public string remark { get; set; }
    }

    public class CollateralMiscellaneousViewModel 
    {
        public int collateralMiscellaneousId { get; set; }
        public int collateralCustomerId { get; set; }
        public string collateralDescription { get; set; }
        public int units { get; set; }
        public decimal unitValue { get; set; }
        public string remark { get; set; }
        public List<collateralMiscellaneousNotesViewModel> collateralMiscellaneousNotes { get; set; }
    }

    public class collateralMiscellaneousNotesViewModel : GenaralEntity
    {
        public int miscellaneousNoteId { get; set; }
        public int? miscellaneousId { get; set; }
        public string columnName { get; set; }
        public string columnValue { get; set; }
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
        public string policyNumber { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string insuranceCompany { get; set; }

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
