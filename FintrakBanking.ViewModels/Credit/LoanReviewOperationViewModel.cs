using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class LoanReviewOperationViewModel : GeneralEntity
    {
        public int loanReviewOperationsId { get; set; }

        public int loanId { get; set; }

        public int loanTypeId { get; set; }

        public int operationTypeId { get; set; }

        public string reviewDetails { get; set; }

        public decimal? interateRate { get; set; }

        public decimal? prepayment { get; set; }

        public int? principalFrequencyTypeId { get; set; }

        public string interestFrequencyTypeId { get; set; }

        public DateTime? principalFirstPaymentDate { get; set; }

        public DateTime? interestFirstPaymentDate { get; set; }

        public int? tenor { get; set; }

        public int? cASA_AccountId { get; set; }

        public decimal? overDraftTopup { get; set; }

        public decimal? fee_Charges { get; set; }

        public string terminationAndReBook { get; set; }

        public string completeWriteOff { get; set; }

        public string cancelUndisbursedLoan { get; set; }

        public int approvalStatusId { get; set; }
    }
}
