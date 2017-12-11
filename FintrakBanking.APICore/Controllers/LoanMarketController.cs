using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
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
    public class LoanMarketController : ApiControllerBase
    {
        private ILoanMarketRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanMarketController(ILoanMarketRepository _repo)
        {
            repo = _repo;
        }

        [Route("markets")]
        [HttpGet]
        public HttpResponseMessage GetAllLoanMarket()
        {
            try
            {
                var data = repo.GetLoanMarket(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("market")]
        [HttpGet]
        public HttpResponseMessage GetAllLoanMarket(int marketId)
        {
            try
            {
                var data = repo.GetLoanMarket(marketId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }
        [Route("update-market")]
        [HttpPut]
        public HttpResponseMessage UpdateLoanMarket(int principalId, LoanMarketViewModel loanMarket)
        {
            try
            {
                string response = repo.UpdateLoanMarket(loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("delete-market")]
        [HttpPut]
        public HttpResponseMessage DeleteLoanMarket(LoanMarketViewModel loanMarket)
        {
            try
            {
                string response = repo.DeleteLoanMarket(loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("add-market")]
        [HttpPost]
        public HttpResponseMessage AddLoanMarket(LoanMarketViewModel loanMarket)
        {
            try
            {
                loanMarket.companyId = token.GetCompanyId;

                var data = repo.AddLoanMarket(loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
    }
}
