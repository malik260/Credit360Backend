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
using FintrakBanking.Common.CustomException;


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
            catch (SecureException ex)
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }


        [Route("update-market/{marketId}")]
       [HttpPut] [ClaimsAuthorization]
        public HttpResponseMessage UpdateLoanMarket(int marketId,LoanMarketViewModel loanMarket)
        {
            try
            {
                loanMarket.companyId = token.GetCompanyId;

                string response = repo.UpdateLoanMarket(marketId,loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("delete-market/{marketId}")]
       [HttpPut] [ClaimsAuthorization]
        public HttpResponseMessage DeleteLoanMarket(int marketId,LoanMarketViewModel loanMarket)
        {
            try
            {
                string response = repo.DeleteLoanMarket(marketId,loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("add-market")]
         [HttpPost] [ClaimsAuthorization]
        public HttpResponseMessage AddLoanMarket(LoanMarketViewModel loanMarket)
        {
            try
            {
                loanMarket.companyId = token.GetCompanyId;

                var data = repo.AddLoanMarket(loanMarket);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }


        [Route("exposure-manual")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllExposureManual()
        {
            try
            {
                var data = repo.GetExposureManual();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }

        [Route("add-exposure")]
        [HttpPost]
        [ClaimsAuthorization]
        public HttpResponseMessage AddExposure(ExposureViewModel expo)
        {
            try
            {  
                var data = repo.AddExposure(expo,token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("Exposure-update/{exposureId}")]
        [HttpPut]
        [ClaimsAuthorization]
        public HttpResponseMessage UpdateExposure(int exposureId, ExposureViewModel expo)
        {
            try
            {
               // loanMarket.companyId = token.GetCompanyId;

                string response = repo.updateExposure(exposureId, expo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }


        [Route("delete-exposure/{exposureId}")]
        [HttpDelete]
        [ClaimsAuthorization]
        public HttpResponseMessage deleteExposure(int exposureId)
        {
            try
            {
                bool response = repo.DeleteExposure(exposureId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

    }
}
