using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Common.Enum
{
    public enum OperationsEnum
    {
        //setup operations
        StaffCreation = 3,
        UserCreation = 4,
        ProductCreation = 5,
        ChartOfAccountCreation = 7,
        CustomerGroupCreation = 8,
        FeeCreation = 10,

        //loan origination operations
        TermLoanBooking = 1,
        LoanApplication = 2,
        CAM = 6,
        LoanPreliminaryEvaluation = 9,        
        CollateralSearch = 12,
        RevolvingLoanBooking = 13,
        ContigentLoanBooking = 14,
        DailyInterestAccural = 15,

        LoanRepayment = 16,
        //ChargeReversal = 17,
        //LoanPrepayment  = 18,

        //Loan management operations
        ContractualInterestRateChange = 19,
        OverdraftSubAllocation = 20,
        Prepayment = 21,
        PrincipalFrequencyChange = 22,
        InterestFrequencyChange = 23,
        InterestandPrincipalFrequencyChange = 24,
        PaymentDateChange= 25,
        TenorChange = 26,
        CASAAccountChange = 27,
        OverdraftTopup = 28,
        Fee_chargeChange = 29,
        TerminateAndRebook = 30,
        CompleteWriteOff = 31,
        CancelUndisbursedLoan = 32,
        InterestSuspension = 33,
        CollateralRelease = 35
    }
}
