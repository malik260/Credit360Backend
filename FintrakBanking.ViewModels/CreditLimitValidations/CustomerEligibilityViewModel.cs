using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.CreditLimitValidations
{
    public class CustomerEligibility
    {
        public bool eligible { get; set; }
        public string message { get; set; }
    }

    public class CustomerEligibilityViewModel
    {
        public string customerCode { get; set; }
        public DateTime dateBlackListed { get; set; }
        public string reason { get; set; }
        public string camsolType { get; set; }

    }
}
