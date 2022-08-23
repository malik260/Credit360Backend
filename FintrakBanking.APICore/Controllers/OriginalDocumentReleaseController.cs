using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class OriginalDocumentReleaseController : ApiControllerBase
    {
        private IOriginalDocumentReleaseRepository _repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public OriginalDocumentReleaseController(IOriginalDocumentReleaseRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("security-release/{id}")]
        public HttpResponseMessage GetSecurityReleaseById(int id)
        {
            try
            {
                var response = _repo.GetOriginalAllDocmentRelease(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-security-release")]
        public HttpResponseMessage GetReleaseApproval()
        {
            try
            {
                var response = _repo.GetLeaseDocumentForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-cash-security-release")]
        public HttpResponseMessage GetCashReleaseApproval()
        {
            try
            {
                var response = _repo.GetCashSecurityReleaseForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-security-release-search/{searchString}")]
        public HttpResponseMessage GetSecurityReleaseSearch(string searchString)
        {
            try
            {
                var response = _repo.GetSecurityReleaseSearch(searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-cash-security-release-search/{searchString}")]
        public HttpResponseMessage GetCashSecurityReleaseSearch(string searchString)
        {
            try
            {
                var response = _repo.GetCashSecurityReleaseSearch(searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("rejected-referred-security-release")]
        public HttpResponseMessage GetRejectedAndReferredSecurityRelease()
        {
            try
            {
                var response = _repo.GetRejectedAndReferredSecurityRelease(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("rejected-referred-cash-security-release")]
        public HttpResponseMessage GetRejectedAndReferredCashSecurityRelease()
        {
            try
            {
                var response = _repo.GetRejectedAndReferredCashSecurityRelease(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("reinitiate-rejected-release/{id}")]
        public HttpResponseMessage reinitiateSecurityRelease(int id)
        {
            try
            {
                var response = _repo.reinitiateSecurityRelease(id, token.GetStaffId, token.GetCompanyId);
                if(response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Error Occurred, Please Contact the System Administrator" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("security-release")]
        public HttpResponseMessage AddSecurityRelease([FromBody]IEnumerable<OriginalDocumentReleaseViewModel> model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    foreach (var x in model)
                    {
                        x.createdBy = token.GetStaffId;
                        x.companyId = token.GetCompanyId;
                    }

                    var response = _repo.AddOriginalDocumentRelease(model);
                    if (response)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "SUCCESS" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An Error Occurred, Kindly Contact the System Administrator" });
                    }
                }

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });

            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("guarantee-release")]
        public HttpResponseMessage AddGuaranteeRelease([FromBody]IEnumerable<OriginalDocumentReleaseViewModel> model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    foreach (var x in model)
                    {
                        x.createdBy = token.GetStaffId;
                        x.companyId = token.GetCompanyId;
                    }

                    var response = _repo.AddOriginalDocumentGuaranteeRelease(model);
                    if (response)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "SUCCESS" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An Error Occurred, Kindly Contact the System Administrator" });
                    }
                }

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });

            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("security-release-go-for-approval")]
        public HttpResponseMessage GoForApproval([FromBody] IEnumerable< OriginalDocumentReleaseViewModel> model)
        {
            if(model == null) { throw new SecureException("Please select a security/collateral to release"); }
            try
            {  foreach(var x in model)
                {
                    x.createdBy = token.GetStaffId;
                    x.companyId = token.GetCompanyId;
                }
                var response = _repo.GoForApproval(model);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message  = "One or More Document may currently be Undergoing Approval"});
                }
                
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("guarantee-release-go-for-approval")]
        //public HttpResponseMessage GoForGuaranteeApproval([FromBody] IEnumerable<OriginalDocumentReleaseViewModel> model)
        //{
        //    try
        //    {
        //        foreach (var x in model)
        //        {
        //            x.createdBy = token.GetStaffId;
        //            x.companyId = token.GetCompanyId;
        //        }
        //        WorkflowResponse response = _repo.GoForGuaranteeApproval(model);
        //        if (response != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response , message = response.responseMessage });
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "One or More Document may currently be Undergoing Approval" });
        //        }

        //    }
        //    catch (SecureException ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [ClaimsAuthorization]
        [Route("guarantee-cash-release-go-for-approval")]
        public HttpResponseMessage GoForGuaranteeApproval([FromBody] CollateralCashReleaseViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                WorkflowResponse response = _repo.GoForGuaranteeCashApproval(model);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = response.responseMessage });
                }

            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("security-release-approval")]
        public HttpResponseMessage SecurityReleaseGoForApproval([FromBody] OriginalDocumentReleaseViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.staffId = token.GetStaffId;
                model.userId = token.GetUserId;
                model.companyId = token.GetCompanyId;

                WorkflowResponse response = _repo.SubmitApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("security-cash-release-approval")]
        public HttpResponseMessage CashSecurityReleaseGoForApproval([FromBody] CollateralCashReleaseViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.staffId = token.GetStaffId;
                model.userId = token.GetUserId;
                model.companyId = token.GetCompanyId;

                WorkflowResponse response = _repo.SubmitCashSecurityReleaseApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("security-release/{operationId}/document-released/{targetId}")]
        public HttpResponseMessage getReleasedDocUploadIds(int operationId, int targetId)
        {
            try
            {
                var response = _repo.GetReleasedDocUploadIds(operationId, targetId, token.GetStaffId);
                if (response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Error Occurred, Please Contact the System Administrator" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        
        [HttpGet]
        [ClaimsAuthorization]
        [Route("security-release/{operationId}/available-documents/{targetId}")]
        public HttpResponseMessage GetAvailableDocumentsForReleease(int operationId, int targetId)
        {
            try
            {
                var response = _repo.GetAvailableDocumentsForReleease(operationId, targetId, token.GetStaffId);
                if (response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Error Occurred, Please Contact the System Administrator" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
