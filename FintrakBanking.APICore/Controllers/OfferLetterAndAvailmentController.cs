using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public HttpResponseMessage GetCamProcessedLoanApplications()
        {
            try
            {
                var response = olAvlmentRepo.GetApplicationsDueForOfferLetterGeneration(token.GetStaffId, token.GetCompanyId).ToList();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("loan-application/send-for-availment/{applicationRefNumber}/statusId/{applicationStatusId}")]
        public HttpResponseMessage UpdateApplicationStatus(int applicationRefNumber, short applicationStatusId)
        {
            try
            {
                var response = olAvlmentRepo.UpdateLoanApplicationStatus(applicationRefNumber.ToString(), applicationStatusId);

                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "record not updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "record updated successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-review")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForReview()
        {
            try
            {
                var response = olAvlmentRepo.GetApplicationsForReviewFromCreditUnit(token.GetStaffId, token.GetCompanyId).ToList();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList(), count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-availment")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForAvailment()
        {
            try
            {
                var response = olAvlmentRepo.GetApplicationsDueForAvailment(token.GetStaffId, token.GetCompanyId).ToList();

                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #region Offer Letter & Availment

        [HttpGet]
        [Route("loan-application/prepared-offer-letter-template")]
        public HttpResponseMessage GenerateOfferLetterTemplate(string applicationRefNumber)
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/")]
        public HttpResponseMessage GetDraftOfferLetterByApplRefNumber(string applicationRefNumber)
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/final/")]
        public HttpResponseMessage GetFinalOfferLetterByApplRefNumber(string applicationRefNumber)
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Offer Letter & Availment

    }
}