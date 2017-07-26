using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/credit")]
    public class LimitController : Controller
    {
        private ILimitRepository repo;

        public LimitController(ILimitRepository _repo)
        {
            this.repo = _repo;
        }

        #region Limits
        [HttpGet("limit")]
        public IActionResult GetAllLimit()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetAllLimit(token.GetCompanyId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch(Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("limit/{limitId}")]
        public IActionResult GetLimitById(int limitId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetLimitById(limitId);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost("limit")]
        public IActionResult AddLimit([FromBody] LimitViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddLimit(model);
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

        [HttpPut("limit/{limitId}")]
        public IActionResult UpdateLimit(int LimitId, [FromBody] LimitViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateLimit(LimitId, model);
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

        [HttpDelete("limit/{limitId}")]
        public IActionResult DeleteLimit(int LimitId)
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

                repo.DeleteLimit(LimitId, user);

                return Ok(new { success = true, result = LimitId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Limits Details
        [HttpGet("limit-detail")]
        public IActionResult GetAllLimitDetail()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetAllLimitDetail();

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("limit-detail/{limitDetailId}")]
        public IActionResult GetLimitDetailById(int limitDetailId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetLimitDetailById(limitDetailId);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost("limit-detail")]
        public IActionResult AddLimitDetail([FromBody] LimitDetailViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddLimitDetail(model);
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

        [HttpPut("limit-detail/{limitDetailId}")]
        public IActionResult UpdateLimitDetail(int limitDetailId, [FromBody] LimitDetailViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateLimitDetail(limitDetailId, model);
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

        [HttpDelete("limit-detail/{limitDetailId}")]
        public IActionResult DeleteLimitDetail(int limitDetailId)
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

                repo.DeleteLimitDetail(limitDetailId, user);

                return Ok(new { success = true, result = limitDetailId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Limits Metric
        [HttpGet("limit-metric")]
        public IActionResult GetAllLimitMetric()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetAllLimitMetric();

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Limits Type
        [HttpGet("limit-type")]
        public IActionResult GetAllLimitType()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetAllLimitType();

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Limits Value Type
        [HttpGet("limit-value-type")]
        public IActionResult GetAllLimitValueType()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetAllLimitValueType();

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Frequency Type
        [HttpGet("limit-frequency-type")]
        public IActionResult GetAllFrequencyType()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetAllFrequencyType();

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion
    }
}