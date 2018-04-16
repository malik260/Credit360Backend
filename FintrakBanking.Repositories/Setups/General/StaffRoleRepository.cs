using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IStaffRoleRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StaffRoleRepository : IStaffRoleRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        public StaffRoleRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
             IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }


        public StaffRoleViewModel GetStaffRole(int jobTitleId)
        {
            var role = (from a in context.TBL_STAFF_ROLE
                        select new StaffRoleViewModel
                        {
                            staffRoleName = a.STAFFROLENAME,
                            companyId = (short)a.COMPANYID,
                            staffRoleId = a.STAFFROLEID
                        }).SingleOrDefault();
            return role;
        }
        public IEnumerable<StaffRoleViewModel> GetStaffRoleByCompanyId(int companyId)
        {
            return from a in context.TBL_STAFF_ROLE
                   where a.COMPANYID == companyId
                   select new StaffRoleViewModel
                   {
                       staffRoleName = a.STAFFROLENAME,
                       companyId = (short)a.COMPANYID,
                       staffRoleCode = a.STAFFROLECODE,
                       staffRoleId = a.STAFFROLEID
                   };

        }

        public IEnumerable<StaffRoleViewModel> GetStaffRole()
        {
            var role = (from a in context.TBL_STAFF_ROLE
                        select new StaffRoleViewModel
                        {
                            staffRoleName = a.STAFFROLENAME,
                            companyId = (short)a.COMPANYID,
                            staffRoleId = a.STAFFROLEID,
                            staffRoleCode = a.STAFFROLECODE
                        });
            return role; 
        }

        public IEnumerable<StaffRoleViewModel> GetStaffRoles()
        {
            return from a in context.TBL_STAFF_ROLE
                   select new StaffRoleViewModel
                   {
                       staffRoleName = a.STAFFROLENAME,
                       staffRoleId = a.STAFFROLEID
                   };
        }
        public bool AddUpdateStaffRole(StaffRoleViewModel entity)
        {
            if (entity != null)
            {
                try
                {

                    TBL_STAFF_ROLE staffRole;
                    if (entity.staffRoleId > 0)
                    {
                        staffRole = context.TBL_STAFF_ROLE.Find(entity.staffRoleId);
                        if (staffRole != null)
                        {
                            staffRole.STAFFROLECODE = entity.staffRoleCode;
                            staffRole.STAFFROLENAME = entity.staffRoleName;
                        }
                    }
                    else
                    {
                        staffRole = new TBL_STAFF_ROLE
                        {
                            STAFFROLECODE = entity.staffRoleCode,
                            STAFFROLENAME = entity.staffRoleName,
                            COMPANYID = entity.companyId
                        };
                        context.TBL_STAFF_ROLE.Add(staffRole);
                    }
                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added/Modified Staff Role",
                        IPADDRESS = entity.userIPAddress,
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return false;
        }
        public bool ValidateStaffRole(string staffRoleCode, string staffRoleName)
        {
            return context.TBL_STAFF_ROLE.Where(x => x.STAFFROLECODE == staffRoleCode || x.STAFFROLENAME == staffRoleName).Any();
        }
    }
}