using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Finance
{
    public class FinanceTransactionViewModel : GeneralEntity
    {  
        public string batchCode { get; set; }
        public int operationId { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime transactionDate { get; set; }
        public short currencyId { get; set; }
        public double currencyRate { get; set; }
        public DateTime postedDateTime { get; set; }
        public bool isApproved { get; set; }
        public int postedBy { get; set; }
        public int approvedBy { get; set; }
        public DateTime approvedDate { get; set; }
        public DateTime approvedDateTime { get; set; }
        public short sourceApplicationId { get; set; }

        public List<FinanceTransactionDetailViewModel> transactionDetails { get; set; }

    }

    public class FinanceTransactionDetailViewModel
    {
        public int transactionId { get; set; }
        public int glAccountId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public int? casaAccountId { get; set; }
        public decimal debitAmount { get; set; }
        public decimal creditAmount { get; set; }
        public short sourceBranchId { get; set; }
        public short destinationBranchId { get; set; }
    }

    public class CasaBalanceViewModel
    {
        public decimal availableBalance { get; set; }
        public decimal ledgerBalance { get; set; }
    }
}
