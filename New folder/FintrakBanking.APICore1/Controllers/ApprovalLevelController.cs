using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.Interfaces.Setups.Approval;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class ApprovalLevelController : Controller
    {
        private IApprovalLevelRepository repo;

        public ApprovalLevelController(IApprovalLevelRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level
        [HttpPost("approval-level")]
        public IActionResult AddApprovalLevel([FromBody] ApprovalLevelViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddApprovalLevel(model);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost("approval-level-multiple")]
        public IActionResult AddMultipleApprovalLevel([FromBody] List<ApprovalLevelViewModel> model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var recordId = repo.AddMultipleApprovalLevel(model);
                if (recordId)
                {
                    return Ok(new { success = true, result = recordId, message = "Approval Levels has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "Approval Levels not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("approval-level")]
        public IActionResult GetAllApprovalLevel()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllApprovalLevel(token.GetCompanyId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("approval-level/approval-level/{id}")]
        public IActionResult GetApprovalLevelById(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetApprovalLevelById(id,token.GetCompanyId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet("approval-level/operation-mapping/{id}")]
        public IActionResult GetApprovalLevelByOperationId(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetApprovalLevelByOperationId(id, token.GetCompanyId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("approval-level/approval-level/{id}")]
        public IActionResult UpdateApprovalLevel(int id, [FromBody] ApprovalLevelViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateApprovalLevel(id, model);

                if (response)
                {

                    return Created("", new { success = true, result = response, message = "The record has been updated successfully" });

                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete("approval-level/approval-level/{id}")]
        public IActionResult DeleteApprovalLevel(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                repo.DeleteApprovalLevel(id, user);

                return Ok(new { success = true, result = id, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion
       
        #region
        [HttpGet("workflowtracker/operation/{oId}/targetId/{tId}")]
        public IActionResult GetApprovalTrailByOperationIdAndTargetId(int oId, int tId) {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetApprovalTrailByOperationIdAndTargetId(oId, tId, token.GetCompanyId );
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpGet("workflowtracker/operation/{id}")]
        public IActionResult GetApprovalTrail(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetApprovalTrail(id,   token.GetCompanyId);
                if(response .Any ())
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

            return null;
        }
        #endregion
    }
}