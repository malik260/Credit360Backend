using System;

namespace FintrakBanking.ReportObjects.ViewModels
{
    public class LoanScheduleViewModel
    {
        public DateTime paymentDate { get; set; }
        public decimal startingBalance { get; set; }
        public decimal principalRepaymentAmount { get; set; }
        public decimal periodInterestAmount { get; set; }
        public decimal closePrincipalAmount { get; set; }
        public DateTime bookingDate { get; set; }
    }
}
