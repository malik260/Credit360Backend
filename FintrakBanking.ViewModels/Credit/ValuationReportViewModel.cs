using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class ValuationReportViewModel : GeneralEntity
    {
        public int valuationReportId { get; set; }

        public int collateralValuationId { get; set; }

        public int valuerId { get; set; }

        public string valuerComment { get; set; }

        public string accountNumber { get; set; }

        public decimal? valuationFee { get; set; }

        public decimal? WHT { get; set; }
        
        public int approvalStatusId { get; set; }
    }
}
