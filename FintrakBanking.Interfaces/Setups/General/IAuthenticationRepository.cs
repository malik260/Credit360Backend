using System;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IAuthenticationRepository
    {
        IEnumerable<UserViewModel> GetAllUsers();

        UserViewModel GetSingleUser(int userId);

        UserViewModel GetSingleUserByUserName(string userName);

        Task<bool> CreateUser(UserViewModel user);

        Task<bool> DeleteUser(int userId);

        Task<bool> UpdateUser(int userId, UserViewModel user);

        UserViewModel FindUserByUserNameAndPassword(string username, string password);

        bool IsUserExits(string username);

        bool IsUserAccountValid(string username);

        // Groups

        IEnumerable<tbl_Profile_Group> GetAllGroups();

    }
}