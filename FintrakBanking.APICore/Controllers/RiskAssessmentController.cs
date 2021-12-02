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
using FintrakBanking.Common.CustomException;
using System.Collections.Generic;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/risk")] 
    public class RiskAssessmentController : ApiControllerBase
    {
        private IRiskImplementation repo;
        private ICreditOfficerRiskRepository corrRepo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public RiskAssessmentController(IRiskImplementation _repo, ICreditOfficerRiskRepository _corrRepo)
        {
            this.repo = _repo;
            this.corrRepo = _corrRepo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment/form")]
        public HttpResponseMessage GetRiskTypeFormElements(int titleId, int? targetId)
        {
            try
            {
                var data = repo.GetRiskFormElements(token.GetCompanyId, titleId, targetId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.InnerException}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("assessment-result")]
        public HttpResponseMessage GetAllAssessmentResultByApplicationId()
        {
            try
            {
                var data = repo.GetAllAssessmentResult(token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.InnerException });
            }
        }











        [HttpGet]
        [ClaimsAuthorization]
        [Route("rating-period")]
        public HttpResponseMessage GetRatingPeriods()
        {
            IEnumerable<RatingPeriodViewModel> response = corrRepo.GetRatingPeriods();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("rating-period")]
        public HttpResponseMessage AddRatingPeriod([FromBody] RatingPeriodViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = corrRepo.AddRatingPeriod(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("credit-officer-search")]
        public HttpResponseMessage AddCreditOfficerSearch([FromBody] CreditOfficerSearchViewModel model)
        {
            List<CreditOfficerRatingViewModel> response = corrRepo.GetCreditOfficerSearch(model);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("credit-officer-risk-rating")]
        public HttpResponseMessage AddOfficerRating([FromBody] OfficerRatingViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = corrRepo.AddOfficerRating(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("key-indicator-assessment-parameters")]
        public HttpResponseMessage GetKeyIndicatorAssessmentParameters()
        {
            KeyIndicatorAssessmentParametersViewModel response = corrRepo.GetKeyIndicatorAssessmentParameters();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.keyIndicators.Count() });
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("current-credit-officer-risk-rating/{id}")]
        public HttpResponseMessage GetCurrentCreditOfficerRiskRating(int id)
        {
            CreditOfficerRiskRatingDetail response = corrRepo.GetCurrentCreditOfficerRiskRating(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.parameters.Count() });
        }

    }
}