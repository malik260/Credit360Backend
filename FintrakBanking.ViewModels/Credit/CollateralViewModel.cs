using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{

    public class CollateralViewModel : GenaralEntity
    {
        public CollateralCustomer collateralCustomer { get; set; }
        public CollateralDeposit collateralDeposit { get; set; }
        public CollateralMachineDetail collateralMachineDetail { get; set; }
        public CollateralMarketableSecurity collateralMarketableSecurity  { get; set; }
        public CollateralProperty collateralProperty { get; set; }
        public CollateralSecurity collateralSecurity  { get; set; }
        public CollateralPreciousMetal collateralPreciousMetal { get; set; }
        public CollateralInsurancePolicy collateralInsurancePolicy  { get; set; }
        public CollateralGaurantee collateralGaurantee  { get; set; }
        public CollateralVehicle collateralVehicle  { get; set; }
        public CollateralMiscellaneous collateralMiscellaneous  { get; set; }
        public CollateralMiscNotes collateralMiscNotes  { get; set; }
    }

    public class CollateralCustomer : GenaralEntity
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
        public short? chargeTypeId { get; set; }
        public short? seniorityOfClaimId { get; set; }
        public decimal? lendableMargin { get; set; }
        public int customerId { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime? revisionDate { get; set; }
        public string camrefNo { get; set; }
        public bool requireFieldInvestigation { get; set; }
        public bool requireValuation { get; set; }
        public bool requireCheck { get; set; }
        public bool allowShare { get; set; }
        public DateTime? revaluationDate { get; set; }
        public DateTime? lastValuationDate { get; set; }
        public string valuationSource { get; set; }
        public decimal? valuationAmount { get; set; }
        public bool releaseCollateral { get; set; }
        public DateTime? releaseDate { get; set; }
        public int approvalStatus { get; set; }
        public DateTime? dateActedOn { get; set; }
        public int? actedOnBy { get; set; }
    }

    public class CollateralDeposit : GenaralEntity
    {
        public int termDepositTranAccId { get; set; }
        public int collateralCustomerId { get; set; }
        public string accountType { get; set; }
        public string accountNo { get; set; }
        public decimal accountBalance { get; set; }
        public string contribution { get; set; }
        public DateTime? maturityDate { get; set; }
        public string remark { get; set; }
       
    }

    public class CollateralMachineDetail : GenaralEntity
    {
        public int machineDetailId { get; set; }
        public int collateralCustomerId { get; set; }
        public string machineDetails { get; set; }
        public string manufacturer { get; set; }
        public string manufacturedYear { get; set; }
        public string purchasedYear { get; set; }
        public byte machineValueBaseId { get; set; }
        public decimal? invoiceValue { get; set; }
        public decimal? writtenDownValue { get; set; }
        public decimal? assessedValue { get; set; }
        public string machineryLocation { get; set; }
        public decimal? replacementValue { get; set; }
        public decimal? thirdPartyChargeAmount { get; set; }
        public string machineryCondition { get; set; }
        public string intendedUse { get; set; }
    }

    public class CollateralMarketableSecurity : GenaralEntity
    {
        public int securityTypeId { get; set; }
        public int collateralCustomerId { get; set; }
        public string securityType { get; set; }
        public string securityCode { get; set; }
        public string description { get; set; }
        public string issuerName { get; set; }
        public string issuerRefNo { get; set; }
        public decimal? unitValue { get; set; }
        public int? noofUnits { get; set; }
        public string remark { get; set; }
    }

    public class CollateralProperty : GenaralEntity
    {
        public int propertyTypeId { get; set; }
        public int collateralCustomerId { get; set; }
        public string propertyType { get; set; }
        public int cityId { get; set; }
        public short countryId { get; set; }
        public string propertyAddress { get; set; }
        public DateTime? constructionDate { get; set; }
        public DateTime? purchaseDate { get; set; }
        public string zoneClassification { get; set; }
        public byte? propertyValueBaseTypeId { get; set; }
        public decimal? marketValue { get; set; }
        public decimal? govtValue { get; set; }
        public decimal? propertyIndexValue { get; set; }
        public double haircut { get; set; }
        public DateTime? lastValuationDate { get; set; }
        public string valuationSource { get; set; }
        public decimal? valuationAmount { get; set; }
        public decimal? otherLendersChargeAmount { get; set; }
        public string remark { get; set; }
    }

    public class CollateralSecurity : GenaralEntity
    {
        public int securityTypeId { get; set; }
        public int collateralCustomerId { get; set; }
        public string securityType { get; set; }
        public string securityCode { get; set; }
        public string description { get; set; }
        public string issuerName { get; set; }
        public string issuerRefNo { get; set; }
        public decimal? unitValue { get; set; }
        public int? noofUnits { get; set; }
        public string remark { get; set; }
    }

    public class CollateralPreciousMetal : GenaralEntity
    {
        public int preciousMetalId { get; set; }
        public int collateralCustomerId { get; set; }
        public string preciousMetal { get; set; }
        public string metalType { get; set; }
        public string weighInGms { get; set; }
        public decimal? valuationAmount { get; set; }
        public double? unitRate { get; set; }
        public string preciousMetalForm { get; set; }
        public string notes { get; set; }
        public string remark { get; set; }
    }

    public class CollateralInsurancePolicy : GenaralEntity
    {
        public int insurancePolicyId { get; set; }
        public int collateralCustomerId { get; set; }
        public string policyNo { get; set; }
        public decimal insuranceAmount { get; set; }
        public DateTime startDate { get; set; }
        public decimal premiumAmount { get; set; }
        public DateTime? assignmentDate { get; set; }
        public string insurerAddress { get; set; }
        public string insurerDetails { get; set; }
        public short?  renewalFrequencyId { get; set; }
        public DateTime? nextRenewalDate { get; set; }
        public string remark { get; set; }
    }

    public class CollateralGaurantee : GenaralEntity
    {
        public int gauranteeId { get; set; }
        public int collateralCustomerId { get; set; }
        public string guaranteeType { get; set; }
        public decimal guaranteeAmount { get; set; }
        public string guarantorCifno { get; set; }
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

    public class CollateralVehicle : GenaralEntity
    {
        public int vehicleTypeId { get; set; }
        public int collateralCustomerId { get; set; }
        public string vehicleType { get; set; }
        public string newOrused { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string year { get; set; }
        public string regnNo { get; set; }
        public string chasisNo { get; set; }
        public string engineNo { get; set; }
        public string owner { get; set; }
        public string regAuthority { get; set; }
        public decimal? resaleValue { get; set; }
        public DateTime? valuationDate { get; set; }
        public decimal? valuationAmount { get; set; }
        public decimal invoiceValue { get; set; }
        public string remark { get; set; }
    }

    public class CollateralMiscellaneous : GenaralEntity
    {

        public int miscellaneousId { get; set; }
        public int collateralCustomerId { get; set; }
        public string collateralDesc { get; set; }
        public int units { get; set; }
        public decimal unitValue { get; set; }
        public string remarks { get; set; }
       
    }

    public class CollateralMiscNotes : GenaralEntity
    {
        public int miscNoteId { get; set; }
        public int? miscellaneousId { get; set; }
        public string columnName { get; set; }
        public string columnValue { get; set; }
    }



   
}
