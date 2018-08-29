using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Credit
{
    public   class LoanApplicationCollateralViewModel : GeneralEntity
    {
        public string collateralReferenceNumber { get; set; }         

        public string applicationReferenceNumber { get; set; }

        public string collateralType { get; set; }

        public string collateralSubtype { get; set; }

        public decimal collateralValue { get; set; }

        public int loanApplicationId { get;set;}

        public int collateralCustomerId { get; set; }

        public int? loanApplicationDetailId {get;set;}

        public int loanAppCollateralId { get; set; }

        public double haircut { get; set; }

        public bool allowSharing { get; set; }

        public int? valuationCycle { get; set; }

        public string currencyCode { get; set; }
        public double securityValue { get { return decimal.ToDouble(collateralValue) - (haircut * 0.01 * decimal.ToDouble(collateralValue)); } }

        public int customerId { get; set; }


        public string collateralDetail { get; set; }
       
        public decimal stapedToCoverAmount { get; set; }

        public decimal facilityAmount { get; set; }
    }


    public   class LoanApplicationCollateralRefNoViewModel
    {
        public int collateralRefNoId { get; set; }

        public int customerCollateralId { get; set; }
        
        public string documentNumber { get; set; }

        public decimal worth { get; set; }

        public bool isBankAccount { get; set; }

    }

    public class LoanApplicationDatailViewModel
    {
        public int loanApplicationId { get; set; }

        public int applicationDetailedId { get; set; }

        public int proposedRate { get; set; }

        public int proposedProductId { get; set; }

        public int proposedTenor { get; set; }

        public decimal proposedAmount { get; set; } 
    }

    public class LoanApplicationCommentViewModel 
    {
        public int applicationId { get; set; }

        public string comments { get; set; }

    }

}
