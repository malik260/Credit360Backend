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
        [Route("loan-camsol/{loancamsolid}")]
        public HttpResponseMessage GetRunningLoans(int loancamsolid)
        {
            try
            {
                var data = repo.GetCamSol(loancamsolid);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
