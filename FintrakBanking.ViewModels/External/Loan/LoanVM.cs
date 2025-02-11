using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanVM
    {
        public string applicationReferenceNumber { get; set; }
        public string loanReferenceNumber { get; set; }
        public string accountNumber { get; set; }
        public decimal loanAmount { get; set; }
        public double interestRate { get; set; }
        public string product { get; set; }
        public string loanStatus { get; set; }
        public DateTime? disbursedDate { get; set; }
        public string company { get; set; }
        public string customerName { get; set; }
        public int tenor { get; set; }
    }
}
