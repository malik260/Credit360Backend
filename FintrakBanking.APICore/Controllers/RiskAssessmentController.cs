using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels.Risk;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/risk")] 
    public class RiskAssessmentController : ApiControllerBase
    {
        private IRiskImplementation repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public RiskAssessmentController(IRiskImplementation _repo)
        {
            this.repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment/form")]
        public HttpResponseMessage GetRiskTypeFormElements(int applicationId, int titleId)
        {
            try
            {
                var data = repo.GetRiskFormElements(token.GetCompanyId, titleId, applicationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("risk-assessment/save")]
        public HttpResponseMessage SaveFormElements([FromBody]AssessmentFormSaveViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.SaveFormElements(entity);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.InnerException}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("assessment-result/application")]
        public HttpResponseMessage GetAllAssessmentResultByApplicationId(int applicationId)
        {
            try
            {
                var data = repo.GetAllAssessmentResultByApplicationId(token.GetCompanyId, applicationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.InnerException });
            }
        }

    }
}