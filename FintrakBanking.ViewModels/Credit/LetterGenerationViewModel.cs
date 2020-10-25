using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LetterGenerationRequestViewModel : GeneralEntity
    {
        public int requestId { get; set; }

        public string requestRef { get; set; }

        public int customerId { get; set; }

        public string customerName { get; set; }

        public string comment { get; set; }

        public DateTime requestDate {get; set;}

        public DateTime asAtDate {get; set;}

        //public DateTime applicationDate { get; set; }

        public int requestType { get; set; }
        public int requestTypeName { get; set; }

        public short? approvalStatusId { get; set; }

        public string approvalStatus { get; set; }

        public int? currentApprovalStateId { get; set; }

        public int? currentApprovalLevelId { get; set; }

        public string currentApprovalLevel { get; set; }

        public string lastComment { get; set; }

        public int approvalTrailId { get; set; }

        public int? toStaffId { get; set; }

        public string responsiblePerson { get; set; }

        public int? currentApprovalLevelTypeId { get; set; }

        public int? applicationStatusId { get; set; }

        public short lcIssuanceStatusId { get; set; }

        public string applicationStatus { get; set; }

        public int? operationId { get; set; }

        public int? forwardAction { get; set; }

        public short? vote { get; set; }
        public string accountNumber { get; set; }
        public string customerCode { get; set; }

        public decimal? loanBalance { get; set; }
        public DateTime arrivalDate { get; set; }
        public int? loopedStaffId { get; set; }

        public List<AuthorisedSignatoryViewModel> letterGenerationsignatories { get; set; }
        public List<CamsolLoanDocumentViewModel> letterGenerationCamsolList { get; set; }
    }
}