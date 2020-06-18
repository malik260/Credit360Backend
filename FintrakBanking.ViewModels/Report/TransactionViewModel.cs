using System;

namespace FintrakBanking.Finance.ViewModels

{
    public class GLAccountSearchViewModel
    {
        public string GLAccountCode { get; set; }
        public string GLAccount { get; set; }
        public int GLAccountId { get; set; }
    }
    public class TransactionViewModel
    {
        public int customerId;

        public string accountNumber { get; set; }

        public int transactionID { get; set; }

        public decimal outstandingBalance { get; set; }

        public string branch { get; set; }
        public int branchId { get; set; }
        public string batchNo { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public string accountName { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime? postedDate { get; set; }
        public DateTime? postedTime { get; set; }
        public string postedBy { get; set; }
        public string approvedBy { get; set; }
        public string postCurrency { get; set; }
        public double currencyRate { get; set; }
        public DateTime? approvedDate { get; set; }
        public string baseCurrency { get; set; }
        public string companyName { get; set; }
        public string logoPath { get; set; }
        public DateTime repostDate { get { return DateTime.Now; } }
        public int postedByStaffId { get; set; }
        public string casaAccountNumber { get; set; }
        public string GLAccount { get; set; }
        public string branchName { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string branchCode { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string batchNo2 { get; set; }
        public string GLAccountCode { get; set; }
        public int GLAccountId { get; set; }
        public int glAccountId { get; set; }
        public string operationName { get; set; }
    }
    public class BulkTransactionViewModel
    {
        public int bulkTransactionID { get; set; }
        public int sid { get; set; }
        public string batchId { get; set; }
        public string debitAccount { get; set; }
        public string creditAccount { get; set; }
        public int operationId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public decimal amount { get; set; }
        public string  transactionType { get; set; }
        public string flowType { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime postedDate { get; set; }
        public int sourceBranchId { get; set; }
        public int destinationBranchId { get; set; }
        public string currencyRateCode { get; set; }
        public double currencyRate { get; set; }
        public DateTime syetemDateTime { get; set; }
        public bool isposted { get; set; }
        public string postedBy { get; set; }
        public string forceDebitAccount { get; set; }
        public string bankId { get; set; }
        public decimal amountCollected { get; set; }
        public string branchName { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string branchCode { get; set; }
    }
    public class DailyAccrualViewModel
    {
        public int dailyAccrualId { get; set; }
        public string referenceNumber { get; set; }
        public string baseReferenceNumber { get; set; }
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int transactionTypeId { get; set; }
        public string transactionTypeName { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
        public int currencyId { get; set; }
        public string currencyName { get; set; }
        public double exchangeRate { get; set; }
        public decimal mainAmount { get; set; }
        public double interestRate { get; set; }
        public int dayCountConventionId { get; set; }
        public decimal dailyAccrualAmount { get; set; }
        public bool repaymentPostedStatus { get; set; }
        public DateTime dateTimeTime { get; set; }
        public DateTime date { get; set; }
        public string productName { get; set; }
        public string productCode { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string branchCode { get; set; }
    }
    
}
