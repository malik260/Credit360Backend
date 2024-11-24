using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class TraderLoanForCreation
    {
        public decimal averageMonthlyTurnover { get; set; }
        public string marketLocations { get; set; }
        public string soldItems { get; set; }
        public int? marketLocationId { get; set; }
        public int? soldItemsId { get; set; }
    }
}
