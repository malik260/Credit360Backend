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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("view-approval-trail")]
        public HttpResponseMessage GetApprovalTrail(WorkflowSupportUtilityViewModel model, int staffId)
        {
            var response = repo.GetApprovalTrail(model.searchString, staffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + model.searchString, result = response.Count() });

        }

    }
}
