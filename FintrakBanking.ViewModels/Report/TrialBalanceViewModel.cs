using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    public class TrialBalanceViewModel
    {
        public int glAccountId { get; set; }
        public byte[] reportData { get; set; }
        public string templateTypeName { get; set; }
        public string accountId { get; set; }
        public string accountCode { get; set; }
        public string accountName { get; set; }
        public string customAccountCode { get; set; }
        public string customAccountName { get; set; }
        public string currency { get; set; }
        public string currencyCode { get; set; }

        public string balanceType { get; set; }

        public double? currencyRate { get; set; }
        public decimal? totalDebit { get; set; }
        public decimal? totalDebitInBaseCurrency { get; set; }
        public decimal? totalCredit { get; set; }
        public decimal? balance { get; set; }
        public decimal? balanceInBaseCurrency { get; set; }
        public decimal? totalCreditInBaseCurrency { get; set; }

        public decimal? debitBalance { get; set; }
        public decimal? creditBalance { get; set; }
        public decimal? creditBalanceInBaseCurrency { get; set; }
        public decimal? debitBalanceInBaseCurrency { get; set; }

        public decimal? difference { get; set; }
        public string placeholderid { get; set; }
        public decimal? creditAmount { get; set; }
        public decimal? debitAmount { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public string sourceReferenceNumber { get; set; }
        public int customChartOfAccountId { get; set; }
        public int currencyId { get; set; }
        public DateTime? transactionDate { get; set; }
        public string batchCode { get; set; }
        public string narration { get; set; }
        public string customerName { get; set; }
    }
}
