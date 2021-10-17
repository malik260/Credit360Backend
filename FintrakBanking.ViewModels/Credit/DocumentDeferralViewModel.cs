
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

    public class PendingDocumentDeferralViewModel
    {
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public string facilityType { get; set; }
        public decimal facilityAmount { get; set; }
        public int reference { get; set; }
        public string accountOfficer { get; set; }
        public string relationshipManager { get; set; }
        public string groupHead { get; set; }
        public string sbu { get; set; }
        public string condition { get; set; }
        public int numberOfDays { get; set; }
        public DateTime? deferredDate { get; set; }
        public int staffId { get; set; }
        public string accountOfficerName { get; set; }
        public string rmName { get; set; }
    }
}