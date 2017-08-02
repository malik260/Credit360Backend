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

namespace FintrakBanking.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private FinTrakBankingContext context;

        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;

        public AdminRepository(FinTrakBankingContext _context,
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        #region Users
        public bool iSUserExit(string username)
        {
            return context.tbl_Profile_User.Any(x => x.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> CreateUser(AppUserViewModel user)
        {
            var _user = new tbl_Profile_User()
            {
                StaffId = user.staffId,
                Username = user.username,
                Password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey),
                IsFirstLoginAttempt = false,
                IsActive = true,
                IsLocked = false,
                FailedLogonAttempt = 0,
                SecurityQuestion = user.securityQuestion,
                SecurityAnswer = user.securityAnswer,
                NextPasswordChangeDate = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                CreatedBy = user.createdBy,
                LastUpdatedBy = user.createdBy,
                DateTimeCreated = DateTime.Now
            };

            context.tbl_Profile_User.Add(_user);
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.UserAdded,
                StaffId = (int)user.createdBy,
                BranchId = (short)user.userBranchId,
                Detail = $"Added User with username: '{user.username}' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------

            if (user.group.Count > 0)
            {
                foreach (var item in user.group)
                {
                    var grpItem = new tbl_Profile_UserGroup()
                    {
                        GroupId = item.groupId,
                        UserId = _user.UserId,
                        DateTimeCreated = DateTime.UtcNow,
                        CreatedBy = user.createdBy
                    };
                    // Audit Section Contd.---------------------------
                    audit.Detail = audit.Detail + " added to group: '{user.group}' ";
                    //end of Audit section -------------------------------
                    this.auditTrail.AddAuditTrail(audit);
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
                        CreatedBy = user.createdBy,
                        DateTimeCreated = DateTime.Now
                    };

                    context.tbl_Profile_AdditionalActivity.Add(userActivity);
                }
            }

            var response = await context.SaveChangesAsync();

            return response != 0;
        }

        public IEnumerable<UserViewModel> GetAllUsers()
        {
            return (from u in context.tbl_Profile_User
                    join st in context.tbl_Staff
                    on u.StaffId equals st.StaffId
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
            targetGroup.DateTimeUpdated = DateTime.UtcNow;
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
                        DateTimeCreated = DateTime.UtcNow,
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
