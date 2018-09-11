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

    public class TenorExtionViewModel : GeneralEntity
    {
        public string appRef { get; set; }
        public int id { get; set; }
        public int loanId { get; set; }
        public int loanAplicationDetailId { get; set; }
        public bool isParent { get; set; }
        public int newTenor { get; set; }
        public string loanRef { get; set; }
    }

    public class InterestReviewViewModel : GeneralEntity
    {
        public double newRate { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
        public DateTime valueDate { get; set; }
    }

    public class subAllocationViewModel : GeneralEntity
    {
        public decimal newPrincipalAmount { get; set; }
        public string loanReferenceNumber { get; set; }
    }

    public class loanPrepaymentViewModel : GeneralEntity
    {
        public string saveStatus;

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
