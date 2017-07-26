
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
            return Ok(new { result = users });
        }

        [HttpPost]
        [Route("user")]
        public    HttpResponseMessage  AddUser(HttpRequestMessage request, [FromBody]AppUserViewModel user)
        {
            TokenDecryptionHelper token = null;
            HttpResponseMessage response = null;

            return   GetHttpResponse(request, () =>
            {
                token = new TokenDecryptionHelper();
                if (I.CanPerformActionOnResource(token.GetUserId, 2, UserActions.Add))
                {
                    if (repo.iSUserExit(user.username))
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { suucess = false, message = "A user with this username already exit" }));
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

                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, result = user, message = "User has been created successfully" }));
                    }
                }
                else
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "You do not have enough right to add user" }));
                }

                response = request.CreateResponse(HttpStatusCode.OK,
                      Ok(new { success = false, message = "An unknown error has occured" }));

                return response;
            });
        }



        [HttpPut]
        [Route("user/{id}")]
        public HttpResponseMessage UpdateUser(HttpRequestMessage request,int id, [FromBody]AppUserViewModel user)
        {
            HttpResponseMessage response = null;

            return GetHttpResponse(request, () =>
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                try
                {
                    //   token = new TokenDecryptionHelper(this.HttpContext);
                    //   user.createdBy = token.GetStaffId;
                    //user.userBranchId = (short)token.GetBranchId;
                    //user.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //user.applicationUrl = Request.Path.Value;
                    //user.companyId = token.GetCompanyId;
                    var data =   repo.UpdateUser(id, user);
                    if (data!= null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, result = user, message = "User has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "An unknown error has occured" }));
                }
                catch (Exception ex)
                {
                    // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" }));
                }

                return response;

            });
        }

        #endregion


        #region Group

        [HttpPost]
        [Route("group/add")]
        public HttpResponseMessage AddGroup(HttpRequestMessage request, [FromBody] AppGroupViewModel group)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {

                    TokenDecryptionHelper token = null;
                    if (repo.iSGroupExist(group.groupName))
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { suucess = false, message = $"{group.groupName} already exit" }));
                    }

                    // token = new TokenDecryptionHelper(this.HttpContext);
                    group.createdBy = token.GetStaffId;
                    group.userBranchId = (short)token.GetBranchId;
                    //group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //group.applicationUrl = Request.Path.Value;
                    group.companyId = token.GetCompanyId;

                    var data = repo.AddGroup(group);
                    if (data.IsCompleted)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, result = group, message = "Group has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "An unknown error has occured" }));

                }
                catch (Exception ex)
                {
                    //  this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" }));
                }

                return response;
            });
        }


        [HttpPut][Route("group/{id}")]
        public HttpResponseMessage UpdateGroup(HttpRequestMessage request, [FromBody] AppGroupViewModel group, short id)
        { 
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                   var   data =   repo.UpdateGroup(id, group);
                    if (data.IsCompleted)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, result = group, message = "Group has been updated successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "An unknown error has occured" }));

                }
                catch (Exception ex)
                {
                 //   this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" }));
                }

                return response;
            });
        }



        [HttpGet][Route("groups")]
        public HttpResponseMessage GetAllGroups(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                TokenDecryptionHelper token = null;
                try
                {
                    // token = new TokenDecryptionHelper(this.HttpContext);
                    var groups = repo.GetAllGroups().ToList();
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = groups }));
                }
                catch (Exception ex)
                {
                    // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" }));
                }
              return   response;
            });
        }

        [HttpGet][Route("group/{id}")]
        public HttpResponseMessage GetGroupById(HttpRequestMessage request,int id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                TokenDecryptionHelper token = null;
                try
                {
                    // token = new TokenDecryptionHelper(this.HttpContext);
                    var group = repo.GetSingleGroup(id);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = group }));
                }
                catch (Exception ex)
                {
                    // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" }));
                }
                return response;
            });
        }

        #endregion

        #region Activities
        [HttpGet] [Route("activities/parents")]
        public HttpResponseMessage GetAllActivities(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                TokenDecryptionHelper token = null;
            try
            {
                // token = new TokenDecryptionHelper(this.HttpContext);
                var groups = repo.GetActivities().ToList();
                response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = true, result = groups }));
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" }));
                }

                return response;
            });

        }


        [HttpGet][Route("group/activities/mapped")]
        public HttpResponseMessage GetGroupActivities(HttpRequestMessage request)
        { HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {

                TokenDecryptionHelper token = null;
                try
                {
                   // token = new TokenDecryptionHelper(this.HttpContext);
                    var groups = repo.GetGroupActivities().ToList();
                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = true, result = groups }));
                }
                catch (Exception ex)
                {
                  //  this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" }));
                }

                return response;
            });
        }



        [HttpPut] [Route("group/activity/access/{id}")]
        public HttpResponseMessage AddAccessToActivity(HttpRequestMessage request, int id, [FromBody] ActivitiesUpdateVm model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                //[FromBody]
                var req = this.Request;
                TokenDecryptionHelper token = null;
                try
                {
                    //   token = new TokenDecryptionHelper(this.HttpContext);
                    var data = repo.AddAccessToActivity(id, model);
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                     Created("", new { success = true, message = "Access right has been updated successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = "An unknown error has occured" }));

                }
                catch (Exception ex)
                {
                    //   this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" }));
                }
                return response;
            });
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

          
        //    return Ok(new { result = result, itemsPerPage = itemsPerPage, totalItems = totalItems });
        //}
    }
}