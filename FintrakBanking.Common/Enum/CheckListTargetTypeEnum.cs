using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{

    public enum CheckListTargetTypeEnum
    {
        LoanApplicationProductChecklist = 1,
        CASA = 2,
        LoanApplicationCustomerChecklist = 3
    }

    public enum CheckTypeEnum
    {
        EligibilityChecklist = 1,
        RegulatoryChecklist = 2,
        ESGMChecklist = 3,
        PreLendingCallGrid = 4
    }
}
