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
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/setups")]
    public class ApprovalLevelController : ApiControllerBase
    {
        private IApprovalLevelRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ApprovalLevelController(IApprovalLevelRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level

         [HttpPost] [ClaimsAuthorization]
        [Route("approval-level")]
        public HttpResponseMessage AddApprovalLevel([FromBody] ApprovalLevelViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddApprovalLevel(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-level/all")]
        public HttpResponseMessage GetAllApprovalLevel()
        {
            try
            {
                var data = repo.GetAllApprovalLevel(token.GetCompanyId);
                if (data == null)
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

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-levels/operation/{operationId}/product-class/{classId}")]
        public HttpResponseMessage GetApprovalLevelByOperationIdAndProductClassId(int operationId, int? classId)
        {
            try
            {
                List<FintrakDropDownSelectList> data = repo.GetApprovalLevelsByOperationIdAndProductClassId(operationId, classId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-level-detailed/all")]
        public HttpResponseMessage GetAllDetailedApprovalLevel()
        {
            try
            {
                var data = repo.GetAllApprovalLevelDetails(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-level/approval-level/{id}")]
        public HttpResponseMessage GetApprovalLevelById(int id)
        {
            try
            {
                var data = repo.GetApprovalLevelById(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-level/group/{groupId}")]
        public HttpResponseMessage GetApprovalLevelByGroupId(int groupId)
        {
            try
            {
                var data = repo.GetApprovalLevelByGroupId(groupId, token.GetCompanyId);
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approval-level/operation/{operationId}")]
        public HttpResponseMessage GetApprovalLevelByOperationId(int operationId)
        {
            try
            {
                var data = repo.GetApprovalLevelByOperationId(operationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("approval-level/{id}")]
        public HttpResponseMessage UpdateApprovalLevel(int id, [FromBody] ApprovalLevelViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = e.InnerException, error = e.InnerException, e.StackTrace });
            }

        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("approval-level/{id}")]
        public async Task<HttpResponseMessage> DeleteApprovalLevelAsync(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var saved = await repo.DeleteApprovalLevel(id, user);
                if (saved)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = id, message = "Record has been deleted successfully" });
                }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = id, message = "Record could not be deleted." });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }

        }
        #endregion

        #region preset route

      [HttpGet] [ClaimsAuthorization]  
        [Route("preset-route-collection/operation/{operationId}/product-class/{classId}")]
        public HttpResponseMessage GetPresetRouteCollection(int operationId, int? classId)
        {
            try
            {
                PresetRouteViewModel response = repo.GetPresetRouteCollection(operationId, classId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost] 
        [ClaimsAuthorization]
        [Route("preset-route")]
        public HttpResponseMessage PresetRoute([FromBody] PresetRouteViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                bool response = repo.PresetRoute(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-level-list-for-approval")]
        public HttpResponseMessage GetApprovalLevelForApproval()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetTempApprovalApprovalLevel(token.GetStaffId);
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
        [Route("go-for-workflow-level-approval")]
        public HttpResponseMessage GoForWorkflowLevelApproval([FromBody]ApprovalLevelViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var data = repo.GoForApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"An error has accoured {ex.Message}" });
            }


        }

        #endregion preset route


        #region route operation

        [HttpPost]
        [Route("routable-operations")]
        public HttpResponseMessage GetRoutableOperations([FromBody] List<int> operationIds)
        {
            List<FintrakDropDownSelectList> data = repo.GetRoutableOperations(operationIds);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [Route("reroute-approval-levels/operation/{operationId}")]
        public HttpResponseMessage GetRerouteApprovalLevels(int operationId)
        {
            List<ApprovalLevelViewModel> data = repo.GetRerouteApprovalLevels(operationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [Route("reroute-operation")]
        public HttpResponseMessage RerouteOperation([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            bool data = repo.RerouteOperation(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        #endregion route operation


        #region trail
        [HttpGet] [ClaimsAuthorization]  
        [Route("workflowtracker/operation/{oId}/target/{tId}")]
        public HttpResponseMessage GetApprovalTrailByOperationIdAndTargetId(int oId, int tId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetApprovalTrailByOperationIdAndTargetId(oId, tId, token.GetCompanyId);
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data.ToList(), count = data.Count() });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = data.ToList(), count = data.Count() });
                }

            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("workflowtracker/operation/{id}")]
        public HttpResponseMessage GetApprovalTrail(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetApprovalTrail(id, token.GetCompanyId);
                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data.ToList(), count = data.Count() });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = data.ToList(), count = data.Count() });
                }

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion


        //

    }
}