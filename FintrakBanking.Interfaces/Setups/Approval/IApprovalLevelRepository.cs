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
        IEnumerable<ApprovalLevelViewModel> GetApprovalLevelById(int ApprovalLevelId, int companyId);
        IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByOperationId(int groupOperationMappingId, int companyId);
        bool AddApprovalLevel(ApprovalLevelViewModel model);
        
        bool AddMultipleApprovalLevel(List<ApprovalLevelViewModel> models);
        bool UpdateApprovalLevel(int ApprovalLevelId, ApprovalLevelViewModel model);
        Task<bool> DeleteApprovalLevel(int ApprovalLevelId, UserInfo user);
        IEnumerable<tbl_Staff_Organogram> GetStaffOrganogram(int companyId);
        bool UpdateApprovalTrail(tbl_Approval_Trail model);
        bool AddApprovalTrail(tbl_Approval_Trail model);
        IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int operationId, int companyId); 
        IQueryable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId);
        IQueryable<tbl_Approval_Trail> GetApprovalTrail(int operationId, int targetId, int approvalLevelId, int numberOfApprovals);
    }    
}
