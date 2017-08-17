using FintrakBanking.Interfaces.Admin;
using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Setups.General;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Common;
using System.Linq;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Setups.Approval;

namespace FintrakBanking.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private FinTrakBankingContext context;
        private IWorkFlowRepository workFlow;
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;
        private IApprovalLevelStaffRepository level;

        public AdminRepository(FinTrakBankingContext _context,
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup,
            IWorkFlowRepository _workFlow,
            IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            workFlow = _workFlow;
            level = _level;
        }

        #region Users
        public bool iSUserExit(string username)
        {
            return context.tbl_Profile_User.Any(x => x.Username.ToLower() == username.ToLower());
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)Operations.UserCreation;

            var response = workFlow.GoForApproval(entity);

            if (response.Result.Item1)
            {
                return ApproveUser(entity.targetId, response.Result.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveUser(int userid, short approvalStatusId, UserInfo user)
        {
            var userRecord = context.tbl_Profile_User.Find(userid);

            userRecord.IsActive = true;
            userRecord.IsLocked = false;
            userRecord.ApprovalStatus = true;
            userRecord.DateApproved = DateTime.Now;
            userRecord.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.UserApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved user '{userRecord.Username}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.context.SaveChanges() > 0;
        }

        public async Task<bool> CreateUser(AppUserViewModel user)
        {
            int output;

            List<tbl_Profile_UserGroup> userGroups = new List<tbl_Profile_UserGroup>();
            List<tbl_Profile_AdditionalActivity> userActivities = new List<tbl_Profile_AdditionalActivity>();

            var _user = new tbl_Profile_User()
            {
                StaffId = user.staffId,
                Username = user.username,
                Password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey),
                IsFirstLoginAttempt = false,
                IsActive = false,
                IsLocked = true,
                FailedLogonAttempt = 0,
                SecurityQuestion = user.securityQuestion,
                SecurityAnswer = user.securityAnswer,
                NextPasswordChangeDate = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                CreatedBy = user.createdBy,
                LastUpdatedBy = user.createdBy,
                DateTimeCreated = DateTime.Now,
                ApprovalStatus = false,
                //tbl_Profile_UserGroup = userGroups,
                //tbl_Profile_AdditionalActivity = userActivities
            };

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.UserAdded,
                StaffId = (int)user.createdBy,
                BranchId = (short)user.userBranchId,
                Detail = $"Added User with username: '{user.username}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };


            if (workFlow.CheckRouteForOperation((int)Operations.UserCreation, user.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.tbl_Profile_User.Add(_user);
                        auditTrail.AddAuditTrail(audit);

                        if (user.group.Count > 0)
                        {
                            foreach (var item in user.group)
                            {
                                var grpItem = new tbl_Profile_UserGroup()
                                {
                                    GroupId = item.groupId,
                                    UserId = _user.UserId,
                                    DateTimeCreated = DateTime.Now,
                                    CreatedBy = user.createdBy
                                };

                                var targetGroupName = context.tbl_Profile_Group.Where(x => x.GroupId == grpItem.GroupId).FirstOrDefault();

                                audit.Detail = audit.Detail + $" added to group: '{targetGroupName.GroupName}' ";
                                //end of Audit section -------------------------------
                                auditTrail.AddAuditTrail(audit);

                                context.tbl_Profile_UserGroup.Add(grpItem);
                            }
                        }

                        if (user.activities.Any())
                        {
                            foreach (var item in user.activities)
                            {
                                var userActivity = new tbl_Profile_AdditionalActivity()
                                {
                                    ActivityId = item.activityId,
                                    UserId = _user.UserId,
                                    CanAdd = false,
                                    CanEdit = false,
                                    CanApprove = false,
                                    CanDelete = false,
                                    CanView = false,
                                    CreatedBy = user.createdBy,
                                    DateTimeCreated = DateTime.Now
                                };

                                context.tbl_Profile_AdditionalActivity.Add(userActivity);
                            }
                        }

                        output = await context.SaveChangesAsync();

                        var entity = new ApprovalViewModel
                        {
                            staffId = user.createdBy,
                            companyId = user.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = _user.StaffId,
                            operationId = (int)Operations.UserCreation,
                            BranchId = user.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }

            return output != 0;
        }

        public IEnumerable<UserViewModel> GetUsersAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)Operations.UserCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from c in context.tbl_Profile_User
                        join br in context.tbl_Branch on c.tbl_Staff.BranchId equals br.BranchId
                        join st in context.tbl_Staff on c.StaffId equals st.StaffId
                        join coy in context.tbl_Company on br.CompanyId equals coy.CompanyId
                        join dept in context.tbl_Department on c.tbl_Staff.DepartmentId equals dept.DepartmentId
                        join atrail in context.tbl_Approval_Trail on c.StaffId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && c.ApprovalStatus == false
                              && atrail.OperationId == (int)Operations.UserCreation && atrail.ToApprovalLevelId == staffApprovalLevelId
                        select new UserViewModel()
                        {
                            user_id = c.UserId,
                            companyId = coy.CompanyId,
                            companyName = coy.Name,
                            branchId = br.BranchId,
                            branchName = br.BranchName,
                            username = c.Username,
                            email = st.Email,
                            staffName = st.FirstName + " " + st.LastName,
                            IsFirstLoginAttempt = c.IsFirstLoginAttempt,
                            isActive = c.IsActive,
                            IsLocked = c.IsLocked,
                            failedLogonAttempt = c.FailedLogonAttempt,
                            securityQuestion = c.SecurityQuestion,
                            securityAnswer = c.SecurityAnswer,
                            createdBy = c.CreatedBy,
                            lastUpdatedBy = c.CreatedBy,
                            dateTimeCreated = c.DateTimeCreated,
                            approvalStatus = c.ApprovalStatus,
                            approvalStatusId = atrail.ApprovalStatusId,
                            operationId = atrail.OperationId,
                            groupId = c.tbl_Profile_UserGroup.Where(x => x.UserId == c.UserId).Select(x => new UserGroupId
                            {
                                groupId = x.GroupId,
                                groupKey = x.tbl_Profile_Group.GroupName
                            }).ToList(),
                            activities = c.tbl_Profile_AdditionalActivity.Where(x => x.UserId == c.UserId).Select(a => new UserActivities
                            {
                                activityId = a.ActivityId,
                                userId = a.UserId,
                                activityName = a.tbl_Profile_Activity.ActivityName
                            }).ToList()
                        });

            return data;
        }
        public IEnumerable<ApprovalStatusViewModel> GetApprovalStatus()
        {
            return from ap in context.tbl_Approval_Status
                   select new ApprovalStatusViewModel
                   {
                       approvalStatusId = ap.ApprovalStatusId,
                       approvalStatusName = ap.ApprovalStatusName,
                       forDisplay = ap.ForDisplay,
                   };
        }

        public IEnumerable<UserViewModel> GetAllUsers()
        {
            return (from u in context.tbl_Profile_User
                    join st in context.tbl_Staff
                    on u.StaffId equals st.StaffId
                    where u.ApprovalStatus == true
                    select new UserViewModel()
                    {
                        user_id = u.UserId,
                        staffId = u.StaffId,
                        username = u.Username,
                        isActive = u.IsActive,
                        staffName = st.FirstName + " " + st.MiddleName + " " + st.LastName,
                        email = st.Email,
                        securityQuestion = u.SecurityQuestion,
                        securityAnswer = u.SecurityAnswer,
                        groupId = u.tbl_Profile_UserGroup.Where(x => x.UserId == u.UserId)
                                    .Select(x => new UserGroupId
                                    {
                                        groupId = x.GroupId,
                                        groupKey = x.tbl_Profile_Group.GroupName
                                    }).ToList(),
                        activities = context.tbl_Profile_AdditionalActivity.Where(x => x.UserId == u.UserId)
                                     .Select(a => new UserActivities
                                     {
                                         activityId = a.ActivityId,
                                         userId = a.UserId
                                     }).ToList()
                    });
        }

        public UserViewModel GetSingleUser(int userId)
        {
            throw new NotImplementedException();
        }

        public UserViewModel GetSingleUserByUserName(string userName)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Group

        public IEnumerable<AppGroupViewModel> GetAllGroups()
        {
            return context.tbl_Profile_Group.Select(x => new AppGroupViewModel
            {
                groupId = x.GroupId,
                groupName = x.GroupName
            });
        }

        public AppGroupViewModel GetSingleGroup(int groupId)
        {
            return context.tbl_Profile_Group.Where(g => g.GroupId == groupId).Select(x => new AppGroupViewModel
            {
                groupId = x.GroupId,
                groupName = x.GroupName
            }).First();

            //var tt = context.TblApprovalGroup.FromSql("[sp_getGroup] @p0, @p1", parameters: new[] { groupId, groupId });
        }

        public bool iSGroupExist(string groupName)
        {
            return context.tbl_Profile_Group.Any(x => x.GroupName.ToLower() == groupName);
        }

        public async Task<bool> AddGroup(AppGroupViewModel group)
        {
            var newGroup = new tbl_Profile_Group()
            {
                GroupName = group.groupName,
                CreatedBy = group.createdBy,
                DateTimeCreated = DateTime.Now
            };

            this.context.tbl_Profile_Group.Add(newGroup);
            var response = await context.SaveChangesAsync();
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.UserGroupAdded,
                StaffId = (int)group.createdBy,
                BranchId = (short)group.userBranchId,
                Detail = $"Added User group: '{group.groupName}' ",
                IPAddress = group.userIPAddress,
                Url = group.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return response > 0;
        }

        public async Task<bool> UpdateGroup(short groupId, AppGroupViewModel groupModel)
        {
            var targetGroup = context.tbl_Profile_Group.Find(groupId);

            targetGroup.GroupName = groupModel.groupName;
            targetGroup.DateTimeUpdated = DateTime.Now;
            targetGroup.LastUpdatedBy = groupModel.createdBy;

            var response = await context.SaveChangesAsync();
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.UserGroupUpdated,
                StaffId = (int)groupModel.createdBy,
                BranchId = (short)groupModel.userBranchId,
                Detail = $"Added User group: '{groupModel.groupName}' ",
                IPAddress = groupModel.userIPAddress,
                Url = groupModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return response != 0;
        }



        #endregion

        #region Activies

        public IEnumerable<ActivityParent> GetActivities()
        {
            return from p in context.tbl_Profile_Activity_Parent
                   select new ActivityParent
                   {
                       activityParentId = p.ActivityParentId,
                       activityParentName = p.ActivityParentName,
                       activities = context.tbl_Profile_Activity
                                      .Where(x => x.ActivityParentId == p.ActivityParentId)
                                      .Select(x => new ActivityViewModel
                                      {
                                          activityId = x.ActivityId,
                                          activityName = x.ActivityName,
                                          activityParentId = x.ActivityParentId
                                      }).ToList()
                   };

        }

        public IEnumerable<GroupVModel> GetGroupActivities()
        {
            var grpIds = context.tbl_Profile_Group_Activity.Select(x => x.GroupId).Distinct().ToList();
            var data = (from g in context.tbl_Profile_Group
                        where grpIds.Contains(g.GroupId)
                        select new GroupVModel
                        {
                            groupId = g.GroupId,
                            name = g.GroupName,
                            activities = (from ga in context.tbl_Profile_Group_Activity
                                          join act in context.tbl_Profile_Activity
                                          on ga.ActivityId equals act.ActivityId
                                          where ga.GroupId == g.GroupId
                                          select new GroupActivitiesModel
                                          {
                                              activityId = ga.ActivityId,
                                              groupActivityId = ga.GroupActivityId,
                                              activityName = act.ActivityName,
                                              canAdd = ga.CanAdd.Value,
                                              canApprove = ga.CanApprove.Value,
                                              canDelete = ga.CanDelete.Value,
                                              canEdit = ga.CanEdit.Value,
                                              canView = ga.CanView.Value
                                          }).ToList()

                        });
            return data;

        }

        public bool AddAccessToActivity(int id, ActivitiesUpdateVm model)
        {
            var targetActivity = context.tbl_Profile_Group_Activity.Find(id);
            targetActivity.CanAdd = model.canAdd;
            targetActivity.CanApprove = model.canApprove;
            targetActivity.CanView = model.canView;
            targetActivity.CanEdit = model.canEdit;
            targetActivity.CanDelete = model.canDelete;

            return context.SaveChanges() > 0;
        }

        public async Task<bool> UpdateUser(int userId, AppUserViewModel user)
        {
            var targetUser = context.tbl_Profile_User.Find(userId);
            if (targetUser.ApprovalStatus == true)
            {
                var targetGroups = context.tbl_Profile_UserGroup.Where(x => x.UserId == userId).ToList();
                var targetActivities = context.tbl_Profile_AdditionalActivity.Where(x => x.UserId == userId).ToList();
                if (targetGroups.Any())
                {
                    foreach (var item in targetGroups)
                    {
                        context.tbl_Profile_UserGroup.Remove(item);
                    }
                }

                if (targetActivities.Any())
                {
                    foreach (var item in targetActivities)
                    {
                        context.tbl_Profile_AdditionalActivity.Remove(item);
                    }
                }

                if (user.group.Count > 0)
                {
                    foreach (var item in user.group)
                    {
                        var grpItem = new tbl_Profile_UserGroup()
                        {
                            GroupId = item.groupId,
                            UserId = userId,
                            DateTimeCreated = DateTime.Now,
                            CreatedBy = user.createdBy
                        };

                        context.tbl_Profile_UserGroup.Add(grpItem);
                    }
                }

                if (user.activities.Any())
                {
                    foreach (var item in user.activities)
                    {
                        var userActivity = new tbl_Profile_AdditionalActivity()
                        {
                            ActivityId = item.activityId,
                            UserId = userId,
                            CreatedBy = user.createdBy,
                            DateTimeCreated = DateTime.Now
                        };

                        context.tbl_Profile_AdditionalActivity.Add(userActivity);
                    }
                }
                var response = await context.SaveChangesAsync();
                return response != 0;
            }
            else
            {
                return false;
            }
        }

        public List<string> GetUserActivities(int userId)
        {
            var userGroupIds = context.tbl_Profile_UserGroup.Where(x => x.UserId == userId)
                                .Select(x => x.GroupId).ToList();

            var activities = (from grpAct in context.tbl_Profile_Group_Activity
                              join act in context.tbl_Profile_Activity on grpAct.ActivityId
                             equals act.ActivityId
                              where userGroupIds.Contains(grpAct.GroupId)
                              select act.ActivityName.ToLower()).ToList();

            var additionalActivities = (from addAct in context.tbl_Profile_AdditionalActivity
                                        join act in context.tbl_Profile_Activity
                                        on addAct.ActivityId equals act.ActivityId
                                        where addAct.UserId == userId
                                        select act.ActivityName.ToLower()).ToList();

            if (additionalActivities.Any())
            {
                return activities.Concat(additionalActivities).Distinct().ToList();
            }
            else
            {
                return activities.Distinct().ToList();
            }

        }

        #endregion
    }
}
