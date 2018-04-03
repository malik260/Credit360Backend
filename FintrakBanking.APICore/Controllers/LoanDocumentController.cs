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

        [HttpGet]
        [Route("loan-document/application/{applicationNumber}")]
        public HttpResponseMessage GetLoanDocumentByApplication(string applicationNumber)
        {
            try
            {
                var data = repo.GetApplicationLoanDocument(applicationNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
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
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [Route("loan-document")]
        public async Task<HttpResponseMessage> AddLoanDocument()
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
                var data = repo.AddLoanDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPut]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }
        }

        #region CREDIT BUREAU REPORT

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPut]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        #endregion

        #region COMMITTEE MINUTES

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpGet]
        [Route("committee-minutes/application/{applicationNumber}")]
        public HttpResponseMessage GetCommitteeDocumentByApplication(string applicationNumber)
        {
            try
            {
                var data = repo.GetCommitteeDocument(applicationNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion COMMITTEE MINUTES

    }
}
