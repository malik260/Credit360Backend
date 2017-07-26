using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IDepartmentRepository
    {
        IEnumerable<DepartmentViewModel> GetAllDepartment();

        DepartmentViewModel GetDepartment(int departmentId);

        bool AddDepartment(DepartmentViewModel entity);

        bool UpdateDepartment(int departmentId, DepartmentViewModel entity);

        bool DeleteDepartment(int departmentId);
    }
}