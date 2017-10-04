using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanRepaymentScheduleViewModel2
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

}
