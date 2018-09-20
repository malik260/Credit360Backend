using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum LoanRepricingModeEnum
    {
        FloatingToMaturity = 1,
        FixedToMaturity = 2,
        FixedToMaturityWithRepricing = 3
    }
}
