
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

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/admin")]
    public class AdminController : ApiControllerBase
    {

        private readonly IAdminRepository repo;
        private readonly IErrorLogRepository errorLogger;
        private readonly ICanAuthorizationRepository I;
        private readonly IAuditTrailRepository audit;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public AdminController(IAdminRepository _repo,
                                IErrorLogRepository _errorLogger,
                                ICanAuthorizationRepository _I,
                                IAuditTrailRepository _audit)
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
            this.audit = _audit;
            this.I = _I;
        }

        #region Users

        [HttpGet]
        [Route("users")]

        public IHttpActionResult GetAllUsers()
        {
            var users = repo.GetAllUsers().ToList();
            return Ok(new { result = users });
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
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
            catch (System.Exception ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        [Route("user")]
        public async Task<HttpResponseMessage> AddUserAsync([FromBody]AppUserViewModel user)
        {
            try
            {
                if (I.CanPerformActionOnResource(token.GetUserId, 2, UserActions.Add))
                {
                    if (repo.isUserExist(user.username))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { suucess = false, message = "A user with this username already exist" });
                    }

                    user.createdBy = token.GetStaffId;
                    user.userBranchId = (short)token.GetBranchId;
                    user.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                    user.applicationUrl = HttpContext.Current.Request.Path;
                    user.companyId = token.GetCompanyId;
                    var result = await repo.CreateUser(user);
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
        [HttpPut]
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
                var data = await repo.UpdateUser(id, user);
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
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("manage-account-status/user/{userId}/lock-status/{lockStatus}")]
        public HttpResponseMessage ManageUserAccountStatus(int userId, int lockStatus)
        {
            try
            {
                var data = repo.ManageUserAccount(userId, lockStatus);

                return Request.CreateResponse(HttpStatusCode.OK, new { data });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"An unknown error occured {ex.Message}" });
            }
        }

        #endregion


        #region Group

        [HttpPost]
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
            catch (Exception ex)
            {
                //  this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }


        }


        [HttpPut]
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
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }


        }



        [HttpGet]
        [Route("groups")]
        public HttpResponseMessage GetAllGroups()
        {
            try
            {
                var groups = repo.GetAllGroups().ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = groups });
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }

        [HttpGet]
        [Route("group/{id}")]
        public HttpResponseMessage GetGroupById(int id)
        {
            try
            {
                var group = repo.GetSingleGroup(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = group });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }

        }

        #endregion

        #region Activities
        [HttpGet]
        [Route("activities/parents")]
        public HttpResponseMessage GetAllActivities()
        {
            try
            {
                var groups = repo.GetActivities().ToList();
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = groups });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }


        }


        [HttpGet]
        [Route("group/activities/mapped")]
        public HttpResponseMessage GetGroupActivities()
        {
            try
            {
                var groups = repo.GetGroupActivities().ToList();
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = groups });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }

        }



        [HttpPut]
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
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }

        }

        #endregion

        #region Audit Trail
        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }
        #endregion

        #region Administration
        [HttpGet]
        [Route("accountmanagement")]
        public IHttpActionResult GetAllApplicationUsers([FromUri] int page, [FromUri] int itemsPerPage)
        {
            var data = repo.GetActiveUsers(token.GetCompanyId);
            int totalItems = data.Count();
            data = data.OrderBy(x => x.staffName);//.Skip(page).Take(itemsPerPage);               
            return Ok( new { success = true, result = data.ToList(), count = data.Count() });
             
        }


        [HttpPut]
        [Route("accountmanagement")]
        public IHttpActionResult UpdateApplicationUsers([FromBody] ActiveUserDetails entity)
        {
            try
            {
                if (entity != null)
            {
                    string message = string.Empty;
                    entity.lastUpdatedBy = token.GetUserId;
                var data = repo.UpdateUserStatus(entity, out message);
                if(data)
                return Ok(new { success = data, result = data, message = message == string.Empty ?  $"Account is cleared" : message });                 
            }

            return Ok(new { success = false,  message = $"Account not fund" });
        }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Action Failed" });
            }

        }
        #endregion
    }
}