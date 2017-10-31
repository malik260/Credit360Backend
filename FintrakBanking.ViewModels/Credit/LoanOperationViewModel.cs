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
}
