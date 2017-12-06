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
    }


    public   class LoanApplicationCollateralRefNoViewModel
    {
        public int collateralRefNoId { get; set; }

        public int customerCollateralId { get; set; }
        
        public string documentNumber { get; set; }

        public decimal worth { get; set; }

        public bool isBankAccount { get; set; }

    }

}
