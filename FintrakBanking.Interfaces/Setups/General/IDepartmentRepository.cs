using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IDepartmentRepository
    {
        IEnumerable<DepartmentViewModel> GetAllDepartment();

        IEnumerable<OperationStaffViewModel> GetAllDepartmentStaff(int departmentId);

        DepartmentViewModel GetDepartment(int departmentId);

        DepartmentViewModel GetStaffDepartment(int staffId);
        
        bool AddDepartment(DepartmentViewModel entity);

        bool UpdateDepartment(int departmentId, DepartmentViewModel entity);

        bool DeleteDepartment(int departmentId);
    }
}