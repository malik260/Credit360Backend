using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.CustomException;

using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{

    public class StaffRoleRepository : IStaffRoleRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        public StaffRoleRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
             IGeneralSetupRepository _genSetup, IWorkflow _workFlow,
            IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            workFlow = _workFlow;
            level = _level;
        }


        public StaffRoleViewModel GetStaffRole(int jobTitleId)
        {
            var role = (from a in context.TBL_STAFF_ROLE
                        select new StaffRoleViewModel
                        {
                            staffRoleName = a.STAFFROLENAME,
                            workEndDuration = a.WORKENDDURATION,
                            workStartDuration = a.WORKSTARTDURATION,
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
                       staffRoleId = a.STAFFROLEID,
                       workEndDuration = a.WORKENDDURATION,
                       workStartDuration = a.WORKSTARTDURATION,
                       userGroup = a.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Where(x => x.STAFFROLEID == a.STAFFROLEID).Select(x => new UserGroup
                       {
                           groupId = x.GROUPID,
                           groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                       }).ToList(),
                       activities = a.TBL_TEMP_PROFILE_STAFF_ROLE_AA.Where(x => x.STAFFROLEID == a.STAFFROLEID).Select(d => new UserActivities
                       {
                           activityId = d.ACTIVITYID,
                           userId = d.STAFFROLEID,
                           activityName = d.TBL_PROFILE_ACTIVITY.ACTIVITYNAME
                       }).ToList()
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
                            staffRoleCode = a.STAFFROLECODE,
                            workEndDuration = a.WORKENDDURATION,
                            workStartDuration = a.WORKSTARTDURATION,
                        });
            return role;
        }

        public IEnumerable<StaffRoleViewModel> GetStaffRoles()
        {
            return from a in context.TBL_STAFF_ROLE
                   select new StaffRoleViewModel
                   {
                       staffRoleName = a.STAFFROLENAME,
                       staffRoleId = a.STAFFROLEID,
                       workEndDuration = a.WORKENDDURATION,
                       workStartDuration = a.WORKSTARTDURATION,
                   };
        }
        public bool AddUpdateStaffRole(StaffRoleViewModel entity)
        {
            try
            {
                if (entity != null)
                {
                    try
                    {
                        bool output = false;

                        List<TBL_TEMP_PROFILE_STAFF_ROL_GRP> tempGroups = new List<TBL_TEMP_PROFILE_STAFF_ROL_GRP>();
                        List<TBL_TEMP_PROFILE_STAFF_ROLE_AA> tempActivities = new List<TBL_TEMP_PROFILE_STAFF_ROLE_AA>();

                        if (entity.activities.Any())
                        {
                            foreach (var item in entity.activities)
                            {
                                var userActivity = new TBL_TEMP_PROFILE_STAFF_ROLE_AA()
                                {
                                    ACTIVITYID = item.activityId,
                                    CANADD = false,
                                    CANEDIT = false,
                                    CANAPPROVE = false,
                                    CANDELETE = false,
                                    CANVIEW = false,
                                    CREATEDBY = entity.createdBy,
                                    DATETIMECREATED = DateTime.Now,
                                    ISCURRENT = true,
                                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending

                                };
                                tempActivities.Add(userActivity);
                            }
                        }

                        if (entity.userGroup.Count > 0)
                        {
                            foreach (var item in entity.userGroup)
                            {
                                var grpItem = new TBL_TEMP_PROFILE_STAFF_ROL_GRP()
                                {
                                    GROUPID = item.groupId,
                                    DATETIMECREATED = DateTime.Now,
                                    CREATEDBY = entity.createdBy,
                                    ISCURRENT = true,
                                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                                };
                                tempGroups.Add(grpItem);
                            }
                        }

                        TBL_STAFF_ROLE staffRole;
                        if (entity.staffRoleId > 0)
                        {
                            staffRole = context.TBL_STAFF_ROLE.Find(entity.staffRoleId);
                            if (staffRole != null)
                            {
                                // Removing existing groups and activities
                                var targetGroups = context.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Where(x => x.STAFFROLEID == staffRole.STAFFROLEID).ToList();
                                var targetActivities = context.TBL_TEMP_PROFILE_STAFF_ROLE_AA.Where(x => x.STAFFROLEID == staffRole.STAFFROLEID).ToList();
                                if (targetGroups.Any())
                                {
                                    foreach (var item in targetGroups)
                                    {
                                        context.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Remove(item);
                                    }
                                }

                                if (targetActivities.Any())
                                {
                                    foreach (var item in targetActivities)
                                    {
                                        context.TBL_TEMP_PROFILE_STAFF_ROLE_AA.Remove(item);
                                    }
                                }

                                staffRole.STAFFROLECODE = entity.staffRoleCode;
                                staffRole.STAFFROLENAME = entity.staffRoleName;
                                staffRole.WORKSTARTDURATION = entity.workStartDuration;
                                staffRole.WORKENDDURATION = entity.workEndDuration;
                                staffRole.TBL_TEMP_PROFILE_STAFF_ROL_GRP = tempGroups;
                                staffRole.TBL_TEMP_PROFILE_STAFF_ROLE_AA = tempActivities;
                            }
                        }
                        else
                        {
                            staffRole = new TBL_STAFF_ROLE
                            {
                                STAFFROLECODE = entity.staffRoleCode,
                                STAFFROLENAME = entity.staffRoleName,
                                WORKSTARTDURATION = entity.workStartDuration,
                                WORKENDDURATION = entity.workEndDuration,
                                COMPANYID = entity.companyId,
                                TBL_TEMP_PROFILE_STAFF_ROL_GRP = tempGroups,
                                TBL_TEMP_PROFILE_STAFF_ROLE_AA = tempActivities
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

                        using (var trans = context.Database.BeginTransaction())
                        {
                            try
                            {
                                auditTrail.AddAuditTrail(audit);
                                output = context.SaveChanges() > 0;

                                workFlow.StaffId = entity.createdBy;
                                workFlow.CompanyId = entity.companyId;
                                workFlow.StatusId = (int)ApprovalStatusEnum.Pending;
                                workFlow.TargetId = staffRole.STAFFROLEID;
                                workFlow.Comment = "Create/Update Staff Role";
                                workFlow.OperationId = (int)OperationsEnum.StaffRoleCreation;
                                workFlow.ExternalInitialization = true;

                                var response = workFlow.LogActivity();
                                if (response)
                                {
                                    trans.Commit();
                                }
                                return output;
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new SecureException(ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new SecureException(ex.Message);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public bool ValidateStaffRole(string staffRoleCode, string staffRoleName)
        {
            return context.TBL_STAFF_ROLE.Where(x => x.STAFFROLECODE == staffRoleCode || x.STAFFROLENAME == staffRoleName).Any();
        }
        public bool ValidateStaffRoleUpdate(int staffRoleId)
        {
            return context.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Where(x => x.STAFFROLEID == staffRoleId && x.ISCURRENT == true && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved).Any();
        }

        public IEnumerable<StaffRoleViewModel> GetStaffRoleAwaitingApproval(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.StaffRoleCreation).ToList();

            var data = (from c in context.TBL_STAFF_ROLE
                      join gr in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP on c.STAFFROLEID equals gr.STAFFROLEID
                      into cc from gr in cc.DefaultIfEmpty()
                        join aa in context.TBL_TEMP_PROFILE_STAFF_ROLE_AA on c.STAFFROLEID equals aa.STAFFROLEID
                       into ca from aa in ca.DefaultIfEmpty()
                        join atrail in context.TBL_APPROVAL_TRAIL on c.STAFFROLEID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                          && (gr.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && gr.ISCURRENT == true) ||
                          (aa.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && aa.ISCURRENT == true)
                              && atrail.RESPONSESTAFFID == null
                              && atrail.OPERATIONID == (int)OperationsEnum.StaffRoleCreation && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                        select new StaffRoleViewModel()
                        {
                            staffRoleName = c.STAFFROLENAME,
                            staffRoleCode = c.STAFFROLECODE,
                            staffRoleId = c.STAFFROLEID,
                            operationId = (int)OperationsEnum.StaffRoleCreation,
                        }).GroupBy(c=> c.staffRoleId).Select(g=>g.FirstOrDefault());

            var userGroup = (from x in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP
                             select new UserGroup
                             {
                                 staffRoleId = x.STAFFROLEID,
                                 groupId = x.GROUPID,
                                 groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                             }).ToList();

            var activities = (from a in context.TBL_TEMP_PROFILE_STAFF_ROLE_AA 
                              join b in context.TBL_PROFILE_ACTIVITY on a.ACTIVITYID equals b.ACTIVITYID
                              select new UserActivities
                              {
                                  activityId = a.ACTIVITYID,
                                  userId = a.STAFFROLEID,
                                  activityName = b.ACTIVITYNAME
                              }).ToList();
           
            foreach (var s in data)
            {
                s.userGroup = userGroup.Where(l => l.staffRoleId == s.staffRoleId).ToList();
                s.activities = activities.Where(u=> u.userId == s.staffRoleId).ToList();
            }

            return data;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.StaffId = entity.staffId;
                    workFlow.CompanyId = entity.companyId;
                    workFlow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId; 
                    workFlow.TargetId = entity.targetId;
                    workFlow.Comment = entity.comment;
                    workFlow.OperationId = (int)OperationsEnum.StaffRoleCreation;

                    workFlow.LogActivity();

                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveUser(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        var tempGroup = (from a in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP where a.STAFFROLEID == entity.targetId select a).ToList();
                        if (tempGroup != null)
                        {
                            foreach (var item in tempGroup)
                            {
                                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                                item.ISCURRENT = true;
                                item.DATEAPPROVED = DateTime.Now;
                            }
                        }
                        context.SaveChanges();
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }
        private bool ApproveUser(int staffRoleId, short approvalStatusId, UserInfo user)
        {
            var tempGroup = (from a in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP where a.STAFFROLEID == staffRoleId select a).ToList();
            var tempActivities = (from a in context.TBL_TEMP_PROFILE_STAFF_ROLE_AA where a.STAFFROLEID == staffRoleId select a).ToList();

            foreach (var item in tempGroup)
            {
                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                item.ISCURRENT = false;
                item.DATEAPPROVED = DateTime.Now;
            }


            List<TBL_PROFILE_STAFF_ROLE_GROUP> userGroups = new List<TBL_PROFILE_STAFF_ROLE_GROUP>();
            List<TBL_PROFILE_STAFF_ROLE_ADT_ACT> userActivities = new List<TBL_PROFILE_STAFF_ROLE_ADT_ACT>();

            if (tempActivities.Any())
            {
                foreach (var item in tempActivities)
                {
                    var userActivity = new TBL_PROFILE_STAFF_ROLE_ADT_ACT()
                    {
                        STAFFROLEID = item.STAFFROLEID,
                        ACTIVITYID = item.ACTIVITYID,
                        CANADD = false,
                        CANEDIT = false,
                        CANAPPROVE = false,
                        CANDELETE = false,
                        CANVIEW = false,
                        CREATEDBY = item.CREATEDBY,
                        DATETIMECREATED = item.DATETIMECREATED
                    };
                    userActivities.Add(userActivity);
                }
            }

            if (tempGroup.Count > 0)
            {
                foreach (var item in tempGroup)
                {
                    var grpItem = new TBL_PROFILE_STAFF_ROLE_GROUP()
                    {
                        STAFFROLEID = item.STAFFROLEID,
                        GROUPID = item.GROUPID,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = item.CREATEDBY,
                        DATEAPPROVED = DateTime.Now,
                        APPROVALSTATUS = true
                    };
                    userGroups.Add(grpItem);
                }
            }

            // Removing existing groups and activities
            var targetGroups = context.TBL_PROFILE_STAFF_ROLE_GROUP.Where(x => x.STAFFROLEID == staffRoleId).ToList();
            var targetActivities = context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.Where(x => x.STAFFROLEID == staffRoleId).ToList();
            if (targetGroups.Any())
            {
                foreach (var item in targetGroups)
                {
                    context.TBL_PROFILE_STAFF_ROLE_GROUP.Remove(item);
                }
            }

            if (targetActivities.Any())
            {
                foreach (var item in targetActivities)
                {
                    context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.Remove(item);
                }
            }

            foreach (var item in tempActivities)
            {
                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                item.ISCURRENT = false;
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.UserApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Staff Role Group and Activities with staffRoleId : '{staffRoleId}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            try
            {
                context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.AddRange(userActivities);
                context.TBL_PROFILE_STAFF_ROLE_GROUP.AddRange(userGroups);
                auditTrail.AddAuditTrail(audit);
                // Audit Section ---------------------------
                var response = context.SaveChanges() > 0;

                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }

        }
    }
}