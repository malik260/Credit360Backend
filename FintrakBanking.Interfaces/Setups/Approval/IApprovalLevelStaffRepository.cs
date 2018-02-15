using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalLevelStaffRepository
    {
        bool AddApprovalLevelStaff(ApprovalLevelStaffViewModel model);

        bool UpdateApprovalLevelStaff(int staffLevelId, ApprovalLevelStaffViewModel model);

        Task<bool> DeleteApprovalLevelStaff(int staffLevelId, UserInfo user);

        //IEnumerable<ApprovalLevelStaffViewModel> GetAllAssignedApprovalLevelStaff(int companyId);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaffByOperationId(int operationId, int companyId);

        ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId, int operationId);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaff(int companyId);

        IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaffById(int staffLevelId, int companyId);

        Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId,
            int targetId, int companyId);

        IQueryable<WorkflowTrackerViewModel> GetAllRecordsOnApprovalTrail(int companyId);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllAssignedApprovalLevelStaff(int companyId);

        ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId);

    }
}