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
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class LetterGenerationRequestController : ApiControllerBase
    {
        private ILetterGenerationRequestRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LetterGenerationRequestController(ILetterGenerationRequestRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("letter-generation-request")]
        public HttpResponseMessage GetLetterGenerationRequests()
        {
            IEnumerable<LetterGenerationRequestViewModel> response = repo.GetLetterGenerationRequests(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("letter-generation-request/signatory/{requestId}")]
        public HttpResponseMessage GetLetterGenerationSignatory(int requestId)
        {
            IEnumerable<AuthorisedSignatoryViewModel> response = repo.GetLetterGenerationSignatory(requestId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("letter-generation-completed")]
        public HttpResponseMessage GetLetterGenerationCompleted()
        {
            IEnumerable<LetterGenerationRequestViewModel> response = repo.GetLetterGenerationCompleted();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("letter-generation-request-approval")]
        public HttpResponseMessage GetLetterGenerationRequestsForApproval()
        {
            IEnumerable<LetterGenerationRequestViewModel> response = repo.GetLetterGenerationRequestsForApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("letter-generation-request/{id}")]
        public HttpResponseMessage GetLetterGenerationRequest(int id)
        {
            LetterGenerationRequestViewModel response = repo.GetLetterGenerationRequest(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("letter-generation-request")]
        public HttpResponseMessage AddLetterGenerationRequest([FromBody] LetterGenerationRequestViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            try
            {
                var response = repo.AddLetterGenerationRequest(model);
                if (response.requestId > 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("letter-generation-request/{id}")]
        public HttpResponseMessage UpdateLetterGenerationRequest([FromBody] LetterGenerationRequestViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                var response = repo.UpdateLetterGenerationRequest(model, id, user);
                if (response.requestId > 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been updated successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("letter-generation-request/{id}")]
        public HttpResponseMessage DeleteLetterGenerationRequest(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteLetterGenerationRequest(id,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-camsol-loans-by-customer-code/{customerName}/customerName/{customerCode}/customerCode")]
        public HttpResponseMessage GetCamsolLoansByCustomerCode(string customerName, string customerCode)
        {
            try
            {
                var camsolLoans = repo.GetCamsolLoansByCustomerCode(customerName, customerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = camsolLoans });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the record. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("get-camsol-loan-document/{typeId}/typeId")]
        public HttpResponseMessage GetCamsolLoanDocument(int typeId, [FromBody] LetterGenerationRequestViewModel model)
        {
            try
            {
                var docHtml = repo.GetCamsolLoanDocument(typeId, model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = docHtml });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the document. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("letter-generation-search")]
        public HttpResponseMessage LetterGenerationStatusSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.Search(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("insurance-search")]
        public HttpResponseMessage insuranceSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.Search(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
