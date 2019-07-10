using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Media
{
    public class OriginalDocumentApprovalViewModel : GeneralEntity
    {
        public int originalDocumentApprovalId { get; set; }

        public int loanApplicationId { get; set; }

        public string description { get; set; }

        public int approvalStatusId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string referenceNumber { get; set; }
        public string approvalStatusName { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public int customerId { get; set; }
        public string branchName { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal applicationAmount { get; set; }
        public double interestRate { get; set; }
        public string productName { get; set; }
        public object relationshipOfficerName { get; set; }
        public object relationshipManagerName { get; set; }
        public int operationId { get; set; }
        public string comment { get; set; }
        public DateTime? approvalDate { get; set; }
        public bool atInitiator { get; set; }
        public string createdByName { get; set; }
    }
}