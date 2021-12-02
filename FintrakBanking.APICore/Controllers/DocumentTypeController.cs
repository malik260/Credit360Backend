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
using FintrakBanking.Interfaces.Media;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class DocumentTypeController : ApiControllerBase
    {
        private IDocumentTypeRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public DocumentTypeController(IDocumentTypeRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-type")]
        public HttpResponseMessage GetDocumentTypes()
        {
            IEnumerable<DocumentTypeViewModel> response = repo.GetDocumentTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-type/{id}")]
        public HttpResponseMessage GetDocumentType(int id)
        {
            DocumentTypeViewModel response = repo.GetDocumentType(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-type")]
        public HttpResponseMessage AddDocumentType([FromBody] DocumentTypeViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddDocumentType(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-type/{id}")]
        public HttpResponseMessage UpdateDocumentType([FromBody] DocumentTypeViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateDocumentType(model,id,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-type/{id}")]
        public HttpResponseMessage DeleteDocumentType(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteDocumentType(id,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
    }
}
