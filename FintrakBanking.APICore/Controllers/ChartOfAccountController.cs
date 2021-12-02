using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.Finance; 
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web ;
using System.Web.Http;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;

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

      [HttpGet] [ClaimsAuthorization]   [Route("")]
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
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet]
        [Route("account-name-by-account-code/")]
        public HttpResponseMessage GetAccountNameByAccountCode(string accountCode)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var accountInfo = repo.GetAccountNameByAccountCode(accountCode);

                if (accountInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = accountInfo });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
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
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = ex.Message });
                }
                  
        }

      [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]   [Route("{accountId}")]
        public HttpResponseMessage Get(   short accountId)
        {
              
                try
                {
                    //var accounts = repo.GetAccountByAccountId(accountId);
                    //return Ok(accounts);

                    var account = repo.GetAccountByAccountId(accountId);
                    if (account == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = account });
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = ex.Message });
                } 
        }

        // POST api/values
        // [HttpPost] [ClaimsAuthorization]
        //public HttpResponseMessage AddChartOfAccount([FromBody]ChartOfAccountViewModel model)
        //{   try
        //        {
        //            TokenDecryptionHelper token =  new TokenDecryptionHelper();

        //            model.createdBy = token.GetStaffId;
        //            model.companyId = token.GetCompanyId;
        //            model.branchId = (short)token.GetBranchId;
        //            model.applicationUrl = HttpContext.Current.Request.Url.AbsoluteUri;
        //            //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        //            var accountId = repo.AddTempAccount(model);

        //            if (accountId >= 1)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK,
        //               new { success = true, result = model, message = "account has been created successfully" });
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = false, message = "account not created" });
        //            }
        //        }
        //        catch (SecureException ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = false, message = ex.Message });
        //        }

        //}

         [HttpPost] [ClaimsAuthorization]
        [Route("")]
        public HttpResponseMessage AddTempAccount([FromBody] ChartOfAccountViewModel model)
        {
            try
            {
                if (repo.IsAccountCodeAlreadyExist(model.accountCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"An Account with {model.accountCode} already exist" });
                }
                if (repo.IsTempAccountExist(model.accountCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"An account with {model.accountCode} already exist waiting for approval" });
                }

                TokenDecryptionHelper token = new TokenDecryptionHelper();

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.branchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;

                var account = repo.AddTempAccount(model);
                if (account)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = account, message = "Account has been created successfully, now waiting for approval" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Account not created" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("{accountId}")]
        public HttpResponseMessage UpdateAccount(short accountId, [FromBody] ChartOfAccountViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = (short)token.GetCompanyId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;

                foreach (var curr in model.currencies)
                {
                    curr.createdBy = model.createdBy;
                    curr.companyId = model.companyId;
                }

                var account = repo.UpdateAccount(accountId, model);
                if (account)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = account, message = "Account has been updated successfully, now waiting for approval" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Account not created" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("approval")]
        public HttpResponseMessage GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Account record has been approved successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approvals/temp")]
        public HttpResponseMessage GetAccountsAwaitingApproval()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var accountInfo = repo.GetAccountsAwaitingApprovals(token.GetStaffId, token.GetCompanyId);

                if (accountInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = accountInfo.ToList() });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approvals/temp/{accountId}")]
        public HttpResponseMessage GetTempProductDetailsById(int accountId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var accountInfo = repo.GetTempAccountDetail(accountId);

                if (accountInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = accountInfo });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
        //[HttpPut] [Route("{accountId}")]
        //public HttpResponseMessage UpdateChartOfAccount(   int accountId, [FromBody] ChartOfAccountViewModel model)
        //{
        //        if (model == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //              new { success = false, message = "No record found" });
        //        }
        //        try
        //        {
        //            TokenDecryptionHelper token =  new TokenDecryptionHelper();

        //            model.createdBy = token.GetStaffId;
        //            model.companyId = token.GetCompanyId;
        //            model.branchId = (short)token.GetBranchId;
        //            model.applicationUrl = HttpContext.Current.Request.ApplicationPath;
        //            repo.UpdateAccount((short)accountId, model);

        //            return Request.CreateResponse(HttpStatusCode.OK,
        //              new { success = true, result = model.accountId, message = "account has been updated successfully" });
        //        }
        //        catch (SecureException ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //              new { success = false, message = ex.Message });
        //        }   
        //}

        // DELETE api/values/5
        [HttpDelete] [ClaimsAuthorization] [Route("{accountId}")]
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
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
                }
                  
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("classes")]
        public HttpResponseMessage GetChartOfAccountClasses()
        {
            try
            {
                var data = repo.GetChartOfAccountClasses();

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, result = data.ToList(), message = "No records found!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = ex.Message });
            }

        }
    }
}
