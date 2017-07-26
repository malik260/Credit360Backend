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
        [HttpPost][Route("approval-level")]
        public HttpResponseMessage AddApprovalLevel(HttpRequestMessage request,[FromBody] ApprovalLevelViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, result = data, message = "The record has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = "There was an error creating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpPost][Route("approval-level-multiple")]
        public HttpResponseMessage AddMultipleApprovalLevel(HttpRequestMessage request, [FromBody] List<ApprovalLevelViewModel> model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var recordId = repo.AddMultipleApprovalLevel(model);
                if (recordId)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = true, result = recordId, message = "Approval Levels has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "Approval Levels not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet][Route("approval-level")]
        public HttpResponseMessage GetAllApprovalLevel(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllApprovalLevel(token.GetCompanyId);
                    if (response == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet][Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage GetApprovalLevelById(HttpRequestMessage request,int id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    var data = repo.GetApprovalLevelById(id, token.GetCompanyId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpGet][Route("approval-level/operation-mapping/{id}")]
        public HttpResponseMessage GetApprovalLevelByOperationId(HttpRequestMessage request, int id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    var data = repo.GetApprovalLevelByOperationId(id, token.GetCompanyId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                      Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                      Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpPut]
        [Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage UpdateApprovalLevel(HttpRequestMessage request, int id, [FromBody] ApprovalLevelViewModel model)
        {

            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                //model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateApprovalLevel(id, model);

                if (data)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Created("", new { success = true, result = data, message = "The record has been updated successfully" }));

                }
                response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error updating this record" }));
            }
            catch (Exception e)
            {
                response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"There was an error updating this record {e.Message}" }));
            }
                return response;
        });
        }

        [HttpDelete][Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage DeleteApprovalLevel(HttpRequestMessage request, int id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        //applicationUrl = Request.Path.Value,
                        //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repo.DeleteApprovalLevel(id, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = id, message = "record has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
        #endregion
       
        #region
        [HttpGet][Route("workflowtracker/operation/{oId}/targetId/{tId}")]
        public HttpResponseMessage GetApprovalTrailByOperationIdAndTargetId(HttpRequestMessage request, int oId, int tId) {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    var data = repo.GetApprovalTrailByOperationIdAndTargetId(oId, tId, token.GetCompanyId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                           Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                           Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpGet][Route("workflowtracker/operation/{id}")]
        public HttpResponseMessage GetApprovalTrail(HttpRequestMessage request, int id)
        { HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    var data = repo.GetApprovalTrail(id, token.GetCompanyId);
                    if (data.Any())
                        response = request.CreateResponse(HttpStatusCode.OK,
                                    Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                                Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });
             
        }
        #endregion
    }
}