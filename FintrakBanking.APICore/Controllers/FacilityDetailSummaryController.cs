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
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/facilitydetailsummary")]
    public class FacilityDetailSummaryController : ApiControllerBase
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
        [Route("loan-product-fees/application/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanProductFeesByFacilityId(int loanApplicationDetailId)
        {
            var response = repo.GetLoanProductFeesByFacilityId(loanApplicationDetailId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-ireg/{loanReviewOperationId}")]
        public HttpResponseMessage GetLoanIregularInput(int loanReviewOperationId)
        {
            try
            {
                var data = repo.GetLoanIregularInput(loanReviewOperationId);
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
            var data = repo.FacilityDetail(loanId);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("third-party-facilty-details/{loanReferenceNumber}")]
        public HttpResponseMessage ThirdPartyFacilityDetails(string loanReferenceNumber)
        {
            var data = repo.ThirdPartyFacilityDetails(loanReferenceNumber);
            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-facilty-details/{loanId}")]
        public HttpResponseMessage GetLMSFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.LMSFacilityDetail(loanId);
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
        [Route("related-facilty-details/{loanId}")]
        public HttpResponseMessage GetRelatedFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.RelatedFacilityDetail(loanId);
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
        [Route("overdraft-facilty-details/{loanId}")]
        public HttpResponseMessage GetOverdraftFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.OverdraftFacilityDetail(loanId);
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
        [ClaimsAuthorization]
        [Route("related-overdraft-facilty-details/{loanId}")]
        public HttpResponseMessage GetRelatedOverdraftFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.RelatedOverdraftFacilityDetail(loanId);
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
        [ClaimsAuthorization]
        [Route("facilty-details-archive/{archiveId}")]
        public HttpResponseMessage GetFacilityDetailArchive(int archiveId)
        {
            try
            {
                var data = repo.FacilityDetailArchive(archiveId);
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
        [ClaimsAuthorization]
        [Route("all-facilty-details-archive/{loanId}")]
        public HttpResponseMessage GetAllFacilityDetailArchive(int loanId)
        {
            try
            {
                var data = repo.ArchiveLoanFacilityDetail(loanId);
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
        [ClaimsAuthorization]
        [Route("all-overdraft-facilty-details-archive/{loanId}")]
        public HttpResponseMessage GetAllRevolvingLoanFacilityDetailArchive(int loanId)
        {
            try
            {
                var data = repo.SearchAllRevolvingLoan(loanId);
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
        [ClaimsAuthorization]
        [Route("otherInformation/{loanId}")]
        public HttpResponseMessage GetotherInformation(int loanId)
        {
            try
            {
                var data = repo.SearchGetotherInformation(loanId);
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
        [ClaimsAuthorization]
        [Route("overdraft-facilty-details-archive/{archiveId}")]
        public HttpResponseMessage GetOverdraftFacilityDetailArchiveArchive(int archiveId)
        {
            try
            {
                var data = repo.OverdraftFacilityDetailArchive(archiveId);
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
        [ClaimsAuthorization]
        [Route("lms-overdraft-facilty-details-archive/{archiveId}")]
        public HttpResponseMessage GetOverdraftLMSFacilityDetailArchiveArchive(int archiveId)
        {
            try
            {
                var data = repo.OverdraftFacilityDetailArchive(archiveId);
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
        [ClaimsAuthorization]
        [Route("archive-periodic-loan-schedule")]
        public HttpResponseMessage GetArchiveLoanSchedule(LoanPaymentSchedulePeriodicViewModel val)
        {
            try
            {
                var data = repo.ArchivedLoanSchedule(val);
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
        [ClaimsAuthorization]
        [Route("contingent-facilty-details/{loanId}")]
        public HttpResponseMessage GetContingentFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.ContingentFacilityDetail(loanId);
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
        [ClaimsAuthorization]
        [Route("contingent-lsm-facilty-details/{loanId}")]
        public HttpResponseMessage GetContingentLmsFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.ContingentLMSFacilityDetail(loanId);
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
        [ClaimsAuthorization]
        [Route("related-contingent-facilty-details/{loanId}")]
        public HttpResponseMessage GetRelatedContingentFacilityDetail(int loanId)
        {
            try
            {
                var data = repo.RelatedContingentFacilityDetail(loanId);
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
        [ClaimsAuthorization]
        [Route("contingent-utilization/{loanId}")]
        public HttpResponseMessage GetRepayment(int loanId)
        {
            try
            {
                var data = repo.ContingentUtilization(loanId);
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
        [ClaimsAuthorization]
        [Route("loan-chargefee/{loanId}/{loanSystemTypeId}")]
        public HttpResponseMessage GetLoanChargeFee(int loanId, short loanSystemTypeId)
        {
            try
            {
                var data = repo.LoanChargeFee(loanId, loanSystemTypeId);
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
        [Route("lms-loan-convenant/{loanId}")]
        public HttpResponseMessage GetLMSLoanConvenantDetail(int loanId)
        {
            try
            {
                var data = repo.LMSLoanCovenantDetail(loanId);
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
        [Route("transaction-detail/{loanRefNo}")]
        public HttpResponseMessage GetTransactionDetail(string loanRefNo)
        {
            try
            {
                var data = repo.TransactionDetail(loanRefNo); ;
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
        [Route("loan-collateral/{loanId}/loanSystemTypeId/{loanSystemTypeId}")]
        public HttpResponseMessage GetCollateralDetail(int loanId,int loanSystemTypeId)
        {
            try
            {
                var data = repo.Collateral(loanId, loanSystemTypeId);
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
        [Route("loan-collateral-loan/{loanId}")]
        public HttpResponseMessage GetCollateralDetailLMS([FromUri]int loanId)
        {

            List<CollateralViewModel> data = repo.CollateralByLoanId(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-loan-search")]
        public HttpResponseMessage LMSLoanSearch([FromBody] SearchViewModel search)
        {
            try
            {
                //List<LoanViewModel> data = repo.LoanSearch(token.GetCompanyId, search);
                var data = repo.LMSLoanSearch(search.productTypeId, search.searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("related-loan")]
        public HttpResponseMessage RelatedLoan([FromBody] SearchViewModel search)
        {
            try
            {
                //List<LoanViewModel> data = repo.LoanSearch(token.GetCompanyId, search);
                var data = repo.RelatedFacility(search.loanSystemTypeId, search.relatedloanReferenceNumber, search.loanReferenceNumber);
                if (data==null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("daily-interest-accrual")]
        public HttpResponseMessage DailyInterestAccrual([FromBody] SearchViewModel search)
        {
            try
            {
                var data = repo.DailyInterestAccrual(search.startDate,search.endDate,search.loanReferenceNumber);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = "No record found" });
                }
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


        [HttpGet]
        [Route("loan-application/availment-completed/{searchValue}")]
        public HttpResponseMessage GetLoanForTrancheFacilityUtilization(string searchValue)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetLoanFacilityUtilization(token.GetCompanyId, token.GetStaffId, token.GetBranchId, searchValue);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-detail/{applicationdetilId}")]
        public HttpResponseMessage GetLoanFacilityId(int applicationdetilId)
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                var response = repo.GetLoanFacilityDetail(applicationdetilId);
                if (response==null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
    }

}
