using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DashboardViewModel
    {
        public string sectorName { get; set; }
        public int loanCount { get; set; }
        public decimal sumOfProposedAmount { get; set; }
    }
}
