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
using System.Data.Entity;
using System.Threading.Tasks;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Interfaces;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using FintrakBanking.Interfaces.Credit;

namespace FintrakBanking.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private FinTrakBankingContext context;
        private IWorkflow workFlow;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IProfileSetupRepository proSetting;
        private IIntegrationWithFinacle finacle;
        //private IApprovalLevelStaffRepository level;
        private ITwoFactorAuthIntegrationService auth;
        bool USE_THIRD_PARTY_INTEGRATION = false;

        public AdminRepository(FinTrakBankingContext _context,
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            IWorkflow _workFlow,
            IProfileSetupRepository _proSetting,
            IIntegrationWithFinacle _finacle,

        // IApprovalLevelStaffRepository _level,
        ITwoFactorAuthIntegrationService _auth)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.auth = _auth;
            workFlow = _workFlow;
            finacle = _finacle;
            this.proSetting = _proSetting;
           // level = _level;

            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;

        }

        #region Users

        public void DeactivateInactiveUsers()
        {
            var currentDateTime = DateTime.Now.Date;

            int userInactivePeriod = context.TBL_PROFILE_SETTING.FirstOrDefault().MAXPERIODOFUSERINACTIVITY;

            var inactiveUsersSub = (from a in context.TBL_PROFILE_USER
                                    where a.ISACTIVE == true //&& a.LASTLOGINDATE != null
                                    //&& (DbFunctions.AddDays(a.LASTLOGINDATE, userInactivePeriod) <= DbFunctions.AddDays(currentDateTime, 0))
                                    && DbFunctions.DiffDays(a.LASTLOGINDATE, currentDateTime) > userInactivePeriod
                                       select a);
            
            var inactiveUsers = inactiveUsersSub.ToList();

            foreach (var item in inactiveUsers)
            {
                item.ISACTIVE = false;
                item.DEACTIVATEDDATE = currentDateTime;
            }

            context.SaveChanges();
        }

        public bool isUserExist(string username)
        {
            return context.TBL_PROFILE_USER.Any(x => x.USERNAME.ToLower() == username.ToLower());
        }

        public bool isStaffExist(long staffId)
        {
            return context.TBL_PROFILE_USER.Any(x => x.STAFFID == staffId);
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
                throw new SecureException(ex.Message);
            }

        }

        public bool CreateUser(AppUserViewModel user)
        {
            if (user == null)
            {
                return false;
            }

            bool output = false;

            List<TBL_PROFILE_USERGROUP> userGroups = new List<TBL_PROFILE_USERGROUP>();
            List<TBL_PROFILE_ADDITIONALACTIVITY> userActivities = new List<TBL_PROFILE_ADDITIONALACTIVITY>();
            var profileSettings = proSetting.GetProfileSettings();
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
                //NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profileSettings.expirePasswordAfter),
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

                    output = context.SaveChanges() > 0;

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
                    throw new SecureException(ex.Message);
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
        public IEnumerable<UserViewModel> GetUsersWithAccountStatusChangeAwaitingApproval(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.UserCreation).ToList();

            var data = (from temp in context.TBL_TEMP_PROFILE_USER
                        join c in context.TBL_PROFILE_USER on temp.USERNAME equals c.USERNAME
                        join br in context.TBL_BRANCH on c.TBL_STAFF.BRANCHID equals br.BRANCHID
                        join st in context.TBL_STAFF on c.STAFFID equals st.STAFFID
                        join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                       // join dept in context.TBL_DEPARTMENT on c.TBL_STAFF.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                        join atrail in context.TBL_APPROVAL_TRAIL on temp.TEMPUSERID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.RESPONSESTAFFID == null
                              && atrail.OPERATIONID == (int)OperationsEnum.UserAccountStatusChange && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                              && temp.OPERATION.ToLower() == "update"

                        select new UserViewModel()
                        {
                            user_id = temp.TEMPUSERID,
                            staffId = c.STAFFID,
                            companyId = coy.COMPANYID,
                            companyName = coy.NAME,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            username = temp.USERNAME,
                            email = st.EMAIL,
                            staffName = st.FIRSTNAME + " " + st.LASTNAME,
                            IsFirstLoginAttempt = temp.ISFIRSTLOGINATTEMPT,
                            isActive = temp.ISACTIVE,
                            isLocked = temp.ISLOCKED,
                            failedLogonAttempt = temp.FAILEDLOGONATTEMPT,
                            securityQuestion = temp.SECURITYQUESTION,
                            securityAnswer = temp.SECURITYANSWER,
                            createdBy = temp.CREATEDBY,
                            lastUpdatedBy = temp.CREATEDBY,
                            dateTimeCreated = temp.DATETIMECREATED,
                            approvalStatus = temp.APPROVALSTATUS,
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

        public UserViewModel GetUsersByStaffId(int staffId)
        {
            var data = (from u in context.TBL_PROFILE_USER
                        join st in context.TBL_STAFF
                        on u.STAFFID equals st.STAFFID
                        where u.STAFFID == staffId && u.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
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
                            password = null,
                            isLocked = u.ISLOCKED
                        }).FirstOrDefault();
            if (data != null)
            {
                var groupId = (from x in context.TBL_PROFILE_USERGROUP
                               join g in context.TBL_PROFILE_GROUP on x.GROUPID equals g.GROUPID
                               where x.USERID == data.user_id
                               select new UserGroupId
                               {
                                   groupId = x.GROUPID,
                                   groupKey = g.GROUPNAME
                               }).ToList();
                data.groupId = groupId;
                var activities = (from a in context.TBL_PROFILE_ADDITIONALACTIVITY
                                  where a.USERID == data.user_id
                                  select new UserActivities
                                  {
                                      activityId = a.ACTIVITYID,
                                      activityName = context.TBL_PROFILE_ACTIVITY.Where(x=>x.ACTIVITYID == a.ACTIVITYID).Select(g=>g.ACTIVITYNAME).FirstOrDefault(),
                                      userId = a.USERID,
                                      activityParentId = context.TBL_PROFILE_ACTIVITY.Where(x => x.ACTIVITYID == a.ACTIVITYID).Select(g => g.ACTIVITYPARENTID).FirstOrDefault(),
                                      activityParentName = context.TBL_PROFILE_ACTIVITY_PARENT.Where(x => x.ACTIVITYPARENTID == context.TBL_PROFILE_ACTIVITY.Where(b => b.ACTIVITYID == a.ACTIVITYID).Select(g => g.ACTIVITYPARENTID).FirstOrDefault()).Select(p => p.ACTIVITYPARENTNAME).FirstOrDefault(),
                                      expireOn = a.EXPIREON,
                                      selected = true,
                                  }).ToList();
                data.activities = activities;
            }
            return data;
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

        public bool UpdateUser(int userId, AppUserViewModel user)
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

                        output = context.SaveChanges() > 0;

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
                        throw new SecureException(ex.Message);
                    }
                }
            }
            return false;
        }

        //public Object ManageUserAccount(int userId, int lockStatus)
        //{
        //    var userAccount = context.TBL_PROFILE_USER.Find(userId);

        //    try
        //    {
        //            userAccount.ISLOCKED = true;
        //            userAccount.ISACTIVE = false;
        //            userAccount.ISLOCKED = false;
        //            userAccount.ISACTIVE = true;

        //        context.SaveChanges();
        //        return new { message = "No Account Found" };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new SecureException(ex.Message);
        //    }
        //}

        #endregion Users
        public GlobalSettingViewModel GetAllGlobalSettings()
        {
            return context.TBL_SETUP_GLOBAL.Select(x => new GlobalSettingViewModel
            {
                applicationSetupId =x.APPLICATIONSETUPID,
                reportPath = x.REPORTPATH,
                useActiveDirectory = x.USE_ACTIVE_DIRECTORY,
                activeDirectoryDomainName = x.ACTIVE_DIRECTORY_DOMAIN_NAME,
                activeDirectoryUserName = x.ACTIVE_DIRECTORY_USERNAME,
                activeDirectoryUserPassword = x.ACTIVE_DIRECTORY_PASSWORD,
                requireAdUser = x.REQUIRE_ADUSER,
                useThirdPArtyIntegration = x.USE_THIRD_PARTY_INTEGRATION,
                useTwoFactorAuthentication = x.USE_TWO_FACTOR_AUTHENTICATION,
                maxFileUploadSize = x.MAXIMUMUPLOADFILESIZE,
                applicationURL = x.APPLICATION_URL,
                supportEmail = x.SUPPORT_EMAIL,
        }).FirstOrDefault();
        }
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

        public bool AddGroup(AppGroupViewModel group)
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
                URL =  group.applicationUrl,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;
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
                                          activityParentId = x.ACTIVITYPARENTID,
                                          selected = false,
                                      }).ToList() 
                   };
        }

        public IEnumerable<UserActivities> GetActivityDetails(int parentId)
        {
            return from x in context.TBL_PROFILE_ACTIVITY
                   where x.ACTIVITYPARENTID == parentId

                   select new UserActivities
                   {
                       activityId = x.ACTIVITYID, 
                        activityName = x.ACTIVITYNAME, 
                        activityParentId = x.ACTIVITYPARENTID,
                       activityParentName = context.TBL_PROFILE_ACTIVITY_PARENT.Where(k => k.ACTIVITYPARENTID == x.ACTIVITYPARENTID).Select(p => p.ACTIVITYPARENTNAME).FirstOrDefault(),
                       selected = false,
                     
                   };
        }

        public IEnumerable<UserActivities> GetActivityDetails(int parentId, int staffId)
        {
            var parentActivities = from x in context.TBL_PROFILE_ACTIVITY
                   where x.ACTIVITYPARENTID == parentId

                   select new UserActivities
                   {
                       activityId = x.ACTIVITYID,
                       activityName = x.ACTIVITYNAME,
                       activityParentId = x.ACTIVITYPARENTID,
                       activityParentName = context.TBL_PROFILE_ACTIVITY_PARENT.Where(k => k.ACTIVITYPARENTID == x.ACTIVITYPARENTID).Select(p => p.ACTIVITYPARENTNAME).FirstOrDefault(),

                       selected = false,

                   };



            return parentActivities;
        }
        //public IEnumerable<ActivityParent> Get(int staffId)
        //{
        //               activities = context.TBL_PROFILE_ACTIVITY
        //                              .Where(x => x.ACTIVITYPARENTID == p.ACTIVITYPARENTID)
        //                              .Select(x => new ActivityViewModel
        //                              {
        //                                  activityId = x.ACTIVITYID,
        //                                  activityName = x.ACTIVITYNAME,
        //                                  activityParentId = x.ACTIVITYPARENTID
        //                              }).ToList()
        //}

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

        public bool IsSuperAdmin(int staffId)
        {
            var userId = (from a in context.TBL_PROFILE_USER                               
                               where a.STAFFID == staffId
                               select a.USERID).FirstOrDefault();

            var userActivities = GetUserActivitiesByUser(userId);

            var superAdmin = userActivities.Where(x => x.ToLower() == "super admin".ToLower()).FirstOrDefault();

            if (superAdmin != null)
                return true;
            else
                return false;            
        }
        public bool StaffHasActivity(int staffId, string activity)
        {
            var userId = (from a in context.TBL_PROFILE_USER
                          where a.STAFFID == staffId
                          select a.USERID).FirstOrDefault();

            var userActivities = GetUserActivitiesByUser(userId);

            var findActivity = userActivities.Where(x => x.ToLower() == activity.ToLower()).FirstOrDefault();

            if (findActivity != null)
                return true;
            else
                return false;
        }

        public List<string> GetUserActivitiesByUser(int userId)
        {
            List<string> listOfActivities = new List<string>();

            var userGroupIds = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == userId)
                                .Select(x => x.GROUPID).ToList();

            //var staffRoleId = (from a in context.TBL_PROFILE_USER
            //                   join b in context.TBL_STAFF
            //                   on a.STAFFID equals b.STAFFID
            //                   where a.USERID == userId
            //                   select b.STAFFROLEID).FirstOrDefault();

            var staffRoleId = (from a in context.TBL_PROFILE_USER
                               join b in context.TBL_STAFF
                               on a.STAFFID equals b.STAFFID
                               where a.USERID == userId
                               select b.STAFFROLEID).FirstOrDefault();

            var staffGroupIds = context.TBL_PROFILE_STAFF_ROLE_GROUP.Where(x => x.STAFFROLEID == staffRoleId)
                                .Select(x => x.GROUPID).ToList();

            var staffRoleActivities = (from grpAct in context.TBL_PROFILE_GROUP_ACTIVITY
                                       join act in context.TBL_PROFILE_ACTIVITY on grpAct.ACTIVITYID
                                      equals act.ACTIVITYID
                                       where staffGroupIds.Contains(grpAct.GROUPID)
                                       select act.ACTIVITYNAME.ToLower()).ToList();

            var staffRoleAdditionalActivities = (from addAct in context.TBL_PROFILE_STAFF_ROLE_ADT_ACT
                                                 join act in context.TBL_PROFILE_ACTIVITY
                                                 on addAct.ACTIVITYID equals act.ACTIVITYID
                                                 where addAct.STAFFROLEID == staffRoleId
                                                 select act.ACTIVITYNAME.ToLower()).ToList();

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
            if (activities.Any())
            {
                listOfActivities = listOfActivities.Concat(activities).Distinct().ToList();
            }
            if (additionalActivities.Any())
            {
                listOfActivities = listOfActivities.Concat(additionalActivities).Distinct().ToList();
            }
            if (staffRoleActivities.Any())
            {
                listOfActivities = listOfActivities.Concat(staffRoleActivities).Distinct().ToList();
            }
            if (staffRoleAdditionalActivities.Any())
            {
                listOfActivities = listOfActivities.Concat(staffRoleAdditionalActivities).Distinct().ToList();
            }

            return listOfActivities.Distinct().ToList();
        }

        #endregion Activies

        #region Administration
        public IEnumerable<ActiveUserDetails> GetActiveUsers(int companyId)
        {
            return UserDetails(companyId);
        }

        private IQueryable<ActiveUserDetails> UserDetails(int companyId)
        {
            var data = (from p in context.TBL_PROFILE_USER
                        join st in context.TBL_STAFF on p.STAFFID equals st.STAFFID
                        join br in context.TBL_BRANCH on st.BRANCHID equals br.BRANCHID
                        join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                        where st.COMPANYID == companyId where st.DELETED == false
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
                            failedLogonAttempt = p.FAILEDLOGONATTEMPT,
                            lastLockedOutDate = p.LASTLOCKOUTDATE

                        });
            return data;
        }

        //public bool UpdateUserStatus(ActiveUserDetails entity, out string message)
        //{
        //    // var data = context.TBL_PROFILE_USER.Where(p => p.USERID == entity.user_id && p.TBL_STAFF.DELETED).FirstOrDefault();
        //    string str = string.Empty;
        //    var data = context.TBL_PROFILE_USER.Find(entity.user_id);

        //    if (data != null)
        //    {
        //        if (entity.lockStatus)
        //        {
        //            data.FAILEDLOGONATTEMPT = 0;
        //            data.ISLOCKED = entity.isLocked;
        //        }


        //        if (entity.accountStatus)
        //        {
        //            data.ISACTIVE = entity.isActive;
        //            data.DEACTIVATEDDATE = DateTime.Now;
        //        }

        //        data.LASTUPDATEDBY = entity.lastUpdatedBy;
        //        entity.actionMessage = "Operation Successful";

        //    }

        //    message = entity.actionMessage;
        //    StaticHelpers.RestartService();
        //    return context.SaveChanges() > 0;
        //}

        public int GoForUserAccountStatusApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.UserAccountStatusChange;

            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    int returnId = 0;
                    workFlow.LogForApproval(entity);
                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }
                   
                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = UpdateUserAccountStatus(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            returnId = entity.approvalStatusId == (short)ApprovalStatusEnum.Approved ? 2 : 3;
                            trans.Commit();
                        }
                        return returnId;
                    }
                    else
                    {
                        returnId = 1;
                        trans.Commit();
                    }

                    return returnId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        public bool UpdateUserAccountStatus(int userId, short approvalStatusId, UserInfo user)
        {
            var tempData = context.TBL_TEMP_PROFILE_USER.Find(userId);
            if (tempData == null) throw new ConditionNotMetException("Could not resolve user account status update.");

            var data = context.TBL_PROFILE_USER.FirstOrDefault(x=>x.USERNAME == tempData.USERNAME);
            data.ISACTIVE = tempData.ISACTIVE;
            data.DEACTIVATEDDATE = tempData.DEACTIVATEDDATE;

            data.ISLOCKED = tempData.ISLOCKED;
            data.FAILEDLOGONATTEMPT = tempData.FAILEDLOGONATTEMPT;
            tempData.ISCURRENT = false;

           //context.TBL_TEMP_PROFILE_USER.Remove(tempData);

            return context.SaveChanges() > 0;
        }

        public bool LogUserStatusUpdateRequest(ActiveUserDetails entity, out string message)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                TBL_TEMP_PROFILE_USER affectedrecord;
                var tempData = context.TBL_TEMP_PROFILE_USER.Where(p => p.USERNAME == entity.username && p.ISCURRENT == true && p.OPERATION.ToLower() == "update").FirstOrDefault();
                var data = context.TBL_PROFILE_USER.Find(entity.user_id);
                if (tempData != null)
                {
                    throw new ConditionNotMetException("There is already a User Account status update currently undergoing approval");
                }
                else
                {
                    var temProfileUser = new TBL_TEMP_PROFILE_USER();
                    temProfileUser.USERNAME = data.USERNAME;
                    temProfileUser.SECURITYQUESTION = data.SECURITYQUESTION;
                    temProfileUser.SECURITYANSWER = data.SECURITYANSWER;
                    temProfileUser.PASSWORD = data.PASSWORD;
                    temProfileUser.NEXTPASSWORDCHANGEDATE = data.NEXTPASSWORDCHANGEDATE;
                    temProfileUser.LOGINCODE = data.LOGINCODE;
                    temProfileUser.APPROVALSTATUS = data.APPROVALSTATUS;
                    temProfileUser.APPROVALSTATUSID = data.APPROVALSTATUSID;
                    temProfileUser.CREATEDBY = data.CREATEDBY;
                    temProfileUser.DATEAPPROVED = data.DATEAPPROVED;
                    temProfileUser.DATETIMECREATED = data.DATETIMECREATED;
                    temProfileUser.DATETIMEUPDATED = data.DATETIMEUPDATED;
                    temProfileUser.DEACTIVATEDDATE = data.DEACTIVATEDDATE;
                    temProfileUser.FAILEDLOGONATTEMPT = data.FAILEDLOGONATTEMPT;
                    temProfileUser.ISACTIVE = data.ISACTIVE;
                    temProfileUser.ISFIRSTLOGINATTEMPT = data.ISFIRSTLOGINATTEMPT;
                    temProfileUser.ISLOCKED = data.ISLOCKED;
                    temProfileUser.LASTLOCKOUTDATE = data.LASTLOCKOUTDATE;
                    temProfileUser.LASTLOGINDATE = data.LASTLOGINDATE;
                    temProfileUser.LASTUPDATEDBY = data.LASTUPDATEDBY;
                    temProfileUser.ISCURRENT = true;
                    temProfileUser.OPERATION = "Update";
                    //temProfileUser.TEMPSTAFFID = data.STAFFID;

                    if (entity.lockStatus)
                    {
                        temProfileUser.FAILEDLOGONATTEMPT = 0;
                        temProfileUser.ISLOCKED = entity.isLocked;
                    }

                    if (entity.accountStatus)
                    {
                        temProfileUser.ISACTIVE = entity.isActive;
                        temProfileUser.DEACTIVATEDDATE = DateTime.Now;
                    }

                    context.TBL_TEMP_PROFILE_USER.Add(temProfileUser);

                    message = entity.actionMessage;

                    var output = context.SaveChanges() > 0;
                    if (!output)
                    {
                        trans.Rollback();
                        throw new SecureException("Failed to Save");
                    }
                    affectedrecord = temProfileUser;
                }

                var model = new ApprovalViewModel
                {
                    staffId = entity.createdBy,
                    companyId = entity.companyId,
                    approvalStatusId = (int)ApprovalStatusEnum.Pending,
                    targetId = affectedrecord.TEMPUSERID,
                    operationId = (int)OperationsEnum.UserAccountStatusChange,
                    BranchId = entity.userBranchId,
                    externalInitialization = true
                };
                var response = workFlow.LogForApproval(model);
                if (response)
                {
                    trans.Commit();
                    return true;
                }
                else 
                { 
                    trans.Rollback();
                    return false;
                }
            }
        }

        #endregion

        #region TwoFactorAuthentication
        public TwoFactorAutheticationOutputViewModel TwoFactorAuthentication(string staffCode, string passCode)
        {
            try
            {
                var output = auth.Authenticate(staffCode, passCode);
                return output;
            }
            catch (TwoFactorAuthenticationException ex)
            {
                throw new TwoFactorAuthenticationException(ex.Message);
            }
             
        }
        public bool TwoFactorAuthenticationEnabled()
        {
            var output = context.TBL_SETUP_GLOBAL.FirstOrDefault().USE_TWO_FACTOR_AUTHENTICATION;
            return output;
        }
        
        public bool Enable2FAForLastApproval(int staffId, int operationId, int? productClassId, int? productId, decimal levelAmount = 0)
        {
            //return true;  ///TEST ONLY> TO BE REMOVED
            bool output = false;
            if (USE_THIRD_PARTY_INTEGRATION == true)
            {
                var staff = context.TBL_STAFF.Find(staffId);

                List<int> approvalLevelIds = new List<int>();

                if (levelAmount > 0)
                {
                    approvalLevelIds = (from x in context.TBL_APPROVAL_GROUP_MAPPING
                                        join y in context.TBL_APPROVAL_GROUP on x.GROUPID equals y.GROUPID
                                        join z in context.TBL_APPROVAL_LEVEL on x.GROUPID equals z.GROUPID
                                        where x.OPERATIONID == operationId && x.PRODUCTCLASSID == null && x.DELETED == false
                                        && z.MAXIMUMAMOUNT >= levelAmount && z.ISACTIVE == true && z.DELETED == false 
                                        orderby x.POSITION, z.POSITION ascending
                                        select z.APPROVALLEVELID
                           ).ToList();
                }
                else
                {
                    approvalLevelIds = (from x in context.TBL_APPROVAL_GROUP_MAPPING
                                        join y in context.TBL_APPROVAL_GROUP on x.GROUPID equals y.GROUPID
                                        join z in context.TBL_APPROVAL_LEVEL on x.GROUPID equals z.GROUPID
                                        where x.OPERATIONID == operationId && x.PRODUCTCLASSID == null 
                                        && x.DELETED == false && z.ISACTIVE == true && z.DELETED == false
                                        orderby x.POSITION, z.POSITION ascending
                                        select z.APPROVALLEVELID
                                               ).ToList();
                }
             

                var levelCount = approvalLevelIds.Count;

                var staffApprovalLevelId = (from x in context.TBL_APPROVAL_GROUP_MAPPING
                                    join y in context.TBL_APPROVAL_GROUP on x.GROUPID equals y.GROUPID
                                    join z in context.TBL_APPROVAL_LEVEL on x.GROUPID equals z.GROUPID
                                    join st in context.TBL_APPROVAL_LEVEL_STAFF on z.APPROVALLEVELID equals st.APPROVALLEVELID into aps
                                    from sub in aps.DefaultIfEmpty()
                                    where x.OPERATIONID == operationId && ((z.STAFFROLEID == staff.STAFFROLEID) || (sub.STAFFID == staff.STAFFID))
                                    select z.APPROVALLEVELID).FirstOrDefault();

                var currentLevel = approvalLevelIds.IndexOf(staffApprovalLevelId) + 1;

                if (currentLevel == levelCount)
                {
                    output = true;
                }
            }

            return output;
        }
        #endregion
        public Users GetStaffActiveDirectoryDetails(string staffCode,string loginUser, string password)
        {
            //var test = ValidateActiveDirectoryCredentials("TMP10004", "!23Helives2");
            var user = GetActiveDirectoryDetails(staffCode, loginUser, password);

            if (USE_THIRD_PARTY_INTEGRATION)
            {
                var userRole = finacle.GetUserRoleFinacle(staffCode);
                if (userRole.staffRole != null)
                {        
                    if (userRole.staffRole == "RM" || userRole.staffRole == "BM")
                    {
                        user.staffRole = userRole.staffRole;
                        user.staffRoleId = context.TBL_STAFF_ROLE.Where(x => x.STAFFROLECODE == user.staffRole).Select(m => m.STAFFROLEID).FirstOrDefault();

                    }
                    else
                    {
                        user.staffRole = null;
                        user.staffRoleId = null;
                    }
                }
                else
                {
                    user.staffRole = null;
                    user.staffRoleId = null;
                }
               
            }
            return user;

        }
       
        public Users GetActiveDirectoryDetails(string userName, string loginUser, string password)
        {
           var appSetup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            Users lstADUsers = new Users();


                using (var pc = new PrincipalContext(ContextType.Domain, appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, loginUser, password))
                {
                    using (var foundUser = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, userName))
                    {
                        if (foundUser != null)
                        {
                           
                            DirectoryEntry directoryEntry = foundUser.GetUnderlyingObject() as DirectoryEntry;
                            lstADUsers.firstName = foundUser.GivenName;
                            lstADUsers.middleName = foundUser.MiddleName;
                            lstADUsers.lastName = foundUser.Surname;
                            lstADUsers.fullName = foundUser.DisplayName;
                            //lstADUsers.firstName = directoryEntry.Properties["givenName"].Value.ToString();
                            //lstADUsers.middleName = directoryEntry.Properties["middleName"].Value.ToString();
                            //lstADUsers.lastName = directoryEntry.Properties["sn"].Value.ToString();
                            //lstADUsers.fullName = directoryEntry.Properties["displayName"].Value.ToString();

                            //many details
                                                  
                    }
                   
                    }
                }


            return lstADUsers;

        }

    }

    public enum UserAccountLockStatusEnum
    {
        Locked = 1,
        Unlocked = 2
    }
}