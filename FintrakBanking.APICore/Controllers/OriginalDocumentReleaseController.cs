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
        [Route("approval-security-release")]
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
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "ERROR! One or More Document has already been sent for Approval" });
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
            try
            {  foreach(var x in model)
                {
                    x.createdBy = token.GetStaffId;
                    x.companyId = token.GetCompanyId;
                }
                var response = _repo.GoForApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
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
                model.companyId = token.GetCompanyId;

                WorkflowResponse response = _repo.SubmitApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
