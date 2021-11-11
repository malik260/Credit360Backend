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
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{     
    [RoutePrefix("api/v1/setups")]
    public class ApprovalGroupController : ApiControllerBase
    {
        private IApprovalGroupMappingRepository repoMapping;

        private IApprovalGroupRepository repoGroup;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ApprovalGroupController(IApprovalGroupMappingRepository _repoMapping, IApprovalGroupRepository _repoGroup)
        {
            this.repoMapping = _repoMapping;
            this.repoGroup = _repoGroup;
        }

        #region Approval Group Mapping
        [HttpPost] [ClaimsAuthorization]
        [Route("approval-group-mapping")]
        public HttpResponseMessage AddApprovalGroupMapping(  [FromBody] ApprovalGroupMappingViewModel model)
        { 
                try
                {
                    model.userBranchId = (short)token.GetBranchId;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;
                model.staffId = token.GetStaffId;

                var data = repoMapping.AddApprovalGroupMapping(model);
                    if (data != -1)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
                catch (SecureException e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message, error = e.InnerException, stack = e.StackTrace });
                }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("approval-group-mapping/{operationMappingId}")]
        public HttpResponseMessage GetApprovalGroupMappingById(  int operationMappingId)
        {    try
                {
                  var token = new TokenDecryptionHelper( );

                    var  data = repoMapping.GetApprovalGroupMapping(operationMappingId);
                    if (data == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data, count = 1 });
                }
                catch (SecureException e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = $"Error: {e.Message}" });
                }
                 
        }

        [HttpGet, Route("approval-group-mapping/operation/{operationId}/product-class/{productClassId}/product/{productId}")]
        public HttpResponseMessage GetApprovalGroupMapping(int operationId, short? productClassId, short? productId)
        {
            try
            {
                productClassId = (productClassId == -1) ? null : productClassId;

                var data = repoMapping.GetApprovalGroupMapping(operationId, productClassId, productId);
                if (data== null)
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


       [HttpPut] [ClaimsAuthorization] [Route("approval-group-mapping/{operationMappingId}")]
        public HttpResponseMessage UpdateApprovalGroupMapping( 
            int operationMappingId, [FromBody] ApprovalGroupMappingViewModel model)
        { 
                try
                {
                    model.userBranchId = (short)token.GetBranchId;
                    // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                     model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.staffId = token.GetStaffId;
                model.lastUpdatedBy = token.GetStaffId;

                var data = repoMapping.UpdateApprovalGroupMapping(operationMappingId, model);

                    if (data)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data, message = "The record has been updated successfully" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "There was an error updating this record" });
                }
                catch (SecureException e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = $"There was an error updating this record {e.Message}" });
                }
                  
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("approval-group-mapping/{operationMappingId}")]
        public HttpResponseMessage DeleteApprovalGroupMapping(int operationMappingId)
        { 
                try
                {
                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        createdBy = token.GetStaffId,
                        staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                    };

                    repoMapping.DeleteApprovalGroupMapping(operationMappingId, user);

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = operationMappingId, message = "record has been deleted successfully" });
                }
                catch (SecureException ex)
                {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }

        }
        #endregion

        #region Approval Group
         [HttpPost] [ClaimsAuthorization]
        [Route("approval-group")]
        public HttpResponseMessage AddApprovalGroup(  [FromBody] ApprovalGroupViewModel model)
        { 
                try
                {
                TokenDecryptionHelper token =   new TokenDecryptionHelper();

                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
               // model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.staffId = token.GetStaffId;

                var data = repoGroup.AddApprovalGroup(model);
                    if (data)
                    {
                       return  Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, result = data, message = "The record has been created successfully" });
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

      [HttpGet] [ClaimsAuthorization]  [Route("approval-group")]
        public HttpResponseMessage GetAllApprovalGroup(HttpRequestMessage request)
        { 
                try
                {
                    var token =  new TokenDecryptionHelper();


                    var data = repoGroup.GetAllApprovalGroup(token.GetCompanyId);
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

        [HttpGet] [ClaimsAuthorization]  [Route("approval-group/{GroupId}")]
        public HttpResponseMessage GetApprovalGroup( int GroupId)
        { 
                try
                {
                    var token =   new TokenDecryptionHelper( );
                    var data = repoGroup.GetApprovalGroupById(GroupId, token.GetCompanyId);
                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = true, result = data, count = 1 });
                }
                catch (SecureException ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = $"There was an error updating this record {ex.Message}" });
                }
                 
            
        }
        [HttpGet]
        [Route("approval-group-list-for-approval")]
        public HttpResponseMessage GetApprovalGroupForApproval()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repoMapping.GetTempApprovalGroupForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An error has accoured {ex.Message}" });
            }


        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("go-for-workflow-group-approval")]
        public HttpResponseMessage GoForWorkflowGroupApproval([FromBody]ApprovalGroupMappingViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var data = repoMapping.GoForApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An error has accoured {ex.Message}" });
            }


        }
        [HttpPut] [ClaimsAuthorization]
        [Route("approval-group/{GroupId}")]
        public HttpResponseMessage UpdateApprovalGroup( int GroupId, [FromBody] ApprovalGroupViewModel model)
        { 
                try
                {
                    var token =  new TokenDecryptionHelper( );
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                   model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repoGroup.UpdateApprovalGroup(GroupId, model);

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

        [HttpDelete] [ClaimsAuthorization]
        [Route("approval-group/{GroupId}")]
        public HttpResponseMessage DeleteApprovalGroup(  int GroupId)
        { 
                try
                {
                    var token =   new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                        // userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repoGroup.DeleteApprovalGroup(GroupId, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = GroupId, message = "record has been deleted successfully" });
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = ex.Message });
                }
                 
           
        }
        #endregion
    }
}