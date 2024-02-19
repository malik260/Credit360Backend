using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralViewModel : GeneralEntity 
    {
        public bool requireInsurancePolicy;
        public int isRegistrationDoneViaLoanApplication;
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
        public int? customerId { get; set; }
        public string customerCode { get; set; }
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public string baseCurrencyCode { get; set; }
        public string currency { get; set; }
        public string collateralCode { get; set; }
        public string camRefNumber { get; set; }
        public bool allowSharing { get; set; }
        public bool? isLocationBased { get; set; }
        public int? valuationCycle { get; set; }
        public double haircut { get; set; }
        public bool hasInsurance { get; set; }
        public string collateralSubTypeName { get; set; }
        public string fundName { get; set; }
        public string insuranceType { get; set; }
        public string metalType { get; set; }
        public string machineType { get; set; }
        public decimal collateralValueLcy { get; set; }
        public double exchangeRate { get; set; }
        public string capturedBy { get; set; }
        public string address { get; set; }

        public DateTime? validTill { get; set; }
        public int? customerGroupId { get; set; }
        public int loanTypeId { get; set; }
        

        // presentation 


        // insurance
        public string referenceNumber { get; set; }
        public decimal sumInsured { get; set; }
        public string insuranceCompany { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public InsurancePolicy insurancePolicy { get; set; }
        public int policyId { get; set; }
        public int approvalStatusName { get; set; }
        public string approvalStatus { get; set; }
        public List<InsurancePolicy> insurancePolicies { get; set; }
        public List<CollateralDocumentViewModel> documents { get; set; }

        public List<CollateralDocumentViewModel> collateralVisitation { get; set; }
        // deposit
        public int collateralDepositId { get; set; }

        public string dealReferenceNumber { get; set; }
        public string accountNumber { get; set; }
        public decimal existingLienAmount { get; set; }
        public decimal lienAmount { get; set; }
        public decimal availableBalance { get; set; }
        public decimal? securityValue { get; set; }
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
        public string note { get; set; }

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
        public int? age { get; set; }

        // casa
        public int collateralCasaId { get; set; }

        public bool isOwnedByCustomer { get; set; }

        // immovableProperty
        public int collateralPropertyId { get; set; }

        public string propertyName { get; set; }
        public int cityId { get; set; }
        public DateTime? constructionDate { get; set; }
        public string propertyAddress { get; set; }
        public DateTime dateOfAcquisition { get; set; }
        public DateTime lastValuationDate { get; set; }
        public short? valuerId { get; set; }
        public string valuerReferenceNumber { get; set; }
        public short propertyValueBaseTypeId { get; set; }
        public string propertyValueBaseTypeName { get; set; }

        public decimal? openMarketValue { get; set; }
        public decimal? collateralValue { get; set; }
        public decimal? forcedSaleValue { get; set; }
        public string stampToCover { get; set; }
        public string valuationSource { get; set; }
        public decimal originalValue { get; set; }
        public decimal availableCollateralValue { get; set; }
        public decimal? collateralUsableAmount { get; set; }
        public string nearestLandMark { get; set; }
        public string nearestBusStop { get; set; }
        public double? longitude { get; set; }
        public double? latitude { get; set; }
        public byte? perfectionStatusId { get; set; }

        public bool? isAssetPledgedByThirdParty { get; set; }

        public string thirdPartyName { get; set; }


        public int? localGovernmentId { get; set; }

        public bool? isAssetManagedByTrustee { get; set; }

        public string trusteeName { get; set; }

        public decimal? bankShareOfCollateral { get; set; }

        public string perfectionStatusReason { get; set; }
        public bool? isOwnerOccupied { get; set; }
        public bool? isResidential { get; set; }

        public string valuerName { get; set; }
        public string valuerAccountNumber { get; set; }
        public DateTime nextValuationDate { get; set; }

        //public string thirdPartyName { get; set; }

        //public decimal forcedSaleValue { get; set; }
        //public decimal estimatedValue { get; set; }
        //public decimal securityValue { get; set; }


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
        public decimal? inSurPremiumAmount { get; set; }
        public decimal policyAmount { get; set; }
        public string insuranceCompanyName { get; set; }
        public string insurerAddress { get; set; }
        public DateTime policyStartDate { get; set; }
        public DateTime assignDate { get; set; }
        public short? renewalFrequencyTypeId { get; set; }
        public string renewalFrequencyTypeName { get; set; }

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
        public DateTime? manufacturedDate { get; set; }
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
        public int? documentTypeId { get; set; }
        public string documentType { get; set; }

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
        //public double securityCollateralValue
        //{
        //    get
        //    {
        //      //  return (collateralValue != null) ? (double)collateralValue - ((double)collateralValue * (float)(haircut * 0.01)) : 0;
        //    }
        //}

       // public decimal availableSecurityValue { get { return (decimal)collateralValue - valueInUse; } }

        public bool canMappedToApplication { get; set; }
        public bool allowApplicationMapping { get; set; }
        public string bank { get; set; }
        //public decimal bankShareOfCollateral { get; set; }
        public int? stateId { get; set; }

        //File Upload
        public int documentId { get; set; }

        public string documentTitle { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] file { get; set; }
        public int? TargetId { get; set; }

        public bool isPrimaryDocument { get; set; }
        public string formData { get; set; }
        public string collateralPrimaryDocumentTitle { get; set; }
        public string comment { get; set; }
        public int? releaseType { get; set; }
        public short? approvalStatusId { get; set; }

        public DateTime nextVisitationDate { get { return lastVisitationDate.AddDays((double)(visitationCycle)); } set { } }
        
        public DateTime lastVisitationDate { get; set; }
        public int visitationCycle { get; set; }
        public bool requireVisitation { get; set; }
        public decimal valueInUse { get; set; }
        public string cityName { get; set; }
        public string countryName { get; set; }
        public string collateralValuer { get; set; }
        public string propertyBaseType { get; set; }
        public string perfectionStatusName { get; set; }
        public decimal stampToCoverAmount { get; set; }
        //public decimal stampToCover { get; set; }
        public short baseCurrency { get; set; }
        public short baseCurrencyId { get; set; }
        public bool disAllowCollateral { get; set; }
        public string policyinsuranceType { get; set; }
        public DateTime dateOfManufacture { get; set; }


        public int collateralPromissoryId { get; set; }
        public string promissoryNoteRefferenceNumber { get; set; }
        public decimal promissoryValue { get; set; }
        public DateTime promissoryEffectiveDate { get; set; }
        public DateTime promissoryMaturityDate { get; set; }

       public int collateralReleaseId { get; set; }
        public int collateralReleaseTypeId { get; set; }
        public string collateralReleaseTypeName { get; set; }
        public bool? jobRequestSent { get; set; }

        public int? collateralReleaseStatusId { get; set; }
        public string collateralReleaseStatusName { get; set; }



        public bool available
        {
            get
            {
                return availableCollateralValue > 0 ? true : false;
            }
        }

        public string accountName { get; set; }
        public string accountNameToDeposit { get; set; }
        public string accountNumberToDeposit { get; set; }
        public decimal? regularPaymentAmount { get; set; }
        public string payer { get; set; }
        public string contractDetail { get; set; }
        public string contractEmployer { get; set; }
        public decimal? contractValue { get; set; }
        public decimal? outstandingInvoiceAmount { get; set; }
        public string accountNumberToDebit { get; set; }
        public string invoiceNumber { get; set; }
        public DateTime? invoiceDate { get; set; }
        public decimal? monthlySalary { get; set; }
        public decimal? annualAllowances { get; set; }
        public decimal? annualEmolument { get; set; }
        public decimal? annualSalary { get; set; }
        public int collateralISPOId { get; set; }
        public int collateralDomiciliationId { get; set; }
        public int collateralIndemnityId { get; set; }
        public string accountNameToDebit { get; set; }
        public string stateName { get; set; }
        public string localGovtName { get; set; }
        public string interval { get; set; }
        public string relatedCollateralCode { get; set; }
        public decimal? estimatedValue { get; set; }
        public int registrationTypeId { get; set; }
        public int loanApplicationCustomerId { get; set; }
        public int applicationCustomerId { get; set; }
        public int? loanApplicationId { get; set; }
        public int? loanApplicationDetailId { get; set; }
        public int? collateralUsageStatus { get; set; }
        public int? status { get; set; }
        public int? premiumPercent { get; set; }
        public int insuranceTypeId { get; set; }
        public int insuranceCompanyId { get; set; }
        public string prevoiusInsurance { get; set; }
        public int requestNumber { get; set; }
        public int insuranceRequestId { get; set; }
        public string statusName { get; set; }
        public string requestReason { get; set; }
        public string requestComment { get; set; }
        public bool isMapped { get; set; }
        public bool isProposed { get; set; }
        public string collateralSummary { get; set; }
        public int? loopedStaffId { get; set; }
        public string lastApprovalComment { get; set; }
        public int loanApplicationId2 { get; set; }
        public decimal facilityAmount { get; set; }
        public string customerAccount { get; set; }
        public int revaluationDuration { get; set; }
        public int approvalTrailId { get; set; }
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

    public class InsurancePolicy : GeneralEntity
    {
        public string referenceNumber { get; set; }
        public decimal? sumInsured { get; set; }
        public string insuranceCompany { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string insuranceType { get; set; }

        public bool hasExpired { get; set; }
        public int? collateraalId { get; set; }
        public string collateralCode { get; set; }
        public string collateralType { get; set; }
        public string collateralSubType { get; set; }
        public decimal? collateralValue { get; set; }
        public int? policyId { get; set; }
        public int? collateralTypeId { get; set; }
        public int? collateralSubTypeId { get; set; }
        public string customerName { get; set; }
        public decimal? premiumAmount { get; set; }
        public decimal? inSurPremiumAmount { get; set; }
        public string description { get; set; }
        public int? premiumPercent { get; set; }
        public int? insuranceTypeId { get; set; }
        public int? insuranceCompanyId { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public int? operationId { get; set; }
        public int requestNumber { get; set; }
        public int insuranceRequestId { get; set; }
        public double haircut { get; set; }
        public string collateralReleaseStatusName { get; set; }
        public int? collateralUsageStatus { get; set; }
        public string requestReason { get; set; }
        public string requestComment { get; set; }
        public short? approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public string currentApprovalLevel { get; set; }
        public int customerId { get; set; }
        public bool differInsurancePolicy { get; set; }
        public string policyStateId { get; set; }
        public string companyAddress { get; set; }
        public int? prevoiusInsuranceId { get; set; }
        public int approvalTrailId { get; set; }
        public int targetId { get; set; }
        public DateTime arrivalTime { get; set; }
        public string accountOfficer { get; set; }
        public int? insurancePolicyTypeId { get; set; }
        public string insurancePolicyType { get; set; }
        public string insuranceStatus { get
            {
                if (this.expiryDate > DateTime.Now)
                {
                    return "Active";
                }
                else
                {
                    return "Expired";
                }
            }
               set { } }

        public string collateralDetails { get; set; }
        public DateTime? valuationStartDate { get; set; }
        public DateTime? valuationEndDate { get; set; }
        public decimal? omv { get; set; }
        public decimal? fsv { get; set; }
        public string valuer { get; set; }
        public int? insuranceStatusId { get; set; }
        public int collateralInsuranceTrackingId { get; set; }
        public string isInformationConfirmed { get; set; }
        public int? loanApplicationDetailId { get; set; }
        public int? customerGroupId { get; set; }
        public string insuranceCompanyName { get; set; }
        public int? valuerId { get; set; }
        public string gpsCoordinates { get; set; }
        public decimal? loanAmount { get; set; }
        public string loanStatus { get; set; }
        public string loanTypeName { get; set; }
        public string securityReleaseStatus { get; set; }
        public string taxNumber { get; set; }
        public string rcNumber { get; set; }
        public string firstLossPayee { get; set; }
        public decimal? insurableValue { get; set; }
        public string customerCode { get; set; }
        public string customerAccount { get; set; }
        public string customerPhone { get; set; }
        public string customerAddress { get; set; }
        public string teamName { get; set; }
        public string divisionName { get; set; }
        public string groupHead { get; set; }
        public string customerEmail { get; set; }
        public string accountOfficerName { get; set; }
        public string accountOfficerEmail { get; set; }
        public string valueCode { get; set; }
        public bool isPolicyInformationConfirmed { get; set; }
        public string applicationreferenceNumber { get; set; }
        public string otherInsuranceCompany { get; set; }
        public string otherValuers { get; set; }
        public int? collateralCustomerId { get; set; }
        public string otherInsurancePolicyType { get; set; }
        public string confirmedAdmin { get; set; }
        public int? businessUnitId { get; set; }
    }

    public class NewCollateralViewModel
    {
        public int? applicationId { get; set; }
        public int customerId { get; set; }
    }

    public class InsurancePolicyRecordViewModel
    {
        public bool isPolicyInformationConfirmed { get; set; }
        public string isInformationConfirmed { get; set; }
    }

    public class AllCollateralViewModel : CollateralViewModel
    {
       // public DateTime nextValuationDate { get { return lastValuationDate.AddDays((double)(valuationCycle)); } set { } }
        public DateTime nextValuationDate { get; set; }
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
        public string accountOfficerName { get; set; }
        public string accountOfficerCode { get; set; }
        public DateTime lastVisitationdate { get; set; }
        public DateTime? nextVisitationDates { get; set; }
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
        public string accountName { get; set; }
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
        public string accountName { get; set; }
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
        public string TrusteeName;

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
        public bool? isAssetPledgedByThirdParty { get; set; }
        public bool? isAssetManagedByTrustee { get; set; }
        public string thirdPartyName { get; set; }
        public string stateName { get; set; }
        public string localGovtName { get; set; }
        public decimal? bankShareOfCollateral { get; set; }
        public string trusteeName { get; set; }
        public decimal? estimatedValue { get; set; }
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
        public int collateraalId { get; set; }
        public string referenceNumber { get; set; }
        public int insuranceCompanyId { get; set; }
        public decimal sumInsured { get; set; }
        public DateTime startDate { get; set; }
        public string description { get; set; }
        public int? premiumPercent { get; set; }
        public int createdBy { get; set; }
        public decimal? inSurPremiumAmount { get; set; }
        public int insuranceTypeId { get; set; }
        public DateTime expiryDate { get; set; }
        public int companyId { get; set; }
        public short userBranchId { get; set; }
        public string applicationUrl { get; set; }
        public string userIPAddress { get; set; }
        public int? policyStateId { get; set; }
        public bool hasExpired { get; set; }
        public string companyAddress { get; set; }
        public int policyId { get; set; }
        public string customerCode { get; set; }
        public bool passed { get; set; }
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
        public DateTime manufacturedDate { get; set; }
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
        public DateTime? dateOfManufacture { get; set; }
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
        public int? cityId { get; set; }
        public string cityName { get; set; }
        public new short? countryId { get; set; }
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
        public decimal? premiumAmount { get; set; }
        public decimal? inSurPremiumAmount { get; set; }
        public int insuranceCompanyId { get; set; }
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
        public string collateralType { get; set; }
        public int? visitationCycle { get; set; }
        public bool isGPScollateralType { get; set; }
        public string GPScollateralType { get; set; }
       
        public bool isGpsCoordinatesCollateralType { get; set; }
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
        public short loanSystemTypeId { get; set; }
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
        public short? releaseApprovalStatusId { get; set; }
        public decimal collateralValue { get; set; }
    }

    public class CollateralLoanApplication
    {
        public int? valuationCycle { get; set; }
        public string collateralTypeName { get; set; }
        public int collateralCustomerId { get; set; }
        public string currencyCode { get; set; }
        public short currencyId { get; set; }
        public string currency { get; set; }

        public int loanApplicationCollateralId { get; set; }
        public double haircut { get; set; }
        public string collateralCode { get; set; }
        public double collateralValue { get; set; }
        public int collateralId { get; set; }
        public int? collateralTypeId { get; set; }
        public string collateralType { get; set; }
        public double securityValue { get { return collateralValue - (haircut * 0.01 * collateralValue); } }
        public int? loanId { get; set; }
        public int? loanType { get; set; }

    }

    public class ApplicationCollateralMapping : GeneralEntity
    {
        public int? collateralId { get; set; }
        public int loanAppCollateralId { get; set; }
        public int applicationId { get; set; }
        public int? loanApplicationDetailId { get; set; }
        public int staffId { get; set; }
        public string collateralCode { get; set; }
    }

    public class CollateralHistoryList
    {
        public string customerName { get; set; }
        public string customerGroupName { get; set; }
        public string usedBy { get; set; }
        public string loanRef { get; set; }
        public DateTime expirationDate { get; set; }
        public decimal collateralValue { get; set; }
        public decimal amountInUse { get; set; }
        public decimal collateralBalance { get; set; }
        public DateTime? dateUsed { get; set; }
        public DateTime dateProposed { get; set; }
        public double haircut { get; set; }
        public double exchangeRate { get; set; }
        public decimal approvedLoanAmount { get; set; }
        public decimal haircutValue { get; set; }
        public decimal runningPrincipal { get; set; }
        public decimal principalAmount { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public decimal totalOutstanding { get; set; }
        public DateTime lastVisitationDate { get; set; }
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
        public int? loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public bool? legalFeeTaken { get; set; }
        public decimal legalFeeAmount { get; set; }
        public DateTime? legalFeeDate { get; set; }
        public string collateralTypeName { get; set; }
        public decimal collateralValue { get; set; }
        public double hairCut { get; set; }
        public decimal valuationCycle { get; set; }
        public int currencyId { get; set; }
        public string currencyCode { get; set; }
        public string currency { get; set; }
    }

    public class CollateralCoverageViewModel : GeneralEntity
    {
        //public int companyId { get; set; }
        public bool coverAll { get; set; }
        public int? currencyId { get; set; }
        public int loanAppCollateralId { get; set; }
        public int loanCollateralMappingId { get; set; }
        public string collateralCurrencyCode { get; set; }
        public string baseCurrencyCode { get; set; }
        public int coverage { get; set; }
        public int collateralSubTypeId { get; set; }
        public string currencyName { get; set; }
        public int collateralCoverageId { get; set; }
        public int collateralId { get; set; }
        public string collateralTypeName { get; set; }
        public int collateralTypeId { get; set; }
        public decimal collateralValue { get; set; }
        public decimal omv { get; set; }
        public decimal fsv { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string collateralOwnerName { get; set; }
        public int collateralOwnerId { get; set; }
        public decimal facilityAmount { get; set; }
        public decimal requestedAmount { get; set; }
        public decimal disbursedAmount { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public DateTime? bookingDate { get; set; }
        public DateTime? maturityDate { get; set; }
        public bool isBooked { get; set; }
        public bool isDisbursed { get; set; }
        public decimal facilityAmountFcy { get; set; }
        public decimal collateralValueFcy { get; set; }
        public string facilityCurrencyCodeFcy { get; set; }
        public int facilityCurrencyId { get; set; }
        public int loanApplicationId { get; set; }
        public int? loanApplicationDetailId { get; set; }
        public decimal availableCollateralValue { get; set; }
        public decimal availableCollateralValueBaseAmount { get; set; }
        public decimal expectedCollateralCoverage { get; set; }
        public decimal actualCollateralCoverage { get; set; }
        public decimal totalCoverage { get; set; }
        public string productAccNumber { get; set; }
        public string referenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string productName { get; set; }
        public string collateralSummary { get; set; }
        public string collateralCode { get; set; }
        public int expectedCoveragePercentage { get; set; }
        public decimal actualCoveragePercentage { get; set; }
        public int approvalStatusId { get; set; }
        public bool isMapped { get; set; }
        public bool isUsed { get; set; }
        public DateTime valuationDate { get; set; }
       
    }
    public class CollateralUsageStatus
    {
        public int collateralStatusId { get; set; }
        public string collateralStatusName { get; set; }
    }

    public class InsuranceCompanyViewModel:GeneralEntity
    {
        public int insuranceCompanyId { get; set; }
        public int iompanyId { get; set; }
        public string  companyName { get; set; }
        public string address { get; set; }
        public string contactEmail { get; set; }
        public string phoneNumber { get; set; }
    }

    public class InsuranceTypeViewModel: GeneralEntity
    {
        public string insuranceType { get; set; }
        public int insuranceTypeId { get; set; }
        public int insuranceStatusId { get; set; }
        public string insuranceStatus { get; set; }
    }

    public class InsuranceStatusViewModel 
    {
        public string insuranceStatus { get; set; }
        public int insuranceStatusId { get; set; }
        public bool deleted { get; set; }
    }


    public class InsurancePolicyTypeViewModel : GeneralEntity
    {
        public string description { get; set; }
        public bool valuationRequired { get; set; }
        public string valuation { get; set; }
        public int policyTypeId { get; set; }
    }


    public class GPSCoordinatesCollateralTypeViewModel : GeneralEntity
    {
        public string collateralType { get; set; }
        public bool required { get; set; }
        public string valuation { get; set; }
        public int collateralTypeId { get; set; }
    }

    public class SectorLimitAlertViewModel
    {
        public int id { get; set; }
        public string sector { get; set; }
        public decimal? bbd { get; set; }
        public decimal? cbd { get; set; }
        public decimal? cibd { get; set; }
        public decimal? rbd { get; set; }
        public decimal? exposure { get; set; }
        public decimal? bank { get; set; }
    }

    public class CollateralCashReleaseViewModel : GeneralEntity
    {
        public int collateralId { get; set; }
        public int collateralTypeId { get; set; }
        public short collateralSubTypeId { get; set; }
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public string currency { get; set; }
        public decimal lienAmount { get; set; }
        public decimal loanAmount { get; set; }
        public string loanReferenceNumber { get; set; }
        public string productName { get; set; }
        public string collateralTypeName { get; set; }
        public string collateralSubTypeName { get; set; }
        public string collateralCode { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string loanTypeName { get; set; }
        public string facility { get; set; }
        public string customerGroupName { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int loanApplicationId { get; set; }
        public int approvalStatusId { get; set; }
        public int cashSecurityReleaseIseId { get; set; }
        public string approvalStatus { get; set; }
        public int operationId { get; set; }
        public DateTime dateRecieved { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public string createdByName { get; set; }
        public string collateralSummary { get; set; }
        public int approvalTrailId { get; set; }
        public int? loopedStaffId { get; set; }
        public int userId { get; set; }
        public string comment { get; set; }
        public int targetId { get; set; }
        public int? approvalLevelId { get; set; }
        public DateTime applicationDate { get; set; }
        public int currentApprovalLevelId { get; set; }
        public decimal collateralValue { get; set; }
        public double haircut { get; set; }
        public string responsiblePerson { get; set; }
        public string currentlyLevel { get; set; }
    }



    public class CollateralInsuranceTrackingViewModel : GeneralEntity
    {
        public int collateralId { get; set; }
        public string referenceNumber { get; set; }
        public decimal sumInsured { get; set; }
        public string insuranceCompany { get; set; }
        public int insuranceTypeId { get; set; }
        public string companyAddress { get; set; }
        public DateTime startDate { get; set; }
        public DateTime expiryDate { get; set; }
        public int? insurancePolicyTypeId { get; set; }
        public int? collateralCustomerId { get; set; }
        public string collateralDetails { get; set; }
        public decimal? forcedSaleValue { get; set; }
        public decimal? openMarketValue { get; set; }
        public DateTime? valuationEndDate { get; set; }
        public string valuer { get; set; }
        public DateTime? valuationStartDate { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int insuranceStatus { get; set; }
        public decimal inSurPremiumAmount { get; set; }
        public string otherInsurancePolicyType { get; set; }
        public string otherInsuranceCompany { get; set; }
        public string otherValuer { get; set; }

        public int? collateralTypeId { get; set; }
        public int? collateralSubTypeId { get; set; }
        public string gpsCoordinates { get; set; }
        public int? insuranceCompanyId { get; set; }
        public int? valuerId { get; set; }
        public string firstLossPayee { get; set; }
        public decimal? insurableValue { get; set; }
        public string comment { get; set; }
        public string customerCode { get; set; }
        public bool passed { get; set; }
        public List<string> errorMessages { get; set; }
        public string isCollateral { get; set; }
        public string policyType { get; set; }
        public string collateralCode { get; set; }
    }

    public class CollateralVisitationViewModel
    {
        public int collateralTypeId { get; set; }
        public string collateralType { get; set; }
        public string collateralCode { get; set; }
        public string collateralSubType { get; set; }
        public string customerName { get; set; }
        public string propertyName { get; set; }
        public DateTime lastVisitationDate { get; set; }
        public DateTime nextVisitationDate { get; set; }
        public int? visitationCycle { get; set; }
        public int relationshipManagerId { get; set; }
        public string relationshipManager { get; set; }
        public string relationshipManagerEmail { get; set; }
    }
    
    public class FacilityStampDutyViewModel
    {
        public int facilityStampDutyId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int collateralCustomerId { get; set; }
        public int currentstatus { get; set; }
        public string osdc { get; set; }
        public string asdc { get; set; }
        public string csdc { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime dateTimeUpdated { get; set; }
        public bool isShared { get; set; }
        public bool deleted { get; set; }
        public decimal customerPercentage { get; set; }
        public decimal bankPercentage { get; set; }
        public string customerName { get; set; }
        public decimal loanAmount { get; set; }
        public int approvedTenor { get; set; }
        public string collateralSubType { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string status { get; set; }
        public int operationId { get; set; }
        public int customerId { get; set; }
        public int? documentTypeId { get; set; }
        public DateTime? bookingDate { get; set; }
        public DateTime? maturityDate { get; set; }
        //public short collateralsubTypeId { get; set; }
        public short collateralSubTypeId { get; set; }
        public decimal stampDutyAmount { get; set; }
        public decimal totalDutyAmount { get; set; }
        public decimal dutiableValue { get; set; }
        public string contractCode { get; set; }
        public decimal? fixedDutyCharge { get; set; }
        public int collateralId { get; set; }
    }

    public class StampDutyConditionViewModel
    {
        public short userBranchId { get; set; }
        public string userIPAddress { get; set; }
        public string applicationUrl { get; set; }
        public int createdBy { get; set; }
        public int companyId { get; set; }
        public int collateralSubTypeId { get; set; }
        public int tenor { get; set; }
        public int tenorModeId { get; set; }
        public bool useTenor { get; set; }
        public decimal dutiableValue { get; set; }
        public bool isPercentage { get; set; }
        public int conditionId { get; set; }
        public string collateralSubType { get; set; }
    }


}
