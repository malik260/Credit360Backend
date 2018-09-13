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
        public string referenceNumber { get; set; }
    }

    public class FacilitySummary
    {
        public int number { get; set; }
        public string facilityType { get; set; }
        public decimal existingLimit { get; set; }
        public decimal recommendedLimit { get; set; } // proposed amount fixed
        public decimal proposedLimit { get; set; } // approved amount
        public decimal change { get { return (recommendedLimit - proposedLimit); } }
        public decimal outstanding { get; set; }
        public decimal PastDuePrincipal { get; set; }
        public decimal PastDueInterest { get; set; }
        public DateTime reviewDate { get; set; }

        public string prudentialGuideline { get; set; }
        public string loanStatus { get; set; }
    }

    public class CustomerProduct
    {
        public int CUSTOMERID { get; set; }
        public int PRODUCTID { get; set; }
    }
}

/*
Customer Exposure --< Facility Summary
-----------------
Number	
Facility Type
Existing Limit
Proposed Limit	* approved
Change	
Outstanding	
Past Due Principal	
Past Due Interest	
Review Date
------------------
Total								
*/
