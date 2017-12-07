using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LoanPrincipalController : ApiControllerBase
    {
        private ILoanPrincipalRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanPrincipalController(ILoanPrincipalRepository _repo)
        {
            repo = _repo;
        }

        [Route("getprincipals")]
        [HttpGet]
        public HttpResponseMessage GetAllLoanPrincipal()
        {
            try
            {
                var data = repo.GetLoanPrincipal(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("getprincipal")]
        [HttpGet]
        public HttpResponseMessage GetAllLoanPrincipal(int principalId)
        {
            try
            {
                var data = repo.GetLoanPrincipal(principalId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }
        [Route("updateprincipal")]
        [HttpPut]
        public HttpResponseMessage UpdateLoanPrincipal(int principalId, LoanPrincipalViewModel loanPrincipal)
        {
            try
            {
                string response  = repo.UpdateLoanPrincipal( loanPrincipal);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("deleteprincipal")]
        [HttpPut]
        public HttpResponseMessage DeleteLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            try
            {
                string response = repo.DeleteLoanPrincipal(loanPrincipal);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("addprincipal")]
        [HttpPost]
        public HttpResponseMessage AddLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            try
            {
                loanPrincipal.companyId = token.GetCompanyId;

                var data = repo.AddLoanPrincipal(loanPrincipal);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
    }
}
