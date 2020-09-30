using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanOperationViewModel
    {

    }
    public class LoanOperationTypeViewModel
    {
        public int operationTypeId { get; set; }
        public string operationTypeName { get; set; }
        public bool required { get; set; }
    }

    public class CreditDocumentationViewModel : GeneralEntity
    {
        public int loanId { get; set; }
        public string loanReviewApplicationId { get; set; }
        public string lmsrApplicationReferenceNumber { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string customerCode { get; set; }
        public int customerId { get; set; }
        public int requestId { get; set; }
        public string loanReferenceNumber { get; set; }
    }

    public class LoanBulkInterestReviewViewModel : GeneralEntity
    {
        public int bulkInterestRateReviewId { get; set; }

        public DateTime effectiveDate { get; set; }

        public short productPriceIndexId { get; set; }

        public string productPriceIndexName { get; set; }

        public double oldInterestRate { get; set; }

        public double newInterestRate { get; set; }

        public bool isProcessed { get; set; }

        public DateTime? processStartTime { get; set; }

        public DateTime? processEndTime { get; set; }
    }

    public class LoanReviewViewModel : GeneralEntity
    {
        public int loanReviewOperationsId { get; set; }
        public int loanId { get; set; }
        //public int?  loanReviewOperationId {get; set;}
        public int loanApplicationDetailId { get; set; }
        public int newTenor { get; set; }
        public double newRate { get; set; }
        public decimal newAmount { get; set; }
        public string loanReferenceNumber { get; set; }
        public DateTime valueDate { get; set; }
        public short approvalStatusId { get; set; }
        public short operationId { get; set; }

        public int targetId { get; set; }
        public IEnumerable<feeDetails> fees { get; set; }
        public string feeSourceModule { get; set; }
    }

    //public class LoanReviewViewModel : GeneralEntity
    //{
    //    public double newRate { get; set; }
    //    public int loanApplicationDetailId { get; set; }
    //    public int loanId { get; set; }
    //    public string loanReferenceNumber { get; set; }
    //    public DateTime valueDate { get; set; }
    //}

    public class subAllocationViewModel : GeneralEntity
    {
        public decimal newPrincipalAmount { get; set; }
        public string loanReferenceNumber { get; set; }
        public int toLoanId { get; set; }
        public decimal amountDifference { get; set; }
        public int fromLoanId { get; set; }

    }

    public class loanPrepaymentViewModel : GeneralEntity
    {
        public string saveStatus;
        public string userName;

        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public decimal amount { get; set; }
        public DateTime effectiveDate { get; set; }
        public bool isPrincipalReduction { get; set; }
        public bool isPreSubmission { get; set; }

        public decimal newPrincipal { get; set; }
        public decimal interestToDate { get; set; }
        public decimal InterestAtMaturity { get; set; }
        public decimal newMaturityAmount { get; set; }
    }
}
