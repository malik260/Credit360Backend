using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General; 
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
   
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private FinTrakBankingContext context;
       

        public AuthenticationRepository(FinTrakBankingContext _context)
        {
            this.context = _context; 
        }

        public async Task<bool> CreateUser(UserViewModel user)
        {
            //if (user.createdBy != null)
            //{
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
                NextPasswordChangeDate = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),/// int.Parse(config["AppConstants:PasswordExpiredDays"])),
                CreatedBy = user.createdBy ?? 0,
                LastUpdatedBy = user.createdBy ?? 0,
                DateTimeCreated = DateTime.Now
            };

              context.tbl_Profile_User.Add(_user);
            if (user.groupId.Count > 0)
            {
                foreach (var grp in user.groupId)
                {
                    var grpItem = new tbl_Profile_UserGroup()
                    {
                        GroupId = grp.groupId,
                        UserId = _user.UserId,
                        DateTimeCreated = DateTime.Now,
                        CreatedBy = _user.CreatedBy ?? 0
                    };

                    context.tbl_Profile_UserGroup.Add(grpItem);
                }
            }
            //}

            var response = await context.SaveChangesAsync();

            return response != 0;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var targetUser = context.tbl_Profile_User.Find(userId);

            context.tbl_Profile_User.Remove(targetUser);

            var response = await context.SaveChangesAsync();

            return response != 0;
        }

        public async Task<bool> UpdateUser(int userId, UserViewModel user)
        {
            bool result = false;
            try
            {
                int response;

                var targetUser = context.tbl_Profile_User.Find(userId);
                if (targetUser == null)
                {
                    return false;
                }

                targetUser.Username = user.username;
                response = await context.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public async Task<List<UserViewModel>> FindUserByUserNameAndPassword(string username,string password)
        {
            var _user = context.tbl_Profile_User.Where(x => x.Username == username && x.Password == password).ToList();

            if (_user.Count > 0)
            {
                return (from p in context.tbl_Profile_User
                        join st in context.tbl_Staff on p.StaffId equals st.StaffId
                        join br in context.tbl_Branch on st.BranchId equals br.BranchId
                        join coy in context.tbl_Company on br.CompanyId equals coy.CompanyId
                        where p.Username == username && p.Password == password
                        select new UserViewModel
                        {
                            companyId = coy.CompanyId,
                            staffId = p.StaffId,
                            user_id = p.UserId,
                            username = p.Username,
                            staffName = st.FirstName + " " + st.MiddleName + " " + st.LastName,
                            branchId = st.BranchId.Value,
                            countryId = coy.CountryId,
                            branchName = br.BranchName,
                            companyName=coy.Name

                        }).ToList();
            }
            else
            {
                return null;
            }
        }

        public bool IsUserExit(string username)
        {
            return context.tbl_Profile_User.Any(x => x.Username.ToLower() == username);
        }

        public IEnumerable<tbl_Profile_Group> GetAllGroups()
        {
            return context.tbl_Profile_Group;
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
                        password = u.Password,
                        securityQuestion = u.SecurityQuestion,
                        securityAnswer = u.SecurityAnswer,
                        groupId = u.tbl_Profile_UserGroup.Where(x => x.UserId == u.UserId)
                                    .Select(x => new UserGroupId
                                    {
                                        groupId = x.GroupId,
                                        groupKey = x.tbl_Profile_Group.GroupName
                                    }).ToList()
                    });
        }

        public UserViewModel GetSingleUser(int userId)
        {
            var user= (from u in context.tbl_Profile_User
                    join st in context.tbl_Staff
                    on u.StaffId equals st.StaffId
                    where u.UserId == userId
                    select new UserViewModel()
                    {
                        user_id = u.UserId,
                        staffId = u.StaffId,
                        username = u.Username,
                        isActive = u.IsActive,
                        staffName = st.FirstName + " " + st.LastName,
                        email = st.Email,
                        password = u.Password,
                        securityQuestion =u.SecurityQuestion,
                        securityAnswer = u.SecurityAnswer
                    }).SingleOrDefault();

            if (user != null)
            {

                user.groupId = context.tbl_Profile_UserGroup.Where(x => x.UserId == user.user_id)
                                        .Select(x=> new UserGroupId
                                        {
                                            groupId = x.GroupId,
                                            groupKey = x.tbl_Profile_Group.GroupName
                                        })
                            .ToList();
            }

            return user;
        }

        public UserViewModel GetSingleUserByUserName(string userName)
        {
            return (from u in context.tbl_Profile_User
                    join st in context.tbl_Staff
                    on u.StaffId equals st.StaffId
                    where u.Username == userName
                    select new UserViewModel()
                    {
                        user_id = u.UserId,
                        staffId = u.StaffId,
                        username = u.Username,
                        isActive = u.IsActive,
                        staffName = st.FirstName + " " + st.MiddleName + " " + st.LastName,
                        email = st.Email
                    }).FirstOrDefault();
        }
    }
}