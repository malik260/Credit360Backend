using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ReceivableInterestReport
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int tenor { get; set; }
        public int tenorToDate { get; set; }
        public int tenorToMaturity { get; set; }
        public string interestType { get; set; }
        public string customerName { get; set; }
        public decimal principalAmount { get; set; }
        public double interestRate { get; set; }
        public decimal interestRateChange { get; set; }
        public decimal interestToDate { get; set; }
        public string accountPayTo { get; set; }
        public string accountReceiveFrom { get; set; }
        public decimal accruedInterestToDate { get; set; }
        public string businessGroup { get; set; }
        public string staffcode { get; set; }

    }
}
