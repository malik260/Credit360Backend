using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
   public class LoanCovenantDetailViewModel : GeneralEntity
    {
        public int loanCovenantDetailId { get; set; }
        public string covenantDetail { get; set; }
        public int loanId { get; set; }
        public short covenantTypeId { get; set; }
        public short? frequencyTypeId { get; set; }
        public decimal? covenantAmount { get; set; }
        public DateTime covenantDate { get; set; }
       
    }
   
}
