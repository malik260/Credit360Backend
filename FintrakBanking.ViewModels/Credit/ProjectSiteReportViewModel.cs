using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class ProjectSiteReportViewModel : GeneralEntity
    {
        public string psrRepeortType { get; set; }

        public int projectSiteReportId { get; set; }

        public int psrReportTypeId { get; set; }

        public string clientName { get; set; }

        public string contractorName { get; set; }

        public string consultantName { get; set; }

        public decimal projectAmount { get; set; }

        public string projectDescription { get; set; }

        public DateTime commencementDate { get; set; }
        public DateTime? inspectionDate { get; set; }

        public DateTime completionDate { get; set; }

       // public DateTime nextVisitationDate { get; set; }
        public int loanApplicationId { get; set; }
        public int approvalStatusId { get; set; }
        public int approvalTrailId { get; set; }
        public string projectLocation { get; set; }
        public string comment { get; set; }
        public string approvalStatusName { get; set; }
        public string appplicationReferenceNumber { get; set; }
        public string customerName { get; set; }
        public int operationId { get; set; }
        public int? currencyId { get; set; }
        public bool acceptance { get; set; }
        public string currency { get; set; }
        
       public IEnumerable<LoanApplicationViewModel> loanApplicationViewModel { get; set; }
        public int? loopedStaffId { get; set; }
        public string projectOffer { get; set; }
        public string groupHead { get; set; }
        public DateTime nextVisitationDate { get; set; }
        public string superComment { get; set; }
        public int? loanApplicationDetailId { get; set; }
    }
    
}