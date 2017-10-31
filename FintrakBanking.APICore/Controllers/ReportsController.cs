using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/report")]
    public class ReportsController : ApiControllerBase
    {
        IReportRoutes repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        IErrorLogRepository errorLogger;
        IFinanceTransactionsReport reportRepo;

        public ReportsController(IReportRoutes _repo, IFinanceTransactionsReport reportRepo, IErrorLogRepository _errorLogger) {

            this.repo = _repo;
            this.reportRepo = reportRepo;
            errorLogger = _errorLogger;
        }

        [HttpGet]
        [Route("workflowsla/loanapplication/{id}")]
        public HttpResponseMessage GetCollateralTypeByProduct(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowSLA(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loans/loanschedule/{loanid}")]
        public HttpResponseMessage GetLoanScheduleReport(int loanid)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanScheduleReport(loanid, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("limitmonitoring/sector")]
        public HttpResponseMessage GetSectorLimitMonitoringReport()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetSectorLimitMonitoringReport(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("limitmonitoring/branch")]
        public HttpResponseMessage GetBranchLoanAmountLimit()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetBranchLoanAmountLimit(token.GetBranchId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("workflow-definition/operation/{id}")]
        public HttpResponseMessage GetWorkflowDefinition(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowDefinition(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("loan-disburstloans")]
        public HttpResponseMessage GetDisburstLoans(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetDisburstLoans(dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #region Offer-Letter Generation & Loan Monitoring Reports

        [HttpGet]
        [Route("offer-letter")]
        public HttpResponseMessage GetGeneratedOfferLetter(string applicationRefNumber)
        {
            try
            {
                var data = repo.GetGeneratedOfferLetter(applicationRefNumber);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("monitoring/collateral-property-revaluation")]
        public HttpResponseMessage GetCollateralPropertyRevaluationReport()
        {
            try
            {
                var data = repo.GetCollateralPropertyRevaluationReport(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data }); 
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("monitoring/almost-due-covenants")]
        public HttpResponseMessage GetCovenantsApproachingDueDateReport()
        {
            try
            {
                var data = repo.GetCovenantsApproachingDueDateReport(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("monitoring/non-performing-loans")]
        public HttpResponseMessage GetNonPerformingLoansReport()
        {
            try
            {
                var data = repo.GetNonPerformingLoansReport(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("monitoring/overdraft-loans")]
        public HttpResponseMessage GetExpiredOverdraftLoansReport()
        {
            try
            {
                var data = repo.GetExpiredOverdraftLoansReport(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (Exception ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Offer-Letter Generation & Loan Monitoring Reports

        [HttpPost]
        [Route("loan-commercial")]
        public HttpResponseMessage GetLoanCommercialReport(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanCommercialReport(dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("posted-finance-transactions")]
        public HttpResponseMessage GetPostedTransactions(ReportSearchEntity searchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetPostedTransactions(searchEntity, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("loan-team-revolving")]
        public HttpResponseMessage GetTeamAndRevolving(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetTeamAndRevolving(dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("loan-earned-unearned-interest")]
        public HttpResponseMessage GetEarnedUnearnedInterest(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetEarnedUnearnedInterest(dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("posted-transactions/date")]
        public HttpResponseMessage PostTransactionsByStaffByDate(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = reportRepo.PostTransactionsByStaffByDate(dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



    }
}
