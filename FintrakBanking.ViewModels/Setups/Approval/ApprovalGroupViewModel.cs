using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Setups.Approval
{
    public class ApprovalGroupViewModel : GeneralEntity
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int roleId { get; set; }
        public bool branchFilter { get; set; }
    }
}
