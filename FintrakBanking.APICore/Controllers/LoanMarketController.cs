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
        public  IEnumerable<string> GetAllExceptionMessages(Exception ex)
        {
            Exception currentEx = ex;
            yield return currentEx.Message;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                yield return currentEx.Message;
            }
        }


        [Route("markets")]
      [HttpGet] [ClaimsAuthorization]  
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
      [HttpGet] [ClaimsAuthorization]  
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
        [Route("update-market/{marketId}")]
        [HttpPut]
        public HttpResponseMessage UpdateLoanMarket(int marketId,LoanMarketViewModel loanMarket)
        {
            try
            {
                loanMarket.companyId = token.GetCompanyId;

                string response = repo.UpdateLoanMarket(marketId,loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (Exception ex)
            {
                IEnumerable<String> error = ex.Messages();

                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("delete-market/{marketId}")]
        [HttpPut]
        public HttpResponseMessage DeleteLoanMarket(int marketId,LoanMarketViewModel loanMarket)
        {
            try
            {
                string response = repo.DeleteLoanMarket(marketId,loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
    }
}
