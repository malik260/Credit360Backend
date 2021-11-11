using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.APICore.Filters;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class ApprovalReliefController : ApiControllerBase
    {

        private IApprovalReliefRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ApprovalReliefController(IApprovalReliefRepository _repo)
        {
            this.repo = _repo;
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("approval-relief")]
        public async Task<HttpResponseMessage> AddApprovalRelief([FromBody] ApprovalReliefViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.AddApprovalRelief(model);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, message = "The record has been created successfully, and Sent For Approval.." });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-relief")]
        public HttpResponseMessage GetAllApprovalRelief(HttpRequestMessage request)
        {
            try
            {
                var data = repo.GetAllApprovalRelief(token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }

        }

       [HttpPut] [ClaimsAuthorization]
        [Route("approval-relief/{reliefId}")]
        public async Task<HttpResponseMessage> UpdateApprovalRelief(int ReliefId, [FromBody] ApprovalReliefViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.reliefId = ReliefId;
                var data = await repo.UpdateApprovalRelief(ReliefId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-relief-awaiting-approval")]
        public HttpResponseMessage GetApprovalReliefAwaitingApproval()
        {
            try
            {
                var data = repo.GetApprovalReliefAwaitingApprovals(token.GetStaffId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("approval-relief-approval")]
        public HttpResponseMessage GoForApproval([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Staff Role has been approved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("staff-relief/staffId/{staffId}")]
        public HttpResponseMessage GetAllStaffRelief(int staffId)
        {
            try
            {
                var data = repo.GetAllStaffRelief(token.GetCompanyId, staffId);

                var rec = data.ToList();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("staff-relief")]
        public HttpResponseMessage AddStaffRelief([FromBody] ApprovalReliefViewModel model)
        {

            model.userBranchId = (short)token.GetBranchId;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.applicationUrl = HttpContext.Current.Request.Path;

            var data = repo.AddStaffRelief(model);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, message = "The Relief has been created successfully." });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "There was an error creating this record" });


        }
    }
}