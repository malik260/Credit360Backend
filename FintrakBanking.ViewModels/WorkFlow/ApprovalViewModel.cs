using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{

    public class ApprovalViewModel : UserInfo
    {
        public int? businessUnitId { get; set; }
        public int? destinationOperationId { get; set; }
        private bool _keepPending = true;
        public int? toStaffId { get; set; }
        public int[] applicationId { get; set; }
        public bool isFlowTest { get; set; }
        public bool isFromPc { get; set; }
        public int? approvalLevelId { get; set; }
        public int operationId { get; set; }
        public int trailId { get; set; }
        public int? nextOperation { get; set; }
        public bool isClassified { get; set; }
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
        public bool isLmsOperations { get; set; }
        public bool isForceApproval { get; set; }
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

        public bool forbidExternalNotification { get; set; }
        public string coreBankingRef { get; set; }

        public int loanChargeFeeId { get; set; }
        public decimal feeAmount { get; set; }
        public double feeRate { get; set; }
        public bool? documentProvided { get; set; }
        public int[] requestId { get; set; }
        public string currentUserCode { get; set; }
        public short sourceOperationId { get; set; }
        public short sourceTargetId { get; set; }
    }
    public class ApprovalResponse
    {
        public int status { get; set; }
        public string approvalLevel { get; set; }

    }

    public class BulkRecoveryApprovalViewModel : UserInfo
    {
        public int? businessUnitId { get; set; }
        public int? destinationOperationId { get; set; }
        private bool _keepPending = true;
        public int? toStaffId { get; set; }
        public int[] applicationId { get; set; }
        public bool isFlowTest { get; set; }
        public bool isFromPc { get; set; }
        public int? approvalLevelId { get; set; }
        public int operationId { get; set; }
        public int? nextOperation { get; set; }
        public bool isClassified { get; set; }
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
        public bool isForceApproval { get; set; }
        public bool keepPending
        {
            get { return _keepPending; }
            set
            {
                if (value == _keepPending) return;
                _keepPending = value;
            }
        }
        public bool deferredExecution { get; set; }
        public string rollOverType { get; set; }


        public int approvalStatusIdUI { get; set; }

        public bool forbidExternalNotification { get; set; }
        public string coreBankingRef { get; set; }

        public int loanChargeFeeId { get; set; }
        public decimal feeAmount { get; set; }
        public double feeRate { get; set; }
        public bool? documentProvided { get; set; }
        public int[] requestId { get; set; }
        public int bulkRecoveryApprovalId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string referenceId { get; set; }
    }
}
