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
    public class ApprovalGroupController : ApiControllerBase
    {
        private IApprovalGroupMappingRepository repoMapping;

        private IApprovalGroupRepository repoGroup;

        public ApprovalGroupController(IApprovalGroupMappingRepository _repoMapping, IApprovalGroupRepository _repoGroup)
        {
            this.repoMapping = _repoMapping;
            this.repoGroup = _repoGroup;
        }

        #region Approval Group Mapping
        [HttpPost]
        [Route("approval-group-mapping")]
        public HttpResponseMessage AddApprovalGroupMapping(HttpRequestMessage request, [FromBody] ApprovalGroupMappingViewModel model)
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

                    var data = repoMapping.AddApprovalGroupMapping(model);
                    if (data != -1)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been created successfully" }));
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

        [HttpGet][Route("approval-group-mapping/{operationMappingId}")]
        public HttpResponseMessage GetApprovalGroupMappingById(HttpRequestMessage request, int operationMappingId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {

                try
                {
                    //var token = new TokenDecryptionHelper(this.HttpContext);

                    var  data = repoMapping.GetApprovalGroupMapping(operationMappingId);
                    if (response == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = 1 }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet] [Route("approval-group-mapping/operation/{operationId}/product/{productClassId}")]
        public HttpResponseMessage GetApprovalGroupMapping(HttpRequestMessage request, int operationId, short productClassId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                  //  var token = new TokenDecryptionHelper(this.HttpContext);

                    short? productClass;

                    if (productClassId != -1)
                        productClass = productClassId;
                    else
                        productClass = null;

                    var data = repoMapping.GetApprovalGroupMapping(operationId, productClass);
                    if (response== null)
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


        [HttpPut] [Route("approval-group-mapping/{operationMappingId}")]
        public HttpResponseMessage UpdateApprovalGroupMapping(HttpRequestMessage request,
            int operationMappingId, [FromBody] ApprovalGroupMappingViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //  model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repoMapping.UpdateApprovalGroupMapping(operationMappingId, model);

                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data, message = "The record has been updated successfully" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "There was an error updating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"There was an error updating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpDelete][Route("approval-group-mapping/{operationMappingId}")]
        public HttpResponseMessage DeleteApprovalGroupMapping(HttpRequestMessage request, int operationMappingId)
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
                       // applicationUrl = Request.Path.Value,
                       // userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repoMapping.DeleteApprovalGroupMapping(operationMappingId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = operationMappingId, message = "record has been deleted successfully" }));
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

        #region Approval Group
        [HttpPost][Route("approval-group")]
        public HttpResponseMessage AddApprovalGroup(HttpRequestMessage request, [FromBody] ApprovalGroupViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    model.userBranchId = (short)token.GetBranchId;
                    //  model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //  model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repoGroup.AddApprovalGroup(model);
                    if (data)
                    {
                        response  = request.CreateResponse(HttpStatusCode.OK,
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

        [HttpGet][Route("approval-group")]
        public HttpResponseMessage GetAllApprovalGroup(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);


                    var data = repoGroup.GetAllApprovalGroup(token.GetCompanyId);
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = response, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet][Route("approval-group/{GroupId}")]
        public HttpResponseMessage GetApprovalGroup(HttpRequestMessage request,int GroupId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    var data = repoGroup.GetApprovalGroupById(GroupId, token.GetCompanyId);
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

        [HttpPut][Route("approval-group/{GroupId}")]
        public HttpResponseMessage UpdateApprovalGroup(HttpRequestMessage request,int GroupId, [FromBody] ApprovalGroupViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    // model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repoGroup.UpdateApprovalGroup(GroupId, model);

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

        [HttpDelete][Route("approval-group/{GroupId}")]
        public HttpResponseMessage DeleteApprovalGroup(HttpRequestMessage request, int GroupId)
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
                        //  applicationUrl = Request.Path.Value,
                        // userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repoGroup.DeleteApprovalGroup(GroupId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = GroupId, message = "record has been deleted successfully" }));
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