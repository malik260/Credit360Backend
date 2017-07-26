using FintrakBanking.ViewModels.Admin;
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

        List<string> GetUserActivities(int userId);

        UserViewModel GetSingleUser(int userId);

        UserViewModel GetSingleUserByUserName(string userName);

        bool iSUserExit(string username);

        Task<bool> CreateUser(AppUserViewModel user);

        Task<bool> UpdateUser(int userId, AppUserViewModel user);
        #endregion Users


        #region Group

        IEnumerable<AppGroupViewModel> GetAllGroups();

        AppGroupViewModel GetSingleGroup(int groupId);

        bool iSGroupExist(string groupName);

        Task<bool> AddGroup(AppGroupViewModel group);

        Task<bool> UpdateGroup(short groupId, AppGroupViewModel groupModel);

        #endregion

        #region Activity

        IEnumerable<ActivityParent> GetActivities();

        IEnumerable<GroupVModel> GetGroupActivities();

        bool AddAccessToActivity(int id, ActivitiesUpdateVm model);

        #endregion

    }
}
