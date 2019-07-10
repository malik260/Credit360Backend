using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
     public class TotalFacilitiesSummaryViewModel
    {
        public int numberOfLoans { get; set; }
        public int numberOfOverdrafts { get; set; }
        public int numberOfContingents { get; set; }
        public int numberOfImportFinanceFacilities { get; set; }
        public int numberOfNewFacilities { get; set; }
        public string currency { get; set; }
        public decimal currencyExchangeRate { get; set; }
        public decimal totalLLLImpact { get; set; }
        public decimal totalCurrentAmount { get; set; }
        public decimal totalProposedAmount { get; set; }
        public decimal totalChange { get; set; }
        public int totalTenors { get; set; }
    }
}
