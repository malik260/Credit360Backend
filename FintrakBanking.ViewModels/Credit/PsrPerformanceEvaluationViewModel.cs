using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrPerformanceEvaluationViewModel : GeneralEntity
    {
        public int psrPerformanceEvaluationId { get; set; }

        public string projectSum { get; set; }
        public string paymentToDate { get; set; }
        public string disbursedTodate { get; set; }
        public string initialProjectSum { get; set; }
        public string vowdToDate { get; set; }
        public string pmuAssessed { get; set; }
        public string consoltantVowd { get; set; }
        public string amortisedApg { get; set; }
        public string apgReceived { get; set; }
        public string costVariation { get; set; }
        public string timeVariation { get; set; }
        public string apgIssued { get; set; }
        public string amountReceived { get; set; }
        public int projectSiteReportId { get; set; }
        public int psrReportTypeId { get; set; }
        public string projectSiteReportName { get; set; }
        public string psrReportType { get; set; }
        public string progressPayment { get; set; }
        public string certifiedVowd { get; set; }
        public short BranchId { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
    }
}