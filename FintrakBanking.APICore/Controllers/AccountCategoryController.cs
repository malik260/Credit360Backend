using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Setups.Finance; 
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
   
    [RoutePrefix("api/v1/setups/account-category")]
    public class AccountCategoryController : ApiControllerBase
    {
        private IAccountCategoryRepository repo;

        public AccountCategoryController(IAccountCategoryRepository _repo)
        {
            this.repo = _repo;
        }

        #region Account Category Actions

        [HttpGet]
        [Route("", Name = "Category")]
        public HttpResponseMessage GetAllAccountType( )
        {
                 try
                {
                    var data = repo.GetAllAccountCategory();
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data.ToList() });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet][Route("{categoryId}", Name = "categoryById")]
        public HttpResponseMessage GetAccountTypeById( int categoryId)
        { 
                try
                {
                    var data = repo.GetAccountCategoryById(categoryId);
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
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