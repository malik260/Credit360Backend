using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.ErrorLogger; 
using System.Collections.Generic; 
using FintrakBanking.ViewModels.CASA;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class OverrideController : ApiControllerBase
    {
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IErrorLogRepository errorLogger;
        private IOverRideRepository _override;
        public OverrideController(IErrorLogRepository _errorLogger, IOverRideRepository _override)
        {
            errorLogger = _errorLogger;
            this._override = _override;

        }

         [HttpPost] [ClaimsAuthorization]
        [Route("add-override-request")]
        public HttpResponseMessage AddOverRideRequest([FromBody] IEnumerable<OverrideDetailVeiwModel> entity)
        {
              
            try
            {
                var responseMessage = string.Empty;

                List<OverrideDetailVeiwModel> model = entity.Select(c => new OverrideDetailVeiwModel
                {
                    approvedStatusId = c.approvedStatusId,
                    createdBy = token.GetStaffId,
                    customerCode = c.customerCode,
                    reason = c.reason,
                    overrideItemId = c.overrideItemId,
                    sourceReferenceNumber = c.sourceReferenceNumber,

                    userBranchId = (short)token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,                    
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host,

            }).ToList();

                var response = _override.AddOverRideRequest(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1, message = "Override Request was successful and undergoing approval." });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("approve-override-request")]
        public HttpResponseMessage ApproveOverRideRequest(ApproveOverrideVeiwModel entity)
        {            
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
               

                var response = _override.ApproveOverride(entity);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Override Approved successfully", result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("delete-override-request")]
        public HttpResponseMessage DeleteOverRideRequest(OverrideDetailVeiwModel entity)
        {
           try
            {
                //UserInfo user = new UserInfo()
                //{
                //    BranchId = token.GetBranchId,
                //    companyId = token.GetCompanyId,
                //    staffId = token.GetStaffId,
                //    applicationUrl = HttpContext.Current.Request.Path,
                //};
                var data = _override.DeleteOverRideRequest(entity); 
                return Request.CreateResponse(HttpStatusCode.OK, Ok(data));
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-all-override-items")]
        public HttpResponseMessage GetAllOverRideItems()
        {
            try
            {
                var response = _override.GetAllOverRideItems();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-all-override-request")]
        public HttpResponseMessage GetAllOverRideRequest()
        { 
            try
            {
                var response = _override.GetAllOverRideRequest();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-override-request/request/{id}")]
        public HttpResponseMessage GetOverRideRequestById(int id)
        {
           
            try
            {
                var response = _override.GetOverRideRequestById(id); 
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-override-request/overrideitems/{id}")]
        public HttpResponseMessage GetOverRideRequestByOverRideItemsId(int id)
        {
            try
            {
                var response = _override.GetOverRideRequestByOverRideItemsId(id);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("override-awaiting-approval")]
        public HttpResponseMessage GetOverrideAwaitingApproval()
        {
            try
            {
                var response = _override.GetOverrideAwaitingApproval(token.GetStaffId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("get-override-request/referencenumber/{refno}")]
        public HttpResponseMessage GetOverRideRequestByReferenceNumber(string refNo)
        {
           try
            {
                var response = _override.GetOverRideRequestByReferenceNumber(refNo);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("update-override-request")]
        public HttpResponseMessage UpdateOverRideRequest(OverrideDetailVeiwModel entity)
        {
            //return _override.UpdateOverRideRequest(entity);
            try
            {
                //UserInfo user = new UserInfo
                //{
                //    BranchId = token.GetBranchId,
                //    companyId = token.GetCompanyId,
                //    staffId = token.GetStaffId,
                //    createdBy = token.GetStaffId,
                //    applicationUrl = HttpContext.Current.Request.Path,
                //    userIPAddress = Request.RequestUri.Host
                //};

                var response = _override.UpdateOverRideRequest(entity); ;

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "update was successful", result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
    }
}