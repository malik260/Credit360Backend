using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ChecklistApprovalViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public int loanApplicationId { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public decimal proposedAmount { get; set; }
        public string approvalStatus { get; set; }
        public string condition { get; set; }
        public int conditionId { get; set; }
        public DateTime? deferredDate { get; set; }
        public int deferralDuration { get; set; }
        public int cummulativeDays { get; set; }
    }
}
