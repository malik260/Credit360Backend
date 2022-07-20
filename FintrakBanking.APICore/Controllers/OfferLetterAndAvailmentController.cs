using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
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
        private IOfferLetterAndAvailmentRepository repo;

        public OfferLetterAndAvailmentController(
            IOfferLetterAndAvailmentRepository _repo
            )
        {
            repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/approved-loans")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForOfferLetter()
        {
            try
            {
                var response = repo.GetApplicationsAtOfferLetter(token.GetStaffId, token.GetCompanyId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count(), message = "No record found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("save-collateral-type-crms")]
        public HttpResponseMessage AddCRMSCollateralType([FromBody] ApprovedLoanDetailViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                int applicationId = model.loanApplicationDetailId;
                var data = repo.AddCRMSCollateralType(applicationId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/due-for-review")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForReview()
        {
            try
            {
                var staffid = token.GetStaffId;
                var response = repo.GetApplicationsAtOfferLetter(token.GetStaffId, token.GetBranchId, token.GetCompanyId).ToList();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList(), count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/due-for-availment")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForAvailment()
        {
            try
            {
                var response = repo.GetApplicationsDueForAvailment(token.GetStaffId, token.GetCompanyId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/credit-assessment-memorandum/due-for-availment-route")]
        public HttpResponseMessage GetApplicationsDueForAvailmentRoute()
        {
            try
            {
                var response = repo.GetApplicationsDueForAvailmentRoute(token.GetStaffId, token.GetCompanyId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("updateFinalOfferLetter/{loanApplicationId}")]
        public HttpResponseMessage UpdateFinalOfferLetter([FromUri] int loanApplicationId, [FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                var data = repo.UpdateFinalOfferLetter(loanApplicationId, model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
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
        //    catch (SecureException ex)
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
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/prepared-offer-letter/final/{loanApplicationId}")]
        public HttpResponseMessage GetFinalOfferLetterByApplRefNumber([FromUri] int loanApplicationId)
        {
            try
            {
                var response = repo.GetFinalOfferLetterByApplRefNumber(loanApplicationId);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (SecureException e)
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
                var response = repo.SaveFinalOfferLetter(1,model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document saved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document not saved successfully" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/availment/approval-decision")]
        public HttpResponseMessage ApproveLoanAvailmentDecision([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            var data = repo.ApproveLoanAvailmentDecision(entity);

            if (data == 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Availment completed, now proceeding to booking" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation successful, request has been routed to the next approving office" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("availment/loan-application/back-to-business")]
        public HttpResponseMessage SendBackToBusinessAvailment([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            bool data = repo.SendBackToBusinessAvailment(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation successful" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("availment/refer-back-one-step")]
        public HttpResponseMessage ReferBackOneStep([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            bool data = repo.ReferBackOneStep(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation successful" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/offer-letter/approval")]
        public HttpResponseMessage LogApplicationForApprovalDuringOfferLetterGeneration([FromBody] LoanAvailmentApprovalViewModel entity)
        {
            entity.BranchId = token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = Request.RequestUri.Host;
            entity.createdBy = token.GetStaffId;

            var response = repo.ApproveOfferLetterGeneration(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { result = response, success = response.success, message = "Application sent to " + response.nextLevelName });
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
            catch (SecureException ex)
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        } 

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("offer-letter/refer-back/{applicationId}")]
        //public HttpResponseMessage OfferLetterReferBack([FromBody] ApprovalViewModel entity)
        //{
        //    entity.BranchId = (short)token.GetBranchId;
        //    entity.companyId = token.GetCompanyId;
        //    entity.createdBy = token.GetStaffId;
        //    entity.staffId = token.GetStaffId;
        //    entity.applicationUrl = HttpContext.Current.Request.Path;

        //    bool response = repo.OfferLetterReferBack(entity);

        //    if (response == true)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        //}

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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/edit/clause")]
        public HttpResponseMessage EditOfferLetterClause(OfferLetterViewModel data)
        {
            try
            {
                bool response = repo.EditOfferLetterClause(data.loanApplicationId, data.offerLetterClauses,data.isLMS, token.GetStaffId, token.GetBranchId);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/edit/acceptance")]
        public HttpResponseMessage EditOfferLetterAcceptance(OfferLetterViewModel data)
        {
            try
            {
                bool response = repo.EditOfferLetterAcceptance(data.loanApplicationId, data.offerLetteracceptance,data.isLMS, token.GetStaffId, token.GetBranchId);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/edit/title")]
        public HttpResponseMessage EditOfferLetterTtitle(OfferLetterViewModel data)
        {
            try
            {
                bool response = repo.EditOfferLetterTitle(data.customerId, data.offerLetterTitle, token.GetStaffId, token.GetBranchId);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/edit/salutation")]
        public HttpResponseMessage EditOfferLetterSalutation(OfferLetterViewModel data)
        {
            try
            {
                bool response = repo.EditOfferLetterSalutation(data.customerId, data.offerLetterSalutation, token.GetStaffId, token.GetBranchId);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("offer-letter/get/salutation/{customerId}")]
        public HttpResponseMessage GetOfferLetterSalutation(int customerId)
        {
            try
            {
                var response = repo.GetOfferLetterSalutation(customerId);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("offer-letter/get/title/{customerId}")]
        public HttpResponseMessage GetOfferLetterTitle(int customerId)
        {
            try
            {
                var response = repo.GetOfferLetterTitle(customerId);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("offer-letter/get/clauses/{loanApplicationId}")]
        public HttpResponseMessage GetOfferLetterClauses(int loanApplicationId)
        {
            try
            {
                var response = repo.GetOfferLetterClause(loanApplicationId);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("offer-letter/get/acceptance/{loanApplicationId}")]
        public HttpResponseMessage GetOfferLetterAcceptance(int loanApplicationId)
        {
            try
            {
                var response = repo.GetOfferLetterAcceptance(loanApplicationId);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/is-offer-letter-generated")]
        public HttpResponseMessage IsOfferLEtterGenerated(OfferLetterViewModel data)
        {
            try
            {
                bool response = repo.IsOfferLetterGenerated(data.documentTemplate, data.loanApplicationId, token.GetStaffId, token.GetBranchId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "success" });


            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("offer-letter/apply-template")]
        public HttpResponseMessage ApplyTemplateToOfferLetter(OfferLetterViewModel data)
        {
            try
            {
                bool response = repo.ApplyTemplateToOfferLetter(data.documentTemplate, data.loanApplicationId, token.GetStaffId, token.GetBranchId);

                //bool response = repo.EditOfferLetterTitle(data.customerId, data.offerLetterTitle, token.GetStaffId, token.GetBranchId);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Template was Attached successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
    }
}