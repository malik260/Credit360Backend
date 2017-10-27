using FintrakBanking.APICore.JWTAuth;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.APICore.Controllers 
{
    [RoutePrefix("api/v1/operations")]
    public class LoanOperationController : ApiControllerBase
    {
        private ILoanOperationsRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanOperationController(ILoanOperationsRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("bulk-rate-customer-excemptions")]
        public HttpResponseMessage GetLoanRateCustomerExcemptions()
        {
            try
            {
                var data = repo.GetLoanRateCustomerExcemptions( token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("new-interest-rate-review")]
        public HttpResponseMessage GetNewInterestRateReviews()
        {
            try
            {
                var data = repo.GetNewInterestRateReviews(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("add-bulk-rate-excemption")]
        public HttpResponseMessage addBulkLoanRateExcemptions([FromBody] LoanViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.branchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.addBulkRateLoanExcemptions(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Bulk Rate Loan Excemption Successfully Added " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("interest-rate-change")]
        public HttpResponseMessage addBulkInterestRateChange([FromBody] LoanBulkInterestReviewViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                // entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.addInterestRateChange(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Interst Rate Successfully Added " });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        //
        [HttpPost]
        [Route("bulk-interest-rate-change/application")]
        public HttpResponseMessage AddLoanScheduleByBulkRate([FromBody] LoanBulkInterestReviewViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.AddLoanScheduleByBulkRate(entity.productPriceIndexId,entity.newInterestRate,entity.effectiveDate,token.GetStaffId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Interst Rate Successfully Added " });
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