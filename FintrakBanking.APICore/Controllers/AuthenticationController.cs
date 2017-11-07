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

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/auth")]
    public class AuthenticationController : ApiController
    {
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        private IAuthenticationRepository repo;
        private IAuditTrailRepository auditTrail;
        private IErrorLogRepository errorLogger;
        private IAdminRepository _adminRepo;
        private IGeneralSetupRepository _genSetup;
        private FinTrakBankingContext context;

        public AuthenticationController(
                IAuthenticationRepository _repo,
                IErrorLogRepository _errorLogger,
                IAdminRepository adminRepo,
                IAuditTrailRepository _auditTrail,
                IGeneralSetupRepository genSetup, 
                FinTrakBankingContext _context
            )
        {
            repo = _repo;
            _adminRepo = adminRepo;
            errorLogger = _errorLogger;
            auditTrail = _auditTrail;
            _genSetup = genSetup;
            context = _context;
        }

        [HttpGet]
        [Route("user")]
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                if (repo != null)
                {
                    var users = repo.GetAllUsers();
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
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an internal error : { ex.Message}" });
            }
        }

        [HttpPost]
        [Route("user")]
        public async Task<HttpResponseMessage> AddUser([FromBody] UserViewModel user)
        {
            try
            {
                if (repo.IsUserExits(user.username.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "A user with this username already exit" });
                }

                user.createdBy = token.GetStaffId;
                user.lastUpdatedBy = token.GetStaffId;

                var response = await repo.CreateUser(user);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, new { success = true, result = user, message = "User has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> DeleteUser(int userId)
        {
            try
            {
                var response = await repo.DeleteUser(userId);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation was successful" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> UpdateUser(int userId, [FromBody] UserViewModel user)
        {
            try
            {
                var response = await repo.UpdateUser(userId, user);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "User has been successfully updated" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error occured while updating user" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //Group

        [HttpGet]
        [Route("group")]
        public HttpResponseMessage GetGroups()
        {
            try
            {
                if (repo != null)
                {
                    var groups = repo.GetAllGroups().Select(x => new
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
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("token")]
        public HttpResponseMessage GetToken([FromBody] TokenVM user)
        {
            try
            {
                user.password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey);
                var foundUser = repo.FindUserByUserNameAndPassword(user.username, user.password);

                if (foundUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Wrong username or password" });
                }

                var currUser = foundUser;

                var userActivities = _adminRepo.GetUserActivities(currUser.user_id);

                var audit = new TBL_AUDIT()
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoggedIn,
                    STAFFID = currUser.staffId,
                    BRANCHID = currUser.branchId,
                    DETAIL = $"{currUser.username} logged in",
                    IPADDRESS = CommonHelpers.GetUserIP(),
                    URL = Request.RequestUri.AbsoluteUri,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = -1
                };

                auditTrail.AddAuditTrail(audit);

                context.SaveChanges();

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
                        staffId = currUser.staffId
                    }
                });

            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("logOut")]
        public HttpResponseMessage LogOut()
        {
            try
            {
                var staffDetails = repo.GetSingleUserByUserName(token.GetUsername);

                if (staffDetails == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "User Not Found" });
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

                auditTrail.AddAuditTrail(audit);

                context.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "User Logged Off" });

            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An unknown error occured {ex.Message}" });
            }

        }

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

    }
}