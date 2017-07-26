using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1")]
    public class AuthorizationController : BaseController
    {
        private IAuthorizationRepository repo;
        IErrorLogRepository errorLogger;
        public AuthorizationController(IAuthorizationRepository _repo,
                                        IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
        }

        [HttpGet("setup/groups")]
        public IActionResult GetGroups()
        {
            string returnMessage = string.Empty;
            try
            {
                var groups = repo.GetGroups().ToList();
                if (groups.Any())
                {
                    return Ok(new { success = true, result = groups, count = groups.Count });
                }

                return Ok(new { success = false, message = "No group found" });
            }
            catch (Exception e)
            {
                returnMessage = e.Message;
            }
            return Ok(new { success = false, message = $"There was error from the endpoint {returnMessage}" });
        }


        [HttpPost("setup/group/add")]
        public async Task<IActionResult> AddGroup([FromBody] GroupModel model)
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                model.createdBy = token.GetStaffId;
                
                var response = await repo.AddGroup(model);
                if (response)
                {
                    return Created("", new { success = true, result = model, message = "Group has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this group" });
            }
            catch (Exception e)
            {

                this.errorLogger.LogError(e, Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this group {e.Message}" });
            }
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

        [HttpPut("setup/group/{groupId}")]
        public async Task<IActionResult> AddGroup(short groupId, [FromBody] GroupViewModel grpModel)
        {
            try
            {
                var response = await repo.UpdateGroup(groupId, grpModel);
                if (response)
                {
                    return Created("", new { success = true, result = grpModel });
                }

                return Ok(new { success = false, message = $"There was an error updating this group {grpModel}" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }

        [HttpGet("setup/activities")]
        public IActionResult GetActivities()
        {
            try
            {
                var activity = repo.GetActivities().ToList();
                if (!activity.Any())
                {
                    return Ok(new { success = false, message = "No activity found" });
                }

                return Ok(new { success = true, result = activity, count = activity.Count });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("setup/activities/group/{grpId}")]
        public IActionResult GetActivitiesByGroupId(int grpId)
        {
            try
            {
                var activities = repo.GetActivitiesByGroupId(grpId);
                if (!activities.Any())
                {
                    return Ok(new { success = false, message = "No activity found" });
                }

                return Ok(new { success = true, result = activities });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost("setup/group/activities")]
        public async Task<IActionResult> AddActivitiesGroup([FromBody]GroupViewModel grpModel)
        {
            try
            {
                var response = await repo.AddActivitiesToGroup(grpModel);
                if (response)
                {
                    return Created("", new { success = true, result = grpModel, message = "Group has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this group" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this group {e.Message}" });
            }
        }
    }
}