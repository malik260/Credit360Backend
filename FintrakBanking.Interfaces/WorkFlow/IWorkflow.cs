using System.Collections.Generic;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Entities.Models;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IWorkflow
    {
        int OperationId { set; }
        TBL_APPROVAL_TRAIL ApprovalTrail { get; set; }
        int? BusinessUnitId { get; set; }
        int? DestinationOperationId { get; set; }
        bool IsFlowTest { get; set; }
        bool IsClassifiedReferBack { get; set; }
        int? ExclusiveFlowChangeId { get; set; }
        int? LoopedStaffId { get; set; }
        int? LoopedRoleId { get; set; }
        int? ProductClassId { set; }
        int? ProductId { set; }
        int StaffId { set; }
        int? ToStaffId { set; }
        int TargetId { set; }
        int CompanyId { set; }
        int Tenor { set; }
        string Comment { set; }
        decimal Amount { set; }
        int StatusId { get; set; }
        int GroupStatusId { get; }
        int NewState { get; }
        int? NextLevelId { get; set; }
        int? FinalLevel { set; }
        bool EmailNotification { set; }
        short? Vote { set; }
        bool InvestmentGrade { set; }
        bool Untenored { set; }
        bool Disputed { set; }
        bool PoliticallyExposed { set; }
        bool SetResponse { set; }
        bool SmsNotification { set; }
        bool ExternalInitialization { set; }
        bool DeferredExecution { set; }
        bool StatusOnly { set; }
        bool KeepPending { set; }
        bool Saved { get; }
        float? InterestRateConcession { set; }
        float? FeeRateConcession { set; }
        bool? IsFromPc { set; }
        bool? TerminateOnApproval { set; }
        string Flow_log { set; }
        bool SkipLimitsCheck { set; }
        bool? IgnorePostApprovalReviewwer { set; }

        AlertPlaceholders Placeholders { set; }
        WorkflowResponse Response { get; set; }
        LevelBusinessRule LevelBusinessRule { set; }
        bool LogActivity();
        void NextProcess(
                int companyId,
                int staffId,
                int operationId,
                int? exclusiveFlowChangeId,
                int targetId,
                int? productClassId,
                string comment,
                bool external,
                bool deferred,
                bool sameDesk = false,
                bool isFlowTest = false,
                int? businessUnitId = null,
                int? finalLevel = null,
                int amount = 0
            );

        bool LogForApproval(ApprovalViewModel model); // <- this property is deprecated!!!
        void ResolveMultipleProductPath(int operationId, List<short> productIds);
    }

    public class WorkflowResponse
    {
        public WorkflowResponse () { success = false; }
        public int statusId { get; set; }
        public int stateId { get; set; }
        public string statusName { get; set; }
        public int? nextLevelId { get; set; }
        public string nextLevelName { get; set; }
        public int? nextPersonId { get; set; }
        public string nextPersonName { get; set; }

        public string nextOperationName { get; set; }
        public string responseMessage { get; set; }
        public bool success { get; set; }
        public int? fromLevelId { get; set; }
        public bool isFinal { get; set; }
        public int? businessUnitId { get; set; }
    }

    public class AlertPlaceholders
    {
        public string customerName { get; set; }
        public string referenceNumber { get; set; }
        public string operationName { get; set; }
        public string facilityType { get; set; }
        public string branchName { get; set; }
        public string locationName { get; set; }
    }

}