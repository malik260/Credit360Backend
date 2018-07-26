using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.Common.CustomException;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/setups")]
    public class AccountTypeController : ApiControllerBase
    {
        private IAccountTypeRepository repo;

        public AccountTypeController(IAccountTypeRepository _repo)
        {
            this.repo = _repo;
        }

        #region Account Type Actions

        [HttpGet]
        [ClaimsAuthorization]
        [Route("account-type", Name = "GetAccountType")]
        public HttpResponseMessage GetAllAccountType(HttpRequestMessage request)
        {
            try
            {
                var data = repo.GetAllAccountType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("account-type/{accountTypId}", Name = "GetAccountTypeById")]
        public HttpResponseMessage GetAllAccountTypeById(HttpRequestMessage request, int accountTypId)
        {
            try
            {
                var data = repo.GetAllAccountTypeById(accountTypId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("account-type")]
        public HttpResponseMessage AddAccountType([FromBody] AddAccountTypeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var isCreated = repo.AddAccountType(model);
                if (isCreated == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = isCreated, message = "account type has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "account type not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("account-type/{accountTypeId}")]
        public HttpResponseMessage UpdateAccountType(int accountTypeId, [FromBody]AccountTypeViewModel model)
        {
            HttpResponseMessage result = null;
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                if (repo.UpdateAccountType(accountTypeId, model))
                {
                    result = Request.CreateResponse(HttpStatusCode.OK, Ok(model));
                }
            }
            catch (SecureException ex)
            {
                result = Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            return result;
        }

        #endregion Account Type Actions
    }
}