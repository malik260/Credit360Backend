using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{

    public class ApprovalViewModel : UserInfo
    {
        public int? destinationOperationId { get; set; }
        private bool _keepPending = true;
        public int? toStaffId;
        public object[] applicationId;

        public int? approvalLevelId { get; set; }
        public int operationId { get; set; }
        public int? productId { get; set; }
        public int? productClassId { get; set; }
        public int? exclusiveFlowChangeId { get; set; }
        public int? loopedStaffId { get; set; }
        public int? loopedRoleId { get; set; }
        public int targetId { get; set; }
        public int? loanApplicationId { get; set; }
        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        public string operationURL { get; set; }
        public int myLevelId { get; set; }
        public int nextLevelId { get; set; }
        public decimal amount { get; set; }
        public bool isPoliticalyExposed { get; set; }
        public bool externalInitialization { get; set; }
        public bool isLms { get; set; }
        public bool keepPending {
            get { return _keepPending; }
            set {
                if (value == _keepPending) return;
                _keepPending = value;
            }
        }
        public bool deferredExecution { get; set; }
        public string rollOverType { get; set; }


        public int approvalStatusIdUI { get; set; }
        

    }
    public class ApprovalResponse
    {
        public int status { get; set; }
        public string approvalLevel { get; set; }

    }
}
