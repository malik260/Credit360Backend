using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.Finance;
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
        public double tenor { get; set; }
        public DateTime terminationDate { get; set; }
        public DateTime effectiveDate { get; set; }
        public string totalRepayment { get { return (principalRepayment + interestAccrual).ToString("#,#.00#"); } }
    }

    public class LoanViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
        public int loanApplicationId { get; set; }

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
        public string approvedBy { get; set; }
        public string approverComment { get; set; }
        public DateTime? dateApproved { get; set; }
        public short loanStatusId { get; set; }
        public short scheduleTypeId { get; set; }
        public bool isDisbursed { get; set; }
        public string disbursedBy { get; set; }
        public string disburserComment { get; set; }
        public DateTime? disburseDate { get; set; }
        public decimal? approvedAmount { get; set; }
        public bool creditAppraisalCompleted { get; set; }
        public int? operationId { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public string trancheBatchCode { get; set; }
        public decimal equityContribution { get; set; }
        public short subSectorId { get; set; }
        public DateTime? firstPrincipalPaymentDate { get; set; }
        public DateTime? firstInterestPaymentDate { get; set; }
        public decimal outstandingPrincipal { get; set; }
        public int? principalAdditionCount { get; set; }
        public int? principalReductionCount { get; set; }
        public bool fixedPrincipal { get; set; }
        public bool profileLoan { get; set; }
        public bool dischargeLetter { get; set; }
        public bool suspendInterest { get; set; }
        public bool booked { get; set; }
        public bool scheduled { get; set; }
        public bool isScheduledPrepayment { get; set; }
        public decimal scheduledPrepaymentAmount { get; set; }
        public DateTime scheduledPrepaymentDate { get; set; }
        public short scheduledPrepaymentFrequencyTypeId { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string customerCode { get; set; }
        public string productAccountNumber { get; set; }
        public int currencyId { get; set; }
        public short accurialBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isCamsol { get; set; }


        //.............Other Attributes................//
        public int productTypeId { get; set; }
        public string productTypeName { get; set; }
        public string creatorName { get; set; }
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string subSectorName { get; set; }
        public string SectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string pricipalFrequencyTypeName { get; set; }
        public string interestFrequencyTypeName { get; set; }


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
        public string SectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string loanStatusName { get; set; }
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
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
        public short branchId { get; set; }
        public short currencyId { get; set; }
        public double exchangeRate { get; set; }
        public int loanApplicationId { get; set; }
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
        public string SectorName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string productName { get; set; }
        public string customerSensitivityLevelName { get; set; }
        public string customerName { get; set; }
        public string loanStatusName { get; set; }

        //......Loan Relational Table View Mapping Models..............//
        public List<LoanCovenantDetailViewModel> loanCovenant { get; set; }
        public List<LoanChargeFeeViewModel> loanChargeFee { get; set; }
        public List<LoanGuarantorViewModel> loanGuarantor { get; set; }
        public List<LoanCollateralMappingViewModel> loanCollateral { get; set; }
        //......End f Loan Relational Table View Mapping Models......//

    }

    public class CamProcessedLoanViewModel : LoanApplicationViewModel
    {
        public string loanDetails { get; set; }
        public string camReference { get; set; }
        public int appraisalMemorandumId { get; set; }
        public string customerCode { get; set; }
        public int casaAccountId { get; set; }
        public string loanStatusName { get; set; }
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public int customerSensitivityLevelId { get; set; }
        public string camDocumentation { get; set; }
    }

    public class AppraisalMemorandumLoanDetailViewModel
    {
        public int appraisalMemorandumLoanDetailId { get; set; }

        public int appraisalMemorandumId { get; set; }

        public decimal principalAmount { get; set; }

        public double interestRate { get; set; }

        public int tenor { get; set; }

    }

    
    public class LoanChargeFeeViewModel : ChargeRangeViewModel

    {
        public int productFeeId { get; set; }
        public int loanChargeFeeId { get; set; }
        public int loanId { get; set; }
        public int productId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeRateValue { get; set; }
        public decimal feeDependentAmount { get; set; }
        public decimal feeAmount { get; set; }
        public short feeIntervalId { get; set; }
        public string feeIntervalName { get; set; }
        public int feeTypeId { get; set; }
        public string feeTypeName { get; set; }
        public bool isIntegralFee { get; set; }

    }

    public class LoanCollateralMappingViewModel : CollateralCustomerViewModel
    {
        public int loanCollateralMappingId { get; set; }
        public int loanId { get; set; }
        public int loanApplicationId { get; set; }

    }



    public class LoanGuarantorViewModel
    {
    public short loanGuarantorId { get; set; }
    public int loanId { get; set; }
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

    }

    public class LoanSearchViewModel
    {
        public string customerName { get; set; }
        public string loanName { get; set; }
        public string loanReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }

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

        public decimal dailyAccuralAmount { get; set; }

        public decimal availableBalance { get; set; }

        public int daysInAYear { get; set; }




    }

}
