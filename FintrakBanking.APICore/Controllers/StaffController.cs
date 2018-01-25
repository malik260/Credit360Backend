using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class StaffController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IStaffRepository repo;
        private IErrorLogRepository errorLogger;

        public StaffController(IStaffRepository _repo,
                                IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
        }

        [HttpGet]
        [Route("staff")]
        public HttpResponseMessage GetstaffInfo()
        {
            try
            {
                var staffInfo = repo.GetAllStaff().Where(x => x.companyId == token.GetCompanyId).ToList();

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = staffInfo, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals/temp")]
        public HttpResponseMessage GetStaffAwaitingApproval()
        {
            try
            {
                var staffInfo = repo.GetStaffAwaitingApprovals(token.GetStaffId, token.GetCompanyId);

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo.ToList() });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals/temp/{staffId}")]
        public HttpResponseMessage GetTempStaffDetailsById(int staffId)
        {
            try
            {
                var staffInfo = repo.GetTempStaffDetail(staffId);

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals/{staffCode}")]
        public HttpResponseMessage GetStaffDetailsById(string staffCode)
        {


            try
            {
                var staffInfo = repo.GetStaffDetail(staffCode, token.GetCompanyId);

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals")]
        public HttpResponseMessage GetStaffDetails()
        {
            try
            {
                var staffInfo = repo.GetStaffDetails(token.GetCompanyId);

                if (staffInfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo.ToList() });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "No record found" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staff/names")]
        public HttpResponseMessage GetStaff()
        {
            try
            {
                var staffInfo = repo.GetStaffNames();

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo.ToList() });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staff/unit/{departmentUnitId}")]
        public HttpResponseMessage GetStaff(short departmentUnitId)
        {
            try
            {
                var staffInfo = repo.GetStaffByUnitId(departmentUnitId);

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo.ToList() });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("approval-status")]
        public HttpResponseMessage GetApprovalStatus()
        {
            try
            {
                var staffInfo = repo.GetApprovalStatus();

                if (staffInfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo.ToList() });

                }
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

        [HttpGet]
        [Route("staff/{staffId}")]
        public HttpResponseMessage GetStaffInfoById(int staffId)
        {
            try
            {
                var staffInfo = repo.GetStaffById(staffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { error = true, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("staff")]
        public async Task<HttpResponseMessage> AddTempStaff([FromBody] StaffInfoViewModel model)
        {
            try
            {
                if (repo.IsStaffCodeAlreadyExist(model.StaffCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"A staff with {model.StaffCode} already exist" });
                }
                if (repo.IsTempStaffExist(model.StaffCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"A staff with {model.StaffCode} already exist waiting for approval" });
                }

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var staff = await repo.AddTempStaff(model);

                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "Staff has been created successfully, now waiting for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("staff/{staffid}")]
        public async Task<HttpResponseMessage> UpdateStaffInfo(int staffid, [FromBody] StaffInfoViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = (short)token.GetCompanyId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;

                var staff = await repo.UpdateStaff(staffid, model);

                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "Staff has been updated successfully, now waiting for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("staff/{staffId}")]
        public HttpResponseMessage DeletestaffInfo(int staffId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host
                };
                var staff = repo.DeleteStaff(staffId, user);
                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "staff has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staffbybranch/{branchId}")]
        public HttpResponseMessage GetstaffInfoByBranchId(int branchId)
        {
            try
            {
                var staffInfo = repo.GetAllStaff().SingleOrDefault(c => c.BranchId == branchId);

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("staff/approval")]
        public HttpResponseMessage GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Staff record has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staff/search/")]
        public HttpResponseMessage SearchStaff(string queryString)
        {
            try
            {
                var data = repo.SearchStaff(queryString, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }

        }

        [HttpGet]
        [Route("staff/{departmentId}/search/")]
        public HttpResponseMessage SearchStaffbyDepartmentId(string queryString, int departmentId)
        {
            try
            {
                var data = repo.SearchStaffbyDepartmentId(queryString, token.GetCompanyId, departmentId);
                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }

        }

        [HttpGet]
        [Route("staff/signature/all")]
        public HttpResponseMessage GetAllStaffSignatures()
        {
            try
            {
                var data = repo.GetAllStaffSignatures(token.GetCompanyId);

                var staffInfo = repo.GetAllStaff();

                foreach (var item in data)
                {
                    var staffName = staffInfo.FirstOrDefault(x => x.StaffCode == item.staffCode);

                    item.StaffName = staffName.FirstName + " " + staffName.MiddleName + " " + staffName.LastName;
                }

                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("staff/upload-signature")]
        public async Task<HttpResponseMessage> UploadStaffSignature()
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

                var entity = new StaffDocumentViewModel
                {
                    staffCode = provider.FormData["staffCode"],
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

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = repo.AddStaffSignature(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Staff signature uploaded successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error uploading staff signature" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPut]
        [Route("staff/signature/{documentId}")]
        public HttpResponseMessage UpdateStaffSignature([FromBody] StaffDocumentViewModel entity, int documentId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateStaffSignature(entity, documentId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("staff/signature")]
        public HttpResponseMessage GetStaffSignatureByStaffCode(string staffCode)
        {
            try
            {
                var data = repo.GetStaffSignatureByStaffCode(staffCode, token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("staff/update-reliever")]
        public HttpResponseMessage UpdateReliever([FromBody]RelieverViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
                bool success = repo.UpdateReliever(entity);
                string message = success == true ? "Reliever Updated Successfully." : "Reliever Update Failed.";
                return Request.CreateResponse(HttpStatusCode.OK, new { success = success, message = message });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
    }

}