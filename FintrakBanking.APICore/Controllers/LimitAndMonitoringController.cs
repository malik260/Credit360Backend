using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class LimitAndMonitoringController : ApiControllerBase
    {
        private readonly ILimitAndMonitoringRepository repo;
        private readonly TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LimitAndMonitoringController(ILimitAndMonitoringRepository _repo)
        {
            this.repo = _repo;
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("monitoring-alert-messages")]
        public HttpResponseMessage GetAllEmailAlertMessages()
        {
            try
            {

                var response = repo.GetAllEmailAlertMessages();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("limit-and-monitoring")]
        public HttpResponseMessage GetAllSetEmailAlertMessages()
        {
            try
            {

                var response = repo.GetAllSetEmailAlertMessages();
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("limit-and-monitoring/{alertId}")]
        public HttpResponseMessage GetAllSetEmailAlertMessages(int alertId)
        {
            try
            {

                var response = repo.GetAllSetEmailAlertMessages(alertId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("add-limit-and-monitoring")]
        public HttpResponseMessage AddLimit([FromBody] LimitAndMonitoringViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddNewEmailMessageSeeting(model);
                if (response!=null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("update-email-lert-message")]
        public HttpResponseMessage UpdateEmailMessageSeeting( [FromBody] LimitAndMonitoringViewModel model)
        {
            try
            {
                ;
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateEmailAlertMessages( model);
                if (response!=null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("delete-limit-and-monitoring")]
        public HttpResponseMessage RemoveEmailMessageSeeting([FromBody] LimitAndMonitoringViewModel LimitId)
        {
            try
            {


                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                repo.RemoveEmailMessageSeeting(LimitId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LimitId, message = "record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
