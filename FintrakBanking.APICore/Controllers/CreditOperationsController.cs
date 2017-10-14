using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Interfaces.Finance;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/creditoperations")]
    public class LoanOperationsController : ApiControllerBase
    {
        private ILoanOperationsRepository repo;
        private ILoanRepository loanRepo;
        private IEndOfDayRepository repoEOD;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanOperationsController(ILoanOperationsRepository _repo,
            IEndOfDayRepository _repoEOD,
            ILoanRepository _loanRepo)
        {
            this.repo = _repo;
            this.repoEOD = _repoEOD;
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
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
        [Route("loan-operationtype/")]
        public HttpResponseMessage GetOperationTypeByLoanId(int productTypeId, int scheduleTypeId)
        {
            try
            {
                var data = repo.GetOperationTypeByLoanId((LoanProductTypeEnum)productTypeId, (LoanScheduleTypeEnum)scheduleTypeId);
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
        [Route("end-of-day")]
        public HttpResponseMessage RunEndOfDay([FromBody] EndOfDayViewModel model)
        {
            try
            {
                var data = repoEOD.RunEndOfDay(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = true, message = "End of day transaction completed successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "End of day transaction failed" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
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


        [HttpGet]
        [Route("loan-schedule-details/")]
        public HttpResponseMessage GetOperationTypeByLoanId(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanScheduleByLoanId(loanId);
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
        [Route("loan-operation/awaiting-approval")]
        public HttpResponseMessage GetLoanBookingAwaitingApproval()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetLoanOperationAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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


        [HttpPost]
        [Route("loanrephasement")]
        public HttpResponseMessage LoanRephasementProcess([FromBody] LoanReviewOperationViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                var response = repo.LoanRephasementProcess((short)model.loanReviewOperationsId,model.loanId,model.createdBy);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Loan Rephasement Process was successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error with Loan Rephasement Process" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error with Loan Rephasement Process {e.Message}" });
            }
        }
    }
}