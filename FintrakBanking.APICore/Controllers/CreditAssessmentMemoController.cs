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


        [HttpPost] [Route("loan-application/operation/{id}")]
        public HttpResponseMessage AddCreditAssessmentMemo(HttpRequestMessage request, int id, [FromBody] CreditAssessmentMemoViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.createdBy = token.GetStaffId;
                    // entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    var data = repo.AddCreditAssessmentMemo(id, entity);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Created("", new { success = true, result = entity, message = "The record has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "There was an error creating this record" }));
                }
                catch (Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpPost] [Route("loan-application/operation/{id}")]
        public HttpResponseMessage GetRequestOnCreditAssessmentMemo(HttpRequestMessage request, int id, [FromUri] int page, [FromUri] int itemsPerPage)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.GetRequestOnCreditAssessmentMemo(token.GetCountryId, token.GetBranchId, token.GetStaffId, id)
                        .Skip(page).Take(itemsPerPage);

                    if (data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });

        }

        [HttpGet] [Route("loan-application/operation/{id}")]
        public HttpResponseMessage GetRequestForCreditAssessmentMemo(HttpRequestMessage request, int id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.GetRequestOnCreditAssessmentMemo(token.GetCompanyId, token.GetBranchId, token.GetStaffId, id);

                    if (response == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                }
                catch (System.Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        //[HttpPost("assessment-template/template")]
        //public IActionResult SubmitRequestForProcessing([FromBody]ApprovalViewModel entity)
        //{
        //  try
        //    {
        //        token = new TokenDecryptionHelper(HttpContext);
        //        entity.BranchId = (short)token.GetBranchId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.createdBy = token.GetStaffId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //        entity.applicationUrl = Request.Path.Value;

        //        var response = repo.SubmitRequestForProcessing(entity);                 
        //        if (response)
        //        {
        //            response = request.CreateResponse(HttpStatusCode.OK, Created("", new { success = true, result = entity, message = "The record has been created successfully" }));
        //        }

        //        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "There was an error creating this record" }));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
        //        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" }));
        //    }
        //}
        [HttpPost] [Route("assessment-template/template")]
        public HttpResponseMessage GetAssessmentTempates(HttpRequestMessage request, [FromBody]CreditTemplateViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();

                    var data = repo.GetAssessmentTempates(entity.approvalLevelId, entity.productClassId, token.GetCompanyId);// repo.GetRequestForCreditAssessmentMemo(token.GetCountryId, token.GetBranchId);

                    if (data != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = response }));
                }
                catch (System.Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPost] [Route("assessment-template/template/customer")]
        public HttpResponseMessage GetAssessmentTempates(HttpRequestMessage request, [FromBody] AssessmentTemplatesViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();
                    entity.companyId = token.GetCompanyId;
                    var data = repo.GetAssessmentTempates(token.GetStaffId, entity.productClassId, token.GetCompanyId);// repo.GetRequestForCreditAssessmentMemo(token.GetCountryId, token.GetBranchId);

                    if (data != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = response }));
                }
                catch (System.Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPut] [Route("assessment-template")]
        public HttpResponseMessage UpdateAssessmentTempates(HttpRequestMessage request, [FromBody]AssessmentTemplatesViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.lastUpdatedBy = token.GetStaffId;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    // entity.userIPAddress =  //Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    var data = repo.UpdateAssessmentTempates(entity);

                    if (data != null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Created("", new { success = true, result = entity, message = "The record has been Update successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "There was an error Update this record" }));
                }
                catch (Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"There was an error Update this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpPost] [Route("assessment-template")]
        public HttpResponseMessage AddAssessmentTempates(HttpRequestMessage request, [FromBody] AssessmentTemplatesViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    token = new TokenDecryptionHelper();
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.createdBy = token.GetStaffId;
                    // entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    var data = repo.AddAssessmentTempates(entity).IsCompleted;
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Created("", new { success = true, result = entity, message = "The record has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "There was an error creating this record" }));
                }
                catch (Exception ex)
                {
                    this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" }));
                }
                return response;
            });
        }
    }
}