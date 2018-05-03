using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum FeeConcessionTypeEnum
    {
        Interest = 1,
        Fee = 2
    }
    public enum FeeTypeEnum
    {
        Rate = 1,
        Amount = 2,
        RangeOfAmounts = 3,
        FixedbyAmount = 4,
        RatebyAmount = 5
    }
}
