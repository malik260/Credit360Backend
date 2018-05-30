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
using static FintrakBanking.Repositories.Credit.LoanApplicationRepository;

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
            try
            {
                var targetUser = context.TBL_PROFILE_USER.Find(userId);
                if (targetUser == null)
                {
                    return false;
                }

                targetUser.USERNAME = user.username;
                var response = await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<UserViewModel> FindUserByUserNameAsync(string username)
        {
            var result = _sessionInfo;

            if (result.state > 0)
                result = new SessionStatusInfo
                {
                    loginCode = Guid.NewGuid(),
                    state = 0,
                    errorMessage = "",
                };

            var user = context.TBL_PROFILE_USER.FirstOrDefault(x => x.USERNAME.ToLower() == username);

            if (user != null)
            {
                var data = (from p in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF on p.STAFFID equals st.STAFFID
                    join br in context.TBL_BRANCH on st.BRANCHID equals br.BRANCHID
                    join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                    where p.USERNAME.ToLower() == username.ToLower()
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
                        companyName = coy.NAME
                    }).FirstOrDefault();

                if (data == null)
                {
                    int count = user.FAILEDLOGONATTEMPT ?? 0;
                    if (count == CommonHelpers.MaxInvalidPasswordAttempts)
                    {
                        user.ISLOCKED = true;
                        user.LASTLOCKOUTDATE = DateTime.Now;
                    }

                    user.LOGINCODE = null;
                    user.FAILEDLOGONATTEMPT += 1;
                }
                else
                {
                    user.LASTLOGINDATE = DateTime.Now;
                    user.LOGINCODE = result.loginCode.ToString();
                }

                context.SaveChanges();
                
                return data;

            }

            throw new Exception("1001 Incorrect username or password.");

            //return null;
        }

        public async Task<SessionStatusInfo> CheckSessionState(string username)
        {
            Guid loginCode = Guid.Empty;
            var user = await context.TBL_PROFILE_USER.FirstOrDefaultAsync(x => x.USERNAME.ToLower() == username); // && x.PASSWORD == password);
            SessionStatusInfo result = null;

            if (user != null)
            {
                if (user.LOGINCODE == null || user.LOGINCODE == Guid.Empty.ToString())
                    result = new SessionStatusInfo
                    {
                        loginCode = Guid.NewGuid(),
                        state = 0,
                        errorMessage = "",

                    };

                else if (user.LOGINCODE != null)
                {
                    int timeStamp = (DateTime.Now - user.LASTLOCKOUTDATE.Value.Date).Minutes;
                    if (timeStamp < 2 && user.LOGINCODE != Guid.Empty.ToString())
                    {
                        result = new SessionStatusInfo
                        {
                            loginCode = Guid.Parse(user.LOGINCODE),
                            state = 0,
                            errorMessage = "",
                        };
                    }
                    else
                    {
                        result = new SessionStatusInfo
                        {
                            loginCode = Guid.Parse(user.LOGINCODE),
                            state = 1,
                            errorMessage = "You are already logged.",
                        };
                    }
                }
            }
            else
            {
                result = new SessionStatusInfo
                {
                    loginCode = Guid.Parse(user.LOGINCODE),
                    state = 1,
                    errorMessage = "You are already logged.",
                };
            }

            return result;
        }



        private SessionStatusInfo _sessionInfo;

        public SessionStatusInfo SessionInfo
        {
            get => _sessionInfo;
            set => _sessionInfo = value;
        }

        public async Task<UserViewModel> FindUserByUserNameAndPassword(string username, string password)
        {
            UserViewModel data = null;
            var result = _sessionInfo;
            data = UserLoginDetails(username, password);
            if (result.state > 0)
            {
                if (data == null)
                {
                    throw new Exception("1001 Incorrect username or password.");
                }
                data.sessionStatusInfo = result;
            }
            data.sessionStatusInfo = result;
            return data;
        }
       
        public async Task<bool> IsAccountLocked(string userName)
        {
            var data = await context.TBL_PROFILE_USER.FirstOrDefaultAsync(c => c.USERNAME.ToLower() == userName);
            if (data != null)
            {

                return data.ISLOCKED;
            }
            throw new Exception("1001 Incorrect username or password.");
        }

        public async Task<bool> IsAccountActive(string userName)
        {
            var data = await context.TBL_PROFILE_USER.FirstOrDefaultAsync(c => c.USERNAME.ToLower() == userName);
            if (data != null)
            {
                return data.ISACTIVE;
            }

            throw new Exception("1001 Incorrect username or password.");
        }

        private UserViewModel UserLoginDetails(string username, string password)
        {
            var data = context.TBL_PROFILE_USER.Where(c => c.USERNAME.ToLower() == username && c.PASSWORD == password);

            
                if (data.Any())
                {
                    return data.Select(c => new UserViewModel
                    {
                        companyId = c.TBL_STAFF.COMPANYID,
                        staffId = c.STAFFID,
                        user_id = c.USERID,
                        username = c.USERNAME,
                        staffName = c.TBL_STAFF.FIRSTNAME + " " + c.TBL_STAFF.MIDDLENAME + " " + c.TBL_STAFF.LASTNAME,
                        branchId = c.TBL_STAFF.BRANCHID.Value,
                        countryId = c.TBL_STAFF.TBL_COMPANY.COUNTRYID,
                        branchName = context.TBL_BRANCH.FirstOrDefault(d => d.BRANCHID == c.TBL_STAFF.BRANCHID.Value).BRANCHNAME,
                        companyName = c.TBL_STAFF.TBL_COMPANY.NAME,
                        logincode = c.LOGINCODE,
                        lastLoginDate = c.LASTLOGINDATE


                    }).FirstOrDefault();
                }
            else
            {
                var record = data.FirstOrDefault();
                if (record == null)
                {
                    var faileddata = context.TBL_PROFILE_USER.FirstOrDefault(c => c.USERNAME.ToLower() == username);
                    if (faileddata != null)
                    {
                        int count = faileddata.FAILEDLOGONATTEMPT ?? 0;
                        if (count == CommonHelpers.MaxInvalidPasswordAttempts)
                        {
                            faileddata.ISLOCKED = true;
                            faileddata.LASTLOCKOUTDATE = DateTime.Now;
                        }

                        faileddata.LOGINCODE = null;
                        faileddata.FAILEDLOGONATTEMPT += 1;
                        context.SaveChanges();

                        throw new Exception("1001 Incorrect username or password.");
                    }
                    else
                    {
                        throw new Exception("1001 Incorrect username or password.");
                    }

                }

                return null;
            }


        }

        public bool IsUserExisting(string username)
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
                    where u.USERNAME.ToLower() == userName.ToLower() && u.ISACTIVE && !u.ISLOCKED
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

        public bool ClearLoginToken(string userName)
        {
            bool result = false;
            var _user = context.TBL_PROFILE_USER.FirstOrDefault(x => x.USERNAME.ToLower() == userName);
            if (_user != null)
            {
                _user.LOGINCODE = null;
                result = context.SaveChanges() > 0;
            }
            return result;
        }

        public List<string> GetUserActivitiesByUser(int userId)
        {
            List<string> activities = new List<string>();
            var user = context.TBL_PROFILE_USER.Find(userId);

            var additionalActivityIds = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == userId).Select(x => x.ACTIVITYID);

            var roleActivityIds = context.TBL_PROFILE_STAFF_ROLE_ADT_ACT.Where(x => x.STAFFROLEID == user.TBL_STAFF.STAFFROLEID).Select(x => x.ACTIVITYID);

            var userGroupActivityIds = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == userId)
                .Select(x => x.TBL_PROFILE_GROUP)
                .SelectMany(x => x.TBL_PROFILE_GROUP_ACTIVITY)
                .Select(x => x.ACTIVITYID);

            var roleGroupActivityIds = user.TBL_STAFF.TBL_STAFF_ROLE.TBL_PROFILE_STAFF_ROLE_GROUP
                .Select(x => x.TBL_PROFILE_GROUP)
                .SelectMany(x => x.TBL_PROFILE_GROUP_ACTIVITY)
                .Select(x => x.ACTIVITYID);

            activities = context.TBL_PROFILE_ACTIVITY.Where(x => additionalActivityIds.Contains(x.ACTIVITYID)
                || roleActivityIds.Contains(x.ACTIVITYID)
                || userGroupActivityIds.Contains(x.ACTIVITYID)
                || roleGroupActivityIds.Contains(x.ACTIVITYID)
            ).Select(x => x.ACTIVITYNAME).ToList();

            //var test1 = userGroupActivityIds.ToList();
            //var test2 = roleGroupActivityIds.ToList();

            return activities;
        }

    }
}