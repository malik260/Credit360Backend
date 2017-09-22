using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class OfferLetterViewModel: GeneralEntity
    {
        public decimal? loanAmount { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public int tenor { get; set; }
        public DateTime maturityDate { get { return this.applicationDate.AddDays(this.tenor);  } }
        public double interestRate { get; set; }
        public string customerAddress { get; set; }

        public DateTime applicationDate { get; set; }
    }
}
