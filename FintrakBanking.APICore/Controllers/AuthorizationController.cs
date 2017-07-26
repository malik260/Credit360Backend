using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1")]
    public class AuthorizationController : ApiControllerBase
    {
        private IAuthorizationRepository repo;
        IErrorLogRepository errorLogger;
        public AuthorizationController(IAuthorizationRepository _repo,
                                        IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
        }

        [HttpGet] [Route("setup/groups")]
        public HttpResponseMessage GetGroups(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                string returnMessage = string.Empty;
                try
                {
                    var groups = repo.GetGroups().ToList();
                    if (groups.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = true, result = groups, count = groups.Count }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = "No group found" }));
                }
                catch (Exception e)
                {
                    returnMessage = e.Message;
                }
                response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"There was error from the endpoint {returnMessage}" }));
                return response;
            });

        }


        [HttpPost] [Route("setup/group/add")]
        public HttpResponseMessage AddGroup(HttpRequestMessage request, [FromBody] GroupModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                TokenDecryptionHelper token = null;
                try
                {
                    // token = new TokenDecryptionHelper(this.HttpContext);
                    model.createdBy = token.GetStaffId;

                    var data = repo.AddGroup(model);
                    if (data != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Created("", new { success = true, result = model, message = "Group has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = "There was an error creating this group" }));
                }
                catch (Exception e)
                {

                    //  this.errorLogger.LogError(e, Request.Path.Value, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"There was an error creating this group {e.Message}" }));
                }
                return response;
            });
        }

        //[HttpPost("setup/group")]
        //public async Task<IActionResult> AddGroup([FromBody]GroupModel grpModel)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);
        //        var response = await repo.AddGroup(grpModel);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = grpModel, message = "Group has been created successfully" });
        //        }

        //        return Ok(new { success = false, message = "There was an error creating this group" });
        //    }
        //    catch (Exception e)
        //    {

        //        this.errorLogger.LogError(e, Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = $"There was an error creating this group {e.Message}" });
        //    }
        //}

        [HttpPut] [Route("setup/group/{groupId}")]
        public HttpResponseMessage AddGroup(HttpRequestMessage request, short groupId, [FromBody] GroupViewModel grpModel)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data =   repo.UpdateGroup(groupId, grpModel);
                    if (data.IsCompleted)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Created("", new { success = true, result = grpModel }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"There was an error updating this group {grpModel}" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"There was an error updating this group {e.Message}" }));
                }
                return response;
            });
            }

        [HttpGet] [Route("setup/activities")]
        public HttpResponseMessage GetActivities(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var activity = repo.GetActivities().ToList();
                    if (!activity.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = "No activity found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = true, result = activity, count = activity.Count }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
            }
        [HttpGet][Route("setup/activities/group/{grpId}")]
        public HttpResponseMessage GetActivitiesByGroupId(HttpRequestMessage request,int grpId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var activities = repo.GetActivitiesByGroupId(grpId);
                    if (!activities.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = "No activity found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = true, result = activities }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpPost][Route("setup/group/activities")]
        public HttpResponseMessage AddActivitiesGroup(HttpRequestMessage request, [FromBody]GroupViewModel grpModel)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.AddActivitiesToGroup(grpModel);
                    if (data.IsCompleted)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Created("", new { success = true, result = grpModel, message = "Group has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = "There was an error creating this group" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"There was an error creating this group {e.Message}" }));
                }
                return response;
            });
        }
    }
}