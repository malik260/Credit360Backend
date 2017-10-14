using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Validation;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private FinTrakBankingContext context;

        public DepartmentRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup, 
            FinTrakBankingContext _context
            )
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this._genSetup = genSetup;
        }

        private bool SaveAll()
        {
            try
            {
                return this.context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
        }

        public bool AddDepartment(DepartmentViewModel entity)
        {
           
            var department = new tbl_Department
            {
                BranchId = entity.BranchId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = DateTime.Now,
                DepartmentCode = entity.DepartmentCode,
                DepartmentName = entity.DepartmentName,
                Description = entity.Description
            };
            this.context.tbl_Department.Add(department);
            // Audit Section ----------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.DepartmentAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = "Added new tbl_Department ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
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
                                  BranchName = context.tbl_Branch.FirstOrDefault(x=> x.BranchId == (short)d.BranchId).BranchName,
                                  DepartmentName = d.DepartmentName,
                                  DepartmentCode = d.DepartmentCode,
                                  Description = d.Description,
                                  DepartmentId = d.DepartmentId
                              });
            return department;
        }

        private IQueryable<DepartmentCustomersViewModel> SearchDepartments(int companyId) 
        {
            var department = (from d in context.tbl_Department
                              join c in context.tbl_Staff on d.DepartmentId equals c.DepartmentId
                              where c.CompanyId ==  companyId
                              select new DepartmentCustomersViewModel()
                              {
                                  createdBy = d.CreatedBy.Value,
                                  BranchId = d.BranchId,
                                  BranchName = context.tbl_Branch.FirstOrDefault(x => x.BranchId == (short)d.BranchId).BranchName,
                                  DepartmentName = d.DepartmentName,
                                  DepartmentCode = d.DepartmentCode,
                                  Description = d.Description,
                                  DepartmentId = d.DepartmentId,
                                  firstname = c.FirstName,
                                  lastname = c.LastName,
                                  staffId = c.StaffId,
                                  middlename = c.MiddleName,
                                  fullname = c.LastName + " " + c.FirstName + " " + c.MiddleName,
                                  rankName = c.tbl_Staff_Rank.RankName,
                                  jobTitleName = c.tbl_Staff_JobTitle.JobTitleName
                              });
            return department;
        }
        //SearchForDepartmentbyStaffId
        public IQueryable<DepartmentCustomersViewModel> SearchForDepartmentStaff(int companyId, string searchQuery , int departmentId)
        {
            IQueryable<DepartmentCustomersViewModel> allstaff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allstaff = SearchDepartments(companyId)
                    .Where(x => x.firstname.Contains(searchQuery)
               || x.lastname.Contains(searchQuery)
               || x.middlename.Contains(searchQuery)
               && x.DepartmentId == departmentId
                );
            }

            return allstaff;
        }

        public IQueryable<DepartmentCustomersViewModel> SearchForDepartmentStaff(int companyId, string searchQuery)
        {
            IQueryable<DepartmentCustomersViewModel> allstaff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allstaff = SearchDepartments(companyId)
                    .Where(x => x.firstname.Contains(searchQuery)
               || x.lastname.Contains(searchQuery)
               || x.middlename.Contains(searchQuery)
                );
            }

            return allstaff;
        }


        public IQueryable<DepartmentCustomersViewModel> SearchDepartment(int departmentId, int companyId, string searchQuery)
        {
            if (departmentId == 0) return null;
            IQueryable<DepartmentCustomersViewModel> allDepartmentStaff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }
           
                allDepartmentStaff = from s in context.tbl_Staff
                              join dept in context.tbl_Department on s.DepartmentId equals dept.DepartmentId
                              where dept.Deleted == false && dept.DepartmentId == departmentId && s.CompanyId == companyId
                              select new DepartmentCustomersViewModel
                              {
                                  createdBy = dept.CreatedBy.Value,
                                  BranchId = dept.BranchId,
                                  BranchName = context.tbl_Branch.FirstOrDefault(x => x.BranchId == (short)dept.BranchId).BranchName,
                                  DepartmentName = dept.DepartmentName,
                                  DepartmentCode = dept.DepartmentCode,
                                  Description = dept.Description,
                                  DepartmentId = dept.DepartmentId,
                                  firstname = s.FirstName,
                                  lastname = s.LastName,
                                  middlename = s.MiddleName,
                                  fullname = s.LastName +" "+ s.FirstName + " "+ s.MiddleName,
                                  staffId = s.StaffId,
                                  rankName = s.tbl_Staff_Rank.RankName,
                                  jobTitleName = s.tbl_Staff_JobTitle.JobTitleName
                              };

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    allDepartmentStaff = allDepartmentStaff
                        .Where(x => x.firstname.ToLower().Contains(searchQuery)
                        || x.middlename.ToLower().Contains(searchQuery)
                        || x.lastname.ToLower().Contains(searchQuery)
                             );
                }
          



            return allDepartmentStaff;
        }
        //IQueryable<CustomerSearchItemViewModels> CustomerSearchRealTime(int companyId, string searchQuery)

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
                                  BranchName = context.tbl_Branch.FirstOrDefault(x => x.BranchId == (short)d.BranchId).BranchName,
                                  DepartmentName = d.DepartmentName,
                                  Description = d.Description,
                                  DepartmentId = d.DepartmentId
                              }).SingleOrDefault();
            return department;
        }

        public DepartmentViewModel GetStaffDepartment(int staffId)
        {
            DepartmentViewModel result = new DepartmentViewModel();

            var department = context.tbl_Staff.Where(x => x.StaffId == staffId)
                .Join(context.tbl_Department,
                a => a.DepartmentId, b => b.DepartmentId, (a, b) => new { a, b })
                .Select(x => new DepartmentViewModel
                {
                    BranchId = x.b.BranchId,
                    DepartmentName = x.b.DepartmentName,
                    Description = x.b.Description,
                    DepartmentId = x.b.DepartmentId
                })
                .FirstOrDefault();
            if (department != null)
            {
                return department;
            }

            return result;
        }

        public bool UpdateDepartment(int departmentId, DepartmentViewModel entity)
        {
            var department = context.tbl_Department.Find(departmentId);

            department.BranchId = entity.BranchId;
            department.DepartmentName = entity.DepartmentName;
            department.Description = entity.Description;
            // Audit Section ----------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.DepartmentUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated tbl_Department with Id: {entity.DepartmentId} ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }

        public IEnumerable<OperationStaffViewModel> GetAllDepartmentStaff(int departmentId)
        {
            return context.tbl_Staff.Where(x=> x.Deleted == false && x.DepartmentId == departmentId).Select(x=> new OperationStaffViewModel
            {
                id = x.StaffId,
                name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
                groupId = departmentId,
            });
        }
    }
}