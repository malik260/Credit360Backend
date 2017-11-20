
using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class DocumentDeferralViewModel  : GeneralEntity
    {
        public int loanId  { get; set; }
        public string loanRefNo { get; set; }
        public int loanApplicationNumberId  { get; set; }
        public int checklistId { get; set; }
        public decimal principalOutstandingBalance  { get; set; }
        public decimal interestOutstandingBalance  { get; set; }
        public int productId { get; set; }
        public int casaAccountId { get; set; }

    }
}