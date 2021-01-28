using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class TermSheetViewModel : GeneralEntity
    {
        public int termSheetId { get; set; }

        public string termSheetCode { get; set; }

        public int? customerId { get; set; }

        public string borrower { get; set; }

        public decimal facilityAmount { get; set; }

        public int facilityType { get; set; }

        public string purpose { get; set; }

        public int tenor { get; set; }

        public string permittedAccount { get; set; }

        public string debtServiceReserveAccount { get; set; }

        public string cancellation { get; set; }

        public string principalRepayment { get; set; }

        public string interestPayment { get; set; }

        public string computationOfInterest { get; set; }

        public string repaymentSource { get; set; }

        public string availability { get; set; }

        public int currencyOfDisbursement { get; set; }

        public string documentation { get; set; }

        public string drawdown { get; set; }

        public string earlyRepaymentOfPrincipal { get; set; }

        public decimal interestRate { get; set; }

        public string pricing { get; set; }

        public decimal managementFees { get; set; }

        public decimal facilityFee { get; set; }

        public decimal processingFee { get; set; }

        public string securityCondition { get; set; }

        public string transactionDynamics { get; set; }

        public string conditionsPrecedentToUtilisation { get; set; }

        public string otherCondition { get; set; }

        public string taxes { get; set; }

        public string presentationsAndWarrantees { get; set; }

        public string covenants { get; set; }

        public string eventsOfDefault { get; set; }

        public string transferability { get; set; }

        public string governingLawAndJurisdiction { get; set; }
        public bool owner { get; set; }
    }
}