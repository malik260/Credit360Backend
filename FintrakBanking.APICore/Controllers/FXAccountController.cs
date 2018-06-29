using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class FXAccountController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IFXAccountCreationRepository repo;

        public FXAccountController(IFXAccountCreationRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-fx-code")]
        public HttpResponseMessage GetListOfFSCode()
        {
            try
            {
                var data = repo.GetListOfFSCode();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-gl-subhead-code")]
        public HttpResponseMessage GetAllGLSubHead(string schemeCode)
        {
            try
            {
                var data = repo.GetAllGLSubHead(schemeCode);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("foreign-currency-account-creation")]
        public HttpResponseMessage ForeignCurrencyAccountCreation([FromBody]CreateAccountViewModel entity)
        {
            try
            {

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };

                var data = repo.ForeignCurrencyAccountCreation(entity, user);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The account has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this account" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
    }
}
