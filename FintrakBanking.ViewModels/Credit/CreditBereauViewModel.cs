using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CreditBereauViewModel
    {
        public int customerId { get; set; }
        public string accountNumber { get; set; }
        public string accountStatus { get; set; }
        public double outstandingBalance { get; set; }
        public double installmentAmount { get; set; }
        public double currentAccount { get; set; }
        public double savingsAccount { get; set; }
    }
}
