using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance; 
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web ;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups/chart-of-account")]
    public class ChartOfAccountController : ApiControllerBase
    {
        private IChartOfAccountRepository repo;

        public ChartOfAccountController(IChartOfAccountRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet] [Route("")]
        public HttpResponseMessage GetAllAccounts( )
        {
             
              
                try
                {
                    var accounts = repo.GetAllAccounts();
                    if (accounts == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = accounts.ToList() });  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet] [Route("category/{accountCategoryId}")]
        public HttpResponseMessage GetAccountsByCategory(   short accountCategoryId)
        { 
                try
                {
                    var accounts = repo.GetAccountsByCategory(accountCategoryId);
                    if (accounts == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = accounts.ToList() });  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet]
        [Route("fs-captions")]
        public HttpResponseMessage GetFinancialSatementCaptionLookup()
        { try
            {
                var accounts = repo.GetFinancialSatementCaptionLookup();
                if (accounts == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, result = accounts.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet] [Route("{accountId}")]
        public HttpResponseMessage Get(   short accountId)
        {
              
                try
                {
                    //var accounts = repo.GetAccountViewModel(accountId);
                    //return Ok(accounts);

                    var account = repo.GetAccountViewModel(accountId);
                    if (account == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = account });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = ex.Message });
                } 
        }

        // POST api/values
        [HttpPost]
        public HttpResponseMessage AddChartOfAccount(  [FromBody]ChartOfAccountViewModel model)
        {   try
                {
                    TokenDecryptionHelper token =  new TokenDecryptionHelper();

                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;
                    model.branchId = (short)token.GetBranchId;
                    model.applicationUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                    var accountId = repo.AddAccount(model);

                    if (accountId >= 1)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = model, message = "account has been created successfully" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "account not created" });
                    }
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
                }
         
        }

        [HttpPut] [Route("{accountId}")]
        public HttpResponseMessage UpdateChartOfAccount(   int accountId, [FromBody] ChartOfAccountViewModel model)
        {
              
                if (model == null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = "No record found" });
                }
                try
                {
                    TokenDecryptionHelper token =  new TokenDecryptionHelper();

                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;
                    model.branchId = (short)token.GetBranchId;
                    model.applicationUrl = HttpContext.Current.Request.ApplicationPath;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    repo.UpdateAccount((short)accountId, model);

                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, result = model.accountId, message = "account has been updated successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
                }
                  
        }

        // DELETE api/values/5
        [HttpDelete] [Route("{accountId}")]
        public HttpResponseMessage DeleteAccount(   int accountId)
        { 
                try
                {
                    TokenDecryptionHelper token =   new TokenDecryptionHelper();
                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Url.AbsoluteUri,
                        //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };
                    repo.DeleteAccount((short)accountId, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, result = accountId, message = "account has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
                }
                  
        }
    }
}
