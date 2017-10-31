using System;

namespace FintrakBanking.Finance.ViewModels

{
    public class TransactionViewModel
    {
        public string branch { get; set; }
        public int branchId { get; set; }
        public string batchNo { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public string accountName { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime postedDate { get; set; }
        public DateTime postedTime { get; set; }
        public string postedBy { get; set; }
        public string approvedBy { get; set; }
        public string postCurrency { get; set; }
        public double currencyRate { get; set; }
        public DateTime approvedDate { get; set; }
        public string baseCurrency { get; set; }
        public string companyName { get; set; }
        public string logoPath { get; set; }
        public DateTime repostDate { get { return DateTime.Now; } }
        public int postedByStaffId { get; set; }
    }

   
}
