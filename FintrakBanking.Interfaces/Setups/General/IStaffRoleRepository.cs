using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IStaffRoleRepository
    {
        IEnumerable<StaffRoleViewModel> GetStaffRole();

        StaffRoleViewModel GetStaffRole(int rankId);

        IEnumerable<StaffRoleViewModel> GetStaffRoleByCompanyId(int companyId);

        IEnumerable<StaffRoleViewModel> GetStaffRoles();
    }
}