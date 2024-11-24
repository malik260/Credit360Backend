using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanApplicationForReturn
    {
        public string applicationReferenceNumber { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; } 
        public string approvalStatus { get; set; }
        public string loanBranch { get; set; }
        public DateTime applicationDate { get; set; }
        public bool submittedForAppraisal { get; set; }
        public string currentApprovalLevel { get; set; }
        public string applicationStatus { get; set; }
        public bool isOfferLetterAvailable { get; set; }

        public List<LoanApplicationDetailForReturn> loanApplicationDetails { get; set; }
    }
}
