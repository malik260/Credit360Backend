using System;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using System.Linq;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IAuthenticationRepository
    {
        IQueryable<UserViewModel> GetAllUsers();

        UserViewModel GetSingleUser(int userId);

        UserViewModel GetSingleUserByUserName(string userName);

        Task<bool> CreateUser(UserViewModel user);

        Task<bool> DeleteUser(int userId);

        bool IsAccountLocked(string userName);

        SessionStatusInfo CheckSessionState(string username, string ipAddress);

        SessionStatusInfo SessionInfo { get; set; }

        Task<bool> UpdateUser(int userId, UserViewModel user);

        bool IsAccountActive(string userName);

        bool ResumptionClosignTime(string userName, bool log);

        UserViewModel FindUserByUserNameAndPassword(string username, string password);

        Task<UserViewModel> FindUserByUserNameAsync(string username);

        bool IsUserExisting(string username);

        bool IsUserAccountValid(string username);

        List<string> GetUserActivitiesByUser(int userId);

        // Groups

        IEnumerable<TBL_PROFILE_GROUP> GetAllGroups();

        bool ClearLoginToken(string userName);

        LookupViewModel GetDashboardStaffRole(int staffId);

        bool PasswordChange(PasswordChangeViewModel pwdChange);

        bool ValidatePasswordPolicy(string password);

        bool ValidateOldPassword(string username, string oldPassword);

        UserViewModel GetUserLoginInfoByUserName(string userName);

    }
}