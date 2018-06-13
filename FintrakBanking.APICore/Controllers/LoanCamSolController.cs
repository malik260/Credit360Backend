using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/camsol")]
    public class LoanCamSolController : ApiControllerBase
    {
        private ILaonCamSolRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanCamSolController(ILaonCamSolRepository _repo)
        {
            repo = _repo;
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol")]
        public HttpResponseMessage GetAllCamsol()
        {
            try
            {
                var data = repo.GetCamSol();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol/{searchValue}")]
        public HttpResponseMessage GetLoanCamsolSearch(string searchValue)
        {
            try
            {
                var data = repo.GetCamSol(searchValue);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol/{id}")]
        public HttpResponseMessage GetLoanCamsolById(int id)
        {
            try
            {
                var data = repo.GetCamSolByType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-type")]
        public HttpResponseMessage GetLoanCansolType()
        {
            try
            {
                var data = repo.GetCamSolType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

    }
}
