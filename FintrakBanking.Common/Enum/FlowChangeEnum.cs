using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum FlowChangeEnum
    {
        CASHBACKED = 1,
        CLEANCARD = 2,
        SALARYBACKED = 3,
        TEMPORARYOVERDRAFT = 4,
        FAM = 5,
        CASHCOLLATERIZED = 10
    }

    public enum CreditCardEnum
    {
        CASHBACKED  =1,
        CLEANCARD = 2,
        SALARYBACKED = 3
    }

    public enum ApprovalFlowTypeEnum
    {
        ROUNDROBIN = 1,
        SBUROUTING = 2,
        POOL = 3
    }



}


