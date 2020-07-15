using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class PsrPerformanceAnalysisViewModel : GeneralEntity
    {
        public string psrReportType;
        public short BranchId;
        public decimal? aTotal { get; set; }
        public decimal? bTotal { get; set; }
        public decimal? netPerformance { get; set; }

        public int psrAnalysisId { get; set; }

        public decimal? valueOfCollateral { get; set; }
        public decimal? ipc { get; set; }
        public decimal? pmu { get; set; }
        public decimal? amountDisbursed { get; set; }
        public decimal? amountRequested { get; set; }
        public int projectSiteReportId { get; set; }
        public bool deleted { get; set; }
        //public int createdBy { get; set; }
        public int deletedBy { get; set; }
        public DateTime? dateTimeDeleted { get; set; }
        public string currency { get; set; }
        public string whatToShow { get; set; }
    }
}
