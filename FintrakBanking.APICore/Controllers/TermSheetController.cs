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

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class TermSheetController : ApiControllerBase
    {
        private ITermSheetRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public TermSheetController(ITermSheetRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("term-sheet")]
        public HttpResponseMessage GetTermSheets()
        {
            
            IEnumerable<TermSheetViewModel> response = repo.GetTermSheets(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("term-sheets/{customerId}")]
        public HttpResponseMessage GetCustomerIdTermSheet(int customerId)
        {
            var response = repo.GetCustomerTermSheets(customerId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("term-sheets")]
        public HttpResponseMessage GetCustomerIdTermSheetCorrection()
        {
            var response = repo.GetCustomerTermSheetsCorrection();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("term-sheets-by-code/{termSheetCode}")]
        public HttpResponseMessage GetCustomerIdTermSheetCorrection(int termSheetCode)
        {
            var response = repo.GetCustomerTermSheetsByCode(termSheetCode);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("term-sheet/{id}")]
        public HttpResponseMessage GetTermSheet(int id)
        {
            TermSheetViewModel response = repo.GetTermSheet(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("term-sheet")]
        public HttpResponseMessage AddTermSheet([FromBody] TermSheetViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddTermSheet(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("term-sheet/{id}")]
        public HttpResponseMessage UpdateTermSheet([FromBody] TermSheetViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateTermSheet(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("term-sheet/{id}")]
        public HttpResponseMessage DeleteTermSheet(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteTermSheet(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
    }
}
