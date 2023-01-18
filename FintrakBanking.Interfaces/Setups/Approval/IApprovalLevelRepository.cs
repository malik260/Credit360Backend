using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalLevelRepository
    {
        IEnumerable<ApprovalTrailViewModel> GenericLMSApprovalTrail(int targetId, int operationId);
        IEnumerable<ApprovalLevelViewModel> GetAllApprovalLevel(int companyId);
        IEnumerable<ApprovalLevelViewModel> GetAllApprovalLevelDetails(int companyId);
        IEnumerable<ApprovalLevelViewModel> GetApprovalLevelById(int ApprovalLevelId, int companyId);
        IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByGroupId(int groupId, int companyId);
        IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByOperationId(int operationId, int companyId);

        bool AddApprovalLevel(ApprovalLevelViewModel model);
        
        bool AddMultipleApprovalLevel(List<ApprovalLevelViewModel> models);
        bool UpdateApprovalLevel(int ApprovalLevelId, ApprovalLevelViewModel model);
        Task<bool> DeleteApprovalLevel(int ApprovalLevelId, UserInfo user);
        IEnumerable<TBL_STAFF> GetStaffOrganogram(int companyId);
        bool UpdateApprovalTrail(TBL_APPROVAL_TRAIL model);
        Task<bool> AddApprovalTrail(TBL_APPROVAL_TRAIL model);
        IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int operationId, int companyId); 
        IQueryable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId);
        IQueryable<TBL_APPROVAL_TRAIL> GetApprovalTrail(int operationId, int targetId, int approvalLevelId, int numberOfApprovals);

        bool PresetRoute(PresetRouteViewModel entity);
        PresetRouteViewModel GetPresetRouteCollection(int operationId, int? classId);
        List<FintrakDropDownSelectList> GetApprovalLevelsByOperationIdAndProductClassId(int operationId, int? classId);
        int GoForApproval(ApprovalLevelViewModel model);
        List<ApprovalLevelViewModel> GetTempApprovalApprovalLevel(int staffId);


        List<FintrakDropDownSelectList> GetRoutableOperations(List<int> operationIds);
        List<ApprovalLevelViewModel> GetRerouteApprovalLevels(int operationId);
        bool RerouteOperation(ForwardViewModel entity);
        IEnumerable<ApprovalTrailViewModel> GenericApprovalTrail(ApprovalTrailRequestViewModel entity);
        List<FintrakDropDownSelectList> GetTranchDisbursmentApprovalLevels();

        IQueryable<WorkflowNotificationViewModel> GetWorkflowMappingNotifications(int MappingId);
        Task<bool> AddWorkflowMappingNotification(WorkflowNotificationViewModel model);
        bool UpdateWorkflowMappingNotification(WorkflowNotificationViewModel model, int workflowNotificationId);
        Task<bool> DeleteWorkflowMappingNotification(int MappingId, UserInfo user);
        IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowContext();
        IEnumerable<OperatorsViewModel> GetAllOperators();
        IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowDataItemDefinition();
        IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowDataItemByContextId(int contextId);
        DynamicWorkflowViewModel GetValueTypeByItemId(int dataItemId);
        bool CreateDynamicWorkflowItemExpression(DynamicWorkflowViewModel model);
        IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowItemExpression();
       // IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowItemExpressionById();
        bool UpdateDynamicWorkflowItemExpression(DynamicWorkflowViewModel model, int expressionId);
        List<DynamicContextListViewModel> GetDynamicBusinessRuleItemValueListByItemId(int dataItemId);
    }
}

