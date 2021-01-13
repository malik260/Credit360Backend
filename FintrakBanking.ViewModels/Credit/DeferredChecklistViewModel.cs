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
        public DateTime? deferredDate { get; set; }
        public int conditionId { get; set; }
        public string condition { get; set; }
        public string checklistStatus { get; set; }
        public int approvalStatusId { get; set; }
        public int approvalStateId { get; set; }
        public int? loopedStaffId { get; set; }
        public int? toStaffId { get; set; }
        public string approvalStatusName { get; set; }
        public string deferralReason { get; set; }
        public string createdBy { get; set; }
        public DateTime dateCreated { get; set; }
        public string customerName { get; set; }
        public string applicationRefNo { get; set; }
        public int deferralDuration { get; set; }
        public int cumulativeDays { get { return (DateTime.Now - this.dateApproved).Value.Days; } }

        public string toApprovalLevelName { get; set; }
        public string fromApprovalLevelName { get; set; }
        public string responsiblePerson { get; set; }
        public int operationId { get; set; }
        public int? deferredDays { get; set; }
        public int customerId { get; set; }
        public int accountOfficer { get; set; }
        public int targetId { get; set; }
        public int toApprovalLevelId { get; set; }
        public int responseStaffId { get; set; }
        public string excludeLegal { get; set; }
        public int approvalTrailId { get; set; }
        public int loanConditionId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public DateTime? deferredDateOnFinalApproval { get; set; }
        public DateTime? dateApproved { get; set; }
    }
}
