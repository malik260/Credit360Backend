using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.CASA
{
    public class CasaViewModel : GenaralEntity
    {
        public int casaAccountId { get; set; }
        public string productAccountNumber { get; set; }
        public string productAccountName { get; set; }
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public int productId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        //public int companyId { get; set; }
        public short branchId { get; set; }
        public string branchCode { get; set; }
        public string branchName { get; set; }
        public bool isCurrentAccount { get; set; }
        public int tenor { get; set; }
        public decimal interestRate { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime terminalDate { get; set; }
        public int actionBy { get; set; }
        public DateTime actionDate { get; set; }
        public short accountStatusId { get; set; }
        public int operationId { get; set; }
        public decimal availableBalance { get; set; }
        public decimal ledgerBalance { get; set; }
        public int relationshipManagerId { get; set; }
        public int relationshipOfficerId { get; set; }
        public string misCode { get; set; }
        public string teamMiscode { get; set; }
        public decimal overdraftAmount { get; set; }
        public decimal overdraftInterestRate { get; set; }
        public DateTime overdraftExpiryDate { get; set; }
        public bool ?  hasOverdraft { get; set; }
        public decimal lienAmount { get; set; }
        public bool hasLien { get; set; }
        public short postNoStatusId { get; set; }
        public string oldProductAccountNumber1 { get; set; }
        public string oldProductAccountNumber2 { get; set; }
        public string oldProductAccountNumber3 { get; set; }
        public string refreshBatchId { get; set; }
        public DateTime ? lastRefreshDatetime { get; set; }
        public short ? aprovalStatusId { get; set; }
        //public int createdBy { get; set; }
        //public int? lastUpdatedBy { get; set; }
        //public DateTime? dateTimeCreated { get; set; }
        //public DateTime? dateTimeUpdated { get; set; }
        //public bool deleted { get; set; }
        //public int deletedBy { get; set; }
        //public DateTime dateTimeDeleted { get; set; }
    }
}
