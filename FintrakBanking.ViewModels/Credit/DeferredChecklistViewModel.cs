using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DeferredChecklistViewModel
    {
        public int checklistDeferralId { get; set; }
        public int loanApplicationId { get; set; }
        public DateTime deferredDate { get; set; }
        public int conditionId { get; set; }
        public string condition { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public string deferralReason { get; set; }
        public string createdBy { get; set; }
        public DateTime dateCreated { get; set; }
        public string customerName { get; set; }
        public string applicationRefNo { get; set; }
        public int deferralDuration { get { return (this.deferredDate - dateCreated).Days; } }
        public int cumulativeDays { get { return (DateTime.Now - this.dateCreated).Days; } }
    }
}
