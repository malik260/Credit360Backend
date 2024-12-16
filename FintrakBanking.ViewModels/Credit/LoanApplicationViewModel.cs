using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.ViewModels.Credit
{

    public class LoanApplicationViewModel : GeneralEntity
    {
        public short productTypeId { get; set; }
        public decimal totalUtilized { get; set; }

        public int? customerBusinessUnitId { get; set; }
        public string divisionShortCode { get; set; }
        public int bookingRequestId { get; set; }

        public string divisionCode { get; set; }
        public string accountToCredit { get; set; }
        public bool iblRequest { get; set; }
        public int iblRequestMark { get; set; }
        public string approvedProductName { get; set; }
        public decimal? equityControl { get; set; }
        public int? moratrium { get; set; }
        public string pledgeCollateral { get; set; }
        public decimal valueOfCollateral { get; set; }
        public string reportTypeName { get; set; }
        public int psrReportTypeId { get; set; }
        public string facility { get; set; }

        public bool? isFacilityCreated { get; set; }

        public LoanApplicationViewModel()
        {
            LoanApplicationDetail = new List<LoanApplicationDetailViewModel>();
        }

        public bool owner { get; set; }
        public int currentOperationId { get; set; }

        public bool jumpedDestination { get; set; }
        public bool failedRacStartCam { get; set; }
        public bool isNewApplication { get; set; }
        public bool closeApplication { get; set; }        
        public int loanApplicationId { get; set; }
        public int? loanApplicationIdForOperation { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int? customerId { get; set; }
        public int? operationId { get; set; }
        public bool requireCollateral { get; set; }
        public DateTime? newApplicationDate { get; set; }
        public decimal? loansWithOthers { get; set; }
        public string ownershipStructure { get; set; }

        public short? branchId { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public short productId { get; set; }
        public int? customerGroupId { get; set; }
        public string customerGroupCode { get; set; }
        public short loanTypeId { get; set; }
        public int? casaAccountId { get; set; }
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public short loanStatusId { get; set; }
        public string loanStatus { get; set; }
        public int relationshipOfficerId { get; set; }
        public int? relationshipManagerId { get; set; }
        public int daysPastDue { get; set; }
        public DateTime? datePastDue { get; set; }
        public DateTime applicationDate { get; set; }
        public DateTime? applicationDateArch { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal monthlyPayment { get; set; }
        public double applicationTenor { get; set; }
        public double interestRate { get; set; }
        public DateTime ? effectiveDate { get; set; }
        public DateTime ? bookingDate { get; set; }
        public DateTime ? maturityDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string customerAccount { get; set; }
        public short tenorModeId { get; set; }
        public string loanInformation { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public bool submittedForAppraisal { get; set; }
        public string operationName { get; set; }
        public bool isRelatedParty { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public bool isInvestmentGrade { set; get; }
        public bool isProjectRelatedLoan { set; get; }
        public bool isOnLending { set; get; }
        public bool isInterventionFunds { set; get; }
        public bool isORRBasedApproval { set; get; }
        public short approvalStatusId { get; set; }

        public decimal proposedAmount { get; set; }
        public int proposedTenor { get; set; }
        public int approvalLevelId { get; set; }

        public short subSectorId { get; set; }
        public int sectorId { get; set; }
        public string sectorName { get; set; }
        
        public int? loantermSheetCode { get; set; }
        public string customerName { get; set; }
        public int customerTypeId { get; set; }
        public string branchName { get; set; }
        public string productName { get; set; }
        public string customerGroupName { get; set; }
        public int? termSheetCode { get; set; }

        public string loanTypeName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string tenorModeName { get; set; }

        //public string amount { get { return this.principalAmount.ToString("#,#.00#"); } }
        public string applicantName { get { return this.customerName + "(" + this.customerGroupName + ")"; } }
        public int lmsApplicationDetailId { get; set; }

        public int? loanPreliminaryEvaluationId { get; set; }
        public int? loantermSheetId { get; set; }
        public double exchangeRate { get; set; }
        public List<LoanApplicationCollateralViewModel> LoanApplicationCollateral { get; set; }
        public List<LoanApplicationDetailViewModel> LoanApplicationDetail { get; set; }
        public IEnumerable<ApprovedLoanDetailViewModel> details { get; set; }
        public int? currentApprovalStateId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string currentApprovalLevel { get; set; }
        public string lastComment { get; set; }
        public int approvalTrailId { get; set; }
        public int? responseStaffId { get; set; }
        // public decimal? approvedAmount { get; set; }
        public short applicationStatusId { get; set; }

        public bool isCollateralBacked { get; set; }

        public string collateralDetail { get; set; }
        //private int _tenor;
        public string loanPurpose { get; set; }

        public int tenor { get; set; }
        public string apiRequestId { get; set; }
        public bool customerInfoValidated { get; set; }
        public bool notInNegativeCrms { get; set; }
        public bool notInBlackbook { get; set; }
        public bool notInCamsol { get; set; }
        public bool notInXds { get; set; }
        public bool notInCrc { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string customerCode { get; set; }
        public int proposedProductId { get; set; }
        public string repaymentTerm { get; set; }
        public bool? isTakeOverApplication { get; set; }
       
        public int? repaymentScheduleId { get; set; }
        public double proposedInterestRate { get; set; }
        public string proposedProductName { get; set; }
        public int approvedProductId { get; set; }
        public string currencyName { get; set; }
        public string lastName { get; set; }
        public int groupRoleId { get; set; }
        public string accountNumber { get; set; }
        public string accountNumber2 { get; set; }
        public string applicationStatus { get; set; }
        public string relatedReferenceNumber { get; set; }
        public int? toStaffId { get; set; }
        public short productClassProcessId { get; set; }
        public string approvalStatus { get; set; }
        public string responsiblePerson { get; set; }
        public DateTime? timeIn { get; set; }
        public DateTime? slaTime { get; set; }
        public string cancellationReason { get; set; }
        public int globalsla { get; set; } 
        public int currentApprovalLevelSlaInterval { get; set; }
        public bool isadhocapplication { get; set; }
        public bool isSkipAppraisalEnabled { get; set; }
        public bool isAtDrawDown { get; set; }
        public int? exclusiveOperationId { get; set; }
        public int? flowchangeId { get; set; }
        public int? loanApprovedLimitId { get; set; }
        public int approvedTenor { get; set; }
        public List<LoanApplicationDetailViewModel> applicationDetails { get; set; }

        public string approvalStatusName { get; set; }

        public string tenorString
        {
            get
            {
                var units = applicationTenor == 1 ? " day" : " days";
                if (applicationTenor < 30) return applicationTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(applicationTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + units;
            }
        }

        public string timeLapse
        {
            get
            {
                if (timeIn == null) return "n/a";
                int count = (int)Math.Round((DateTime.Now - (DateTime)timeIn).TotalDays);
                string units = count == 1 ? " day" : " days";
                if ((DateTime.Now - (DateTime)timeIn).TotalHours < 24) return timeIn.ToString();
                return count.ToString() + units;
            }
        }
        
        public int tempApplicationCancellationId { get; set; }
        public IQueryable<string> staffName { get; set; }
        public string creatorName { get; set; }
        public string comment { get; set; }
        public bool isOfferLetterAvailable { get; set; }
        public int? currentApprovalLevelTypeId { get; set; }
        public short? tranchLevelId { get; set; }
        public int? regionId { get; set; }
        public bool editMode { get; set; }
        public short? requireCollateralTypeId { get; set; }
        public RacInformationViewModel rac { get; set; }
        public List<DateTimeAndTimeOfDayViewModel> dateTimeAndTimeOfDay { get; set; }


        public string slaGlobalStatus { get; set; }

        public string slaInduvidualStatus { get; set; }

        public string customerType { get; set; }
        public string isProjectRelated { get; set; }
        public decimal facilityAmount { get; set; }
        public string operationTypeName { get; set; }
        public string reviewLoanDetaile { get; set; }
        public string referenceNumber { get; set; }
        public DateTime systemDateTime { get; set; }
        public int? bookingOperationId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string lienReferenceNumber { get; set; }
        public decimal lienAmount { get; set; }
        public DateTime lienDateTimeCreated { get; set; }
        public string currency { get; set; }
        public decimal principalAmount { get; set; }
        public string loanReferenceNumber { get; set; }
        public int? loanId { get; set; }
        public int loanSystemTypeId { get; set; }
        public string legalContingentCode { get; set; }
        public int? singleCustomerId { get; set; }
        public bool isEmployerRelated { get; set; }
        public int? relatedEmployerId { get; set; }
        public string employer { get; set; }
        public int creditAppraisalOperationId { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int subsidiaryId { get; set; }
        public int? customerGlobalId { get; set; }
        public string productClassProcess { get; set; }
        public decimal totalExposureAmount { get; set; }
        public string countryCode { get; set; }
        public string createdByName { get; set; }

        public int? creditGradeId { get; set; }
        public decimal investmentGradeLimit { get; set; }
        public decimal standardGradeLimit { get; set; }
        public decimal renewalLimit { get; set; }
        public TBL_APPROVAL_LEVEL_STAFF approvalLevelStaff { get; set; }
        public int loanDetailReviewTypeId { get; set; }
        public string loanDetailReviewTypeName { get; set; }
        public bool isExternal { get; set; }
    }

    public class InterestIncomeViewModel
    {
        public string referenceNumber { get; set; }
        public int prudentialGuideLineTypeId { get; set; }
        public decimal dailyAccrualAmount { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public string period { get; set; }
        public decimal totalMonthlyIncome { get; set; }
        public decimal performing { get; set; }
        public decimal nonPerforming { get; set; }

        public decimal totalIncome { get; set; }
    }

    public class FixedDepositCollateralViewModel
    {
        public string collateralCode { get; set; }
        public decimal collateralValue { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public string valuationCycle { get; set; }
        public string collateralType { get; set; }
        public string collateralSummary { get; set; }
        public DateTime validityExpiryDate { get; set; }
        public string customerCode { get; set; }
        public string accountNumber { get; set; }
        public decimal availableBalance { get; set; }
        public decimal securityValue { get; set; }
        public DateTime effectiveDate { get; set; }
        
        public DateTime maturityDate { get; set; }
        public string customerName { get; set; }

        public string facilityDetails { get; set; }
        public string facilityReference { get; set; }
        public string currencyCode { get; set; }
        public string comment { get; set; }
        public string contractCode { get; set; }
        public string facilityType { get; set; }
        public string applicationReference { get; set; }
    }

    public class LoanApplicationUpdateMessage
    {
        public bool isdone { get; set; } 
        public string messageStr { get; set; }
        public int checkListIndex { get; set; }
        public bool jumpToDrawdown { get; set; }
    }

    public class RacReturnInfoViewModel
    {
        public int? loanApplicationId { get; set; }
        public int? loanApplicationDetailId { get; set; }
    }

    public class RacInformationViewModel
    {
        public List<RacFormControlValue> form { get; set; }
        public int? operationId { get; set; }
        public int? targetId { get; set; }
        public int? checklistStatus { get; set; }
        public int? productId { get; set; }
        public int? productClassId { get; set; }
        public int racDefinitionId { get; set; }
        public int createdBy { get; set; }
        public string actualValue { get; set; }
    }
    public class RacFormControlValue
    {
        public int criteriaId { get; set; }
        public string value { get; set; }
    }

    public class LoanApplicationUpdateViewModel
    {
        public int applicationId { get; set; }
        public int staffId { get; set; }
        public int checkListIndex { get; set; }
    }

    public class CollateralLenPlacementViewModel : GeneralEntity
    {
        public string acountNumber { get; set; }
        public int cityId { get; set; }
        public int stateId { get; set; }
        public int collateralTypeId { get; set; }
        public string certificateOfOccupancy { get; set; }
        public string loanApplicationRefrence { get; set; }
        public string customerName { get; set; }
    }

    public class ProductClassViewModel
    {
        public string productTypeName { get; set; }
        public short productTypeId { get; set; }

        public short productClassId { get; set; }
        public string productClassName { get; set; }
        public short productClassTypeId { get; set; }
    }
    public class ValidateDataViewModel
    {
        public int productId { get; set; }
        public DateTime date { get; set; }
        public int? dayInterval { get; set; }
        public bool InvoiceStatus { get; set; }
        public int dayCount { get; set; }
    }
    public class ValidateNumberViewModel
    {
        public string contractNumber { get; set; }
        public int customerId { get; set; }
        public int productId { get; set; }
        public int principalId { get; set; }
        public bool invoiceStatus { get; set; }
        public string documentNo { get; set; }
        public string purchaseOrderNumber { get; set; }
        public string certificateNumber { get; set; }
        public bool reValidated { get; set; }
    }

    public class LoanApplicationDetailViewModel : GeneralEntity
    {
        public LoanApplicationDetailViewModel()
        {
            invoiceDetails = new List<InvoiceDetailViewModel>();
            productFees = new List<ProductFeesViewModel>();
            syndicatedLoan = new List<SyndicatedLoanDetailViewModel>();
        }

        public string approvedProductName { get; set; }
        public bool? isLineFacility { get; set; }

        public int applicationStatusPosition { get; set; }

        public string currencyCode { get; set; }
        public DateTime applicationDate { get; set; }
        public short applicationStatusId { get; set; }
        public int approvalStatusId { get; set; }
        public DateTime systemArrivalDate { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string branchName { get; set; }
        public string customerGroupName { get; set; }
        public bool? isTakeOverApplication { get; set; }

        public string customerType { get; set; }
        public short? customerTypeId { get; set; }
        public int loanDetailReviewTypeId { get; set; }
        public int? approvedTradeCycleId { get; set; }
        public string customerAccountNumber { get; set; }

        public bool requireCollateral { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int loanApplicationId { get; set; }

        public string applicationReferenceNumber { get; set; }

        public int customerId { get; set; }
        public int? customerGroupId { get; set; }

        public string customerName { get; set; }

        public decimal? equityAmount { get; set; }

        public int? equityCasaAccountId { get; set; }

        public short proposedProductId { get; set; }

        public string proposedProductName { get; set; }

        public int proposedTenor { get; set; }

        public double? proposedInterestRate { get; set; }

        public decimal proposedAmount { get; set; }

        public int? flowChangeId { get; set; }

        public short approvedProductId { get; set; }

        public string productName { get; set; }

        public int approvedTenor { get; set; }

        public int? tenorModeId { get; set; }

        public int? tenorFrequencyTypeId
        {
            get
            {
                return tenorModeId == null ? (int?)TenorMode.Daily : tenorModeId; // default to days
            }
        }

        public double approvedInterestRate { get; set; }

        public decimal approvedAmount { get; set; }

        public short currencyId { get; set; }

        public string currencyName { get; set; }

        public double exchangeRate { get; set; }

        public decimal exchangeAmount { get { return (decimal)exchangeRate * proposedAmount; } }

        public short subSectorId { get; set; }

        public short statusId { get; set; }

        public int? casaAccountId { get; set; }
        public int? operatingCasaAccountId { get; set; }

        public short sectorId { get; set; }

        public short? productClassId { get; set; }
        public short? productTypeId { get; set; }

        public short? productClassProcessId { get; set; }

        public string loanPurpose { get; set; }

        public string repaymentTerm { get; set; }

        public int? repaymentScheduleId { get; set; }

        public int?  crmsFundingSourceId { get; set; }

        public int? crmsPaymentSourceId { get; set; }

        public int? exclusiveOperationId { get; set; }
        public int operationId { get; set; }

        public string crmsFundingSourceCategory { get; set; }

        public string crms_ECCI_Number { get; set; }

        public string conditionPrecedent { get; set; }

        public string cflRrequestId { get; set; }

        public string conditionSubsequent { get; set; }

        public string transactionDynamics { get; set; }

        public string fieldOne { get; set; }

        public string fieldTwo { get; set; }

        public decimal? fieldThree { get; set; }

        public bool isSpecialised { get; set; }

        public short? productPriceIndexId { get; set; }

        public double? productPriceIndexRate { get; set; }

        public int loanTypeId { get; set; }

        public List<InvoiceDetailViewModel> invoiceDetails { get; set; }

        public EducationLoanViewModel educationLoan { get; set; }

        public TraderLoanViewModel traderLoan { get; set; }
        public List<ProductFeesViewModel> productFees { get; set; }
        public BondsAndGuranty bondDetails { get; set; }
        public List<SyndicatedLoanDetailViewModel> syndicatedLoan { get; set; }
        public IEnumerable<LoanCreditBureauViewModel> LoanCreditBereauReport { get; set; }
        public string sectorName { get; set; }
        public string productClass { get; set; }

        public string interestRepayment { get; set; }
        public int? interestRepaymentId { get; set; }
        public string moratorium { get; set; }
        public bool? isMoratorium { get; set; }
        public bool iblRequest { get; set; }
        public decimal? approvedLineLimit { get; set; }
        


        public string priceIndexName { get; set; }
        public int? priceIndexId { get; set; }
        public double priceIndexRate { get; set; }
        public string liborInfo { get { return priceIndexId == null ? "" : "(" + priceIndexName + ")"; } }

        public string proposedTenorString
        {
            get
            {
                var units = proposedTenor == 1 ? " day" : " days";
                if (proposedTenor < 15) return proposedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(proposedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public string approvedTenorString
        {
            get
            {
                var units = approvedTenor == 1 ? " day" : " days";
                if (approvedTenor < 15) return approvedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(approvedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public int proposedTenorConverted
        {
            get
            {
                int tenor = 0;
                switch (tenorModeId)
                {
                    case (int)TenorMode.Daily: tenor = proposedTenor; break;
                    case (int)TenorMode.Monthly: tenor = proposedTenor / 30; break;
                    case (int)TenorMode.Yearly: tenor = proposedTenor / 365; break;
                    default: tenor = proposedTenor; break;
                }
                return tenor;
            }
        }

        public string email { get; set; }
        public short? requireCollateralTypeId { get; set; }
        public int relationshipOfficerId { get; set; }
        public string breachedLimitName { get; set; }
        public int approvalTrailId { get; set; }
        public string approvalStatus { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string currentApprovalLevel { get; set; }
        public bool isTemplateUploaded { get; set; }
        public string oldApplicationRefForRenewal { get; set; }
        public string accountOfficerName { get; set; }

        // public int? loanreViewApplicationId { get; set; }
        public short? propertyTypeId { get; set; }
        public string propertyTitle { get; set; }
        public decimal? propertyPrice { get; set; }
        public decimal? downPayment { get; set; }
    }

    public class ProductFeesViewModel// : GeneralEntity
    {
        public int loanChargeFeeId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public bool hasConsession { get; set; }
        public string consessionReason { get; set; }
        public decimal defaultfeeRateValue { get; set; }
        public decimal recommededFeeRateValue { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public int feeId { get; set; }
        public string feeName { get; set; }
        public decimal rate { get; set; }
        public int createdBy { get; set; }
    }

    public class BondsAndGuranty
    {
        public int loanApplicationDetailId { get; set; }
        public decimal bondAmount { get; set; }
        public int? principalId { get; set; }
        public int? casaAccountId { get; set; }        
        public short bondCurrencyId { get; set; }
        public DateTime contractStartDate { get; set; }
        public DateTime contractEndDate { get; set; }
        public bool isTenored { get; set; }
        public bool isBankFormat { get; set; }
        public string referenceNo { get; set; }
        public string principalName { get; set; }


    }

    public class SearchViewModel
    {
        public int performanceTypeId { get; set; }
        public int statusId { get; set; }
        public int productTypeId { get; set; }
        public string searchString { get; set; }
        public short loanSystemTypeId { get; set; }
        public string relatedloanReferenceNumber { get; set; }
        public string loanReferenceNumber { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
   

    public class CommitteFilterViewModel
    {
        public int? applicationTypeId { get; set; }
        public int staffId { get; set; }
    }

    public class jobLoanApplicationDetailViewModel : LoanApplicationViewModel
    {
        //public string customerType { get; set; }

        public string applicationRefNo { get; set; }

        public new short proposedProductId { get; set; }

        public string proposedProductName { get; set; }

        public double proposedInterestRate { get; set; }

        public short approvedProductId { get; set; }

        public int approvedTenor { get; set; }

        public double approvedInterestRate { get; set; }

        public string currencyName { get; set; }

        public decimal exchangeAmount { get { return (decimal)exchangeRate * proposedAmount; } }

        public short statusId { get; set; }

        public List<LoanApplicationDetailInvoiceViewModel> invoiceDiscountDetail { get; set; }

        public List<EducationLoanViewModel> firstEducationtDetail { get; set; }

        public List<TraderLoanViewModel> firstTradderDetail { get; set; }

        public List<CollateralViewModel> loanCollateral { get; set; }
        public List<BondsAndGauranteeViewModel> bondsAndGaurantees { get; set; }
    }

    public class LoanApplicationDetailInvoiceViewModel
    {
        public bool revalidated { get; set; }

        public string contractNumber { get; set; }
        public string purchaseOrderNumber { get; set; }

        public int invoiceId { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int principalId { get; set; }

        public string invoiceNo { get; set; }

        public DateTime invoiceDate { get; set; }

        public decimal invoiceAmount { get; set; }

        public string principalName { get; set; }

        public string principalAccount { get; set; }

        public string principalRegNo { get; set; }

        public short invoiceCurrencyId { get; set; }

        public string invoiceCurrencyCode { get; set; }

        public DateTime contractStartDate { get; set; }

        public DateTime contractEndDate { get; set; }

        public short? approvaStatusId { get; set; }

        public string approvalStatusName { get; set; }

        public string approvalComment { get; set; }

        public int? approvedBy { get; set; }

        public DateTime? approvedDateTime { get; set; }

        public bool reValidated { get; set; }

        public string entrySheetNumber { get; set; }
    }

    public class RegionLoanApplicationViewModel : GeneralEntity
    {
        public RegionLoanApplicationViewModel()
        {
            LoanApplicationDetail = new List<LoanApplicationDetailViewModel>();

        }

        public int loanApplicationId { get; set; }
        public DateTime applicationDate { get; set; }
        public string applicationReferenceNumber { get; set; }
        public short? branchId { get; set; }
        public decimal applicationAmount { get; set; }
        public double interestRate { get; set; }
        public int applicationTenor { get; set; }
        public bool submittedForAppraisal { get; set; }
        public int approvalStatusId { get; set; }

        public List<LoanApplicationDetailViewModel> LoanApplicationDetail { get; set; }
        public int? customerId { get; set; }
        public int operationId { get; set; }
        public int? approvalTrailId { get; set; }
        public string currentApprovalLevel { get; set; }
        public int requestStaffId { get; set; }
        public int? toStaffId { get; set; }
        public int? toApprovalLevelId { get; set; }
        public DateTime timeIn { get; set; }
        public DateTime? timeOut { get; set; }
        public string responsiblePerson { get; set; }
        public short? productClassId { get; set; }
        public int? finalApprovalLevelId { get; set; }
        public short? nextApplicationStatusId { get; set; }
        public string customerName { get; set; }
        public string customerGroupName { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int? currentApprovalLevelTypeId { get; set; }
        public string timeLapse
        {
            get
            {
                if (timeIn == null) return "n/a";
                int count = (int)Math.Round((DateTime.Now - (DateTime)timeIn).TotalDays);
                string units = count == 1 ? " day" : " days";
                if ((DateTime.Now - (DateTime)timeIn).TotalHours < 24) return timeIn.ToString();
                return count.ToString() + units;
            }
        }

        public string facilityType { get; set; }
    }

    public class InvoiceDetailViewModel
    {
        public int invoiceId { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int principalId { get; set; }

        public string contractNo { get; set; }

        public string purchaseOrderNumber { get; set; }

        public string certificateNumber { get; set; }

        public string invoiceNo { get; set; }

        public DateTime invoiceDate { get; set; }

        public decimal invoiceAmount { get; set; }

        public short invoiceCurrencyId { get; set; }

        public DateTime contractStartDate { get; set; }

        public DateTime contractEndDate { get; set; }

        public string principalName { get; set; }

        public string invoiceCurrencyName { get; set; }

        public short? approvalStatusId { get; set; }

        public int productClassId { get; set; }

        public bool reValidated { get; set; }

        public string entrySheetNumber { get; set; }

    }

    public class EducationLoanViewModel
    {
        public int educationId { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int numberOfStudent { get; set; }

        public decimal averageSchoolFees { get; set; }

        public decimal schoolFeesCollected { get; set; }

        public decimal totalPreviousTermSchoolFees { get; set; }

        public decimal productClassId { get; set; }
        public string productClassName { get; set; }

    }

    public class TraderLoanViewModel
    {
        public int tradderId { get; set; }

        public int marketId { get; set; }

        public decimal averageMonthlyTurnover { get; set; }

        public string soldItems { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int traderId { get; set; }

        public string marketName { get; set; }

        public int productClassId { get; set; }
    }

    public class BondsAndGauranteeViewModel
    {
        public int bondId { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int? principalId { get; set; }

         public int? casaAccountId { get; set; }

        public decimal amount { get; set; }

        public short currencyId { get; set; }

        public DateTime contractStartDate { get; set; }

        public DateTime contractEndDate { get; set; }

        public string referenceNo { get; set; }

        public bool isTenored { get; set; }

        public bool isBankFormat { get; set; }

        public short? approvalStatusId { get; set; }

        public string approvalComment { get; set; }

        public int? approvedBy { get; set; }

        public DateTime? approvedDateTime { get; set; }
        public string principalName { get; set; }

        public string invoiceCurrencyCode { get; set; }

        public string approvalStatusName { get; set; }

        public int productClassId { get; set; }

        public string principalNameOthers { get; set; }
    }

    public class SyndicatedLoanDetailViewModel
    {
        public int syndicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string bankCode { get; set; }
        public string bankName { get; set; }
        public decimal amountContributed { get; set; }
        public short typeId { get; set; }
        public string typeName { get; set; }
        public int productClassId { get; set; }

    }

    public class CamViewModel : GeneralEntity
    {
        public int documentationId { get; set; }
        public string documentation { get; set; }
        public int applicationId { get; set; }
        public int approvalLevelId { get; set; }
        public string referenceNumber { get; set; }
        public bool createNew { get; set; }
    }

    public class ForwardReviewViewModel : GeneralEntity
    {
        public bool isFlowTest { get; set; }
        public bool isFromPc { get; set; }
        public int forwardAction { get; set; } // statusId
        public int applicationId { get; set; } // targetId
        public int appraisalMemorandumId { get; set; }
        public int? productClassId { get; set; }
        public int? productId { get; set; }
        public int receiverLevelId { get; set; }
        public int? receiverStaffId { get; set; }
        public int? trailId { get; set; }
        public decimal amount { get; set; }
        public bool politicallyExposed { get; set; }
        public short? vote { get; set; }
        public string comment { get; set; }
        public decimal principal { get; set; }
        public decimal totalExposureAmount { get; set; }
        public double rate { get; set; }
        public int tenor { get; set; }
        public bool investmentGrade { get; set; }
        public int applicationTenor { get; set; }
        public int operationId { get; set; }
        public bool untenored { get; set; }
        public bool isBusiness { get; set; }
        public float? interestRateConcession { get; set; }
        public float? feeRateConcession { get; set; }
        public List<RecommendedChangesViewModel> recommendedChanges { get; set; }
        public bool? isAvailment { get; set; }
    }

    public class CreditApplicationViewModel
    {
       

        public int loanApplicationId { get; set; }
        public string applicationType { get; set; }
        public DateTime applicationDate { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerName { get { return this.firstName + "" + this.middleName + "" + this.lastName; } }
        public string customerGroupName { get; set; }
        public List<LoanApplicationDatailViewModel> details { get; set; }
        public string customerCode { get; set; }
        public int customerId { get; set; }
        public int operationId { get; set; }
    }
    
    public class ManagementPositionViewModel : GeneralEntity
    {
        public int applicationDetailId { get; set; }
        public string managementPosition { get; set; }
    }

    public class CustomerApplicationTransactionsViewModels // TEMPORARY LOCATION
    {
        public CustomerApplicationTransactionsViewModels()
        {

            firstTransaction = new List<CustomerTransactionsViewModels>();
            secondTransaction = new List<CustomerTransactionsViewModels>();
        }
        public List<CustomerTransactionsViewModels> firstTransaction { get; set; }
        public List<CustomerTransactionsViewModels> secondTransaction { get; set; }

    }
    public class CustomerTransactionsViewModels // TEMPORARY LOCATION
    {
        public string foracid { get; set; } // ": "2022072744",
        public string cust_Id { get; set; } // ": "483008974",
        public string schm_Type { get; set; } // ": "ODA|OVERDRAFT A/C",
        public string period { get; set; } // ": "Apr-15",
        public decimal? min_Debit_Balance { get; set; } // ": "",
        public decimal? max_Debit_Balance { get; set; } // ": "",
        public decimal? min_Credit_Balance { get; set; } // ": "34218.39",
        public decimal? max_Credit_Balance { get; set; } // ": "1050843.73",
        public decimal? debit_Turnover { get; set; } // ": "1159360.11",
        public decimal? credit_Turnover { get; set; } // ": "1207425.56",
        public string sms_Alert { get; set; } // ": "-176",
        public string amc { get; set; } // ": "",
        public string vat { get; set; } // ": "-92.50",
        public string management_Fee { get; set; } // ": "",
        public string commitment_Fees { get; set; } // ": "",
        public string com_Contigent_Liab { get; set; } // ": "",
        public string lc_Commission { get; set; } // ": 
        public decimal? float_Charge { get; set; } // "2081981.94",
        public decimal? interest { get; set; } // "2909416.54",
        public string accountNumber { get; set; } // "2909416.54",
        public string productName { get; set; } // "2909416.54",
        public int? month { get; set; } // "0",
        public int? year { get; set; } // "0",
        public string productAccountName { get; set; }
        public int casaAccountId { get; set; }
        public DateTime periodDate { get; set; }
    }


    public class LoanApplicationTagsViewModel : GeneralEntity
    {
        public bool withInstruction { get; set; }
        public bool domiciliationNotInPlace;
        public bool isProjectRelated { get; set; }
        public bool isOnLending { get; set; }
        public bool isInterventionFunds { get; set; }
        public bool isAgricRelated { get; set; }
        public bool isSyndicated { get; set; }  
        public bool iblRenewal { get; set; }  
         
    }

    public class LoanApplicationTagsLMSViewModel : GeneralEntity
    {
        public bool? withInstruction { get; set; }
        public bool? domiciliationNotInPlace;
        public bool? isProjectRelated { get; set; }
        public bool? isOnLending { get; set; }
        public bool? isInterventionFunds { get; set; }
        public bool isAgricRelated { get; set; }
        public bool isSyndicated { get; set; }
    }


    public class ApprovalLevelDetailsModel : GeneralEntity
    {
        public int? approvalLevelId { get; set; }
        public int? staffRoleId { get; set; }
        public string levelName { get; set; }
        public int? groupPosition { get; set; }
        public int? levelPosition { get; set; }

    }

    public class RevisedProcessFlowModel : GeneralEntity
    {
        public bool hasOperationBasedRac { get; set; }

        public string label { get; set; }
        public short? productTypeId { get; set; }

        public short flowchangeId { get; set; }
        public string placeHolder { get; set; }
        public short? productClassId { get; set; }
        public int? productId { get; set; }
        public int operationId { get; set; }
        public string destinationUrl { get; set; }
        public bool skipProcessFlowEnabled { get; set; }


    }

    public class LoanApplicationFlowChangeViewModel : GeneralEntity
    {
        public string skipFlo { get; set; }

        public int FlowChangeId { get; set; }
        public string label { get; set; }
        public string placeHolder { get; set; }
        public bool skipflow { get; set; }
        public int? interestPayment { get; set; }
        public short? productClassId { get; set; }
        public int? productId { get; set; }
        public int operationId { get; set; }
        public string destinationUrl { get; set; }
        public short? productTypeId { get; set; }
        public string productType { get; set; }
        public string productClass { get; set; }
        public string operation { get; set; }
        public  int documentOperation { get; set; }
    }

    public class accountsViewModels // TEMPORARY LOCATION
    {
        public string accountNumber { get; set; } // "2909416.54",
    }

    public class LoanApplicationLienViewModel : GeneralEntity
    {
        public bool isReleased { get; set; }
        public string accountNo { get; set; }
        public int applicationDetailLienId { get; set; }
        public int applicationDetailId { get; set; }
        public int collateralId { get; set; }
        public decimal amount { get; set; }
    }

    public class FacilityModificationViewModel : GeneralEntity
    {
        public int facilityModificationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short approvedProductId { get; set; }
        public double approvedInterestRate { get; set; }
        public int approvedTenor { get; set; }
        public int tenorModeId { get; set; }
        public int sectorId { get; set; }
        public short subSectorId { get; set; }
        public int productClassId { get; set; }
        public int loanDetailReviewTypeId { get; set; }
        public int productClassProcessId { get; set; }
        public int? productClassProcessId2 { get; set; }
        public decimal approvedAmount { get; set; }
        public string reviewDetails { get; set; }
        public string customerName { get; set; }
        public string applicationRef { get; set; }
        public List<ProductFeesViewModel> fees { get; set; }
        public int repaymentScheduleId { get; set; }
        public int interestRepaymentId { get; set; }
        public string repaymentTerms { get; set; }
        public string interestRepayment { get; set; }
        public string approvalStatus { get; set; }
        public DateTime systemArrivalDateTime { get; set; }

    }

    public class DateTimeAndTimeOfDayViewModel
    {
        public DateTime dateTime { get; set; }
        public TimeSpan timeOfDay { get; set; }
    }

    public class ContractorCriteriaFormControlValue
    {
        public int criteriaId { get; set; }
        public decimal value { get; set; }
    }

    public class IBLChecklistFormControlValue
    {
        public int iblChecklistId { get; set; }
        public decimal value { get; set; }
        public int optionId { get; set; }
    }

    public class ProjectRistratingFormControlValue
    {
        public int categoryId { get; set; }
        public int value { get; set; }
    }
    public class ContractorTieringViewModel : GeneralEntity
    {
        public List<ContractorCriteriaFormControlValue> form { get; set; }
        public int contractorTierId { get; set; }
        public int loanApplicationId { get; set; }
        public int customerId { get; set; }
        public int contractorCriteriaId { get; set; }
        public decimal actualValue { get; set; }
        public string criteria { get; set; }
        public string tier { get; set; }
        public decimal computation { get; set; }
        
    }

    public class ProjectRiskRatingViewModel : GeneralEntity
    {
        public List<ProjectRistratingFormControlValue> form { get; set; }
        public int projectRiskRatingId { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int? loanBookingRequestId { get; set; }
        public int categoryId { get; set; }
        public int categoryValue { get; set; }
        public string categoryName { get; set; }
        public int computation { get; set; }
        public string riskCategorisation { get; set; }
        public decimal customerTier { get; set; }
        public string projectDetails { get; set; }
        public string projectLocation { get; set; }
    }

    public class ContractorCriteriaViewModel : GeneralEntity
    {
        public int criteriaId { get; set; }
        public string criteria { get; set; }
        public decimal tierOne { get; set; }
        public decimal tierTwo { get; set; }
        public decimal tierThree { get; set; }
        public object options { get; set; }
        public int contractorTieringId { get; set; }
    }
    public class IBLChecklistViewModel : GeneralEntity
    {
        public int iblChecklistId { get; set; }
        public string checklist { get; set; }
        public string optionName { get; set; }
        public List<IBLChecklistOptionViewModel> options { get; set; }
        public int optionId { get; set; }
        public int iblChecklistDetailId { get; set; }
        public int loanApplicationId { get; set; }
        public int customerId { get; set; }
        public List<IBLChecklistFormControlValue> form { get; set; }
        public string actualValue { get; set; }
    }
    public class IBLChecklistOptionViewModel
    {
        public int optionId { get; set; }
        public string optionName { get; set; }
        public int iblChecklistId { get; set; }
    }

    public class ContractorCriteriaOptionViewModel : GeneralEntity
    {
        public int criteriaId { get; set; }
        public int optionId { get; set; }
        public string optionName { get; set; }
        public decimal optionValue { get; set; }
        public string criteria { get; set; }
    }

    public class ProjectRiskRatingCategoryViewModel : GeneralEntity
    {
        public ProjectRiskRatingCategoryViewModel()
        {

            criterias = new List<ProjectRiskRatingCriteriaViewModel>();
        }
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public List<ProjectRiskRatingCriteriaViewModel> criterias { get; set; }
        public string criteria { get; set; }
        public int criteriaValue { get; set; }
    }

    public class ProjectRiskRatingCriteriaViewModel : GeneralEntity
    {
        public int projectRiskRatingCriteriaId { get; set; }
        public string criteria { get; set; }
        public decimal criteriaValue { get; set; }
        public int projectRiskRatingCategoryId { get; set; }
        public string category { get; set; }
    }

    public class RetailRecoveryCustomerTransactionsViewModels 
    {
        
        public decimal totalExposure { get; set; }
        public string foracid { get; set; }
        public string cust_Id { get; set; } 
        public string schm_Type { get; set; } 
        public string period { get; set; } 
        public decimal? min_Debit_Balance { get; set; } 
        public decimal? max_Debit_Balance { get; set; } 
        public decimal? min_Credit_Balance { get; set; } 
        public decimal? max_Credit_Balance { get; set; } 
        public decimal? debit_Turnover { get; set; } 
        public decimal? credit_Turnover { get; set; } 
        public string sms_Alert { get; set; } 
        public string amc { get; set; } 
        public string vat { get; set; } 
        public string management_Fee { get; set; } 
        public string commitment_Fees { get; set; } 
        public string com_Contigent_Liab { get; set; } 
        public string lc_Commission { get; set; } 
        public decimal? float_Charge { get; set; } 
        public decimal? interest { get; set; } 
        public string accountNumber { get; set; } 
        public string productName { get; set; } 
        public int? month { get; set; } 
        public int? year { get; set; } 
        public string productAccountName { get; set; }
        public int casaAccountId { get; set; }
        public DateTime periodDate { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<RetailRecoveryCustomerTransactionsViewModels> firstTransaction { get; set; }
        public List<RetailRecoveryCustomerTransactionsViewModels> secondTransaction { get; set; }
        public int loanApplicationId { get; set; }
        public short currencyId { get; set; }
        public string accreditedConsultantName { get; set; }
        public string accreditedConsultantCompany { get; set; }
        public int accreditedConsultantId { get; set; }
        public DateTime? expCompletionDate { get; set; }
        public int creditAppraisalOperationId { get; set; }
        public short loanSystemTypeId { get; set; }
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public short productTypeId { get; set; }
        public string casaAccount { get; set; }
        public string casaAccountName { get; set; }
        public short branchId { get; set; }
        public decimal totalAmountRecovery { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public short principalFrequencyTypeId { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal principalAmount { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int? operationId { get; set; }
        public short loanTypeId { get; set; }
        public decimal equityContribution { get; set; }
        public short subSectorId { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public bool profileLoan { get; set; }
        public string customerCode { get; set; }
        public string loanTypeName { get; set; }
        public string customerName { get; set; }
        public string branchName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public decimal approvedAmount { get; set; }
        public string creatorName { get; set; }
        public decimal debitAmount { get; set; }
        public decimal creditAmount { get; set; }
        public string loanReference { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime postedDate { get; set; }
        public string description { get; set; }
        public string productCode { get; set; }
        public string branchCode { get; set; }
        public short productClassId { get; set; }
        public decimal? totalUnsettledAmount { get; set; }
    }




    //original document submission by facility
    public class OriginalDocumentSubmissionByFacilityViewModel : GeneralEntity
    {
        public OriginalDocumentSubmissionByFacilityViewModel()
        {
            invoiceDetails = new List<InvoiceDetailViewModel>();
            productFees = new List<ProductFeesViewModel>();
            syndicatedLoan = new List<SyndicatedLoanDetailViewModel>();
        }

        public string approvedProductName { get; set; }
        public bool? isLineFacility { get; set; }

        public int applicationStatusPosition { get; set; }

        public string currencyCode { get; set; }
        public DateTime applicationDate { get; set; }
        public short applicationStatusId { get; set; }
        public int approvalStatusId { get; set; }
        public DateTime systemArrivalDate { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string branchName { get; set; }
        public string customerGroupName { get; set; }
        public bool? isTakeOverApplication { get; set; }

        public string customerType { get; set; }
        public short? customerTypeId { get; set; }
        public int loanDetailReviewTypeId { get; set; }
        public string customerAccountNumber { get; set; }

        public bool requireCollateral { get; set; }

        public int loanApplicationDetailId { get; set; }

        public int loanApplicationId { get; set; }

        public string applicationReferenceNumber { get; set; }

        public int customerId { get; set; }
        public int? customerGroupId { get; set; }

        public string customerName { get; set; }

        public decimal? equityAmount { get; set; }

        public int? equityCasaAccountId { get; set; }

        public short proposedProductId { get; set; }

        public string proposedProductName { get; set; }

        public int proposedTenor { get; set; }

        public double? proposedInterestRate { get; set; }

        public decimal proposedAmount { get; set; }

        public int? flowChangeId { get; set; }

        public short approvedProductId { get; set; }

        public string productName { get; set; }

        public int approvedTenor { get; set; }

        public int? tenorModeId { get; set; }

        public int? tenorFrequencyTypeId
        {
            get
            {
                return tenorModeId == null ? (int?)TenorMode.Daily : tenorModeId; // default to days
            }
        }

        public double approvedInterestRate { get; set; }

        public decimal approvedAmount { get; set; }

        public short currencyId { get; set; }

        public string currencyName { get; set; }

        public double exchangeRate { get; set; }

        public decimal exchangeAmount { get { return (decimal)exchangeRate * proposedAmount; } }

        public short subSectorId { get; set; }

        public short statusId { get; set; }

        public int? casaAccountId { get; set; }
        public int? operatingCasaAccountId { get; set; }

        public short sectorId { get; set; }

        public short? productClassId { get; set; }
        public short? productTypeId { get; set; }

        public short? productClassProcessId { get; set; }

        public string loanPurpose { get; set; }

        public string repaymentTerm { get; set; }

        public int? repaymentScheduleId { get; set; }

        public int? crmsFundingSourceId { get; set; }

        public int? crmsPaymentSourceId { get; set; }

        public int? exclusiveOperationId { get; set; }

        public string crmsFundingSourceCategory { get; set; }

        public string crms_ECCI_Number { get; set; }

        public string conditionPrecedent { get; set; }

        public string cflRrequestId { get; set; }

        public string conditionSubsequent { get; set; }

        public string transactionDynamics { get; set; }

        public string fieldOne { get; set; }

        public string fieldTwo { get; set; }

        public decimal? fieldThree { get; set; }

        public bool isSpecialised { get; set; }

        public short? productPriceIndexId { get; set; }

        public double? productPriceIndexRate { get; set; }

        public int loanTypeId { get; set; }

        public List<InvoiceDetailViewModel> invoiceDetails { get; set; }

        public EducationLoanViewModel educationLoan { get; set; }

        public TraderLoanViewModel traderLoan { get; set; }
        public List<ProductFeesViewModel> productFees { get; set; }
        public BondsAndGuranty bondDetails { get; set; }
        public List<SyndicatedLoanDetailViewModel> syndicatedLoan { get; set; }
        public IEnumerable<LoanCreditBureauViewModel> LoanCreditBereauReport { get; set; }
        public string sectorName { get; set; }
        public string productClass { get; set; }

        public string interestRepayment { get; set; }
        public int? interestRepaymentId { get; set; }
        public string moratorium { get; set; }
        public bool? isMoratorium { get; set; }
        public decimal? approvedLineLimit { get; set; }



        public string priceIndexName { get; set; }
        public int? priceIndexId { get; set; }
        public double priceIndexRate { get; set; }
        public string liborInfo { get { return priceIndexId == null ? "" : "(" + priceIndexName + ")"; } }

        public string proposedTenorString
        {
            get
            {
                var units = proposedTenor == 1 ? " day" : " days";
                if (proposedTenor < 15) return proposedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(proposedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public string approvedTenorString
        {
            get
            {
                var units = approvedTenor == 1 ? " day" : " days";
                if (approvedTenor < 15) return approvedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(approvedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public int proposedTenorConverted
        {
            get
            {
                int tenor = 0;
                switch (tenorModeId)
                {
                    case (int)TenorMode.Daily: tenor = proposedTenor; break;
                    case (int)TenorMode.Monthly: tenor = proposedTenor / 30; break;
                    case (int)TenorMode.Yearly: tenor = proposedTenor / 365; break;
                    default: tenor = proposedTenor; break;
                }
                return tenor;
            }
        }

        public string email { get; set; }
        public short? requireCollateralTypeId { get; set; }
        public int relationshipOfficerId { get; set; }
        public string breachedLimitName { get; set; }
        public int approvalTrailId { get; set; }
        public string approvalStatus { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string currentApprovalLevel { get; set; }
        public bool isTemplateUploaded { get; set; }
        public string loanInformation { get; set; }
        public short branchId { get; set; }
        public string relationshipOfficerName { get; set; }
        public int relationshipManagerId { get; set; }
        public string relationshipManagerName { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public double interestRate { get; set; }
        public bool isRelatedParty { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public bool submittedForAppraisal { get; set; }
        public string loanTypeName { get; set; }
        public int applicationTenor { get; set; }
        public decimal applicationAmount { get; set; }
        public string collateralDetail { get; set; }
        public bool isEmployerRelated { get; set; }
        public string employer { get; set; }
        public DateTime systemDateTime { get; set; }
        public string facility { get; set; }
        public string productClassName { get; set; }
        public string divisionCode { get; set; }
        public string divisionShortCode { get; set; }
        public int applicationId { get; set; }
        public string obligorName { get; set; }
        public double proposedRate { get; set; }
        public double approvedRate { get; set; }
    }
    public class SubsidiaryViewModel
    {
        public int subsidiaryId { get; set; }
        public string subsidiaryName { get; set; }
        public int countryId { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string relatedReferenceNumber { get; set; }
        public int? customerId { get; set; }
        public long? customerGlobalId { get; set; }
        public string countryCode { get; set; }
        public string productClassName { get; set; }
        public string productClassProcess { get; set; }
        public DateTime applicationDate { get; set; }
        public DateTime? systemDateTime { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal totalExposureAmount { get; set; }
        public double interestRate { get; set; }
        public int applicationTenor { get; set; }
        public int currentApprovalLevelId { get; set; }
        public int currentApprovalLevelTypeId { get; set; }
        public int? toStaffId { get; set; }
        public string divisionCode { get; set; }
        public DateTime? timeIn { get; set; }
        public short approvalStatusId { get; set; }
        public short applicationStatusId { get; set; }
        public string operationName { get; set; }
        public string customerName { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public int? createdBy { get; set; }
        public string createdByName { get; set; }
        public int? targetId { get; set; }
        public int? operationId { get; set; }
        public int subBasicId { get; set; }
        public bool actedOn { get; set; }
        public string loanTypeName { get; set; }
        public string divisionShortCode { get; set; }
        public string facility { get; set; }
        public int subApprovalLevelId { get; set; }
        public bool submitted { get; set; }
        public string referenceNumber { get; set; }
    }
}

