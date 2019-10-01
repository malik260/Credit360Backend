using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanRecoveryPaymentViewModel
    {
        public short loanRecoveryPaymentId { get; set; }
        public int loanReviewOperationId { get; set; }
        public decimal paymentAmount { get; set; }
        public DateTime paymentDate { get; set; }
        public int approvalStatusId { get; set; }
        public int createdById { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public int? lastUpdatedBy { get; set; }
        public bool deleted { get; set; }
        public int deletedBy { get; set; }
        public DateTime? dateTimeDeleted { get; set; }
        public string customerName { get; set; }
        public string loanReferenceNumber { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime? maturityDateNew { get; set; }
        public decimal principalAmount { get; set; }
        public decimal? principalAmountNew { get; set; }
        public int loanId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int createdBy { get; set; }
        public int companyId { get; set; }
        public string userName { get; set; }
        public string passCode { get; set; }
        public int staffId { get; set; }
        public string comment { get; set; }
        public string applicationReferenceNumber { get; set; }
        public decimal totalAmountRecovered { get; set; }
        public string currencyCode { get; set; }
        public int operationId { get; set; }
    }


    public class LoanRecoveryPaymentViewModelNew
    {
        public string customerName { get; set; }
        public string loanReferenceNumber { get; set; }
        public DateTime effectiveDate { get; set; }
        public int loanReviewOperationId { get; set; }
        public DateTime maturityDate { get; set; }
        public decimal principalAmount { get; set; }
        public int loanId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}
