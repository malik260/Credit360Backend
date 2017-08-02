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

    public class LoanPaymentSchedulePeriodicViewModel
    {
        public int paymentNumber { get; set; }        
        public DateTime paymentDate { get; set; }
        public double startPrincipalAmount { get; set; }
        public double periodicPaymentAmount { get; set; }
        public double periodInterestAmount { get; set; }
        public double periodPrincipalAmount { get; set; }        
        public double endPrincipalAmount { get; set; }
        public float interestRate { get; set; }

        public int amortisedPaymentNumber { get; set; }
        public DateTime amortisedPaymentDate { get; set; }
        public double amortisedStartPrincipalAmount { get; set; }
        public double amortisedPeriodicPaymentAmount { get; set; }
        public double amortisedPeriodInterestAmount { get; set; }
        public double amortisedPeriodPrincipalAmount { get; set; }
        public double amortisedEndPrincipalAmount { get; set; }
        public double internalRateOfReturn { get; set; }
    }

    //Sequence int, RefNo varchar(50), [Date] datetime, PaymentDate datetime, OpeningBalance money, AmountPrincInit money, DailyPayment money, 
    //DailyInt money, DailyPrinc money, ClosingBalance money, AmountPrincEnd money,NorminalRate float, AMSequence int, AMRefNo varchar(50), AMDate datetime,
    //  AMPaymentDate datetime, AMOpeningBalance money, AMAmountPrincInit money,AMDailyPayment money, AMDailyInt money, AMDailyPrinc money, AMClosingBalance money,
    //AMAmountPrincEnd money, BalloonAmt money, EffectiveRate float,NoOfPeriods int,PostedDate datetime, LastRunDate datetime
}
