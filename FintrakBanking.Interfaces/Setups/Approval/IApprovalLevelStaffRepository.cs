using FintrakBanking.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalLevelStaffRepository
    {
        bool AddApprovalLevelStaff(ApprovalLevelStaffViewModel model);

        bool UpdateApprovalLevelStaff(int StaffLevelId, ApprovalLevelStaffViewModel model);

        Task<bool> DeleteApprovalLevelStaff(int StaffLevelId, UserInfo user);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaffByOperationId(int operationId, int companyId);

        ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId, int operationId);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaff(int companyId);

        IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaffById(int StaffLevelId, int companyId);
    }
}
