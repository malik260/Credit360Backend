using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class CommercialLoanReport
    {
        public int termLoanId { get; set; }
        public DateTime capturesDate { get; set; }
        public DateTime dealDate { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int tenor { get; set; }
        public int tenorToDate { get; set; }
        public string loanReferenceNo { get; set; }
        public string status { get; set; }
        public string interestType { get; set; }
        public string customerName { get; set; }
        public decimal principalAmount { get; set; }
        public double interestRate { get; set; }
        public decimal interestRateChange { get; set; }
        public decimal interestToDate { get; set; }
        public string accountPayTo { get; set; }
        public string accountReceiveFrom { get; set; }
        public string narration { get; set; }
        public string currency { get; set; }
        public string businessGroup { get; set; }
        public string staffcode { get; set; }

    }
}
