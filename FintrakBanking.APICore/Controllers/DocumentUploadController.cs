using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Media;
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
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class DocumentUploadController : ApiControllerBase
    {
        private IDocumentUploadRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public DocumentUploadController(IDocumentUploadRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-upload")]
        public HttpResponseMessage GetDocumentUploads()
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploads();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-upload/{id}")]
        public HttpResponseMessage GetDocumentUpload(int id)
        {
            DocumentUploadViewModel response = repo.GetDocumentUpload(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-upload")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddDocumentUploadAsync()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var issueDate = provider.FormData["issueDate"];
            var expiryDate = provider.FormData["expiryDate"];

            var entity = new DocumentUploadViewModel
            {
                fileName = provider.FormData["fileName"],
                fileExtension = provider.FormData["fileExtension"],
                fileSize = provider.FormData["fileSize"],
            };

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

           // if (issueDate != null) { entity.issueDate = Convert.ToDateTime(issueDate); }
           // if (expiryDate != null) { entity.expiryDate = Convert.ToDateTime(expiryDate); }
            entity.userBranchId = (short)token.GetBranchId;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;

            var file = provider.Contents.FirstOrDefault();
            var buffer = await file.ReadAsByteArrayAsync();
            var response = repo.AddDocumentUpload(entity, buffer);


            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-upload/{id}")]
        public HttpResponseMessage UpdateDocumentUpload([FromBody] DocumentUploadViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateDocumentUpload(model,id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-upload/{id}")]
        public HttpResponseMessage DeleteDocumentUpload(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteDocumentUpload(id,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }
    }
}
