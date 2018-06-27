using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.ViewModels.Credit;
using System.Threading.Tasks;
using System.Linq;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/workflow")]
    public class JobRequestController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IJobRequestRepository repo;

        public JobRequestController(IJobRequestRepository repo)
        {
            this.repo = repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request")]
        public HttpResponseMessage GetJobRequest()
        {
            try
            {
                var data = repo.GetAllJobRequest();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-detail/legal")]
        public HttpResponseMessage GetJobRequestLegalJobDetails()
        {
            try
            {
                var data = repo.GetJobRequestLegalJobDetails();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/loan-application-details/{applicationId}")]
        public HttpResponseMessage GetLoanApplicationJobsById(int applicationId)
        {
            try
            {
                var data = repo.GetLoanApplicationJobsById(applicationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-status-feedback/{statusId}/{jobTypeId}")]
        public HttpResponseMessage GetJobRequestStatusFeedback(short statusId, short jobTypeId)
        {
            try
            {
                var data = repo.GetJobRequestStatusFeedback(statusId, jobTypeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/staff")]
        public HttpResponseMessage getJobRequestByStaffId()
        {
            try
            {
                var data = repo.GetJobRequestByStaffId(token.GetStaffId, token.GetBranchId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/{jobRequestId}")]
        public HttpResponseMessage GetJobRequest(int jobRequestId)
        {
            try
            {
                var data = repo.GetJobRequest(jobRequestId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception )
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("application-detail-job-request/{applicationDetailId}")]
        public HttpResponseMessage GetApplicationJobRequest(int applicationDetailId)
        {
            try
            {
                var data = repo.GetApplicationJobRequest(applicationDetailId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An eror occured" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/comments/{jobRequestId}")]
        public HttpResponseMessage GetJobComments(int jobRequestId)
        {
            try
            {
                var data = repo.GetJobComments(jobRequestId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/department")]
        public HttpResponseMessage GetJobRequestByDepartment()
        {
            try
            {

                var data = repo.GetJobRequestByDepartment(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/department/unit")]
        public HttpResponseMessage GetJobRequestByDepartmentUnit()
        {
            try
            {

                var data = repo.GetJobRequestByDepartment(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message ="An error occured."});
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("job-request/legal-collateral-job")]
        public HttpResponseMessage EffectLegaCollateralJobs([FromBody] JobRequestCollateralSearchViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.EffectLegaCollateralJobs(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Collateral Search Instructions Saved Successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Failure! Collateral Search Instructions failed to save " });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record. " });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("job-request/collateral-customer/job/{actionName}/charge/{actionType}/{loanApplicationDetailId}")]
        public HttpResponseMessage ChargeCustomerJob([FromBody] CollateralViewModel entity, string actionName, string actionType, int loanApplicationDetailId)
        { 
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.ChargeCustomerJob(entity, actionName, actionType,loanApplicationDetailId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured."});
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("global-job-request")]
        public HttpResponseMessage AddGlobalJobRequest([FromBody] JobRequestViewModel entity)
         {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddGlobalJobRequest(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The request logged successfully." });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error logging this request" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error logging this request" });
            }
        }

        [HttpPost] [ClaimsAuthorization]
        [Route("job-request/comment")]
        public HttpResponseMessage AddJobComment([FromBody] JobRequestMessageViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddJobComment(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Comment added successfully." });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error commenting on this job" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error commenting on this request" });
            }
        }


        [HttpPut] [ClaimsAuthorization]
        [Route("job-request/reply/{jobRequestId}")]
        public HttpResponseMessage ReplyJobRequest([FromBody] JobRequestViewModel entity, int jobRequestId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.ReplyJobRequest(entity, jobRequestId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record." });
            }
        }

        [HttpPut, Route("job-request/reassign/{jobRequestId}")]
        public HttpResponseMessage ReassignJobRequest([FromBody] JobRequestViewModel entity, int jobRequestId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.ReassignJobRequest(entity, jobRequestId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The job been assigned successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record" });
            }
        }

        public HttpResponseMessage AcknowledgeJob([FromBody] JobRequestViewModel entity, int jobRequestId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AcknowledgeJob(entity, jobRequestId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "Job Acknowledged." });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (BadLogicException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record" });
            }
        }
        #region Middle Office Request
        [HttpPut, Route("job-request/invoice-status")]
        public HttpResponseMessage UpdateInvoiceStatus([FromBody] JobRequestInvoiceViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = true;// repo.UpdateInvoiceStatus(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record." });
            }
        }
        #endregion Middle Office Request

        #region Job-Documents

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-documents/{jobRequestCode}")]
        public HttpResponseMessage GetJobRequestDocuments(string jobRequestCode)
        {
            try
            {
                var data = repo.GetJobRequestDocuments(jobRequestCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-document/{documentId}")]
        public HttpResponseMessage GetJobRequestDocumentById(int documentId)
        {
            try
            {
                var data = repo.GetJobRequestDocumentById(documentId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("job-document")]
        public async Task<HttpResponseMessage> AddJobDocument()
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

                var entity = new RequestDocumentViewModel
                {
                    //targetId = Convert.ToInt32(provider.FormData["targetId"]),
                    //targetReferenceNumber = provider.FormData["targetReferenceNumber"],
                    jobRequestCode = provider.FormData["jobRequestCode"],
                    documentTitle = provider.FormData["documentTitle"],
                    documentTypeId = (short)uploadType,
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                    physicalFileNumber = provider.FormData["physicalFileNumber"],
                    physicalLocation = provider.FormData["physicalLocation"],
                };

                var receiverStaffId = provider.FormData["receiverStaffId"];

                var requestModel = new JobRequestViewModel();
                requestModel.departmentId = (short)Convert.ToInt32(provider.FormData["departmentId"]);
                requestModel.departmentUnitId = (short)Convert.ToInt32(provider.FormData["departmentUnitId"]);
                requestModel.requestTitle = provider.FormData["requestSubject"];
                requestModel.senderComment = provider.FormData["senderComment"];
                requestModel.targetId = Convert.ToInt32(provider.FormData["targetId"]);
                requestModel.operationsId = Convert.ToInt32(provider.FormData["operationId"]);
                requestModel.jobTypeId = (short)Convert.ToInt32(provider.FormData["jobTypeId"]);
                requestModel.isReassigned = provider.FormData["isReassigned"].ToLower() != "undefined" ? Convert.ToBoolean(provider.FormData["isReassigned"]) : false;
                requestModel.isAcknowledged = provider.FormData["isAcknowledged"] != "undefined" ?Convert.ToBoolean(provider.FormData["isAcknowledged"]) : false;

                if (receiverStaffId != null && receiverStaffId != string.Empty && receiverStaffId != "" && receiverStaffId != "undefined" && receiverStaffId != "null")
                    requestModel.receiverStaffId = Convert.ToInt32(receiverStaffId);

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                requestModel.userBranchId = (short)token.GetBranchId;
                requestModel.companyId = token.GetCompanyId;
                requestModel.createdBy = token.GetStaffId;
                requestModel.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddJobDocument(entity, requestModel, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record." + ex.Message +". "+ex.InnerException });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("job-reply-and-job-document")]
        public async Task<HttpResponseMessage> AddJobReplyAndDocument()
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

                var entity = new RequestDocumentViewModel();
                entity.jobRequestCode = provider.FormData["jobRequestCode"];
                entity.documentTitle = provider.FormData["documentTitle"];
                entity.documentTypeId = (short)uploadType;
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.physicalFileNumber = provider.FormData["physicalFileNumber"];
                entity.physicalLocation = provider.FormData["physicalLocation"];
                entity.comment = provider.FormData["responseComment"];

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddJobReplyAndDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record." });
            }
        }
        #endregion Job-Documents


        #region job-type

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-type")]
        public HttpResponseMessage GetJobType()
        {
            try
            {
                var data = repo.GetAllJobType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-sub-type/{jobId}")]
        public HttpResponseMessage GetJobSubType(short jobId)
        {
            try
            {
                var data = repo.GetJobSubType(jobId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured." });
            }
        }

         [HttpPost] [ClaimsAuthorization][Route("job-type")]
        public HttpResponseMessage AddJobType([FromBody] JobTypeViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddJobType(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record." });
            }
        }

       [HttpPut] [ClaimsAuthorization][Route("job-type/{jobTypeId}")]
        public HttpResponseMessage UpdateJobType([FromBody] JobTypeViewModel entity, short jobTypeId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateJobType(entity, jobTypeId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record." });
            }
        }

        #endregion job-type


        #region Job Request Feedback
        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-request-status")]
        public HttpResponseMessage GetJobRequestStatus()
        {
            try
            {
                var data = repo.GetJobRequestStatus();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-request-feedback")]
        public HttpResponseMessage GetAllJobRequestStatusFeedback()
        {
            try
            {
                var data = repo.GetAllJobRequestStatusFeedback();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("job-request-feedback")]
        public HttpResponseMessage AddUpdateCompanyDirector([FromBody]JobRequestStatusFeedbackViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.jobStatusFeedbackId != 0 || entity.jobStatusFeedbackId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateJobRequestFeedBack(entity.jobStatusFeedbackName))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = false, message = $"Job request feedback {entity.jobStatusFeedbackName} already exist." });
                    }
              
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateJobRequestFeedBack(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        #endregion

        //[HttpGet]
        //[Route("operation-staff/{operationId}")]
        //public HttpResponseMessage GetOperationStaff(int operationId)
        //{
        //    try
        //    {
        //        var data = repo.GetOperationStaff(operationId);

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

        //[HttpGet]
        //[Route("job-request/group")]
        //public HttpResponseMessage GetJobRequestByGroupId()
        //{
        //    try
        //    {
        //        var data = repo.GetJobRequestByGroupId(token.GetStaffId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}

    }
}
