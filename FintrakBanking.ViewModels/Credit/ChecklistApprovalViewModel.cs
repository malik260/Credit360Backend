using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ChecklistApprovalViewModel
    {
        public int loanApplicationDetailId;
        //public int deferralDuration2 { get { return (this.deferredDate - this.dateTimeDefCreated).Value.Days; } }
        //public int cummulativeDays2 { get { return (this.deferredDate - this.dateTimeDefCreated).Value.Days; } }
        public int operationId { get; set; }

        public string applicationReferenceNumber { get; set; }
        public int loanApplicationId { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public decimal proposedAmount { get; set; }
        public string approvalStatus { get; set; }
        public string condition { get; set; }
        public string checklistStatus { get; set; }
        public int conditionId { get; set; }
        public int deferralId { get; set; }
        public DateTime? deferredDate { get; set; }
        public DateTime? dateCreated { get; set; }
        public DateTime? systemArrivalDateTime { get; set; }
        public int deferralDuration { get; set; }
        public int cumulativeDays { get { return (DateTime.Now - this.dateApproved).Value.Days; } }

        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal applicationTenor { get; set; }
        public DateTime applicationDate { get; set; }
        public bool isInvestmentGrade { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public bool isRelatedParty { get; set; }
        public int approvalStatusId { get; set; }
        public int applicationStatusId { get; set; }
        public int loanId { get; set; }
        public short LOANSYSTEMTYPEID { get; set; }
        public bool submittedForAppraisal { get; set; }
        public string loanInformation { get; set; }
        public bool isLms { get; set; }
        public string reason { get; set; }
        public int customerId { get; set; }
        public string toApprovalLevelName { get; set; }
        public string fromApprovalLevelName { get; set; }
        public string comment { get; set; }
        public string excludeLegal { get; set; }
        public int approvalTrailId { get; set; }
        public int loanConditionId { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public string divisionShortCode { get; set; }
        public string divisionCode { get; set; }
        public DateTime? deferredDateOnFinalApproval { get; set; }
        public DateTime? dateApproved { get; set; }
        public int numberOfTimesDeferred { get; set; }
    }
}
