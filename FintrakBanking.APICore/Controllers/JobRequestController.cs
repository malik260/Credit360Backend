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
        
        private IJobRequestRepository repo;

        public JobRequestController(IJobRequestRepository repo)
        {
            this.repo = repo;
        }
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //[HttpGet] [ClaimsAuthorization]  
        //[Route("job-request")]
        //public HttpResponseMessage GetJobRequest()
        //{
        //    var data = repo.GetAllJobRequest();
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //}       

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-detail/legal-details")]
        public HttpResponseMessage GetLegalJobRequestDetails()
        {
            var data = repo.GetLegalJobRequestDetails();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("legal-job-request-detail/{jobRequestId}")]
        public HttpResponseMessage GetJobRequestDetailsById(int jobRequestId)
        {
            var data = repo.GetJobRequestDetailsById(jobRequestId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/loan-application-details/{applicationId}")]
        public HttpResponseMessage GetLoanApplicationJobsById(int applicationId)
        {
            var data = repo.GetLoanApplicationJobsById(applicationId, token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-status-feedback/{statusId}/{jobTypeId}")]
        public HttpResponseMessage GetJobRequestStatusFeedback(short statusId, short jobTypeId)
        {
            var data = repo.GetJobRequestStatusFeedback(statusId, jobTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("reasigned-job-by-staff/{staffId}")]
        public HttpResponseMessage GetJobReasignmentStaffById(int staffId)
        {
            var data = repo.GetJobReasignmentStaffById(staffId, token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/staff")]
        public HttpResponseMessage getJobRequestByStaffId()
        {
            var data = repo.GetJobRequestByStaffId(token.GetStaffId, token.GetBranchId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-type-admin-staff")]
        public HttpResponseMessage GetJobTypeReasignmentAdminStaff()
        {
            var data = repo.GetJobTypeReasignmentAdmin(token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-type-hub-staff")]
        public HttpResponseMessage GetJobTypeHubStaff()
        {
            var data = repo.GetJobTypeHubStaff();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/{jobRequestId}")]
        public HttpResponseMessage GetJobRequest(int jobRequestId)
        {
            var data = repo.GetJobRequest(jobRequestId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("filter-job-request-by-status/{filter}")]
        public HttpResponseMessage GetJobRequestByFilter(string filter)
        {
            var data = repo.GetJobRequestByFilter(token.GetStaffId, token.GetBranchId, filter);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("application-detail-job-request/{applicationDetailId}")]
        public HttpResponseMessage GetApplicationJobRequest(int applicationDetailId)
        {
            var data = repo.GetApplicationJobRequest(applicationDetailId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-request-approval-status")]
        public HttpResponseMessage GetJobRequestApprovaStatus()
        {
            var data = repo.GetJobRequestApprovaStatus();

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/comments/{jobRequestId}")]
        public HttpResponseMessage GetJobComments(int jobRequestId)
        {
            var data = repo.GetJobComments(jobRequestId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/department")]
        public HttpResponseMessage GetJobRequestByDepartment()
        {
            var data = repo.GetJobRequestByDepartment(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request/department/unit")]
        public HttpResponseMessage GetJobRequestByDepartmentUnit()
        {
            var data = repo.GetJobRequestByDepartment(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [ClaimsAuthorization]
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Collateral Search charge instruction sent Successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Failure! Collateral Search charge Instructions failed to save " });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record. " });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("job-request/place-legal-job-charges")]
        public HttpResponseMessage PlaceChargeOnCustomerForCollateralSearch([FromBody] JobRequestCollateralSearchViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.PlaceChargeOnCustomerForCollateralSearch(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Operation Performed Successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Failure! failed to Perform Operation " });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (TwoFactorAuthenticationException fa)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{fa.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record. " });
            }
        }

        //[HttpPost] [ClaimsAuthorization]
        //[Route("job-request/collateral-customer/job/{actionName}/charge/{actionType}/{loanApplicationDetailId}")]
        //public HttpResponseMessage ChargeCustomerJob([FromBody] CollateralViewModel entity, string actionName, string actionType, int loanApplicationDetailId)
        //{ 
        //    try
        //    {
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.createdBy = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;

        //        var data = repo.ChargeCustomerJob(entity, actionName, actionType,loanApplicationDetailId);
        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        //    }
        //    catch (ConditionNotMetException ce)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
        //    }
        //    catch (BadLogicException be)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
        //    }
        //    catch (Exception)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occured."});
        //    }
        //}


         [HttpPost] [ClaimsAuthorization]
        [Route("global-job-request")]
        public HttpResponseMessage AddGlobalJobRequest([FromBody] JobRequestViewModel entity)
         {
            try
            {
                entity.companyId = (int)token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var code = repo.AddGlobalJobRequest(entity);
                if (code != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = code, message = "Request logged successfully. The Request Code is "+ code });
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
            catch (Exception ex)
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "Job response was successfully saved" });
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
            catch (SecureException ex)
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
                entity.staffId = token.GetStaffId;

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
            catch (SecureException ex)
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
            catch (SecureException ex)
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

                var data = repo.UpdateInvoiceStatus(entity);
                if (data)
                {
                    if (entity.status)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "Invoice successfully approved." });
                    }
                    else return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "Invoice successfully disapproved." });
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
            var data = repo.GetJobRequestDocuments(jobRequestCode);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("job-request-document/{documentId}")]
        public HttpResponseMessage GetJobRequestDocumentById(int documentId)
        {
            var data = repo.GetJobRequestDocumentById(documentId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
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
                //if(documentList)

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
                requestModel.jobSubTypeId = (short)Convert.ToInt32(provider.FormData["jobSubTypeId"]);
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
                var code = repo.AddJobDocument(entity, requestModel, buffer);

                if (code != string.Empty)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = code, message = "Request logged successfully. The Request Code is " + code });
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
            catch (SecureException ex)
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
                entity.statusId = (short)Convert.ToInt32(provider.FormData["statusId"]);

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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("job-document-only")]
        public async Task<HttpResponseMessage> AddJobDocumentOnly()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                //int uploadType;
                //if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                //}

                var entity = new RequestDocumentViewModel();
                entity.jobRequestCode = provider.FormData["jobRequestCode"];
                entity.documentTitle = provider.FormData["documentTitle"];
                entity.documentTypeId = 1; // (short)uploadType;
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.physicalFileNumber = provider.FormData["physicalFileNumber"];
                entity.physicalLocation = provider.FormData["physicalLocation"];
               // entity.comment = provider.FormData["responseComment"];

                if(entity.jobRequestCode =="undefined") return Request.CreateResponse(HttpStatusCode.BadRequest, "upload failed");

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
                var data = repo.AddJobDocumentOnly(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Document uploaded successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error occured while uploaded document" });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record." });
            }
        }
        #endregion Job-Documents


        #region job-type

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-type")]
        public HttpResponseMessage GetJobType()
        {
            var data = repo.GetAllJobType();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-type-hub/{jobTypeId}")]
        public HttpResponseMessage GetAllJobTypeHub(short jobTypeId)
        {
            var data = repo.GetAllJobTypeHub(jobTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        } 

        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-type-unit/{jobTypeId}")]
        public HttpResponseMessage GetAllJobTypeUnit(short jobTypeId)
        {
            var data = repo.GetAllJobTypeUnit(jobTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-hub-staff/{hubId}")]
        public HttpResponseMessage GetHubStaffByHubId(short hubId)
        {
            var data = repo.GetHubStaffByHubId(hubId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-unit-hub-staff/{unitId}")]
        public HttpResponseMessage GetHubStaffByHubTypeUnitId(short unitId)
        {
            var data = repo.GetHubStaffByHubTypeUnitId(unitId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("job-sub-type/{jobId}")]
        public HttpResponseMessage GetJobSubType(short jobId)
        {
            var data = repo.GetJobSubType(jobId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
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

        [HttpPost, Route("map-job-type-hub-staff")]
        public HttpResponseMessage AssignJobTypeToStaff([FromBody] JobTypeHubViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                //entity.dateTimeCreated = 


                var data = repo.mapJobTypeHubStaff(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The staff - hub mapping was successful" });
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record" });
            }
        }


        [HttpPost, Route("assign-job-type")]
        public HttpResponseMessage AssignJobTypeToStaff([FromBody] jobReasignment entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                //entity.dateTimeCreated = 
                

                var data = repo.AssignJobTypeToStaff(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The job type been assigned successfully" });
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record" });
            }
        }

        [HttpPut, Route("update-assigned-job-type")]
        public HttpResponseMessage UpdateAssignJobTypeToStaff([FromBody] jobReasignment entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                //entity.dateTimeCreated = 


                var data = repo.UpdateAsignedJobTypeToStaff(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The job type been assigned successfully" });
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record" });
            }
        }

        [HttpPost, Route("delete-assigned-job-type")]
        public HttpResponseMessage DeleteAssignedJobTypeToStaff([FromBody] jobReasignment entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                //entity.dateTimeCreated = 


                var data = repo.DeleteJobTypeForAStaff(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The job type been assigned successfully" });
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("reasigned-job-type")]
        public HttpResponseMessage GetJobTypeReasignment(int staffId)
        {
            var data = repo.GetJobTypeReasignmentAdmin(token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
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
            catch (SecureException e)
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
            catch (SecureException e)
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        #endregion

    }
}
