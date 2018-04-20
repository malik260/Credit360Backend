using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IStaffRoleRepository
    {
        IEnumerable<StaffRoleViewModel> GetStaffRole();

        StaffRoleViewModel GetStaffRole(int rankId);

        IEnumerable<StaffRoleViewModel> GetStaffRoleByCompanyId(int companyId);

        IEnumerable<StaffRoleViewModel> GetStaffRoles();

        bool AddUpdateStaffRole(StaffRoleViewModel entity);

        bool ValidateStaffRole(string staffRoleCode, string staffRoleName);

        bool ValidateStaffRoleUpdate(int staffRoleId);

        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<StaffRoleViewModel> GetStaffRoleAwaitingApproval(int staffId, int companyId);
    }
}