using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanReviewOperationViewModel : GeneralEntity
    {
        public decimal? contingentOutstandingPrincipal { get; set; }
        public int loanReviewOperationsId { get; set; }
        public string legalContingentCode { get; set; }

        public List<LoanViewModel> dataCollection { get; set; }
        public int loanId { get; set; }

        public short productTypeId { get; set; }

        public int operationTypeId { get; set; }

        public DateTime proposedEffectiveDate { get; set; }

        public string reviewDetails { get; set; }

        public decimal? interateRate { get; set; }

        public decimal? prepayment { get; set; }

        public short? principalFrequencyTypeId { get; set; }

        public short? interestFrequencyTypeId { get; set; }

        public DateTime? principalFirstPaymentDate { get; set; }

        public DateTime? interestFirstPaymentDate { get; set; }

        public DateTime? maturityDate { get; set; }

        public int? tenor { get; set; }
        public int? amountType { get; set; }

        public int? prepaymentMethodId { get; set; }

        public int? cASA_AccountId { get; set; }

        public decimal? overDraftTopup { get; set; }

        public decimal? fee_Charges { get; set; }

        public string terminationAndReBook { get; set; }

        public string completeWriteOff { get; set; }

        public string cancelUndisbursedLoan { get; set; }

        public int approvalStatusId { get; set; }

        public bool isManagementRate { get; set; }

        public bool operationCompleted { get; set; }

        public short? scheduleTypeId { get; set; }

        public short? scheduleDayCountId { get; set; }

        public short? interestTypeId { get; set; }

        public int? lmsApplicationDetailId { get; set; }

        public string instructionType { get; set; }

        public string actionBy { get; set; }

        public string isUsed { get; set; }

        public List<LoanReviewIrregularScheduleViewModel> reviewIrregularSchedule { get; set; }
        public short loanSystemTypeId { get; set; }
        public string operationName { get; set; }
        public string approvalStatus { get; set; }
        public int loanApplicationId { get; set; }
        public int operationId { get; set; }
        public IEnumerable<feeDetails> fees { get; set; }

        public short? feeTypeId { get; set; }



        //File Upload
        public string documentTitle { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] file { get; set; }
        public int? TargetId { get; set; }

        public bool isPrimaryDocument { get; set; }
        public string formData { get; set; }
        public string feeSourceModule { get; set; }
        public decimal? rebookAmount { get; set; }
        public DateTime dateRebook { get; set; }
        public decimal? exposureBeforeRebook { get; set; }
        public string previousOperator { get; set; }
        public decimal bondAmount { get; set; }
        public DateTime? rebookDate { get; set; }
        public int? syncsOperationId { get; set; }
        public int? loanReviewApplicationId { get; set; }
        public int reviewOperationId { get; set; }
        public string customersName { get; set; }
        public string loanReferenceNumber { get; set; }
    }

    public class LoanIrregularScheduleViewModel
    {
        public DateTime realDate { get; set; }
        public decimal interestAmount { get; set; }
        public int payTypeId { get; set; }
        public string payType { get; set; }
        public decimal payAmount { get; set; }
    }

    public class LoanReviewIrregularScheduleViewModel
    {
        public int IrregularScheduleInputId { get; set; }
        public int LoanReviewOperationId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }

    public class LoanRecoveryAssignmentViewModel
    {
        public int? productId { get; set; }
        public int? productClassId { get; set; }
        public string loanReferenceNumber { get; set; }
        public int loanAssignId { get; set; }
        public string assignmentType { get; set; }
        public string referenceId { get; set; }
        public int? approvalStatusId { get; set; }
        public int? operationId { get; set; }
        public int loanId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public DateTime dateAssigned { get; set; }
        public int createdBy { get; set; }
        public decimal? totalAmountRecovery { get; set; }
        public bool isFullyRecovered { get; set; }
        public bool operationCompleted { get; set; }
        public DateTime? expCompletionDate { get; set; }
        public string comment { get; set; }
        public string source { get; set; }
        public int loanApplicationDetailId { get; set; }
    }

    public class RemoveLienViewModel
    {
        public int unfreezeLienAccountId { get; set; }
        public int casaLienAccountId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int fileSize { get; set; }
        public string fileSizeUnit { get; set; }
        public byte[] fileData { get; set; }
        public int createdBy { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public int approvalStatusId { get; set; }
        public bool operationCompleted { get; set; }
        public int operationId { get; set; }
        public short userBranchId { get; set; }
        public string applicationUrl { get; set; }
        public int companyId { get; set; }
        public string userIPAddress { get; set; }
        public string comment { get; set; }
        public bool overwrite { get; set; }
        public int lastUpdatedBy { get; set; }
        public int forwardAction { get; set; }
        public int? receiverStaffId { get; set; }
        public int? receiverLevelId { get; set; }
        public short? vote { get; set; }
        public bool isFlowTest { get; set; }
        public bool? isFromPc { get; set; }
        public DateTime? requestDate { get; set; }
    }


    public class CollateralLiquidationRecoveryViewModel : GeneralEntity
    {
        public string loanReference { get; set; }
        public int collateralLiquidationRecoveryId { get; set; }
        public int loanId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public int loanAssignId { get; set; }
        public bool isFullyRecovered { get; set; }
        public bool overwrite { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int? fileSize { get; set; }
        public string fileSizeUnit { get; set; }
        public DateTime receiptDate { get; set; }
        public decimal totalRecoveryAmount { get; set; }
        public decimal recoveredAmount { get; set; }
        public decimal? outstandingAmount { get; set; }
        public string collateralCode { get; set; }
        public string collectionMode { get; set; }
        public decimal? percentageCommission { get; set; }
    }

    public class LoanReviewOperationApprovalViewModel
    {
        public int? dpdExposure { get; set; }
        public int mgtOperationId { get; set; }
        public decimal? prepaymentAmount { get; set; }
        public decimal? TotalExposure { get; set; }
        public string source { get; set; }
        public string assignmentType { get; set; }

        public DateTime? expCompletionDate { get; set; }
        public int loanAssignId { get; set; }
        public int loanChargeFeeId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeAmount { get; set; }
        public string description { get; set; }
        public int? takeFeeCasaAccountId { get; set; }
        public string takeFeeCasaAccountName { get; set; }
        public string legalContingentCode { get; set; }

        public int? loanReviewApplicationId { get; set; }
        public int? appraisalOperationId { get; set; }
        public string currencyCode { get; set; }
        public bool isBankFormat { get; set; }
        public int companyId { get; set; }
        public string createdByName { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public decimal maturityAmount { get; set; }
        public string relatedReferenceNumber { get; set; }
        public decimal interestAmount { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int productClassId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public string casaAccount { get; set; }

        public string casaAccountName { get; set; }
        public int loanApplicationDetailId { get; set; }

        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public int dpd { get { return (DateTime.UtcNow - this.maturityDate).Days; } }
        public short principalFrequencyTypeId { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string loanCategory { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal principalAmount { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int interestInstallmentLeft { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public int? approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public short scheduleTypeId { get; set; }
        public bool isDisbursed { get; set; }
        public int? disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public string operationName { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }

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
        public string scheduledPrepaymentFrequencyTypeName { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }
        public int internalPrudentialGuidelineStatusId { get; set; }
        public int externalPrudentialGuidelineStatusId { get; set; }
        public DateTime nplDate { get; set; }
        public short? scheduleDayCountConventionId { get; set; }
        public string scheduleDayCountConventionIName { get; set; }

        public short? scheduleDayInterestTypeId { get; set; }
        public int customerRiskRatingId { get; set; }

        // public double productPriceIndexRate { get; set; }
        public bool allowForceDebitRepayment { get; set; }

        //.............Fee Attribute.....................//
        public double exchangeRate { get; set; }

        public DateTime paymentDate { get; set; }
        public decimal totalAmount { get; set; }
        public int chargeFeeId { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; }

        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public string comment { get; set; }

        //Loan Review Operation
        public int loanReviewOperationsId { get; set; }
        public int operationTypeId { get; set; }
        public string operationTypeName { get; set; }
        public DateTime newEffectiveDate { get; set; }
        public string reviewDetails { get; set; }
        public double? newInterateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int? newPrincipalFrequencyTypeId { get; set; }
        public string newPrincipalFrequencyTypeName { get; set; }
        public int? newInterestFrequencyTypeId { get; set; }
        public string newInterestFrequencyTypeName { get; set; }
        public DateTime? newPrincipalFirstPaymentDate { get; set; }
        public DateTime? newInterestFirstPaymentDate { get; set; }
        public int? newTenor { get; set; }
        public int? cASA_AccountId { get; set; }
        public string cASA_AccountName { get; set; }
        public string cASA_Account { get; set; }

        public decimal? overDraftTopup { get; set; }
        public decimal? fee_Charges { get; set; }
        public string terminationAndReBook { get; set; }
        public string completeWriteOff { get; set; }
        public string cancelUndisbursedLoan { get; set; }
        public string lmsLoanReferenceNumber { get; set; }
        public DateTime? newMaturityDate { get; set; }
        public int? loanSystemTypeId { get; set; }
        public int? maturityInstructionTypeId { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal pastDuePrincipal { get; set; }
        public decimal interestOnPastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal accruedInterest { get; set; }
        public decimal writeOffAmount
        {
            get
            {
                return pastDueInterest + pastDuePrincipal
+ interestOnPastDueInterest + interestOnPastDuePrincipal + outstandingPrincipal + accruedInterest;
            }
        }
        public List<feeDetails> fees { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int lmsLoanApplicationId { get; set; }
        public int lmsOperationId { get; set; }

        public int creditAppraisalOperationId { get; set; }
        public string lmsrApplicationReferenceNumber { get; set; }
        public decimal amount { get; set; }
        public string divisionCode { get; set; }
        public string divisionShortCode { get; set; }


        public int collateralLiquidationRecoveryId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool isFullyRecovered { get; set; }
        public bool overwrite { get; set; }
        public byte?[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int? fileSize { get; set; }
        public string fileSizeUnit { get; set; }
        public DateTime receiptDate { get; set; }
        public decimal totalRecoveryAmount { get; set; }
        public decimal recoveredAmount { get; set; }
        public decimal? outstandingAmount { get; set; }
        public string collateralCode { get; set; }
        public string collectionMode { get; set; }
        public int createdBy { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string lienReferenceNumber { get; set; }
        public decimal lienAmount { get; set; }
        public DateTime lienDateTimeCreated { get; set; }
        public DateTime applicationDate { get; set; }
        public string lmsReferenceNumber { get; set; }
        public int loanApplicationId { get; set; }
        public int casaLienAccountId { get; set; }
        public int lienRemovalId { get; set; }
        public int lienRemovalOperationId { get; set; }
        public int unfreezeLienAccountId { get; set; }
        public string accreditedConsultantName { get; set; }
        public string accreditedConsultantCompany { get; set; }
        public string referenceId { get; set; }
        public DateTime requestDate { get; set; }
        public int numberOfLoans { get; set; }
        public string category { get; set; }
        public int bulkRecoveryApprovalId { get; set; }
        public int accreditedConsultantId { get; set; }
        public decimal? agentCommission { get; set; }
        public decimal? percentageCommission { get; set; }
        public decimal totalAmountRecovery { get; set; }
        public string nameOfRecoveryAgent { get; set; }
        public string address { get; set; }
        public string telephoneNumber { get; set; }
        public DateTime expectedRecoveryDate { get; set; }

        public int periodToRecover { get { return (this.expectedRecoveryDate.Date - this.dateOfAssignment.Date).Days; } }
        public decimal amountRecovered { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public DateTime? dateOfEngagement { get; set; }
        public double accountBalance { get; set; }
        public string email { get; set; }
        public DateTime dateOfAssignment { get; set; }
        public int loanRecoveryReportBatchId { get; set; }
        public decimal? totalAllrecoveryAmount { get; set; }
        public string recoveryMisCode { get; set; }
        public string recoveryRegion { get; set; }
        public int loanRecoveryCommissionBatchId { get; set; }
        public string region { get; set; }
        public decimal? creditToAgent { get; set; }
        public decimal? amountToDebit { get; set; }
        public decimal? amountToCredit { get; set; }
        public string listOfAccountsAssigned { get; set; }
        public DateTime? recoveryDate { get; set; }
        public int? flagStatus { get; set; }
        public List<string> customerAddresses { get; set; }
        public string agentCategory { get; set; }
        public string agentAccountNumber { get; set; }
        public int? stateId { get; set; }
        public string assignedBy { get; set; }
        public string staffFullName { get; set; }
        public string accreditedConsultantEmail { get; set; }
        public decimal? commissionAmountLessWht { get; set; }
        public decimal? whtAmount { get; set; }
        public decimal? whtRate { get; set; }
        public decimal? commissionAmount { get; set; }
        public string phoneNumber { get; set; }
        public decimal? contingentOutstandingPrincipal { get; set; }
        public decimal? contigentOutstandingPrincipal { get; set; }
        public decimal? totalPrepayment { get; set; }

        public string exposureType { get; set; }
        public string expiryBand { get; set; }
        public string divisionName { get; set; }
        public DateTime arrivalDate { get; set; }
        public int currApprovalStatusId { get; set; }
    }


    public class LoanReviewApplicationViewModel : GeneralEntity
    {
        public string slaGlobalStatus { get; set; }
        public string slaInduvidualStatus { get; set; }
        public string currentApprovalStatus { get; set; }
        public DateTime? slaTime { get; set; }
        public int currentApprovalLevelSlaInterval { get; set; }

        public string loanTypeName { get; set; }
        public string facility { get; set; }
        public string customerGroupName { get; set; }
        public decimal? approvedAmount { get; set; }
        public short? productClassProcessId { get; set; }

        public short? productClassId { get; set; }
        public int? productId { get; set; }
        public short? loanApplicationTypeId { get; set; }


        public int loanReviewApplicationId { get; set; }
        public int loanId { get; set; }
        public short productTypeId { get; set; }

        public int operationTypeId { get; set; }
        public string reviewDetails { get; set; }

        public IEnumerable<applicationDetails> applicationDetails { get; set; }

        public float interateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int principalFrequencyTypeId { get; set; }
        public int interestFrequencyTypeId { get; set; }
        public DateTime? principalFirstPaymentDate { get; set; }
        public DateTime? interestFirstPaymentDate { get; set; }
        public DateTime? maturityDate { get; set; }
        public int tenor { get; set; }
        public int casaAccountId { get; set; }
        public decimal? overDraftTopup { get; set; }
        public decimal? feeCharges { get; set; }
        public int approvalStatusId { get; set; }
        public bool isManagementInterestRate { get; set; }
        public int? approvalTrailId { get; set; }
        public DateTime? newApplicationDate { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int? toStaffId { get; set; }
        public short branchId { get; set; }
        public DateTime applicationDate { get; set; }
        public string referenceNumber { get; set; }
        public string relatedReferenceNumber { get; set; }

        //public double amount { get; set; }
        public DateTime effectiveDate { get; set; }
        //public double interest { get; set; }
        public decimal principalAmount { get; set; }
        public double interestRate { get; set; }
        public string customerName { get; set; }
        public string operationType { get; set; }
        public string currentApprovalLevel { get; set; }
        public string lastComment { get; set; }
        public short currentApprovalStateId { get; set; }
        public string currentStage { get; set; }
        public string currentApprovalState { get; set; }
        public string approvalStatus { get; set; }
        public int customerId { get; set; }
        public string branchName { get; set; }
        public string approvalState { get; set; }
        public short loanSystemTypeId { get; set; }
        public int performanceTypeId { get; set; }
        public int? operationId { get; set; }
        public DateTime? timeIn { get; set; }
        public DateTime? timeOut { get; set; }
        public string facilityType { get; set; }
        public string creditOperationType { get; set; }

        public int? currentApprovalLevelTypeId { get; set; }
        public string responsiblePerson { get; set; }
        public int requestStaffId { get; set; }
        public int? toApprovalLevelId { get; set; }
        public bool atInitiator { get; set; }
        public int? regionId { get; set; }
        public int proposedTenor { get; set; }
        public int proposedInterest { get; set; }
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

        public string divisionShortCode { get; set; }
        public int globalsla { get; set; }
        public string operationTypeName { get; set; }
        public string createdByName { get; set; }
        public DateTime systemArrivalDate { get; set; }

        public string staffRoleCode { get; set; }

        public string loanReferenceNumber { get; set; }
        public int lmsOperationId { get; set; }
        public int lmsApplicationId { get; set; }
        public short applicationStatusId { get; set; }
        public string cancellationReason { get; set; }
        public int tempApplicationCancellationId { get; set; }
        public string comment { get; set; }
        public int applicationTenor { get; set; }
        public decimal applicationAmount { get; set; }
        public int loanApplicationId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string productName { get; set; }
        public int? customerGroupId { get; set; }
        public int relationshipManagerId { get; set; }
        public int relationshipOfficerId { get; set; }
        public string creatorName { get; set; }
        public int? bookingOperationId { get; set; }
        public int bookingRequestId { get; set; }
        public string loanInformation { get; set; }
        public bool? isOnLending { get; set; }
        public bool? isInterventionFunds { get; set; }
        public string applicationStatus { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string misCode { get; set; }
        public bool owner { get; set; }
        public string employer { get; set; }
        public string operationName { get; set; }
        public int currentOperationId { get; set; }
        public int? creditAppraisalOperationId { get; set; }
        public int? creditAppraisalLoanApplicationId { get; set; }
        public int? loanReviewApplicationDetailId { get; set; }
    }

    public class applicationDetails
    {
        public short reviewStageId;

        public int operationTypeId { get; set; } // remove after refactor
        public string reviewDetails { get; set; }
        public int duration { get; set; }
        //public string operationType { get; set; } // not relevant 
        public int loanId { get; set; }
        public int customerId { get; set; }
        public int detailId { get; set; }
        public int loanApplicationId { get; set; }

        public short loanSystemTypeId { get; set; }
        public int operationId { get; set; }
        //public string loanSystemType { get; set; }
        public string loanSystemTypeName { get; set; }
        public string operationName { get; set; }
        public short productId { get; set; }

        public string obligorName { get; set; }
        public decimal? proposedTenor { get; set; }
        public double proposedRate { get; set; }
        public decimal proposedAmount { get; set; }
        public int approvedTenor { get; set; }
        public double approvedRate { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal? customerProposedAmount { get; set; }
        public decimal? proposedInterest { get; set; }

        public string proposedTenorString
        {
            get
            {
                var units = proposedTenor == 1 ? " day" : " days";
                if (proposedTenor < 15) return proposedTenor.ToString() + units;
                if (proposedTenor == null) { proposedTenor = 1; }
                var months = Math.Ceiling((Math.Floor((int)proposedTenor / 15.00)) / 2);
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
                if (proposedTenor == null) { proposedTenor = 1; }
                var months = Math.Ceiling((Math.Floor(approvedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public string loanReferenceNumber { get; set; }
        public int statusId { get; set; }
        public string terms { get; set; }
        public int repaymentScheduleId { get; set; }
        public string schedule { get; set; }
        public int loanApplicationDetailId { get; set; }

        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int creditAppraisalOperationId { get; set; }
        public short? currencyId { get; set; }

    }

    public class LMSOperationListViewModel
    {
        public int loanId { get; set; }
        public int customerId { get; set; }

        public short operationId { get; set; }
        public string operationName { get; set; }
        public short loanSystemTypeId { get; set; }
        public int productTypeId { get; set; }
    }

    public class SelectListViewModel
    {
        public List<DropDownSelect> casaAccounts { get; set; }
        public List<DropDownSelect> productTypes { get; set; }
        public List<DropDownSelect> operationTypes { get; set; }
        public List<DropDownSelect> interestFrequencyTypes { get; set; }
        public List<DropDownSelect> principalFrequencyTypes { get; set; }
        public List<DropDownSelect> feeCharges { get; set; }

    }

    public class LoanReviewOperationParentChildViewModel
    {
        public decimal interestAtMuturity { get; set; }

        public int? tenorLeft { get; set; }

        public short loanStatusId { get; set; }
        public string customerName { get; set; }
        public int? approvedTenor { get; set; }
        public DateTime? lineEffectiveDate { get; set; }
        public DateTime? expiryDate { get; set; }

        public double? approvedInterestRate { get; set; }
        public string approvedProductName { get; set; }
        public int numberofTranchesBooked { get; set; }
        public int numberofrunningTranches { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public int casaAccountId2 { get; set; }
        public int loanApplicationDetailId { get; set; }

        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public decimal principalAmount { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int interestInstallmentLeft { get; set; }
        public decimal? approvedAmount { get; set; }
        public int? operationId { get; set; }
        public string operationName { get; set; }
        public short loanTypeId { get; set; }

        public decimal equityContribution { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public decimal outstandingInterest { get; set; }

        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public List<LoanReviewOperationApprovalViewModel> runningTranches { get; set; }
    }

    public class DropDownSelect
    {
        public int id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public int typeId { get; set; }
        public int currencyId { get; set; }
        public int? productTypeId { get; set; }
    }

    public class MaturityIntructionViewModel : GeneralEntity
    {
        public int maturityInstructionId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string customerName { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public decimal outstandingInterest { get; set; }
        public int oldTenor { get; set; }
        public int newTenor { get; set; }
        public double interestRate { get; set; }
        public int? operationId { get; set; }

        public DateTime? valueDate { get; set; }

        public short instructionTypeId { get; set; }
        public string instructionTypeName { get; set; }
        public int loanId { get; set; }
        public int tenor { get; set; }
        public short loanSystemTypeId { get; set; }
        public short approvalStatusId { get; set; }
        public int? loanReviewOperationsId { get; set; }
        public IEnumerable<feeDetails> fees { get; set; }

    }


    public class LoanRecoveryReportBatchViewModel : GeneralEntity
    {
        public int loanRecoveryReportBatchId { get; set; }
        public string referenceId { get; set; }
        public int? approvalStatusId { get; set; }
        public int? operationId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public decimal? totalAmountRecovery { get; set; }
        public decimal? amountRecovered { get; set; }
        public bool operationCompleted { get; set; }
        public decimal? recoveredAmount { get; set; }
        public decimal? totalRecoveryAmount { get; set; }
        public int? collateralLiquidationRecoveryId { get; set; }
    }

    public class LoanRecoveryReportApprovalViewModel : GeneralEntity
    {
        public int loanRecoveryReportApprovalId { get; set; }
        public string referenceId { get; set; }
        public int? approvalStatusId { get; set; }
        public int? operationId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public int customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool operationCompleted { get; set; }
        public decimal? totalAmountRecovery { get; set; }
        public decimal? amountRecovered { get; set; }
        //for approval records
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public decimal? accountBalance { get; set; }
        public string misCode { get; set; }
        public string region { get; set; }
        public string comment { get; set; }
        public int numberOfLoans { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string approverComment { get; set; }
    }

    public class LoanRecoveryCommissionApprovalViewModel : GeneralEntity
    {
        public int loanRecoveryCommissionApprovalId { get; set; }
        public string referenceId { get; set; }
        public int? approvalStatusId { get; set; }
        public int? operationId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public int customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool operationCompleted { get; set; }
        public decimal? totalAmountRecovery { get; set; }
        public decimal? amountRecovered { get; set; }
        //for approval records
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public decimal? accountBalance { get; set; }
        public string misCode { get; set; }
        public string region { get; set; }
        public string comment { get; set; }
        public int numberOfLoans { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string approverComment { get; set; }
        public string agentAccountNumber { get; set; }
        public DateTime? dateOfEngagement { get; set; }
        public DateTime? collectionDate { get; set; }
        public string modeOfCollection { get; set; }
        public decimal? commissionRate { get; set; }
        public decimal? commissionAmount { get; set; }
        public decimal? commissionAmountLessWht { get; set; }
        public decimal? whtAmount { get; set; }
        public decimal? whtRate { get; set; }
    }


    public class LoanRecoveryCommissionBatchViewModel : GeneralEntity
    {
        public int loanRecoveryCommissionBatchId { get; set; }
        public string referenceId { get; set; }
        public int? approvalStatusId { get; set; }
        public int? operationId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public decimal? totalAmountRecovery { get; set; }
        public decimal? amountRecovered { get; set; }
        public bool operationCompleted { get; set; }
        public decimal? recoveredAmount { get; set; }
        public decimal? totalRecoveryAmount { get; set; }
        public string misCode { get; set; }
        public string region { get; set; }
        public string recoveryMisCode { get; set; }
        public string recoveryRegion { get; set; }
        public int? loanRecoveryReportBatchId { get; set; }
    }

    public class RemedialAssetReportViewModel
    {
        public string casaAccount { get; set; }
        public int PeriodToRecover { get { return (this.ExpectedRecoveryDate.Date - this.DateOfAssignment.Date).Days; } }
        public int TenorAgreed { get { return (this.maturityDate.Value - this.effectiveDate).Days; } }
        public int TenorOfPayment { get { return (this.maturityDate.Value - this.effectiveDate).Days; } }
        public string AccountNumber { get; set; }
        public string accountNumber { get; set; }
        public string AccountName { get; set; }
        public string accountName { get; set; }
        public string NameOfRecoveryAgent { get; set; }
        public string nameOfRecoveryAgent { get; set; }
        public string Address { get; set; }
        public string address { get; set; }
        public string TelephoneNumber { get; set; }
        public string telephoneNumber { get; set; }
        public DateTime ExpectedRecoveryDate { get; set; }
        public DateTime expectedRecoveryDate { get; set; }
        public decimal AmountRecovered { get; set; }
        public DateTime DateOfAssignment { get; set; }
        public string Email { get; set; }
        public int accreditedConsultant { get; set; }
        public string ListOfAccountsAssigned { get; set; }
        public string RecoveryAgentName { get; set; }
        public decimal? TotalExposure { get; set; }
        public decimal? AccountRecovered { get; set; }
        public DateTime DateOfRecovery { get; set; }
        public decimal? PercentageCommissionPaid { get; set; }
        public decimal? CommissionPayable { get; set; }
        public DateTime? DateOfEngagement { get; set; }
        public decimal AccountBalance { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal FullAndFinalAmountApproved { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime? maturityDate { get; set; }
        public string ExpectedPaymentPeriod { get; set; }
        public int AgeOfLastCredit { get; set; }
        public string Classification { get; set; }
        public string CollateralDescription { get; set; }
        public string Branch { get; set; }
        public string Region { get; set; }
        public string StatusOfPerction { get; set; }
        public DateTime DateOfValuation { get; set; }
        public double OMV { get; set; }
        public double FSV { get; set; }
        public string NBV { get; set; }
        public double AmountSold { get; set; }
        public double PAndLImpact { get; set; }
        public string DateSold { get; set; }
        public string FacilityType { get; set; }
        public string Provision { get; set; }
        public double ResidualBalance { get; set; }
        public double MonthlyExpectedPayment { get; set; }
        public string Telephone { get; set; }
    }

    public class RepaymentAlertViewModel
    {
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public decimal outStandingPrincipal { get; set; }
        public string guarantorEmail { get; set; }
        public DateTime paymentDate { get; set; }
        public decimal periodPaymentAmount { get; set; }
        public decimal periodPrincipalAmount { get; set; }
        public decimal endPrincipalAmount { get; set; }
        public string guarantorName { get; set; }
    }


    public class RetailLoanRecoveryCommissionViewModel : GeneralEntity
    {
        public int loanRecoveryCommissionId { get; set; }
        public string referenceId { get; set; }
        public int? approvalStatusId { get; set; }
        public int? operationId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string customerId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool operationCompleted { get; set; }
        public decimal? totalAmountRecovery { get; set; }
        public decimal? amountRecovered { get; set; }
        public DateTime? recoveryMonth { get; set; }

        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public decimal? accountBalance { get; set; }
        public string misCode { get; set; }
        public string region { get; set; }
        public string comment { get; set; }
        public int numberOfLoans { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string approverComment { get; set; }
        public string agentAccountNumber { get; set; }
        public DateTime? dateOfEngagement { get; set; }
        public DateTime? collectionDate { get; set; }
        public string modeOfCollection { get; set; }
        public decimal? commissionRate { get; set; }
        public decimal? commissionPayable { get; set; }
        public int? loanAssignId { get; set; }

        public decimal? prepaymentAmount;
        public decimal? TotalExposure { get; set; }

        public DateTime? expCompletionDate { get; set; }
        public int loanChargeFeeId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeAmount { get; set; }
        public string description { get; set; }
        public int? takeFeeCasaAccountId { get; set; }
        public string takeFeeCasaAccountName { get; set; }
        public string legalContingentCode { get; set; }

        public int? loanReviewApplicationId { get; set; }
        public int? appraisalOperationId { get; set; }
        public string currencyCode { get; set; }
        public bool isBankFormat { get; set; }
        public int companyId { get; set; }
        public string createdByName { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public decimal maturityAmount { get; set; }
        public string relatedReferenceNumber { get; set; }
        public decimal interestAmount { get; set; }

        public short productId { get; set; }
        public int? productClassId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public string casaAccount { get; set; }

        public string casaAccountName { get; set; }
        public int loanApplicationDetailId { get; set; }

        public short branchId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public short principalFrequencyTypeId { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string loanCategory { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public int productTypeId { get; set; }

        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public int operationTypeId { get; set; }
        public string operationTypeName { get; set; }
        public DateTime newEffectiveDate { get; set; }
        public string reviewDetails { get; set; }
        public double? newInterateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int? newPrincipalFrequencyTypeId { get; set; }
        public string newPrincipalFrequencyTypeName { get; set; }
        public int? newInterestFrequencyTypeId { get; set; }
        public string newInterestFrequencyTypeName { get; set; }
        public DateTime? newPrincipalFirstPaymentDate { get; set; }
        public DateTime? newInterestFirstPaymentDate { get; set; }
        public int? newTenor { get; set; }
        public int? cASA_AccountId { get; set; }
        public string cASA_AccountName { get; set; }
        public string cASA_Account { get; set; }

        public decimal? overDraftTopup { get; set; }
        public decimal? fee_Charges { get; set; }
        public string terminationAndReBook { get; set; }
        public string completeWriteOff { get; set; }
        public string cancelUndisbursedLoan { get; set; }
        public string lmsLoanReferenceNumber { get; set; }
        public int collateralLiquidationRecoveryId { get; set; }
        public decimal totalRecoveryAmount { get; set; }
        public decimal recoveredAmount { get; set; }
        public decimal? outstandingAmount { get; set; }
        public string collateralCode { get; set; }
        public string collectionMode { get; set; }
        public int createdBy { get; set; }
        public int loanApplicationId { get; set; }
        public int casaLienAccountId { get; set; }
        public int lienRemovalId { get; set; }
        public int lienRemovalOperationId { get; set; }
        public int unfreezeLienAccountId { get; set; }
        public string accreditedConsultantName { get; set; }
        public string accreditedConsultantCompany { get; set; }
        public int bulkRecoveryApprovalId { get; set; }
        public int accreditedConsultantId { get; set; }
        public decimal? agentCommission { get; set; }
        public decimal? percentageCommission { get; set; }
        public string nameOfRecoveryAgent { get; set; }
        public string address { get; set; }
        public string telephoneNumber { get; set; }
        public DateTime expectedRecoveryDate { get; set; }

        public int periodToRecover { get { return (this.expectedRecoveryDate.Date - this.dateOfAssignment.Date).Days; } }
        public List<string> listOfAccountAssigned { get; set; }
        public string email { get; set; }
        public DateTime dateOfAssignment { get; set; }
        public int loanRecoveryReportBatchId { get; set; }
        public decimal? totalAllrecoveryAmount { get; set; }
        public string recoveryMisCode { get; set; }
        public string recoveryRegion { get; set; }
        public int loanRecoveryCommissionBatchId { get; set; }
        public decimal? creditToAgent { get; set; }
        public decimal? amountToDebit { get; set; }
        public decimal? amountToCredit { get; set; }
        public string listOfAccountsAssigned { get; set; }
        public DateTime? recoveryDate { get; set; }
        public int? flagStatus { get; set; }
        public List<string> customerAddresses { get; set; }
        public string agentCategory { get; set; }
        public int? dpdExposure { get; set; }
        public string category { get; set; }
        public string divisionCode { get; set; }
        public string branchCode { get; set; }
        public decimal? totalUnsettledAmount { get; set; }
    }

    public class RecoveryCollectionsViewModel
    {
        public decimal? prepaymentAmount { get; set; }
        public double orlMinimumAssigned { get; set; }

        public decimal? TotalExposure { get; set; }
        public decimal totalExposureLcy { get; set; }
        public DateTime? expCompletionDate { get; set; }
        public int loanAssignId { get; set; }
        public int loanChargeFeeId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeAmount { get; set; }
        public string description { get; set; }
        public int? takeFeeCasaAccountId { get; set; }
        public string takeFeeCasaAccountName { get; set; }
        public string legalContingentCode { get; set; }

        public int? loanReviewApplicationId { get; set; }
        public int? appraisalOperationId { get; set; }
        public string currencyCode { get; set; }
        public bool isBankFormat { get; set; }
        public int companyId { get; set; }
        public string createdByName { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public decimal maturityAmount { get; set; }
        public string relatedReferenceNumber { get; set; }
        public decimal interestAmount { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public string casaAccount { get; set; }

        public string casaAccountName { get; set; }
        public int loanApplicationDetailId { get; set; }

        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        //public int dpd { get { return (DateTime.Now - this.maturityDate).Days; } }
        public int? dpd { get; set; }
        public string dpdRange
        {
            get
            {
                var newRange = "";
                if (this.dpd >= 31 && this.dpd <= 60)
                {
                    newRange = "31-60 DPD";
                }
                if (this.dpd >= 61 && this.dpd <= 90)
                {
                    newRange = "61-90 DPD";
                }
                if (this.dpd > 90)
                {
                    newRange = "90+DPD";
                }

                return newRange;
            }
        }
        public bool isDigital { get; set; }
        public int? maturityBand { get; set; } //{ get { return (this.maturityDate - DateTime.Now).Days; } }
        public short principalFrequencyTypeId { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string loanCategory { get; set; }
        public DateTime? bookingDate { get; set; }
        public decimal principalAmount { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int interestInstallmentLeft { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public int? approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public short scheduleTypeId { get; set; }
        public bool isDisbursed { get; set; }
        public int? disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public string operationName { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }

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
        public string scheduledPrepaymentFrequencyTypeName { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }
        public int internalPrudentialGuidelineStatusId { get; set; }
        public int externalPrudentialGuidelineStatusId { get; set; }
        public DateTime nplDate { get; set; }
        public short? scheduleDayCountConventionId { get; set; }
        public string scheduleDayCountConventionIName { get; set; }

        public short? scheduleDayInterestTypeId { get; set; }
        public int customerRiskRatingId { get; set; }

        // public double productPriceIndexRate { get; set; }
        public bool allowForceDebitRepayment { get; set; }

        //.............Fee Attribute.....................//
        public double exchangeRate { get; set; }

        public DateTime paymentDate { get; set; }
        public decimal totalAmount { get; set; }
        public int chargeFeeId { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; }

        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public string comment { get; set; }

        //Loan Review Operation
        public int loanReviewOperationsId { get; set; }
        public int operationTypeId { get; set; }
        public string operationTypeName { get; set; }
        public DateTime newEffectiveDate { get; set; }
        public string reviewDetails { get; set; }
        public double? newInterateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int? newPrincipalFrequencyTypeId { get; set; }
        public string newPrincipalFrequencyTypeName { get; set; }
        public int? newInterestFrequencyTypeId { get; set; }
        public string newInterestFrequencyTypeName { get; set; }
        public DateTime? newPrincipalFirstPaymentDate { get; set; }
        public DateTime? newInterestFirstPaymentDate { get; set; }
        public int? newTenor { get; set; }
        public int? cASA_AccountId { get; set; }
        public string cASA_AccountName { get; set; }
        public string cASA_Account { get; set; }

        public decimal? overDraftTopup { get; set; }
        public decimal? fee_Charges { get; set; }
        public string terminationAndReBook { get; set; }
        public string completeWriteOff { get; set; }
        public string cancelUndisbursedLoan { get; set; }
        public string lmsLoanReferenceNumber { get; set; }
        public DateTime? newMaturityDate { get; set; }
        public int? loanSystemTypeId { get; set; }
        public int? maturityInstructionTypeId { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal pastDuePrincipal { get; set; }
        public decimal interestOnPastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal accruedInterest { get; set; }
        public decimal writeOffAmount
        {
            get
            {
                return pastDueInterest + pastDuePrincipal
+ interestOnPastDueInterest + interestOnPastDuePrincipal + outstandingPrincipal + accruedInterest;
            }
        }

        public string range
        {
            get
            {
                var newRange = "";
                if (this.dpd >= 31 && this.dpd <= 60)
                {
                    newRange = "31-60 DPD";
                }
                if (this.dpd >= 61 && this.dpd <= 90)
                {
                    newRange = "61-90 DPD";
                }
                if (this.dpd > 90)
                {
                    newRange = "90+DPD";
                }

                return newRange;
            }
        }
        public List<feeDetails> fees { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int lmsLoanApplicationId { get; set; }
        public int lmsOperationId { get; set; }
        public int creditAppraisalOperationId { get; set; }
        public string lmsrApplicationReferenceNumber { get; set; }
        public decimal amount { get; set; }
        public int collateralLiquidationRecoveryId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool isFullyRecovered { get; set; }
        public bool overwrite { get; set; }
        public byte?[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int? fileSize { get; set; }
        public string fileSizeUnit { get; set; }
        public DateTime receiptDate { get; set; }
        public decimal totalRecoveryAmount { get; set; }
        public decimal recoveredAmount { get; set; }
        public decimal? outstandingAmount { get; set; }
        public string collateralCode { get; set; }
        public string collectionMode { get; set; }
        public int createdBy { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string lienReferenceNumber { get; set; }
        public decimal lienAmount { get; set; }
        public DateTime lienDateTimeCreated { get; set; }
        public DateTime applicationDate { get; set; }
        public string lmsReferenceNumber { get; set; }
        public int loanApplicationId { get; set; }
        public int casaLienAccountId { get; set; }
        public int lienRemovalId { get; set; }
        public int lienRemovalOperationId { get; set; }
        public int unfreezeLienAccountId { get; set; }
        public string accreditedConsultantName { get; set; }
        public string accreditedConsultantCompany { get; set; }
        public string referenceId { get; set; }
        public DateTime requestDate { get; set; }
        public int numberOfLoans { get; set; }
        public int bulkRecoveryApprovalId { get; set; }
        public int accreditedConsultantId { get; set; }
        public decimal? agentCommission { get; set; }
        public decimal? percentageCommission { get; set; }
        public decimal totalAmountRecovery { get; set; }
        public string nameOfRecoveryAgent { get; set; }
        public string address { get; set; }
        public string telephoneNumber { get; set; }
        public DateTime expectedRecoveryDate { get; set; }

        public int periodToRecover { get { return (this.expectedRecoveryDate.Date - this.dateOfAssignment.Date).Days; } }
        public decimal amountRecovered { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public DateTime? dateOfEngagement { get; set; }
        public double accountBalance { get; set; }
        public List<string> listOfAccountAssigned { get; set; }
        public string email { get; set; }
        public DateTime dateOfAssignment { get; set; }
        public int loanRecoveryReportBatchId { get; set; }
        public decimal? totalAllrecoveryAmount { get; set; }
        public string recoveryMisCode { get; set; }
        public string recoveryRegion { get; set; }
        public int loanRecoveryCommissionBatchId { get; set; }
        public string region { get; set; }
        public decimal? creditToAgent { get; set; }
        public decimal? amountToDebit { get; set; }
        public decimal? amountToCredit { get; set; }
        public string listOfAccountsAssigned { get; set; }
        public DateTime? recoveryDate { get; set; }
        public int? flagStatus { get; set; }
        public List<string> customerAddresses { get; set; }
        public string agentCategory { get; set; }
        public string agentAccountNumber { get; set; }
        public string referenceNumber { get; set; }
        public string productClass { get; set; }
        public string main { get; set; }
        public string businessLine { get; set; }
        public string subBusinessLine { get; set; }
        public string productCode { get; set; }
        public decimal interest { get; set; }
        public decimal penalCharges { get; set; }
        public decimal amountDue { get; set; }
        public decimal loanAmountLcy { get; set; }
        public decimal? collections { get; set; }
        public string facilityType { get; set; }
        public string staffCode { get; set; }
        public string regionName { get; set; }
        public int? supervisorId { get; set; }
        public string groupHeadName { get; set; }
        public string groupName { get; set; }
        public string location { get; set; }
        public string teamName { get; set; }
        public string loanReference { get; set; }
        public string agentAssigned { get; set; }
        public string settlementAccount { get; set; }
        public decimal principalOutstandingBalLcy { get; set; }
        public DateTime? valueDate { get; set; }
        public DateTime? referenceDate { get; set; }
        public string accountOfficerCode { get; set; }
        public string mobileNumber { get; set; }
        public string divisionName { get; set; }
        public string accountOfficerName { get; set; }
        public DateTime? processDate { get; set; }
        public DateTime dateAssigned { get; set; }
        public decimal? actualRecovery { get; set; }
        public decimal? commission { get; set; }
        public string newCountReferenceNumber { get; set; }
        public decimal minimumAmountDueUnpaid { get; set; }
        public decimal? totalOutstanding { get; set; }
        public decimal? commissionOne { get; set; }
        public decimal? commissionTwo { get; set; }
        public decimal? target { get; set; }
        public decimal? totalAmountRecovered { get; set; }
        public decimal? totalAmountAssigned { get; set; }
        public decimal? amountRecoveredCreditCard { get; set; }
        public decimal? creditCardMinimumAssigned { get; set; }
        public double amountRecoveredOrl { get; set; }
        public decimal? paydayLoanMinimumAssigned { get; set; }
        public decimal? amountRecoveredPaydayLoan { get; set; }
        public decimal? initialAssigned { get; set; }
        public string category { get; set; }
    }


    public class RecoveryAutoAssignmentViewModel
    {
        public decimal? prepaymentAmount { get; set; }
        public decimal? TotalExposure { get; set; }
        public string source { get; set; }
        public string assignmentType { get; set; }

        public DateTime? expCompletionDate { get; set; }
        public int loanAssignId { get; set; }
        public int loanChargeFeeId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeAmount { get; set; }
        public string description { get; set; }
        public int? takeFeeCasaAccountId { get; set; }
        public string takeFeeCasaAccountName { get; set; }
        public string legalContingentCode { get; set; }

        public int? loanReviewApplicationId { get; set; }
        public int? appraisalOperationId { get; set; }
        public string currencyCode { get; set; }
        public bool isBankFormat { get; set; }
        public int companyId { get; set; }
        public string createdByName { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public decimal maturityAmount { get; set; }
        public string relatedReferenceNumber { get; set; }
        public decimal interestAmount { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int productClassId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public string casaAccount { get; set; }

        public string casaAccountName { get; set; }
        public int loanApplicationDetailId { get; set; }

        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }

        public short principalFrequencyTypeId { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string loanCategory { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal principalAmount { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int interestInstallmentLeft { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public int? approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public short scheduleTypeId { get; set; }
        public bool isDisbursed { get; set; }
        public int? disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public string operationName { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }

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
        public string scheduledPrepaymentFrequencyTypeName { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }
        public int internalPrudentialGuidelineStatusId { get; set; }
        public int externalPrudentialGuidelineStatusId { get; set; }
        public DateTime nplDate { get; set; }
        public short? scheduleDayCountConventionId { get; set; }
        public string scheduleDayCountConventionIName { get; set; }

        public short? scheduleDayInterestTypeId { get; set; }
        public int customerRiskRatingId { get; set; }

        // public double productPriceIndexRate { get; set; }
        public bool allowForceDebitRepayment { get; set; }

        //.............Fee Attribute.....................//
        public double exchangeRate { get; set; }

        public DateTime paymentDate { get; set; }
        public decimal totalAmount { get; set; }
        public int chargeFeeId { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; }

        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public string comment { get; set; }

        //Loan Review Operation
        public int loanReviewOperationsId { get; set; }
        public int operationTypeId { get; set; }
        public string operationTypeName { get; set; }
        public DateTime newEffectiveDate { get; set; }
        public string reviewDetails { get; set; }
        public double? newInterateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int? newPrincipalFrequencyTypeId { get; set; }
        public string newPrincipalFrequencyTypeName { get; set; }
        public int? newInterestFrequencyTypeId { get; set; }
        public string newInterestFrequencyTypeName { get; set; }
        public DateTime? newPrincipalFirstPaymentDate { get; set; }
        public DateTime? newInterestFirstPaymentDate { get; set; }
        public int? newTenor { get; set; }
        public int? cASA_AccountId { get; set; }
        public string cASA_AccountName { get; set; }
        public string cASA_Account { get; set; }

        public decimal? overDraftTopup { get; set; }
        public decimal? fee_Charges { get; set; }
        public string terminationAndReBook { get; set; }
        public string completeWriteOff { get; set; }
        public string cancelUndisbursedLoan { get; set; }
        public string lmsLoanReferenceNumber { get; set; }
        public DateTime? newMaturityDate { get; set; }
        public int? loanSystemTypeId { get; set; }
        public int? maturityInstructionTypeId { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal pastDuePrincipal { get; set; }
        public decimal interestOnPastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal accruedInterest { get; set; }
        public decimal writeOffAmount
        {
            get
            {
                return pastDueInterest + pastDuePrincipal
+ interestOnPastDueInterest + interestOnPastDuePrincipal + outstandingPrincipal + accruedInterest;
            }
        }
        public List<feeDetails> fees { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int lmsLoanApplicationId { get; set; }
        public int lmsOperationId { get; set; }

        public int creditAppraisalOperationId { get; set; }
        public string lmsrApplicationReferenceNumber { get; set; }
        public decimal amount { get; set; }


        public int collateralLiquidationRecoveryId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool isFullyRecovered { get; set; }
        public bool overwrite { get; set; }
        public byte?[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int? fileSize { get; set; }
        public string fileSizeUnit { get; set; }
        public DateTime receiptDate { get; set; }
        public decimal totalRecoveryAmount { get; set; }
        public decimal recoveredAmount { get; set; }
        public decimal? outstandingAmount { get; set; }
        public string collateralCode { get; set; }
        public string collectionMode { get; set; }
        public int createdBy { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string lienReferenceNumber { get; set; }
        public decimal lienAmount { get; set; }
        public DateTime lienDateTimeCreated { get; set; }
        public DateTime applicationDate { get; set; }
        public string lmsReferenceNumber { get; set; }
        public int loanApplicationId { get; set; }
        public int casaLienAccountId { get; set; }
        public int lienRemovalId { get; set; }
        public int lienRemovalOperationId { get; set; }
        public int unfreezeLienAccountId { get; set; }
        public string accreditedConsultantName { get; set; }
        public string accreditedConsultantCompany { get; set; }
        public string referenceId { get; set; }
        public DateTime requestDate { get; set; }
        public int numberOfLoans { get; set; }
        public int bulkRecoveryApprovalId { get; set; }
        public int accreditedConsultantId { get; set; }
        public decimal? agentCommission { get; set; }
        public decimal? percentageCommission { get; set; }
        public decimal totalAmountRecovery { get; set; }
        public string nameOfRecoveryAgent { get; set; }
        public string address { get; set; }
        public string telephoneNumber { get; set; }
        public DateTime expectedRecoveryDate { get; set; }

        public int periodToRecover { get { return (this.expectedRecoveryDate.Date - this.dateOfAssignment.Date).Days; } }
        public decimal amountRecovered { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public DateTime? dateOfEngagement { get; set; }
        public double accountBalance { get; set; }
        public string email { get; set; }
        public DateTime dateOfAssignment { get; set; }
        public int loanRecoveryReportBatchId { get; set; }
        public decimal? totalAllrecoveryAmount { get; set; }
        public string recoveryMisCode { get; set; }
        public string recoveryRegion { get; set; }
        public int loanRecoveryCommissionBatchId { get; set; }
        public string region { get; set; }
        public decimal? creditToAgent { get; set; }
        public decimal? amountToDebit { get; set; }
        public decimal? amountToCredit { get; set; }
        public string listOfAccountsAssigned { get; set; }
        public DateTime? recoveryDate { get; set; }
        public int? flagStatus { get; set; }
        public List<string> customerAddresses { get; set; }
        public string agentCategory { get; set; }
        public string agentAccountNumber { get; set; }
        public int? stateId { get; set; }
        public bool operationCompleted { get; set; }

    }

    public class GlobalExposureApplicationViewModel : GeneralEntity
    {
        public string range
        {
            get
            {
                var newRange = "";
                if (this.dpdExposure >= 31 && this.dpdExposure <= 60)
                {
                    newRange = "31-60 DPD";
                }
                if (this.dpdExposure >= 61 && this.dpdExposure <= 90)
                {
                    newRange = "61-90 DPD";
                }
                if (this.dpdExposure > 90)
                {
                    newRange = "90+DPD";
                }

                return newRange;
            }
        }
        public int? dpdExposure { get; set; }
        public bool isDigital { get; set; }
        public int mgtOperationId { get; set; }
        public decimal? prepaymentAmount { get; set; }
        public decimal? TotalExposure { get; set; }
        public string source { get; set; }
        public string assignmentType { get; set; }

        public DateTime? assignedDate { get; set; }
        public DateTime? expCompletionDate { get; set; }
        public int loanAssignId { get; set; }
        public int loanChargeFeeId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeAmount { get; set; }
        public string description { get; set; }
        public int? takeFeeCasaAccountId { get; set; }
        public string takeFeeCasaAccountName { get; set; }
        public string legalContingentCode { get; set; }

        public int? loanReviewApplicationId { get; set; }
        public int? appraisalOperationId { get; set; }
        public string currencyCode { get; set; }
        public bool isBankFormat { get; set; }
        public string createdByName { get; set; }
        public decimal maturityAmount { get; set; }
        public string relatedReferenceNumber { get; set; }
        public decimal interestAmount { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public int loanId { get; set; }
        public string customerId { get; set; }
        public int productId { get; set; }
        public int productClassId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public string casaAccount { get; set; }

        public string casaAccountName { get; set; }
        public int loanApplicationDetailId { get; set; }

        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public int dpd { get { return (DateTime.UtcNow - this.maturityDate).Days; } }
        public short principalFrequencyTypeId { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string loanCategory { get; set; }
        public DateTime bookingDate { get; set; }
        public decimal principalAmount { get; set; }
        public int principalInstallmentLeft { get; set; }
        public int interestInstallmentLeft { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public int? approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public short scheduleTypeId { get; set; }
        public bool isDisbursed { get; set; }
        public int? disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public string operationName { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }

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
        public string scheduledPrepaymentFrequencyTypeName { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }
        public int internalPrudentialGuidelineStatusId { get; set; }
        public int externalPrudentialGuidelineStatusId { get; set; }
        public DateTime nplDate { get; set; }
        public short? scheduleDayCountConventionId { get; set; }
        public string scheduleDayCountConventionIName { get; set; }

        public short? scheduleDayInterestTypeId { get; set; }
        public int customerRiskRatingId { get; set; }

        // public double productPriceIndexRate { get; set; }
        public bool allowForceDebitRepayment { get; set; }

        //.............Fee Attribute.....................//
        public double exchangeRate { get; set; }

        public DateTime paymentDate { get; set; }
        public decimal totalAmount { get; set; }
        public int chargeFeeId { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; }

        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string sectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }
        public string comment { get; set; }

        //Loan Review Operation
        public int loanReviewOperationsId { get; set; }
        public int operationTypeId { get; set; }
        public string operationTypeName { get; set; }
        public DateTime newEffectiveDate { get; set; }
        public string reviewDetails { get; set; }
        public double? newInterateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int? newPrincipalFrequencyTypeId { get; set; }
        public string newPrincipalFrequencyTypeName { get; set; }
        public int? newInterestFrequencyTypeId { get; set; }
        public string newInterestFrequencyTypeName { get; set; }
        public DateTime? newPrincipalFirstPaymentDate { get; set; }
        public DateTime? newInterestFirstPaymentDate { get; set; }
        public int? newTenor { get; set; }
        public int? cASA_AccountId { get; set; }
        public string cASA_AccountName { get; set; }
        public string cASA_Account { get; set; }

        public decimal? overDraftTopup { get; set; }
        public decimal? fee_Charges { get; set; }
        public string terminationAndReBook { get; set; }
        public string completeWriteOff { get; set; }
        public string cancelUndisbursedLoan { get; set; }
        public string lmsLoanReferenceNumber { get; set; }
        public DateTime? newMaturityDate { get; set; }
        public int? loanSystemTypeId { get; set; }
        public int? maturityInstructionTypeId { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal pastDuePrincipal { get; set; }
        public decimal interestOnPastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal accruedInterest { get; set; }
        public decimal writeOffAmount
        {
            get
            {
                return pastDueInterest + pastDuePrincipal
+ interestOnPastDueInterest + interestOnPastDuePrincipal + outstandingPrincipal + accruedInterest;
            }
        }
        public List<feeDetails> fees { get; set; }
        public int creditAppraisalLoanApplicationId { get; set; }
        public int lmsLoanApplicationId { get; set; }
        public int lmsOperationId { get; set; }

        public int creditAppraisalOperationId { get; set; }
        public string lmsrApplicationReferenceNumber { get; set; }
        public decimal amount { get; set; }
        public string divisionCode { get; set; }
        public string divisionShortCode { get; set; }


        public int collateralLiquidationRecoveryId { get; set; }
        public int accreditedConsultant { get; set; }
        public bool isFullyRecovered { get; set; }
        public bool overwrite { get; set; }
        public byte?[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int? fileSize { get; set; }
        public string fileSizeUnit { get; set; }
        public DateTime receiptDate { get; set; }
        public decimal totalRecoveryAmount { get; set; }
        public decimal recoveredAmount { get; set; }
        public decimal? outstandingAmount { get; set; }
        public string collateralCode { get; set; }
        public string collectionMode { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string lienReferenceNumber { get; set; }
        public decimal lienAmount { get; set; }
        public DateTime lienDateTimeCreated { get; set; }
        public DateTime applicationDate { get; set; }
        public string lmsReferenceNumber { get; set; }
        public int loanApplicationId { get; set; }
        public int casaLienAccountId { get; set; }
        public int lienRemovalId { get; set; }
        public int lienRemovalOperationId { get; set; }
        public int unfreezeLienAccountId { get; set; }
        public string accreditedConsultantName { get; set; }
        public string accreditedConsultantCompany { get; set; }
        public string referenceId { get; set; }
        public DateTime requestDate { get; set; }
        public int numberOfLoans { get; set; }
        public string category { get; set; }
        public int bulkRecoveryApprovalId { get; set; }
        public int accreditedConsultantId { get; set; }
        public decimal? agentCommission { get; set; }
        public decimal? percentageCommission { get; set; }
        public decimal totalAmountRecovery { get; set; }
        public decimal? totalUnsettledAmount { get; set; }
        
        public string nameOfRecoveryAgent { get; set; }
        public string address { get; set; }
        public string telephoneNumber { get; set; }
        public DateTime expectedRecoveryDate { get; set; }

        public int periodToRecover { get { return (this.expectedRecoveryDate.Date - this.dateOfAssignment.Date).Days; } }
        public decimal amountRecovered { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public DateTime? dateOfEngagement { get; set; }
        public double accountBalance { get; set; }
        public string email { get; set; }
        public DateTime dateOfAssignment { get; set; }
        public int loanRecoveryReportBatchId { get; set; }
        public decimal? totalAllrecoveryAmount { get; set; }
        public string recoveryMisCode { get; set; }
        public string recoveryRegion { get; set; }
        public int loanRecoveryCommissionBatchId { get; set; }
        public string region { get; set; }
        public decimal? creditToAgent { get; set; }
        public decimal? amountToDebit { get; set; }
        public decimal? amountToCredit { get; set; }
        public string listOfAccountsAssigned { get; set; }
        public DateTime? recoveryDate { get; set; }
        public int? flagStatus { get; set; }
        public List<string> customerAddresses { get; set; }
        public string agentCategory { get; set; }
        public string agentAccountNumber { get; set; }
        public int? stateId { get; set; }
        public string assignedBy { get; set; }
        public string staffFullName { get; set; }
        public string accreditedConsultantEmail { get; set; }
        public decimal? commissionAmountLessWht { get; set; }
        public decimal? whtAmount { get; set; }
        public decimal? whtRate { get; set; }
        public decimal? commissionAmount { get; set; }
        public string phoneNumber { get; set; }
        public decimal? contingentOutstandingPrincipal { get; set; }
        public decimal? contigentOutstandingPrincipal { get; set; }
        public decimal? totalPrepayment { get; set; }

        public string exposureType { get; set; }
        public string expiryBand { get; set; }
        public string divisionName { get; set; }
        public bool operationCompleted { get; set; }
        public string branchCode { get; set; }
        public string productCode { get; set; }
        public string facilityType { get; set; }
        public string phoneNo { get; set; }
        public DateTime? valueDate { get; set; }
        public DateTime? maturityRevDate { get; set; }
        public decimal overduePrincipalAmount { get; set; }
        public decimal overdueInterestAmount { get; set; }
        public DateTime alueDate { get; set; }
    }

    public class CompletedCreditDocumentationModel
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
