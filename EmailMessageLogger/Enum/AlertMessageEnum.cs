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
        LoanCoveantOverdue = 2,
        CollateralRevaluation = 3,
        NonPerformingLoans = 4,
        SelfLiquidatingLoanExpiry = 5,
        OverdraftLoansAlmostDue = 6,
        InsuranceForCollateralApproachingDueDates = 7,
        CovenantsApproachingDueDate = 8,
        CASAwithPND = 9,
        BondAndGuarantee = 10,
        InactiveBondAndGuarantee = 11,
        ExpiredActiveBondAndGuarantee = 12,
        AccountWithExeption = 13,
        AccountsThatPastDueObligation = 14,
        InsuranceApprochingExpiration = 15,
        ExpiredInsurance = 16,
        TurnoverCovenant = 17
    }
}
