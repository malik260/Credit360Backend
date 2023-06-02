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
using FintrakBanking.Common.CustomException;
using System.Text;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Security.Cryptography;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/auth")]
    public class AuthenticationController : ApiController
    {
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        private readonly IAuthenticationRepository _repo;
        private readonly IAuditTrailRepository _auditTrail;
        private readonly IErrorLogRepository _errorLogger;
        // private IAdminRepository _adminRepo;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly FinTrakBankingContext _context;

        public AuthenticationController(
                IAuthenticationRepository repo,
                IErrorLogRepository errorLogger,
                // IAdminRepository adminRepo,
                IAuditTrailRepository auditTrail,
                IGeneralSetupRepository genSetup,
                FinTrakBankingContext context
            )
        {
            this._repo = repo;
            // _adminRepo = adminRepo;
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("user")]
        public async Task<HttpResponseMessage> AddUser([FromBody] UserViewModel user)
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

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> DeleteUser(int userId)
        {

            var response = await _repo.DeleteUser(userId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation was successful" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("user/{userId}")]
        public async Task<HttpResponseMessage> UpdateUser(int userId, [FromBody] UserViewModel user)
        {
            //try
            //{
            var response = await _repo.UpdateUser(userId, user);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "User has been successfully updated" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error occured while updating user" });
            //}
            //catch (SecureException ex)
            //{
            //    _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            //}
        }

        //Group

        [HttpGet]
        [ClaimsAuthorization]
        [Route("group")]
        public HttpResponseMessage GetGroups()
        {
            //try
            //{
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
            //}
            //catch (SecureException ex)
            //{
            //    _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            //}
        }
        //public string GetIpAddress(HttpRequestMessage request)
        //{
        //    if (!request.Properties.ContainsKey(HttpContext)) return null;
        //    dynamic context = request.Properties[HttpContext];
        //    return context != null ? (string)context.Request.UserHostAddress : null;
        //}

        [HttpPost]// [ClaimsAuthorization]
        [Route("token")]
        public HttpResponseMessage GetTokenAsync([FromBody] TokenVM user)
        {
            //  return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = user , password =  user.password, username = user.username, validTo = user.validTo, encodedToken = user.encodedToken});

            try
            {
                //var keybytes = Encoding.UTF8.GetBytes("7061737323313233");
                //var iv = Encoding.UTF8.GetBytes("7061737323313233");
                //var sanitizdPassword = user.password.Replace(" ", "+");

                //var encrypted = Convert.FromBase64String(sanitizdPassword);//Encoding.ASCII.GetBytes(context.Password);
                //var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);


                //byte[] pass = Convert.FromBase64String(context.Password);
                //string password = decriptedFromJavascript;// Encoding.UTF8.GetString(pass);

                byte[] pass = Convert.FromBase64String(user.password);
                string password = Encoding.UTF8.GetString(pass);


                user.password = StaticHelpers.EncryptSha512(password, StaticHelpers.EncryptionKey);
                string ipAddressStr = String.Empty;
                if (token.LoginCode == null) ipAddressStr = token.LoginCode.Split('@')[1];

                _repo.SessionInfo = _repo.CheckSessionState(user.username.ToLower(), ipAddressStr);
                var foundUser = _repo.FindUserByUserNameAndPassword(user.username.ToLower(), user.password);

                if (foundUser == null)
                {
                    var found = _repo.GetSingleUserByUserName(user.username.ToLower());

                    if (found.branchId != null)
                    {
                        var audit1 = new TBL_AUDIT
                        {
                            AUDITTYPEID = (short)AuditTypeEnum.LoginFailed,
                            STAFFID = found.staffId,
                            BRANCHID = (short)found.branchId,
                            DETAIL = $"{user.username} login failed",
                            IPADDRESS = CommonHelpers.GetLocalIpAddress(),//CommonHelpers.GetUserIP(),
                            URL = Request.RequestUri.AbsoluteUri,
                            APPLICATIONDATE = _genSetup.GetApplicationDate(),
                            SYSTEMDATETIME = DateTime.Now,
                            TARGETID = -1,
                            OSNAME = "Testing"
                            //OSNAME = CommonHelpers.FriendlyName()
                            // OSNAME = "test",
                        };

                        _auditTrail.AddAuditTrail(audit1);
                    }

                    _context.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "1001 Login Failure." });
                }

                var currUser = foundUser;
                var userRole = _repo.GetDashboardStaffRole(currUser.staffId);
                var userActivities = _repo.GetUserActivitiesByUser(currUser.user_id);

                if (currUser.branchId != null)
                {
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.LoggedIn,
                        STAFFID = currUser.staffId,
                        BRANCHID = (short)currUser.branchId,
                        DETAIL = $"{currUser.username} logged in",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),//CommonHelpers.GetUserIP(),
                        URL = Request.RequestUri.AbsoluteUri,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = -1,
                        OSNAME = "",
                        //OSNAME = CommonHelpers.FriendlyName()
                        //OSNAME = "test",
                    };

                    _auditTrail.AddAuditTrail(audit);
                }

                _context.SaveChanges();

                //var ttttt = HttpUtility.HtmlDecode(user.encodedToken);
                //var dat = HttpUtility.HtmlDecode(user.validTo);
                byte[] data = Convert.FromBase64String(user.encodedToken);
                string encodedToken = Encoding.UTF8.GetString(data);
                byte[] data2 = Convert.FromBase64String(user.validTo);
                string validTo = Encoding.UTF8.GetString(data2);
                //var sanitizdToken = user.encodedToken.Replace(" ", "+");

                //byte[] data = Convert.FromBase64String(sanitizdToken);
                //var tokenEncode = DecryptStringFromBytes(data, keybytes, iv);
                //string encodedToken = tokenEncode;// Encoding.UTF8.GetString(data);

                //var sanitizdvalidTo = user.validTo.Replace(" ", "+");

                //byte[] data2 = Convert.FromBase64String(sanitizdvalidTo);
                //var validDate = DecryptStringFromBytes(data2, keybytes, iv);

                //string validTo = validDate;
                // build the json response
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    access_token = encodedToken,
                    expiration = validTo,
                    userInfo = new UserInfo
                    {
                        branchName = currUser.branchName,
                        companyName = currUser.companyName,
                        userName = currUser.username,
                        activities = userActivities,
                        staffId = currUser.staffId,
                        staffName = currUser.staffName,
                        sessionStatusInfo = currUser.sessionStatusInfo,
                        applicationDate = _genSetup.GetApplicationDate(),
                        lastLoginDate = currUser.lastLoginDate,
                        staffRole = userRole.lookupName,
                        corrMatrixId = currUser.corrMatrixId,
                        corrMatrixDescription = currUser.corrMatrixDescription,
                        businessUnitName = currUser.businessUnitName,
                        staffRoleId = userRole.lookupId
                    }
                });

                //var result = new
                //{
                //    success = true,
                //    access_token = encodedToken,
                //    expiration = validTo,
                //    userInfo = new UserInfo
                //    {
                //        branchName = currUser.branchName,
                //        companyName = currUser.companyName,
                //        userName = currUser.username,
                //        activities = userActivities,
                //        staffId = currUser.staffId,
                //        staffName = currUser.staffName,
                //        sessionStatusInfo = currUser.sessionStatusInfo,
                //        applicationDate = _genSetup.GetApplicationDate(),
                //        lastLoginDate = currUser.lastLoginDate,
                //        staffRole = userRole.lookupName,
                //        corrMatrixId = currUser.corrMatrixId,
                //        corrMatrixDescription = currUser.corrMatrixDescription,
                //        businessUnitName = currUser.businessUnitName,
                //        staffRoleId = userRole.lookupId
                //    }
                //};

                //var sResult = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                ////var dResult = Newtonsoft.Json.JsonConvert.DeserializeObject(sResult);
                //var encryptedResult = Common.Crypto.Crypto.NgEncrypt(sResult, "SSljsdkkdlo4454M", "kljsdkkdlo4454GG");
                //return Request.CreateResponse(HttpStatusCode.OK, encryptedResult);
            }
            catch (SecureException ex)
            {
                //{ throw ex; }
                //string str = string.Empty;
                //_errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                //if (CommonHelpers.IsNumeric(CommonHelpers.Left(ex.Message, 4)))
                //{
                //    str = ex.Message.Replace("1001", "");
                //}
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            
        }
    }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        [HttpPost] //[ClaimsAuthorization]
        [Route("endpendingsession")]
        public IHttpActionResult SignOutUser([FromBody] TokenVM user)
        {
            //try
            //{

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

            //}
            //catch (SecureException ex)
            //{
            //    _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);

            //    return this.Ok(new { success = false, message = $"An unknown error occured while generate token {ex.Message}" });
            //}
        }

        [HttpPost] //[ClaimsAuthorization]
        [Route("logOut")]
        public IHttpActionResult LogOut()
        {
            //try
            //{
            _repo.ClearLoginToken(token.GetUsername);
            var staffDetails = _repo.GetSingleUserByUserName(token.GetUsername);

            if (staffDetails == null)
            {
                return this.Ok(new { success = false, message = "User Not Found" });
            }


            //Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            var authTypes = new string[] { DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.ExternalBearer, DefaultAuthenticationTypes.TwoFactorCookie, CookieAuthenticationDefaults.AuthenticationType, "Bearer" };
            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var audit = new TBL_AUDIT()
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoggedOut,
                STAFFID = token.GetStaffId,
                BRANCHID = (short)token.GetBranchId,
                DETAIL = $"{token.GetUsername} logged out",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),//CommonHelpers.GetUserIP(),
                URL = Request.RequestUri.AbsoluteUri,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = -1,
                OSNAME = CommonHelpers.FriendlyName()
            };

            _auditTrail.AddAuditTrail(audit);

            _context.SaveChanges();
            
            return this.Ok(new { success = true, message = "User Logged Off" });

            //}
            //catch (SecureException ex)
            //{
            //    _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
            //    return this.Ok(new { success = false, message = $"An unknown error occured {ex.Message}" });
            //}

        }

        [HttpPost] //[ClaimsAuthorization]
        [Route("logout-idle")]
        public IHttpActionResult LogOutIdle()
        {
            //try
            //{
            var isFirstLogOut = _repo.ClearLoginToken(token.GetUsername);
            //if (!isFirstLogOut)
            //{
            //    return this.Ok(new { success = true, message = "User Logged Off" });
            //}
            var staffDetails = _repo.GetSingleUserByUserName(token.GetUsername);

            if (staffDetails == null || staffDetails.username == "")
            {
                return this.Ok(new { success = false, message = "User Not Found" });
            }


            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);


            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoggedOut,
                STAFFID = token.GetStaffId,
                BRANCHID = (short)token.GetBranchId,
                DETAIL = $"{token.GetUsername} logged out due to system idle timeout " + (DateTime.Now).ToLongTimeString(),
                IPADDRESS = CommonHelpers.GetLocalIpAddress(), //CommonHelpers.GetUserIP(),
                URL = Request.RequestUri.AbsoluteUri,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = -1,
                OSNAME = CommonHelpers.FriendlyName()
            };

            _auditTrail.AddAuditTrail(audit);

            _context.SaveChanges();
            
            return this.Ok(new { success = true, message = "User Logged Off" });

            //}
            //catch (SecureException ex)
            //{
            //    _errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
            //    return this.Ok(new { success = false, message = $"An unknown error occured {ex.Message}" });
            //}

        }

        [HttpPost] //[ClaimsAuthorization]
        [Route("passwordchange")]
        public IHttpActionResult PasswordChange(PasswordChangeViewModel pwdChange)
        {

            if (!_repo.ValidateOldPassword(pwdChange.username, StaticHelpers.EncryptSha512(pwdChange.currentPassword, StaticHelpers.EncryptionKey)))
            {
                return this.Ok(new { success = false, message = "Invalid current password." });
            }
            if (!_repo.ValidatePasswordPolicy(pwdChange.newPassword))
            {
                return this.Ok(new { success = false, message = "Password must contain at least 8 characters, a number, lowercare and uppercase" });
            }
            var password = new PasswordChangeViewModel
            {
                username = pwdChange.username,
                currentPassword = StaticHelpers.EncryptSha512(pwdChange.currentPassword, StaticHelpers.EncryptionKey),
                newPassword = StaticHelpers.EncryptSha512(pwdChange.newPassword, StaticHelpers.EncryptionKey),
            };

            var res = _repo.PasswordChange(password);
            return this.Ok(new { success = true, message = "Password Change was successful" });

        }

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

    }

}