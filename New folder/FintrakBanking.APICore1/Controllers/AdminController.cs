using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using Microsoft.AspNetCore.Cors;
using System.Net.Http;
using FintrakBanking.ViewModels.Setups.General;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/admin")]
    public class AdminController : BaseController
    {
        private IAdminRepository repo;
        IErrorLogRepository errorLogger;
        ICanAuthorizationRepository I;
        IAuditTrailRepository audit;
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

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = repo.GetAllUsers().ToList();
            return Ok(new { result = users });
        }

        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody]AppUserViewModel user)
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);


                if (I.CanPerformActionOnResource(token.GetUserId, 2, UserActions.Add))
                {
                    if (repo.iSUserExit(user.username))
                    {
                        return Ok(new { suucess = false, message = "A user with this username already exit" });
                    }

                    user.createdBy = token.GetStaffId;
                    user.userBranchId = (short)token.GetBranchId;
                    user.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    user.applicationUrl = Request.Path.Value;
                    user.companyId = token.GetCompanyId;
                    var response = await repo.CreateUser(user);
                    if (response)
                    {
                        return Created("", new { success = true, result = user, message = "User has been created successfully" });
                    }
                }
                else
                {
                    return Ok(new { success = false, message = "You do not have enough right to add user" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });


            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }



        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody]AppUserViewModel user)
        {
            TokenDecryptionHelper token = null;
            try
            {
                
                token = new TokenDecryptionHelper(this.HttpContext);
                user.createdBy = token.GetStaffId;
                //user.userBranchId = (short)token.GetBranchId;
                //user.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                //user.applicationUrl = Request.Path.Value;
                //user.companyId = token.GetCompanyId;
                var response = await repo.UpdateUser(id, user);
                if (response)
                {
                    return Created("", new { success = true, result = user, message = "User has been created successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });


            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }

        #endregion


        #region Group

        [HttpPost("group/add")]
        public async Task<IActionResult> AddGroup([FromBody] AppGroupViewModel group)
        {


            TokenDecryptionHelper token = null;
            try
            {
                if (repo.iSGroupExist(group.groupName))
                {
                    return Ok(new { suucess = false, message = $"{group.groupName} already exit" });
                }

                token = new TokenDecryptionHelper(this.HttpContext);
                group.createdBy = token.GetStaffId;
                group.userBranchId = (short)token.GetBranchId;
                group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                group.applicationUrl = Request.Path.Value;
                group.companyId = token.GetCompanyId;

                var response = await repo.AddGroup(group);
                if (response)
                {
                    return Created("", new { success = true, result = group, message = "Group has been created successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });

            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }


        [HttpPut("group/{id}")]
        public async Task<IActionResult> UpdateGroup([FromBody] AppGroupViewModel group, short id)
        {
            //[FromBody]
            var req = this.Request;
            TokenDecryptionHelper token = null;
            try
            {

                token = new TokenDecryptionHelper(this.HttpContext);
                group.createdBy = token.GetStaffId;
                group.userBranchId = (short)token.GetBranchId;
                group.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                group.applicationUrl = Request.Path.Value;
                group.companyId = token.GetCompanyId;

                var response = await repo.UpdateGroup(id,group);
                if (response)
                {
                    return Created("", new { success = true, result = group, message = "Group has been updated successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });

            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }



        [HttpGet("groups")]
        public IActionResult GetAllGroups()
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var groups = repo.GetAllGroups().ToList();
                return Ok(new { success = true, result = groups });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }
        }

        [HttpGet("group/{id}")]
        public IActionResult GetGroupById(int id)
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var group = repo.GetSingleGroup(id);
                return Ok(new { success = true, result = group });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }
        }

        #endregion

        #region Activities
        [HttpGet("activities/parents")]
        public IActionResult GetAllActivities()
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var groups = repo.GetActivities().ToList();
                return Ok(new { success = true, result = groups });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }
        }


        [HttpGet("group/activities/mapped")]
        public IActionResult GetGroupActivities()
        {
            TokenDecryptionHelper token = null;
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var groups = repo.GetGroupActivities().ToList();
                return Ok(new { success = true, result = groups });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured while fetching groups - {ex.Message}" });
            }
        }



        [HttpPut("group/activity/access/{id}")]
        public IActionResult AddAccessToActivity(int id,[FromBody] ActivitiesUpdateVm model)
        {
            //[FromBody]
            var req = this.Request;
            TokenDecryptionHelper token = null;
            try
            {

                token = new TokenDecryptionHelper(this.HttpContext);


                var response = repo.AddAccessToActivity(id, model);
                if (response)
                {
                    return Created("", new { success = true,message = "Access right has been updated successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });

            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"An unhandled error occured {ex.Message}" });
            }
        }

        #endregion

        [HttpGet("audit/log")]
        public IActionResult GetAuditLog([FromQuery] int page,[FromQuery] int itemsPerPage)
        {
            TokenDecryptionHelper token = null;
                        
            token = new TokenDecryptionHelper(this.HttpContext);
            var allAuditLog = audit.GetAuditTrail((short)token.GetBranchId);
            int totalItems = allAuditLog.Count();

            //if (search != null || !string.IsNullOrWhiteSpace(search))
            //{
            //    allAuditLog.Where(x => x.firstName.ToLower().Contains(search.ToLower())
            //    || x.auditType.ToLower().Contains(search.ToLower())
            //    || x.details.ToLower().Contains(search.ToLower()));
            //}
            //allAuditLog = allAuditLog.OrderBy(x => x.systemDate).Skip((page-1) * itemsPerPage).Take(itemsPerPage);

            allAuditLog = allAuditLog.OrderBy(x => x.systemDate).Skip(page).Take(itemsPerPage);

            var result = allAuditLog.ToList();

          
            return Ok(new { result = result, itemsPerPage = itemsPerPage, totalItems = totalItems });
        }
    }
}