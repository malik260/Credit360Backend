using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
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
            var _user = new TBL_PROFILE_USER()
            {
                STAFFID = user.staffId,
                USERNAME = user.username,
                PASSWORD = user.password.EncryptSha512(StaticHelpers.EncryptionKey),
                ISFIRSTLOGINATTEMPT = false,
                ISACTIVE = true,
                ISLOCKED = false,
                FAILEDLOGONATTEMPT = 0,
                SECURITYQUESTION = user.securityQuestion,
                SECURITYANSWER = user.securityAnswer,
                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),/// int.Parse(config["AppConstants:PasswordExpiredDays"])),
                CREATEDBY = user.createdBy ?? 0,
                LASTUPDATEDBY = user.createdBy ?? 0,
                DATETIMECREATED = DateTime.Now
            };

            context.TBL_PROFILE_USER.Add(_user);
            if (user.groupId.Count > 0)
            {
                foreach (var grp in user.groupId)
                {
                    var grpItem = new TBL_PROFILE_USERGROUP()
                    {
                        GROUPID = grp.groupId,
                        USERID = _user.USERID,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = _user.CREATEDBY ?? 0
                    };

                    context.TBL_PROFILE_USERGROUP.Add(grpItem);
                }
            }
            //}

            var response = await context.SaveChangesAsync();

            return response != 0;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var targetUser = context.TBL_PROFILE_USER.Find(userId);

            context.TBL_PROFILE_USER.Remove(targetUser ?? throw new InvalidOperationException());

            var response = await context.SaveChangesAsync();

            return response != 0;
        }

        public async Task<bool> UpdateUser(int userId, UserViewModel user)
        {
            bool result = false;
            try
            {
                var targetUser = context.TBL_PROFILE_USER.Find(userId);
                if (targetUser == null)
                {
                    return false;
                }

                targetUser.USERNAME = user.username;
                var response = await context.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
        public UserViewModel FindUserByUserName(string username)
        {
            var _user = context.TBL_PROFILE_USER.FirstOrDefault(x => x.USERNAME == username );

            if (_user != null)
            {
                try
                {
                    var data = (from p in context.TBL_PROFILE_USER
                                join st in context.TBL_STAFF on p.STAFFID equals st.STAFFID
                                join br in context.TBL_BRANCH on st.BRANCHID equals br.BRANCHID
                                join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                                where p.USERNAME == username
                                select new UserViewModel
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

                                }).First();

                    if (data == null)
                    {
                        _user.FAILEDLOGONATTEMPT += 1;

                        context.SaveChanges();
                    }

                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return null;
        }


        public UserViewModel FindUserByUserNameAndPassword(string username, string password)
        {
            var _user = context.TBL_PROFILE_USER.FirstOrDefault(x => x.USERNAME == username); // && x.PASSWORD == password);

            var appSetup = context.TBL_SETUP_GLOBAL.Single();

            UserViewModel data;

            if (_user != null)
            {
                try
                {
                    if (appSetup.USE_ACTIVE_DIRECTORY)
                    {
                        data = FindUserByUserName(username);
                    }
                    else
                    {
                        data = (from p in context.TBL_PROFILE_USER
                                join st in context.TBL_STAFF on p.STAFFID equals st.STAFFID
                                join br in context.TBL_BRANCH on st.BRANCHID equals br.BRANCHID
                                join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                                where p.USERNAME == username && p.PASSWORD == password
                                select new UserViewModel
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

                                }).First();
                    }

                    if (data == null)
                    {
                        _user.FAILEDLOGONATTEMPT += 1;

                        context.SaveChanges();
                    }

                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return null;
        }

        public bool IsUserExits(string username)
        {
            return context.TBL_PROFILE_USER.Any(x => x.USERNAME.ToLower() == username);
        }

        public bool IsUserAccountValid(string username)
        {
            var isValid = GetAllUsers().FirstOrDefault(x => x.username.ToLower() == username.ToLower() && x.isLocked == false && x.isActive == true);

            if (isValid != null)
            {
                return true;
            }

            return false;
        }

        public IEnumerable<TBL_PROFILE_GROUP> GetAllGroups()
        {
            return context.TBL_PROFILE_GROUP;
        }

        public IEnumerable<UserViewModel> GetAllUsers()
        {
            return (from u in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF
                    on u.STAFFID equals st.STAFFID
                    select new UserViewModel()
                    {
                        user_id = u.USERID,
                        staffId = u.STAFFID,
                        username = u.USERNAME,
                        isActive = u.ISACTIVE,
                        staffName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                        email = st.EMAIL,
                        password = u.PASSWORD,
                        securityQuestion = u.SECURITYQUESTION,
                        securityAnswer = u.SECURITYANSWER,
                        groupId = u.TBL_PROFILE_USERGROUP.Where(x => x.USERID == u.USERID)
                                    .Select(x => new UserGroupId
                                    {
                                        groupId = x.GROUPID,
                                        groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                                    }).ToList(),
                        isLocked = u.ISLOCKED,
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

    }
}