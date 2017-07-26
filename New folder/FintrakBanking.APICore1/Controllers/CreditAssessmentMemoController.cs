using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Business;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/cam")]
    public class CreditAssessmentMemoController : BaseController
    {
        private ICreditAssessmentMemoRepository repo;
        TokenDecryptionHelper token = null;
        IErrorLogRepository errorLogger;

        public CreditAssessmentMemoController(ICreditAssessmentMemoRepository _repo,

        IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;

            errorLogger = _errorLogger;
        }


        [HttpPost("loan-application/operation/{id}")]
        public async Task<IActionResult> AddCreditAssessmentMemo(int id, [FromBody] CreditAssessmentMemoViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                var response = await repo.AddCreditAssessmentMemo(id, entity);
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

        [HttpPost("loan-application/operation/{id}")]
        public IActionResult GetRequestOnCreditAssessmentMemo(int id, [FromQuery] int page, [FromQuery] int itemsPerPage)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRequestOnCreditAssessmentMemo(token.GetCountryId, token.GetBranchId, token.GetStaffId, id)
                    .Skip(page).Take(itemsPerPage);

                if (response.Any())
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

        [HttpGet("loan-application/operation/{id}")]
        public IActionResult GetRequestForCreditAssessmentMemo(int id)
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRequestOnCreditAssessmentMemo(token.GetCompanyId, token.GetBranchId, token.GetStaffId, id);

                if (response.Any())
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

        //[HttpPost("assessment-template/template")]
        //public IActionResult SubmitRequestForProcessing([FromBody]ApprovalViewModel entity)
        //{
        //  try
        //    {
        //        token = new TokenDecryptionHelper(HttpContext);
        //        entity.BranchId = (short)token.GetBranchId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.createdBy = token.GetStaffId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //        entity.applicationUrl = Request.Path.Value;

        //        var response = repo.SubmitRequestForProcessing(entity);                 
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = entity, message = "The record has been created successfully" });
        //        }

        //        return Ok(new { success = false, message = "There was an error creating this record" });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
        //    }
        //}
        [HttpPost("assessment-template/template")]
        public IActionResult GetAssessmentTempates([FromBody]CreditTemplateViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAssessmentTempatesByApprovalLevel(entity.approvalLevelId, entity.productClassId, token.GetCompanyId);// repo.GetRequestForCreditAssessmentMemo(token.GetCountryId, token.GetBranchId);

                if (!response.Any())
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

        [HttpPost("assessment-template/template/customer")]
        public IActionResult GetAssessmentTempates([FromBody] AssessmentTemplatesViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                entity.companyId = token.GetCompanyId;
                var response = repo.GetAssessmentTempates(token.GetStaffId, entity , token.GetCompanyId);// repo.GetRequestForCreditAssessmentMemo(token.GetCountryId, token.GetBranchId);

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

        [HttpPut("assessment-template")]
        public async Task<IActionResult> UpdateAssessmentTempates([FromBody]AssessmentTemplatesViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var response = await repo.UpdateAssessmentTempates(entity);

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

        [HttpPost("assessment-template")]
        public async Task<IActionResult> AddAssessmentTempates([FromBody] AssessmentTemplatesViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                var response = await repo.AddAssessmentTempates(entity); ;
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
    }
}