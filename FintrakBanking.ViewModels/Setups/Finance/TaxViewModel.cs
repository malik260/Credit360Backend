using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Finance
{
    public class TaxViewModel : GeneralEntity
    {
        public int taxId { get; set; }
        public string taxName { get; set; }
        public decimal? amount { get; set; }
        public double? rate { get; set; }
        public int gLAccountId { get; set; }
        public bool? useAmount { get; set; }
    }
}
