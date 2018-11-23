using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum JobTypeEnum
    {
        legal = 1,
        middleOfficeVerification  = 2,
        camsolCheck = 4,
        blackBookCheck   = 5,
        others = 6
    }

   

    public enum JobRequestStatusEnum
    {
        approved = 3,
        cancel = 5,
        disapproved = 4,
        pending = 1,
        processing = 2

    }


}
