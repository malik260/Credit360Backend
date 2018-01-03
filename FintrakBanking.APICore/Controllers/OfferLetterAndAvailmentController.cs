using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class OfferLetterAndAvailmentController : ApiControllerBase
    {
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IErrorLogRepository errorLogger;
        private IOfferLetterAndAvailmentRepository olAvlmentRepo;

        public OfferLetterAndAvailmentController(
            IErrorLogRepository _errorLogger,
            IOfferLetterAndAvailmentRepository _olAvlmentRepo
            )
        {
            errorLogger = _errorLogger;
            olAvlmentRepo = _olAvlmentRepo;
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/approved-loans")]
        public async Task<HttpResponseMessage> GetCamProcessedLoanApplicationsDueForOfferLetter()
        {
            try
            {
                var response = await olAvlmentRepo.GetApplicationsDueForOfferLetterGeneration(token.GetStaffId, token.GetCompanyId).ToListAsync();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }



        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-bondandguarantees")]
        public async Task<HttpResponseMessage> GetApplicationsDueBondAndGuarantees()
        {
            try
            {
                var response = await olAvlmentRepo.GetApplicationsDueBondAndGuarantees(token.GetStaffId, token.GetCompanyId).ToListAsync();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-review")]
        public async Task<HttpResponseMessage> GetCamProcessedLoanApplicationsDueForReview()
        {
            try
            {
                var response = await olAvlmentRepo.GetApplicationsForReviewFromCreditUnit(token.GetStaffId, token.GetCompanyId).ToListAsync();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList(), count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-availment")]
        public async Task<HttpResponseMessage> GetCamProcessedLoanApplicationsDueForAvailment()
        {
            try
            {
                var response = await olAvlmentRepo.GetApplicationsDueForAvailment(token.GetStaffId, token.GetCompanyId).ToListAsync();

                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/under-review")]
        public async Task<HttpResponseMessage> GetCamProcessedApplicationsUnderReview()
        {
            try
            {
                var response = await olAvlmentRepo.GetApplicationsUnderForReview(token.GetCompanyId).ToListAsync();

                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("loan-application/applicationRef/{applicationRefNumber}/statusId/{applicationStatusId}")]
        public HttpResponseMessage UpdateApplicationStatus([FromUri] string applicationRefNumber, [FromUri] short applicationStatusId)
        {
            try
            {
                var response = olAvlmentRepo.UpdateLoanApplicationStatus(applicationRefNumber.Trim(), applicationStatusId);

                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "record not updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "record updated successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }


        [HttpPut]
        [Route("updateFinalOfferLetter/{applicationRef}")]
        public HttpResponseMessage UpdateFinalOfferLetter(string applicationRef, OfferLetterTemplateViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                var data = olAvlmentRepo.UpdateFinalOfferLetter(applicationRef, model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }



        #region Offer Letter & Availment

        [HttpGet]
        [Route("loan-application/prepared-offer-letter-template/{applicationRefNumber}")]
        public HttpResponseMessage GenerateOfferLetterTemplate([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = olAvlmentRepo.GenerateOfferLetterTemplate(applicationRefNumber);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-form38b-template/{applicationRefNumber}")]
        public HttpResponseMessage GenerateForm3800Template([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = olAvlmentRepo.GenerateForm3800Template(applicationRefNumber);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan-application/prepared-offer-letter")]
        public HttpResponseMessage SaveDraftOfferLetter([FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                var response = olAvlmentRepo.SaveDraftOfferLetter(model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document saved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document not saved successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("loan-application/prepared-offer-letter/{documentId}")]
        public HttpResponseMessage UpdateDraftOfferLetter(int documentId, [FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = (short)token.GetCompanyId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;

                var response = olAvlmentRepo.UpdateDraftOfferLetter(documentId, model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document not updated successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/all")]
        public HttpResponseMessage GetAllDraftOfferLetters()
        {
            try
            {
                var response = olAvlmentRepo.GetAllDraftOfferLetters().ToList();

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/{applicationRefNumber}")]
        public HttpResponseMessage GetDraftOfferLetterByApplRefNumber([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = olAvlmentRepo.GetDraftOfferLetterByApplRefNumber(applicationRefNumber);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/final/all")]
        public HttpResponseMessage GetAllFinalOfferLetters()
        {
            try
            {
                var response = olAvlmentRepo.GetAllFinalOfferLetters().ToList();

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/final/{applicationRefNumber}")]
        public HttpResponseMessage GetFinalOfferLetterByApplRefNumber([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = olAvlmentRepo.GetFinalOfferLetterByApplRefNumber(applicationRefNumber);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan-application/prepared-offer-letter/final")]
        public HttpResponseMessage SaveFinalOfferLetter([FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                var response = olAvlmentRepo.SaveFinalOfferLetter(model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document saved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document not saved successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan-application/availment/approval-decision")]
        public HttpResponseMessage ApproveLoanAvailmentDecision([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.createdBy = token.GetStaffId;

                var data = olAvlmentRepo.ApproveLoanAvailmentDecision(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Availment completed, now proceeding to booking" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("loan-application/availment/approval")]
        public HttpResponseMessage LogApplicationForApprovalDuringAvailment([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.createdBy = token.GetStaffId;

                var data = olAvlmentRepo.LogApplicationForApprovalDuringAvailment(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation not successfull" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("loan-application/offer-letter/approval")]
        public HttpResponseMessage LogApplicationForApprovalDuringOfferLetterGeneration([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
                entity.createdBy = token.GetStaffId;

                var data = olAvlmentRepo.ApproveOfferLetterGeneration(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Now proceeding to availment" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.Message });
            }
        }

        #endregion Offer Letter & Availment

    }
}