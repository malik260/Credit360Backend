using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FintrakBanking.APICore.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        private IAuditTrailRepository auditTrail;
        
        //private readonly FinTrakBankingContext _bankingContext;
        private TBL_SETUP_GLOBAL appSetup;
        private const string HttpContext = "MS_HttpContext";
        

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null) throw new ArgumentNullException("publicClientId");
            //this._bankingContext = new FinTrakBankingContext();
        }

        public string GetIpAddress(HttpRequestMessage request)
        {
            if (!request.Properties.ContainsKey(HttpContext)) return null;
            dynamic context = request.Properties[HttpContext];
            return context != null ? (string)context.Request.UserHostAddress : null;
        }

        // string IPAddress = 
        public string GetIpAddress()
        {
            string ipAddress = string.Empty;
            IPHostEntry Host = default(IPHostEntry);
            string hostname = null;
            hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(hostname);
            foreach (IPAddress ip in Host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = Convert.ToString(ip);
                }
            }
            return ipAddress;
        }

        //public override async Task GrantResourceOwnerCredentialsOld(OAuthGrantResourceOwnerCredentialsContext context)
        //{
        //    //var origin = context.OwinContext.Request.Headers["Origin"];
        //    try
        //    {
        //        var test = context.Password.EncryptSha512(StaticHelpers.EncryptionKey);
        //        string ipAddress = GetIpAddress();
        //        UserViewModel user = null;

        //        byte[] pass = Convert.FromBase64String(context.Password);
        //        string password = Encoding.UTF8.GetString(pass);
        //        var test2 = password.EncryptSha512(StaticHelpers.EncryptionKey);


        //        //var profile2 = _bankingContext.TBL_PROFILE_USER.FirstOrDefault(c => c.USERNAME.ToLower() == context.UserName.ToLower());// && c.PASSWORD == password);

        //        var exipredHr = int.Parse(ConfigurationManager.AppSettings["tokenExpiryHour"]);
        //        var exipredMin = int.Parse(ConfigurationManager.AppSettings["tokenExpiryMinute"]);
        //        var exipredSec = int.Parse(ConfigurationManager.AppSettings["tokenExpirySecond"]);

        //        var userVm = new UserViewModel
        //        {
        //            //StaticHelpers.EncryptSha512(password, StaticHelpers.EncryptionKey);
        //            password = password.EncryptSha512(StaticHelpers.EncryptionKey),
        //            username = context.UserName
        //        };
        //        ClaimsIdentity identity;

        //        FinTrakBankingContext _bankingContext = new FinTrakBankingContext();
        //        var authRepo = new AuthenticationRepository(_bankingContext,null,null);

               
        //        ActiveUserDetails userInfo = authRepo.GetUserAuthenticationInfo(userVm.username);
        //        ActiveUserDetails userInformation = authRepo.GetUserInformation(userVm.username);

        //        if (authRepo.GetRunningEndOfDayProcess(userInfo.companyId))
        //        {
        //            context.SetError("invalid_grant", "End of day process is running!");
        //            return;
        //        }

        //        if (userInfo.grantMessage != "valid")
        //        {
        //            context.SetError("invalid_grant", userInfo.grantMessage);
        //            return;
        //        }

        //        if (userInformation != null && userInformation.deleted)
        //        {
        //            context.SetError("invalid_grant", "User does not exist or have been deactivated!");
        //            return;
        //        }

        //        if (authRepo.ConcurrentUsers() > 2000) // 2000 setup somewhere
        //        {
        //            context.SetError("invalid_grant", "Maximum Concurrent Users exceeded!");
        //            return;
        //        }

        //        _bankingContext.TBL_SETUP_GLOBAL.AsNoTracking();
        //        _bankingContext.TBL_PROFILE_USER.AsNoTracking();

        //        appSetup = _bankingContext.TBL_SETUP_GLOBAL.FirstOrDefault();

        //        //appSetup.USE_ACTIVE_DIRECTORY = false;
        //        if (appSetup != null && appSetup.USE_ACTIVE_DIRECTORY)
        //        {
        //            if (Task.FromResult(
        //                ValidateActiveDirectoryCredentials(context.UserName, context.Password, out identity)
        //                ).Result)
        //            {
        //                authRepo.SessionInfo = authRepo.CheckSessionState(userVm.username.ToLower(), ipAddress);//.GetAwaiter().GetResult();
        //                user = authRepo.FindUserByUserName(userVm.username.ToLower());
        //                // user = await Task.FromResult(authRepo.FindUserByUserNameAsync(userVm.username.ToLower())).Result;
        //            }
        //            else
        //            {
        //                var profile = _bankingContext.TBL_PROFILE_USER.FirstOrDefault(c => c.USERNAME.ToLower() == userVm.username.ToLower());// && c.PASSWORD == password);
        //                if (profile != null)
        //                {
        //                    profile.LOGINCODE = null;
        //                    profile.FAILEDLOGONATTEMPT += 1;
        //                    int count = profile.FAILEDLOGONATTEMPT ?? 0;
        //                    TBL_PROFILE_SETTING prosett = new TBL_PROFILE_SETTING();
        //                    prosett = _bankingContext.TBL_PROFILE_SETTING.FirstOrDefault();
        //                    //if (count == CommonHelpers.MaxInvalidPasswordAttempts)
        //                    if (count == prosett.MAXINVALIDPASSWORDATTEMPTS)
        //                    {
        //                        profile.ISLOCKED = true;
        //                        profile.LASTLOCKOUTDATE = DateTime.Now;
        //                    }

        //                    //db.Entry(faileddata).State = EntityState.Modified;
        //                    _bankingContext.SaveChanges();

        //                }
        //                context.SetError("invalid_grant", "Login Failure.");
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            authRepo.SessionInfo = authRepo.CheckSessionState(userVm.username.ToLower(), ipAddress);//.GetAwaiter().GetResult();
        //            //user = await Task
        //            //    .FromResult(authRepo.FindUserByUserNameAndPassword(userVm.username.ToLower(), userVm.password))
        //            //    .Result;
        //            user = authRepo.FindUserByUserNameAndPassword(userVm.username.ToLower(), userVm.password);
        //        }

        //        if (user == null)
        //        {
        //            context.SetError("invalid_grant", "Login Failure.");
        //            return;
        //        }

        //        bool isUserAccountValid;

        //        //if (Task.FromResult(authRepo.IsUserAccountValid(userVm.username)).Result)
        //        //{
        //        //    isUserAccountValid = true;
        //        //}
        //        //else
        //        //{
        //        //    isUserAccountValid = false;
        //        //}

        //        //if (isUserAccountValid)
        //        //{
        //            var currIdentity = new ClaimsIdentity(context.Options.AuthenticationType);

        //            currIdentity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
        //            currIdentity.AddClaim(new Claim("username", user.username));
        //            currIdentity.AddClaim(new Claim("companyId", user.companyId.ToString()));
        //            currIdentity.AddClaim(new Claim("staffId", user.staffId.ToString()));
        //            currIdentity.AddClaim(new Claim("branchId", user.branchId.ToString()));
        //            currIdentity.AddClaim(new Claim("countryId", user.countryId.ToString()));
        //            currIdentity.AddClaim(new Claim("userId", user.user_id.ToString()));
        //            currIdentity.AddClaim(new Claim("roleId", user.roleId.ToString()));
        //            currIdentity.AddClaim(new Claim("logincode", user.logincode == null ? Guid.NewGuid().ToString()+"@"+ipAddress : user.logincode));
        //            var today = DateTime.Now;
        //            TimeSpan duration = new TimeSpan(exipredHr, exipredMin, exipredSec); //(exipredHr, 0, 0);

        //            var props = new AuthenticationProperties(new Dictionary<string, string>
        //            {
        //                {
        //                    "expiry_date", today.Add(duration).ToString("ddd MMM dd yyyy HH':'mm':'ss 'GMT'K")
        //                }
        //            });
              

        //            var ticket = new AuthenticationTicket(currIdentity, props);

        //            context.Validated(ticket);

        //            context.Request.Context.Authentication.SignIn(currIdentity);
              


        //        await Task.CompletedTask;

        //    }
        //    catch (Exception ex)
        //    {
        //        if (CommonHelpers.IsNumeric(CommonHelpers.Left(ex.Message, 4)))
        //        {
        //            string str = ex.Message.Replace("1001", "");
        //            context.SetError("invalid_grant", str);
        //            return;
        //        }
        //        else
        //        {
        //            string innerException = string.Empty;
        //            if (ex.InnerException != null) innerException = ex.InnerException.Message;
        //            string str = ex.Message + " : " + innerException;
        //            context.SetError("invalid_grant", str);
        //            return;
        //        }
        //    }
        //}

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var origin = context.OwinContext.Request.Headers["Origin"];
            try
            {
                string ipAddress = GetIpAddress();
                UserViewModel user = null;

                //var sanitizdPassword = context.Password.Replace(" ", "+");

                //var keybytes = Encoding.UTF8.GetBytes("7061737323313233");
                //var iv = Encoding.UTF8.GetBytes("7061737323313233");
                var sanitizdPassword = context.Password.Replace(" ", "+");


                //var encrypted = Convert.FromBase64String(sanitizdPassword);//Encoding.ASCII.GetBytes(context.Password);
                //var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);


                //string password = decriptedFromJavascript;// Encoding.UTF8.GetString(pass);

                byte[] pass = Convert.FromBase64String(sanitizdPassword);
                string password = Encoding.UTF8.GetString(pass);

                //var exipredHr = int.Parse(ConfigurationManager.AppSettings["tokenExpiryHour"]);                
                //var exipredSec = int.Parse(ConfigurationManager.AppSettings["tokenExpirySecond"]);

                var userVm = new UserViewModel
                {
                    password = password.EncryptSha512(StaticHelpers.EncryptionKey),
                    username = context.UserName
                };

                ClaimsIdentity identity;

                FinTrakBankingContext _bankingContext = new FinTrakBankingContext();
                var authRepo = new AuthenticationRepository(_bankingContext, null, null);

                // this section is for lisence validation

                /* license*/
               /* 
                    var result = authRepo.ExamineLicense();
                    var checkMessage = _bankingContext.TBL_RECORD_TRACKING.Where(x => DbFunctions.TruncateTime(x.CURRENTDATE) == DbFunctions.TruncateTime(DateTime.Now)).FirstOrDefault();
                if (checkMessage == null)
                {
                    var entity = new TBL_RECORD_TRACKING();
                    if (result.Status == LicenseStatus.CorruptLicenseFile)
                    {
                        //context.SetError("invalid_grant", "Currupt License File!");
                        //return;
                        entity.STATUSMESSAGE = "Currupt License File!";
                        entity.CURRENTDATE = DateTime.Now;
                    }
                    else if (result.Status == LicenseStatus.InvalidSignature)
                    {
                        //context.SetError("invalid_grant", "Invalid Signature!");
                        //return;
                        entity.STATUSMESSAGE = "Invalid Signature!";
                        entity.CURRENTDATE = DateTime.Now;
                    }
                    else if (result.Status == LicenseStatus.LicenseExpired)
                    {
                        // context.SetError("invalid_grant", "License Expired!");
                        // return;
                        entity.STATUSMESSAGE = "License Expired!";
                        entity.CURRENTDATE = DateTime.Now;
                    }
                    else if (result.Status == LicenseStatus.MissingLicenseFile)
                    {
                        //context.SetError("invalid_grant", "Missing License File!");
                        //return;
                        entity.STATUSMESSAGE = "Missing License File!";
                        entity.CURRENTDATE = DateTime.Now;
                    }
                    else if (result.Status == LicenseStatus.NotYetLicensed)
                    {
                        //context.SetError("invalid_grant", "Application Not Licensed!");
                        // return;
                        entity.STATUSMESSAGE = "Application Not Licensed!";
                        entity.CURRENTDATE = DateTime.Now;
                    }
                    else if (result.Status == LicenseStatus.VersionMismatch)
                    {
                        //context.SetError("invalid_grant", "Application License Version Mismatch!");
                        // return;
                        entity.STATUSMESSAGE = "Application License Version Mismatch!";
                        entity.CURRENTDATE = DateTime.Now;
                    }
                    var save = _bankingContext.TBL_RECORD_TRACKING.Add(entity);
                    _bankingContext.SaveChanges();
                }*/
                /*end of lisence validation*/


                ActiveUserDetails userInfo = authRepo.GetUserAuthenticationInfo(userVm.username);
                ActiveUserDetails userInformation = authRepo.GetUserInformation(userVm.username);

                if (authRepo.GetRunningEndOfDayProcess(userInfo.companyId))
                {
                    context.SetError("invalid_grant", "End of day process is running!");
                    return;
                }

                if (userInfo.grantMessage != "valid")
                {
                    context.SetError("invalid_grant", userInfo.grantMessage);
                    return;
                }

                if (userInformation != null && userInformation.deleted)
                {
                    context.SetError("invalid_grant", "User does not exist or have been deactivated!");
                    return;
                }

                if (authRepo.ConcurrentUsers() > 2000) // 2000 setup somewhere
                {
                    context.SetError("invalid_grant", "Maximum Concurrent Users exceeded!");
                    return;
                }

                _bankingContext.TBL_SETUP_GLOBAL.AsNoTracking();
                _bankingContext.TBL_PROFILE_USER.AsNoTracking();

                appSetup = _bankingContext.TBL_SETUP_GLOBAL.FirstOrDefault();
                var record = (from a in _bankingContext.TBL_PROFILE_USER
                              join b in _bankingContext.TBL_STAFF on a.STAFFID equals b.STAFFID
                              join c in _bankingContext.TBL_STAFF_ROLE on b.STAFFROLEID equals c.STAFFROLEID
                              where a.USERNAME.ToUpper() == context.UserName.ToUpper() && !b.DELETED
                              select new
                              {
                                  a,
                                  c.STAFFROLECODE,
                                  c.WORKSTARTDURATION,
                                  c.WORKENDDURATION
                              }).FirstOrDefault();



                //if (record.WORKSTARTDURATION == null || record.WORKENDDURATION == null)
                //{

                //}
                //else
                //{
                //    if (record.WORKSTARTDURATION > 0 || record.WORKENDDURATION > 0)
                //    {
                //        if (DateTime.Now.Hour < record.WORKSTARTDURATION)
                //        {
                //            context.SetError("invalid_grant", "Warning!!! You Are Not Allowed to Login At the Moment...");
                //            return;
                //        }
                //        if (DateTime.Now.Hour > record.WORKENDDURATION)
                //        {
                //            context.SetError("invalid_grant", "Your Working Hours For The Day Has Ellapsed !!!");
                //            return;
                //        }
                //    }
                //}

                //appSetup.USE_ACTIVE_DIRECTORY = false;

                

                if (appSetup != null && appSetup.USE_ACTIVE_DIRECTORY && record.STAFFROLECODE != "SYSADM")
                {
                    if (Task.FromResult(
                        ValidateActiveDirectoryCredentials(context.UserName, password, out identity)
                        ).Result)
                    {
                        authRepo.SessionInfo = authRepo.CheckSessionState(userVm.username.ToLower(), ipAddress);//.GetAwaiter().GetResult();
                        user = authRepo.FindUserByUserName(userVm.username.ToLower());
                        // user = await Task.FromResult(authRepo.FindUserByUserNameAsync(userVm.username.ToLower())).Result;
                    }
                    else
                    {
                        var profile = _bankingContext.TBL_PROFILE_USER.FirstOrDefault(c => c.USERNAME.ToLower() == userVm.username.ToLower());// && c.PASSWORD == password);
                        if (profile != null)
                        {
                            profile.LOGINCODE = null;
                            //int count = profile.FAILEDLOGONATTEMPT ?? 0;
                            int count = profile.FAILEDLOGONATTEMPT == 0 ? 1 : profile.FAILEDLOGONATTEMPT.Value + 1;
                            profile.FAILEDLOGONATTEMPT += 1;

                            TBL_PROFILE_SETTING prosett = new TBL_PROFILE_SETTING();
                            prosett = _bankingContext.TBL_PROFILE_SETTING.FirstOrDefault();
                            //if (count == CommonHelpers.MaxInvalidPasswordAttempts)
                            if (count == prosett.MAXINVALIDPASSWORDATTEMPTS)
                            {
                                profile.ISLOCKED = true;
                                profile.LASTLOCKOUTDATE = DateTime.Now;
                            }

                            //db.Entry(faileddata).State = EntityState.Modified;
                            _bankingContext.SaveChanges();

                        }
                        context.SetError("invalid_grant", "Active Directory Login Failure.");
                        return;
                    }
                }
                else
                {
                    authRepo.SessionInfo = authRepo.CheckSessionState(userVm.username.ToLower(), ipAddress);//.GetAwaiter().GetResult();
                    //user = await Task
                    //    .FromResult(authRepo.FindUserByUserNameAndPassword(userVm.username.ToLower(), userVm.password))
                    //    .Result;
                    user = authRepo.FindUserByUserNameAndPassword(userVm.username.ToLower(), userVm.password);
                }

                if (user == null)
                {
                    context.SetError("invalid_grant", "Invalid Username Or Password");
                    return;
                }

                bool isUserAccountValid;

                //if (Task.FromResult(authRepo.IsUserAccountValid(userVm.username)).Result)
                //{
                //    isUserAccountValid = true;
                //}
                //else
                //{
                //    isUserAccountValid = false;
                //}


                if (user.logincode == null)
                {
                    //ify, to eliminate multiple sources of truth for the logincode
                    var profile = _bankingContext.TBL_PROFILE_USER.FirstOrDefault(p => p.USERNAME == user.username);
                    var loginCode = Guid.NewGuid().ToString() + "@" + ipAddress;
                    profile.LOGINCODE = loginCode;
                    user.logincode = loginCode;
                    _bankingContext.SaveChanges();
                }

                var userActivities = authRepo.GetUserActivitiesByUser(user.user_id);
                string userActivitiesJson = JsonConvert.SerializeObject(userActivities);

                var currIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
                
                currIdentity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                currIdentity.AddClaim(new Claim("username", user.username));
                currIdentity.AddClaim(new Claim("companyId", user.companyId.ToString()));
                currIdentity.AddClaim(new Claim("staffId", user.staffId.ToString()));
                currIdentity.AddClaim(new Claim("branchId", user.branchId.ToString()));
                currIdentity.AddClaim(new Claim("countryId", user.countryId.ToString()));
                currIdentity.AddClaim(new Claim("userId", user.user_id.ToString()));
                currIdentity.AddClaim(new Claim("roleId", user.roleId.ToString()));
                currIdentity.AddClaim(new Claim("userGroupId", user.userGroupId.ToString()));
                currIdentity.AddClaim(new Claim("usersRole", record.STAFFROLECODE.ToString()));
                currIdentity.AddClaim(new Claim("logincode", user.logincode == null ? Guid.NewGuid().ToString() + "@" + ipAddress : user.logincode));
                currIdentity.AddClaim(new Claim("userActivities", userActivitiesJson));
                var today = DateTime.Now;

                var exipredMin = int.Parse(ConfigurationManager.AppSettings["tokenExpiryMinute"]);
                TimeSpan duration = new TimeSpan(0, exipredMin, 0); //(exipredHr, 0, 0);

                var props = new AuthenticationProperties(new Dictionary<string, string>
                    {
                        {
                            "expiry_date", today.Add(duration).ToString("ddd MMM dd yyyy HH':'mm':'ss 'GMT'K")
                        }
                    });


                var ticket = new AuthenticationTicket(currIdentity, props);
                context.Validated(ticket);

                context.Request.Context.Authentication.SignIn(currIdentity);



                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                if (CommonHelpers.IsNumeric(CommonHelpers.Left(ex.Message, 4)))
                {
                    string str = ex.Message.Replace("1001", " ");
                    context.SetError("invalid_grant", str);
                    return;
                }
                else
                {
                    string innerException = string.Empty;
                    if (ex.InnerException != null) innerException = ex.InnerException.Message;
                    string str = ex.Message + " : " + innerException;
                    context.SetError("invalid_grant", str);
                    return;
                }
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

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == this._publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }


        public bool ValidateActiveDirectoryCredentials(string userName, string password, out ClaimsIdentity identity)
        {
            //appSetup = _bankingContext.TBL_SETUP_GLOBAL.FirstOrDefault();

            if (appSetup != null && appSetup.REQUIRE_ADUSER)
            {
                using (var pc = new PrincipalContext(ContextType.Domain, appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, appSetup.ACTIVE_DIRECTORY_USERNAME, appSetup.ACTIVE_DIRECTORY_PASSWORD))
                {
                    bool isValid = pc.ValidateCredentials(userName, password);
                    if (isValid)
                    {
                        identity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
                        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

                        SaveToAPIlogSuccess(appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, userName, isValid);
                    }
                    else
                    {
                        identity = null;
                        SaveToAPIlogFail(appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, userName, isValid);
                    }

                    return isValid;
                }
            }
            else
            {
                if (appSetup != null)
                    using (var pc = new PrincipalContext(ContextType.Domain, appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME))
                    {
                        bool isValid = pc.ValidateCredentials(userName, password);
                        if (isValid)
                        {
                            identity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);
                            identity.AddClaim(new Claim(ClaimTypes.Name, userName));

                            SaveToAPIlogSuccess(appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, userName, isValid);
                        }
                        else
                        {
                            identity = null;
                            SaveToAPIlogFail(appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME, userName, isValid);
                        }

                        return isValid;
                    }
            }
            identity = null;
            return false;
        }

     public void SaveToAPIlogSuccess(string domain, string userName, bool isValid)
        {
            var logs = new TBL_CUSTOM_API_LOGS
            {
                APIURL = appSetup.ACTIVE_DIRECTORY_DOMAIN_NAME,
                LOGTYPEID = 4,
                REFERENCENUMBER = userName,
                REQUESTDATETIME = DateTime.UtcNow,
                REQUESTMESSAGE = userName,
                RESPONSEDATETIME = DateTime.UtcNow,
                RESPONSEMESSAGE = $"{isValid.ToString()}; AD Login Success!",
            };

            FinTrakBankingContext logContext = new FinTrakBankingContext();
            logContext.TBL_CUSTOM_API_LOGS.Add(logs);
            logContext.SaveChanges();
        }

        public void SaveToAPIlogFail(string domain, string userName, bool isValid)
        {
            var logs = new TBL_CUSTOM_API_LOGS
            {
                APIURL = domain,
                LOGTYPEID = 4,
                REFERENCENUMBER = userName,
                REQUESTDATETIME = DateTime.UtcNow,
                REQUESTMESSAGE = userName,
                RESPONSEDATETIME = DateTime.UtcNow,
                RESPONSEMESSAGE = $"{isValid.ToString()}; AD Login Failed!",
            };
            
            FinTrakBankingContext logContext = new FinTrakBankingContext();
            logContext.TBL_CUSTOM_API_LOGS.Add(logs);
            logContext.SaveChanges();

        }
    }
}
























