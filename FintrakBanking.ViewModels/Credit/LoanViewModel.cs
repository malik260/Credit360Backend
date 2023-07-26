using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanRepaymentScheduleViewModel
    {
        public int loanId { get; set; }
        public int customerId { get; set; }
        public decimal principalRepayment { get; set; }
        public decimal principalAmount { get; set; }
        public decimal interestAccrual { get; set; }
        public string productName { get; set; }
        public double interestRate { get; set; }
        public DateTime terminationDate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string loanReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public string totalRepayment { get { return (principalRepayment + interestAccrual).ToString("#,#.00#"); } }

        public int loanApplicationId { get; set; }
        public int productTypeId { get; set; }
    }

    public class FacilityReport
    {
        public string loanType { get; set; }
        public string facilityType { get; set; }
        public string refNo { get; set; }
        public string customerNames { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal unitlizedAmount { get; set; }
        public double accountBalance { get; set; }
        public double tenor { get; set; }
        public double interest { get; set; }
        public DateTime dateApproved { get; set; }
        public string branchName { get; set; }
        
    }

    public class LoanViewModel : GeneralEntity
    {
        public short? approvedLineStatusId { get; set; }

        public string divisionShortCode { get; set; }

        public string divisionCode { get; set; }

        public string facilityType { get; set; }
        public Decimal sanctionLimit { get; set; }
        public bool isSuspenseCredit { get; set; }
        public decimal bookedAmount { get; set; }
        public DateTime operationDate { get; set; }
        public decimal prepaymentAmount { get; set; }
        public string productAccountName2 { get; set; }
        public string businessUnit { get; set; }
        public string nameOfRM { get; set; }
        public decimal facilityRate { get; set; }
        public string operationTypeName { get; set; }
        public int? currentApprovalLevelId { get; set; } 
        public int approvedTenor { get; set; }
        public string staffCode { get; set; }
        public int tenorToDate { get; set; }

        public string payingAccountNumber { get; set; }

        public string legalContingentCode { get; set; } 
        public int zeroToThirtyDays { get; set; }
        public int ThirtyOneToNinety { get; set; }
        public int ninetyOneToOneEightyDays { get; set; }
        public int OneEightyDaysToThreeSixtyDays { get; set; }
        public int overOneToThreeYears { get; set; }
        public int overThreeYears { get; set; }
        public double? newInterestRate { get; set; }
        public decimal? newLineAmount { get; set; }
        public int? loanReviewOperationId { get; set; }
        public string operationPerformed { get; set; }
        public short? instructionTypeId { get; set; }
        public string instructionTypeName { get; set; }
        public string customerAcct { get; set; }


        public short productPriceIndexId { get; set; }
        public int tenorLeft { get; set; }

        public string customerType { get; set; }

        public string groupCustomerName { get; set; }

        public short trailApprovalStatus { get; set; }
        public string currencyCode { get; set; }

        public string userActivity { get; set; }
        public string ApprovalStatus { get; set; }
        public string approvedByName { get; set; }
        public string approvalStatusName { get; set; }

        public short requestStatusId { get; set; }
        public bool requestDeleted { get; set; }
        public string casaAccountDetails { get; set; }


        public decimal disbursableAmount { get; set; }
        public string loanStatusName { get; set; }
        public bool isBidbond { get; set; }
        public bool isOverdraft { get; set; }
        public string commercialPrincipal { get; set; }

        public int notificationDuration { get; set; }
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public double productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public int? casaAccountId2 { get; set; }
        public short? crmsRepaymentAgreementTypeId { get; set; }
        public int loanApplicationId { get; set; }
        public int? loanReviewApplicationId { get; set; }

        public int loanApplicationDetailId { get; set; }
        public double approvedInterestRate { get; set; }
        
        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string relatedloanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public bool? isLineFacility { get; set; }
        public string lmsApplicationReferenceNumber { get; set; }

        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public int tenorUsed { get; set; }
        public short? principalFrequencyTypeId { get; set; }
        public short? interestFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal principalAmount { get; set; }
        public decimal? writtenOffAccruedAmount { get; set; }
        public decimal? principalAmountz { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int interestInstallmentLeft { get; set; }
        public int approvalStatusId { get; set; }
        public int? approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public string loanStatus { get; set; }
        public short scheduleTypeId { get; set; }
        public string scheduleTypeName { get; set; }
        public bool shouldDisbursed { get; set; }
        public bool isDisbursed { get; set; }
        public int? disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public int? appraisalOperationId { get; set; }
        public int? operationTypeId { get; set; }
        public string operationName { get; set; }
        public string reviewLoanDetaile { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public short? loanTypeId2 { get; set; }
        public decimal overDraft { get; set; }
        public string archiveCode { get; set; }
        public int contigentAmount {get; set;}
        public string branchCode { get; set; }
        public DateTime? crmsDate { get; set; }

        public decimal equityContribution { get; set; }
        public short subSectorId { get; set; }
        public DateTime? firstPrincipalPaymentDate { get; set; }
        public DateTime? firstInterestPaymentDate { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public decimal outstandingInterest { get; set; }
        public int? principalAdditionCount { get; set; }
        public int? principalReductionCount { get; set; }
        public bool fixedPrincipal { get; set; }
        public bool profileLoan { get; set; }
        public bool dischargeLetter { get; set; }
        public bool suspendInterest { get; set; }
        public bool booked { get; set; }
        public bool? scheduled { get; set; }
        public bool? isScheduledPrepayment { get; set; }
        public decimal? scheduledPrepaymentAmount { get; set; }
        public DateTime? scheduledPrepaymentDate { get; set; }
        public short? scheduledPrepaymentFrequencyTypeId { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string casaAccountNumber  { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public decimal integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }
        public int? internalPrudentialGuidelineStatusId { get; set; }
        public int? externalPrudentialGuidelineStatusId { get; set; }
        public int? userPrudentialGuidelineStatusId { get; set; }
        public string internalPrudentialGuidelineStatus { get; set; }
        public string externalPrudentialGuidelineStatus { get; set; }
        public string userPrudentialGuidelineStatus { get; set; }
        public DateTime? nplDate { get; set; }
        public short scheduleDayCountConventionId { get; set; }
        public short scheduleDayInterestTypeId { get; set; }
        public int? customerRiskRatingId { get; set; }
        public int teno { get; set; }

        public bool allowForceDebitRepayment { get; set; }

        //.............Fee Attribute.....................//
        public double exchangeRate { get; set; }

        public DateTime paymentDate { get; set; }
        public decimal totalAmount { get; set; }
        public int chargeFeeId { get; set; }
        public decimal facilityLimit { get; set; }

        //...................For Loan Review................//
        public int loanReviewOperationTypeId { get; set; }
        public string reviewDetails { get; set; }
        //.............Other Attributes................//
        public int productTypeId { get; set; } 
        public int? productClassId { get; set; }
        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string loanPurpose { get; set; }

        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string customerGroupName { get; set; }
        public string principalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public string comment { get; set; }
        public string relationshipManagerEmail { get; set; }
        public string relationshipOfficerEmail { get; set; }
        public decimal customerAvailableAmount { get; set; }
        public bool feeOverride { get; set; }
        public int loanBookingRequestId { get; set; }
        //public int staffId { get; set; }
        public int staffName { get; set; }
        public bool hasLein { get; set; }
        public int postNoStatusId { get; set; }
        public int lmsApplicationDetailId  { get; set; }

        public decimal availableBalance { get; set; }

        public decimal overdraftDrawnAmount{ get; set; }
        public decimal overdraftUndrawnAmount { get; set; }

        public bool isExternalFacility { get; set; }
        public bool isSavedExternalFacility { get; set; }

        public List<ApprovalLevelStaffViewModel> loanOperationApprovers { get; set; }

        //............Loan Repayment Schedule Model..........................//
        public LoanPaymentScheduleInputViewModel loanScheduleInput { get; set; }

        //............End of Loan Repayment Schedule Model.....................//

        //...........Other Loans Types Model........................//
        public RevolvingLoanViewModel revolvingLoanInput { get; set; }

        public ContingentLoanViewModel contingentLoanInput { get; set; }
        //...........End of Other Loans Types Model.................//

        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        public List<LoanMonitoringTriggerViewModel> monitoringTriggers { get; set; }
        public decimal overdraftLimit { get; set; }
        public bool maintainTenor { get; set; }
        public decimal? accrualedAmount { get; set; }       
        public int newTenor { get; set; }
        public short scheduleTypeCategoryId { get; set; }
        public DateTime previousEffectiveDate { get; set; }
        public decimal pastDueTotal { get; set; }
        public decimal overDraftCheckAmount { get; set; }

        public DateTime? pastDueDate { get; set; }
        public int? pastDueDays { get; set; }
        public decimal pastDuePrincipal { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal interesrtOnPastDueInterest { get; set; }
        public decimal penalChargeAmount { get; set; }
        public DateTime? lastRestructureDate  { get; set; }
        public short loanSystemTypeId  { get; set; }
        public bool isPerforming { get; set; }
        public decimal? loanPrincipal { get; set; }
        public List<LoanDisbursementViewModel> loanBeneficiary { get; set; }
        public string approvedComment { get; set; }
        public decimal scheduleDayCountConvention { get; set; }
        public string isDisbursedState { get; set; }
        public string productPriceIndexName { get; set; }
        public string revolvingType { get; set; }
        public short revolvingTypeId { get; set; }
        public string istenored { get; set; }
        public string isbankFormat { get; set; }
        public int loadArchiveId { get; set; }
        public bool isTermLoam { get; set; }
        public bool isOD { get; set; }
        public int postedByStaffId { get; set; }
        public string batchNo { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime? postedDate { get; set; }
        public DateTime? postedTime { get; set; }
        public double currencyRate { get; set; }
        public string postCurrency { get; set; }
        public string sourceReferenceNumber { get; set; }
        public decimal requestedAmount { get; set; }
        public string remark { get; set; }
        public string nostroAccountId { get; set; }
        public string nostroRateCode { get; set; }
        public string notstroCurrency { get; set; }
        public decimal? nostroRateAmount { get; set; }
        public string nostroAccount { get; set; }
        public int? nostroRateCodeId { get; set; }
        public string baseReferenceNumber { get; set; }
        public string categoryName { get; set; }
        public string currencyName { get; set; }
        public int tenorToMaturity { get; set; }
        public decimal dailyAccrualAmount { get; set; }
        public DateTime date { get; set; }
        public decimal mainAmount { get; set; }
        public bool writtenOff { get; set; }
        public DateTime firstPrincipalPaymentDate1 { get; set; }
        public DateTime firstInterestPaymentDate1 { get; set; }
        public string productPriceIndex { get; set; }
        public DateTime? timeIn { get; set; }
        public string approvedTenorString
        {
            get
            {
                var units = tenor == 1 ? " day" : " days";
                if (tenor < 15) return tenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(tenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public int tenorLms { get { return (this.approvedTenor); } }
        public string approvedLoanTenorString
        {
            get
            {
                var units = tenorLms == 1 ? " day" : " days";
                if (tenorLms < 15) return tenorLms.ToString() + units;
                var months = Math.Ceiling((Math.Floor(tenorLms / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public int performanceTypeId
        {
            get
            {
                if (writtenOff) return 3;
                if (!isPerforming) return 2;
                return 1;
            }
        }

        public string performanceType
        {
            get
            {
                if (writtenOff) return "Written Off";
                if (!isPerforming) return "Non Performing";
                return "Performing";
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

        public string casaAccountNumber2 { get; set; }
        public bool isInEditMode { get; set; }
        public LoanReviewOperationApprovalViewModel operationReview { get; set; }
        public CamProcessedLoanViewModel processLoanApplicationDetails { get; set; }
        public decimal totalOutstandingAmount { get; set; }
        public decimal totalExistingLimitAmount { get; set; }
        public string crmsCode { get; set; }
        public string customerEmail { get; set; }
        public decimal? totalRepayment { get; set; }
        public string loanSystemTypeName { get; set; }
        public decimal writtenOffAmount { get; set; }
        public bool passed { get; set; }
        public List<string> errorMessage { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string responsiblePerson { get; set; }
        public short productClassProcessId { get; set; }
        public short? productClassProcessId2 { get; set; }
        public short loanApplicationTypeId { get; set; }
        public int appraisalApplicationId { get; set; }
        public int? appraisalCustomerId { get; set; }
        public int? appraisalGroupCustomerId { get; set; }
        public int appraisalLoanReviewApplicationId { get; set; }
        public string appraisalApplicationReferenceNumber { get; set; }
        public int appraisalLoanApplicationId { get; set; }
        public int? synOperationId { get; set; }
        public int creditAppraisalOperationId { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int? tenorModeId { get; set; }
        public int applicationDetailId { get; set; }
        public string appraisalOperationName { get; set; }
        public short sectorId { get; set; }
        public decimal? contigentOutstandingPrincipal { get; set; }
        public decimal? totalPrepayment { get; set; }
        public DateTime? issueDate { get; set; }
        public DateTime expiryDate { get; set; }
        public string guaranteeType { get; set; }
        public string customerTier { get; set; }
        public string apgAccountNumber { get; set; }
        public string projectDetails { get; set; }
        public string projectLocation { get; set; }
        public string projectRiskRatings { get; set; }
        public string contractEmployer { get; set; }
        public string ccy { get; set; }
        public decimal guaranteeAmount { get; set; }
        public string relationshipTeam { get; set; }
        public string accountOfficer { get; set; }
        public decimal? exposureOnGuarantee { get; set; }
        public string sbu { get; set; }
        public decimal? rebookedAmount { get; set; }
        public DateTime? dateRebooked { get; set; }
        public decimal? currentExposure { get; set; }
        public decimal? previousExposure { get; set; }
        public DateTime? dateAmortised { get; set; }
        public decimal? amortisedAmount { get; set; }
        public DateTime? dateDischarged { get; set; }
        public decimal? prepayment { get; set; }
        public int lmsOperationId { get; set; }
        public int lmsLoanApplicationId { get; set; }
        public DateTime lmsdatecreated { get; set; }



        //......End f Loan Relational Table View Mapping Models......//
    }

    public class RevolvingLoanViewModel : GeneralEntity
    {
        public string divisionShortCode { get; set; }

        public string divisionCode { get; set; }

        public decimal bookedAmount { get; set; }

        public short scheduleDayCountConventionId { get; set; }

        public short? crmsRepaymentAgreementTypeId { get; set; }

        public string productAccountName2 { get; set; }

        public string currencyCode { get; set; }

        public string revolvingTypeName { get; set; }

        public string casaAccountDetails { get; set; }

        public string casaAccountNumber { get; set; }

        public decimal disbursableAmount { get; set; }
        public string loanStatus { get; set; }
        public bool isOverdraft { get; set; }
        public bool isBidbond { get; set; }
        public short accrualBasis { get; set; }
        public bool isTemporaryOverdraft { get; set; }
        public short revolvingTypeId { get; set; }

        public string serialNumber { get; set; }
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
        public short branchId { get; set; }
        public short currencyId { get; set; }
        public double exchangeRate { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string relatedLoanReferenceNumber  { get; set; }
        public short subSectorId { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal overdraftLimit { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal? disbursedAmount { get; set; }
        public decimal? interestAmount { get; set; }
        public int approvalStatusId { get; set; }
        public int approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public bool isDisbursed { get; set; }
        public string disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public int? operationId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int? operationTypeId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public string trancheBatchCode { get; set; }
        public bool dischargeLetter { get; set; }
        public bool suspendInterest { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public int? internalPrudentialGuidelineStatusId { get; set; }
        public int? externalPrudentialGuidelineStatusId { get; set; }
        public DateTime? nplDate { get; set; }
        public DateTime? crmsDate { get; set; }
        public string crmsCode { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; } 
        public int? productClassId { get; set; }
        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string loanStatusName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string teamMiscode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public string comment { get; set; }
        public int loanBookingRequestId { get; set; }
        //.............End of Other Attributes...........//

        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        public List<LoanMonitoringTriggerViewModel> monitoringTriggers { get; set; }
        //......End f Loan Relational Table View Mapping Models......//

        public decimal pastDuePrincipal { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal interesrtOnPastDueInterest { get; set; }
        public decimal penalChargeAmount { get; set; }
        public int dayCountConventionId { get; set; }
        public short loanSystemTypeId { get; set; }
        public int? userPrudentialGuidelineStatusId { get; set; }

        public DateTime? timeIn { get; set; }

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

        public int principalAmount { get; set; }
    }

    public class ContingentLoanViewModel : GeneralEntity
    {
        public string divisionShortCode { get; set; }

        public string divisionShortCodem { get; set; }

        public string divisionCode { get; set; }

        public string currencyCode { get; set; }
        public string casaAccountDetails { get; set; }

        public decimal disbursableAmount { get; set; }
        public bool isBidbond { get; set; }
        public bool isOverdraft { get; set; }

        public int loanId { get; set; }
        public int loanApplicationId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
        public string casaAccountNumber { get; set; }
        public short branchId { get; set; }
        public short currencyId { get; set; }
        public double exchangeRate { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string loanReferenceNumber { get; set; }
        public short subSectorId { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime bookingDate { get; set; }
        public DateTime systemArrivalDateTime { get; set; }
        public decimal contingentAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public int approvalStatusId { get; set; }
        public int approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public bool isDisbursed { get; set; }
        public string disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public int? operationId { get; set; }
        public int? operationTypeId { get; set; }
        public int? appraisalOperationId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public string trancheBatchCode { get; set; }
        public bool dischargeLetter { get; set; }
        public short customerSensitivityLevelId { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; } 
        public int? productClassId { get; set; }
        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string productAccountName2 { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string loanStatusName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string teamMiscode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public string comment { get; set; }
        public int loanBookingRequestId { get; set; }
        public DateTime? crmsDate { get; set; }
        public string crmsCode { get; set; }
        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        public List<LoanMonitoringTriggerViewModel> monitoringTriggers { get; set; }
        public short loanSystemTypeId { get; set; }
        //......End f Loan Relational Table View Mapping Models......//

        public DateTime?  timeIn { get; set; }
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

    public class LoanBookingRequestViewModel : GeneralEntity
    {
        public bool? chargeFeeOnce { get; set; }
        public int? customerId { get; set; }
        public int? casaAccountId { get; set; }
        public int? casaAccountId2 { get; set; }
        public bool? isUsed { get; set; }
        public string currencyCode { get; set; }

        public int loanBookingRequestId { get; set; }
        
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }

        public decimal amount_Requested { get; set; }

        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        public string customerName { get; set; }
        public string approvedProductTypeName { get; set; }
        public int approvedProductTypeId { get; set; }
        public decimal approvedAmount { get; set; }
        public short productId { get; set; }
        public int? operationId { get; set; }
        public int? tenor { get; set; }
        public int toStaffId { get; set; }
        public bool isLienPlacementForLoan { get; set; }


    }

    public class CustomerExposure
    {
        public int customerId { get; set; }
    }

    public class CamProcessedLoanViewModel : LoanApplicationViewModel
    {
        public short? documentProvided { get; set; }

        //public bool? isAwaitingLineMaintenance { get; set; }

        public CamProcessedLoanViewModel()
        {
            bookedLoanData = new LoanViewModel();
            bookedRevolvingFacilityData = new RevolvingLoanViewModel();
        }

        public bool isLineMaintained { get; set; }

        public bool? isLineFacility { get; set; }

        public string isLineFacilityString { get; set; }
        public DateTime systemArrivalDateTime { get; set; }

        public int appraisalOperationId { get; set; }

        public int appraiselOperationId { get; set; }

        public string crmsCode { get; set; }

        public string productClassName { get;set;}

        public double approvedInterestRate { get; set; }
        public List<feeDetails> fees { get; set; }

        public string feeAccountName { get; set; }
        public int? loanSystemTypeId { get; set; }

        public bool isLocalCurrrency { get; set; }
        public int? loanId { get; set; }
        public string productPriceDescription { get; set; }
        public string reviewDetails { get; set; }
        public short? productScheduleTypeId { get; set; }
        public short? productDealTypeId { get; set; }
        public short? productDayCountConventionId { get; set; }
        public string lmsApplicationReferenceNumber { get; set; }
        public int? loanReviewApplicationId { get; set; }
        public LoanReviewOperationApprovalViewModel operationReview { get; set; }

        public int? loanReviewOperationId { get; set; }

        public int requestStaffId { get; set; }
        public int? toApprovalLevelId { get; set; }

        public string approvalStatusName { get; set; }
        public string lmsOperationName { get; set; }
        public int? lmsOperationId { get; set; }

        public string operationName { get; set; }

        public int tenorUsed { get; set; }

        public bool isBooked { get; set; }

        public bool isUnderApproval { get; set; }
        public bool isApprovalOwner { get; set; }
        public string routedToStaff { get; set; }
        public bool canReRouteBooking { get; set; }

        public decimal approveRequestAmount { get; set; }
        public decimal pendingRequestAmount { get; set; }
        public decimal allRequestAmount { get; set; }
        public decimal disapprovedCount { get; set; }
        public decimal disApprovedAmount { get; set; }

        public bool? isTemporaryOverdraft { get; set; }

        public string repaymentTerms { get; set; }
        public string repaymentSchedule { get; set; }

        public bool isFinal { get; set; }
        public bool isFirstApprover { get; set; }
        public bool isBidbond { get; set; }
        public bool isOverdraft { get; set; }

        public DateTime? approvedDate { get; set; }
        public List<LoanMonitoringTriggerViewModel> loanMonitoringTrigger { get; set; }

        public decimal requestedAmount { get; set; }
        public DateTime? approvalDate { get; set; }

        public DateTime? availmentDate { get; set; }

        public string loanDetails { get; set; }
        public string camReference { get; set; }
        public List<CasaViewModel> customerAccounts { get; set; }
        public int? appraisalMemorandumId { get; set; }
        public int? casaAccountId { get; set; }
        public string casaAccountNumber { get; set; }
        public int? casaAccountId2 { get; set; }
        public string casaAccountNumber2 { get; set; }
        public string loanStatusName { get; set; }
        public string sectorName { get; set; }
        public string subSectorName { get; set; }
        public string sectorSubSectorName { get {return (this.sectorName + "/" + this.subSectorName); } } 
        public short productTypeId { get; set; }
        public short? productPriceIndexId { get; set; }
        public double? productPriceIndexRate { get; set; }
        public string productTypeName { get; set; }
        public int customerSensitivityLevelId { get; set; }
        public string camDocumentation { get; set; }
        public decimal groupApprovedAmount { get; set; }
        public int approvedTenor { get; set; }
        public short approvalStatusId { get; set; }
        public short applicationStatusId { get; set; }
        public int proposedTenor { get; set; }
        public decimal ? customerAvailableAmount { get; set; }
        public string customerOccupation { get; set; }
        public string customerType { get; set; }
        public int? bookingRequestStatusId { get; set; }
        public decimal? bookingAmountRequested { get; set; }
        public DateTime requestDate { get; set; }
        public string requestedBy { get; set; }
        public short requestOperationId { get; set; }
        public decimal amountDisbursed { get; set; }
        public int loanBookingRequestId { get; set; }
        //public int operationId { get; set; }
        public bool isInEditMode { get; set; } 
        public string purpose { get; set; }
        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanApplicationCollateralViewModel> loanApplicationCollateral { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        public CustomerCompanyInfomationViewModels companyInformation { get; set; }
        public List<CamDocumentViewModel> camDocuments { get; set; }
        public LoanViewModel bookedLoanData { get; set; }
        public RevolvingLoanViewModel bookedRevolvingFacilityData { get; set; }
        public short? productClassProcessId { get; set; }
        public bool undergoingConcession { get; set; }
        public string productPriceIndex { get; set; }
        public bool isLocalCurrency { get; set; }
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
        public string newTenorString
        {
            get
            {
                var units = tenor == 1 ? " day" : " days";
                if (tenor < 15) return tenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(tenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }
        public string approvedAmountCurrency { get; set; }
        public bool atInitiator { get; set; }
        public double? newinterestRate { get; set; }
        public decimal? newLineAmount { get; set; }
        public int interestRateString { get; set; }
        public string interestRateAndFees { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int? capRegionId { get; set; }
        public string apiRequestId { get; set; }
        public int creditAppraisalOperationId { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int documentationFillingId { get; set; }
        public int requestId { get; set; }
        public string module { get; set; }
        public int? flagStatus { get; set; }
        public DateTime fillingRequestDate { get; set; }
        public bool isProjectRelate { get; set; }
        public int trailId { get; set; }



        //......End f Loan Relational Table View Mapping Models......//
    }

    public class AppraisalMemorandumLoanDetailViewModel
    {
        public int appraisalMemorandumLoanDetailId { get; set; }

        public int appraisalMemorandumId { get; set; }

        public decimal principalAmount { get; set; }

        public double interestRate { get; set; }

        public int tenor { get; set; }
    }

    public class LoanChargeFeeViewModel : ChargeRangeViewModel //GeneralEntity //ChargeRangeViewModel

    {
        public int dealTypeId { get; set; }
        public string productName { get; set; }
        public string casaAccountName { get; set; }
        public decimal casaAccountBalance { get; set; }
        public string productTypeName { get; set; }
        public string customerName { get; set; }
        public string loanReferenceNumber { get; set; }

        public int productFeeId { get; set; }
        public int loanChargeFeeId { get; set; }
        public short loanSystemTypeId { get; set; }
        public int applicationDetailIdId { get; set; }
        public int loanId { get; set; }
        public int productId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeRateValue { get; set; }
        public decimal feeDependentAmount { get; set; }
        public decimal chargeAmount { get; set; }
        public decimal feeAmount { get; set; }
        public short feeIntervalId { get; set; }
        public string feeIntervalName { get; set; }
        public int feeTypeId { get; set; }
        public string feeTypeName { get; set; }
        public bool isIntegralFee { get; set; }
        public List<ChargeRangeViewModel> chargeRange { get; set; }
        public decimal newFeeAmount { get; set; }
        public decimal feeAmountDiff { get; set; }
        public int casaAccountId { get; set; }
        public bool required { get; set; }
        public bool recurring { get; set; }
        public int feeTargetId { get; set; }
        public string feeTargetName { get; set; }
        public bool byAmountRequired { get; set; }
        public bool isPosted { get; set; }
        public int operationId { get; set; }
        public decimal loanAmount { get; set; }
        public List<LoanChargeFeeViewModel> loanDeferredFeeList { get; set; }
        public DateTime effectiveDate  { get; set; }
        public decimal feeRate { get; set; }
        public string description { get; set; }
        public bool isDeferred { get; set; }
        public string feeSourceModule { get; set; }
        //public double? rate { get; set; }

    }

    public class LoanCollateralMappingViewModel : CollateralViewModel
    {
        public int loanCollateralMappingId { get; set; }
        public int loanId { get; set; }
        public short loanSystemTypeId { get; set; }
        public int? loanApplicationId { get; set; }

        
    }

    public class LoanGuarantorViewModel : GeneralEntity
    {
        public short loanGuarantorId { get; set; }
        public int loanApplicationId { get; set; }
        public string fullName { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string middlename { get; set; }
        public string phoneNumber1 { get; set; }
        public string phoneNumber2 { get; set; }
        public string address { get; set; }
        public string relationship { get; set; }
        public int? relationshipDuration { get; set; }
        public string emailAddress { get; set; }
        public string bvn { get; set; }
        public string rcNumber { get; set; }
        public string taxNumber  { get; set; }
        public string customerTypeName { get; set; }
        public short customerTypeId { get; set; }
        public string customerType { get; set; }
    }

    public class LoanFeeDefferalViewModel : LoanViewModel
    {
        public int loanChargeFeeid { get; set; }
        public short chargeFeeid { get; set; }
        public bool isPosted { get; set; }
        public decimal feeRateValue { get; set; }
        public decimal feeDependentAmount { get; set; }
        public decimal feeAmount { get; set; }
        public bool isIntegralFee { get; set; }
        public bool isRecurring { get; set; }
        public short recurringPaymentDay { get; set; }
        public decimal deferredFeeAmount { get; set; }
    }

    public class LoanSearchViewModel
    {
        public string customerName { get; set; }
        public string loanName { get; set; }
        public string loanReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }
    }
    public class LoanContingentViewModel : GeneralEntity
    {
        public int? casaAccountId;

        public int loanCovenantDetailId { get; set; }
        public string covenantDetail { get; set; }
        public int loanId { get; set; }
        public decimal? contingentAmount { get; set; }

        public string loanRef { get; set; }
        public string productName { get; set; }
        public int casaId { get; set; }
        public int maximumDrawDownDuration { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime? dueDate { get; set; }
        public string loanRefNumber { get; set; }
        public string relationshipManager { get; set; }
        public string managerEmail { get; set; }
        public string relationshipOfficer { get; set; }
        public string officerEmail { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }

        public int notificationDuration { get; set; }
        public int loanApplicationDetailId { get; set; }
        public bool isPercentage { get; set; }
        public DateTime? nextCovenantDate { get; set; }
        public short loanSystemTypeId { get; set; }
        public string productCustomerName { get; set; }
    }

    public class LoanCovenantDetailViewModel : GeneralEntity
    {
        public int? casaAccountId;

        public int loanCovenantDetailId { get; set; }
        public string covenantDetail { get; set; }
        public int loanId { get; set; }
        public short covenantTypeId { get; set; }
        public short? frequencyTypeId { get; set; }
        public decimal? covenantAmount { get; set; }
        public DateTime  covenantDate { get; set; }
        public string covenantTypeName { get; set; }
        public string frequencyTypeName { get; set; }
        public string loanRef { get; set; }
        public string productName { get; set; }
        public int casaId { get; set; }
        public int maximumDrawDownDuration { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime? dueDate { get; set; }
        public string loanRefNumber { get; set; }
        public string relationshipManager { get; set; }
        public string managerEmail { get; set; }
        public string relationshipOfficer { get; set; }
        public string officerEmail { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }

        public int notificationDuration { get; set; }
        public int loanApplicationDetailId { get; set; }
        public bool isPercentage { get; set; }
        public DateTime? nextCovenantDate { get; set; }
        public short loanSystemTypeId { get; set; }
        public string productCustomerName { get; set; }
        public bool isFinancial { get; set; }
    }

    public class LoanMonitoringTriggerViewModel : GeneralEntity
    {
        public short loanSystemTypeId { get; set; }
        public int loanMonitoringTriggerId { get; set; }
        public int loanId { get; set; }
        public short productTypeId { get; set; }
        public int? monitoringTriggerId { get; set; }
        public string monitoringTrigger { get; set; }
        public string monitoringTriggerSetupName { get; set; }
    }


    public class DailyInterestAccrualViewModel : GeneralEntity

    {
        public string referenceNumber { get; set; }

        public int? chargedFeeId { get; set; }

        public string baseReferenceNumber { get; set; }

        public DateTime maturityDate { get; set; }

        public DateTime effectiveDate { get; set; }
        
        public short categoryId { get; set; }

        public byte transactionTypeId { get; set; }

        public short productId { get; set; }

        //public int companyId { get; set; }

        public short branchId { get; set; }

        public short currencyId { get; set; }

        public double exchangeRate { get; set; }

        public decimal mainAmount { get; set; }

        public double interestRate { get; set; }

        public DateTime date { get; set; }

        public DateTime? pastDueDate { get; set; }

        public int ? gracePeriod { get; set; }
        public short dayCountConventionId { get; set; }

        public double dailyAccuralAmount { get; set; }

        public short accuralTypeId { get; set; }

        public decimal availableBalance { get; set; }

        public int daysInAYear { get; set; }

        public int loanChargedFeeId { get; set; }

        public int? loanId { get; set; }

        public string productCode { get; set; }
        public string currencyCode { get; set; }

        public string branchCode { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int loanApplicationDetailId { get; set; }

        //public string currencyCode { get; set; }
        //referenceNumber = groupedQ.Key.PRODUCTCODE + '/' + groupedQ.Key.CURRENCYCODE + '/' + groupedQ.Key.BRANCHCODE + '/' + groupedQ.Key.COMPANYID.ToString(),

    }


    //public class BulkIntegrationPostingViewModel : GeneralEntity

    //{
    //    public int sid { get; set; }

    //    public string batchId  { get; set; }

    //    public short categoryId { get; set; }

    //    public byte transactionTypeId { get; set; }

    //    public short productId { get; set; }

    //    //public int companyId { get; set; }

    //    public short branchId { get; set; }

    //    public short currencyId { get; set; }

    //    public double exchangeRate { get; set; }

    //    public decimal mainAmount { get; set; }

    //    public double interestRate { get; set; }

    //    public DateTime date { get; set; }

    //    public short dayCountConventionId { get; set; }

    //    public double dailyAccuralAmount { get; set; }

    //    public decimal availableBalance { get; set; }

    //    public int daysInAYear { get; set; }

    //}

    public class SubAllocationViewModel 
    {
        public int fromLoanId  { get; set; }
        public decimal fromAmount  { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }


    }

    public class LoanAvailmentApprovalViewModel : ApprovalViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public short applicationStatusId { get; set; }
        public int? toStaffId { get; set; }
        public short? productClassId { get; set; }
        public short? productClassProcessId { get; set; }

    }

    public class CamDocumentViewModel: CamProcessedLoanViewModel
    {
        public string approvalLevelName { get; set; }
    }


    public class LoanClassificationViewModel 
    {
        public int loanId { get; set; }
        public decimal amount { get; set; }
        public string refNo  { get; set; }

    }


    public class CleanUpViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public decimal casaBalance { get; set; }
        public DateTime? nextCovenantDate { get; set; }
        public short freqValue  { get; set; }
        public int casaAccountId { get; set; }
        public short branchId { get; set; }

    }

    
    public class CommentOnLoanAvailmentViewModel 
    {
        public string name  { get; set; }

        public string comments  { get; set; }

        public DateTime date { get; set; }

        public string approvalState { get; set; }

        public int approvalTrailId { get; set; } 
    }
    public class PrudGuildlineTypeViewModel
    {
        public int prudentialGuildlineTypeId { get; set; }
        public string prudentialGuildlineTypeName { get; set; }
    }

    public class PastDueOnPastDueViewModel : GeneralEntity

    {
        public DateTime date { get; set; }

        public int count  { get; set; }

        public decimal amount { get; set; }

        public decimal interestOnAmount { get; set; }

    }

    public class PastDueOnPastDueViewModel1  : GeneralEntity

    {
        public DateTime date { get; set; }

        public int count { get; set; }

        public decimal amount { get; set; }

        public decimal interestOnAmount { get; set; }

    }

    public class LoanDisbursementViewModel : GeneralEntity
    {
        public decimal beneficiaryAmount { get; set; }

        public int loanDisbursementId { get; set; }

        public int termLoanId { get; set; }

        public string  accountNumber { get; set; }

        public decimal amountDisbursed { get; set; }

        public string customerName { get; set; }

        public string loanReferenceNo { get; set; }
        public short beneficiaryCurrencyId { get; set; }
        public short beneficiaryRateCodeId { get; set; }
        public string beneficiaryReason { get; set; }
        public decimal? beneficiaryRateAmount { get; set; }
        public int loanId { get; set; }
    }
    public class ProductType
    {
        public short LOANSYSTEMTYPEID { get; set; }
        public string loanSystemTypeName { get; set; }
    }

    public class MiddleOfficeViewModel : GeneralEntity
    {
        public short? statusFeedbackId;

        public string jobRequestCode { get; set; }
        public string customerName { get; set; }
       
        public string createdByName { get; set; }
        public string customerAccount { get; set; }
        public string branchName { get; set; }
        public string principalName { get; set; }
        public string invoiceNumber { get; set; }
        public DateTime invoiceDate { get; set; }
        public string currencyType { get; set; }
        public string  loanType { get; set; }
        public string modVerificationOfficerName { get; set; }
        public string modVerificationOfficerStaffNo { get; set; }
        public string middleOfficeComment { get; set; }
        public string status { get; set; }
        public int statusId { get; set; }
        public string businessGroup { get; set; }
        public string  businessUnit { get; set; }
        public string staffCode { get; set; }
        


    }

    public class CollateralValuationViewModel : GeneralEntity
    {
        public int solId { get; set; }
        public string  branchName { get; set; }
        public string groupDescription { get; set; }
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public decimal wht { get; set; }
        public string valuerComment { get; set; }
        public string bvn { get; set; }
        public string tin { get; set; }
        public string  facilityType { get; set; }
        public string businessTypes { get; set; }
        public DateTime dateGranted { get; set; }
        public DateTime lastCreditDate { get; set; }
        public DateTime expiryDate { get; set; }
        public string sanctionLimit { get; set; }
        public int tenor { get; set; }
        public decimal balance { get; set; }
        public string  currency { get; set; }
        public string  collateralDetail { get; set; }
        public decimal  collateralValue { get; set; }
        public string perfectionStatus { get; set; }
        public string titleDocsSighted { get; set; }
        public string location { get; set; }
        public string valuationReportSighted { get; set; }
        public DateTime valuationDate { get; set; }
        public string  valuationReportLocation { get; set; }
        public string insuranceDocsSighted { get; set; }
        public DateTime dateOfInsurance { get; set; }
        public DateTime dateOfInspection { get; set; }
        public string stateOfCollateral { get; set; }
        public string collateralAdequacy { get; set; }
        public string inspectingStaffNo { get; set; }
        public string businessDevelopmentManager { get; set; }
        public string groupHead { get; set; }
        public int relationshipManagerId { get; set; }

        public int collateralValuationId { get; set; }
        public int collateralCustomerId { get; set; }
        public string valuationName { get; set; }
        public string valuationReason { get; set; }
        public string createdByName { get; set; }
        
    }

    public class AgeAnalysisViewModel
    {
        public string businessDevelopmentManager { get; set; }
        public string groupName { get; set; }
        public string schemmeCode { get; set; }
        public string customerName { get; set; }
        public string  operativeAccount { get; set; }
        public int sanctionLimit { get; set; }
        public DateTime disbursedDate { get; set; }
        public DateTime expireDate { get; set; }
        public string branchName { get; set; }
        public decimal totalExposure { get; set; }
        public string status { get; set; }
        public DateTime pastDueDate { get; set; }
        public int pastDueDays { get; set; }
        
    }

    public class CreditScheduleViewModel
    {
        public string accountNumber { get; set; }
        public string bvn { get; set; }
        public string customerName { get; set; }
        public string tin { get; set; }
        public string glSubHeadCode { get; set; }
        public string facilityType { get; set; }
        public string businessType { get; set; }
        public string sector { get; set; }
        public string subSector { get; set; }
        public int customerId { get; set; }
        public string groupOrganization { get; set; }
        public DateTime dateGranted { get; set; }
        public DateTime lastCreditDate { get; set; }
        public DateTime expiryDate { get; set; }
        public decimal sanctionLimit { get; set; }
        public decimal previousLimit { get; set; }
        public string repaymentFrequencyForInterest { get; set; }
        public string repaymentFrequencyForPrincipal { get; set; }
        public decimal cumRepaymentAmountDue { get; set; }
        public decimal cumRepaymentAmountPaid { get; set; }
        public decimal cumInterestDueNotYetPaid { get; set; }
        public decimal cumPrincipalDueNotYetPaid { get; set; }
        public double interestRate { get; set; }
        public int  tenor { get; set; }
        public decimal balance { get; set; }
        public string curr  { get; set; }
        public string bankClassification { get; set; }
        public string detailsOfSecuritiesOthers { get; set; }
        public decimal collateralValue { get; set; }
        public int collateralStatus { get; set; }
        public string staffCode { get; set; }
    }


    public class SanctionLimitReportViewModel
    {
        public string initSol { get; set; }
        public string branchCode { get; set; }
        public string branchName { get; set; }
        public string initSoldDesc { get; set; }
        public string currency { get; set; }

        public string loanOdAcct { get; set; }
        public string accountNumber { get; set; }
        public DateTime acctopNdate { get; set; }
        public int customerId { get; set; }
        public string glSubHeadCode { get; set; }
        public string productName { get; set; }
        public string accountName { get; set; }
        public string sanctionLimit { get; set; }
        public DateTime limitSanctionDate { get; set; }
        public DateTime applicableDate { get; set; }
        public string status { get; set; }
        public DateTime limitExpiryDate { get; set; }
        public int tenor { get; set; }
        public DateTime interestStartDate { get; set; }
        public string interestRepaymentFrequency { get; set; }
        public string principalRepaymentFrequency { get; set; }
        public DateTime principalStartDate { get; set; }
        public string   lchgUserId { get; set; }
        public DateTime lchgTime { get; set; }
        public string rcreUserid { get; set; }
        public DateTime rcreTime { get; set; }
        public int staffId { get; set; }
        public string staffName { get; set; }
        public string staffLevel { get; set; }
        public string staffCode { get; set; }
       
        public string approvalId { get; set; }
        public string  approvalName { get; set; }
        public string approvalLevel { get; set; }
        public decimal clrBalanceAmount { get; set; }
        public string relationshipManagerCode { get; set; }
        public string relationshipManagerSbu { get; set; }
        public string limitLevel { get; set; }
        public decimal limitInterestRate { get; set; }
        public decimal limitAccountInterestRate { get; set; }
        public string cotCode { get; set; }
        public string sbuCode  { get; set; }
        public string sbuName { get; set; }
        public string sbuBranch { get; set; }
        public string entererName { get; set; }
        public string entererLevel { get; set; }
        public string entererCode { get; set; }
        public int entererId { get; set; }
        public string entererAppName { get; set; }
        public string authAppName { get; set; }
        public int relationshipManagerId { get; set; }
        public int branchId { get; set; }


    }

    public class ExpiredViewModel : RuniningLoanViewModel
    {
        public string subClassification { get; set; }
    }


    public class ImpairedWatchListViewModel
    {
        public string branchName { get; set; }
        
       
        public string currencyType { get; set; }
        public string staffCode { get; set; }
        public decimal clrBalance { get; set; }
        public decimal interestOverDue { get; set; }
        public decimal principalOverDue { get; set; }
        public decimal totalExposure { get; set; }


        public string teamCode { get; set; }
        public string deskCode { get; set; }
        public string rmCode { get; set; }
        public string buCode { get; set; }
        public string schemeCode { get; set; }
        public string schemeType { get; set; }
        public string teamDescription { get; set; }
        public string deskDescription { get; set; }
        public string buDescription { get; set; }
        public decimal sanctionLimit { get; set; }
        public DateTime? pastDueDate { get; set; }
        public decimal pastDueDat { get; set; }
        public string groupDescription { get; set; }
        public string accountName { get; set; }
        public string account { get; set; }
        public DateTime limitExpiryDate { get; set; }
        public int customerId { get; set; }
        public string customerI { get; set; }

        public string branchCode { get; set; }


        public double interestRate { get; set; }
      
        public string groupCode { get; set; }

        public string glSubHeadCode { get; set; }
        public string loanRefNo { get; set; }
        public string customerName { get; set; }
        public int loanId { get; set; }
        public short loanSystemTypeId { get; set; }
    }


    public class SubHeadCode {
        public string glSubHeadCode { get; set; }
        public string schemeCodes { get; set; }
    }

   
    public class LoanMart : ImpairedWatchListViewModel
    {
       
        public decimal transactionDateBalance { get; set; }
        public string userClassification { get; set; }
        public string subClassification { get; set; }
        public DateTime classificationDate { get; set; }
        public DateTime expiryDate { get; set; }
        

    }
    public class ExcessViewModel: RuniningLoanViewModel
    {
        public decimal excess { get; set; }
        public string subClassification { get; set; }
    }

    public class InsuranceViewModel : RuniningLoanViewModel
    {
        public short branchId { get; set; }
        public string collateralType { get; set; }
        public string perfectionStatus { get; set; }
        public string insuranceType { get; set; }
        public string insurancePolicyNumber { get; set; }
        public string insuranceCompanyName { get; set; }
        public decimal premiumPaid { get; set; }
        public decimal insuredValue { get; set; }
        public DateTime startDate { get; set; }
        
        public int days { get; set; }
        public string workFlowID { get; set; }
        public string  status { get; set; }
        
        public string remarks { get; set; }
        
        
        public string subHead { get; set; }
        public string customerCode { get; set; }
    }

    public class RuniningLoanViewModel : ImpairedWatchListViewModel
    {
        public string rmName { get; set; }
        public DateTime endDate { get; set; }
        public string userClassification { get; set; }
        public decimal receivableAmount { get; set; }
        public decimal  pastDueInterest { get; set; }
        public decimal transactionDateBalance { get; set; }
        public DateTime sanctionLimitDate { get; set; }
        public string schemeDescription  { get; set; }
        public string securityDetails { get; set; }
        
        public string sector { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal interestInSupense { get; set; }
        public DateTime expiryDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime effectiveDate { get; set; }
        public decimal facilityGrantedAmount { get; set; }
        public string subSectorCode { get; set; }
        public decimal otherCharges { get; set; }
        public decimal finalBalance { get; set; }
        public string subUserClassification { get; set; }
        public decimal subStandard { get; set; }
        public decimal doubtfull { get; set; }
        public decimal lost { get; set; }
        public DateTime classificationDate { get; set; }
        public DateTime applicationDate { get; set; }
        public DateTime lastCreditDate { get; set; }
        public decimal lastCreditAmount { get; set; }
        public DateTime limitExpiryyDate { get; set; }
        public decimal fxRate { get; set; }
        public string insiderFlag { get; set; }
        public decimal otherIncome { get; set; }
        
        public int pastDueDays { get; set; }
        public int loanId { get; set; }
        public int loanSytemTypeId { get; set; }
        public string businessDevelopmentManger { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string product { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime effective { get; set; }
        public string divisionName { get; set; }
        public int initiationAge { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public int currentUserAge { get { return (DateTime.UtcNow - this.maturityDate).Days; } }
    }



    public class LoanMartWatchList : ImpairedWatchListViewModel
    {
        public string subClassification { get; set; }
    }
    public class SbHead
    {
        public string subHead { get; set; }
        public string staffCode { get; set; }
        public string teamUnit { get; set; }
        public string depart { get; set; }
      
        public string region { get; set; }
        public string deptName { get; set; }
    }

    public class UnutilizedFacilityViewModel
    {
        public decimal utilizedAmount { get; set; }
        public decimal amountDisbursed { get; set; }
        public decimal approvedAmount { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short productTypeId { get; set; }
        public decimal undisbursedAmount { get; set; }

        public DateTime dateTimeCreated { get; set; }
        public string businessUnits { get; set; }
        public string group { get; set; }
        public string branch { get; set; }
        public string customerName { get; set; }
        public string capSld { get; set; }
        public DateTime reviwedDate { get; set; }
        public string status { get; set; }
        public string purpose { get; set; }
        public string newApprovalInNaira{ get; set; }
        public string newApprovalInDollar { get; set; }
        public string staffCode { get; set; }
        public string cap { get; set; }
    }

  
    //public class bulkDisbursementInputViewModel
    //{
    //    public string applicationReferenceNumber { get; set; }
    //    public string customerCode { get; set; }
    //    public string schemeCode { get; set; }
    //    public decimal LoanAmount { get; set; }
    //    public int schemeId { get; set; }
    //    public int tenor { get; set; }
    //    public string accountnumber { get; set; }
    //    public int loanApplicationDetailId { get; set; }
    //    public bool passed { get; set; }
    //    public List<string> errorMessage { get; set; }
    //}

    public class multipleDisbursementOutputViewModel : GeneralEntity
    {
        public int multipleBulkDisbursementId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int schemeId { get; set; }
        public string schemeCode { get; set; }
        public string schemeName { get; set; }
        public string customerCode { get; set; }
        public int customerId { get; set; }
        public string currencyCode { get; set; }
        public int productId { get; set; }
        public string productName { get; set; }
        public string productTypeName { get; set; }
        public decimal loanAmount { get; set; }
        public int tenor { get; set; }
        public DateTime firstPrincipalPaymentDate { get; set; }
        public DateTime firstInterestPaymentDate { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string accountNumber { get; set; }
        public int casaAccountId { get; set; }
        public int? casaAccountId2 { get; set; }
        public int currencyId { get; set; }
        public int interestRepaymentFrequencyId { get; set; }
        public int principalRepaymentFrequencyId { get; set; }
        public string repaymentScheduleMethodName { get; set; }
        public string interestRepaymentFrequencyName { get; set; }
        public string principalRepaymentFrequencyName { get; set; }
        public List<LoanPaymentSchedulePeriodicViewModel> periodicSchedule { get; set; }
        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public LoanPaymentScheduleInputViewModel loanScheduleInput { get; set; }
        public bool shouldDisburse { get; set; }
        public bool passed { get; set; }
        public List<string> errorMessages { get; set; }
        public RacInformationViewModel rac { get; set; }
    }

    public class creditBureauModel
    {

        public DateTime Test { get; set; }
        public int? branchCode { get; set; }
        public string branchName { get; set; }
        public string staffCode { get; set; }
        public DateTime requestedDate { get; set; }
        public int? relationshipManagerID { get; set; }
        public string rmName { get; set; }


        public string userName { get; set; }
        public string subjectCriteria { get; set; }
        public decimal Amount { get; set; }
    }


    public class MultipleInsuranceOutputViewModel : GeneralEntity
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
        public int requestNumber { get; set; }
        public int insuranceRequestId { get; set; }
        public double haircut { get; set; }
        public string collateralReleaseStatusName { get; set; }
        public int? collateralUsageStatus { get; set; }
        public string customerId { get; set; }
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
        public int insuranceStatus { get; set; }
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
        public decimal loanAmount { get; set; }
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
        public bool passed { get; set; }
        public List<string> errorMessages { get; set; }
        public string isCollateral { get; set; }
        public string policyType { get; set; }
        public string comment { get; set; }
        public string otherValuer { get; set; }
        public decimal? forcedSaleValue { get; set; }
        public decimal? openMarketValue { get; set; }
        public string batchCode { get; set; }
        public int? approvalStatusId { get; set; }
        public string validityStatus { get; set; }
        public string collateralDescription { get; set; }
        public string nameOfSecurity { get; set; }
    }


    public class MultipleInsuranceOutputApprovalViewModel : GeneralEntity
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
        public int? approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public string currentApprovalLevel { get; set; }
        public string customerId { get; set; }
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
        public int insuranceStatus { get; set; }
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
        public decimal loanAmount { get; set; }
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
        public bool passed { get; set; }
        public List<string> errorMessages { get; set; }
        public string isCollateral { get; set; }
        public string policyType { get; set; }
        public string comment { get; set; }
        public string otherValuer { get; set; }
        public decimal? forcedSaleValue { get; set; }
        public decimal? openMarketValue { get; set; }
        public string batchCode { get; set; }
        public int? toStaffId { get; set; }
        public int requestStaffId { get; set; }
        public int? responseStaffId { get; set; }
        public int requestOperationId { get; set; }
        public int? iCustomerId { get; set; }
        public int? bulkInsuranceUploadApprovalId { get; set; }
        public string validityStatus { get; set; }
        public string collateralDescription { get; set; }
        public bool? isCollaterals { get; set; }
        
    }

    //public class MaturityIntructionViewModel : GeneralEntity
    //{
    //    public int maturityInstructionId { get; set; }
    //    public string loanReferenceNumber { get; set; }
    //    public string customerName { get; set; }
    //    public decimal outstandingPrincipal { get; set; }
    //    public decimal outstandingInterest { get; set; }
    //    public int oldTenor { get; set; }
    //    public int newTenor { get; set; }
    //    public double interestRate { get; set; }

    //    public short instructionTypeId { get; set; }
    //    public string instructionTypeName { get; set; }
    //    public int loanId { get; set; }
    //    public int tenor { get; set; }
    //    public short loanSystemTypeId { get; set; }
    //    public short approvalStatusId { get; set; }
    //}

    //public class Accrual

    public class MultipleFsCaptionOutputViewModel : GeneralEntity
    {
        public string fsGroup { get; set; }
        public string fsItem { get; set; }
        public decimal fsValue { get; set; }
        public List<string> errorMessages { get; set; }
        public bool passed { get; set; }
        public string referenceId { get; set; }
        public int customerId { get; set; }
        public short fsGroupId { get; set; }
        public int captionId { get; set; }
        public string validityStatus { get; set; }
    }

}