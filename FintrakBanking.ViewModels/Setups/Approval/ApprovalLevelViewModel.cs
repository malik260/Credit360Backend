using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Setups.Approval
{
    public class ApprovalLevelViewModel : GeneralEntity
    {
        public int? roleIdToRoute { get; set; }

        public int approvalLevelId { get; set; }
        public int tempApprovalLevelId { get; set; }
        public string levelName { get; set; }
        public int position { get; set; }
        public int? tenor { get; set; }
        public short? tenorModeId { get; set; }
        public decimal maximumAmount { get; set; }
        public decimal? investmentGradeAmount { get; set; }
        public decimal? standardGradeAmount { get; set; }
        public decimal? renewalLimit { get; set; }
    
        public int numberOfUsers { get; set; }
        public int numberOfApprovals { get; set; }
        public int slaInterval { get; set; }
        public bool canRouteBack { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public bool isActive { get; set; }
        //public bool canDoRiskAssessment { get; set; }
        //public bool canRecieveAdjustment { get; set; }
        public bool canRecieveEmail { get; set; }
        public bool canRecieveSms { get; set; }
        //public bool hasChecklist { get; set; }
        //public bool canPerformFinancialAnalysis { get; set; }
        //public bool requireAuthorisation { get; set; }
        //public bool canOverideAuthorisation { get; set; }
        public bool routeViaStaffOrganogram { get; set; }
        public int groupId { get; set; }
        public int? roleId { get; set; }

        public bool canViewDocument { get; set; }
        public bool canEdit { get; set; }
        public bool canViewUploadedFile { get; set; }
        public bool canUploadFile { get; set; }
        public bool canViewApproval { get; set; }
        public bool canApprove { get; set; }

        public int operationId { get; set; }
        public bool canResolveDispute { get; set; }
        public bool canApproveUntenored { get; set; }
        public bool canEscalate { get; set; }
        public double? feeRate { get; set; }
        public double? interestRate { get; set; }
        public int slaNotificationInterval { get; set; }
        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        public string canEscalateValue { get; set; }
        public string canResolveDisputeValue { get; set; }
        public string canApproveUntenoredValue { get; set; }
        public string isActiveValue { get; set; }
        public string canViewDocumentValue { get; set; }
        public string canEditValue { get; set; }
        public string canViewUploadedFileValue { get; set; }
        public string canUploadFileValue { get; set; }
        public string canViewApprovalValue { get; set; }
        public string canApproveValue { get; set; }
        public string canRecieveSmsValue { get; set; }
        public string canRecieveEmailValue { get; set; }
        public string groupName { get; set; }
        public string isPoliticallyExposedValue { get; set; }
        public string operation { get; set; }
        public int? levelTypeId { get; set; }
        public string roleName { get; set; }
        public int levelPosition { get; set; }
        public int groupPosition { get; set; }

        public string approvalLevelAndRoleName
        {
            get
            {
                if (String.IsNullOrEmpty(roleName))
                    return levelName;
                else
                    return levelName + "(" + roleName + ")";
            }
        }

        public int? levelBusinessRuleId { get; set; }
        public bool isPostApprovalReviewer { get; set; }
        public bool ignoreIfApprovalLevelStaff { get; set; }
    }

    public class PresetRouteViewModel : GeneralEntity
    {
        public int moduleId { get; set; }
        public int applicationId { get; set; }
        public int finalApprovalLevelId { get; set; }
        public int nextApplicationStatusId { get; set; }

        public List<FintrakDropDownSelectList> approvalLevels { get; set; }
        public List<FintrakDropDownSelectList> applicationStatus { get; set; }
    }

    public class FintrakDropDownSelectList
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class WorkflowNotificationViewModel : GeneralEntity
    {
        public int workflowNotificationId { get; set; }
        public int groupOperationMappingId { get; set; }
        public int approvalLevelId { get; set; }
        public int? proceedingActionsAlertTitleId { get; set; }
        public int? poolAlertTitleId { get; set; }
        public int? ownerAlertTitleId { get; set; }
        public int? pendingApprovalAlertTitleId { get; set; }
        public bool includePoolInNotification { get; set; }
        public bool notifyOfProceedingWorkflowActions { get; set; }
        public bool notifyOfPendingApprovals { get; set; }
        public bool notifyOnwer { get; set; }
    }

    public class DynamicWorkflowViewModel : GeneralEntity
    {
        public int? idValue { get; set; }
        public bool? booleanValue { get; set; }
        public string textValue { get; set; }

        public string expression { get; set; }
        public int workflowId { get; set; }
        public int contextId { get; set; }
        public int dataItemId { get; set; }
        public int comparisonId { get; set; }
        public int valueId { get; set; }
        public int combineId { get; set; }
        public string contextName { get; set; }
        public int valueTypeId { get; set; }
        public string dataItemName { get; set; }
        public int expressionId { get; set; }
        public string value { get; set; }
        public string comparison { get; set; }
        public string workflowExpression { get; set; }
        public int? approvalBusinessRuleId { get; set; }
        public string approvalBusinessRule { get; set; }
        public string valueTypeName { get; set; }
    }

    public class OperatorsViewModel
    {
        public int operatorId { get; set; }
        public string operators { get; set; }
        public string description { get; set; }
    }

    public class DynamicContextListViewModel
    {
        public int id { get; set; }
        public string value { get; set; }
    }

    public class DigitalStampViewModel
    {
        public int digitalStampId { get; set; }
        public int staffRoleId { get; set; }
        public string stampName { get; set; }
        public string digitalStamp { get; set; }
        public bool deleted { get; set; }
        public int deletedBy { get; set; }
        public DateTime? datetimeDeleted { get; set; }
        public int createdBy { get; set; }
        public DateTime? datetimeCreated { get; set; }
        public int updatedBy { get; set; }
        public DateTime? datetimeUpdated { get; set; }
        public short userBranchId { get; set; }
        public string applicationUrl { get; set; }
        public string userIPAddress { get; set; }
        public string staffRoleName { get; set; }
        public string fileExtension { get; set; }
    }
}
