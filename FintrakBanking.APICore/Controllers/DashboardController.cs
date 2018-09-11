using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/dashboard")]
    public class DashboardController : ApiController
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IDashboardRepository dashboard;
        public DashboardController(IDashboardRepository _dashboard)
        {
            dashboard = _dashboard;
        }

        [HttpPost]
        [Route("loan-application-sector")]
        public HttpResponseMessage GetAppraisalMemorandumDocumentation(DateRange val)
        {
            try
            {
                var data = dashboard.LoanApplicationsBySector(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("loan-performamce")]
        public HttpResponseMessage GetLoanPerformaceByStatus(DateRange val)
        {
           
            try
            {
                var data = dashboard.LoanPerformance(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("loans-on-pipeline")]
        public HttpResponseMessage GetLoansOnPipeline(DateRange val)
        {
            try
            {
                var data = dashboard.LoanOnThePipeline(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("loans-risk-exposure")]
        public HttpResponseMessage GetLoansByRiskExposure(DateRange val)
        {
            try
            {
                var data = dashboard.ExpotureByRiskRating(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("collateral-exposure")]
        public HttpResponseMessage GetCollateralExposure(DateRange val)
        {
            try
            {
                var data = dashboard.CollateralCoverage(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("risk-exposure")]
        public HttpResponseMessage GetTotalRiskExposure(DateRange val)
        {
            try
            {
                var data = dashboard.TotalRiskExposure(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("approved-loan")]
        public HttpResponseMessage GetApprovedLoan(DateRange val)
        {
            try
            {
                var data = dashboard.ApprovedLoan(val.startDate, val.endDate, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
    
}
