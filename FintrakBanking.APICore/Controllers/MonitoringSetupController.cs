using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class MonitoringSetupController : ApiControllerBase
    {
        private IMonitoringSetupRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public MonitoringSetupController(IMonitoringSetupRepository _repo)
        {
            repo = _repo;
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("addmonitoringSetup")]
        public HttpResponseMessage AddMonitoringSetup( [FromBody]MonitoringSetupViewModel entity)
        {
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Empty Record" });
            }

            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var MonitoringSetup = repo.AddMonitoringSetup(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = MonitoringSetup, message = "The record has been created successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       
      [HttpGet] [ClaimsAuthorization]  
        [Route("monitoringSetup")]
        public HttpResponseMessage GetAllMonitoringSetup()
        {
            var Message = string.Empty;
            try
            {
                var MonitoringSetup = repo.GetAllMonitoringSetup().ToList();
                if (MonitoringSetup.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = MonitoringSetup, count = MonitoringSetup.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No MonitoringSetup found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("message-type")]
        public HttpResponseMessage GetAllMessageType ()
        {
            var Message = string.Empty;
            try
            {
                var MonitoringSetup = repo.GetAllMessageType().ToList();
                if (MonitoringSetup.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = MonitoringSetup, count = MonitoringSetup.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("product")]
        public HttpResponseMessage GetAllProduct()
        {
            var Message = string.Empty;
            try
            {
                var MonitoringSetup = repo.GetAllProduct().ToList();
                if (MonitoringSetup.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = MonitoringSetup, count = MonitoringSetup.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }




       [HttpPut] [ClaimsAuthorization]
        [Route("updatemonitoringSetup/{monitoringSetupId}")]
        public HttpResponseMessage UpdateMonitoringSetup(int MonitoringSetupId, MonitoringSetupViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var data = repo.UpdateMonitoringSetup(MonitoringSetupId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("getmonitoringSetup/{monitoringSetupId}")]
        public HttpResponseMessage GetMonitoringSetup(int monitoringSetupId )
        {
            var account = repo.GetMonitoringSetup(monitoringSetupId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.GetMonitoringSetup(monitoringSetupId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = depart });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


    }
}