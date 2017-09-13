using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;
using System.Collections.Generic;
using System.Web;
using System.Linq;

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
                    var data = repo.GetExchangeRate(currencyId, token.GetCompanyId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                }
             
        }


        [HttpPost]
        [Route("Posttransaction")]
        public HttpResponseMessage PostTransaction( [FromBody] List<FinanceTransactionViewModel> transaction)
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
        public HttpResponseMessage AddCollateralSearchLien([FromBody] CasaLienViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;
                model.userBranchId = (short)token.GetBranchId;


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
        public HttpResponseMessage PostCollateralSearch( [FromBody] CasaLienViewModel model)
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