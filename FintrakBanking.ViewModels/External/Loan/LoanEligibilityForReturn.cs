using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanEligibilityForReturn
    {
        public string eligibilityMessage { get; set; }
        public bool isEligible { get; set; }
        public decimal eligibleAmount { get; set; } 
    }
}
