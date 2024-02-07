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
using FintrakBanking.ViewModels.Setups.Credit;

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
        public HttpResponseMessage AddDigitalStamp([FromBody] DigitalStampViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = Request.RequestUri.Host;

                var data = repo.AddDigitalStamp(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
        [Route("digital-stamp/{staffRoleId}")]
        public HttpResponseMessage GetDigitalStampByApprovalLevel(int staffRoleId)
        {
            try
            {
                var data = repo.GetDigitalStampByApprovalLevel(staffRoleId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-digital-stamp/{digitalStampid}")]
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
