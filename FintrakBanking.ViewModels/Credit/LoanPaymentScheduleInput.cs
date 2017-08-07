using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanPaymentScheduleInputViewModel
    {
        public short scheduleMethodId { get; set; }
        public Double principalAmount { get; set; }
        public DateTime effectiveDate { get; set; }
        public double interestRate { get; set; }
        public short principalFrequency { get; set; }
        public short interestFrequency { get; set; }
        public int tenor { get; set; }
        public DateTime principalFirstpaymentDate { get; set; }
        public DateTime interestFirstpaymentDate { get; set; }
        public DateTime maturityDate { get; set; }
        public short accurialBasis { get; set; }
        public double integralFeeAmount { get; set; }
        public short firstDayType { get; set; }

        public List<IrregularLoanScheduleInputViewModel> irregularPaymentSchedule { get; set; }
        //public int numberOfInstallments { get; set; }

        //public DateTime firstPaymentDate { get; set; }
        //public int numberOfPayments { get; set; }
        //public int numberOfPaymentsInAYear { get; set; }
        //public int daysInAYear { get; set; }
        //public Double feeRate { get; set; }
    }

    public class LoanPaymentScheduleExtendedInputViewModel: LoanPaymentScheduleInputViewModel
    {
        public int numberOfPayments { get; set; }
        public int numberOfPaymentsInAYear { get; set; }
        public int daysInAYear { get; set; }        
    }

    public class IrregularLoanScheduleInputViewModel
    {
        public DateTime paymentDate { get; set; }
        public Double paymentAmount { get; set; }
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
}
