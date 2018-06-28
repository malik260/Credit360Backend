using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class CurrentCustomerExposure
    {
        public string facilityType { get; set; }
        public decimal existingLimit { get; set; }
        public decimal proposedLimit { get; set; }
        public decimal change { get { return (existingLimit - proposedLimit); } }
        public decimal outstandings { get { return proposedLimit; } }

        public decimal PastDueObligationsPrincipal { get; set; }
        public decimal PastDueObligationsInterest { get; set; }
        public DateTime reviewDate { get; set; }        
        public string prudentialGuideline { get; set; }
        public string loanStatus { get; set; }
        public decimal recommendedLimit { get; set; }
    }

}
