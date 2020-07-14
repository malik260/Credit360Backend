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

        public decimal? projectSum { get; set; }
        public decimal? paymentToDate { get; set; }
        public decimal? disbursedTodate { get; set; }
        public decimal? initialProjectSum { get; set; }
        public decimal? vowdToDate { get; set; }
        public decimal? pmuAssessed { get; set; }
        public decimal? consoltantVowd { get; set; }
        public decimal? amortisedApg { get; set; }
        public decimal? apgReceived { get; set; }
        public decimal? costVariation { get; set; }
        public string timeVariation { get; set; }
        public decimal? apgIssued { get; set; }
        public decimal? amountReceived { get; set; }
        public int projectSiteReportId { get; set; }
        public int psrReportTypeId { get; set; }
        public string projectSiteReportName { get; set; }
        public string psrReportType { get; set; }
        public decimal? progressPayment { get; set; }
        public decimal? certifiedVowd { get; set; }
        public short BranchId { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public decimal? amountDisbursedPercent { get; set; }
        public decimal? pmuPercentage { get; set; }
        public string currency { get; set; }
        public decimal? percentageOne { get; set; }
        public decimal? percentageTwo { get; set; }
        public decimal? percentageThree { get; set; }
        public decimal? percentageFour { get; set; }
        public decimal? percentageFive { get; set; }
    }
}