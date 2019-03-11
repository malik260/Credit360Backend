using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FintrakBanking.Repositories.Credit.LoanApplicationRepository;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using FintrakBanking.Repositories.Admin;

namespace FintrakBanking.Repositories.Setups.General
{

    public class AuthenticationRepository : IAuthenticationRepository
    {
        private FinTrakBankingContext context;
        private  IAuditTrailRepository _auditTrail;
        private SessionStatusInfo _sessionInfo;

        TBL_PROFILE_SETTING profileSetting = null;

        public string LogCode { get; set; }

        public AuthenticationRepository(FinTrakBankingContext _context, IAuditTrailRepository auditTrail)
        {
            context = _context;
            _auditTrail = auditTrail != null ? auditTrail : new AuditTrailRepository(_context);
        }

        private TBL_PROFILE_SETTING GetProfileSettings()
        {
            if (profileSetting != null) return profileSetting;
            return context.TBL_PROFILE_SETTING.FirstOrDefault();
        }

        public async Task<bool> CreateUser(UserViewModel user)
        {
            FinTrakBankingContext db = new FinTrakBankingContext();
            TBL_PROFILE_SETTING prosett = new TBL_PROFILE_SETTING();
            prosett = db.TBL_PROFILE_SETTING.FirstOrDefault();

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
                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(prosett.EXPIREPASSWORDAFTER),
                //NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                /// int.Parse(config["AppConstants:PasswordExpiredDays"])),
                CREATEDBY = user.createdBy ?? 0,
                LASTUPDATEDBY = user.createdBy ?? 0,
                DATETIMECREATED = DateTime.Now
            };

            db.TBL_PROFILE_USER.Add(_user);
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

                    db.TBL_PROFILE_USERGROUP.Add(grpItem);
                }
            }
            //}

            var response = await db.SaveChangesAsync();

            return response != 0;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var targetUser = context.TBL_PROFILE_USER.Find(userId);

            context.TBL_PROFILE_USER.Remove(targetUser ?? throw new InvalidOperationException());

            var response = await context.SaveChangesAsync();

            return response != 0;
        }

        public ActiveUserDetails GetUserInformation(string username)
        {
            return (from u in context.TBL_PROFILE_USER
                        join st in context.TBL_STAFF on u.STAFFID equals st.STAFFID
                        where u.USERNAME.ToLower() == username
                        select new ActiveUserDetails
                        {
                            user_id = u.USERID,
                            staffId = u.STAFFID,
                            username = u.USERNAME,
                            isActive = u.ISACTIVE,
                            deleted = st.DELETED
                        })
                    .FirstOrDefault();
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
                throw new SecureException(ex.Message);
            }
        }

        public int ConcurrentUsers()
        {
            return context.TBL_PROFILE_USER.Where(x => 
                x.ISACTIVE == true &&
                x.ISLOCKED == false &&
                x.LOGINCODE != null  
                ).Count();
        }

        public ActiveUserDetails GetUserAuthenticationInfo(string username)
        {
            username = username.ToLower();
            ActiveUserDetails result = new ActiveUserDetails();
            // var user = GetAllUsers().FirstOrDefault(c => c.username.ToLower() == username);

            var user = (from u in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF on u.STAFFID equals st.STAFFID
                    where u.USERNAME.ToLower() == username
                    select new UserViewModel
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
                        branchId = st.BRANCHID,
                        roleId = st.STAFFROLEID,
                        groupId = u.TBL_PROFILE_USERGROUP.Where(x => x.USERID == u.USERID)
                                    .Select(x => new UserGroupId
                                    {
                                        groupId = x.GROUPID,
                                        groupKey = x.TBL_PROFILE_GROUP.GROUPNAME
                                    }).ToList(),
                        isLocked = u.ISLOCKED,
                    })
                    .FirstOrDefault();

            if (user == null) throw new SecureException("The user is not registered in the application. Contact the system administrator.");

            result.grantMessage = "valid";
            if (!user.isActive) result.grantMessage = "This account is INACTIVE";
            if (user.isLocked) result.grantMessage = "This account is LOCKED";
            if (!ResumptionClosingTime(user)) result.grantMessage = "You cannot login at this time";

            if (result.grantMessage != "valid")
            {
                _auditTrail.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoginFailed,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.branchId,
                    DETAIL = $"{username} - {result.grantMessage}",
                    IPADDRESS = CommonHelpers.GetUserIP(),
                    URL = "/Token",
                    APPLICATIONDATE = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = -1
                });
                context.SaveChanges();
            }

            return result;
        }

        public async Task<UserViewModel> FindUserByUserNameAsync(string username)
        {
            FinTrakBankingContext db = new FinTrakBankingContext();

            var result = _sessionInfo;

            if (result.state > 0)
                result = new SessionStatusInfo
                {
                    loginCode = Guid.NewGuid(),
                    state = 0,
                    errorMessage = "",
                };

            var user = db.TBL_PROFILE_USER.FirstOrDefault(x => x.USERNAME.ToLower() == username);

            if (user != null)
            {
                var data = (from p in db.TBL_PROFILE_USER
                            join st in db.TBL_STAFF on p.STAFFID equals st.STAFFID
                            join br in db.TBL_BRANCH on st.BRANCHID equals br.BRANCHID
                            join coy in db.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
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
                    user.LOGINCODE = null;
                    user.FAILEDLOGONATTEMPT += 1;
                    int count = user.FAILEDLOGONATTEMPT ?? 0;
                    TBL_PROFILE_SETTING prosett = new TBL_PROFILE_SETTING();
                    prosett = db.TBL_PROFILE_SETTING.FirstOrDefault();
                    //if (count == CommonHelpers.MaxInvalidPasswordAttempts)
                    if (count > prosett.MAXINVALIDPASSWORDATTEMPTS)
                    {
                        user.ISLOCKED = true;
                        user.LASTLOCKOUTDATE = DateTime.Now;
                    }


                }
                else
                {
                    user.LASTLOGINDATE = DateTime.Now;
                    user.LOGINCODE = result.loginCode.ToString() + "@" + result.ipaddress;
                }

                db.SaveChanges();

                return data;

            }

            throw new SecureException("1001 Login Failure.");

            //return null;
        }

        public SessionStatusInfo CheckSessionState(string username, string ipAddress)
        {
            Guid loginCode = Guid.Empty;
            TBL_PROFILE_USER user = new TBL_PROFILE_USER();
            var LoginCode = (from a in context.TBL_PROFILE_USER
                    where a.USERNAME.ToLower() == username
                    select a.LOGINCODE).FirstOrDefault();


            //.FirstOrDefault(x => x.USERNAME.ToLower() == username); // && x.PASSWORD == password);
            SessionStatusInfo result = null;
            string loginCodeStr = null;
            string ipAddressStr = null;

            if (user != null)
            {
                if (LoginCode != null)
                {
                    this.LogCode = LoginCode;
                    var gcode = LoginCode.Split('@');
                    loginCodeStr = gcode[0];
                    ipAddressStr = gcode[1];
                }

                if (loginCodeStr == null || loginCodeStr == Guid.Empty.ToString())
                {
                    var gcode = Guid.NewGuid();
                    result = new SessionStatusInfo
                    {
                        loginCode = Guid.NewGuid(),
                        state = 0,
                        ipaddress = ipAddressStr,
                        errorMessage = "",
                    };
                    this.LogCode = gcode.ToString() + "@" + ipAddress;
                }
                else if (loginCodeStr != null)
                {
                    //  int timeStamp = 1;// (DateTime.Now - Convert.ToDateTime(user.LASTLOCKOUTDATE.HasValue) ).Minutes;
                    if (ipAddressStr == ipAddress && loginCodeStr != Guid.Empty.ToString())
                    {
                        this.LogCode = loginCodeStr + "@" + ipAddressStr;
                        result = new SessionStatusInfo
                        {
                            loginCode = Guid.Parse(loginCodeStr),
                            state = 0,
                            ipaddress = ipAddressStr,
                            errorMessage = "",
                        };
                    }
                    //else if (this.LogCode.Split('@')[1] != null)
                    //{
                    //    this.LogCode = loginCodeStr + "@" + ipAddressStr;
                    //    result = new SessionStatusInfo
                    //    {
                    //        loginCode = Guid.Parse(loginCodeStr),
                    //        state = 0,
                    //        ipaddress = ipAddressStr,
                    //        errorMessage = "",
                    //    };
                    //}
                    else
                    {
                        this.LogCode = loginCodeStr + "@" + ipAddressStr;
                        result = new SessionStatusInfo
                        {
                            loginCode = Guid.Parse(loginCodeStr),
                            state = 1,
                            ipaddress = ipAddressStr,
                            errorMessage = "You already have an active session.",
                        };
                    }
                }
            }
            else
            {
                result = new SessionStatusInfo
                {
                    loginCode = Guid.Parse(loginCodeStr),
                    state = 1,
                    errorMessage = "You already have an active session.",
                    ipaddress = ipAddressStr,
                };
            }

            return result;
        }

        public SessionStatusInfo SessionInfo
        {
            get => _sessionInfo;
            set => _sessionInfo = value;
        }

        public UserViewModel FindUserByUserNameAndPassword(string username, string password) // ERROR POINT 2 - 
        {
            UserViewModel data = null;
            var result = _sessionInfo;

            //try { 
            data = UserLoginDetails(username, password);
            //}catch(Exception ex)
               // {

                //}
    result.isPasswordExpired = IsPasswordExpired(username);
            result.isFirstLogin = IsFirstLogin(username);
            if (result.state > 0)
            {
                if (data == null) throw new SecureException("1001 Login Failure.");
            }
            data.sessionStatusInfo = result;
            return data;
        }

        public bool IsPasswordExpired(string userName)
        {
            FinTrakBankingContext db = new FinTrakBankingContext();
            TBL_PROFILE_SETTING prosett = new TBL_PROFILE_SETTING();
            prosett = db.TBL_PROFILE_SETTING.FirstOrDefault();
            var AD = context.TBL_SETUP_GLOBAL.FirstOrDefault();

            if (AD.USE_ACTIVE_DIRECTORY == true)
            {
                return false;
            }

            if ((bool)prosett.ENABLEPASSWORDRESET)
            {
                DateTime changeDate = (DateTime)context.TBL_PROFILE_USER.FirstOrDefault(p => p.USERNAME.ToUpper() == userName.ToUpper()).NEXTPASSWORDCHANGEDATE;
                var duration = (changeDate.Date - DateTime.Now).Days;

                if (duration >= prosett.EXPIREPASSWORDAFTER)
                    return true;
            }

            return false;
        }

        public bool IsFirstLogin(string userName)
        {
            var data = context.TBL_PROFILE_USER.FirstOrDefault(p => p.USERNAME.ToUpper() == userName.ToUpper());
            if (data != null)
            {
                if (data.LASTLOGINDATE == null)
                    return true;
            }
            return false;
        }

        public bool IsAccountLocked(string userName) // ERROR POINT 1 - 
        {
            var data = GetAllUsers().FirstOrDefault(c => c.username.ToLower() == userName);
            if (data == null) throw new SecureException("1001 Login Failure.");
            if (data.isLocked)
            {
                var  loginInfo = GetUserLoginInfoByUserName(userName);
                _auditTrail.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoginFailed,
                    STAFFID = loginInfo.staffId,
                    BRANCHID = (short)data.branchId,//(short)context.TBL_STAFF.Where(x => x.STAFFID == loginInfo.staffId).Select(x => x.BRANCHID).FirstOrDefault(),
                    DETAIL = $"{loginInfo.username} - This account is LOCKED",
                    IPADDRESS = CommonHelpers.GetUserIP(),
                    URL = String.Empty, // Request.RequestUri.AbsoluteUri,
                    APPLICATIONDATE = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = -1
                });
                context.SaveChanges();
                return true;
            }
            return false;
        }
        
        public bool IsAccountActive(string userName)
        {
            var data = GetAllUsers().FirstOrDefault(c => c.username.ToLower() == userName);
            if (data != null)
            {
                var loginInfo = GetUserLoginInfoByUserName(userName);

                var audit = new TBL_AUDIT()
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoginFailed,
                    STAFFID = loginInfo.staffId,
                    BRANCHID = (short)context.TBL_STAFF.Where(x => x.STAFFID == loginInfo.staffId).Select(x => x.BRANCHID).FirstOrDefault(),
                    DETAIL = $"{loginInfo.username} - This account is INACTIVE",
                    IPADDRESS = CommonHelpers.GetUserIP(),
                    //URL = Request.RequestUri.AbsoluteUri,
                    APPLICATIONDATE = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = -1
                };

                _auditTrail.AddAuditTrail(audit);
                context.SaveChanges();

                return data.isActive;
            }

            throw new SecureException("1001 Login Failure.");
        }

        private bool ResumptionClosingTime(UserViewModel user)
        {
            var presentTime = DateTime.Now.TimeOfDay;

            var data = (from u in context.TBL_PROFILE_USER
                        join st in context.TBL_STAFF
                        on u.STAFFID equals st.STAFFID
                        where u.USERNAME.ToLower() == user.username
                        select new
                        {
                            staffId = st.STAFFID,
                            workStartTime = st.WORKSTARTDURATION,
                            workEndTime = st.WORKENDDURATION
                        }).FirstOrDefault();

            if (data != null && data.workStartTime != null && data.workEndTime != null)
            {
                var startTime = TimeSpan.FromHours((int)data.workStartTime);
                var endTime = TimeSpan.FromHours((int)data.workEndTime);
                return startTime < presentTime || endTime < presentTime;
            }
            else
            {
                var staffRole = context.TBL_STAFF_ROLE.FirstOrDefault(x => x.STAFFROLEID == user.roleId);
                if (staffRole != null)
                {
                    if (staffRole.WORKSTARTDURATION == null || staffRole.WORKENDDURATION == null) return true;
                    var startTime = TimeSpan.FromHours((int)staffRole.WORKSTARTDURATION);
                    var endTime = TimeSpan.FromHours((int)staffRole.WORKENDDURATION);
                    return startTime < presentTime || endTime < presentTime;
                }
            }
            return false;
        }

        public bool ResumptionClosignTime(string userName, bool logAudit = true)
        {
            var presentTime = DateTime.Now.TimeOfDay;

            var data = (from u in context.TBL_PROFILE_USER
                        join st in context.TBL_STAFF
                        on u.STAFFID equals st.STAFFID
                        where u.USERNAME.ToLower() == userName
                        select new
                        {
                            staffId = st.STAFFID,
                            workStartTime = st.WORKSTARTDURATION,
                            workEndTime = st.WORKENDDURATION
                        }).FirstOrDefault();

            if (data != null && data.workStartTime != null && data.workEndTime != null)
            {
                var startTime = TimeSpan.FromHours((int)data.workStartTime);
                var endTime = TimeSpan.FromHours((int)data.workEndTime);

                return startTime < presentTime || endTime < presentTime;
            }
            else
            {
                var staffRole = (from u in context.TBL_STAFF_ROLE
                                 join st in context.TBL_STAFF on u.STAFFROLEID equals st.STAFFROLEID
                                 join p in context.TBL_PROFILE_USER on st.STAFFID equals p.STAFFID
                                 where p.USERNAME.ToLower() == userName
                                 select new
                                 {
                                     staffId = st.STAFFID,
                                     workStartTime = u.WORKSTARTDURATION,
                                     workEndTime = u.WORKENDDURATION
                                 }).FirstOrDefault();
                if (staffRole != null)
                {
                    if (staffRole.workStartTime == null || staffRole.workEndTime == null)
                    {
                        return true;
                    }
                    var startTime = TimeSpan.FromHours((int)staffRole.workStartTime);
                    var endTime = TimeSpan.FromHours((int)staffRole.workEndTime);

                    return startTime < presentTime || endTime < presentTime;
                }
            }

            if (logAudit)
            {
                var loginInfo = GetUserLoginInfoByUserName(userName);

                var audit = new TBL_AUDIT()
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoginFailed,
                    STAFFID = loginInfo.staffId,
                    BRANCHID = (short)context.TBL_STAFF.Where(x => x.STAFFID == loginInfo.staffId).Select(x => x.BRANCHID).FirstOrDefault(),
                    DETAIL = $"{loginInfo.username} - You cannot resume now.",
                    IPADDRESS = CommonHelpers.GetUserIP(),
                    //URL = Request.RequestUri.AbsoluteUri,
                    APPLICATIONDATE = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = -1
                };

                _auditTrail.AddAuditTrail(audit);
                context.SaveChanges();
            }

            throw new SecureException("You cannot resume now.");
        }

        public bool PasswordStandard(string password)
        {
            var setting = GetProfileSettings();

            if (setting.MINREQUIREDPASSWORDLENGTH > password.Length)
                throw new SecureException($"Password should not be {setting.MINREQUIREDPASSWORDLENGTH} less characters");

            if (setting.MINREQUIREDNONALPHANUMERICCHAR > 0)
            {
                if (!CommonHelpers.isAlphaNumeric(password))
                {
                    throw new SecureException($"Password should alphanumeric.");
                }
                else
                    throw new SecureException($"Password should not be {setting.MINREQUIREDNONALPHANUMERICCHAR} less characters");

            }

            return true;
        }

        private UserViewModel UserLoginDetails(string username, string password) // ERROR POINT 3 - underlying provider...
        {
            TBL_PROFILE_USER profile = new TBL_PROFILE_USER();
            profile = context.TBL_PROFILE_USER.FirstOrDefault(c => c.USERNAME.ToUpper() == username.ToUpper());// && c.PASSWORD == password);

            if (profile != null && context.TBL_SETUP_GLOBAL.FirstOrDefault().USE_ACTIVE_DIRECTORY)
            {
                var staff1 = context.TBL_STAFF.Find(profile.STAFFID);
                var userInfo = new UserViewModel();

                userInfo.companyId = staff1.COMPANYID;
                userInfo.staffId = profile.STAFFID;
                userInfo.user_id = profile.USERID;
                userInfo.username = profile.USERNAME;
                userInfo.staffName = staff1.FIRSTNAME + " " + staff1.MIDDLENAME + " " + staff1.LASTNAME;
                userInfo.branchId = staff1.BRANCHID;
                userInfo.countryId = staff1.TBL_COMPANY.COUNTRYID;
                userInfo.branchName = context.TBL_BRANCH.FirstOrDefault(d => d.BRANCHID == staff1.BRANCHID)?.BRANCHNAME;
                userInfo.companyName = staff1.TBL_COMPANY.NAME;
                userInfo.logincode = profile.LOGINCODE;
                userInfo.lastLoginDate = profile.LASTLOGINDATE;
                userInfo.roleId = staff1.STAFFROLEID;

                profile.LASTLOGINDATE = DateTime.Now;
                profile.LOGINCODE = LogCode;
                profile.FAILEDLOGONATTEMPT = 0;

                context.Entry(profile).State = EntityState.Modified;
                context.SaveChanges();

                return userInfo;
            }
            else if (profile != null && profile.PASSWORD == password)
            {
                var staff = context.TBL_STAFF.Find(profile.STAFFID);
                var userInfo = new UserViewModel();

                userInfo.companyId = staff.COMPANYID;
                userInfo.staffId = profile.STAFFID;
                userInfo.user_id = profile.USERID;
                userInfo.username = profile.USERNAME;
                userInfo.staffName = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;
                userInfo.branchId = staff.BRANCHID;
                userInfo.countryId = staff.TBL_COMPANY.COUNTRYID;
                userInfo.branchName = context.TBL_BRANCH.FirstOrDefault(d => d.BRANCHID == staff.BRANCHID)?.BRANCHNAME;
                userInfo.companyName = staff.TBL_COMPANY.NAME;
                userInfo.logincode = profile.LOGINCODE;
                userInfo.lastLoginDate = profile.LASTLOGINDATE;
                userInfo.roleId = staff.STAFFROLEID;

                profile.LASTLOGINDATE = DateTime.Now;
                profile.LOGINCODE = LogCode;
                profile.FAILEDLOGONATTEMPT = 0;

                context.Entry(profile).State = EntityState.Modified;
                context.SaveChanges();

                return userInfo;
            }
            else
            {
                //FinTrakBankingContext db = new FinTrakBankingContext();
                // var faileddata = context.TBL_PROFILE_USER.FirstOrDefault(c => c.USERNAME.ToLower() == username);
                if (profile != null)
                {
                    int count = profile.FAILEDLOGONATTEMPT ?? 0;
                    TBL_PROFILE_SETTING prosett = new TBL_PROFILE_SETTING();
                    prosett = context.TBL_PROFILE_SETTING.FirstOrDefault();
                    profile.LOGINCODE = null;
                    profile.FAILEDLOGONATTEMPT += 1;
                    if (count == prosett.MAXINVALIDPASSWORDATTEMPTS)
                    {

                    //if (count == CommonHelpers.MaxInvalidPasswordAttempts)

                        profile.ISLOCKED = true;
                        profile.LASTLOCKOUTDATE = DateTime.Now;
                        //profile.FAILEDLOGONATTEMPT = 0;

                    }

                   context.Entry(profile).State = EntityState.Modified;

                    context.SaveChanges();

                    throw new SecureException("1001 Login Failure.");
                }
                else
                {
                    throw new SecureException("1001 Not Fund.");
                }
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

        public IQueryable<UserViewModel> GetAllUsers()
        {
            return (from u in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF on u.STAFFID equals st.STAFFID
                    select new UserViewModel
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
                        branchId = st.BRANCHID,
                        roleId = st.STAFFROLEID,
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
            var setting = GetProfileSettings();

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
                        email = st.EMAIL,
                        sessionTimeout = setting.SESSIONTIMEOUT
                    }).FirstOrDefault();

        }

        public UserViewModel GetUserLoginInfoByUserName(string userName)
        {
            var setting = GetProfileSettings();

            return (from u in context.TBL_PROFILE_USER
                    join st in context.TBL_STAFF
                    on u.STAFFID equals st.STAFFID
                    where u.USERNAME.ToLower() == userName.ToLower()
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
                        sessionTimeout = setting.SESSIONTIMEOUT
                    }).FirstOrDefault();

        }

        public bool ClearLoginToken(string userName)
        {
            bool result = false;
           // var _user = context.TBL_PROFILE_USER.FirstOrDefault(x => x.USERNAME.ToLower() == userName);
            TBL_PROFILE_USER _user = new TBL_PROFILE_USER();
            _user = (from a in context.TBL_PROFILE_USER
                     where a.USERNAME.ToLower() == userName
                     select a).FirstOrDefault();

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

        public LookupViewModel GetDashboardStaffRole(int staffId)
        {
            var dash = (from st in context.TBL_STAFF
                        join sr in context.TBL_STAFF_ROLE on st.STAFFROLEID equals sr.STAFFROLEID
                        where st.STAFFID == staffId
                        select new LookupViewModel
                        {
                            lookupId = (short)sr.STAFFROLEID,
                            lookupName = sr.STAFFROLENAME
                        }).FirstOrDefault();
            return dash;
        }

        public bool PasswordChange(PasswordChangeViewModel pwdChange)
        {
            var setting = GetProfileSettings();

            if (pwdChange.currentPassword != pwdChange.newPassword)
            {
                var data = context.TBL_PROFILE_USER.Where(u => u.USERNAME.ToUpper() == pwdChange.username.ToUpper()).FirstOrDefault();

                var daat = context.TBL_PROFILE_PASSWORD_HISTORY.Where(p => p.USERID == data.USERID && p.PASSWORD == pwdChange.newPassword)
                                                            .OrderBy(p => p.DATETIMECREATED)
                                                            .Take(setting.ALLOWPASSWORDREUSEAFTER);
                if (!daat.Any())
                {
                    if (data != null && data.PASSWORD == pwdChange.currentPassword)
                    {
                        int staffId = data.STAFFID;
                        data.PASSWORD = pwdChange.newPassword;
                        data.DATETIMEUPDATED = DateTime.Now;
                        data.LASTUPDATEDBY = staffId;
                        data.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(setting.EXPIREPASSWORDAFTER);
                        var history = new TBL_PROFILE_PASSWORD_HISTORY
                        {
                            CREATEDBY = staffId,
                            DATETIMECREATED = DateTime.Now,
                            PASSWORD = pwdChange.newPassword,
                            USERID = daat.FirstOrDefault().USERID
                        };
                        context.TBL_PROFILE_PASSWORD_HISTORY.Add(history);

                        return context.SaveChanges() > 0;
                    }
                    else
                        throw new SecureException("Password is not valid");
                }
                else
                    throw new SecureException($"You are not allow to re-use the previous {setting.ALLOWPASSWORDREUSEAFTER} passwords");
            }
            else
                throw new SecureException("New Password should not be same as the Current Password");
        }

        public bool ValidatePasswordPolicy(string password)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");

            var isValidated = hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) && hasMinimum8Chars.IsMatch(password);
            return isValidated;
        }

        public bool ValidateOldPassword(string username, string oldPassword)
        {
            bool isOldPasswordValid = false;
            var data = context.TBL_PROFILE_USER.Where(u => u.USERNAME.ToUpper() == username.ToUpper()).FirstOrDefault();
            if (data != null)
            {
                if (data.PASSWORD == oldPassword)
                {
                    isOldPasswordValid = true;
                    return isOldPasswordValid;
                }
            }
            return isOldPasswordValid;
        }

        public bool GetRunningEndOfDayProcess(int companyId)
        {
            var applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            return context.TBL_FINANCE_ENDOFDAY
                .Where(x => x.DATE == applicationDate && x.COMPANYID == companyId && x.EODSTATUSID == (int)EodOperationStatusEnum.Processing)
                .Any();
        }
    }
}