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

        [HttpPost]
        [Route("approval-relief")]
        public HttpResponseMessage AddApprovalRelief([FromBody] ApprovalReliefViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddApprovalRelief(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpPut]
        [Route("approval-relief/{reliefId}")]
        public HttpResponseMessage UpdateApprovalRelief(int ReliefId, [FromBody] ApprovalReliefViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateApprovalRelief(ReliefId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }
       
    }
}