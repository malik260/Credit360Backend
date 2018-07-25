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
    [RoutePrefix("api/v1/facilitydetailsummary")]
    public class FacilityDetailSummaryController : ApiController
    {
        private IFacilityDetailSummary repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        public FacilityDetailSummaryController(IFacilityDetailSummary _repo)
        {
            repo = _repo;
        }
      

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-schedule/{loanId}")]
        public HttpResponseMessage GetLoanSchedule(int loanId)
        {
            try
            {
                var data = repo.LoanSchedule(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("facilty-details/{loanId}")]
        public HttpResponseMessage GetFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.FacilityDetail(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-chargefee/{loanId}")]
        public HttpResponseMessage GetLoanChargeFee(int loanId)
        {
            try
            {
                var data = repo.LoanChargeFee(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-convenant/{loanId}")]
        public HttpResponseMessage GetLoanConvenantDetail(int loanId)
        {
            try
            {
                var data = repo.LoanCovenantDetail(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-collateral/{loanId}")]
        public HttpResponseMessage GetCollateralDetail(int loanId)
        {
            try
            {
                var data = repo.Collateral(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-search")]
        public HttpResponseMessage LoanSearch([FromBody] SearchViewModel search)
        {
            try
            {
                //List<LoanViewModel> data = repo.LoanSearch(token.GetCompanyId, search);
                var data = repo.LoanSearch(search.productTypeId, search.searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("product-type")]
        public HttpResponseMessage GetProductType()
        {
            try
            {
                var data = repo.ProductType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
    }
}
