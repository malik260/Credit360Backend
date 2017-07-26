using FintrakBanking.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public class ApprovalLevelStaffViewModel : GenaralEntity
    {
        public int staffLevelId { get; set; }
        public int groupId { get; set; }
        public int position { get; set; }
        public int operationId { get; set; }
        public int staffId { get; set; }
        public int approvalLevelId { get; set; }
        public decimal maximumAmount { get; set; }
        public decimal minimumAmount { get; set; }
        public string staffLevelName {get; set; }
        public string approvalLevelName{ get; set; }

    }
}
