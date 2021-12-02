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
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

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

         [HttpPost] [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("go-for-level-staff-approval")]
        public HttpResponseMessage GoForWorkflowGroupApproval([FromBody]ApprovalLevelStaffViewModel model)
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

        [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("temp-approval-level-staff")]
        public HttpResponseMessage GetTempAllApprovalLevelStaff()
        {
            try
            {
                var data = repo.GetTempApprovalLevelStaff( token.GetStaffId);
                if (!data.Any())
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
        [Route("approval-level-staff/staff-level/{id}")]
        public HttpResponseMessage GetApprovalLevelStaffById(int id)
        {
            try
            {
                var data = repo.GetApprovalLevelStaffById(id, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("approval-level-staff/{StaffLevelId}")]
        public async Task<HttpResponseMessage> DeleteApprovalLevelStaffAsync(int StaffLevelId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
            };

                var saved = await repo.DeleteApprovalLevelStaff(StaffLevelId, user);

                if (saved)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = StaffLevelId, message = "record has been deleted successfully" });

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = StaffLevelId, message = "Record could not be saved" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException, stack = ex.StackTrace });
            }
        }

        #endregion Approval Level Staff

        #region Workflow Tracker

        [HttpGet] [ClaimsAuthorization]  
        [Route("work-flow-tracker/operation/{operationId}/target/{targetId}")]
        public HttpResponseMessage GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId)
        {
               var data =  repo.GetApprovalTrailByOperationIdAndTargetId(operationId, targetId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("work-flow-tracker/operation-site-report/{targetId}")]
        public HttpResponseMessage GetApprovalTrailBySiteTargetId(int targetId)
        {
            var data = repo.GetApprovalTrailBySiteTargetId(targetId, token.GetCompanyId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = "No record found"});
            }else

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }


        [HttpGet] [ClaimsAuthorization]  
        [Route("work-flow-tracker/approval-trail/all")]
        public async Task<HttpResponseMessage> GetAllRecordsOnApprovalTrail([FromUri] int page, [FromUri] int itemsPerPage)
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

        [HttpGet]
        [ClaimsAuthorization]
        [Route("work-flow-tracker/approval-status")]
        public HttpResponseMessage GetAllApprovalStatus()
        {
                var data = repo.GetAllApprovalStatus();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data  });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("work-flow-tracker/approval-operation")]
        public HttpResponseMessage GetAllApprovalOperations()
        {
            
                var data = repo.GetAllApprovalOperations();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }
        #endregion Workflow Tracker

        #region Approval Monitoring 
        [HttpPost]
        [ClaimsAuthorization]
        [Route("work-flow-tracker/approval-monitoring")]
        public HttpResponseMessage GetApprovalMointoring(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();

            dateRange.companyId = token.GetCompanyId;
            var data = repo.GetApprovalMointoring(dateRange);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            
        }

        [HttpPost]
        [Route("work-flow-tracker/approval-booking-monitoring")]
        public HttpResponseMessage GetBookingMointoring(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();

            dateRange.companyId = token.GetCompanyId;
            var data = repo.GetBookingMointoring(dateRange);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [Route("work-flow-tracker/approval-review-monitoring")]
        public HttpResponseMessage GetContractReviewMointoring(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();

            dateRange.companyId = token.GetCompanyId;
            var data = repo.GetContractReviewMointoring(dateRange);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("work-flow-tracker/target/{targetId}")]
        public HttpResponseMessage GetApprovalTrailByTargetId(int targetId)
        {
            
                var data = repo.GetApprovalTrailByTargetId(targetId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("work-flow-tracker/booking/target/{targetId}")]
        public HttpResponseMessage GetBookingApprovalTrailByTargetId(int targetId)
        {
                var data = repo.GetBookingApprovalTrailByTargetId(targetId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("approval-monitoring/export")]
        public HttpResponseMessage ExportScheduleToExcel([FromBody] DateRange model)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                var fileBytes = repo.GenerateApprovalMonitoringReport(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("approval-comments/export/{requireAll}")]
        public HttpResponseMessage ExportApprovalComments([FromBody] List<ApprovalTrailViewModel> model, bool requireAll)
        {
            try
            {
                //model.companyId = token.GetCompanyId;
                var fileBytes = repo.ExportApprovalComments(model, requireAll);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }

        }
        #endregion Approval Monitoring
    }
}
