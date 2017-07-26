using FintrakBanking.Interfaces.Setups.Finance;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups/account-category")]
    public class AccountCategoryController : BaseController
    {
        private IAccountCategoryRepository repo;

        public AccountCategoryController(IAccountCategoryRepository _repo)
        {
            this.repo = _repo;
        }

        #region Account Category Actions

        [HttpGet("", Name = "Category")]
        public IActionResult GetAllAccountType()
        {
            try
            {
                var data = repo.GetAllAccountCategory();
                return Ok(new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{categoryId}", Name = "categoryById")]
        public IActionResult GetAccountTypeById(int categoryId)
        {
            try
            {
                var data = repo.GetAccountCategoryById(categoryId);
                return Ok(new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        //[HttpPost("accountCategory/addproductgroup")]
        //public IActionResult AddProductGroup([FromBody]AccountCategoryViewModel model)
        //{
        //    try
        //    {
        //var token = new TokenDecryptionHelper(this.HttpContext);
        //model.userBranchId = (short) token.GetBranchId;
        //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //model.applicationUrl = Request.Path.Value;
        //model.createdBy = token.GetStaffId;
        //model.companyId = token.GetCompanyId;

        //        if (repo.AddFinanceAccountCategorySetup(model))
        //        {
        //            return Created("", model);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return BadRequest();
        //    }

        //    return BadRequest();
        //}

        #endregion Account Category Actions
    }
}