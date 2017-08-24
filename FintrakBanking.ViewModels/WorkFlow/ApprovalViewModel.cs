using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class ApprovalViewModel : UserInfo
    {
        public int operationId { get; set; }
        public int targetId { get; set; }
        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        public string operationURL { get; set; }
        public int myLevelId { get; set; }
        public int nextLevelId { get; set; }
        public decimal amount { get; set; }
        public bool isPoliticalyExposed { get; set; }
    }
}
