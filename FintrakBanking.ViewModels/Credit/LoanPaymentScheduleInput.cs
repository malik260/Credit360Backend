using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanPaymentScheduleInputViewModel : GeneralEntity

    {
        public short scheduleMethodId { get; set; }
        public Double principalAmount { get; set; }
        public Double? payAmount { get; set; }
        public DateTime effectiveDate { get; set; }
        public double interestRate { get; set; }
        public int? loanId { get; set; }
        public short? principalFrequency { get; set; }
        public short? interestFrequency { get; set; }
        public int? principalFrequencyTypeId { get; set; }
        public int operationTypeId { get; set; }
        public int? interestFrequencyTypeId { get; set; }
        public Boolean shouldDisburse { get; set; }
        public DateTime loanDate { get; set; }
        public DateTime firstPaymentDate { get; set; }
        public string tenorMode { get; set; }
        public int numberOfPayments { get; set; }
        public int scheduleList { get; set; }
        public int? prepaymentMethodId { get; set; }
        public short? repricingModeId { get; set; }
        public int? repricingDuration { get; set; }
        public int? priceIndexId { get; set; }
        public double? equityContribution { get; set; }

        //public int tenor { get { return  }  }
        private int _tenor;

        public int tenor
        {
            get { return (maturityDate - effectiveDate).Days; }
            set { _tenor = value; }
        }

        public DateTime principalFirstpaymentDate { get; set; }
        public DateTime interestFirstpaymentDate { get; set; }
        public DateTime maturityDate { get; set; }
        public short accrualBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }
        public bool isExistingFacility { get; set; }


        public List<IrregularLoanScheduleInputViewModel> irregularPaymentSchedule { get; set; }
        //public int numberOfInstallments { get; set; }

        //public DateTime firstPaymentDate { get; set; }
        //public int numberOfPayments { get; set; }
        //public int numberOfPaymentsInAYear { get; set; }
        //public int daysInAYear { get; set; }
        //public Double feeRate { get; set; }
    }


    public class LoanPaymentRestructureScheduleInputViewModel : LoanPaymentScheduleInputViewModel
    {

        public int? loanRecoveryPaymentId { get; set; }
        public short dealTypeId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public int casaAccountId { get; set; }

        public int? loanReviewOperationsId { get; set; }

        public int? proposedTenor { get; set; }
        //public int tenor { get { return  }  }
        public short loanChangeType { get; set; }
        //public int loanId { get; set; }
        public Double payAmount { get; set; }
        public Double newAmount { get; set; }
        public int productId { get; set; }
        public int? newPrincipalFrequency { get; set; }
        public int? newInterestFrequency { get; set; }
        public DateTime? newPrincipalFirstpaymentDate { get; set; }
        public DateTime? newInterestFirstpaymentDate { get; set; }
        public Double payInterest { get; set; }
        public Double newInterest { get; set; }

        public int newTenor { get { return (maturityDate - newEffectiveDate).Days; } }
        public DateTime newEffectiveDate { get; set; }
        public decimal prepayment { get; set; }
        public int operationId { get; set; }
        public bool isManagementInterestRate { get; set; }
        public DateTime? newMaturityDate { get; set; }
        public int newTenorPrepayment { get { return ((DateTime)newMaturityDate - newEffectiveDate).Days; } }
        public int customerId { get; set; }
        public DateTime date { get; set; }
        public decimal feeRate { get; set; }
        public decimal feeAmount { get; set; }
        public decimal earnedFeeAmount { get; set; }
        public int chargeFeeId { get; set; }
        public int chargeFeeTypeId { get; set; }
        public decimal feeAmountDiff { get; set; }
        public int oldCasaAccountId { get; set; }
        public int? newCasaAccountId { get; set; }
        public short? loanSystemTypeId { get; set; }
        public short currencyId { get; set; }
        public new int operationTypeId { get; set; }
        public int? loanReviewApplicationId { get; set; }
    }


    public class LoanPaymentScheduleExtendedInputViewModel : LoanPaymentScheduleInputViewModel
    {
        public int numberOfPayments { get; set; }
        public int numberOfPaymentsInAYear { get; set; }
        public int daysInAYear { get; set; }
    }

    public class IrregularLoanScheduleInputViewModel
    {
        public DateTime paymentDate { get; set; }
        public Double paymentAmount { get; set; }
        public short paymentTypeId { get; set; }
    }

    public class LoanPaymentScheduleInput
    {
        public Double principalAmount { get; set; }
        public DateTime loanDate { get; set; }
        public Double interestRate { get; set; }
        public DateTime firstPaymentDate { get; set; }
        public int numberOfPayments { get; set; }
        public int numberOfPaymentsInAYear { get; set; }
        public int daysInAYear { get; set; }
        public Double feeRate { get; set; }
    }

    public class PaymentScheduleExcelViewModel
    {
        public PaymentScheduleExcelViewModel()
        {
            scheduleList = new List<PaymentScheduleVM>();
        }

        public Double principalAmount { get; set; }
        public DateTime loanDate { get; set; }
        public Double interestRate { get; set; }
        public DateTime firstPaymentDate { get; set; }
        public int numberOfPayments { get; set; }
        public string tenorMode { get; set; }

        public List<PaymentScheduleVM> scheduleList { get; set; }
    }

    public class PaymentScheduleVM
    {
        public DateTime paymentDate { get; set; }
        public double startPrincipalAmount { get; set; }
        public double periodicPaymentAmount { get; set; }
        public double periodInterestAmount { get; set; }
        public double periodPrincipalAmount { get; set; }
        public double deferredInterestAmount { get; set; }
        public double endPrincipalAmount { get; set; }
    }

    public class LoanRepaymentViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public string loanRefNo { get; set; }
        public DateTime paymentDate { get; set; }
        public decimal periodInterestAmount { get; set; }
        public decimal periodPrincipalAmount { get; set; }
        public double interestRate { get; set; }
        public short productId { get; set; }
        public short categoryId { get; set; }
        public double exchangeRate { get; set; }
        public short branchId { get; set; }
        public short currencyId { get; set; }
        public decimal totalAmount { get; set; }
        public int casaAccountId { get; set; }
        public byte transactionTypeId { get; set; }

        public int loanApplicationNumberId { get; set; }
        public int checklistId { get; set; }
        public short operationId { get; set; }
        public short maturityInstructionTypeId { get; set; }
        public decimal pastDueInterestAmount { get; set; }
        public decimal pastDuePrincipalAmount { get; set; }

        public decimal interestOnPastDueInterest { get; set; }

        public decimal interestOnPastDuePrincipal { get; set; }

        public int? casaAccountId2 { get; set; }


    }


    public class LoanPastDueViewModel : GeneralEntity

    {
        public int loanId { get; set; }
        public DateTime date { get; set; }
        public byte transactionTypeId { get; set; }
        public string pastDueCode { get; set; }
        public string parent_PastDueCode { get; set; }
        public string description { get; set; }
        public decimal debitAmount { get; set; }
        public decimal creditAmount { get; set; }
        public decimal totalAmount { get; set; }


    }
}
