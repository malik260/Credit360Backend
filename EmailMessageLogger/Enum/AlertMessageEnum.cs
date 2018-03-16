using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailMessageLogger.Enum
{
    public enum AlertMessageEnum
    {
        LoanCovenantApproachingDueDates = 1,
        LoanCoveantOverdue =2,
        CollateralRevaluation =3,
        NonPerformingLoans =4,
        SelfLiquidatingLoanExpiry =5,
        OverdraftLoansAlmostDue = 6,
        InsuranceForCollateralApproachingDueDates =7,
        CovenantsApproachingDueDate=8,
            CASAwithPND =9
    }
}
