using System;
using System.Linq;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.ViewModels.Credit;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using System.Web;
using FintrakBanking.APICore.core;
using FintrakBanking.ViewModels.Business;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/cam")]
    public class CreditAssessmentMemoController : ApiControllerBase
    {
        private ICreditAssessmentMemoRepository repo;
        TokenDecryptionHelper token = null;
        IErrorLogRepository errorLogger;

        public CreditAssessmentMemoController(ICreditAssessmentMemoRepository _repo,

        IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;

            errorLogger = _errorLogger;
        }

        [HttpPost]
        [Route("loan-application/credit/operation/{id}")]
        public HttpResponseMessage AddCreditAssessmentMemo(int id, [FromBody] CreditAssessmentMemoViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddCreditAssessmentMemo(id, entity);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/operation/{id}")]
        public HttpResponseMessage GetRequestOnCreditAssessmentMemo(int id, [FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                token = new TokenDecryptionHelper();

                var data = repo.GetRequestOnCreditAssessmentMemo(token.GetCountryId, token.GetBranchId, token.GetStaffId, id)
                    .Skip(page).Take(itemsPerPage);

                if (data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-application/credit/request/{id}")]
        public HttpResponseMessage GetRequestForCreditAssessmentMemo(int id)
        {
            try
            {
                token = new TokenDecryptionHelper();

                var data = repo.GetRequestOnCreditAssessmentMemo(token.GetCompanyId, token.GetBranchId, token.GetStaffId, id);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("assessment/submit")]
        public HttpResponseMessage SubmitRequestForProcessing([FromBody]ApprovalViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.BranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var response = repo.SubmitRequestForProcessing(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }
        [HttpPost]
        [Route("assessment-template/template")]
        public HttpResponseMessage GetAssessmentTempates([FromBody]CreditTemplateViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();

                var data = repo.GetAssessmentTempates(entity.approvalLevelId, entity.productClassId, token.GetCompanyId);// repo.GetRequestForCreditAssessmentMemo(token.GetCountryId, token.GetBranchId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("assessment-template/staffproductclass")]
        public HttpResponseMessage GetAssessmentTempates([FromBody] AssessmentTemplatesViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.companyId = token.GetCompanyId;
                var data = repo.GetAssessmentTempates(token.GetStaffId, entity.productClassId, token.GetCompanyId);// repo.GetRequestForCreditAssessmentMemo(token.GetCountryId, token.GetBranchId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("assessment-template")]
        public HttpResponseMessage UpdateAssessmentTempates([FromBody]AssessmentTemplatesViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                var data = repo.UpdateAssessmentTempates(entity);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("assessment-template")]
        public HttpResponseMessage AddAssessmentTempates([FromBody] AssessmentTemplatesViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                var data = repo.AddAssessmentTempates(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }

        }
    }
}