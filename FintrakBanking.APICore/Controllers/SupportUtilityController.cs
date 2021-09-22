using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.SupportUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/support-utility")]
    public class SupportUtilityController: ApiControllerBase
    {
        private ISupportUtilityRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public SupportUtilityController(ISupportUtilityRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-support-issue-type/{supportIssueTypeId}")]
        public HttpResponseMessage GetSupportIssueType(int supportIssueTypeId)
        {
            SupportUtilityViewModel response = repo.GetSupportIssueType(supportIssueTypeId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-support-issue-type")]
        public HttpResponseMessage GetDocumentTypes()
        {
            IEnumerable<SupportUtilityViewModel> response = repo.GetAllSupportIssueType();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-issue-by-param")]
        public HttpResponseMessage GetCustomersIssuesByParams(string searchParam, short? IssueTypeId)
        {
            IEnumerable<CustomerViewModels> response = repo.GetCustomersIssuesByParams(searchParam, IssueTypeId) ;
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-approval-trail/{searchString}")]
        public HttpResponseMessage GetApprovalTrail(string searchString)
        {
            var response = repo.GetApprovalTrail(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-unique-operations/{searchString}")]
        public HttpResponseMessage GetUniqueOperations(string searchString)
        {
            var response = repo.GetDistinctOperations(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-expected-workflow/{searchString}")]
        public HttpResponseMessage GetExpectedWorkFlow(int searchString)
        {
            var response = repo.GetExpectedWorkFlow(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-business-rule")]
        public HttpResponseMessage GetBusinessRule()
        {
            var response = repo.GetBusinessRule();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " ,  result = response });

        }


    }
}
