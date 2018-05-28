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
        public bool isParent { get; set; }
        public int newTenor { get; set; }
        public string loanRef { get; set; }
    }

    public class InterestReviewViewModel : GeneralEntity
    {
        public double newRate { get; set; }
        public int aplicationDetailId { get; set; }
    }

    public class subAllocationViewModel : GeneralEntity
    {
        public decimal newPrincipalAmount { get; set; }
        public string loanReferenceNumber { get; set; }
    }

}
