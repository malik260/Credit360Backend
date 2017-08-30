using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.CreditOperations;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/creditoperations")]
    public class CreditOperationsController : ApiControllerBase
    {
        private ICreditOperationsRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CreditOperationsController(ICreditOperationsRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("getcollateralsearchchargeamount{stateId}")]
        public HttpResponseMessage GetCollateralSearchChargeAmount(int stateId)
        { 
                try
                {
                    var data = repo.GetCollateralSearchChargeAmount(stateId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                }
             
        }


    }
} 