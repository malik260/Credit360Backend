using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum AdjustedFacilityTypeEnum
    {
        MORTGAGELOAN = 1,
        OVERDRAFT = 2,
        TERMLOAN = 3,
        CREDITCARD = 4,
        PERSONALLOAN = 5,
        OTHER = 6,
        ONLENDING = 7,
        PROJECTFINANCELOAN = 8,
        TERMLOANN = 9,
        BONDGTEE = 10,
        LC = 11,
        GUARANTEE = 12,
        STAFFLOAN = 13,
        //ONLENDING = 15
        NA = 0,
        OTHERSN = 17,
        ADVANCESUNDERLEASEN = 18,
        TIMELOAN = 19,
        TRADELOAN = 20,
        ONLENDINGCBN = 21,
        USANCE = 22,
        FINANCELEASE = 23
    }

    public enum ExposureTypeEnum
    {
        Direct = 10,
        Contingent = 11
    }

}
