using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum RequestStatusEnum
    {
        Pending = 1,
        Processing = 2,
        Approved = 3,
        Disapproved = 4,
        Cancel = 5
    }
}