using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/credit")]
    public class CollateralController : BaseController
    {
        TokenDecryptionHelper token = null;
        private ICollateralCustomerRepository repo;
        IErrorLogRepository errorLogger;
        public CollateralController(ICollateralCustomerRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region Collateral 
        [HttpPost("collateral")]
        public async Task<IActionResult> AddCollateral([FromBody] CollateralViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = Request.Path.Value;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.companyId = token.GetCompanyId;

                var response = await repo.AddCollateralCustomer(entity);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "Created successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("collateral/{collateralCustomerId}")]
        public async Task<IActionResult> UpdateCustomCollateral(int collateralCustomerId, [FromBody] CollateralViewModel entity)
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = Request.Path.Value;
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                var response = await repo.UpdateCollateralCustomer(collateralCustomerId, entity);
                if (!response)
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

        [HttpDelete("collateral/{collateralCustomerId}")]
        public async Task<IActionResult> DeleteCollateralDetails(int collateralCustomerId)
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

                var response = await repo.DeleteCollateralDetails(collateralCustomerId, user);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "Created successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("collateral")]
        public IActionResult GetCollateralDetails()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralDetails(token.GetCompanyId);
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
        #endregion Collateral 

        #region Seniority Of Claims
        [HttpGet("collateral-seniority-of-claims")]
        public IActionResult GetCollateralSeciorityOfClaims()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralSeniorityOfClaims();
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
        #endregion Seniority Of Claims


        #region Listing Functions
        [HttpGet("collateral-value-base-type")]
        public IActionResult GetCollateralValueBaseType()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralValueBaseType();
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

        [HttpGet("collateral-valuers")]
        public IActionResult GetCollateralValuers()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralValuers(token.GetCompanyId);
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

        [HttpGet("collateral-valuer-type")]
        public IActionResult GetCollateralValuerType()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralValuerType();
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

        //[HttpGet("collateral-policy/customer/{collateralCustomerId}")]
        //public IActionResult GetCollateralCustomerPolicyByCollateralCustomerId(short collateralCustomerId)
        //{
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralCustomerPolicyByCollateralCustomerId(collateralCustomerId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet("collateral-sub-type/{collateralSubTypeId}")]
        public IActionResult GetCollateralSubTypeById(short collateralSubTypeId)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralSubTypeById(collateralSubTypeId);
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


        [HttpGet("collateral-sub-type/collateral-type/{collateralTypeId}")]
        public IActionResult GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
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
        #endregion End of Listing Functions
    }
}