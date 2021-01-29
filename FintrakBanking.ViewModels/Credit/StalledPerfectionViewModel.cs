using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public  class StalledPerfectionViewModel
    {
        public int loanId { get; set; }

       // public short loanSystemTypeId { get; set; }

        public string loanRefno { get; set; } 
        public string customerName { get; set; }
        public decimal outstandingInterest { get; set; }
        public decimal outstandingBalance { get; set; }
        public string reasonsforStalledPerfection { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string collateralCode { get; set; }
        public DateTime captureDate { get; set; }
        public short colaterallSubType { get; set; }
        public string collateralSubType { get; set; }
        public string collateralType { get; set; }
        public DateTime perfectionDate { get; set; }
    }
}
