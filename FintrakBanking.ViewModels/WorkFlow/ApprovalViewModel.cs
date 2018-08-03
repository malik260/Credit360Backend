using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{

    public class ApprovalViewModel : UserInfo
    {
        private bool _keepPending = true;

        public int operationId { get; set; }
        public int targetId { get; set; }
        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        public string operationURL { get; set; }
        public int myLevelId { get; set; }
        public int nextLevelId { get; set; }
        public decimal amount { get; set; }
        public bool isPoliticalyExposed { get; set; }
        public bool externalInitialization { get; set; }
        public bool keepPending {
            get { return _keepPending; }
            set {
                if (value == _keepPending) return;
                _keepPending = value;
            }
        }
        public bool deferredExecution { get; set; }
    }
}
