using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Authentication;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/auth")]
    public class AuthenticationController : ApiController
    {
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        private readonly IAuthenticationRepository _repo;
        private readonly IAuditTrailRepository _auditTrail;
        private readonly IErrorLogRepository _errorLogger;
        private IAdminRepository _adminRepo;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly FinTrakBankingContext _context;

        public AuthenticationController(
                IAuthenticationRepository repo,
                IErrorLogRepository errorLogger,
                IAdminRepository adminRepo,
                IAuditTrailRepository auditTrail,
                IGeneralSetupRepository genSetup,
                FinTrakBankingContext context
            )
        {
            this._repo = repo;
            _adminRepo = adminRepo;
            this._errorLogger = errorLogger;
            this._auditTrail = auditTrail;
            _genSetup = genSetup;
            this._context = context;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("user")]
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                if (_repo != null)
                {
                    var users = _repo.GetAllUsers();
                    if (users == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No user found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = users.ToList() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"No user found" });
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an internal error : { ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("user")]
        public async Task<HttpResponseMessage> AddUser([FromBody] UserViewModel user)
        {
            try
            {
                if (_repo.IsUserExisting(user.username.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "A user with this username already exit" });
                }

                user.createdBy = token.GetStaffId;
                user.lastUpdatedBy = token.GetStaffId;

                var response = await _repo.CreateUser(user);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, new { success = true, result = user, message = "User has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> DeleteUser(int userId)
        {
            try
            {
                var response = await _repo.DeleteUser(userId);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation was successful" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> UpdateUser(int userId, [FromBody] UserViewModel user)
        {
            try
            {
                var response = await _repo.UpdateUser(userId, user);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "User has been successfully updated" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error occured while updating user" });
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //Group

        [HttpGet]
        [ClaimsAuthorization]
        [Route("group")]
        public HttpResponseMessage GetGroups()
        {
            try
            {
                if (_repo != null)
                {
                    var groups = _repo.GetAllGroups().Select(x => new
                    {
                        groupId = x.GROUPID,
                        groupName = x.GROUPNAME
                    }).ToList();

                    if (groups.Any() == false)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No group found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = groups.ToList() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        //public string GetIpAddress(HttpRequestMessage request)
        //{
        //    if (!request.Properties.ContainsKey(HttpContext)) return null;
        //    dynamic context = request.Properties[HttpContext];
        //    return context != null ? (string)context.Request.UserHostAddress : null;
        //}

        [HttpPost]// [ClaimsAuthorization]
        [Route("token")]
        public async Task<HttpResponseMessage> GetTokenAsync([FromBody] TokenVM user)
        {
            try
            {
                user.password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey);
                string ipAddressStr = null;
                if(token.LoginCode == null)
                    ipAddressStr = token.LoginCode.Split('@')[1];

               _repo.SessionInfo = await _repo.CheckSessionState(user.username.ToLower(), ipAddressStr);
                var foundUser = await _repo.FindUserByUserNameAndPassword(user.username.ToLower(), user.password);

                if (foundUser == null)
                {
                    var found = _repo.GetSingleUserByUserName(user.username.ToLower());

                    if (found.branchId != null)
                    {
                        var audit1 = new TBL_AUDIT()
                        {
                            AUDITTYPEID = (short)AuditTypeEnum.Loggedfailed,
                            STAFFID = found.staffId,
                            BRANCHID = (short)found.branchId,
                            DETAIL = $"{user.username} logged failed",
                            IPADDRESS = CommonHelpers.GetUserIP(),
                            URL = Request.RequestUri.AbsoluteUri,
                            APPLICATIONDATE = _genSetup.GetApplicationDate(),
                            SYSTEMDATETIME = DateTime.Now,
                            TARGETID = -1
                        };

                        _auditTrail.AddAuditTrail(audit1);
                    }

                    _context.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Wrong username or password" });
                }

                var currUser = foundUser;

                var userActivities = _repo.GetUserActivitiesByUser(currUser.user_id);

                if (currUser.branchId != null)
                {
                    var audit = new TBL_AUDIT()
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.LoggedIn,
                        STAFFID = currUser.staffId,
                        BRANCHID = (short)currUser.branchId,
                        DETAIL = $"{currUser.username} logged in",
                        IPADDRESS = CommonHelpers.GetUserIP(),
                        URL = Request.RequestUri.AbsoluteUri,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = -1
                    };

                    _auditTrail.AddAuditTrail(audit);
                }

                await _context.SaveChangesAsync();

                // build the json response
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    access_token = user.encodedToken,
                    expiration = user.validTo,
                    userInfo = new UserInfo
                    {
                        branchName = currUser.branchName,
                        companyName = currUser.companyName,
                        UserName = currUser.username,
                        activities = userActivities,
                        staffId = currUser.staffId,
                        staffName = currUser.staffName,
                        sessionStatusInfo = currUser.sessionStatusInfo,
                        applicationDate = _genSetup.GetApplicationDate(),
                        lastLoginDate = currUser.lastLoginDate,
                    }
                });

            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
            }
        }

        [HttpPost] //[ClaimsAuthorization]
        [Route("endpendingsession")]
        public IHttpActionResult SignOutUser([FromBody] TokenVM user)
        {
            try
            {

                //var audit = new TBL_AUDIT()
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.LoggedOut,
                //    STAFFID = token.GetStaffId,
                //    BRANCHID = (short)token.GetBranchId,
                //    DETAIL = $"{token.GetUsername} ended a pending session",
                //    IPADDRESS = CommonHelpers.GetUserIP(),
                //    URL = Request.RequestUri.AbsoluteUri,
                //    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now,
                //    TARGETID = -1
                //};

                //auditTrail.AddAuditTrail(audit);

                var res = _repo.ClearLoginToken(user.username);
                Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                return this.Ok(new { success = true, message = "Session Ended. Login To Continue" });

            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return this.Ok(new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
            }
        }

        [HttpPost] //[ClaimsAuthorization]
        [Route("logOut")]
        public IHttpActionResult LogOut()
        {
            try
            {
                _repo.ClearLoginToken(token.GetUsername);
                var staffDetails = _repo.GetSingleUserByUserName(token.GetUsername);

                if (staffDetails == null)
                {
                    return this.Ok(new { success = false, message = "User Not Found" });
                }


                Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);


                var audit = new TBL_AUDIT()
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoggedOut,
                    STAFFID = token.GetStaffId,
                    BRANCHID = (short)token.GetBranchId,
                    DETAIL = $"{token.GetUsername} logged out",
                    IPADDRESS = CommonHelpers.GetUserIP(),
                    URL = Request.RequestUri.AbsoluteUri,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = -1
                };

                _auditTrail.AddAuditTrail(audit);

                _context.SaveChanges();

                return this.Ok(new { success = true, message = "User Logged Off" });

            }
            catch (Exception ex)
            {
                _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return this.Ok(new { success = false, message = $"An unknown error occured {ex.Message}" });
            }

        }

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

    }
}