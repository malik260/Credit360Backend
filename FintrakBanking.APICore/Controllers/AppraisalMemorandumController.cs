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
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.WorkFlow;

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
                var data = repo.GetAppraisalMemorandum(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
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
            catch (SecureException ex)
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
            catch (SecureException ex)
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
            catch (SecureException ex)
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

                WorkflowResponse response = repo.ForwardAppraisalMemorandum(entity);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application has been acted on successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/trail/{applicationId}/operation/{operationId}")]
        public HttpResponseMessage GetAppraisalMemorandumTrail(int applicationId, int operationId)
        {
            try
            {
                var data = repo.GetAppraisalMemorandumTrail(applicationId, operationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

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
            catch (SecureException ex)
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
                LoanApplicationDetailsViewModel data = repo.GetLoanApplicationDetail(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
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
            catch (SecureException ex)
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet, Route("loan-application-approval-process")]
        public HttpResponseMessage GetPendingLoanApplications([FromUri] int operationId, [FromUri] int page, [FromUri] int itemsPerPage, [FromUri] int? classId, [FromUri] string searchString)
        {
            try
            {
                IQueryable<LoanApplicationViewModel> items;
                items = repo.GetPendingLoanApplications(operationId, token.GetCountryId, token.GetBranchId, token.GetStaffId, classId);


                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    items = items.Where(x =>
                        x.applicationReferenceNumber.Contains(searchString)
                        //|| x.applicationAmount.ToString().Contains(searchString)
                        || x.customerName.ToLower().Contains(searchString)
                        || x.customerGroupName.ToLower().Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.loanApplicationId) // OrderBy() must be called for Skip() to work!
                    .Skip(page)
                    .Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            catch (SecureException ex)
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("regional-loan-application")]
        public HttpResponseMessage GetRegionalLoanApplications([FromUri] int page, [FromUri] int itemsPerPage, [FromUri] string searchString)
        {
            try
            {
                var items = repo.GetRegionalLoanApplications(token.GetStaffId);

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    items = items.Where(x =>
                        x.applicationReferenceNumber.Contains(searchString)
                        || x.customerName.ToLower().Contains(searchString)
                        || x.customerGroupName.ToLower().Contains(searchString)
                        ).Take(itemsPerPage);
                }

                var data = items
                    .OrderByDescending(x => x.applicationDate) // OrderBy() must be called for Skip() to work!
                    .ThenByDescending(x => x.loanApplicationId)
                    .Skip(page)
                    .Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

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
            catch (SecureException ex)
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
            catch (SecureException ex)
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage SaveApplicationMonitoringTriggers(int applicationId, [FromBody] List<MonitoringTriggersViewModel> entity)
        {
            try
            {
                var response = repo.SaveApplicationMonitoringTriggers(applicationId, entity, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion MONITORING TRIGGERS

        [HttpPost]
        [ClaimsAuthorization]
        [Route("repayment-schedule-terms")]
        public HttpResponseMessage SaveRepaymentScheduleAndTerms([FromBody] RepaymentScheduleTermsViewModel entity)
        {
            try
            {
                List<RepaymentScheduleTermsViewModel> response = repo.SaveRepaymentScheduleAndTerms(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("product-limit-validation")]
        public HttpResponseMessage SaveProductLimitValidation([FromBody] ProductLimitValidationViewModel entity)
        {
            try
            {
                List<ProductLimitValidationViewModel> response = repo.SaveProductLimitValidation(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("product-limit-validation/{applicationId}/class/{classId}")]
        public HttpResponseMessage GetProductLimitValidation(int applicationId, int classId)
        {
            try
            {
                List<ProductLimitValidationViewModel> response = repo.GetProductLimitValidation(applicationId, classId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("appraisal-memorandum/workflow-test")]
        public HttpResponseMessage WorkflowTest()
        {
            try
            {
                bool data = repo.WorkflowTest();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #region recommended collateral

        [HttpGet]
        [Route("recommended-collateral/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateral(int applicationId)
        {
            try
            {
                List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateral(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("recommended-collateral-history/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateralHistory(int applicationId)
        {
            try
            {
                List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateralHistory(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recommended-collateral")]
        public HttpResponseMessage AddRecommendedCollateral([FromBody] RecommendedCollateralViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.createdBy = (short)token.GetStaffId;
                List<RecommendedCollateralViewModel> response = repo.AddRecommendedCollateral(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("recommended-collateral")]
        public HttpResponseMessage UpdateRecommendedCollateral([FromBody] RecommendedCollateralViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.createdBy = (short)token.GetStaffId;
                List<RecommendedCollateralViewModel> response = repo.UpdateRecommendedCollateral(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion recommended collateral



        #region LMS APPROVAL

        [HttpGet]
        [Route("lms-application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage GetApplicationMonitoringTriggersLms(int applicationId)
        {
            try
            {
                IEnumerable<MonitoringTriggersViewModel> response = repo.GetApplicationMonitoringTriggersLms(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-application-monitoring-triggers/{applicationId}")]
        public HttpResponseMessage SaveApplicationMonitoringTriggersLms(int applicationId, [FromBody] List<MonitoringTriggersViewModel> entity)
        {
            try
            {
                IEnumerable<MonitoringTriggersViewModel> response = repo.SaveApplicationMonitoringTriggersLms(applicationId, entity, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-repayment-schedule-terms")]
        public HttpResponseMessage SaveRepaymentScheduleAndTermsLms([FromBody] RepaymentScheduleTermsViewModel entity)
        {
            try
            {
                List<RepaymentScheduleTermsViewModel> response = repo.SaveRepaymentScheduleAndTermsLms(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("lms-recommended-collateral/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateralLms(int applicationId)
        {
            try
            {
                List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateralLms(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("lms-recommended-collateral-history/{applicationId}")]
        public HttpResponseMessage GetRecommendedCollateralHistoryLms(int applicationId)
        {
            try
            {
                List<RecommendedCollateralViewModel> response = repo.GetRecommendedCollateralHistoryLms(applicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-recommended-collateral")]
        public HttpResponseMessage AddRecommendedCollateralLms([FromBody] RecommendedCollateralViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                List<RecommendedCollateralViewModel> response = repo.AddRecommendedCollateralLms(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lms-recommended-collateral")]
        public HttpResponseMessage UpdateRecommendedCollateralLms([FromBody] RecommendedCollateralViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                List<RecommendedCollateralViewModel> response = repo.UpdateRecommendedCollateralLms(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion LMS APPROVAL
        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("tranch-disbursment-approval-level")]
        public HttpResponseMessage saveTranchDisbursmentApprovalLevel([FromBody] TranchDisbursmentViewModel entity)
        {
            try
            {
                bool response = repo.saveTranchDisbursmentApprovalLevel(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }
}
