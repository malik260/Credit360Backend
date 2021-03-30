using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.Media;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/document")] // TODO: modify!
    public class OriginalDocumentApprovalController : ApiControllerBase
    {
        private IOriginalDocumentApprovalRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public OriginalDocumentApprovalController(IOriginalDocumentApprovalRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-document-approval")]
        public HttpResponseMessage GetOriginalDocumentApprovals()
        {
            IEnumerable<OriginalDocumentApprovalViewModel> response = repo.GetOriginalDocumentApprovals(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-document-search/{searchString}")]
        public HttpResponseMessage GetOriginalDocumentSearch(string searchString)
        {
            IEnumerable<OriginalDocumentApprovalViewModel> response = repo.GetOriginalDocumentSearch(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-document-approval/{id}")]
        public HttpResponseMessage GetOriginalDocumentApproval(int id)
        {
            OriginalDocumentApprovalViewModel response = repo.GetOriginalDocumentApproval(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-document-by-id/{id}")]
        public HttpResponseMessage GetOriginalDocumentByCollateralCustomerId(int id)
        {
            var response = repo.GetOriginalDocumentByCollateralCustomerId(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-original-document-by-id/{id}")]
        public HttpResponseMessage GetApprovedOriginalDocumentByCollateralCustomerId(int id)
        {
            var response = repo.GetApprovedOriginalDocumentByCollateralCustomerId(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-document-status-by-id/{id}")]
        public HttpResponseMessage GetReleaseDocumentByCollateralCustomerId(int id)
        {
            var response = repo.GetReleaseDocumentByCollateralCustomerId(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-facility-document-status-by-id/{id}")]
        public HttpResponseMessage GetReleaseDocumentByCustomerFacilityId(int id)
        {
            var response = repo.GetReleaseDocumentByCustomerFacilityId(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("original-document/{id}")]
        public HttpResponseMessage GetOriginalDocument(int id)
        {
            var response = repo.GetOriginalDocument(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-upload-list")]
        public HttpResponseMessage GetDocumentUploadList()
        {
            var response = repo.GetDocumentUploadList(token.GetStaffId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("original-document-approval")]
        public HttpResponseMessage AddOriginalDocumentApproval([FromBody] OriginalDocumentApprovalViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddOriginalDocumentApproval(model);
                if (response > 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("original-document-approval/{id}")]
        public HttpResponseMessage UpdateOriginalDocumentApproval([FromBody] OriginalDocumentApprovalViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateOriginalDocumentApproval(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("original-document-approval/{id}")]
        public HttpResponseMessage DeleteOriginalDocumentApproval(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteOriginalDocumentApproval(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loanapplication-search/{parameter}")]
        public HttpResponseMessage Search(string parameter)
        {
            var response = repo.Search(parameter);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("original-document/approval")]
        public HttpResponseMessage GoForApproval([FromBody] OriginalDocumentApprovalViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var approvalStatusId = model.approvalStatusId;

                var response = repo.GoForApproval(model, (short)approvalStatusId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, count = 1 });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("original-document/submit-approval")]
        public HttpResponseMessage SubmitApproval([FromBody] OriginalDocumentApprovalViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                WorkflowResponse response = repo.SubmitApproval(model);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, count = 1 });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-original-document/{parameter}")]
        public HttpResponseMessage SearchForApprovedOriginalDocument(string parameter)
        {
            var response = repo.SearchForApprovedOriginalDocument(parameter);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
    }
}
