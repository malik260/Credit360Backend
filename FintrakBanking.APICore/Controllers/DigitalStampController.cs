using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Repositories.Setups.Approval;
using FintrakBanking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Setups.General;
using System.Threading;
using FintrakBanking.ViewModels.Setups.Approval;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class DigitalStampController: ApiControllerBase
    {
        private IDigitalStampRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public DigitalStampController(IDigitalStampRepository _repo)
        {
            this.repo = _repo;
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-digital-stamp")]
        public async Task<HttpResponseMessage> AddDigitalStampAsync()
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
                


                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                var entity = new DigitalStampViewModel();
               
                entity.createdBy = Convert.ToInt32(provider.FormData["createdBy"]);
                entity.staffRoleId = Convert.ToInt32(provider.FormData["staffRoleId"]);
                entity.approvalLevelId = Convert.ToInt32(provider.FormData["approvalLevelId"]);
             
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                
                {
                    var file = provider.Contents.FirstOrDefault();
                    var buffer = await file.ReadAsByteArrayAsync();
                    var response = repo.AddDigitalStamp(entity, buffer);


                    if (response == true) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                    //if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Error saving record" });
                }
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }

        }


        [HttpGet]
        [Route("digital-stamp/all")]
        public HttpResponseMessage GetAllDigitalStamp()
        {
            try
            {
                var data = repo.GetAllDigitalStamp();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("digital-stamp/{id}")]
        public HttpResponseMessage GetDigitalStampByApprovalLevel(int approvalLevelId)
        {
            try
            {
                var data = repo.GetDigitalStampByApprovalLevel(approvalLevelId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-digital-stamp/{id}")]
        public HttpResponseMessage DeleteDigitalStamp(int digitalStampid)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    createdBy = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var saved = repo.DeleteDigitalStamp(digitalStampid, user);
                if (saved)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = digitalStampid, message = "Record has been deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = digitalStampid, message = "Record could not be deleted." });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }

        }
    }
}
