using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IDepartmentRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DepartmentRepository : IDepartmentRepository
    {
        private FinTrakBankingContext context;

        public DepartmentRepository(FinTrakBankingContext _context)
        {
            context = _context;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public bool AddDepartment(DepartmentViewModel entity)
        {
            var department = new tbl_Department
            {
                BranchId = entity.BranchId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = entity.dateTimeCreated,
                DepartmentCode = entity.DepartmentCode,
                DepartmentName = entity.DepartmentName,
                Description = entity.Description
            };
            this.context.tbl_Department.Add(department);
            return SaveAll();
        }

        /// <summary>
        /// Delete department using the id
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public bool DeleteDepartment(int departmentId)
        {
            var department = this.context.tbl_Department.Find(departmentId);
            department.Deleted = true;
            return SaveAll();
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DepartmentViewModel> GetAllDepartment()
        {
            var department = (from d in context.tbl_Department
                              select new DepartmentViewModel()
                              {
                                  createdBy = d.CreatedBy.Value,
                                  BranchId = d.BranchId,
                                  DepartmentName = d.DepartmentName,
                                  Description = d.Description,
                                  DepartmentId = d.DepartmentId
                              });
            return department;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public DepartmentViewModel GetDepartment(int departmentId)
        {
            var department = (from d in context.tbl_Department
                              where d.DepartmentId == departmentId
                              select new DepartmentViewModel()
                              {
                                  createdBy = d.CreatedBy.Value,
                                  BranchId = d.BranchId,
                                  DepartmentName = d.DepartmentName,
                                  Description = d.Description,
                                  DepartmentId = d.DepartmentId
                              }).SingleOrDefault();
            return department;
        }

        public bool UpdateDepartment(int departmentId, DepartmentViewModel entity)
        {
            var department = context.tbl_Department.Find(departmentId);

            department.CreatedBy = entity.createdBy;
            department.BranchId = entity.BranchId;
            department.DepartmentName = entity.DepartmentName;
            department.Description = entity.Description;
            department.DepartmentId = entity.DepartmentId;

            this.context.tbl_Department.Add(department);
            return SaveAll();
        }
    }
}