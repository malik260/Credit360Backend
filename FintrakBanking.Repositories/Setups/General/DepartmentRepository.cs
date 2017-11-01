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
        /// <summary>
        /// Adds the department.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool AddDepartment(DepartmentViewModel entity)
        {
           
            var department = new TBL_DEPARTMENT
            {
                BRANCHID = entity.BranchId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DEPARTMENTCODE = entity.DepartmentCode,
                DEPARTMENTNAME = entity.DepartmentName,
                DESCRIPTION = entity.Description
            };
            this.context.TBL_DEPARTMENT.Add(department);
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = "Added new tbl_Department ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
            var department = this.context.TBL_DEPARTMENT.Find(departmentId);
            department.DELETED = true;
            return SaveAll();
        }

        /// <summary>
        /// Gets all department.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DepartmentViewModel> GetAllDepartment()
        {
            var department = (from d in context.TBL_DEPARTMENT
                              select new DepartmentViewModel()
                              {
                                  createdBy = d.CREATEDBY.Value,
                                  BranchId = d.BRANCHID,
                                  BranchName = context.TBL_BRANCH.FirstOrDefault(x=> x.BRANCHID == (short)d.BRANCHID).BRANCHNAME,
                                  DepartmentName = d.DEPARTMENTNAME,
                                  DepartmentCode = d.DEPARTMENTCODE,
                                  Description = d.DESCRIPTION,
                                  DepartmentId = d.DEPARTMENTID
                              });
            return department;
        }

        private IQueryable<DepartmentCustomersViewModel> SearchDepartments(int companyId) 
        {
            var department = (from d in context.TBL_DEPARTMENT
                              join c in context.TBL_STAFF on d.DEPARTMENTID equals c.DEPARTMENTID
                              where c.COMPANYID ==  companyId
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
        /// <summary>
        /// Searches for department staff.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="departmentId">The department identifier.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Searches for department staff.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Searches the department.
        /// </summary>
        /// <param name="departmentId">The department identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public IQueryable<DepartmentCustomersViewModel> SearchDepartment(int departmentId, int companyId, string searchQuery)
        {
            if (departmentId == 0) return null;
            IQueryable<DepartmentCustomersViewModel> allDepartmentStaff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }
           
                allDepartmentStaff = from s in context.TBL_STAFF
                              join dept in context.TBL_DEPARTMENT on s.DEPARTMENTID equals dept.DEPARTMENTID
                              where dept.DELETED == false && dept.DEPARTMENTID == departmentId && s.COMPANYID == companyId
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
        /// Gets the department.
        /// </summary>
        /// <param name="departmentId">The department identifier.</param>
        /// <returns></returns>
        public DepartmentViewModel GetDepartment(int departmentId)
        {
            var department = (from d in context.TBL_DEPARTMENT
                              where d.DEPARTMENTID == departmentId
                              select new DepartmentViewModel()
                              {
                                  createdBy = d.CREATEDBY.Value,
                                  BranchId = d.BRANCHID,
                                  BranchName = context.TBL_BRANCH.FirstOrDefault(x => x.BRANCHID == (short)d.BRANCHID).BRANCHNAME,
                                  DepartmentName = d.DEPARTMENTNAME,
                                  Description = d.DESCRIPTION,
                                  DepartmentId = d.DEPARTMENTID
                              }).SingleOrDefault();
            return department;
        }
        /// <summary>
        /// Gets the staff department.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <returns></returns>
        public DepartmentViewModel GetStaffDepartment(int staffId)
        {
            DepartmentViewModel result = new DepartmentViewModel();

            var department = context.TBL_STAFF.Where(x => x.STAFFID == staffId)
                .Join(context.TBL_DEPARTMENT,
                a => a.DEPARTMENTID, b => b.DEPARTMENTID, (a, b) => new { a, b })
                .Select(x => new DepartmentViewModel
                {
                    BranchId = x.b.BRANCHID,
                    DepartmentName = x.b.DEPARTMENTNAME,
                    Description = x.b.DESCRIPTION,
                    DepartmentId = x.b.DEPARTMENTID
                })
                .FirstOrDefault();
            if (department != null)
            {
                return department;
            }

            return result;
        }
        /// <summary>
        /// Updates the department.
        /// </summary>
        /// <param name="departmentId">The department identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool UpdateDepartment(int departmentId, DepartmentViewModel entity)
        {
            var department = context.TBL_DEPARTMENT.Find(departmentId);

            department.BRANCHID = entity.BranchId;
            department.DEPARTMENTNAME = entity.DepartmentName;
            department.DESCRIPTION = entity.Description;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_Department with Id: {entity.DepartmentId} ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }
        /// <summary>
        /// Gets all department staff.
        /// </summary>
        /// <param name="departmentId">The department identifier.</param>
        /// <returns></returns>
        public IEnumerable<OperationStaffViewModel> GetAllDepartmentStaff(int departmentId)
        {
            return context.TBL_STAFF.Where(x=> x.DELETED == false && x.DEPARTMENTID == departmentId).Select(x=> new OperationStaffViewModel
            {
                id = x.STAFFID,
                name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME,
                groupId = departmentId,
            });
        }
    }
}