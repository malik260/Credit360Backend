using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class ApprovalLevelStaffController : ApiControllerBase
    {
        private IApprovalLevelStaffRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ApprovalLevelStaffController(IApprovalLevelStaffRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level Staff

        [HttpPost]
        [Route("approval-level-staff")]
        public HttpResponseMessage AddApprovalLevelStaff([FromBody] ApprovalLevelStaffViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddApprovalLevelStaff(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet]
        [Route("approval-level-staff/operations/{operationMappingId}")]
        public HttpResponseMessage GetAllApprovalLevelStaff(int operationMappingId)
        {
            try
            {
                var data = repo.GetAllApprovalLevelStaffByOperationId(operationMappingId, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("approval-level-staff/staff-level/{id}")]
        public HttpResponseMessage GetApprovalLevelStaffById(int id)
        {
            try
            {
                var data = repo.GetApprovalLevelStaffById(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("approval-level-staff/{id}")]
        public HttpResponseMessage UpdateApprovalLevelStaff([FromBody] ApprovalLevelStaffViewModel model, int id)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateApprovalLevelStaff(id, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete]
        [Route("approval-level-staff/{StaffLevelId}")]
        public async Task<HttpResponseMessage> DeleteApprovalLevelStaffAsync(int StaffLevelId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                };

                var saved = await repo.DeleteApprovalLevelStaff(StaffLevelId, user);

                if (saved)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = StaffLevelId, message = "record has been deleted successfully" });

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = StaffLevelId, message = "Record could not be saved" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }
        }

        #endregion Approval Level Staff

        #region Workflow Tracker

        [HttpGet]
        [Route("work-flow-tracker/operation/{operationId}/target/{targetId}")]
        public async Task<HttpResponseMessage> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId)
        {
            try
            {
                var data = await repo.GetApprovalTrailByOperationIdAndTargetId(operationId, targetId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        
        [HttpGet]
        [Route("work-flow-tracker/approval-trail/all")]
        public async Task<HttpResponseMessage> GetAllRecordsOnApprovalTrail([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var item = repo.GetAllRecordsOnApprovalTrail(token.GetCompanyId);

                var data = await item.Skip(page).Take(itemsPerPage)
                    .ToListAsync();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = item.Count() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = item.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        #endregion Workflow Tracker
    }
}