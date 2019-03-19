using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Finance
{
  public  class IndexRateChangeViewModel
    {
        public string product { get; set; }
        public string currency { get; set; }
        public decimal offerRate { get; set; }
        public decimal bidRate { get; set; }
        public DateTime? date { get; set; }
    }
}
