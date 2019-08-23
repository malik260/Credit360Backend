using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
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
    [RoutePrefix("api/v1/document")] // TODO: modify!
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
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploads(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-upload/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetDocumentUploads(int operationId, int targetId)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploads(token.GetStaffId, operationId, targetId);
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
        [Route("document-uploads")]
        public HttpResponseMessage GetDocumentUpload([FromBody] IEnumerable<DocumentUploadViewModel> model)
        {
            var response = repo.GetDocumentUpload(model);
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

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try {
                var entity = new DocumentUploadViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]);
                entity.issueDate = GetCulture(provider.FormData["issueDate"]);
                entity.expiryDate = GetCulture(provider.FormData["expiryDate"]);
                entity.targetReferenceNumber = provider.FormData["targetReferenceNumber"];
                entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
                entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
            
                var a = provider.FormData["targetId"];

                if (provider.FormData["targetId"] != null && provider.FormData["targetId"]!= "undefined")
                {
                    entity.targetId = Convert.ToInt32(provider.FormData["targetId"]);
                }

                    entity.userBranchId = (short)token.GetBranchId;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;

            var file = provider.Contents.FirstOrDefault();
            var buffer = await file.ReadAsByteArrayAsync();
            int response = repo.AddDocumentUpload(entity, buffer);


            if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
            if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        private DateTime? GetCulture(string dt)
        {
            if (String.IsNullOrEmpty(dt)) return null;
            var actualDate = dt.Substring(0, 15);
            return DateTime.ParseExact(actualDate, "ddd MMM dd yyyy", System.Globalization.CultureInfo.InvariantCulture);
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


        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("document-download")]
        //public HttpResponseMessage GetUploadedDocument(DocumentUploadViewModel model)
        //{
        //    DocumentUploadViewModel response = repo.GetUploadedDocument(model);
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //}

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-download/{documentId}")]
        public HttpResponseMessage GetDocument(int documentId)
        {
            DocumentUploadViewModel data = repo.GetDocument(documentId);
            if (data == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }







        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-type/category/{id}")]
        public HttpResponseMessage GetDocumentTypes(int id)
        {
            IEnumerable<DocumentTypeViewModel> response = repo.GetDocumentTypes(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-category")]
        public HttpResponseMessage GetDocumentCategories()
        {
            IEnumerable<DocumentCategoryViewModel> response = repo.GetDocumentCategories();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer-document")]
        public HttpResponseMessage GetCustomerDocuments([FromBody] DocumentUploadViewModel model)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            CustomerDocumentSearchViewModel response = repo.GetCustomerDocuments(model,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.documents.Count() });
        }
    }
}
