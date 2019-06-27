using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class ProjectSiteReportViewModel : GeneralEntity
    {
        public int projectSiteReportId { get; set; }

        public int psrReportTypeId { get; set; }

        public string clientName { get; set; }

        public string contractorName { get; set; }

        public string consultantName { get; set; }

        public decimal projectAmount { get; set; }

        public string projectDescription { get; set; }

        public DateTime commencementDate { get; set; }

        public DateTime completionDate { get; set; }

        public DateTime nextVisitationDate { get; set; }
        public int loanApplicationId { get; set; }
        public int approvalStatusId { get; set; }
        public string projectLocation { get; set; }
    }
}