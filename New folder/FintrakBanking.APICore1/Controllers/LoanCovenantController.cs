using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/credit")]
    public class LoanCovenantController : BaseController
    {
        private ILoanCovenantRepository repo;
        TokenDecryptionHelper token = null;
        IErrorLogRepository errorLogger;
        public LoanCovenantController(ILoanCovenantRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        [HttpPost("covenant-detail")]
        public async Task<IActionResult> AddLoanCovenantDetail([FromBody] LoanCovenantDetailViewModel entity)
        {

            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.AddLoanCovenantDetail(entity);
                if (response)
                {
                    return Created("", new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete("covenant-detail")]
        public async Task<IActionResult> DeleteLoanCovenantDetail(int loanCovenantDetailId)
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var response = await repo.DeleteLoanCovenantDetail(loanCovenantDetailId, user);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "Deleted successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("covenant-detail/{id}")]
        public async Task<IActionResult> UpdateLoanCovenantDetail([FromBody] LoanCovenantDetailViewModel entity, int id)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.UpdateLoanCovenantDetail(id, entity);
                if (response)
                {
                    return Created("", new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Ok(new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

        [HttpGet("covenant-detail/covenant-type/{id}")]
        public IActionResult GetLoanCovenantDetailByCovenantType(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetLoanCovenantDetailByCovenantType(id, token.GetCompanyId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("covenant-detail/{id}")]
        public IActionResult GetLoanCovenantDetailById(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetLoanCovenantDetailById(id, token.GetCompanyId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("covenant-detail/loan/{id}")]
        public IActionResult GetLoanCovenantDetailByloanId(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetLoanCovenantDetailByloanId(id, token.GetCompanyId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }



        [HttpPost("covenant-type")]
        public async Task<IActionResult> AddLoanCovenantType([FromBody] LoanCovenantTypeViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.AddLoanCovenantType(entity);
                if (response)
                {
                    return Created("", new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpGet("covenant-type")]
        public IActionResult GetLoanCovenantType()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetLoanCovenantType(token.GetCompanyId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("covenant-type/{id}")]
        public async Task<IActionResult> UpdateLoanCovenantType([FromBody] LoanCovenantTypeViewModel entity, short id)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.UpdateLoanCovenantType(id, entity);
                if (response)
                {
                    return Created("", new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
    }
}