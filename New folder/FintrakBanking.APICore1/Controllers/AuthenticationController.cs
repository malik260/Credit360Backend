using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/auth")]
    public class AuthenticationController : Controller
    {
        TokenDecryptionHelper token = null;
        private IAuthenticationRepository repo;
        private IConfigurationRoot _config;
        IErrorLogRepository errorLogger;
        private IAdminRepository _adminRepo;
        public AuthenticationController(IAuthenticationRepository _repo,
                                        IConfigurationRoot config,
                                        IErrorLogRepository _errorLogger,
                                        IAdminRepository adminRepo)
        {
            this.repo = _repo;
            this._config = config;
            this._adminRepo = adminRepo;
            this.errorLogger = _errorLogger;
        }



        [HttpGet("user")]
        public IActionResult GetAllUsers()
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                if (repo != null)
                {
                    var users = repo.GetAllUsers().ToList();
                    if (users == null)
                    {
                        return NotFound(new { success = false, message = "No user found" });
                    }
                    return Ok(new { success = true, result = users });
                }

                return Ok(new { success = false, message = $"No user found" });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an internal error : { ex.Message}" });
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody] UserViewModel user)
        {
            try
            {
                if (repo.IsUserExit(user.username.ToLower()))
                {
                    return Ok(new { suucess = false, message = "A user with this username already exit" });
                }
                token = new TokenDecryptionHelper(this.HttpContext);
                //user.staffId = token.GetStaffId;
                user.createdBy = token.GetStaffId;
                user.lastUpdatedBy = token.GetStaffId;
                var response = await repo.CreateUser(user);
                if (response)
                {
                    return Created("", new { success = true, result = user, message = "User has been created successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var response = await repo.DeleteUser(userId);
                if (response)
                {
                    return Ok();
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("user/login")]
        public IActionResult LoginUser([FromBody] UserViewModel user)
        {
            try
            {

                user.password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey);
                var foundUser = repo.FindUserByUserNameAndPassword(user);
                if (foundUser == null)
                {
                    return Ok(new { success = false, message = "Wrong username or password" });
                }

                return Ok(new { success = true, result = foundUser });
            }
            catch (Exception ex)
            {

                return Ok(new { success = false, status = 400, message = ex.Message });
            }
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserViewModel user)
        {
            try
            {
                var response = await repo.UpdateUser(userId, user);
                if (response)
                {
                    return Ok(new { success = true, message = "User has been successfully updated" });
                }
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    success = false,
                    message = $"An unknown error occured while updating user: {e.Message}"
                });
            }
            return Ok(new { success = false, message = "An unknown error occured while updating user" });
        }

        //Group

        [HttpGet("group")]
        public IActionResult GetGroups()
        {
            try
            {
                if (repo != null)
                {
                    var groups = repo.GetAllGroups();
                    if (groups == null)
                    {
                        return NotFound(new { success = false, message = "No group found" });
                    }
                    return Ok(new { success = true, result = groups.ToList() });
                }

                return BadRequest(new { success = false, message = $"No group found" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = $"There was an internal error : { ex.Message}" });
            }
        }

        [HttpPost("token")]
        public IActionResult GetToken([FromBody] UserViewModel user)
        {
            try
            {


                user.password = StaticHelpers.EncryptSha512(user.password, StaticHelpers.EncryptionKey);
                var foundUser = repo.FindUserByUserNameAndPassword(user);
                if (foundUser == null)
                {
                    return Ok(new { success = false, message = "Wrong username or password" });
                }

                var currUser = foundUser.First();

                DateTime now = DateTime.UtcNow;
                var userActivities = this._adminRepo.GetUserActivities(currUser.user_id);

                var tokenKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtProvider.TokenKey));
                var appSigningCredentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256);

                var mytoken = new JwtSecurityToken(
                    issuer: _config["Token:Issuer"],
                    audience: _config["Token:Audience"],
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromMinutes(int.Parse(_config["Token:TokenExpiration"]))),
                    signingCredentials: appSigningCredentials
                    );

                //Add Custom payloads
                mytoken.Payload["iat"] = new DateTimeOffset(now).ToUnixTimeSeconds().ToString();
                mytoken.Payload["username"] = currUser.username;
                mytoken.Payload["companyId"] = currUser.companyId;
                mytoken.Payload["staffId"] = currUser.staffId;
                mytoken.Payload["jti"] = Guid.NewGuid().ToString();
                mytoken.Payload["branchId"] = currUser.branchId;
                mytoken.Payload["countryId"] = currUser.countryId;
                mytoken.Payload["userId"] = currUser.user_id;
                //mytoken.Payload["roles"] = userActivities;

                var encodedToken = new JwtSecurityTokenHandler().WriteToken(mytoken);

                // build the json response
                return Ok(new
                {
                    success = true,
                    access_token = encodedToken,
                    expiration = mytoken.ValidTo,
                    userInfo = new UserCoyInfo {
                        branchName = currUser.branchName,
                        companyName =currUser.companyName,
                        UserName =currUser.username,
                        roles=userActivities
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
            }
        }
    }
    public class UserCoyInfo
    {
        public string companyName { get; set; }
        public string branchName { get; set; }
        public string UserName { get; set; }
        public List<string> roles { get; set; }
    }

}