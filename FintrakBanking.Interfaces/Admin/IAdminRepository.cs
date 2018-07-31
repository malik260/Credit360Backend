using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Admin
{
    public interface IAdminRepository
    {
              #region Users
        IEnumerable<UserViewModel> GetAllUsers();

        UserViewModel GetUsersByStaffId(int staffId);


        UserViewModel GetSingleUser(int userId);

        UserViewModel GetSingleUserByUserName(string userName);

        bool isUserExist(string username);

        bool isStaffExist(long staffId);

        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<UserViewModel> GetUsersAwaitingApproval(int staffId, int companyId);
        IEnumerable<ApprovalStatusViewModel> GetApprovalStatus();

        bool CreateUser(AppUserViewModel user);

        bool UpdateUser(int userId, AppUserViewModel user);

        //   Object ManageUserAccount(int userId, int lockStatus);

        #endregion Users

        #region Group

        IEnumerable<AppGroupViewModel> GetAllGroups();

        AppGroupViewModel GetSingleGroup(int groupId);

        bool isGroupExist(string groupName);

        bool AddGroup(AppGroupViewModel group);

        bool UpdateGroup(short groupId, AppGroupViewModel groupModel);
        //Task<bool> AddGroup(AppGroupViewModel group);

        // Task<bool> UpdateGroup(short groupId, AppGroupViewModel groupModel);

        #endregion

        #region Activity

        IEnumerable<ActivityParent> GetActivities();

        IEnumerable<GroupVModel> GetGroupActivities();

        bool AddAccessToActivity(int id, ActivitiesUpdateVm model);

        #endregion

        bool UpdateUserStatus(ActiveUserDetails entity, out string message);
        IEnumerable<ActiveUserDetails> GetActiveUsers(int companyId);


        #region Two Factor Authentication
        bool TwoFactorAuthentication(string staffCode, string passCode);
        bool TwoFactorAuthenticationEnabled();
        bool Enable2FAForLastApproval(int staffId, int operationId, int? productClassId, int? productId);
        #endregion
    }
}
