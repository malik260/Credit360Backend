using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class PsrNextInspectionTaskViewModel : GeneralEntity
    {
        public DateTime? inspectionDate;

        public int psrNextInspectionTaskId { get; set; }

        public string comment { get; set; }

        public bool isDone { get; set; }
        public DateTime nextInspectionDate { get; set; }
        public int projectSiteReportId { get; set; }
    }
}