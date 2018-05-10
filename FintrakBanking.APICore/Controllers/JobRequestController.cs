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

        [HttpGet]
        [Route("job-request")]
        public HttpResponseMessage GetJobRequest()
        {
            try
            {
                var data = repo.GetAllJobRequest();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-request-detail/legal")]
        public HttpResponseMessage GetJobRequestLegalJobDetails()
        {
            try
            {
                var data = repo.GetJobRequestLegalJobDetails();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-request/loan-application-details/{applicationId}")]
        public HttpResponseMessage GetLoanApplicationJobsById(int applicationId)
        {
            try
            {
                var data = repo.GetLoanApplicationJobsById(applicationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-request-status-feedback/{statusId}/{jobTypeId}")]
        public HttpResponseMessage GetJobRequestStatusFeedback(short statusId, short jobTypeId)
        {
            try
            {
                var data = repo.GetJobRequestStatusFeedback(statusId, jobTypeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-request/staff")]
        public HttpResponseMessage getJobRequestByStaffId()
        {
            try
            {
                var data = repo.GetJobRequestByStaffId(token.GetStaffId, token.GetBranchId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-request/department")]
        public HttpResponseMessage GetJobRequestByDepartment()
        {
            try
            {

                var data = repo.GetJobRequestByDepartment(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [Route("job-request/department/unit")]
        public HttpResponseMessage GetJobRequestByDepartmentUnit()
        {
            try
            {

                var data = repo.GetJobRequestByDepartment(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record. " + ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }


        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error logging this request" , error = ex.Message });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error commenting on this request", error = ex.Message });
            }
        }


        [HttpPut]
        [Route("job-request/reply/{jobRequestId}")]
        public HttpResponseMessage ReplyJobRequest([FromBody] JobRequestViewModel entity, int jobRequestId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.ReplyJobRequest(entity, jobRequestId);
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        #region Job-Documents

        [HttpGet]
        [Route("job-request-documents/{jobRequestCode}")]
        public HttpResponseMessage GetJobRequestDocuments(string jobRequestCode)
        {
            try
            {
                var data = repo.GetJobRequestDocuments(jobRequestCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-request-document/{documentId}")]
        public HttpResponseMessage GetJobRequestDocumentById(int documentId)
        {
            try
            {
                var data = repo.GetJobRequestDocumentById(documentId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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

                var requestModel = new JobRequestViewModel
                {
                    departmentId = (short)Convert.ToInt32(provider.FormData["departmentId"]),
                    departmentUnitId = (short)Convert.ToInt32(provider.FormData["departmentUnitId"]),
                    requestTitle = provider.FormData["requestSubject"],
                    senderComment = provider.FormData["senderComment"],
                    isReassigned = Convert.ToBoolean(provider.FormData["isReassigned"]),
                    isAcknowledged = Convert.ToBoolean(provider.FormData["isAcknowledged"]),
                    targetId = Convert.ToInt32(provider.FormData["targetId"]),
                    operationsId = Convert.ToInt32(provider.FormData["operationId"]),
                    jobTypeId = (short)Convert.ToInt32(provider.FormData["jobTypeId"])
                };




                if (!(receiverStaffId == null || receiverStaffId == string.Empty || receiverStaffId == ""))
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPost]
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
                    comment = provider.FormData["comment"],
                };

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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }
        #endregion Job-Documents


        #region job-type

        [HttpGet]
        [Route("job-type")]
        public HttpResponseMessage GetJobType()
        {
            try
            {
                var data = repo.GetAllJobType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("job-sub-type/{jobId}")]
        public HttpResponseMessage GetJobSubType(short jobId)
        {
            try
            {
                var data = repo.GetJobSubType(jobId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost][Route("job-type")]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPut][Route("job-type/{jobTypeId}")]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        #endregion job-type

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
