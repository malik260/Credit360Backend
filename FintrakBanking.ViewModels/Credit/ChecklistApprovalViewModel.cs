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
        public string checklistStatus { get; set; }
        public int conditionId { get; set; }
        public DateTime? deferredDate { get; set; }
        public DateTime? dateCreated { get; set; }
        public int deferralDuration { get; set; }
        public int cummulativeDays { get; set; }

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
        public bool submittedForAppraisal { get; set; }
        public string loanInformation { get; set; }



        
            
            
            
    }
}
