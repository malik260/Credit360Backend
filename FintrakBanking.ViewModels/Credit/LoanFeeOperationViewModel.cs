using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanFeeOperationViewModel
    {
        public int loanId { get; set; }
        public int loanFeeId { get; set; }
        public int chargeFeeId { get; set; }
        public string chargeFeeName { get; set; }
        public int customerId { get; set; }
        public string loanReferenceNumber { get; set; }
        public decimal feeAmount { get; set; }
        public decimal feeRateValue { get; set; }
        public decimal feeEarnedAmount { get; set; }
        public decimal feeUnearnedAmount { get; set; }
        public decimal Amount { get; set; }
        public int operationTypeId { get; set; }
    }
}
