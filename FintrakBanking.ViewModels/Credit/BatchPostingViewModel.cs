using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class BatchPostingViewModel
    {
        public int sid { get; set; }
        public string batchId { get; set; }
        public int batchRefId  { get; set; }
        public string    trancType { get; set; }
        public string    flowType { get; set; }
        public decimal amount { get; set; }
        public string drAccount { get; set; }
        public string crAccount { get; set; }
        public string currencyCode { get; set; }
        public decimal rate { get; set; }
        public string naration { get; set; }
        public string status { get; set; }
        public string tranactionId { get; set; }
        public string postedFlag { get; set; }
        public DateTime? postedDate { get; set; }
        public DateTime? creditDate { get; set; }
        public string postedUserId { get; set; }
        public string failedFlag { get; set; }
        public string deleteFlag { get; set; }
        public string failureReasonCode { get; set; }
        public decimal amountCollected { get; set; }
        public decimal lienAmount { get; set; }
        public string lienFlg { get; set; }
        public string TodFlg { get; set; }
        public int valueDateNumber { get; set; }
        public string loanAccount   { get; set; }
        public string fintrakFlag   { get; set; }
        public string bankId   { get; set; }
        public decimal? totalAmount { get; set; }
        public decimal? totalAmountCollected { get; set; }
        public string    isSelected { get; set; }
        public string rcreUser { get; set; }
        public decimal? recCount { get; set; }
        public string rateCode { get; set; }
        public decimal amt { get; set; }
        public DateTime? rcreDate { get; set; }
        public string failureReason { get; set; }
    }
}
