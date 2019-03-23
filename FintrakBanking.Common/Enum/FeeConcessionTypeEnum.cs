using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum FeeConcessionTypeEnum
    {
        Interest = 1,
        Fee = 2
    }
    public enum FeeTypeEnum
    {
        Rate = 1,
        Amount = 2,
        RangeOfAmounts = 3,
        FixedbyAmount = 4,
        RatebyAmount = 5
    }

    public enum ChargeFeeDetailTypeEnum
    {
        Customer = 1,
        Primary = 2,
        Tax = 3,
        Others = 4
    }

    public enum ChargeFeeTargetEnum
    {
        Turnover = 1,
        TransactionDebitAndCredit = 2,
        Balance = 3,
        Credit = 6,
        Debit  = 5,
        OutstandingPrincipal = 7,
        Principal = 4,
        ApprovedLoanAmount = 8
    }
}
