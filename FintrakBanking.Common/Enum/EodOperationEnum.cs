using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum EodOperationEnum
    {
        ProcessAutomaticInterestRepricing = 3,
        ProcessReleaseLien = 1,
        ProcessDailyTermLoansInterestAccrual = 2,
        ProcessDailyInterestOnPastDueInterestAccrual = 4,
        ProcessDailyInterestOnPastDuePrincipalAccrual = 7,
        ProcessLoanRepaymentPostingForceDebit = 5,
        ProcessLoanRepaymentPostingPastDue = 6,
        ProcessAutomaticCommercialLoanRollover = 12,
        UpdateLoanApplicationCovenant = 8,
        UpdateLoanClassification = 9,
        DailyWrittenOffFacilityAccrual = 10,
        ProcessDailyFeeAccrual = 11,
    }

    //public enum EodOperationEnum
    //{
    //    ProcessAutomaticInterestRepricing = 1,
    //    ProcessReleaseLien = 2,
    //    ProcessDailyTermLoansInterestAccrual = 3,
    //    ProcessDailyInterestOnPastDueInterestAccrual = 4,
    //    ProcessDailyInterestOnPastDuePrincipalAccrual = 5,
    //    ProcessLoanRepaymentPostingForceDebit = 6,
    //    ProcessLoanRepaymentPostingPastDue = 7,
    //    ProcessAutomaticCommercialLoanRollover = 8,
    //    UpdateLoanApplicationCovenant = 9,
    //    UpdateLoanClassification = 10,
    //    DailyWrittenOffFacilityAccrual = 11,
    //    ProcessDailyFeeAccrual = 12,
    //}
}

