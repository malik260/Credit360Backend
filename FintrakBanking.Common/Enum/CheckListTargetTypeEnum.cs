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

    public enum CheckListTypeEnum
    {
        EligibilityChecklist = 1,
        RegulatoryChecklist = 2,
        ESGMChecklist = 3,
        PreLendingCallGrid = 4,
        CAPChecklist = 5,
        AvailmentCheckList = 6,
        OfferLetterChecklist = 7,
        BlackBookCheck = 8,
        WriteOffCheck = 9,
        GreenRating = 10
    }

    public enum ESGScoreGradeEnum
    {
        A = 1,
        B = 2,
        C = 3
    }
}
