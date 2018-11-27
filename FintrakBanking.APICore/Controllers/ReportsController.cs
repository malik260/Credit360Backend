using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.Repositories.Credit;
using FintrakBanking.ViewModels.Reports;
//using RazorEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ReportObjects;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/report")]
    public class ReportsController : ApiControllerBase
    {
        IReportRoutes repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        IErrorLogRepository errorLogger;
        IFinanceTransactionsReport reportRepo;
        ILoanOperationsRepository flow;

        public ReportsController(IReportRoutes _repo, IFinanceTransactionsReport reportRepo, IErrorLogRepository _errorLogger,
            ILoanOperationsRepository _flow) {

            this.repo = _repo;
            this.reportRepo = reportRepo;
            errorLogger = _errorLogger;
            flow = _flow;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("workflowsla/loanapplication/{id}")]
        public HttpResponseMessage GetCollateralTypeByProduct(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowSLA(id, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("workflow/sla-monitoring")]
        public HttpResponseMessage GetSLAMonitoring(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowSLAMonitoring( token.GetCompanyId, dateRange);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet] [ClaimsAuthorization]  
        [Route("loans/loanschedule/{loanid}")]
        public HttpResponseMessage GetLoanScheduleReport(int loanid)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanScheduleReport(loanid, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("limitmonitoring/sector")]
        public HttpResponseMessage GetSectorLimitMonitoringReport()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetSectorLimitMonitoringReport(token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("limitmonitoring/branch")]
        public HttpResponseMessage GetBranchLoanAmountLimit()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetBranchLoanAmountLimit(token.GetBranchId, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("workflow-definition/operation/{id}")]
        public HttpResponseMessage GetWorkflowDefinition(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetWorkflowDefinition(id, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan-disbursedloans")]            
        public HttpResponseMessage GetDisburstLoans(DateRange dateRange) 
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetDisburstLoans(dateRange, token.GetCompanyId,token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("monitoring/expired-self-liquidating-loans")]
        public HttpResponseMessage GetSelfLiquidationLoans(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetSelfLiquidatingLoansReport(dateRange, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("monitoring/property-due-for-vistation")]
        public HttpResponseMessage GetCollateralpropertyDueForVisitation(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetCollateralPropertyDueForVisitationReport(token.GetCompanyId, dateRange, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #region Offer-Letter Generation & Loan Monitoring Reports

      [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("form3800b-los")]
        public HttpResponseMessage GetGeneratedForm3800bLos(string applicationRefNumber)
        {
            try
            {
                var data = repo.GetGeneratedFORM3800BLOS(applicationRefNumber);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("form3800b-lms")]
        //public HttpResponseMessage GetGeneratedForm3800bLMS(string applicationRefNumber)
        //{
        //    try
        //    {
        //        var data = repo.GetGeneratedFORM3800BLMS(applicationRefNumber);
        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, result = data });
        //    }
        //    catch (SecureException ex)
        //    {
        //        errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}
        [HttpGet]
        [ClaimsAuthorization]
        [Route("offer-letter-lms")]
        public HttpResponseMessage GetGeneratedOfferLetterLMS(string applicationRefNumber)
        {
            try
            {
                var data = repo.GetGeneratedOfferLetterLMS(applicationRefNumber);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost] [ClaimsAuthorization]
        [Route("monitoring/collateral-property-revaluation")]
        public HttpResponseMessage GetCollateralPropertyRevaluationReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetCollateralPropertyRevaluationReport(token.GetCompanyId, dateRange, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data }); 
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("monitoring/almost-due-covenants")]
        public HttpResponseMessage GetCovenantsApproachingDueDateReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetCovenantsApproachingDueDateReport(token.GetCompanyId, token.GetStaffId, dateRange);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpPost] [ClaimsAuthorization]  
        [Route("monitoring/non-performing-loans")]
        public HttpResponseMessage GetNonPerformingLoansReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetNonPerformingLoansReport(dateRange,token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpPost] [ClaimsAuthorization]  
        [Route("monitoring/overdraft-loans")]
        public HttpResponseMessage GetExpiredOverdraftLoansReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetExpiredOverdraftLoansReport(dateRange,token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("monitoring/bond-and-guarantee")]
        public HttpResponseMessage GetBondAndGuaranteeReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetBondAndGuaranteeReport(dateRange, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      
        [HttpPost]
        [ClaimsAuthorization]
        [Route("monitoring/insurance-expiration")]
        public HttpResponseMessage GetInsuranceExpirationReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetCollateralInsuranceReport(dateRange, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("monitoring/turnover-covenant")]
        public HttpResponseMessage GetTurnoverCovenantReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetTurnoverCovenantReport(dateRange, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                errorLogger.LogError(ex, Common.CommonHelpers.GetUserIP(), token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Offer-Letter Generation & Loan Monitoring Reports

        [HttpPost] [ClaimsAuthorization]
        [Route("loan-commercial")]
        public HttpResponseMessage GetLoanCommercialReport(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanCommercialReport(dateRange, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("posted-finance-transactions")]
        public HttpResponseMessage GetPostedTransactions(ReportSearchEntity searchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                searchEntity.staffId = token.GetStaffId;

                var data = repo.GetPostedTransactions(searchEntity, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("loan-team-revolving")]
        public HttpResponseMessage GetTeamAndRevolving(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetTeamAndRevolving(dateRange, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-earned-unearned-interest")]
        public HttpResponseMessage GetEarnedUnearnedInterest(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetEarnedUnearnedInterest(dateRange, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("audit-trail")]
        public HttpResponseMessage GetAuditTrail(DateRange dateRange)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetAuditTrail(dateRange, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("posted-transactions-staff/date")]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("posted-transactions-branch/date")]
        public HttpResponseMessage PostTransactionsByBranchByDate(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = reportRepo.PostTransactionsByBranchByDate(dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loanstatement/loan/{id}")]
        public HttpResponseMessage GetLoanStatement(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanStatement(token.GetCompanyId,id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-LoanAnniversery")]
        public HttpResponseMessage GetLoanAnniversery(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanAnniversery(dateRange, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan/document-waived")]
        public HttpResponseMessage GetLoanDocumentWaived(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                var data = repo.GetLoanDocumentWaived(token.GetCompanyId, dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan/document-deferrals")]
        public HttpResponseMessage GetLoanDocumentDeferrals(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                var data = repo.GetLoanDocumentDeferrals(token.GetCompanyId, dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loan/document-deferrals-mcc")]
        public HttpResponseMessage GetLoanDocumentDeferralsForMCC(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                var data = repo.GetLoanDocumentDeferralsMCC(token.GetCompanyId, dateRange, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // [HttpPost] [ClaimsAuthorization]
        //[Route("loan/collateral-estimated")]
        //public HttpResponseMessage GetCollateralEstimated(string acctNumber, string collateralCode)
        //{
        //    var token = new TokenDecryptionHelper();
        //    try
        //    {

        //        var data = repo.GetCollateralEstimated(token.GetCompanyId, acctNumber, collateralCode);
        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, result = data });  //Ok(accounts);
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

      [HttpGet] [ClaimsAuthorization]  
        [Route("collateralestimated/loan/{collateralCode}")]
        public HttpResponseMessage GetCollateralEstimated(string collateralCode)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetCollateralEstimated(token.GetCompanyId, collateralCode, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("fcyscheuledloan/loan/{id}")]
        public HttpResponseMessage GetFCYScheuledLoan(int id)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetFCYScheuledLoan(token.GetCompanyId, id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      
         [HttpPost] [ClaimsAuthorization]
        [Route("stakeholders-on-experation-ftp")]
        public HttpResponseMessage GetStakeHolderOnExperationOfFfp(ReportSearchEntity reportSearchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetStakeholdersOnExpirationOfFTP(reportSearchEntity, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan/facility-approved-not-utilized")]
        public HttpResponseMessage FacilityApprovedNotUtilized(ReportSearchEntity reportSearchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetFacilityApprovedNotUtilized(reportSearchEntity, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan/runing-loans-by-loantype")]
        public HttpResponseMessage RuningLoansByLoanType(ReportSearchEntity reportSearchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRuningLoansByLoanType(reportSearchEntity, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan/loan-interest-receivable-and-payable")]
        public HttpResponseMessage LoansInterestReceivabelAndPayable(ReportSearchEntity reportSearchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanInterestReceivableAndPayable(reportSearchEntity, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("loan/loan-repayment")]
        public HttpResponseMessage LoansRepaymentSchedule(ReportSearchEntity reportSearchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLoanInterestReceivableAndPayable(reportSearchEntity, token.GetCompanyId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("camsol/blacklist")]
        public HttpResponseMessage Blacklist(ReportSearchEntity reportSearchEntity)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetBlacklist(reportSearchEntity);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("daily-accrual/categories")]
        public HttpResponseMessage GetAllDailyAccrualCategories()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = reportRepo.GetAllDailyAccrualCategories();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-transaction/type")]
        public HttpResponseMessage GetAllLoanTransactionType()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = reportRepo.GetAllLoanTransactionType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("daily-accrual")]
        public HttpResponseMessage GetDailyAccrual([FromBody]ReportSearchEntity param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetDailyAccrual(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("repayment")]
        public HttpResponseMessage GetRepayment([FromBody]ReportSearchEntity param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetRepayment(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("custom-facility-repayment")]
        public HttpResponseMessage GetCustomeFacilityRepayment([FromBody]ReportSearchEntity param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCustomeFacilityRepayment(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("operations")]
        public HttpResponseMessage GetOperations()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = reportRepo.Operations();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("flow-type")]
        public HttpResponseMessage GetFlowType()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = flow.FlowTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("account-with-lien")]
        public HttpResponseMessage GetAccountWithLien([FromBody]ReportSearchEntity param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.AccountWithLein(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("audit-search")]
        public HttpResponseMessage GetAuditType(string searchQuery)
        {
            try
            {
                var data = repo.AuditType(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        
        [HttpGet]
        [ClaimsAuthorization]
        [Route("accountcode-search")]
        public HttpResponseMessage SearchForBranch(string searchQuery)
        {
                var data = repo.GLAccount(searchQuery);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("stalled-Perfection")]
        public HttpResponseMessage GetStalledPerfection([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetStalledPerfection(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("Collateral-Perfection-YetToCommence")]
        public HttpResponseMessage GetCollateralPerfectionYetToCommence([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCollateralPerfectionYetToCommence(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("all-comercial-loan-report")]
        public HttpResponseMessage GetAllCommercialLoanReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetAllCommercialLoanReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("unearned-interest-Report")]
        public HttpResponseMessage GetUnearnedLoanInterestReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetUnearnedLoanInterestReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("receivable-interest-Report")]
        public HttpResponseMessage GetReceivableInterestReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetReceivableInterestReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("cashbacked-Report")]
        public HttpResponseMessage GetCashBackedReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCashBackedReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("cashbacked-bond-guarantee")]
        public HttpResponseMessage GetCashBackedBondAndGuarantee([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCashBackedBondAndGuarantee(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("weeklyrecovery-Reportfor-FINCON")]
        public HttpResponseMessage GetweeklyrecoveryReportforFINCON([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetweeklyRecoveryReportforFINCON(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("cash-collaterized-credits")]
        public HttpResponseMessage GetCashCollaterizedCredits([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCashCollaterizedCredits(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("dropdown-product-class")]
        public HttpResponseMessage getAllproductClass()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = LoanReportObjects.GetProductClass();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("staff-priviledge-change-report")]
        public HttpResponseMessage GetStaffPrivilegeChangeReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetStaffPrivilegeChangeReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("user-group-change-report")]
        public HttpResponseMessage GetUserGroupChangeReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetUserGroupChangeReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("profile-activity-report")]
        public HttpResponseMessage GetProfileActivityReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetProfileActivityReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("staff-role-profile-group-report")]
        public HttpResponseMessage GetStaffRoleProfileGroupReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetStaffRoleProfileGroupReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("staff-role-profile-activity-report")]
        public HttpResponseMessage GetStaffRoleProfileActivityReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetStaffRoleProfileActivityReport(param);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("running-facilities")]
        public HttpResponseMessage GetRunningFacilities(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRunningFacilities(dateRange, token.GetCompanyId, token.GetStaffId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("inactive-contigent-liability-report")]
        public HttpResponseMessage GetInActiveContigentLiabilityReport()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetInActiveContigentLiabilityReport(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }
}



