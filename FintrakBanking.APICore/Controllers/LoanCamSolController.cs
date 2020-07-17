using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using System.Threading.Tasks;
using System.Web;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/camsol")]
    public class LoanCamSolController : ApiControllerBase
    {
        private ILaonCamSolRepository repo;
        private IErrorLogRepository errorLogger;

        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanCamSolController(ILaonCamSolRepository _repo, IErrorLogRepository _errorLogger)
        {
            repo = _repo;
            this.errorLogger = _errorLogger;

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol")]
        public HttpResponseMessage GetAllCamsol()
        {
            try
            {
                var data = repo.GetCamSol();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-search/{searchValue}")]
        public HttpResponseMessage GetLoanCamsolSearch(string searchValue)
        {
            try
            {
                var data = repo.GetCamSol(searchValue);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-approval")]
        public HttpResponseMessage GetLoanCamsolAwaitingApproval()
        {
            try
            {
                var staffId = token.GetStaffId;
                var companyId = token.GetCompanyId;

                var data = repo.CamSolAwaitingApproval(companyId, staffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("bulk-approval")]
        public HttpResponseMessage GoForBulkApproval([FromBody]List<LoanCAMSOLViewModel> entity)
        {
            var val = new List<string>();
            try
            {
                foreach(var data in entity)
                {
                    data.createdBy = token.GetStaffId;
                    data.BranchId = (short?)token.GetBranchId;
                    data.companyId = token.GetCompanyId;
                    data.staffId = token.GetStaffId;
                    data.applicationUrl = HttpContext.Current.Request.Path;
                    data.userIPAddress = Request.RequestUri.Host;
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
                    new { success = true, message = "Camsol record has been approved successfully" });
                   // return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = val });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("go-for-camsol-approval")]
        public HttpResponseMessage goForApproval([FromBody] LoanCAMSOLViewModel data)
        {
            try
            {
                data.companyId = token.GetCompanyId;
                data.createdBy = (short)token.GetStaffId;
                var val = repo.goForApproval(data);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = val });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = $"Error: {ex.Message}" });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-type-id/{id}")]
        public HttpResponseMessage GetLoanCamsolById(int id)
        {
            try
            {
                var data = repo.GetCamSolByType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-customer-code/{customercode}")]
        public HttpResponseMessage GetLoanCamsolByCustomerCode(string  customercode)
        {
            try
            {
                var data = repo.GetCamSolByCustomerCode(customercode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-loan-camsol-type/{id}")]
        public HttpResponseMessage ViewLoanCamsolById(int id)
        {
            try
            {
                var data = repo.ViewCamSolByType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("camsol-approval-type/{id}")]
        public HttpResponseMessage GetCamsolAwaitingApprovalById(int id)
        {
            try
            {
                var data = repo.CamSolAwaitingApprovalById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
 

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-type")]
        public HttpResponseMessage GetLoanCansolType()
        {
            try
            {
                var data = repo.GetCamSolType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("approve-loan-camsol")]
        public HttpResponseMessage ApproveCamsol([FromBody] LoanCAMSOLViewModel updateOptions)
        {
            try
            {
                var data = repo.ApproveCamsol(updateOptions);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Camsol override was not successful" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("multiple-camsol-data")]
        public async Task<HttpResponseMessage> UploadCamsolData()
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
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "File Type is invalid.");
                //}

                var entity = new CamsolDocumentViewModel
                {
                    customerCode = provider.FormData["customerCode"],
                    documentTitle = provider.FormData["documentTitle"],
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
                entity.branchId = (short)token.GetBranchId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.UploadCamsolData(entity, buffer);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Camsol document uploaded successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error uploading staff data" });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record. " + ex.Message });
            }
        }


    }
}
