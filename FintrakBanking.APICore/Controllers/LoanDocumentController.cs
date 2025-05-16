using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LoanDocumentController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ILoanDocumentRepository repo;

        public LoanDocumentController(ILoanDocumentRepository repo)
        {
            this.repo = repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-document/application/{applicationNumber}")]
        public HttpResponseMessage GetLoanDocumentByApplication(string applicationNumber)
        {
            try
            {
                var data = repo.GetApplicationLoanDocument(applicationNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-document-appNo-refNo/")]
        public HttpResponseMessage GetLoanDocumentByApplicationNumberRefno(string refNo, string applicationNumber)
        {
            try
            {
                var data = repo.GetLoanDocumentByAppNoRefNo(refNo, applicationNumber);
                if(data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-document/{loanDocumentId}")]
        public HttpResponseMessage GetLoanDocument(int loanDocumentId)
        {
            try
            {
                var data = repo.GetLoanDocument(loanDocumentId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route("loan-document/company")]
        //public HttpResponseMessage GetLoanDocumentByCompanyId()
        //{
        //    try
        //    {
        //        var data = repo.GetAllLoanDocumentByCompanyId(token.GetCompanyId);

        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPost] [ClaimsAuthorization]
        //[Route("loan-document")]
        //public async Task<HttpResponseMessage> AddLoanDocument1() // DEPRECATED
        //{
        //    try
        //    {
        //        if (!Request.Content.IsMimeMultipartContent())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
        //        }

        //        MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        int uploadType;
        //        if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
        //        }

        //        var entity = new LoanDocumentViewModel
        //        {
        //            loanApplicationNumber = provider.FormData["loanApplicationNumber"],
        //            loanReferenceNumber = provider.FormData["loanReferenceNumber"],
        //            documentTitle = provider.FormData["documentTitle"],
        //            documentTypeId = (short)uploadType,
        //            //SourceId = Convert.ToInt32( provider.FormData["sourceId"]),
        //            fileName = provider.FormData["fileName"],
        //            fileExtension = provider.FormData["fileExtension"],
        //            physicalFileNumber = provider.FormData["physicalFileNumber"],
        //            physicalLocation = provider.FormData["physicalLocation"],
        //            isPrimaryDocument = provider.FormData["isPrimaryDocument"] == "true",
        //        };

        //        if (!provider.FileStreams.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
        //        }

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.createdBy = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;

        //        var file = provider.Contents.FirstOrDefault();
        //        var buffer = await file.ReadAsByteArrayAsync();
        //        var data = repo.AddLoanDocument(entity, buffer);

        //        if (data == 2)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
        //        }
        //        else if (data == 3)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "There was an error creating this record because the record already exists" });
        //        }
        //        else if (data == 4)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "Record with File Number already exists" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
        //    }
        //}

        public class LoanDocumentUploadModel
        {
            public string LoanApplicationNumber { get; set; }
            public string LoanReferenceNumber { get; set; }
            public string DocumentTitle { get; set; }
            public string DocumentTypeId { get; set; }
            public string FileName { get; set; }
            public string FileExtension { get; set; }
            public string PhysicalFileNumber { get; set; }
            public string PhysicalLocation { get; set; }
            public bool IsPrimaryDocument { get; set; }
            public Microsoft.AspNetCore.Http.IFormFile File { get; set; }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-document")]
        public async Task<HttpResponseMessage> AddLoanDocument()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Content is not multipart.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            byte[] fileBytes = null;
            string fileName = null;

            var model = new LoanDocumentUploadModel();

            foreach (var content in provider.Contents)
            {
                var name = content.Headers.ContentDisposition.Name?.Trim('\"');

                if (name == "file")
                {
                    fileBytes = await content.ReadAsByteArrayAsync();
                    fileName = content.Headers.ContentDisposition.FileName?.Trim('\"');
                }
                else
                {
                    var stringValue = await content.ReadAsStringAsync();
                    switch (name)
                    {
                        case "loanApplicationNumber": model.LoanApplicationNumber = stringValue; break;
                        case "loanReferenceNumber": model.LoanReferenceNumber = stringValue; break;
                        case "documentTitle": model.DocumentTitle = stringValue; break;
                        case "documentTypeId": model.DocumentTypeId = stringValue; break;
                        case "fileName": model.FileName = stringValue; break;
                        case "fileExtension": model.FileExtension = stringValue; break;
                        case "physicalFileNumber": model.PhysicalFileNumber = stringValue; break;
                        case "physicalLocation": model.PhysicalLocation = stringValue; break;
                        case "isPrimaryDocument": model.IsPrimaryDocument = stringValue.ToLower() == "true"; break;
                    }
                }
            }

            if (fileBytes == null || fileBytes.Length == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");

            if (!int.TryParse(model.DocumentTypeId, out int uploadType))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Document Type");

            var entity = new LoanDocumentViewModel
            {
                loanApplicationNumber = model.LoanApplicationNumber,
                loanReferenceNumber = model.LoanReferenceNumber,
                documentTitle = model.DocumentTitle,
                documentTypeId = (short)uploadType,
                fileName = model.FileName ?? fileName,
                fileExtension = model.FileExtension,
                physicalFileNumber = model.PhysicalFileNumber,
                physicalLocation = model.PhysicalLocation,
                isPrimaryDocument = model.IsPrimaryDocument,
                userBranchId = (short)token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
            };

            var data = repo.AddLoanDocument(entity, fileBytes);

            switch (data)
            {
                case 2:
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                case 3:
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "Record already exists" });
                case 4:
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "File number already exists" });
                default:
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "Error creating record" });
            }
        }




        [HttpPut] [ClaimsAuthorization]
        [Route("loan-document/{loanDocumentId}")]
        public HttpResponseMessage UpdateLoanDocument([FromBody] LoanDocumentViewModel entity, int loanDocumentId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateLoanDocument(entity, loanDocumentId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-document/applicationRefNum/{referenceNumber}")]
        public HttpResponseMessage GetLoanDocumentByApplicationReferenceNum(string referenceNumber)
        {
            try
            {
                var data = repo.GetLoanDocumentByReferenceNumber(referenceNumber).ToList();
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("loan-document-delete/")]
        public HttpResponseMessage DeleteLoanDocument(string invoiceNo, string applicationNumber)
        {
            try
            {
                var data = repo.DeleteLoanDocument(invoiceNo, applicationNumber);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Record has been deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Record could not be deleted." });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }
        }


        #region CREDIT BUREAU REPORT

        [HttpGet] [ClaimsAuthorization]  
        [Route("credit-bureau-report/{customerCreditBureauId}")]
        public HttpResponseMessage GetCreditBureauReportDocument(int customerCreditBureauId)
        {
            try
            {
                var data = repo.GetCreditBureauReportDocument(customerCreditBureauId).ToList();
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("credit-bureau-report/{customerCreditBureauId}/{documentId}")]
        public HttpResponseMessage GetCreditBureauReportDocumentByDocumentID(int customerCreditBureauId, int documentId)
        {
            try
            {
                var data = repo.GetCreditBureauReportDocumentByDocumentID(customerCreditBureauId, documentId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("credit-bureau-report")]
        public async Task<HttpResponseMessage> AddCreditBureauReportDocument()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                int uploadType;
                if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                var entity = new LoanDocumentViewModel
                {
                    customerCreditBureauId = Convert.ToInt32(provider.FormData["customerCreditBureauId"]),
                    documentTitle = provider.FormData["documentTitle"],
                    documentTypeId = (short)uploadType,
                    //SourceId = Convert.ToInt32( provider.FormData["sourceId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddCreditBureauReportDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("credit-bureau-report/{documentId}")]
        public HttpResponseMessage UpdateCreditBureauReportDocument([FromBody] LoanDocumentViewModel entity, int documentId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateCreditBureauReportDocument(entity, documentId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        #endregion

        #region COMMITTEE MINUTES

         [HttpPost] [ClaimsAuthorization]
        [Route("committee-minutes")]
        public async Task<HttpResponseMessage> AddCommitteDocument()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                int uploadType;
                if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");

                }

                var entity = new LoanDocumentViewModel
                {
                    loanApplicationNumber = provider.FormData["loanApplicationNumber"],
                    loanReferenceNumber = provider.FormData["loanReferenceNumber"],
                    documentTitle = provider.FormData["documentTitle"],
                    documentTypeId = (short)uploadType,
                    //SourceId = Convert.ToInt32( provider.FormData["sourceId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                    physicalFileNumber = provider.FormData["physicalFileNumber"],
                    physicalLocation = provider.FormData["physicalLocation"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddCommitteeDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("committee-minutes/application/{applicationNumber}")]
        public HttpResponseMessage GetCommitteeDocumentByApplication(string applicationNumber)
        {
            try
            {
                var data = repo.GetCommitteeDocument(applicationNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("committee-minutes/{loanDocumentId}")]
        public HttpResponseMessage GetCommitteeDocument(int loanDocumentId)
        {
            try
            {
                var data = repo.GetCommitteeDocument(loanDocumentId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion COMMITTEE MINUTES

        #region OPERATIONDOCUMENTATION 
        /*deprecated!!*/
        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("operation-documentation")]
        //public HttpResponseMessage GetAllPendingOperationDocumentation()
        //{
        //    try
        //    {
        //        var data = repo.GetAllPendingOperationDocumentation().ToList();
        //        if (data.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "No record found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("operation-documentation/deferral/{checker}")]
        //public HttpResponseMessage GetAllPendingDeferralDocumentation(bool checker)
        //{
        //    try
        //    {
        //        var data = repo.GetAllPendingDeferralDocumentation(checker).ToList();
        //        if (data.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "No record found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("operation-documentation-checker")]
        //public HttpResponseMessage GetAllPendingOperationDocumentationApproval()
        //{
        //    try
        //    {
        //        var data = repo.GetAllPendingOperationDocumentationApproval().ToList();
        //        if (data.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "No record found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("operation-documentation")]
        //public HttpResponseMessage AddOperationDocumentationApproval(OperationDocumentationViewModel param)
        //{

        //    param.createdBy = token.GetStaffId;
        //    try
        //    {
        //        var data = repo.AddOperationDocumentationApproval(param);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Documents sent for filing Approval" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "Documents not sent" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        #endregion OPERATIONDOCUMENTATION
    }
}
