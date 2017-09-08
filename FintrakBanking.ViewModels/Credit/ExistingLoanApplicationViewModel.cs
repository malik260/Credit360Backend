using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class ExistingLoanApplicationViewModel{

        public string applicationReferenceNumber { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal   principalAmount { get; set; }
        public double interestRate { get; set; }
        public double exchangeRate { get; set; }
        public string loanTypeName { get; set; }
        public int tenor { get; set; }
        public string  branch { get; set; }

    }

}
