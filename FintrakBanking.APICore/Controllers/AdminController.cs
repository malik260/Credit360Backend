
using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.Admin;
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
            return Ok( new { result = users });
        }

        [HttpPost]
        [Route("user")]
        public HttpResponseMessage AddUser(  [FromBody]AppUserViewModel user)
        {
            TokenDecryptionHelper token = null;

            token = new TokenDecryptionHelper();
            if (I.CanPerformActionOnResource(token.GetUserId, 2, UserActions.Add))
            {
                if (repo.iSUserExit(user.username))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { suucess = false, message = "A user with this username already exit" });
                }

                user.createdBy = token.GetStaffId;
                user.userBranchId = (short)token.GetBranchId;
                //user.userIPAddress =  Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                user.applicationUrl = HttpContext.Current.Request.Path;
                user.companyId = token.GetCompanyId;
                var result = repo.CreateUser(user).IsCompleted;
                if (result)
                {
                    repo.CreateUser(user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = user, message = "User has been created successfully" });
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "You do not have enough right to add user" });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "An unknown error has occured" });

        } 
        [HttpPut]
        [Route("user/{id}")]
        public HttpResponseMessage UpdateUser(int id, [FromBody]AppUserViewModel user)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                user.createdBy = token.GetStaffId;
                user.userBranchId = (short)token.GetBranchId;
                // user.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                user.applicationUrl = HttpContext.Current.Request.Path;
                user.companyId = token.GetCompanyId;
                var data = repo.UpdateUser(id, user);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                         new { success = true, result = user, message = "User has been created successfully" });
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


        #region Group

        [HttpPost]
        [Route("group/add")]
        public HttpResponseMessage AddGroup(  [FromBody] AppGroupViewModel group)
        {
            try
            {


                if (repo.iSGroupExist(group.groupName))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { suucess = false, message = $"{group.groupName} already exit" });
                }

                var token = new TokenDecryptionHelper();
                group.createdBy = token.GetStaffId;
                group.userBranchId = (short)token.GetBranchId;
                //group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                group.applicationUrl = HttpContext.Current.Request.Path;
                group.companyId = token.GetCompanyId;

                var data = repo.AddGroup(group);
                if (data.IsCompleted)
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
        public HttpResponseMessage UpdateGroup(  [FromBody] AppGroupViewModel group, short id)
        {
            //[FromBody]
            var req = this.Request;
            TokenDecryptionHelper token = null;
            try
            {

                //   token = new TokenDecryptionHelper(this.HttpContext);
                group.createdBy = token.GetStaffId;
                group.userBranchId = (short)token.GetBranchId;
                //group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                //group.applicationUrl = Request.Path.Value;
                group.companyId = token.GetCompanyId;

                var data = repo.UpdateGroup(id, group);
                if (data.IsCompleted)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = group, message = "Group has been updated successfully" });
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



        [HttpGet]
        [Route("groups")]
        public HttpResponseMessage GetAllGroups( )
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper();
                var groups = repo.GetAllGroups().ToList();
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = groups });
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
        public HttpResponseMessage GetGroupById(  int id)
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper();
                var group = repo.GetSingleGroup(id);
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = group });
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
        public HttpResponseMessage GetAllActivities( )
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper();
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
        public HttpResponseMessage GetGroupActivities( )
        {

            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper();
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
        public HttpResponseMessage AddAccessToActivity(  int id, [FromBody] ActivitiesUpdateVm model)
        {

            //[FromBody]
            var req = this.Request;
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper();
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

        // [HttpGet][Route("audit/log")]
        //public IHttpActionResult GetAuditLog([FromQuery] int page,[FromQuery] int itemsPerPage)
        //{
        //     TokenDecryptionHelper token = null;

        //    //token = new TokenDecryptionHelper(this.HttpContext);
        //    var allAuditLog = audit.GetAuditTrail((short)token.GetBranchId);
        //    int totalItems = allAuditLog.Count();

        //    allAuditLog = allAuditLog.OrderBy(x => x.systemDate).Skip(page).Take(itemsPerPage);

        //    var result = allAuditLog.ToList();


        //    returnnew { result = result, itemsPerPage = itemsPerPage, totalItems = totalItems });
        //}
    }
}