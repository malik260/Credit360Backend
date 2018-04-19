using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private FinTrakBankingContext context;
        private IWorkflow workFlow;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IApprovalLevelStaffRepository level;

        public AdminRepository(FinTrakBankingContext _context,
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            IWorkflow _workFlow,
            IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            workFlow = _workFlow;
            level = _level;
        }

        #region Users

        public bool isUserExist(string username)
        {
            return context.TBL_PROFILE_USER.Any(x => x.USERNAME.ToLower() == username.ToLower());
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.UserCreation;

            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.LogForApproval(entity);
                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
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
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        private bool ApproveUser(int userId, short approvalStatusId, UserInfo user)
        {
            var userRecord = context.TBL_PROFILE_USER.Find(userId);

            if (userRecord != null)
            {
                userRecord.ISLOCKED = false;
                userRecord.ISACTIVE = true;
                userRecord.APPROVALSTATUSID = approvalStatusId;
                userRecord.APPROVALSTATUS = true;
                userRecord.DATEAPPROVED = DateTime.Now;
                userRecord.DATETIMEUPDATED = DateTime.Now;
                
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.UserApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved user with username : '{userRecord?.USERNAME}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            try
            {
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
                throw new Exception(ex.Message);
            }

        }

        public async Task<bool> CreateUser(AppUserViewModel user)
        {
            if (user == null)
            {
                return false;
            }

            bool output = false;

            List<TBL_PROFILE_USERGROUP> userGroups = new List<TBL_PROFILE_USERGROUP>();
            List<TBL_PROFILE_ADDITIONALACTIVITY> userActivities = new List<TBL_PROFILE_ADDITIONALACTIVITY>();

            if (user.activities.Any())
            {
                foreach (var item in user.activities)
                {
                    var userActivity = new TBL_PROFILE_ADDITIONALACTIVITY()
                    {
                        ACTIVITYID = item.activityId,
                        //UserId = _user.UserId,
                        CANADD = false,
                        CANEDIT = false,
                        CANAPPROVE = false,
                        CANDELETE = false,
                        CANVIEW = false,
                        CREATEDBY = user.createdBy,
                        DATETIMECREATED = DateTime.Now
                    };

                    userActivities.Add(userActivity);
                }
            }

            if (user.group.Count > 0)
            {
                foreach (var item in user.group)
                {
                    var grpItem = new TBL_PROFILE_USERGROUP()
                    {
                        GROUPID = item.groupId,
                        //UserId = _user.UserId,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = user.createdBy
                    };

                    userGroups.Add(grpItem);
                }
            }

            var _user = new TBL_PROFILE_USER()
            {
                STAFFID = user.staffId,
                USERNAME = user.username,
                PASSWORD = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey),
                ISFIRSTLOGINATTEMPT = false,
                ISACTIVE = false,
                ISLOCKED = true,
                FAILEDLOGONATTEMPT = 0,
                SECURITYQUESTION = user.securityQuestion,
                SECURITYANSWER = user.securityAnswer,
                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                CREATEDBY = user.createdBy,
                LASTUPDATEDBY = user.createdBy,
                DATETIMECREATED = DateTime.Now,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                APPROVALSTATUS = false,

                TBL_PROFILE_ADDITIONALACTIVITY = userActivities,
                TBL_PROFILE_USERGROUP = userGroups
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.UserAdded,
                STAFFID = (int)user.createdBy,
                BRANCHID = (short)user.userBranchId,
                DETAIL = $"Added User with username: '{user.username}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.TBL_PROFILE_USER.Add(_user);
                    auditTrail.AddAuditTrail(audit);

                    output = await context.SaveChangesAsync() > 0;

                    var entity = new ApprovalViewModel
                    {
                        staffId = user.createdBy,
                        companyId = user.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = _user.USERID,
                        operationId = (int)OperationsEnum.UserCreation,
                        BranchId = user.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public IEnumerable<UserViewModel> GetUsersAwaitingApproval(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.UserCreation).ToList();

            var data = (from c in context.TBL_PROFILE_USER
                        join br in context.TBL_BRANCH on c.TBL_STAFF.BRANCHID equals br.BRANCHID
                        join st in context.TBL_STAFF on c.STAFFID equals st.STAFFID
                        join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                        join dept in context.TBL_DEPARTMENT on c.TBL_STAFF.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                        join atrail in context.TBL_APPROVAL_TRAIL on c.USERID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        //&& c.ApprovalStatus == false
                              && atrail.RESPONSESTAFFID == null
                              && atrail.OPERATIONID == (int)OperationsEnum.UserCreation && ids.Contains((int)atrail.TOAPPROVALLEVELID)

                        select new UserViewModel()
                        {
                            user_id = c.USERID,
                            staffId = c.STAFFID,
                            companyId = coy.COMPANYID,
                            companyName = coy.NAME,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            username = c.USERNAME,
                            email = st.EMAIL,
                            staffName = st.FIRSTNAME + " " + st.LASTNAME,
                            IsFirstLoginAttempt = c.ISFIRSTLOGINATTEMPT,
                            isActive = c.ISACTIVE,
                            isLocked = c.ISLOCKED,
                            failedLogonAttempt = c.FAILEDLOGONATTEMPT,
                            securityQuestion = c.SECURITYQUESTION,
                            securityAnswer = c.SECURITYANSWER,
                            createdBy = c.CREATEDBY,
                            lastUpdatedBy = c.CREATEDBY,
                            dateTimeCreated = c.DATETIMECREATED,
                            approvalStatus = c.APPROVALSTATUS,
                            //approvalStatusId = atrail.ApprovalStatusId,
                            operationId = atrail.OPERATIONID,
                            groupId = c.TBL_PROFILE_USERGROUP.Where(x => x.USERID == c.USERID).Select(x => new UserGroupId
                            {
                                groupId = x.GROUPID,
                                groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                            }).ToList(),
                            activities = c.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == c.USERID).Select(a => new UserActivities
                            {
                                activityId = a.ACTIVITYID,
                                userId = a.USERID,
                                activityName = a.TBL_PROFILE_ACTIVITY.ACTIVITYNAME
                            }).ToList()
                        }).GroupBy(x => x.user_id).Select(g => g.FirstOrDefault());

            return data;
        }

        public IEnumerable<ApprovalStatusViewModel> GetApprovalStatus()
        {
            return from ap in context.TBL_APPROVAL_STATUS
                   select new ApprovalStatusViewModel
                   {
                       approvalStatusId = ap.APPROVALSTATUSID,
                       approvalStatusName = ap.APPROVALSTATUSNAME,
                       forDisplay = ap.FORDISPLAY,
                   };
        }

        public IEnumerable<UserViewModel> GetAllUsers()
        {
            return (from u in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF
                    on u.STAFFID equals st.STAFFID
                    where u.APPROVALSTATUS == true
                    select new UserViewModel()
                    {
                        user_id = u.USERID,
                        staffId = u.STAFFID,
                        username = u.USERNAME,
                        isActive = u.ISACTIVE,
                        staffName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                        email = st.EMAIL,
                        securityQuestion = u.SECURITYQUESTION,
                        securityAnswer = u.SECURITYANSWER,
                        groupId = u.TBL_PROFILE_USERGROUP.Where(x => x.USERID == u.USERID)
                                    .Select(x => new UserGroupId
                                    {
                                        groupId = x.GROUPID,
                                        groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                                    }).ToList(),
                        activities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == u.USERID)
                                     .Select(a => new UserActivities
                                     {
                                         activityId = a.ACTIVITYID,
                                         userId = a.USERID
                                     }).ToList(),
                        isLocked = u.ISLOCKED
                    });
        }

        public UserViewModel GetSingleUser(int userId)
        {
            var user = (from u in context.TBL_PROFILE_USER
                        join st in context.TBL_STAFF
                        on u.STAFFID equals st.STAFFID
                        where u.USERID == userId
                        select new UserViewModel()
                        {
                            user_id = u.USERID,
                            staffId = u.STAFFID,
                            username = u.USERNAME,
                            isActive = u.ISACTIVE,
                            staffName = st.FIRSTNAME + " " + st.LASTNAME,
                            email = st.EMAIL,
                            password = u.PASSWORD,
                            securityQuestion = u.SECURITYQUESTION,
                            securityAnswer = u.SECURITYANSWER
                        }).SingleOrDefault();

            if (user != null)
            {

                user.groupId = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == user.user_id)
                                        .Select(x => new UserGroupId
                                        {
                                            groupId = x.GROUPID,
                                            groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                                        })
                            .ToList();
            }

            return user;
        }

        public UserViewModel GetSingleUserByUserName(string userName)
        {
            return (from u in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF
                    on u.STAFFID equals st.STAFFID
                    where u.USERNAME == userName
                    select new UserViewModel()
                    {
                        user_id = u.USERID,
                        staffId = u.STAFFID,
                        username = u.USERNAME,
                        isActive = u.ISACTIVE,
                        staffName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                        email = st.EMAIL
                    }).FirstOrDefault();
        }

        public async Task<bool> UpdateUser(int userId, AppUserViewModel user)
        {
            bool output = false;
            var targetUser = context.TBL_PROFILE_USER.Find(userId);
            if (targetUser != null)
            {
                // Removing existing groups and activities
                var targetGroups = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == userId).ToList();
                var targetActivities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == userId).ToList();
                if (targetGroups.Any())
                {
                    foreach (var item in targetGroups)
                    {
                        context.TBL_PROFILE_USERGROUP.Remove(item);
                    }
                }

                if (targetActivities.Any())
                {
                    foreach (var item in targetActivities)
                    {
                        context.TBL_PROFILE_ADDITIONALACTIVITY.Remove(item);
                    }
                }

                List<TBL_PROFILE_USERGROUP> userGroups = new List<TBL_PROFILE_USERGROUP>();
                List<TBL_PROFILE_ADDITIONALACTIVITY> userActivities = new List<TBL_PROFILE_ADDITIONALACTIVITY>();

                if (user.group.Count > 0)
                {
                    foreach (var item in user.group)
                    {
                        var grpItem = new TBL_PROFILE_USERGROUP()
                        {
                            GROUPID = item.groupId,
                            //UserId = userId,
                            DATETIMECREATED = DateTime.Now,
                            CREATEDBY = user.createdBy
                        };

                        userGroups.Add(grpItem);
                    }
                }

                if (user.activities.Any())
                {
                    foreach (var item in user.activities)
                    {
                        var userActivity = new TBL_PROFILE_ADDITIONALACTIVITY()
                        {
                            ACTIVITYID = item.activityId,
                            //UserId = _user.UserId,
                            CANADD = false,
                            CANEDIT = false,
                            CANAPPROVE = false,
                            CANDELETE = false,
                            CANVIEW = false,
                            CREATEDBY = user.createdBy,
                            DATETIMECREATED = DateTime.Now
                        };

                        userActivities.Add(userActivity);
                    }
                }

                // Updating the target user
                targetUser.STAFFID = user.staffId;
                targetUser.USERNAME = user.username;
                targetUser.ISFIRSTLOGINATTEMPT = false;
                targetUser.ISACTIVE = false;
                targetUser.ISLOCKED = true;
                targetUser.FAILEDLOGONATTEMPT = 0;
                targetUser.CREATEDBY = user.createdBy;
                targetUser.LASTUPDATEDBY = user.createdBy;
                targetUser.DATETIMEUPDATED = DateTime.Now;
                targetUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                targetUser.APPROVALSTATUS = false;
                targetUser.TBL_PROFILE_USERGROUP = userGroups;
                targetUser.TBL_PROFILE_ADDITIONALACTIVITY = userActivities;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.UserUpdated,
                    STAFFID = user.createdBy,
                    BRANCHID = user.userBranchId,
                    DETAIL = $"Updated User with username: '{user.username}'",
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);

                        output = await context.SaveChangesAsync() > 0;

                        var entity = new ApprovalViewModel
                        {
                            staffId = user.createdBy,
                            companyId = user.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = userId,
                            operationId = (int)OperationsEnum.UserCreation,
                            BranchId = user.userBranchId,
                            externalInitialization = true
                        };

                        var response = workFlow.LogForApproval(entity);

                        if (response)
                        {
                            trans.Commit();
                        }

                        return output;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
            return false;
        }

        public Object ManageUserAccount(int userId, int lockStatus)
        {
            var userAccount = context.TBL_PROFILE_USER.Find(userId);

            try
            {
                if (userAccount != null && lockStatus == (int)UserAccountLockStatusEnum.Locked)
                {
                    userAccount.ISLOCKED = true;
                    userAccount.ISACTIVE = false;

                    context.SaveChanges();

                    return new { message = "User Account Locked" };
                }
                if (userAccount != null && lockStatus == (int)UserAccountLockStatusEnum.Unlocked)
                {
                    userAccount.ISLOCKED = false;
                    userAccount.ISACTIVE = true;

                    context.SaveChanges();

                    return new { message = "User Account Unlocked" };
                }

                return new { message = "No Account Found" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion Users

        #region Group

        public IEnumerable<AppGroupViewModel> GetAllGroups()
        {
            return context.TBL_PROFILE_GROUP.Select(x => new AppGroupViewModel
            {
                groupId = x.GROUPID,
                groupName = x.GROUPNAME
            });
        }

        public AppGroupViewModel GetSingleGroup(int groupId)
        {
            return context.TBL_PROFILE_GROUP.Where(g => g.GROUPID == groupId).Select(x => new AppGroupViewModel
            {
                groupId = x.GROUPID,
                groupName = x.GROUPNAME
            }).First();

            //var tt = context.TblApprovalGroup.FromSql("[sp_getGroup] @p0, @p1", parameters: new[] { groupId, groupId });
        }

        public bool isGroupExist(string groupName)
        {
            return context.TBL_PROFILE_GROUP.Any(x => x.GROUPNAME.ToLower() == groupName);
        }

        public  bool AddGroup(AppGroupViewModel group)
        {
            var newGroup = new TBL_PROFILE_GROUP()
            {
                GROUPNAME = group.groupName,
                CREATEDBY = group.createdBy,
                DATETIMECREATED = DateTime.Now
            };
            this.context.TBL_PROFILE_GROUP.Add(newGroup);  
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.UserGroupAdded,
                STAFFID = (int)group.createdBy,
                BRANCHID = (short)group.userBranchId,
                DETAIL = $"Added User group with name : '{group.groupName}' ",
                IPADDRESS = group.userIPAddress,
                URL = group.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return context.SaveChanges() > 0;
        }

        public bool UpdateGroup(short groupId, AppGroupViewModel groupModel)
        {
            var targetGroup = context.TBL_PROFILE_GROUP.Find(groupId);

            if (targetGroup != null)
            {
                targetGroup.GROUPNAME = groupModel.groupName;
                targetGroup.DATETIMEUPDATED = DateTime.Now;
                targetGroup.LASTUPDATEDBY = groupModel.createdBy;
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.UserGroupUpdated,
                STAFFID = (int)groupModel.createdBy,
                BRANCHID = (short)groupModel.userBranchId,
                DETAIL = $"Udate User group with name : '{groupModel.groupName}' ",
                IPADDRESS = groupModel.userIPAddress,
                URL = groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return  context.SaveChanges() != 0;
        }

        #endregion Group

        #region Activies

        public IEnumerable<ActivityParent> GetActivities()
        {
            return from p in context.TBL_PROFILE_ACTIVITY_PARENT
                   select new ActivityParent
                   {
                       activityParentId = p.ACTIVITYPARENTID,
                       activityParentName = p.ACTIVITYPARENTNAME,
                       activities = context.TBL_PROFILE_ACTIVITY
                                      .Where(x => x.ACTIVITYPARENTID == p.ACTIVITYPARENTID)
                                      .Select(x => new ActivityViewModel
                                      {
                                          activityId = x.ACTIVITYID,
                                          activityName = x.ACTIVITYNAME,
                                          activityParentId = x.ACTIVITYPARENTID
                                      }).ToList()
                   };
        }

        public IEnumerable<GroupVModel> GetGroupActivities()
        {
            var grpIds = context.TBL_PROFILE_GROUP_ACTIVITY.Select(x => x.GROUPID).Distinct().ToList();
            var data = (from g in context.TBL_PROFILE_GROUP
                        where grpIds.Contains(g.GROUPID)
                        select new GroupVModel
                        {
                            groupId = g.GROUPID,
                            name = g.GROUPNAME,
                            activities = (from ga in context.TBL_PROFILE_GROUP_ACTIVITY
                                          join act in context.TBL_PROFILE_ACTIVITY
                                          on ga.ACTIVITYID equals act.ACTIVITYID
                                          where ga.GROUPID == g.GROUPID
                                          select new GroupActivitiesModel
                                          {
                                              activityId = ga.ACTIVITYID,
                                              groupActivityId = ga.GROUPACTIVITYID,
                                              activityName = act.ACTIVITYNAME,
                                              canAdd = ga.CANADD.Value,
                                              canApprove = ga.CANAPPROVE.Value,
                                              canDelete = ga.CANDELETE.Value,
                                              canEdit = ga.CANEDIT.Value,
                                              canView = ga.CANVIEW.Value
                                          }).ToList()
                        });
            return data;
        }

        public bool AddAccessToActivity(int id, ActivitiesUpdateVm model)
        {
            var targetActivity = context.TBL_PROFILE_GROUP_ACTIVITY.Find(id);
            if (targetActivity != null)
            {
                targetActivity.CANADD = model.canAdd;
                targetActivity.CANAPPROVE = model.canApprove;
                targetActivity.CANVIEW = model.canView;
                targetActivity.CANEDIT = model.canEdit;
                targetActivity.CANDELETE = model.canDelete;
            }

            return context.SaveChanges() > 0;
        }

        public List<string> GetUserActivitiesByUser(int userId)
        {
            var userGroupIds = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == userId)
                                .Select(x => x.GROUPID).ToList();

            var activities = (from grpAct in context.TBL_PROFILE_GROUP_ACTIVITY
                              join act in context.TBL_PROFILE_ACTIVITY on grpAct.ACTIVITYID
                             equals act.ACTIVITYID
                              where userGroupIds.Contains(grpAct.GROUPID)
                              select act.ACTIVITYNAME.ToLower()).ToList();

            var additionalActivities = (from addAct in context.TBL_PROFILE_ADDITIONALACTIVITY
                                        join act in context.TBL_PROFILE_ACTIVITY
                                        on addAct.ACTIVITYID equals act.ACTIVITYID
                                        where addAct.USERID == userId
                                        select act.ACTIVITYNAME.ToLower()).ToList();

            if (additionalActivities.Any())
            {
                return activities.Concat(additionalActivities).Distinct().ToList();
            }
            else
            {
                return activities.Distinct().ToList();
            }
        }

        #endregion Activies


        #region Administration
        public IEnumerable<ActiveUserDetails> GetActiveUsers(int companyId)
        {
            return UserDetails(companyId);
        }

        private IQueryable<ActiveUserDetails> UserDetails(int companyId)
        {
            return from p in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF on p.STAFFID equals st.STAFFID
                    join br in context.TBL_BRANCH on st.BRANCHID equals br.BRANCHID
                    join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                    where   st.COMPANYID == companyId 
                    select new ActiveUserDetails
                    {
                        companyId = coy.COMPANYID,
                        staffId = p.STAFFID,
                        user_id = p.USERID,
                        username = p.USERNAME,
                        staffName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                        branchId = st.BRANCHID.Value,
                        countryId = coy.COUNTRYID,
                        branchName = br.BRANCHNAME,
                        companyName = coy.NAME,
                        logincode = p.LOGINCODE,
                        lastLoginDate = p.LASTLOGINDATE,
                        isActive = p.ISACTIVE,
                        isLocked = p.ISLOCKED,
                        failedLogonAttempt = p.FAILEDLOGONATTEMPT  ,
                         lastLockedOutDate = p.LASTLOCKOUTDATE
                         
                    } ;
        }

        public bool UpdateUserStatus(ActiveUserDetails entity , out string message)
        {
           // var data = context.TBL_PROFILE_USER.Where(p => p.USERID == entity.user_id && p.TBL_STAFF.DELETED).FirstOrDefault();

            var data = context.TBL_PROFILE_USER.Find(entity.user_id);

            if (data != null)
            {
                data.ISACTIVE = entity.isActive;

                data.DATETIMEUPDATED = DateTime.Now;
                data.LASTUPDATEDBY = entity.lastUpdatedBy;

                if (entity.isLocked )
                {
                    data.FAILEDLOGONATTEMPT = 0;
                    data.ISLOCKED = entity.isLocked;
                    data.LASTLOCKOUTDATE = DateTime.Now;
                    entity.actionMessage = "Account has been locked successfully";
                }
 

                if (!entity.isActive)
                {
                    data.ISACTIVE = entity.isActive;
                    data.DEACTIVATEDDATE = DateTime.Now;
                    entity.actionMessage = "Account has been deactivated successfully";
                }
                 

            }
            message = entity.actionMessage;

            return context.SaveChanges() > 0;
        }
        
        #endregion  

    }

    public enum UserAccountLockStatusEnum
    {
        Locked = 1,
        Unlocked = 2
    }
}