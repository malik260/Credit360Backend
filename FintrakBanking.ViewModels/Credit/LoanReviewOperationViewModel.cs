using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanReviewOperationViewModel : GeneralEntity
    {
        public int loanReviewOperationsId { get; set; }

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

        public List<LoanReviewIrregularScheduleViewModel> reviewIrregularSchedule { get; set; }
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

    public class LoanReviewOperationApprovalViewModel
    {
        public string currencyCode { get; set; }
        public bool isBankFormat { get; set; }
        public int companyId { get; set; }
        public string createdByName { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public decimal maturityAmount { get; set; }
        public string relatedReferenceNumber { get; set; }
        public decimal interestAmount { get; set; }

        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public decimal productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
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
        public short scheduledPrepaymentFrequencyTypeId { get; set; }
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
        public short scheduleDayCountConventionId { get; set; }
        public short scheduleDayInterestTypeId { get; set; }
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
        public decimal? newInterateRate { get; set; }
        public decimal? prepayment { get; set; }
        public int? newPrincipalFrequencyTypeId { get; set; }
        public int? newInterestFrequencyTypeId { get; set; }
        public DateTime? newPrincipalFirstPaymentDate { get; set; }
        public DateTime? newInterestFirstPaymentDate { get; set; }
        public int? newTenor { get; set; }
        public int? cASA_AccountId { get; set; }
        public decimal? overDraftTopup { get; set; }
        public decimal? fee_Charges { get; set; }
        public string terminationAndReBook { get; set; }
        public string completeWriteOff { get; set; }
        public string cancelUndisbursedLoan { get; set; }
    }


    public class LoanReviewApplicationViewModel : GeneralEntity
    {
        public int loanReviewApplicationId { get; set; }
        public int loanId { get; set; }
        public int productTypeId { get; set; }
        public int operationTypeId { get; set; }
        public string reviewDetails { get; set; }
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
    }

    public class SelectListViewModel
    {
        public List<DropDownSelect> casaAccounts { get; set; }
        public List<DropDownSelect> productTypes { get; set; }
        public List<DropDownSelect> operationTypes { get; set; }
        public List<DropDownSelect> interestFrequencyTypes { get; set; }
        public List<DropDownSelect> principalFrequencyTypes { get; set; }
    }

    public class LoanReviewOperationParentChildViewModel
    {
        public double approvedInterestRate { get; set; }
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
        public List<LoanReviewOperationApprovalViewModel>  runningTranches { get; set; }
    }

    public class DropDownSelect
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class MaturityIntructionTypeViewModel
    {
        public short instructionTypeId { get; set; }
        public string instructionTypeName { get; set; }
    }
}
