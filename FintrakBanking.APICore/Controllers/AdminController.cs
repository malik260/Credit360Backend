
using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces;
using FintrakBanking.ViewModels.Reports;
using System.Text;
using FintrakBanking.ViewModels.SupportUtility;
using FintrakBanking.Interfaces.Credit;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/admin")]
    public class AdminController : ApiControllerBase
    {
        private IProfileSetupRepository profileSetup;
        private readonly IAdminRepository repo;
        private readonly IErrorLogRepository errorLogger;
        private readonly ICanAuthorizationRepository canAuthorization;
        private readonly IAuditTrailRepository audit;
        private readonly IAPIErrorLog _log;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public AdminController(IAdminRepository _repo,
                                IErrorLogRepository _errorLogger,
                                ICanAuthorizationRepository _canAuthorization,
                                IAuditTrailRepository _audit, IProfileSetupRepository _profileSetup,
                                IAPIErrorLog log
                    )
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
            this.audit = _audit;
            this.profileSetup = _profileSetup;
            this.canAuthorization = _canAuthorization;
            _log = log;

        }


        private string username { get { return token.GetUsername; } }

     
        #region Users

        // AdminController cont = new AdminController();



        [HttpGet]
        [ClaimsAuthorization]
        [Route("users")]
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                var users = repo.GetAllUsers().ToList();

                if (!users.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = users.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("users-by-staffId/")]
        public HttpResponseMessage GetUsersByStaffId(int staffId)
        {
            try
            {
                var users = repo.GetUsersByStaffId(staffId);

                if (users == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = users });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("user/approval")]
        public HttpResponseMessage GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "User account has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("user-account-status-update/approval")]
        public HttpResponseMessage GoForUserAccountStatusApproval([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForUserAccountStatusApproval(entity);

                if (data == 1)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation successful, request has been routed to the next approving office" });

                else if (data == 2)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "User account Status Change has been approved successfully" });

                else if (data == 3)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "User account Status Change has been disapproved successfully" });

                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "Operation unsuccessful, an error occured while saving changes. " });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("user/approvals/temp")]
        public HttpResponseMessage GetUsersAwaitingApproval()
        {
            try
            {
                var staffinfo = repo.GetUsersAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (!staffinfo.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo.ToList() });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("user/account-status/approvals/temp")]
        public HttpResponseMessage GetUsersWithAccountStatusChangeAwaitingApproval()
        {
            try
            {
                var staffinfo = repo.GetUsersWithAccountStatusChangeAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (!staffinfo.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo.ToList() });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("user")]
        public async Task<HttpResponseMessage> AddUserAsync([FromBody]AppUserViewModel user)
        {
            try
            {
                if (canAuthorization.CanPerformActionOnResource(token.GetUserId, 2, UserActions.Add))
                {
                    if (repo.isUserExist(user.username))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { suucess = false, message = "A user with this username already exist" });
                    }
                    if (repo.isStaffExist(user.staffId))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest,
                           new { suucess = false, message = "Selected staff is already a user." });
                    }
                    user.createdBy = token.GetStaffId;
                    user.userBranchId = (short)token.GetBranchId;
                    user.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                    user.applicationUrl = HttpContext.Current.Request.Path;
                    user.companyId = token.GetCompanyId;
                    var result = repo.CreateUser(user);
                    if (result)
                    {
                        //repo.CreateUser(user);

                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = true, result = user, message = "User has been created successfully, now awaiting approval" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, result = user, message = "User not created successfully" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "You do not have enough right to add user" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }
        [HttpPut]
        [ClaimsAuthorization]
        [Route("user/{id}")]
        public async Task<HttpResponseMessage> UpdateUser(int id, [FromBody]AppUserViewModel user)
        {
            try
            {

                user.createdBy = token.GetStaffId;
                user.userBranchId = (short)token.GetBranchId;
                user.userIPAddress = HttpContext.Current.Request.Url.AbsoluteUri;
                user.applicationUrl = HttpContext.Current.Request.Path;
                user.companyId = token.GetCompanyId;
                var data = repo.UpdateUser(id, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = user, message = "User has been updated successfully, now awaiting approval" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = user, message = "User not updated successfully" });
                }
            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-user-activities")]
        public HttpResponseMessage GetUserActivitiesByCurrentUser()
        {
            try
            {
                var act = repo.GetUserActivitiesByUser(token.GetStaffId);

                if (act == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = act });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPut]
        //[ClaimsAuthorization]
        //[Route("manage-account-status/user/{userId}/lock-status/{lockStatus}")]
        //public IHttpActionResult ManageUserAccountStatus(int userId, int lockStatus)
        //{
        //    try
        //    {
        //        var data = repo.ManageUserAccount(userId, lockStatus);


        //        return this.Ok(new { data });
        //        //  return Request.CreateResponse(HttpStatusCode.OK, new { data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
        //        return this.Ok(new { success = true, message = $"An unknown error occured {ex.Message}" });
        //        // return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An unknown error occured {ex.Message}" });
        //    }
        //}

        #endregion


        #region Group

        [HttpPost]
        [ClaimsAuthorization]
        [Route("group/add")]
        public HttpResponseMessage AddGroup([FromBody] AppGroupViewModel group)
        {
            try
            {


                if (repo.isGroupExist(group.groupName))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { suucess = false, message = $"{group.groupName} already exit" });
                }

                group.createdBy = token.GetStaffId;
                group.userBranchId = (short)token.GetBranchId;
                //group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                group.applicationUrl = HttpContext.Current.Request.Path;
                group.companyId = token.GetCompanyId;

                var data = repo.AddGroup(group);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = group, message = "Group has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "An unknown error has occured" });

            }
            catch (SecureException ex)
            {
                //  this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }


        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("global-settings")]
        public HttpResponseMessage GetAllGlobalSettings()
        {
            try
            {
                var globalSettings = repo.GetAllGlobalSettings();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = globalSettings });
            }
            catch (SecureException ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                   new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("group/{id}")]
        public HttpResponseMessage UpdateGroup([FromBody] AppGroupViewModel group, short id)
        {
            //[FromBody]
            var req = this.Request;
            try
            {
                group.createdBy = token.GetStaffId;
                group.userBranchId = (short)token.GetBranchId;
                //group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                //group.applicationUrl = Request.Path.Value;
                group.companyId = token.GetCompanyId;

                var data = repo.UpdateGroup(id, group);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = group, message = "Group has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "An unknown error has occured" });

            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("groups")]
        public HttpResponseMessage GetAllGroups()
        {
            try
            {
                var groups = repo.GetAllGroups().ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = groups });
            }
            catch (SecureException ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("group/{id}")]
        public HttpResponseMessage GetGroupById(int id)
        {
            try
            {
                var group = repo.GetSingleGroup(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = group });
            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }

        }

        #endregion

        #region Activities
        [HttpGet]
        [ClaimsAuthorization]
        [Route("activities/parents")]
        public HttpResponseMessage GetAllActivities()
        {
            try
            {
                var groups = repo.GetActivities().ToList();
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = groups });
            }
            catch (SecureException ex)
            {
               // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("activities/parents/details/{parentId}")]
        public HttpResponseMessage GetAllActivityDetails(int parentId)
        {
            try
            {
                var groups = repo.GetActivityDetails(parentId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = groups });
            }
            catch (SecureException ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("staff-activity/{activity}")]
        public HttpResponseMessage StaffHasActivity( string activity)
        {
            try
            {
                var res = repo.StaffHasActivity(token.GetStaffId,activity);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("group/activities/mapped")]
        public HttpResponseMessage GetGroupActivities()
        {
            try
            {
                var groups = repo.GetGroupActivities().ToList();
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = groups });
            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }

        }



        [HttpPut]
        [ClaimsAuthorization]
        [Route("group/activity/access/{id}")]
        public HttpResponseMessage AddAccessToActivity(int id, [FromBody] ActivitiesUpdateVm model)
        {

            //[FromBody]
            var req = this.Request;
            try
            {
                var data = repo.AddAccessToActivity(id, model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, message = "Access right has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "An unknown error has occured" });

            }
            catch (SecureException ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }

        }

        #endregion

        #region Audit Trail
        [HttpGet]
        [ClaimsAuthorization]
        [Route("audit/dormant-staff-log")]
        public HttpResponseMessage GetAuditDormantStaffLog()
        {
            try
            {
                var allDeletedStaffLog = audit.GetDormantStaffLog((short)token.GetBranchId);
                int totalItems = allDeletedStaffLog.Count();

                //allDeletedStaffLog = allDeletedStaffLog.OrderBy(x => x.lastLoginDate);

                var data = allDeletedStaffLog.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = totalItems });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("audit/deleted-staff-log")]
        public HttpResponseMessage GetAuditDeletedStaffLog()
        {
            try
            {
                var allDeletedStaffLog = audit.GetDeletedStaffLog((short)token.GetBranchId);
                int totalItems = allDeletedStaffLog.Count();

                allDeletedStaffLog = allDeletedStaffLog.OrderBy(x => x.deletedDate);

                var data = allDeletedStaffLog.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = totalItems });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("audit/deleted-staff-log/search")]
        public HttpResponseMessage FilterDeletedStaffLog([FromUri] int page, string searchQuery)
        {
            try
            {
                var allAuditLog = audit.GetAuditTrail((short)token.GetBranchId).Where(x => x.auditTypeId == 130);

                allAuditLog = allAuditLog.OrderBy(x => x.systemDate).Skip(page)
                    .Where(x => x.auditType.ToLower().Contains(searchQuery.ToLower())
                    || x.firstName.ToLower().Contains(searchQuery) || x.lastName.ToLower().Contains(searchQuery)
                    );

                var data = allAuditLog.ToList();

                int totalItems = allAuditLog.Count();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = allAuditLog.ToList(), count = allAuditLog.Count() });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("audit/log")]
        public HttpResponseMessage GetAuditLog([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var allAuditLog = audit.GetAuditTrail((short)token.GetBranchId);
                int totalItems = allAuditLog.Count();

                allAuditLog = allAuditLog.OrderBy(x => x.systemDate).Skip(page).Take(itemsPerPage);

                var data = allAuditLog.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = totalItems });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("audit/log/search")]
        public HttpResponseMessage FilterAuditLog([FromUri] int page, string searchQuery)
        {
            try
            {
                var allAuditLog = audit.GetAuditTrail((short)token.GetBranchId);

                allAuditLog = allAuditLog.OrderBy(x => x.systemDate).Skip(page)
                    .Where(x => x.auditType.ToLower().Contains(searchQuery.ToLower())
                    || x.firstName.ToLower().Contains(searchQuery) || x.lastName.ToLower().Contains(searchQuery)
                    );

                var data = allAuditLog.ToList();

                int totalItems = allAuditLog.Count();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = allAuditLog.ToList(), count = allAuditLog.Count() });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }
        #endregion

        #region Administration
        [HttpGet]
        [ClaimsAuthorization]
        [Route("accountmanagement")]
        public IHttpActionResult GetAllApplicationUsers([FromUri] int page, [FromUri] int itemsPerPage)
        {
            var data = repo.GetActiveUsers(token.GetCompanyId);
            int totalItems = data.Count();
            data = data.OrderBy(x => x.staffName);//.Skip(page).Take(itemsPerPage);               
            return Ok(new { success = true, result = data.ToList(), count = data.Count() });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("accountmanagement")]
        public IHttpActionResult LogUserStatusUpdateRequest([FromBody] ActiveUserDetails entity)
        {
            if (entity != null)
            {
                string message = string.Empty;
                entity.lastUpdatedBy = token.GetUserId;
                entity.companyId = token.GetCompanyId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.createdBy = token.GetStaffId;
                var data = repo.LogUserStatusUpdateRequest(entity, out message);
                if (data)
                    return Ok(new { success = true, result = data, message =  $"Account Status Change was successful and currently undergoing approval."  });
            }

            return Ok(new { success = false, message = $"Account Status Change failed" });
            //try
            //{
                
            //}
            //catch (SecureException ex)
            //{
            //    return Ok(new { success = false, message = $"Action Failed" });
            //}

        }

       

        [HttpGet]
        [ClaimsAuthorization]
        [Route("getprofileconfiguration")]
        public HttpResponseMessage GetProfileConfiguration()
        {
            try
            {
                var data = profileSetup.GetProfileConfiguration();

                if (data == null)
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, result = data, message = $"Record not fund" });
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, result = data, message = $"Record not fund" });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, (new { success = false, message = $"Action Failed" }));
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("profilesettings")]
        public HttpResponseMessage GetAllApplicationProfileSettings()
        {
            try
            {
                var data = profileSetup.GetProfileSettings();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
         
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("updateprofilesettings")]
        public IHttpActionResult UpdateProfileSettings([FromBody] ProfileSettingViewModel entity)
        {
            try
            {
                if (entity != null)
                {
                    string message = string.Empty;
                    entity.lastUpdatedBy = token.GetUserId;
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.createdBy = token.GetStaffId;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.userIPAddress = Request.RequestUri.Host;

                    var data = profileSetup.UpdateProfileConfiguration(entity);
                    if (data != null)
                        return Ok(new { success = true, result = data, message = message == string.Empty ? $"Record Updated Successfully" : message });
                }

                return Ok(new { success = false, message = $"Record not fund" });
            }
            catch (SecureException ex)
            {
                return Ok(new { success = false, message = $"Action Failed" });
            }

        }
        #endregion 

        #region Two Factor Authentication
        [HttpGet]
        [ClaimsAuthorization]
        [Route("two-factor-auth")]
        public HttpResponseMessage TwoFactorAuthentication(string staffCode, string passCode)
        {
            try
            {
                if (repo.TwoFactorAuthenticationEnabled() == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Two Factor Authentication not enabled" });
                }

                var data = repo.TwoFactorAuthentication(staffCode, passCode);

                if (data.authenticated == true)
                {
                       return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data, message = data.message });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = false, result = data, message = data.message });
            }
            catch (TwoFactorAuthenticationException et)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = et.Message });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
           
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("two-factor-auth-enabled")]
        public HttpResponseMessage TwoFactorAuthenticationEnabled()
        {
            try
            {
                var data = repo.TwoFactorAuthenticationEnabled();
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = data, message = "" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
     
        [HttpGet]
        [ClaimsAuthorization]
        [Route("two-factor-auth-last-approval")]
        public HttpResponseMessage TwoFactorAuthenticationEnabled(int operationId,int? productClassId, int? productId, decimal levelAmount = 0)
        {
            
                var data = repo.Enable2FAForLastApproval(token.GetStaffId, operationId, productClassId ,productId, levelAmount);
                if (!data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = data, message = "" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
        }
        #endregion

        [HttpPost]
        [ClaimsAuthorization]
        [Route("api-log")]
        public HttpResponseMessage GetAPILog([FromBody] DateRange range)
        {
            try
            {
                if (range != null)
                {
                    string message = string.Empty;
                    var data = _log.GetAPILog(range.startDate, range.endDate, range.loanRefNo);
                    if (data!=null)
                        return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = data, message = "" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                                     new { success = true, result = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                     new { success = true, result = "No Record Found" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("error-log")]
        public HttpResponseMessage GetErrorLog([FromBody] DateRange range)
        {
            try
            {
                if (range != null)
                {
                    string message = string.Empty;
                    var data = _log.GetErrorLog(range.startDate, range.endDate);
                    if (data != null)
                        return Request.CreateResponse(HttpStatusCode.OK,
                                              new { success = false, result = data, message = "" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                                     new { success = true, result = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                     new { success = true, result = "No Record Found" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("getStaffactiveDirectoryDetails/{staffCode}/{passCode}")]
        public HttpResponseMessage GetStaffADDetails(string staffCode, string passCode)
        {
            try
            {
                byte[] pass = Convert.FromBase64String(passCode);
                string password = Encoding.UTF8.GetString(pass);
                string loginUser = token.GetUsername;

                var data = repo.GetStaffActiveDirectoryDetails(staffCode,loginUser, password);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = false, result = data, message = $"Staff Code not fund on Active Directory" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                         new { success = true, result = data, message = $"Success" });
                }


            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, (new { success = false, message = $"Action Failed" }));
            }

        }

       

    }
}