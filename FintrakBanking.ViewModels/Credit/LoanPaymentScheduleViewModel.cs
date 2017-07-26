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
}
