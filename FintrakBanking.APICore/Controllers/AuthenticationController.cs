
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/auth")]
    public class AuthenticationController : ApiController
    {
        TokenDecryptionHelper token = null;
        private IAuthenticationRepository repo;
        //private IConfigurationRoot _config;
        IErrorLogRepository errorLogger;
        private IAdminRepository _adminRepo;
        public AuthenticationController(IAuthenticationRepository _repo,
                                        //IConfigurationRoot config,
                                        IErrorLogRepository _errorLogger,
                                        IAdminRepository adminRepo)
        {
            this.repo = _repo;
            //this._config = config;
            this._adminRepo = adminRepo;
            this.errorLogger = _errorLogger;
        }



        [HttpGet]
        [Route("user")]
        public HttpResponseMessage GetAllUsers()
        {

            try
            {
                //token = new TokenDecryptionHelper(this.HttpContext);
                if (repo != null)
                {
                    var users = repo.GetAllUsers().ToList();
                    if (users == null)
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No user found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = users });
                    //return new { success = true, result = users });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"No user found" });

            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.RequestUri.Host, "");// token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an internal error : { ex.Message}" });
            }
            //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"No user found" });
        }

        [HttpPost]
        [Route("user")]
        public async Task<HttpResponseMessage> AddUser([FromBody] UserViewModel user)
        {
            try
            {
                if (repo.IsUserExit(user.username.ToLower()))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "A user with this username already exit" });

                }
                token = new TokenDecryptionHelper();
                //user.staffId = token.GetStaffId;
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
                this.errorLogger.LogError(ex, this.Request.RequestUri.Host, "");// token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> DeleteUser(int userId)
        {
            try
            {
                //token = new TokenDecryptionHelper(this.HttpContext);
                var response = await repo.DeleteUser(userId);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation was successful" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });


            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.RequestUri.Host, "");// token.GetUsername);
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
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.RequestUri.Host, "");// token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error occured while updating user" });
        }

        //Group

        
        [HttpGet]
        [Route("group")]
        public HttpResponseMessage GetGroups()
        {
            try
            {
                token = new TokenDecryptionHelper();
                var staffId = token.GetStaffId;
                var createdBy = token.GetStaffId;
                var lastUpdatedBy = token.GetStaffId;
                var username = token.GetUsername;

                if (repo != null)
                {
                    var groups = repo.GetAllGroups().Select(x => new
                    {
                        groupId = x.GroupId,
                        groupName = x.GroupName
                    });
                    if (groups == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No group found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = groups.ToList() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.RequestUri.Host, "");// token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("token")]
        public async Task<HttpResponseMessage> GetToken([FromBody] TokenVM user)
        {
            try
            {
                user.password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey);
                var foundUser = await repo.FindUserByUserNameAndPassword(user.username, user.password);
                if (foundUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Wrong username or password" });
                }

                var currUser = foundUser.First();

                DateTime now = DateTime.Now;
                var userActivities = this._adminRepo.GetUserActivities(currUser.user_id);

                // build the json response
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    access_token = user.encodedToken,
                    expiration = user.validTo,
                    userInfo = new UserCoyInfo
                    {
                        branchName = currUser.branchName,
                        companyName = currUser.companyName,
                        UserName = currUser.username,
                        activities = userActivities
                    }
                });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.RequestUri.Host, "");// token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
                //return new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
            }
        }
    }

    public class UserCoyInfo
    {
        public string companyName { get; set; }
        public string branchName { get; set; }
        public string UserName { get; set; }
        public List<string> activities { get; set; }
    }

    public class TokenVM
    {
        public string username { get; set; }
        public string password { get; set; }
        public string encodedToken { get; set; }
        public string validTo { get; set; }
    }
}

