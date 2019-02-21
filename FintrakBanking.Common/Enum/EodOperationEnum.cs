using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum EodOperationEnum
    {
        ProcessAutomaticInterestRepricing = 1,
        ProcessReleaseLien = 2,
        ProcessDailyTermLoansInterestAccrual = 3,
        ProcessDailyInterestOnPastDueInterestAccrual = 4,
        ProcessDailyInterestOnPastDuePrincipalAccrual = 5,
        ProcessLoanRepaymentPostingForceDebit = 6,
        ProcessLoanRepaymentPostingPastDue = 7,
        ProcessAutomaticCommercialLoanRollover = 8,
        UpdateLoanApplicationCovenant = 9,
        UpdateLoanClassification = 10
    }
}

