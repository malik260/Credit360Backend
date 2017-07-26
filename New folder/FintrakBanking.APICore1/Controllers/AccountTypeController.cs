using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels.Setups.Finance;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class AccountTypeController : BaseController
    {
        private IAccountTypeRepository repo;

        public AccountTypeController(IAccountTypeRepository _repo)
        {
            this.repo = _repo;
        }

        #region Account Type Actions

        [HttpGet("account-type", Name = "GetAccountType")]
        public IActionResult GetAllAccountType()
        {
            try
            {
                var data = repo.GetAllAccountType();
                return Ok(new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("account-type/{accountTypId}", Name = "GetAccountTypeById")]
        public IActionResult GetAllAccountTypeById(int accountTypId)
        {
            try
            {
                var data = repo.GetAllAccountTypeById(accountTypId);
                return Ok(new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("account-type")]
        public IActionResult AddAccountType([FromBody] AddAccountTypeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var isCreated = repo.AddAccountType(model);
                if (isCreated == true)
                {
                    return Ok(new { success = true, result = isCreated, message = "account type has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "account type not created" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("account-type/{accountTypeId}")]
        public IActionResult UpdateAccountType(int accountTypeId, [FromBody]AccountTypeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                if (repo.UpdateAccountType(accountTypeId, model))
                {
                    return Ok(model);
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
            return BadRequest();
        }

        #endregion Account Type Actions
    }
}