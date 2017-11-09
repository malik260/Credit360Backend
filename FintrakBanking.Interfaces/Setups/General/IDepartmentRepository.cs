using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IDepartmentRepository
    {
        IEnumerable<DepartmentViewModel> GetAllDepartment();
        IEnumerable<DepartmentViewModel> GetAllDepartmentUnits(short departmentId);

        IEnumerable<OperationStaffViewModel> GetAllDepartmentStaff(int departmentId);

        IQueryable<DepartmentCustomersViewModel> SearchForDepartmentStaff(int companyId, string searchQuery, int departmentId);
        IQueryable<DepartmentCustomersViewModel> SearchForDepartmentStaff(int companyId, string searchQuery);
        IQueryable<DepartmentCustomersViewModel> SearchDepartment(int departmentId, int companyId, string searchQuery);


        bool AddDepartment(DepartmentViewModel entity);

        bool UpdateDepartment(int departmentId, DepartmentViewModel entity);

        bool DeleteDepartment(int departmentId);
    }
}