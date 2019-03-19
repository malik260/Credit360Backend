using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Common.Enum
{
    public enum DailyAccrualCategory
    {
        TermLoan = 1,
        AuthorisedOverdraft = 2,
        UnauthorisedOverdraft = 3,
        PastDuePrincipal = 4,
        //CreditCards = 5,
        PastDueInterest = 6,
        //CommercialLoan = 7,
        //FXRevolvingLoan = 8,
        Fee = 9,
        Tax = 10,
        WrittenOffTermLoanInterestAccural = 11,
    }
}
