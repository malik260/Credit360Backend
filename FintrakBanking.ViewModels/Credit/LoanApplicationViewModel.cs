using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.ViewModels.Credit
{

    public class LoanApplicationViewModel : GeneralEntity
    {
        public LoanApplicationViewModel()
        {
            LoanApplicationDetail = new List<LoanApplicationDetailViewModel>();
        }

        public bool isNewApplication { get; set; }
        public bool closeApplication { get; set; }        
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int? customerId { get; set; }
        public int? operationId { get; set; }
        public bool requireCollateral { get; set; }
        public DateTime newApplicationDate { get; set; }
        
        public short? branchId { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public short productId { get; set; }
        public int? customerGroupId { get; set; }
        public string customerGroupCode { get; set; }
        public short loanTypeId { get; set; }
        public int casaAccountId { get; set; }
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public short loanStatusId { get; set; }
        public string loanStatus { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public double applicationTenor { get; set; }
        public double interestRate { get; set; }
        public DateTime ? effectiveDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string customerAccount { get; set; }
        public short tenorModeId { get; set; }
        public string loanInformation { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public bool submittedForAppraisal { get; set; }
        public bool isRelatedParty { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public short approvalStatusId { get; set; }

        public decimal proposedAmount { get; set; }
        public int proposedTenor { get; set; }
        public int approvalLevelId { get; set; }

        public short subSectorId { get; set; }
        public int sectorId { get; set; }
        public string sectorName { get; set; }
        public bool isInvestmentGrade { set; get; }
        public string customerName { get; set; }
        public string branchName { get; set; }
        public string productName { get; set; }
        public string customerGroupName { get; set; }

        public string loanTypeName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string tenorModeName { get; set; }

        //public string amount { get { return this.principalAmount.ToString("#,#.00#"); } }
        public string applicantName { get { return this.customerName + "(" + this.customerGroupName + ")"; } }
        public int lmsApplicationDetailId { get; set; }

        public int? loanPreliminaryEvaluationId { get; set; }
        public double exchangeRate { get; set; }
        public List<LoanApplicationCollateralViewModel> LoanApplicationCollateral { get; set; }
        public List<LoanApplicationDetailViewModel> LoanApplicationDetail { get; set; }
        public IEnumerable<ApprovedLoanDetailViewModel> details { get; set; }
        public int? currentApprovalStateId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string currentApprovalLevel { get; set; }
        public string lastComment { get; set; }
        public int approvalTrailId { get; set; }
        // public decimal? approvedAmount { get; set; }
        public short applicationStatusId { get; set; }

        public bool isCollateralBacked { get; set; }

        public string collateralDetail { get; set; }
        //private int _tenor;

        public int tenor { get; set; }
        public bool customerInfoValidated { get; set; }
        public bool notInNegativeCrms { get; set; }
        public bool notInBlackbook { get; set; }
        public bool notInCamsol { get; set; }
        public bool notInXds { get; set; }
        public bool notInCrc { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string customerCode { get; set; }
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
        
        public string tenorString
        {
            get
            {
                var units = applicationTenor == 1 ? " day" : " days";
                if (applicationTenor < 15) return applicationTenor.ToString() + units;
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
        public string comment { get; set; }
        public bool isOfferLetterAvailable { get; set; }
        public int? currentApprovalLevelTypeId { get; set; }
        public short? tranchLevelId { get; set; }
        public int? regionId { get; set; }
        public bool editMode { get; set; }
        public short? requireCollateralTypeId { get; set; }
    }

    public class LoanApplicationUpdateMessage
    {
        public bool isdone { get; set; } 
        public string messageStr { get; set; }
        public int checkListIndex { get; set; }
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

        public int applicationStatusPosition { get; set; }

        public string currencyCode { get; set; }
        public DateTime applicationDate { get; set; }
        public short applicationStatusId { get; set; }
        public int approvalStatusId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string branchName { get; set; }
        public string customerGroupName { get; set; }

        public string customerType { get; set; }
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

        public short sectorId { get; set; }

        public short? productClassId { get; set; }

        public short? productClassProcessId { get; set; }

        public string loanPurpose { get; set; }

        public string repaymentTerm { get; set; }

        public int?  crmsFundingSourceId { get; set; }

        public int? crmsPaymentSourceId { get; set; }

        public string crmsFundingSourceCategory { get; set; }

        public string crms_ECCI_Number { get; set; }

        public string conditionPrecedent { get; set; }

        public string conditionSubsequent { get; set; }

        public string transactionDynamics { get; set; }

        public string fieldOne { get; set; }

        public string fieldTwo { get; set; }

        public decimal? fieldThree { get; set; }

        public bool isSpecialised { get; set; }

        public short? productPriceIndexId { get; set; }

        public double? productPriceIndexRate { get; set; }

        public List<InvoiceDetailViewModel> invoiceDetails { get; set; }

        public EducationLoanViewModel educationLoan { get; set; }

        public TraderLoanViewModel traderLoan { get; set; }
        public List<ProductFeesViewModel> productFees { get; set; }
        public BondsAndGuranty bondDetails { get; set; }
        public List<SyndicatedLoanDetailViewModel> syndicatedLoan { get; set; }
        public IEnumerable<LoanCreditBureauViewModel> LoanCreditBereauReport { get; set; }
        public string sectorName { get; set; }
        public string productClass { get; set; }


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

        // public int? loanreViewApplicationId { get; set; }
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
        public int productTypeId { get; set; }
        public string searchString { get; set; }
        public int loanSystemTypeId { get; set; }
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
        public string customerType;

        public string applicationRefNo { get; set; }

        public short proposedProductId { get; set; }

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


    }

}
