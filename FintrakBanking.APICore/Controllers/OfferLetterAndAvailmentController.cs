using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels;
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
        private IOfferLetterAndAvailmentRepository repo;

        public OfferLetterAndAvailmentController(
            IErrorLogRepository _errorLogger,
            IOfferLetterAndAvailmentRepository _repo
            )
        {
            errorLogger = _errorLogger;
            repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/approved-loans")]
        public async Task<HttpResponseMessage> GetCamProcessedLoanApplicationsDueForOfferLetter()
        {
            try
            {
                var response = await repo.GetApplicationsAtOfferLetter(token.GetStaffId, token.GetCompanyId).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count(), message = "No record found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/due-for-review")]
        public async Task<HttpResponseMessage> GetCamProcessedLoanApplicationsDueForReview()
        {
            try
            {
                var staffid = token.GetStaffId;
                var response = await repo.GetApplicationsAtOfferLetter(token.GetStaffId, token.GetBranchId, token.GetCompanyId).ToListAsync();
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
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/due-for-availment")]
        public async Task<HttpResponseMessage> GetCamProcessedLoanApplicationsDueForAvailment()
        {
            try
            {
                var response = await repo.GetApplicationsDueForAvailment(token.GetStaffId, token.GetCompanyId).ToListAsync();

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
        [ClaimsAuthorization]
        [Route("availment-due-for-checklist")]
        public HttpResponseMessage GetApplicationsDueForAvailmentCheckList()
        {
            try
            {
                var response = repo.GetApplicationsAtOfferLetter(token.GetStaffId, token.GetBranchId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/under-review")]
        public async Task<HttpResponseMessage> GetCamProcessedApplicationsUnderReview()
        {
            try
            {
                var response = await repo.GetApplicationsAtOfferLetter(token.GetStaffId, token.GetBranchId, token.GetCompanyId).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("updateFinalOfferLetter/{applicationRef}")]
        public HttpResponseMessage UpdateFinalOfferLetter(string applicationRef, OfferLetterTemplateViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                var data = repo.UpdateFinalOfferLetter(applicationRef, model);
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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter-template/{applicationRefNumber}")]
        public HttpResponseMessage GenerateOfferLetterTemplate([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = repo.GenerateOfferLetterTemplate(applicationRefNumber);

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-form38b-template/{applicationRefNumber}")]
        public HttpResponseMessage GenerateForm3800Template([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = repo.GenerateForm3800Template(applicationRefNumber);

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-form38b-template-lms/{applicationRefNumber}")]
        public HttpResponseMessage GenerateForm3800TemplateLMS([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = repo.GenerateForm3800TemplateLMS(applicationRefNumber);

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter")]
        public HttpResponseMessage SaveDraftOfferLetter([FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                var response = repo.SaveDraftOfferLetter(model);

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

        //[HttpGet]
        //[Route("loan-application-collateral/{loanApplicationId}")]
        //public HttpResponseMessage GetLoanBookingAwaitingApproval(int loanApplicationId)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //        var data = repo.GetLoanApplicationCollateralByApplicationId(loanApplicationId);

        //        if (data.Any() == false)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        //    }
        //    catch (ConditionNotMetException ce)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
        //    }
        //    catch (BadLogicException be)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
        //    }
        //}

        [HttpPut]
        [ClaimsAuthorization]
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

                var response = repo.UpdateDraftOfferLetter(documentId, model);

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter/all")]
        public HttpResponseMessage GetAllDraftOfferLetters()
        {
            try
            {
                var response = repo.GetAllDraftOfferLetters().ToList();

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter/{applicationRefNumber}")]
        public HttpResponseMessage GetDraftOfferLetterByApplRefNumber([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = repo.GetDraftOfferLetterByApplRefNumber(applicationRefNumber);

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter/final/all")]
        public HttpResponseMessage GetAllFinalOfferLetters()
        {
            try
            {
                var response = repo.GetAllFinalOfferLetters().ToList();

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter/final/{applicationRefNumber}")]
        public HttpResponseMessage GetFinalOfferLetterByApplRefNumber([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = repo.GetFinalOfferLetterByApplRefNumber(applicationRefNumber);

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
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter/final")]
        public HttpResponseMessage SaveFinalOfferLetter([FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                var response = repo.SaveFinalOfferLetter(model);

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
        [ClaimsAuthorization]
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

                var data = repo.ApproveLoanAvailmentDecision(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Availment completed, now proceeding to booking" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $" {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{be.Message}" });
            }
            catch (APIErrorException ae)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ae.Message}" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.Message, message = "An error occured" });
            }
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("loan-application/availment/approval")]
        //public HttpResponseMessage LogApplicationForApprovalDuringAvailment([FromBody] LoanAvailmentApprovalViewModel entity)
        //{
        //    try
        //    {
        //        entity.BranchId = token.GetBranchId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.staffId = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.userIPAddress = Request.RequestUri.Host;
        //        entity.createdBy = token.GetStaffId;

        //        var data = repo.LogApplicationForApprovalDuringAvailment(entity);

        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, message = "Operation successful, request has been routed to the next approving office" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, message = "Operation not successfull" });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.Message });
        //    }
        //}

        [HttpPost]
        [ClaimsAuthorization]
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

                var data = repo.ApproveOfferLetterGeneration(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation successful!" });
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/forward-bonds-and-guarantee")]
        public HttpResponseMessage ForwardBondsAndGuarantee([FromBody] ForwardViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var response = repo.ForwardBondsAndGuarantee(entity);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/rejection")]
        public HttpResponseMessage OfferLetterRejection([FromBody] ForwardViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                bool response = repo.OfferLetterRejection(entity);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/comments/{applicationRefNumber}")]
        public HttpResponseMessage GetCommentOnLoanAvailment([FromUri] string applicationRefNumber)
        {
            try
            {
                var response = repo.GetCommentOnLoanAvailment(applicationRefNumber);

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

    }
}