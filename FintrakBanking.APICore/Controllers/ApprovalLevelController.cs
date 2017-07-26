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

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/setups")]
    public class ApprovalLevelController : ApiControllerBase
    {
        private IApprovalLevelRepository repo;

        public ApprovalLevelController(IApprovalLevelRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level
        [HttpPost]
        [Route("approval-level")]
        public HttpResponseMessage AddApprovalLevel([FromBody] ApprovalLevelViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                // model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddApprovalLevel(model);
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

        [HttpPost]
        [Route("approval-level-multiple")]
        public HttpResponseMessage AddMultipleApprovalLevel([FromBody] List<ApprovalLevelViewModel> model)
        {
            try
            {
                var token = new TokenDecryptionHelper();

                var recordId = repo.AddMultipleApprovalLevel(model);
                if (recordId)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = recordId, message = "Approval Levels has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "Approval Levels not created" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("approval-level")]
        public HttpResponseMessage GetAllApprovalLevel()
        {
            try
            {
                var token = new TokenDecryptionHelper();

                var data = repo.GetAllApprovalLevel(token.GetCompanyId);
                if (data == null)
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

        [HttpGet]
        [Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage GetApprovalLevelById(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetApprovalLevelById(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpGet]
        [Route("approval-level/operation-mapping/{id}")]
        public HttpResponseMessage GetApprovalLevelByOperationId(int id)
        {

            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetApprovalLevelByOperationId(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpPut]
        [Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage UpdateApprovalLevel(int id, [FromBody] ApprovalLevelViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateApprovalLevel(id, model);

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

        [HttpDelete]
        [Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage DeleteApprovalLevel(int id)
        {
            try
            {
                TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                repo.DeleteApprovalLevel(id, user);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = id, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region
        [HttpGet]
        [Route("workflowtracker/operation/{oId}/targetId/{tId}")]
        public HttpResponseMessage GetApprovalTrailByOperationIdAndTargetId(int oId, int tId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetApprovalTrailByOperationIdAndTargetId(oId, tId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpGet]
        [Route("workflowtracker/operation/{id}")]
        public HttpResponseMessage GetApprovalTrail(int id)
        {
            HttpResponseMessage result= null;
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetApprovalTrail(id, token.GetCompanyId);
                if (data.Any())
                {
                    result= Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = data, count = 1 });
                }

            }
            catch (System.Exception ex)
            {
                result = Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
            return result;
        }
        #endregion
    }
}