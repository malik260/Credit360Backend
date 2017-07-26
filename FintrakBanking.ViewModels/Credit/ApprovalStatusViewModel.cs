using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class ApprovalStatusViewModel
    {
        public short approvalStatusId { get; set; }
        public string approvalStatusName { get; set; }
        public bool? forDisplay { get; set; }
    }
}
