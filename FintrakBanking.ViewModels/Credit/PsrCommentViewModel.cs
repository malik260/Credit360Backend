using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrCommentViewModel : GeneralEntity
    {
        public int psrCommentId { get; set; }

        public string comment { get; set; }
        public int projectSiteReportId { get; set; }
    }

    public class PsrReportViewModel 
    {
        public string comment { get; set; }
        public string projectDetail { get; set; }
        public string facilityDetail { get; set; }
        public string performanceEvaluation { get; set; }
        public string observation { get; set; }
        public string recomendation { get; set; }
        public string taskForNextInspection { get; set; }
        public string apgExposure { get; set; }
    }
}