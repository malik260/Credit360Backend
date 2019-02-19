using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CashCollaterizedCredits
    {
        public string productaccountnumber { get; set; }
        public decimal availablebalance { get; set; }
        public decimal lienamount { get; set; }
        public string loanaccountnumber { get; set; }
        public decimal cashBalance { get; set; }
        public string lien { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string loanOverdraftAccount { get; set; }
        public decimal loanOverdraftAccountBalance { get; set; }
        public string isLien { get; set; }
        public bool hasLien { get; set; }
        public string overdraftAccount { get; set; }
        
    }
}





