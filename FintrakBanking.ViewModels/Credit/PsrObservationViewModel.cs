using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrObservationViewModel : GeneralEntity
    {
        public int psrObservationId { get; set; }

        public string comment { get; set; }
        public int projectSiteReportId { get; set; }
    }
}