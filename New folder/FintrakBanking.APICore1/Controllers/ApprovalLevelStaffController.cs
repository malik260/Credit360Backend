using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class ApprovalLevelStaffController : Controller
    {
        private IApprovalLevelStaffRepository repo;

        public ApprovalLevelStaffController(IApprovalLevelStaffRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level Staff
        [HttpPost("approval-level-staff")]
        public IActionResult AddApprovalLevelStaff([FromBody] ApprovalLevelStaffViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddApprovalLevelStaff(model);
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

        [HttpGet("approval-level-staff/operations/{operationMappingId}")]
        public IActionResult GetAllApprovalLevelStaff(int operationMappingId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllApprovalLevelStaffByOperationId(operationMappingId, token.GetCompanyId);
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

        [HttpGet("approval-level-staff/staff-level/{id}")]
        public IActionResult GetApprovalLevelStaffById(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
 
                var response = repo.GetApprovalLevelStaffById( id, token.GetCompanyId); 
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("approval-level-staff/{id}")]
        public IActionResult UpdateApprovalLevelStaff([FromBody] ApprovalLevelStaffViewModel model, int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateApprovalLevelStaff(id, model);

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

        [HttpDelete("approval-level-staff/{StaffLevelId}")]
        public IActionResult DeleteApprovalLevelStaff(int StaffLevelId)
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

                repo.DeleteApprovalLevelStaff(StaffLevelId, user);

                return Ok(new { success = true, result = StaffLevelId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}