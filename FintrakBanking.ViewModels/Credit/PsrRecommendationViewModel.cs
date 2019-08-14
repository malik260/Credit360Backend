using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrRecommendationViewModel : GeneralEntity
    {
        public int psrRecommendationId { get; set; }

        public string comment { get; set; }
        public int projectSiteReportId { get; set; }
        public string projectRiskRating { get; set; }
        public string customerRating { get; set; }
    }
}