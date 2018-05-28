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
using FintrakBanking.Common.Enum;
using FintrakBanking.Repositories.Credit;
using System.Collections.Generic;
using FintrakBanking.ViewModels;

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

      [HttpGet] [ClaimsAuthorization]  
        [Route("bulk-rate-customer-excemptions")]
        public HttpResponseMessage GetLoanRateCustomerExcemptions()
        {
            try
            {
                var data = repo.GetLoanRateCustomerExcemptions(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("getrunningloan/{refNo}")]
        public HttpResponseMessage GetRunningLoans(string refNo)
        {
            try
            {
                var data = repo.GetRunningLoans(token.GetCompanyId, refNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("mature-commercial-loans/parent")]
        public HttpResponseMessage GetMaturedCommercialLoans()
        {
            try
            {
                var data = repo.GetMaturedCommercialLoansParent(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
      //[HttpGet] [ClaimsAuthorization]  
      //  [Route("maturity-instruction-type")]
      //  public HttpResponseMessage GetMaturityInstructionType()
      //  {
      //      try
      //      {
      //          var data = repo.GetMaturityInstructionType();
      //          return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
      //      }
      //      catch (Exception ex)
      //      {
      //          return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
      //      }
      //  }

      [HttpGet] [ClaimsAuthorization]  
        [Route("mature-commercial-loans/detail/{loanApplicationDetailId}")]
        public HttpResponseMessage GetMaturedCommercialLoans(int loanApplicationDetailId)
        {
            try
            {
                var data = repo.GetMaturedCommercialLoans(token.GetCompanyId, loanApplicationDetailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        //[HttpGet]
        //[Route("running-commercial-loans/detail")]
        //public HttpResponseMessage GetRunningCommercialLoanLines()
        //{
        //    try
        //    {
        //        var data = repo.GetRunningCommercialLoanLines(token.GetCompanyId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}

      [HttpGet] [ClaimsAuthorization]  
        [Route("running-commercial-loans/{refNo}")]
        public HttpResponseMessage GetRunningCommercialLoans(string refNo)
        {
            try
            {
                var data = repo.GetRunningCommercialLoans(token.GetCompanyId, refNo);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        //[HttpGet]
        //[Route("loan-maturity-instructions")]
        //public HttpResponseMessage GetLoanMaturityInstructions()
        //{
        //    try
        //    {
        //        var data = repo.GetLoanMaturityInstructions();
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
        //    }
        //}
        
        // [HttpPost] [ClaimsAuthorization]
        //[Route("commercial-loan-maturity-instruction")]
        //public HttpResponseMessage addMaturityInstruction([FromBody] MaturityIntructionViewModel entity)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;
        //        entity.companyId = token.GetCompanyId;

        //        var data = repo.addMaturityInstruction(entity);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "New Maturity Instruction Successfully Added " });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
        //    }
        //}

        // [HttpPost] [ClaimsAuthorization]
        //[Route("commercial-loan-interest-rate-change")]
        //public HttpResponseMessage CommercialPaperRateReview([FromBody] InterestReviewViewModel entity)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;
        //        entity.companyId = token.GetCompanyId;

        //        var data = repo.CommercialPaperRateReview(entity.aplicationDetailId, entity.newRate, entity);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Interest Rate Change was Successful " });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error running this update" });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record" });
        //    }
        //}

        // [HttpPost] [ClaimsAuthorization]
        //[Route("commercial-loan-roll-over")]
        //public HttpResponseMessage ProcessCommercialPaperRollOver([FromBody] MaturityIntructionViewModel entity)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;
        //        entity.companyId = token.GetCompanyId;

        //        var data = repo.ProcessCommercialPaperRollOver(entity, null);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Loan Rollover process was Successfully." });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an processing rollover for this record" });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. {e.Message}" });
        //    }
        //}

        // [HttpPost] [ClaimsAuthorization]
        //[Route("commercial-loan-tenor-extension")]
        //public HttpResponseMessage CommercialPaperTenorReview([FromBody] TenorExtionViewModel entity)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;
        //        entity.companyId = token.GetCompanyId;

        //        var data = repo.CommercialPaperTenorReview(entity);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Tenor successfully extended." });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an processing tenor extension for this record" });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error in this transaction. " });
        //    }
        //}


      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-charge-fee-byloanid/")]
        public HttpResponseMessage GetLoanChargeFeeByLoanId(int loanId)
        {
            try
            {
                var data = repo.GetLoanChargeFeeByLoanId(loanId);
                if(data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data,  });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record found."});

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }

        }

         [HttpPost] [ClaimsAuthorization]
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

         [HttpPost] [ClaimsAuthorization]
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
         [HttpPost] [ClaimsAuthorization]
        [Route("bulk-interest-rate-change/application")]
        public HttpResponseMessage AddLoanScheduleByBulkRate([FromBody] LoanBulkInterestReviewViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.BulkRateReview(entity.productPriceIndexId, entity.newInterestRate, entity.effectiveDate, token.GetStaffId, (int)OperationsEnum.TermLoanBooking);
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