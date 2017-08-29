using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/finance")]
    public class FinanceTransactionController : ApiControllerBase
    {
        private IFinanceTransactionRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

         

        public FinanceTransactionController(IFinanceTransactionRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("getexchangerate/{currencyId}")]
        public HttpResponseMessage GetExchangeRate(short currencyId)
        { 
                try
                {
                    var data = repo.GetExchangeRate(1, 1);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                }
             
        }


        [HttpPost]
        [Route("Posttransaction")]
        public HttpResponseMessage PostTransaction(HttpResponseMessage request, [FromBody] FinanceTransactionViewModel transaction)
        {
            try
            {
                var data = repo.PostTransaction(transaction);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Transaction Posted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


        [HttpPost]
        [Route("addcollateralsearchlien")]
        public HttpResponseMessage AddCollateralSearchLien(HttpResponseMessage request, [FromBody] CasaLienViewModel model)
        {
            try
            {
                var data = repo.AddCollateralSearchLien(model);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


        [HttpPost]
        [Route("Posttransaction")]
        public HttpResponseMessage PostCollateralSearch(HttpResponseMessage request, [FromBody] CasaLienViewModel model)
        {
            try
            {
                var data = repo.PostCollateralSearch(model);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Transaction Posted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


    }
} 