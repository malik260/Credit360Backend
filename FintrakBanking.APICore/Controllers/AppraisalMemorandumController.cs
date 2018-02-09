using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class AppraisalMemorandumController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IAppraisalMemorandumRepository repo;

        public AppraisalMemorandumController(IAppraisalMemorandumRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-application/{loanApplicationId}")]
        public HttpResponseMessage GetAppraisalMemorandumByLoanApplicationId(int loanApplicationId)
        {
            try
            {
                var data = repo.GetAppraisalMemorandum(loanApplicationId,token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-application/{loanApplicationId}/documentation")]
        public HttpResponseMessage GetAppraisalMemorandumDocumentation(int loanApplicationId)
        {
            try
            {
                var data = repo.GetAllDocumentation(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("appraisal-memorandum")]
        public HttpResponseMessage AddAppraisalMemorandum([FromBody] AppraisalMemorandumViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddAppraisalMemorandum(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut]
        [Route("appraisal-memorandum/{appraisalMemorandumId}")]
        public HttpResponseMessage UpdateAppraisalMemorandum([FromBody] AppraisalMemorandumViewModel entity, int appraisalMemorandumId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateAppraisalMemorandum(entity, appraisalMemorandumId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("appraisal-memorandum/forward")]
        public HttpResponseMessage ForwardAppraisalMemorandum([FromBody] ForwardViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                int response = repo.ForwardAppraisalMemorandum(entity);

                if (response == 0)
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "There was an error creating this record" });

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application has been acted on successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/trail/{loanApplicationId}/operation/{operationId}")]
        public HttpResponseMessage GetAppraisalMemorandumTrail(int loanApplicationId, int operationId)
        {
            try
            {
                var data = repo.GetAppraisalMemorandumTrail(loanApplicationId,operationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route("appraisal-memorandum/privilege/{loanApplicationId}/operation/{operationId}")]
        //public HttpResponseMessage GetUserPrivilege(int loanApplicationId, int operationId)
        //{
        //    try
        //    {
        //        var data = repo.GetUserPrivilege(token.GetStaffId, loanApplicationId, operationId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}
        /*
        [HttpPost]
        [Route("appraisal-memorandum/forward")]
        public HttpResponseMessage ForwardAppraisalMemorandum([FromBody] ForwardViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;*/

        [HttpPost]
        [Route("appraisal-memorandum/privilege")]
        public HttpResponseMessage GetUserPrivilege([FromBody] AuthoritySignatureViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                var data = repo.GetUserPrivilege(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-detail/{loanApplicationId}")]
        public HttpResponseMessage GetApprovedLoanDetail(int loanApplicationId)
        {
            try
            {
                var data = repo.GetApprovedLoanDetail(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-detail-fees/{loanApplicationId}")]
        public HttpResponseMessage GetLoanDetailsFee(int loanApplicationId)
        {
            try
            {
                var data = repo.GetLoanDetailsFee(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-detail-change-log/{loanApplicationId}")]
        public HttpResponseMessage GetLoanDetailChangeLog(int loanApplicationId)
        {
            try
            {
                var data = repo.GetLoanDetailChangeLog(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/confirmation/{type}/application/{loanApplicationId}")]
        public HttpResponseMessage Confirmation(int type, int loanApplicationId)
        {
            try
            {
                var data = repo.Confirmation(type, loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet, Route("loan-application-approval-process")]
        public HttpResponseMessage GetPendingLoanApplications([FromUri] int page, [FromUri] int itemsPerPage, [FromUri] int? classId, [FromUri] string searchString)
        {
            try
            {
                IQueryable<LoanApplicationViewModel> items;
                items = repo.GetPendingLoanApplications(token.GetCountryId, token.GetBranchId, token.GetStaffId, classId);

                if (!String.IsNullOrEmpty(searchString))
                {
                    items = items.Where(x => 
                        x.applicationReferenceNumber.Contains(searchString)
                        || x.applicationAmount.ToString().Contains(searchString)
                        || x.customerName.Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.newApplicationDate) // OrderBy() must be called for Skip() to work!
                    .ThenByDescending(x => x.loanApplicationId)
                    .Skip(page)
                    .Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [Route("current-committee/application/{loanApplicationId}")]
        public HttpResponseMessage GetCurrentCommitteeByLoanApplicationId(int loanApplicationId)
        {
            try
            {
                var data = repo.GetCurrentCommittee(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("appraisal-memorandum/forward-secretariat")]
        public HttpResponseMessage SecretariatForwardAppraisalMemorandum([FromBody] ForwardCommitteeCamViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var response = repo.SecretariatForwardAppraisalMemorandum(entity);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("regional-loan-application")]
        public HttpResponseMessage GetRegionalLoanApplications([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var items = repo.GetRegionalLoanApplications(token.GetStaffId);

                var data = items.OrderByDescending(x => x.applicationDate)
                    .ThenByDescending(x => x.loanApplicationId)
                    .Skip(page).Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        //[HttpGet]
        //[Route("appraisal-workflow/next-level-staff/{loanApplicationId}/operation/{operationId}`")]
        //public HttpResponseMessage GetNextLevelStaff(int loanApplicationId, int operationId)
        //{
        //    try
        //    {
        //        var data = repo.GetNextLevelStaff(token.GetStaffId, loanApplicationId, operationId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        //credit/loan-application/product-programs

        /*UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };*/


        [HttpGet]
        [Route("appraisal-memorandum/pending-product-program")]
        public HttpResponseMessage GetPendingProductProgram()
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = repo.GetPendingProductProgram(user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [Route("untenored-status/application/{applicationId}")]
        public HttpResponseMessage GetUntenoredStatus(int applicationId)
        {
            try
            {
                bool status = repo.GetUntenoredStatus(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #region MONITORING TRIGGERS

        [HttpGet]
        [Route("application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage GetApplicationMonitoringTriggers(int applicationId)
        {
            try
            {
                var response = repo.GetApplicationMonitoringTriggers(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage SaveApplicationMonitoringTriggers(int applicationId, [FromBody] List<MonitoringTriggersViewModel> entity)
        {
            try
            {
                var response = repo.SaveApplicationMonitoringTriggers(applicationId, entity, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion MONITORING TRIGGERS



    }
}
