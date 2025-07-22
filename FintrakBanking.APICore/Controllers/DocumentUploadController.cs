using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/document")] // TODO: modify!
    public class DocumentUploadController : ApiControllerBase
    {
        private IDocumentUploadRepository repo;

        private HttpClient httpClient;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public DocumentUploadController(IDocumentUploadRepository _repo)
        {
            this.repo = _repo;
            this.httpClient = new HttpClient();
            //this.httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["CoreAPIUrl"] + "api/v1/document/");
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
        [Route("document-upload/operation/{operationId}/target/{targetId}/isOperationSpecific/{isOperationSpecific}")]
        public HttpResponseMessage GetDocumentUploads(int operationId, int targetId, bool isOperationSpecific)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploads(token.GetStaffId, operationId, targetId, isOperationSpecific);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-upload1/target/{targetId}")]
        public HttpResponseMessage GetDocumentUploads1(int targetId)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploads1(token.GetStaffId, targetId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("deferred-document/{loanApplicationId}")]
        public HttpResponseMessage GetDeferredDocuments(int loanApplicationId)
        {
            IEnumerable<DeferredDocumentsViewModel> response = repo.GetDeferredDocumentsByLoandApplicationId(loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("deferred-document-all")]
        public HttpResponseMessage GetDeferredDocuments()
        {
            IEnumerable<DeferredDocumentsViewModel> response = repo.GetAllDeferredDocuments();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-upload/operation/{operationId}/target/{targetId}/isOperationSpecific/{isOperationSpecific}/isLms/{isLms}")]
        public HttpResponseMessage GetDocumentUploads(int operationId, int targetId, bool isOperationSpecific, bool isLms = false)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploadsLms(token.GetStaffId, operationId, targetId, isOperationSpecific, isLms);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-uploadlms/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetDocumentUploadsLmss([FromUri] int operationId, [FromUri] int targetId)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentUploadsLmss(token.GetStaffId, operationId, targetId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-deleted/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetDocumentDeleted(int operationId, int targetId)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetDocumentDeleted(token.GetStaffId, operationId, targetId);
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
        [Route("add-deferred-document")]
        public HttpResponseMessage AddDeferredDocument([FromBody] DeferredDocumentsViewModel model)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            var response = repo.AddDeferredDocument(model, user);
            if (!response) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Saving record failed" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record saved successfully" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-upload")]
        public async Task<HttpResponseMessage> AddDocumentUploadAsync()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                Task.Factory
                    .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning, // guarantees separate thread
                        TaskScheduler.Default)
                    .Wait();
                //MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider(); ---OLD Code
                //await Request.Content.ReadAsMultipartAsync(provider);


                if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
           
                var entity = new DocumentUploadViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.isOriginalCopy = Convert.ToBoolean(provider.FormData["isOriginalCopy"]);
                entity.documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]);
                entity.issueDate = GetCulture(provider.FormData["issueDate"]);
                entity.expiryDate = GetCulture(provider.FormData["expiryDate"]);
                entity.targetReferenceNumber = provider.FormData["targetReferenceNumber"];
                if (provider.FormData["operationId"] != null && provider.FormData["operationId"] != "undefined")
                {
                    entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
                }               
                entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
                entity.customerGroupId = Convert.ToInt32(provider.FormData["customerGroupId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.source = (int)DocUploadSourceEnum.InApp;
                entity.countryCode = provider.FormData["X-COUNTRYCODE"];
                var a = provider.FormData["targetId"];

                if (provider.FormData["targetId"] != null && provider.FormData["targetId"] != "undefined")
                {
                    entity.targetId = Convert.ToInt32(provider.FormData["targetId"]);
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                if (entity.countryCode == "NG")
                {
                    var file = provider.Contents.FirstOrDefault();
                    var buffer = await file.ReadAsByteArrayAsync();
                    int response = repo.AddDocumentUpload(entity, buffer);


                    if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                    if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });

                }
                else
                {
                    var file = provider.Contents.FirstOrDefault();
                    var buffer = await file.ReadAsByteArrayAsync();
                    var formContent = new MultipartFormDataContent
                {

                // send Image Here
                     {new ByteArrayContent(buffer),"file","file"},

                //send form text values here
                    {new StringContent(entity.fileName),"fileName"},
                    {new StringContent(entity.fileExtension),"fileExtension"},
                    {new StringContent( Convert.ToString(entity.fileSize)),"fileSize"},
                    {new StringContent( Convert.ToString(entity.documentTypeId)),"documentTypeId"},
                    {new StringContent(entity.issueDate.ToString()),"issueDate"},
                    {new StringContent(entity.expiryDate.ToString()),"expiryDate"},
                    {new StringContent(entity.targetReferenceNumber),"targetReferenceNumber"},
                    { new StringContent(entity.operationId.ToString()),"operationId"},
                    { new StringContent(entity.customerId.ToString()),"customerId"},
                    { new StringContent(entity.customerGroupId.ToString()),"customerGroupId"},
                    { new StringContent(entity.overwrite.ToString()),"overwrite"},
                    { new StringContent(entity.targetId.ToString()),"targetId"},
                };

                    if (!provider.FileStreams.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                    }
                    IEnumerable<string> headerValues;
                    if (Request.Headers.TryGetValues("Authorization", out headerValues)) ;
                    var token = headerValues.First();
                    var response = repo.AddDocumentUploadToSubsidiaryResult(entity, buffer, token, formContent);

                    if (response != null && response.result > 0)
                    {
                        if (response.result == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                        if (response.result == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
                        // return response;
                    }
                }

            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("sd-document-upload")]
        public async Task<HttpResponseMessage> AddSDDocumentUploadAsync()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                Task.Factory
                    .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning, // guarantees separate thread
                        TaskScheduler.Default)
                    .Wait();
                //MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider(); ---OLD Code
                //await Request.Content.ReadAsMultipartAsync(provider);


                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                var entity = new DocumentUploadViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.isOriginalCopy = Convert.ToBoolean(provider.FormData["isOriginalCopy"]);
                entity.documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]);
                entity.issueDate = GetCulture(provider.FormData["issueDate"]);
                entity.expiryDate = GetCulture(provider.FormData["expiryDate"]);
                entity.targetReferenceNumber = provider.FormData["targetReferenceNumber"];
                entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
                entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
                entity.customerGroupId = Convert.ToInt32(provider.FormData["customerGroupId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.source = (int)DocUploadSourceEnum.InApp;
                //entity.countryCode = provider.FormData["X-COUNTRYCODE"];
                var a = provider.FormData["targetId"];

                if (provider.FormData["targetId"] != null && provider.FormData["targetId"] != "undefined")
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
                    int response = repo. AddSDDocumentUpload(entity, buffer);


                    if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Stamp duty closed successfully. Stamp duty code " + entity.csdc});
                    if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });

                
               

            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("sd-document-upload-bulk")]
        //public async Task<HttpResponseMessage> AddSDDocumentUploadBulkAsync()
        //{
        //    try
        //    {
        //        if (!Request.Content.IsMimeMultipartContent())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
        //        }

        //        MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
        //        Task.Factory
        //            .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
        //                CancellationToken.None,
        //                TaskCreationOptions.LongRunning, // guarantees separate thread
        //                TaskScheduler.Default)
        //            .Wait();
        //        //MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider(); ---OLD Code
        //        //await Request.Content.ReadAsMultipartAsync(provider);


        //        if (!provider.FileStreams.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
        //        }

        //        var entity = new DocumentUploadViewModel();
        //        entity.fileName = provider.FormData["fileName"];
        //        entity.fileExtension = provider.FormData["fileExtension"];
        //        entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
        //        entity.isOriginalCopy = Convert.ToBoolean(provider.FormData["isOriginalCopy"]);
        //        entity.documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]);
        //        entity.issueDate = GetCulture(provider.FormData["issueDate"]);
        //        entity.expiryDate = GetCulture(provider.FormData["expiryDate"]);
        //        entity.targetReferenceNumber = provider.FormData["targetReferenceNumber"];
        //        entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
        //        entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
        //        entity.customerGroupId = Convert.ToInt32(provider.FormData["customerGroupId"]);
        //        entity.overwrite = provider.FormData["overwrite"] == "true";
        //        entity.source = (int)DocUploadSourceEnum.InApp;
        //        //entity.countryCode = provider.FormData["X-COUNTRYCODE"];
        //        var a = provider.FormData["targetId"];

        //        if (provider.FormData["targetId"] != null && provider.FormData["targetId"] != "undefined")
        //        {
        //            entity.targetId = Convert.ToInt32(provider.FormData["targetId"]);
        //        }

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.createdBy = token.GetStaffId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.isBulk = true;

        //        var file = provider.Contents.FirstOrDefault();
        //        var buffer = await file.ReadAsByteArrayAsync();
        //        int response = repo.AddSDDocumentUpload(entity, buffer);


        //        if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
        //        if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });




        //    }
        //    catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        //}

        [HttpPost]
        [ClaimsAuthorization]
        [Route("sd-document-upload-bulk")]
        public HttpResponseMessage AddSDDocumentUploadBulkAsync([FromBody] List<FacilityStampDutyViewModel> entity)
        {
            var val = new List<string>();
            try
            {
                foreach (var data in entity)
                {
                    
                    //var data = repo.GoForBulkApproval(entity, info);
                    var record = repo.GoForBulkApproval(data);
                    var result = "";

                    if (record == true)
                    {
                        result = "Record Approved";
                    }
                    else
                    {
                        result = "Record Not Approved";
                    }

                    val.Add(result);
                }

                if (val.Count() != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Stamp duty closed successfully. Stamp duty code " + entity[0].csdc });
                    // return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = val });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("document-upload")]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> AddDocumentUploadAsync()
        //{

        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
        //    }

        //    MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
        //    await Request.Content.ReadAsMultipartAsync(provider);

        //    if (!provider.FileStreams.Any())
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
        //    }
        //    //try {
        //    var entity = new DocumentUploadViewModel();
        //    entity.fileName = provider.FormData["fileName"];
        //    entity.fileExtension = provider.FormData["fileExtension"];
        //    entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
        //    entity.isOriginalCopy = Convert.ToBoolean(provider.FormData["isOriginalCopy"]);
        //    entity.documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]);
        //    entity.issueDate = GetCulture(provider.FormData["issueDate"]);
        //    entity.expiryDate = GetCulture(provider.FormData["expiryDate"]);
        //    entity.targetReferenceNumber = provider.FormData["targetReferenceNumber"];
        //    entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
        //    entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
        //    entity.customerGroupId = Convert.ToInt32(provider.FormData["customerGroupId"]);
        //    entity.overwrite = provider.FormData["overwrite"] == "true";
        //    entity.source = (int)DocUploadSourceEnum.InApp;
        //    var a = provider.FormData["targetId"];

        //    if (provider.FormData["targetId"] != null && provider.FormData["targetId"] != "undefined")
        //    {
        //        entity.targetId = Convert.ToInt32(provider.FormData["targetId"]);
        //    }

        //    entity.userBranchId = (short)token.GetBranchId;
        //    entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //    entity.applicationUrl = HttpContext.Current.Request.Path;
        //    entity.createdBy = token.GetStaffId;
        //    entity.companyId = token.GetCompanyId;

        //    var file = provider.Contents.FirstOrDefault();
        //    var buffer = await file.ReadAsByteArrayAsync();
        //    int response = repo.AddDocumentUpload(entity, buffer);


        //    if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
        //    if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
        //    //}
        //    //catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        //}



        [HttpPost]
        [ClaimsAuthorization]
        [Route("edms-document-upload")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddDocumentUploadEdmsAsync()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            Task.Factory
                .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning, // guarantees separate thread
                    TaskScheduler.Default)
                .Wait();


            var entity = new DocumentUploadViewModel();
            entity.fileName = provider.FormData["fileName"];
            entity.fileExtension = provider.FormData["fileExtension"];
            entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
            entity.isOriginalCopy = Convert.ToBoolean(provider.FormData["isOriginalCopy"]);
            entity.documentTypeId = Convert.ToInt32(provider.FormData["documentTypeId"]);
            entity.issueDate = GetCulture(provider.FormData["issueDate"]);
            entity.expiryDate = GetCulture(provider.FormData["expiryDate"]);
            entity.targetReferenceNumber = provider.FormData["targetReferenceNumber"];
            entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
            entity.customerId = Convert.ToInt32(provider.FormData["customerId"]);
            entity.customerGroupId = Convert.ToInt32(provider.FormData["customerGroupId"]);
            entity.overwrite = provider.FormData["overwrite"] == "true";
            entity.source = (int)DocUploadSourceEnum.InApp;
            entity.edmsDocumentId = Convert.ToInt32(provider.FormData["edmsDocumentId"]);
            entity.countryCode = provider.FormData["X-COUNTRYCODE"];
            // entity.targetId = Convert.ToInt32(provider.FormData["targetId"]);



            var a = provider.FormData["targetId"];

            if (provider.FormData["targetId"] != null && provider.FormData["targetId"] != "undefined")
            {
                entity.targetId = Convert.ToInt32(provider.FormData["targetId"]);
            }

            entity.userBranchId = (short)token.GetBranchId;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;



            var buffer = new byte[] { };
            int response = repo.AddDocumentUpload(entity, buffer);
            if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
            if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            //}
            //catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }



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
            bool response = repo.UpdateDocumentUpload(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("deferred-document-update/{id}")]
        public HttpResponseMessage UpdateDeferredDocument([FromBody] DeferredDocumentsViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateDeferredDocument(model, id, user);
            if (!response) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Updating record failed" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record updated successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-upload/{id}/{documentTypeId}")]
        public HttpResponseMessage DeleteDocumentUpload(int id, int documentTypeId)
        {

            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteDocumentUpload(id, documentTypeId, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-deferred-document/{id}")]
        public HttpResponseMessage DeleteDeferredDocument(int id)
        {

            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteDeferredDocument(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-recovery-document-upload/{id}")]
        public HttpResponseMessage DeleteRecoveryDocumentUpload(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRecoveryDocumentUpload(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        //[HttpPost]
        //[ClaimsAuthorization], string documentTypeName
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
        [Route("document-download1/{documentId}")]
        public HttpResponseMessage GetDocument1(int documentId)
        {
            DocumentUploadViewModel data = repo.GetDocument1(documentId);
            if (data == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-download-credit-bereau/{documentId}")]
        public HttpResponseMessage GetDocumentCreditBereau(int documentId)
        {
            DocumentUploadViewModel data = repo.GetDocumentCreditBereau(documentId);
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
            CustomerDocumentSearchViewModel response = repo.GetCustomerDocuments(model, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.documents.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-reporting-document-upload")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddRecoveryReportingDocumentUploadAsync()
        {

            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            Task.Factory
                .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning, // guarantees separate thread
                    TaskScheduler.Default)
                .Wait();

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            var entity = new RecoveryReportingDocumentViewModel();
            entity.description = provider.FormData["description"];
            entity.fileName = provider.FormData["fileName"];
            entity.fileExtension = provider.FormData["fileExtension"];
            entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
            entity.operationId = Convert.ToInt32(provider.FormData["operationId"]);
            entity.referenceId = provider.FormData["referenceId"];
            entity.overwrite = provider.FormData["overwrite"] == "true";

            var a = provider.FormData["targetId"];
            if (provider.FormData["targetId"] != null && provider.FormData["targetId"] != "undefined")
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
            int response = repo.AddRecoveryReportingDocumentUpload(entity, buffer);


            if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
            if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-reporting-documents/{referenceId}")]
        public HttpResponseMessage getAllLoanRecoveryReportingDocuments(string referenceId)
        {
            IEnumerable<RecoveryReportingDocumentViewModel> response = repo.getAllLoanRecoveryReportingDocuments(referenceId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-report-document-download/{loanRecoveryReportApprovalId}")]
        public HttpResponseMessage GetRecoveryReportDocument(int loanRecoveryReportApprovalId)
        {
            RecoveryReportingDocumentViewModel data = repo.GetRecoveryReportDocument(loanRecoveryReportApprovalId);
            if (data == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-credit-bureau-documents/{customerId}")]
        public HttpResponseMessage GetCustomerCreditBureauDocuments(int customerId)
        {
            IEnumerable<DocumentUploadViewModel> response = repo.GetCustomerCreditBureauDocuments(customerId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }
    }
}
