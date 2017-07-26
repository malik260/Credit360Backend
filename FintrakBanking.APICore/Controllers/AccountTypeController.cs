using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels.Setups.Finance; 
 
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        [HttpGet][Route("account-type", Name = "GetAccountType")]
        public HttpResponseMessage GetAllAccountType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllAccountType();
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet][Route("account-type/{accountTypId}", Name = "GetAccountTypeById")]
        public HttpResponseMessage GetAllAccountTypeById(HttpRequestMessage request,int accountTypId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllAccountTypeById(accountTypId);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPost]
        [Route("account-type")]
        public HttpResponseMessage AddAccountType(HttpRequestMessage request, [FromBody] AddAccountTypeViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;///new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    // model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;
                    var isCreated = repo.AddAccountType(model);
                    if (isCreated == true)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = isCreated, message = "account type has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "account type not created" }));
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPut][Route("account-type/{accountTypeId}")]
        public HttpResponseMessage UpdateAccountType(HttpRequestMessage request, int accountTypeId, [FromBody]AccountTypeViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    // model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;
                    if (repo.UpdateAccountType(accountTypeId, model))
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(model));
                    }
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        #endregion Account Type Actions
    }
}