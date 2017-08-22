using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
   public class LoanViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public int customerId { get; set; }
        public short productId { get; set; }
        public int casaAccountId { get; set; }
        public short branchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public int tenor { get; set; }
        public short tenorModeId { get; set; }
        public short principalFrequencyTypeId { get; set; }
        public short interestFrequencyTypeId { get; set; }
        public short feeFrequencyTypeId { get; set; }
        public int principalNumberOfInstallment { get; set; }
        public int interestNumberOfInstallment { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public double interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime terminalDate { get; set; }
        public DateTime dateCreated { get; set; }
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
        public bool hasLien { get; set; }
        public bool hasOfferLetter { get; set; }
        public int? customerGroupId { get; set; } 
        public decimal ? groupAmount { get; set; }
        public short loanTypeId { get; set; }
        public int? loanTypeBatchId { get; set; }
        public string trancheBatchCode { get; set; }
        public decimal? equityContribution { get; set; }
        public decimal? feePercent { get; set; }
        public DateTime? firstPrincipalPaymentDate { get; set; }
        public DateTime? firstInterestPaymentDate { get; set; }
        public decimal? outstandingPrincipal { get; set; }
        public int? principalAdditionCount { get; set; }
        public int? principalReductionCount { get; set; }
        public bool fixedPrincipal { get; set; }
        public bool profileLoan { get; set; }
        public bool dischargeLetter { get; set; }
        public bool suspendInterest { get; set; }
        public bool canDisburse { get; set; }
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
        public string productAccountName { get; set; }
        public string loanTypeName { get; set; }
        public string branchName { get; set; }
        public string customerName { get; set; }
    }

    public class CamProcessedLoanViewModel : LoanApplicationViewModel
    {
        public string loanDetails { get; set; }
        public string camReference { get; set; }
        public string customerCode { get; set; }
        public short productId { get; set; }
    }

    public class LoanSearchViewModel
    {
        public string customerName { get; set; }
        public string loanName { get; set; }
        public string loanReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }

    }

}
