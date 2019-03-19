using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
   public enum CollateralReleaseStatus
    {
      	InVault = 1,
        ReleasedToBM = 2,
        ReleasedToCustomer = 3,
                    ReleasedToLegal = 4

    }
    public enum CollateralReleaseType
    {
        FinalRelease = 1,
        TemporaryRelease = 2,
    }
}
