using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Text;

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
    }

    public class LoanViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        //public decimal productPriceIndexRate { get; set; }
        public double productPriceIndexRate { get; set; }
        public int casaAccountId { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        
        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int tenor { get { return (this.maturityDate - this.effectiveDate).Days; } }
        public short ? principalFrequencyTypeId { get; set; }
        public short ? interestFrequencyTypeId { get; set; }
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
        public int? approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public short scheduleTypeId { get; set; }
        public string scheduleTypeName { get; set; }
        public bool isDisbursed { get; set; }
        public int? disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public string operationName {get; set; }
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
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public string currency { get; set; }
        public short accurialBasis { get; set; }
        public decimal integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }
        public int? internalPrudentialGuidelineStatusId { get; set; }
        public int? externalPrudentialGuidelineStatusId { get; set; }
        public DateTime? nplDate { get; set; }
        public short scheduleDayCountConventionId { get; set; }
        public short scheduleDayInterestTypeId { get; set; }
        public int? customerRiskRatingId { get; set; }

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
        public string relationshipManagerEmail { get; set; }
        public string relationshipOfficerEmail { get; set; }
        public decimal customerAvailableAmount { get; set; }
        public bool feeOverride { get; set; }

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
        public decimal overdraftLimit { get; set; }

        //......End f Loan Relational Table View Mapping Models......//
    }

    public class RevolvingLoanViewModel : GeneralEntity
    {
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
        public int approvalStatusId { get; set; }
        public string approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public bool isDisbursed { get; set; }
        public string disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public int? operationId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public string trancheBatchCode { get; set; }
        public bool dischargeLetter { get; set; }
        public bool suspendInterest { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public int? internalPrudentialGuidelineStatusId { get; set; }
        public int? externalPrudentialGuidelineStatusId { get; set; }
        public DateTime? nplDate { get; set; }

        //.............Other Attributes................//
        public int productTypeId { get; set; }

        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        //public string SectorName { get; set; }
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
        //.............End of Other Attributes...........//

        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        //......End f Loan Relational Table View Mapping Models......//
    }

    public class ContingentLoanViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public int loanApplicationId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
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
        public decimal contingentAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public int approvalStatusId { get; set; }
        public string approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public bool isDisbursed { get; set; }
        public string disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public int? operationId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public string trancheBatchCode { get; set; }
        public bool dischargeLetter { get; set; }
        public short customerSensitivityLevelId { get; set; }

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
        public string loanStatusName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string teamMiscode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public string comment { get; set; }
       // public string SectorName { get; set; }
        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        //......End f Loan Relational Table View Mapping Models......//
    }

    public class LoanBookingRequestViewModel : GeneralEntity
    {
        public int loanBookingRequestId { get; set; }

        public int loanApplicationId { get; set; }

        public decimal amount_Requested { get; set; }

        public short approvalStatusId { get; set; }
        public string comment { get; set; }

    }

    public class CustomerExposure
    {
        public int customerId { get; set; }
    }

    public class CamProcessedLoanViewModel : LoanApplicationViewModel
    {
        public string loanDetails { get; set; }
        public string camReference { get; set; }
        public List<CasaViewModel> customerAccounts { get; set; }
        public int appraisalMemorandumId { get; set; }
        public int? casaAccountId { get; set; }
        public string loanStatusName { get; set; }
        public string sectorName { get; set; }
        public string subSectorName { get; set; }
        public string sectorSubSectorName { get {return (this.sectorName + "/" + this.subSectorName); } } 
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public int customerSensitivityLevelId { get; set; }
        public string camDocumentation { get; set; }
        public decimal groupApprovedAmount { get; set; }
        public int approvedTenor { get; set; }
        public decimal ? customerAvailableAmount { get; set; }
        public string customerOccupation { get; set; }
        public string customerType { get; set; }

        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }

        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        public CustomerCompanyInfomationViewModels companyInformation { get; set; }

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
        public int productFeeId { get; set; }
        public int loanChargeFeeId { get; set; }
        public short productTypeId { get; set; }
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
    }

    public class LoanCollateralMappingViewModel : CollateralViewModel
    {
        public int loanCollateralMappingId { get; set; }
        public int loanId { get; set; }
        public int loanApplicationId { get; set; }

        
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
    }

    public class LoanSearchViewModel
    {
        public string customerName { get; set; }
        public string loanName { get; set; }
        public string loanReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }
    }

    public class LoanCovenantDetailViewModel : GeneralEntity
    {
        public int loanCovenantDetailId { get; set; }
        public string covenantDetail { get; set; }
        public int loanId { get; set; }
        public short covenantTypeId { get; set; }
        public short? frequencyTypeId { get; set; }
        public decimal? covenantAmount { get; set; }
        public DateTime covenantDate { get; set; }
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
    }

    public class DailyInterestAccrualViewModel : GeneralEntity

    {
        public string referenceNumber { get; set; }

        public string baseReferenceNumber { get; set; }

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

        public short dayCountConventionId { get; set; }

        public double dailyAccuralAmount { get; set; }

        public decimal availableBalance { get; set; }

        public int daysInAYear { get; set; }




    }


    public class SubAllocationViewModel 
    {
        public int fromLoanId  { get; set; }
        public decimal fromAmount  { get; set; }
    }

    public class LoanAvailmentApprovalViewModel: ApprovalViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public short applicationStatusId { get; set; }
    }

}