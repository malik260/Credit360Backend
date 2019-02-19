using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class AccountBalanceViewModel
    {
       
        public decimal outstandingPrincipal { get; set; }
        public decimal pastDuePrincipal { get; set; }
        public decimal pastDueInterest { get; set; }
        public decimal interestOnPastDuePrincipal { get; set; }
        public decimal interestOnPastDueInterest { get; set; }
        public decimal accruedInterest { get; set; }

        public decimal totalOutstandingBalance {
            get {
                return pastDueInterest + pastDuePrincipal
                  + interestOnPastDuePrincipal + interestOnPastDueInterest + accruedInterest + outstandingPrincipal;
            }
        }
    }
}
