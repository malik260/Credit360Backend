using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class CashBackedBondAndGuarantee
    {
        public string serialNo { get; set; }
        public string bondGuaranteeType { get; set; }
        public string accountNo { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string beneficiary { get; set; }
        public decimal bondAmount { get; set; }
        public decimal nairaEqualvalentOfFCY { get; set; }
        public string currencyType { get; set; }
        public string purpose { get; set; }
        public DateTime dateIssue { get; set; }
        public string cashSecurityAccount { get; set; }
        public string typeofSecurity { get; set; }
        public decimal securityValue { get; set; }
        //public decimal securityAmountInNaira { get; set; }
        //public decimal securityAmountInDollar { get; set; }
        //public decimal securityAmountInEuro { get; set; }



    }
}
