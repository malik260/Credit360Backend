using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.AlertMonitoring
{
    public enum AlertMessageEnum
    {
        LoanCovenantApproachingDueDates = 1,
        LoanCoveantOverdue = 2,
        CollateralApproachingRevaluation = 3,
        NonPerformingLoans = 4,
        SelfLiquidatingLoanExpiry = 5,
        OverdraftLoansAlmostDue = 6,
        InactiveBondAndGuarantee = 8,
        ExpiredActiveBondAndGuarantee = 9,
        OverdrawnAccount = 10,
        PastDueObligations = 11,
        CovenantsInsuranceApproachingDueDate = 12,
        ExpiredInsurance = 13,
        TurnoverCovenantNotMet = 14,
        CollateralDueForRevaluation = 15,
        WatchListedAccount = 16,
        AuathorizedAccount = 19,
        LoanRepayment = 17,
        CustomerAlertForLoanRepaymentApproachingDueDate = 18,
        ExpiredBGAlert = 20,
        TerminatedBG = 21,
       BGDesk = 22

    }
}
