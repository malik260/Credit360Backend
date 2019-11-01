using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class CashBacked
    {
        public double exchangeRate { get; set; }
        public string currencyID { get; set; }
        public decimal securityValueDollars { get; set; }
        public decimal securityValuePounds { get; set; }


        public int collateralCustomerId { get; set; }
        public string currencyUnit { get; set; }
        public string currencySecurityUnit { get; set; }
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
        //public double exchangeRate { get; set; }
        public string staffCode { get; set; }
        public string groupDescription { get; set; }
        public string teamDescription { get; set; }
        public string deskDescription { get; set; }
        public string buDescription { get; set; }
    }

    public class CashBackViewModel : GeneralEntity
    {
        public int cashBackId { get; set; }
        public string background { get; set; }
        public string issues { get; set; }
        public string request { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int operationId { get; set; }
    }
}
