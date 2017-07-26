using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{ 
    [RoutePrefix("api/v1/setups")]
    public class ApprovalLevelStaffController : ApiControllerBase
    {
        private IApprovalLevelStaffRepository repo;

        public ApprovalLevelStaffController(IApprovalLevelStaffRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level Staff
        [HttpPost][Route("approval-level-staff")]
        public HttpResponseMessage AddApprovalLevelStaff(HttpRequestMessage request, [FromBody] ApprovalLevelStaffViewModel model)
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

                    var data = repo.AddApprovalLevelStaff(model);
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

        [HttpGet][Route("approval-level-staff/operations/{operationMappingId}")]
        public HttpResponseMessage GetAllApprovalLevelStaff(HttpRequestMessage request, int operationMappingId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllApprovalLevelStaffByOperationId(operationMappingId, token.GetCompanyId);
                    if (!data.Any())
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

        [HttpGet][Route("approval-level-staff/staff-level/{id}")]
        public HttpResponseMessage GetApprovalLevelStaffById(HttpRequestMessage request, int id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetApprovalLevelStaffById(id, token.GetCompanyId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                                Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                                Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
            return    response;
            });
        }

        [HttpPut][Route("approval-level-staff/{id}")]
        public HttpResponseMessage UpdateApprovalLevelStaff(HttpRequestMessage request, [FromBody] ApprovalLevelStaffViewModel model, int id)
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

                    var data = repo.UpdateApprovalLevelStaff(id, model);

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

        [HttpDelete][Route("approval-level-staff/{StaffLevelId}")]
        public HttpResponseMessage DeleteApprovalLevelStaff(HttpRequestMessage request, int StaffLevelId)
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

                    repo.DeleteApprovalLevelStaff(StaffLevelId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                             Ok(new { success = true, result = StaffLevelId, message = "record has been deleted successfully" }));
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
    }
}