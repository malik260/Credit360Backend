using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
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
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DEPARTMENTCODE = entity.departmentCode,
                DEPARTMENTNAME = entity.departmentName,
                DESCRIPTION = entity.description
            };

            this.context.TBL_DEPARTMENT.Add(department);

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = "Added new tbl_Department ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }

        /// <summary>
        /// Delete department using the id
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>

        /// <summary>
        /// Adds the department unit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool AddUnit(DepartmentViewModel entity)
        {
            
            var unit = new TBL_DEPARTMENT_UNIT
            {
               DEPARTMENTID = entity.departmentId,
               EMAIL = entity.unitEmail,
               DEPARTMENTUNITNAME = entity.unitName
            };

            this.context.TBL_DEPARTMENT_UNIT.Add(unit);

            // Audit Section ----------------------------
            var departmentName = context.TBL_DEPARTMENT.Where(x => x.DEPARTMENTID == entity.departmentId).FirstOrDefault().DEPARTMENTNAME;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentUnitAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added new department unit {entity.unitName} to {departmentName} department",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }

        /// <summary>
        /// Updates the department unit.
        /// </summary>
        /// <param name="unitId">The department unit identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public bool UpdateUnit(short unitId, DepartmentViewModel entity)
        {
            var unit = context.TBL_DEPARTMENT_UNIT.Find(unitId);

            unit.DEPARTMENTID = entity.departmentId;
            unit.DEPARTMENTUNITID = unitId;
            unit.DEPARTMENTUNITNAME = entity.unitName;
            unit.EMAIL = entity.unitEmail;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentUnitUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated department unit with Id: {unitId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }
        /// <summary>
        /// Update department unit staff.
        /// </summary>
        /// <param name="unitId">The department identifier.</param>
        /// <returns></returns>
        /// 

        /// <summary>
        /// Delete department unit using the id
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        /// 
        public bool DeleteUnit(UserInfo user, int unitId)
        {
            DateTime date = _genSetup.GetApplicationDate();
            var unit = this.context.TBL_DEPARTMENT_UNIT.Find(unitId);
            unit.DELETED = true;
            //context.TBL_DEPARTMENT_UNIT.Remove(unit);
           // return SaveAll();

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentUnitDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete department unit with Id: {unitId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = date,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }

        /// <summary>
        /// Gets all department.
        /// </summary>
        /// <returns></returns>
        /// 

        /// <summary>
        /// Delete department using the id
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        /// 
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
                                  departmentName = d.DEPARTMENTNAME,
                                  departmentCode = d.DEPARTMENTCODE,
                                  description = d.DESCRIPTION,
                                  departmentId = d.DEPARTMENTID
                              });
            return department;
        }

        public IEnumerable<DepartmentViewModel> GetJobDepartmentByJobTypeId(short jobTypeId)
        {
            //var departmentList = new List<DepartmentViewModel>();

            ////var staffDepartment = department.GetAllDepartment().Where(x => x.staffId == staffId);
            //var jobDeptMapping = context.TBL_JOB_TYPE_DEPARTMENT.Where(x => x.JOBTYPEID == jobTypeId);

            //foreach (var mapping in jobDeptMapping)
            //{
            //    var depts = (from d in context.TBL_DEPARTMENT
            //                 where d.DEPARTMENTID == mapping.DEPARTMENTID
            //                 select new DepartmentViewModel()
            //                 {
            //                     createdBy = d.CREATEDBY.Value,
            //                     departmentName = d.DEPARTMENTNAME,
            //                     departmentCode = d.DEPARTMENTCODE,
            //                     description = d.DESCRIPTION,
            //                     departmentId = d.DEPARTMENTID
            //                 }).ToList();

            //    foreach (var dept in depts)
            //    {
            //        departmentList.Add(dept);
            //    }
            //}

            //return departmentList;

            return null;
        }

        public IEnumerable<DepartmentViewModel> GetAllDepartmentUnits(short departmentId)
        {
            var departmentUnits = (from d in context.TBL_DEPARTMENT_UNIT where d.DEPARTMENTID == departmentId
                                   && d.DELETED ==false
                                   select new DepartmentViewModel()
                              {
                                  unitId = d.DEPARTMENTUNITID,
                                  departmentId = d.DEPARTMENTID,
                                  unitName = d.DEPARTMENTUNITNAME,
                                  unitEmail = d.EMAIL
                              });
            return departmentUnits;
        }

        public IEnumerable<DepartmentViewModel> GetAllUnits(int companyId)
        {
            var departmentUnits = (from d in context.TBL_DEPARTMENT_UNIT
                                   where d.TBL_DEPARTMENT.COMPANYID == companyId && d.DELETED == false
                                   select new DepartmentViewModel()
                                   {
                                       unitId = d.DEPARTMENTUNITID,
                                       departmentId = d.DEPARTMENTID,
                                       departmentName = d.TBL_DEPARTMENT.DEPARTMENTNAME,
                                       unitName = d.DEPARTMENTUNITNAME,
                                       unitEmail = d.EMAIL
                                   });
            return departmentUnits;
        }


        private IQueryable<DepartmentCustomersViewModel> SearchDepartments(int companyId) 
        {
            var department = (from d in context.TBL_DEPARTMENT
                              where d.COMPANYID ==  companyId
                              select new DepartmentCustomersViewModel()
                              {
                                  createdBy = d.CREATEDBY.Value,
                                  departmentName = d.DEPARTMENTNAME,
                                  departmentCode = d.DEPARTMENTCODE,
                                  description = d.DESCRIPTION,
                                  departmentId = d.DEPARTMENTID,
                              });
            return department;
        }
        private IQueryable<DepartmentCustomersViewModel> SearchDepartmentStaffByUnits(int companyId, int departmentUnitId)
        {
            var department = (from d in context.TBL_DEPARTMENT
                              join u in context.TBL_DEPARTMENT_UNIT on d.DEPARTMENTID equals u.DEPARTMENTID
                              join c in context.TBL_STAFF on u.DEPARTMENTUNITID equals c.DEPARTMENTUNITID
                              where c.COMPANYID == companyId && u.DEPARTMENTUNITID == departmentUnitId
                              select new DepartmentCustomersViewModel()
                              {
                                  createdBy = d.CREATEDBY.Value,
                                  branchName = c.TBL_BRANCH_REGION.Any() ? c.TBL_BRANCH_REGION.FirstOrDefault().TBL_BRANCH.Any() ? c.TBL_BRANCH_REGION.FirstOrDefault().TBL_BRANCH.FirstOrDefault().BRANCHNAME : null : null,
                                  departmentName = d.DEPARTMENTNAME,
                                  departmentCode = d.DEPARTMENTCODE,
                                  departmentUnitName = u.DEPARTMENTUNITNAME,
                                  departmentUnitId = u.DEPARTMENTUNITID,
                                  description = d.DESCRIPTION,
                                  departmentId = d.DEPARTMENTID,
                                  firstname = c.FIRSTNAME,
                                  lastname = c.LASTNAME,
                                  staffId = c.STAFFID,
                                  middlename = c.MIDDLENAME,
                                  staffCode = c.STAFFCODE,
                                  staffPhone = c.PHONE,
                                  staffEmail = c.EMAIL,
                                  roleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                                  jobTitleName = c.TBL_STAFF_JOBTITLE.JOBTITLENAME
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
        public IQueryable<DepartmentCustomersViewModel> SearchForDepartmentStaffByUnitId(int companyId, string searchQuery, int departmentUnitId)
        {
                    IQueryable<DepartmentCustomersViewModel> allstaff = null;
            
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allstaff = SearchDepartmentStaffByUnits(companyId, departmentUnitId)
                    .Where(x =>
                           (x.firstname.ToLower().Contains(searchQuery)
                           || x.lastname.ToLower().Contains(searchQuery)
                           || x.middlename.ToLower().Contains(searchQuery)
                           || x.staffCode.ToLower().Contains(searchQuery)
                           || x.staffEmail.ToLower().Contains(searchQuery)
                           || x.staffPhone.ToLower().Contains(searchQuery)
                           )
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
                              join dept in context.TBL_DEPARTMENT on s.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                              where dept.DELETED == false && dept.DEPARTMENTID == departmentId && s.COMPANYID == companyId
                              select new DepartmentCustomersViewModel
                              {
                                  createdBy = dept.CREATEDBY.Value,
                                  departmentName = dept.DEPARTMENTNAME,
                                  departmentCode = dept.DEPARTMENTCODE,
                                  description = dept.DESCRIPTION,
                                  departmentId = dept.DEPARTMENTID,
                                  firstname = s.FIRSTNAME,
                                  lastname = s.LASTNAME,
                                  middlename = s.MIDDLENAME,                                
                                  staffId = s.STAFFID,
                                  roleName = s.TBL_STAFF_ROLE.STAFFROLENAME,
                                  jobTitleName = s.TBL_STAFF_JOBTITLE.JOBTITLENAME
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
                                  departmentName = d.DEPARTMENTNAME,
                                  description = d.DESCRIPTION,
                                  departmentId = d.DEPARTMENTID
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
                a => a.TBL_DEPARTMENT_UNIT.DEPARTMENTID, b => b.DEPARTMENTID, (a, b) => new { a, b })
                .Select(x => new DepartmentViewModel
                {
                    departmentName = x.b.DEPARTMENTNAME,
                    description = x.b.DESCRIPTION,
                    departmentId = x.b.DEPARTMENTID
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

            department.DEPARTMENTNAME = entity.departmentName;
            department.DESCRIPTION = entity.description;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DepartmentUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_Department with Id: {entity.departmentId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
            return context.TBL_STAFF.Where(x=> x.DELETED == false && x.TBL_DEPARTMENT_UNIT.DEPARTMENTID == departmentId).Select(x=> new OperationStaffViewModel
            {
                id = x.STAFFID,
                name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME,
                groupId = departmentId,
            });
        }
    }
}