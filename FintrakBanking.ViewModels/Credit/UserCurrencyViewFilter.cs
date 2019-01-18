using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class UserCurrencyViewFilter
    {
        public int DefaultCurrencyId { get; set; }

        public bool CanSeeLocalCurrency { get; set; }

        public bool CanSeeForeignCurrency { get; set; }
    }
}
