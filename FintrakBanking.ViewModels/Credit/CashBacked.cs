using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class CashBacked
    {
        public string branch { get; set; }
        public string accountNo { get; set; }
        public string accountName { get; set; }
        public string securityType { get; set; }
        public string depositAccountNo { get; set; }
        public decimal loanLimit { get; set; }
        public decimal loanBalance { get; set; }
        public decimal loanLimitForeignCurrency { get; set; }
        public decimal loanBalanceForeignCurrency { get; set; }
        public decimal securityValue { get; set; }
        public string currencyType { get; set; }
        public string securityInTheNameOf { get; set; }
        public string loanReferenceNumber { get; set; }
    }
}
