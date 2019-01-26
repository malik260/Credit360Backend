using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanPaymentScheduleViewModel
    {
        public int paymentNumber { get; set; }
        public DateTime paymentDate { get; set; }
        public double startPrincipalAmount { get; set; }
        public double periodicPaymentAmount { get; set; }
        public double periodInterestAmount { get; set; }
        public double periodPrincipalAmount { get; set; }
        public double deferredInterestAmount { get; set; }
        public double endPrincipalAmount { get; set; }
    }

    public class LoanPaymentSchedulePeriodicViewModel: GeneralEntity
    {
        public int paymentNumber { get; set; }        
        public DateTime paymentDate { get; set; }
        public double startPrincipalAmount { get; set; }
        public double periodPaymentAmount { get; set; }
        public double periodInterestAmount { get; set; }
        public double periodPrincipalAmount { get; set; }        
        public double endPrincipalAmount { get; set; }
        public double interestRate { get; set; }

        //public int amortisedPaymentNumber { get; set; }
        //public DateTime amortisedPaymentDate { get; set; }
        public double amortisedStartPrincipalAmount { get; set; }
        public double amortisedPeriodPaymentAmount { get; set; }
        public double amortisedPeriodInterestAmount { get; set; }
        public double amortisedPeriodPrincipalAmount { get; set; }
        public double amortisedEndPrincipalAmount { get; set; }
        public double effectiveInterestRate { get; set; }
        public int loanId  { get; set; }
        public double? previousInterestAmount  { get; set; }
        public double? previousPrincipalAmount { get; set; }
        public string archiveCode { get; set; }
        public string LoanReferenceNumber { get; set; }
        public string customerName { get; set; }
        public decimal casaBalance { get; set; }
        public DateTime nextPaymentDate { get; set; }
        public int notificationDuration { get; set; }
        public int relationshipManagerId { get; set; }
        public string relationshipManagerEmail { get; set; }
        public decimal principalAmount { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string scheduleTypeName { get; set; }
        public double interestRateArc { get; set; }
        public string email { get; set; }
    }

    public class LoanPaymentScheduleDailyViewModel: GeneralEntity
    {
        public int paymentNumber { get; set; }
        public DateTime date { get; set; }
        public DateTime paymentDate { get; set; }

        public double openingBalance { get; set; }
        public double startPrincipalAmount { get; set; }
        public double dailyPaymentAmount { get; set; }
        public double dailyInterestAmount { get; set; }
        public double dailyPrincipalAmount { get; set; }
        public double closingBalance { get; set; }
        public double endPrincipalAmount { get; set; }
        public double accruedInterest { get; set; }
        public double amortisedCost { get; set; }
        public double norminalInterestRate { get; set; }


        public double amOpeningBalance { get; set; }
        public double amStartPrincipalAmount { get; set; }
        public double amDailyPaymentAmount { get; set; }
        public double amDailyInterestAmount { get; set; }
        public double amDailyPrincipalAmount { get; set; }
        public double amClosingBalance { get; set; }
        public double amEndPrincipalAmount { get; set; }
        public double amAccruedInterest { get; set; }
        public double amAmortisedCost { get; set; }

        public double balloonAmt { get; set; }
        public double discountPremium { get; set; }
        public double unEarnedFee { get; set; }
        public double earnedFee { get; set; }
        public double effectiveInterestRate { get; set; }
        public int numberOfPeriods { get; set; }
        public int loanId { get; set; }
        public double ballonAmount { get; set; }
        public double? previousInterestAmount { get; set; }
        public double? previousPrincipalAmount { get; set; }


    }

    public class FeePaymentScheduleViewModel
    {
        public int paymentNumber { get; set; }
        public DateTime feeDate { get; set; }
        public decimal feeAmount { get; set; }
  
    }

}
