using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    public class AuthorisedController : ApiControllerBase
    {

        private IAuthourisedSignatoriesRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public AuthorisedController(IAuthourisedSignatoriesRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-category")]
        public HttpResponseMessage GetSignatories()
        {
            IEnumerable<AuthourisedSignatoriesViewModel> response = repo.GetSignatories();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-category/{id}")]
        public HttpResponseMessage GetSignatory(int id)
        {
            AuthourisedSignatoriesViewModel response = repo.GetSignatoryName(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-category")]
        public HttpResponseMessage AddSignatory([FromBody] AuthourisedSignatoriesViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddSignatory(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-category/{id}")]
        public HttpResponseMessage UpdateSignatory([FromBody] AuthourisedSignatoriesViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateSignatory(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-category/{id}")]
        public HttpResponseMessage DeleteSignatory(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteSignatory(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
    }
}