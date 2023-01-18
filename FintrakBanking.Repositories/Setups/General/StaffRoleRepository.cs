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
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Credit;

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
                            staffRoleShortCode = a.STAFFROLESHORTCODE,
                            staffRoleCode = a.STAFFROLECODE,
                            workEndDuration = a.WORKENDDURATION,
                            workStartDuration = a.WORKSTARTDURATION,
                            companyId = (short)a.COMPANYID,
                            staffRoleId = a.STAFFROLEID
                        }).SingleOrDefault();
            return role;
        }



        public bool DeleteBulkPrepayment(int bulkPrepaymentId, UserInfo user)
        {

            bool result = false;

            var targetRecord = context.TBL_BULK_PREPAYMENT.Where(x => x.BULK_PREPAYMENTID == bulkPrepaymentId).FirstOrDefault();

            context.TBL_BULK_PREPAYMENT.Remove(targetRecord);

            result = context.SaveChanges() > 0;

            return result;

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
                       staffRoleShortCode = a.STAFFROLESHORTCODE,
                       workEndDuration = a.WORKENDDURATION,
                       workStartDuration = a.WORKSTARTDURATION,
                       useRoundRublin = a.USEROUNDROBIN,
                      
                       approvalFlowTypeId = a.APPROVALFLOWTYPEID,
                       

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
        public StaffRoleViewModel GetStaffRoleByStaffId(int staffId)
        {
            var role = (from a in context.TBL_STAFF
                        join b in context.TBL_STAFF_ROLE on a.STAFFROLEID equals b.STAFFROLEID 

                        where a.STAFFID == staffId
                        select new StaffRoleViewModel
                        {
                            staffRoleName = b.STAFFROLENAME.Trim(),
                            companyId = (short)a.COMPANYID,
                            staffRoleId = a.STAFFROLEID,
                            staffRoleCode = b.STAFFROLECODE.Trim().ToUpper(),
                            staffRoleShortCode = b.STAFFROLESHORTCODE.Trim().ToUpper(),
                            workEndDuration = a.WORKENDDURATION,
                            workStartDuration = a.WORKSTARTDURATION,
                            allocationTypeId = b.APPROVALFLOWTYPEID,
                            staffId = staffId,
                        }).FirstOrDefault();
            return role;
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
                            staffRoleShortCode = a.STAFFROLESHORTCODE,
                            workEndDuration = a.WORKENDDURATION,
                            workStartDuration = a.WORKSTARTDURATION
                        });
            return role;
        }

        public IEnumerable<StaffRoleViewModel> GetStaffRoles()
        {
            return from a in context.TBL_STAFF_ROLE
                   select new StaffRoleViewModel
                   {
                       staffRoleName = a.STAFFROLENAME,
                       staffRoleCode = a.STAFFROLECODE,
                       staffRoleShortCode = a.STAFFROLESHORTCODE,
                       staffRoleId = a.STAFFROLEID,
                       workEndDuration = a.WORKENDDURATION,
                       workStartDuration = a.WORKSTARTDURATION,
                   };
        }

        

        public bool ValidateStaffRole(string staffRoleCode, string staffRoleName)
        {
            return context.TBL_STAFF_ROLE.Where(x => x.STAFFROLECODE == staffRoleCode || x.STAFFROLENAME == staffRoleName).Any();
        }

        public bool ValidateStaffRoleUpdate(int staffRoleId)
        {
            return context.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Where(x => x.STAFFROLEID == staffRoleId && x.ISCURRENT == true && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved).Any();
        }

        public bool AddUpdateStaffRole(StaffRoleViewModel entity)
        {
            if (entity == null) throw new SecureException("Session seems to have expired. Please logout and login again.");

            List<TBL_TEMP_PROFILE_STAFF_ROL_GRP> tempGroups = new List<TBL_TEMP_PROFILE_STAFF_ROL_GRP>();
            List<TBL_TEMP_PROFILE_STAFF_ROLE_AA> tempActivities = new List<TBL_TEMP_PROFILE_STAFF_ROLE_AA>();

            foreach (var item in entity.activitieIds)
            {
                tempActivities.Add(new TBL_TEMP_PROFILE_STAFF_ROLE_AA()
                {
                    ACTIVITYID = item,
                    CANADD = false,
                    CANEDIT = false,
                    CANAPPROVE = false,
                    CANDELETE = false,
                    CANVIEW = false,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    ISCURRENT = true,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                });
            }

            foreach (var item in entity?.userGroupIds)
            {
                tempGroups.Add(new TBL_TEMP_PROFILE_STAFF_ROL_GRP()
                {
                    GROUPID = (short)item,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = entity.createdBy,
                    ISCURRENT = true,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                });
            }

            TBL_STAFF_ROLE staffRole;

            if (entity.staffRoleId > 0) // update
            {
                staffRole = context.TBL_STAFF_ROLE.Find(entity.staffRoleId);
                if (staffRole == null) throw new SecureException("Cannot find the specified role!");

                // Removing existing temp groups and activities
                var targetGroups = context.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Where(x => x.STAFFROLEID == staffRole.STAFFROLEID).ToList();
                var targetActivities = context.TBL_TEMP_PROFILE_STAFF_ROLE_AA.Where(x => x.STAFFROLEID == staffRole.STAFFROLEID).ToList();
                if (targetGroups.Any()) foreach (var item in targetGroups) context.TBL_TEMP_PROFILE_STAFF_ROL_GRP.Remove(item);
                if (targetActivities.Any()) foreach (var item in targetActivities) context.TBL_TEMP_PROFILE_STAFF_ROLE_AA.Remove(item);

                staffRole.STAFFROLECODE = entity.staffRoleCode;
                staffRole.USEROUNDROBIN = entity.useRoundRublin;
                staffRole.STAFFROLENAME = entity.staffRoleName;
                staffRole.STAFFROLESHORTCODE = entity.staffRoleShortCode;
                staffRole.WORKSTARTDURATION = entity.workStartDuration;
                staffRole.WORKENDDURATION = entity.workEndDuration;
                staffRole.APPROVALFLOWTYPEID = entity.approvalFlowTypeId;
                staffRole.USEROUNDROBIN = entity.useRoundRublin;
               
                staffRole.TBL_TEMP_PROFILE_STAFF_ROL_GRP = tempGroups;
                staffRole.TBL_TEMP_PROFILE_STAFF_ROLE_AA = tempActivities;
            }
            else // create new
            {
                staffRole = new TBL_STAFF_ROLE
                {
                    STAFFROLECODE = entity.staffRoleCode,
                    STAFFROLENAME = entity.staffRoleName,
                    STAFFROLESHORTCODE = entity.staffRoleShortCode,
                    WORKSTARTDURATION = entity.workStartDuration,
                    WORKENDDURATION = entity.workEndDuration,
                    COMPANYID = entity.companyId,
                    APPROVALFLOWTYPEID = entity.approvalFlowTypeId,
                    USEROUNDROBIN = entity.useRoundRublin,
                    
                    TBL_TEMP_PROFILE_STAFF_ROL_GRP = tempGroups,
                    TBL_TEMP_PROFILE_STAFF_ROLE_AA = tempActivities
                };
                context.TBL_STAFF_ROLE.Add(staffRole);
            }

            // Audit Section ----------------------------
            auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = "Added/Modified Staff Role",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });

            var test = context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == entity.companyId
                                && x.OPERATIONID == (int)OperationsEnum.StaffRoleCreation
                                && x.TARGETID == staffRole.STAFFROLEID
                                && x.RESPONSESTAFFID == null
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                            ).ToList();

            if (context.TBL_APPROVAL_TRAIL.Where(x =>
                                x.COMPANYID == entity.companyId
                                && x.OPERATIONID == (int)OperationsEnum.StaffRoleCreation
                                && x.TARGETID == staffRole.STAFFROLEID
                                && x.RESPONSESTAFFID == null
                                && (x.APPROVALSTATEID != (int)ApprovalState.Ended && x.RESPONSEDATE == null)
                            ).Any())
            {
                throw new SecureException("There is an operation that is yet to be approved on this item!");
            }

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    if (context.SaveChanges() > 0)
                    {
                        workFlow.StaffId = entity.createdBy;
                        workFlow.CompanyId = entity.companyId;
                        workFlow.StatusId = (int)ApprovalStatusEnum.Pending;
                        workFlow.TargetId = staffRole.STAFFROLEID;
                        workFlow.Comment = "Create/Update Staff Role";
                        workFlow.OperationId = (int)OperationsEnum.StaffRoleCreation;
                        workFlow.ExternalInitialization = true;
                        if (workFlow.LogActivity()) trans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }

            return true;
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
                        where ((atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) || (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing))
                          && (gr.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && gr.ISCURRENT == true) ||
                          (aa.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && aa.ISCURRENT == true)
                              && atrail.RESPONSESTAFFID == null
                              && atrail.OPERATIONID == (int)OperationsEnum.StaffRoleCreation && ids.Contains((int)atrail.TOAPPROVALLEVELID) orderby atrail.ARRIVALDATE descending
                        select new StaffRoleViewModel()
                        {
                            staffRoleName = c.STAFFROLENAME,
                            staffRoleCode = c.STAFFROLECODE,
                            staffRoleShortCode = c.STAFFROLESHORTCODE,
                            staffRoleId = c.STAFFROLEID,
                            operationId = (int)OperationsEnum.StaffRoleCreation,
                        }).GroupBy(c=> c.staffRoleId).Select(g=>g.FirstOrDefault()).ToList() ;

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
            workFlow.StaffId = entity.staffId;
            workFlow.CompanyId = entity.companyId;
            workFlow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
            workFlow.TargetId = entity.targetId;
            workFlow.Comment = entity.comment;
            workFlow.DeferredExecution = true;
            workFlow.OperationId = (int)OperationsEnum.StaffRoleCreation;

            workFlow.LogActivity();

            if (workFlow.NewState == (int)ApprovalState.Ended && workFlow.StatusId == (short)ApprovalStatusEnum.Approved)
            {
                ApproveUserRoleUpdate(entity.targetId, entity);
            }
            else
            {
                var tempGroup = (from a in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP where a.STAFFROLEID == entity.targetId select a).ToList();
                var tempActivities = (from a in context.TBL_TEMP_PROFILE_STAFF_ROLE_AA where a.STAFFROLEID == entity.targetId select a).ToList();

                foreach (var item in tempGroup)
                {
                    item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                    item.ISCURRENT = true;
                }

                foreach (var item in tempActivities)
                {
                    item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                    item.ISCURRENT = true;
                }
            }
            return context.SaveChanges() > 0;
        }

        private void ApproveUserRoleUpdate(int staffRoleId, UserInfo user)
        {
            var tempGroup = (from a in context.TBL_TEMP_PROFILE_STAFF_ROL_GRP where a.STAFFROLEID == staffRoleId select a).ToList();
            var tempActivities = (from a in context.TBL_TEMP_PROFILE_STAFF_ROLE_AA where a.STAFFROLEID == staffRoleId select a).ToList();

            var existingGroups = context.TBL_PROFILE_STAFF_ROLE_GROUP.Where(x => x.STAFFROLEID == staffRoleId).ToList();
            var existingActivities = context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.Where(x => x.STAFFROLEID == staffRoleId).ToList();

            if (existingGroups.Any()) context.TBL_PROFILE_STAFF_ROLE_GROUP.RemoveRange(existingGroups);
            if (existingActivities.Any()) context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.RemoveRange(existingActivities);

            List<TBL_PROFILE_STAFF_ROLE_GROUP> newGroups = new List<TBL_PROFILE_STAFF_ROLE_GROUP>();
            List<TBL_PROFILE_STAFF_ROLE_ADT_ACT> newActivities = new List<TBL_PROFILE_STAFF_ROLE_ADT_ACT>();

            foreach (var item in tempGroup)
            {
                //if (existingGroups.Any(x => x.GROUPID == item.GROUPID)) continue;
                newGroups.Add(new TBL_PROFILE_STAFF_ROLE_GROUP()
                {
                    STAFFROLEID = item.STAFFROLEID,
                    GROUPID = item.GROUPID,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = item.CREATEDBY,
                    DATEAPPROVED = DateTime.Now,
                    APPROVALSTATUS = true
                });
            }

            foreach (var item in tempActivities)
            {
                //if (existingActivities.Any(x => x.ACTIVITYID == item.ACTIVITYID)) continue;
                newActivities.Add(new TBL_PROFILE_STAFF_ROLE_ADT_ACT()
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
                });
            }

            foreach (var item in tempGroup)
            {
                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                item.ISCURRENT = false;
                item.DATEAPPROVED = DateTime.Now;
            }

            foreach (var item in tempActivities)
            {
                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                item.ISCURRENT = false;
                item.DATETIMEUPDATED = DateTime.Now;
            }

            context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.AddRange(newActivities);
            context.TBL_PROFILE_STAFF_ROLE_GROUP.AddRange(newGroups);
            auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.UserApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Staff Role Group and Activities with staffRoleId : '{staffRoleId}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section ---------------------------
        }

        public bool AddApprovalSetUp(ApprovalSetUpViewModel entity)
        {
            var data = new TBL_APPROVAL_SETUP()
            {
                USEROUNDROBIN = entity.useRoundRublin,
                ISRETAILONLYROUNDROBIN = entity.isRetailOnlyRoundRobin
            };
            context.TBL_APPROVAL_SETUP.Add(data);

            return context.SaveChanges() > 0;
        }

        public IEnumerable<ApprovalSetUpViewModel> GetApprovalSetup()
        {
            return from a in context.TBL_APPROVAL_SETUP
                   select new ApprovalSetUpViewModel
                   {
                       approvalsetupId = a.APPROVALSETUPID,
                       useRoundRublin = a.USEROUNDROBIN,
                       isRetailOnlyRoundRobin = a.ISRETAILONLYROUNDROBIN
                   };
        }

        //public IEnumerable<ApprovalFlowTypeViewModel> GetAllApprovalFlowTypes()
        //{
        //    return from a in context.TBL_APPROVAL_FLOW_TYPE
        //           select new ApprovalFlowTypeViewModel
        //           {
        //                       approvalFlowTypeId = a.APPROVALFLOWTYPEID,
        //                       flowTypeName = a.FLOWTYPENAME
        //           };

        //}
        public IEnumerable<ApprovalFlowTypeViewModel> GetAllApprovalFlowTypes()
        {
            var flowTypes = (from x in context.TBL_APPROVAL_FLOW_TYPE
                             select new ApprovalFlowTypeViewModel
                             {
                                 approvalFlowTypeId = (short)x.APPROVALFLOWTYPEID,
                                 flowTypeName = x.FLOWTYPENAME,
                             }).ToList();

            return flowTypes;

        }

        public bool UpdateApprovalSetUp(ApprovalSetUpViewModel entity)
        {
            var data = this.context.TBL_APPROVAL_SETUP.Find(entity.approvalsetupId);
            {
                data.ISRETAILONLYROUNDROBIN = entity.isRetailOnlyRoundRobin;
                data.USEROUNDROBIN = entity.useRoundRublin;
            }

            return context.SaveChanges() > 0;
        }
        public IEnumerable<OperationPageOrderViewModel> GetAllOperationOrder()
        {
            var data = (from cs in context.TBL_LMSR_FLOW_ORDER
                        join b in context.TBL_OPERATIONS on cs.OPERATIONID equals b.OPERATIONID
                        select new OperationPageOrderViewModel()
                        {
                            tag = cs.TAG,
                           operationId = cs.OPERATIONID,
                           operationName = b.OPERATIONNAME,
                           floworderId = cs.FLOWORDERID,
                           requiredAppraisal = cs.REQUIREAPPRAISAL,
                           requiredAvailment = cs.REQUIREAVAILMENT,
                           requiredOfferLetter = cs.REQUIREOFFERLETTER,
                        }).ToList();

            return data;
        }

        public IEnumerable<OperationPageOrderViewModel> GetAllOperations()
        {
            var data = (from b in context.TBL_OPERATIONS 
                        select new OperationPageOrderViewModel()
                        {
                            operationId = b.OPERATIONID,
                            operationName = b.OPERATIONNAME,
                        }).ToList();

            return data;
        }

        public bool AddFlowOrder(OperationPageOrderViewModel entity)
        {
            var data = new TBL_LMSR_FLOW_ORDER()
            {
                TAG = entity.tag,
                REQUIREAPPRAISAL = entity.requiredAppraisal,
                REQUIREAVAILMENT = entity.requiredAvailment,
                REQUIREOFFERLETTER = entity.requiredOfferLetter,
                OPERATIONID = (short)entity.operationId,
                CREATEDBY = entity.createdBy,
                COMPANYID = entity.companyId,
                DATETIMECREATED = DateTime.Now,
            };

            context.TBL_LMSR_FLOW_ORDER.Add(data);

            return context.SaveChanges() > 0;
        }

        public bool UpdateFlowOrder(OperationPageOrderViewModel entity)
        {
            var data = this.context.TBL_LMSR_FLOW_ORDER.Find(entity.floworderId);
            {
              
                data.REQUIREOFFERLETTER = entity.requiredOfferLetter;
                data.REQUIREAVAILMENT = entity.requiredAvailment;
                data.REQUIREAPPRAISAL = entity.requiredAppraisal;
                data.TAG = entity.tag;
                data.DATETIMEUPDATED = DateTime.Now;
                data.UPDATEDBY = entity.staffId;
                data.OPERATIONID =(short)entity.operationId;
            }

            return context.SaveChanges() > 0;
        }
    }
}