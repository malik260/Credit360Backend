using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalLevelRepository
    {
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
    }    
}
