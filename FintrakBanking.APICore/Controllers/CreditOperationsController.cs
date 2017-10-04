using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Credit;
using System.Web;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/creditoperations")]
    public class LoanOperationsController : ApiControllerBase
    {
        private ILoanOperationsRepository repo;
        private ILoanRepository loanRepo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanOperationsController(ILoanOperationsRepository _repo,
            ILoanRepository _loanRepo)
        {
            this.repo = _repo;
            this.loanRepo = _loanRepo;
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
        [HttpGet]
        [Route("loan-operationtype")]
        public HttpResponseMessage GetOperationType()
        {
            try
            {
                var data = repo.GetOperationType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("loan-search/")]
        public HttpResponseMessage SearchForLoan(string searchQuery)
        {
            try
            {
                var data = loanRepo.SearchForLoan(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        [Route("loan-guarantor/")]
        public HttpResponseMessage GetLoanGuarantor(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanGuarantors(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("loan-convenant/")]
        public HttpResponseMessage GetLoanConvenant(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanCovenant(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("loan-chargefee/")]
        public HttpResponseMessage GetLoanChargeFee(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanChargeFee(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("add-loan-review")]
        public HttpResponseMessage AddOperationReview([FromBody] LoanReviewOperationViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddOperationReview(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
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