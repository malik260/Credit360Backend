using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
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
        IFinanceTransactionsReport reportRepo;

        public ReportsController(IReportRoutes _repo, IFinanceTransactionsReport reportRepo) {

            this.repo = _repo;
            this.reportRepo = reportRepo;
        }

        [HttpGet]
        [Route("workflowsla/loanapplication/{id}")]
        public HttpResponseMessage GetCollateralTypeByProduct(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowSLA(id,token.GetCompanyId);
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
                var data = repo.GetBranchLoanAmountLimit( token.GetBranchId ,token.GetCompanyId);
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
        public HttpResponseMessage GetWorkflowDefinition( int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowDefinition(id,token.GetCompanyId);
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
