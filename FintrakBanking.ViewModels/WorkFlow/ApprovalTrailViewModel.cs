using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class ApprovalTrailViewModel : GeneralEntity
    {
        public int approvalTrailId { get; set; }
        public int targetId { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime systemArrivalDateTime { get; set; }
        public DateTime? responseDate { get; set; }
        public DateTime? systemResponseDateTime { get; set; }
        public int? responseStaffId { get; set; }
        public int requestStaffId { get; set; }
        public int? fromApprovalLevelId { get; set; }
        public int? toApprovalLevelId { get; set; }
        public short approvalStateId { get; set; }
        public short approvalStatusId { get; set; }
        public int operationId { get; set; }
        public string comment { get; set; }
        public string staffName { get; set; }
    }
    public class ApprovalTrailDetailsViewModel : ApprovalTrailViewModel
    {
        public string targetName { get; set; }
        public string operationName { get; set; }
        public string approvalStatusName { get; set; }
        public string approvalLevelName { get; set; }
    }
}
