using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups/risk")]
    public class RiskController : BaseController
    {
        IErrorLogRepository errorLogger;
        private IRiskSetupRepository repo;
        TokenDecryptionHelper token = null;
        public RiskController(IRiskSetupRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region Risk Assessment Index

        [HttpPost("risk-assessment-index")]
        public async Task<IActionResult> AddRiskAssessmentIndex([FromBody]  RiskAssessmentIndexViewModels entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.AddRiskAssessmentIndexs(entity);
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

        [HttpDelete("risk-assessment-index/{id}")]
        public async Task<IActionResult> DeleteForeHeader(int id)
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

                var response = await repo.DeleteRiskAssessmentIndex(id, user);
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

        [HttpPut("risk-assessment-index/{id}")]
        public async Task<IActionResult> UpdateRiskAssessmentIndex(int id, [FromBody]  RiskAssessmentIndexViewModels entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.UpdateRiskAssessmentIndex(id, entity);
                if (response)
                {
                    return Created("", new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpGet("risk-assessment-index/{id}")]
        public IActionResult GetRiskAssessmentIndexById(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentIndexById(id, token.GetCompanyId);
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

        [HttpGet("risk-assessment-index/risktitle/{id}")]
        public IActionResult GetRiskAssessmentIndexByRiskTitle(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentIndexByRiskTitle(id, token.GetCompanyId);
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

        [HttpGet("risk-assessment-index/parent/{id}")]
        public IActionResult GetRiskAssessmentIndexByParent(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentIndexByParent(id, token.GetCompanyId);
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



        [HttpGet("risk-assessment-index/itemlevel/{id}")]
        public IActionResult GetRiskAssessmentIndexByItemLevel(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentIndexByItemLevel(id, token.GetCompanyId);
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

        #endregion Risk Assessment Index

        #region RiskRating

        [HttpPost("risk-rating")]
        public async Task<IActionResult> AddRiskRating([FromBody] RiskRatingViewModel entity)
        {
            try
            {
                var response = await repo.AddRiskRating(entity);
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

        [HttpDelete("risk-rating/{ratingId}")]
        public async Task<IActionResult> DeleteRiskRating(int ratingId, [FromBody] RiskRatingViewModel entity)
        {
            try
            {
                var response = await repo.DeleteRiskRating(ratingId, entity);
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

        [HttpGet("risk-rating")]
        public IActionResult GetRiskRating()
        {
            try
            {
                var response = repo.GetRiskRating();
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

        [HttpGet("risk-rating-by-product/{productId}")]
        public IActionResult GetRiskRatingByProductId(int productId)
        {
            try
            {
                var response = repo.GetRiskRatingByProductId(productId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No activity found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut("risk-rating/{ratingId}")]
        public async Task<IActionResult> UpdateRiskRating(int ratingId, [FromBody]  RiskRatingViewModel entity)
        {
            try
            {
                var response = await repo.UpdateRiskRating(ratingId, entity);
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

        #endregion RiskRating


        #region Risk Assessment title
        [HttpPost("risk-assessment-title")]
        public async Task<IActionResult> AddRiskAssessmentTitle([FromBody]RiskAssessmentTitleViewModels entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.AddRiskAssessmentTitle(entity);
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

        [HttpPut("risk-assessment-title/{id}")]
        public async Task<IActionResult> UpdateRiskAssessmentTitle(int id, [FromBody] RiskAssessmentTitleViewModels entity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;

                var response = await repo.UpdateRiskAssessmentTitle(id, entity);
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

        [HttpDelete("risk-assessment-title/{id}")]
        public async Task<IActionResult> DeleteRiskAssessmentTitle(int id)
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

                var response = await repo.DeleteRiskAssessmentTitle(id, user);
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
        [HttpGet("risk-assessment-title/{id}")]
        public IActionResult GetRiskAssessmentTitleById(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentTitleById(id, token.GetCompanyId);
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


        [HttpGet("risk-assessment-title/product/{id}")]
        public IActionResult GetRiskAssessmentTitleByProductId(int id)
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentTitleByProductId(id, token.GetCompanyId);
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

        [HttpGet("risk-assessment-title/risktype/{id}")]
        public IActionResult GetRiskAssessmentTitleByRiskType(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentTitleByRiskType(id, token.GetCompanyId);
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


        [HttpGet("risk-assessment-title")]
        public IActionResult GetRiskAssessmentTitle()
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskAssessmentTitle(token.GetCompanyId);
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

        #endregion Risk Assessment title
    }
}