using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanPaymentScheduleVM
    {          
        public int paymentNumber { get; set; }
        public DateTime paymentDate { get; set; }
        public double startPrincipalAmount { get; set; }
        public double periodPaymentAmount { get; set; }
        public double periodInterestAmount { get; set; }
        public double periodPrincipalAmount { get; set; }
        public double endPrincipalAmount { get; set; }
        public decimal interestRate { get; set; }

        //public int amortisedPaymentNumber { get; set; }
        //public DateTime amortisedPaymentDate { get; set; }
        //public double amortisedStartPrincipalAmount { get; set; }
        //public double amortisedPeriodPaymentAmount { get; set; }
        //public double amortisedPeriodInterestAmount { get; set; }
        //public double amortisedPeriodPrincipalAmount { get; set; }
        //public double amortisedEndPrincipalAmount { get; set; }
        //public double effectiveInterestRate { get; set; }
        public int loanId { get; set; }
       
    }

    public class ScheduleLoans
    {
        public string applicationReferenceNumber { get; set; }
        public string loanReferenceNumber { get; set; }
        public string nhfAccount { get; set; }
        public decimal loanAmount { get; set; }
        public decimal interestRate { get; set; }
        public string product { get; set; }
    }

    public class LoanRepaymentScheduleForCreation
    {
        public string loanRefNo { get; set; }

    }
}
