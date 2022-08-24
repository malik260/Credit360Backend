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
        decimal FacilityAmount { set; }
        int StatusId { get; set; }
        int GroupStatusId { get; }
        int NewState { get; }
        int? NextLevelId { get; set; }
        bool IgnorePostApprovalReviewer { get; set; }
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
        //int? CreditGradeId { set; }
        //bool? IgnorePostApprovalReviewwer { set; }o
        List<WorkflowSetup> WorkflowSetup { get; }
        int? OwnerId { set; }


        AlertPlaceholders Placeholders { set; }
        WorkflowResponse Response { get; set; }
        LevelBusinessRule LevelBusinessRule { set; }
        bool LogActivity();
        IEnumerable<dynamic> GetWorkFlowSetupLevelIds();
        //worked on by ifeanyi and zino on 23/06/2021 for account officer offer letter (productId was added)
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
                int amount = 0,
                int? productId = null
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

    public class WorkflowSetup
    {
        public int Sn { get; set; }
        public int SlaInterval { get; set; }
        public int GroupPosition { get; set; }
        public int LevelPosition { get; set; }
        public int ApprovalLevelId { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfApprovals { get; set; }
        public bool CanRouteBack { get; set; }
        public bool IsPoliticallyExposed { get; set; }
        //public bool IsInsiderRelated { get; set; }
        public bool IsActive { get; set; }
        public bool CanEdit { get; set; }
        public bool CanRecieveEmail { get; set; }
        public bool CanRecieveSMS { get; set; }
        public bool RouteViaStaffOrganogram { get; set; }
        public int? Tenor { get; set; }
        public decimal MaximumAmount { get; set; }
        public decimal? InvestmentGradeAmount { get; set; }
        public int? DefaultRoleId { get; set; }
        public int? LevelTypeId { get; set; }
        public int? LevelBusinessRuleId { get; set; }
        public TBL_APPROVAL_LEVEL Level { get; set; }
        public TBL_APPROVAL_GROUP Group { get; set; }
        public TBL_APPROVAL_GROUP_MAPPING Mapping { get; set; }
        public IEnumerable<TBL_APPROVAL_LEVEL_STAFF> Staff { get; set; }
        public TBL_APPROVAL_BUSINESS_RULE LevelBusinessRule { get; set; }
        public bool AllowMultipleInitiator { get; set; }
        public int? RoleIdToRoute { get; set; }
        public int? ROLEIDTOROUTE { get; set; }
        public bool IsPostApprovalReviewer { get; set; }
        public decimal? InvestmentGradeLimit { get; set; }
        public decimal? StandardGradeLimit { get; set; }
        public decimal? RenewalLimit { get; set; }
        public bool IsSyndicated { get; set; }
        public bool IgnoreWhenLevelIsApprovalLevelStaff { get; set; }
    }

}