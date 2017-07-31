using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class WorkflowTrackerViewModel
    {
        public string requestStaffName { get; set; }

        public string requestStaffCode { get; set; }
        public string requestApprovalLevel { get; set; }
        public DateTime arrivalDate { get; set; }
        public string approvalStatus { get; set; }
        public DateTime responseDate { get; set; }
        public string responseStaffName { get; set; }
        public string responseStaffCode { get; set; }
        public string responseApprovalLevel { get; set; }
        public int targetId { get; set; }
        public string comment { get; set; }
    }
}
