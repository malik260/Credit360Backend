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
using FintrakBanking.Common.CustomException;

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

        [Route("loan-principals")]
      [HttpGet] [ClaimsAuthorization]  
        public HttpResponseMessage GetAllLoanPrincipal()
        {
            try
            {
                var data = repo.GetLoanPrincipal(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("getprincipal")]
      [HttpGet] [ClaimsAuthorization]  
        public HttpResponseMessage GetAllLoanPrincipal(int id)
        {
            try
            {
                var data = repo.GetLoanPrincipal(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }
        [Route("update-principal")]
       [HttpPut] [ClaimsAuthorization]
        public HttpResponseMessage UpdateLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            try
            {
                //if (ModelState.IsValid)
                //{

                //}
                string response  = repo.UpdateLoanPrincipal( loanPrincipal);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("delete-principal")]
       [HttpPut] [ClaimsAuthorization]
        public HttpResponseMessage DeleteLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            try
            {
                string response = repo.DeleteLoanPrincipal(loanPrincipal);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("add-principal")]
         [HttpPost] [ClaimsAuthorization]
        public HttpResponseMessage AddLoanPrincipal(LoanPrincipalViewModel loanPrincipal)
        {
            try
            {
                loanPrincipal.companyId = token.GetCompanyId;

                var data = repo.AddLoanPrincipal(loanPrincipal);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
    }
}
