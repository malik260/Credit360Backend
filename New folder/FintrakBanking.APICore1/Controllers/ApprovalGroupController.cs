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
    public class ApprovalGroupController : Controller
    {
        private IApprovalGroupMappingRepository repoMapping;

        private IApprovalGroupRepository repoGroup;

        public ApprovalGroupController(IApprovalGroupMappingRepository _repoMapping, IApprovalGroupRepository _repoGroup)
        {
            this.repoMapping = _repoMapping;
            this.repoGroup = _repoGroup;
        }

        #region Approval Group Mapping
        [HttpPost("approval-group-mapping")]
        public IActionResult AddApprovalGroupMapping([FromBody] ApprovalGroupMappingViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repoMapping.AddApprovalGroupMapping(model);
                if (response != -1)
                {
                    return Ok(new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet("approval-group-mapping/{operationMappingId}")]
        public IActionResult GetApprovalGroupMappingById(int operationMappingId)
        {
            try
            {
                //var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repoMapping.GetApprovalGroupMapping(operationMappingId);
                if (response == null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("approval-group-mapping/operation/{operationId}/product/{productClassId}")]
        public IActionResult GetApprovalGroupMapping(int operationId, short productClassId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                short? productClass;

                if (productClassId != -1)
                    productClass = productClassId;
                else
                    productClass = null;

                var response = repoMapping.GetApprovalGroupMapping(operationId, productClass);
                // if (!response.Any())
                // {
                //     return Ok(new { success = false, message = "No record found" });
                // }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPut("approval-group-mapping/{operationMappingId}")]
        public IActionResult UpdateApprovalGroupMapping(int operationMappingId, [FromBody] ApprovalGroupMappingViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repoMapping.UpdateApprovalGroupMapping(operationMappingId, model);

                if (response)
                {

                    return Ok(new { success = true, result = response, message = "The record has been updated successfully" });

                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete("approval-group-mapping/{operationMappingId}")]
        public IActionResult DeleteApprovalGroupMapping(int operationMappingId)
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

                repoMapping.DeleteApprovalGroupMapping(operationMappingId, user);

                return Ok(new { success = true, result = operationMappingId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region Approval Group
        [HttpPost("approval-group")]
        public IActionResult AddApprovalGroup([FromBody] ApprovalGroupViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repoGroup.AddApprovalGroup(model);
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

        [HttpGet("approval-group")]
        public IActionResult GetAllApprovalGroup()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);


                var response = repoGroup.GetAllApprovalGroup(token.GetCompanyId);
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

        [HttpGet("approval-group/{GroupId}")]
        public IActionResult GetApprovalGroup(int GroupId)
        {
            try
            {

                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repoGroup.GetApprovalGroupById(GroupId,token.GetCompanyId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("approval-group/{GroupId}")]
        public IActionResult UpdateApprovalGroup(int GroupId, [FromBody] ApprovalGroupViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repoGroup.UpdateApprovalGroup(GroupId, model);

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

        [HttpDelete("approval-group/{GroupId}")]
        public IActionResult DeleteApprovalGroup(int GroupId)
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

                repoGroup.DeleteApprovalGroup(GroupId, user);

                return Ok(new { success = true, result = GroupId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}