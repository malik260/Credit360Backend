using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups/chart-of-account")]
    public class ChartOfAccountController : BaseController
    {
        private IChartOfAccountRepository repo;

        public ChartOfAccountController(IChartOfAccountRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("")]
        public IActionResult GetAllAccounts()
        {
            try
            {
                var accounts = repo.GetAllAccounts();
                if (accounts == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = accounts.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("category/{accountCategoryId}")]
        public IActionResult GetAccountsByCategory(short accountCategoryId)
        {
            try
            {
                var accounts = repo.GetAccountsByCategory(accountCategoryId);
                if (accounts == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = accounts.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("fs-captions")]
        public IActionResult GetFinancialSatementCaptionLookup()
        {
            try
            {
                var accounts = repo.GetFinancialSatementCaptionLookup();
                if (accounts == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = accounts.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{accountId}")]
        public IActionResult Get(short accountId)
        {
            try
            {
                //var accounts = repo.GetAccountViewModel(accountId);
                //return Ok(accounts);

                var account = repo.GetAccountViewModel(accountId);
                if (account == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = account });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // POST api/values
        [HttpPost]
        public IActionResult AddChartOfAccount([FromBody]ChartOfAccountViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;
                model.applicationUrl = Request.Path.Value;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                var accountId = repo.AddAccount(model);

                if (accountId >= 1)
                {
                    return Ok(new { success = true, result = model, message = "account has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "account not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{accountId}")]
        public IActionResult UpdateChartOfAccount(int accountId, [FromBody] ChartOfAccountViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            //var account = repo.UpdateAccount(accountId, model);
            //if (account == null)
            //{
            //    return NotFound(new { success = false, message = "No record found" });
            //}

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;
                model.applicationUrl = Request.Path.Value;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                repo.UpdateAccount(accountId, model);

                return Ok(new { success = true, result = model.accountId, message = "account has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // DELETE api/values/5
        [HttpDelete("{accountId}")]
        public IActionResult DeleteAccount(int accountId)
        {
            //var account = repo.GetAccountViewModel(accountId);
            //if (account == null)
            //{
            //    return NotFound(new { success = false, message = "No record found" });
            //}

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
                repo.DeleteAccount(accountId,user);

                return Ok(new { success = true, result = accountId, message = "account has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}