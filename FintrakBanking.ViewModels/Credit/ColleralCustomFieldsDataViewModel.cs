using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
   public class ColleralCustomerViewModel: GeneralEntity
    {
        public int colleralCustomerId { get; set; }
        public int collateralTypeId { get; set; }
        public string documentNo { get; set; }
        public int? cityId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public string productName { get; set; }
        public decimal collateralValue { get; set; }
        public DateTime collateralValueDate { get; set; }
        public int? quantity { get; set; }
        public bool releaseCollateral { get; set; }
        public DateTime? releaseDate { get; set; }
        public int customerId { get; set; }
    }

    public class CustomerControllerRequestViewModel
    {
        public int collateralTypeId { get; set; }
        public int customerId { get; set; }
        //public int LoanId { get; set; }
    }
}
