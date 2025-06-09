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
using System.Diagnostics;
using FintrakBanking.ViewModels;
using FintrakBanking.ReportObjects.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using FintrakBanking.ViewModels.Credit;

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
        ILoanRepository loanRepo;
        ICreditTemplateRepository creditTemplateRepo;
        OfferLetterInfo offerLetterRepo;
        public ReportsController(IReportRoutes _repo, IFinanceTransactionsReport reportRepo, IErrorLogRepository _errorLogger,
            ILoanOperationsRepository _flow, ILoanRepository _loanRepo, ICreditTemplateRepository _creditTemplateRepo) {

            this.repo = _repo;
            this.reportRepo = reportRepo;
            errorLogger = _errorLogger;
            flow = _flow;
            loanRepo = _loanRepo;
            creditTemplateRepo = _creditTemplateRepo;
            offerLetterRepo = new OfferLetterInfo();
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
        [Route("offer-letter-cfl")]
        public HttpResponseMessage GetGeneratedCFLOfferLetter(string applicationRefNumber)
        {
            try
            {

                //var data = repo.GetGeneratedCFLOfferLetter(applicationRefNumber, StatusCode, RequestId, WorkflowStage, ReasonForRejection, ActionByName);
                var data = repo.GetGeneratedCFLOfferLetter(applicationRefNumber, token.GetUsername);

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
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                var loanAppId = repo.GetLoanApplicationIdByReferenceNumber(applicationRefNumber);
                var data = creditTemplateRepo.GetSavedDocumentation(6, loanAppId);
                if (data.Count == 0)
                {
                    data = creditTemplateRepo.GetLoadedDocumentation(token.GetStaffId, 6, loanAppId, user);
                }
               // var data = repo.GetGeneratedFORM3800BLOS(applicationRefNumber);
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
        [Route("form3800b-lmsr")]
        public HttpResponseMessage GetGeneratedForm3800bLmsr(string applicationRefNumber)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.staffId = token.GetStaffId;
                user.companyId = token.GetCompanyId;
                var loanAppId = repo.GetLoanApplicationIdByReferenceNumberLMS(applicationRefNumber);
                var data = creditTemplateRepo.GetSavedDocumentation(46, loanAppId);
                if (data.Count == 0)
                {
                    data = creditTemplateRepo.GetLoadedDocumentation(token.GetStaffId, 46, loanAppId, user);
                }
                // var data = repo.GetGeneratedFORM3800BLOS(applicationRefNumber);
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
        [Route("form3800b-lms")]
        public HttpResponseMessage GetGeneratedForm3800bLMS(string applicationRefNumber)
        {
            try
            {
                var data = repo.GetGeneratedFORM3800BLMS(applicationRefNumber);
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
        //        var loanAppId = repo.GetLmsrApplicationIdByReferenceNumber(applicationRefNumber);
        //        var data = creditTemplateRepo.GetLoadedDocumentation(token.GetStaffId, 6, loanAppId);
        //        //var data = repo.GetGeneratedFORM3800BLMS(applicationRefNumber);
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
                var data = repo.GetCovenantsApproachingDueDateReport(token.GetStaffId, dateRange,token.GetCompanyId);
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
        [Route("monitoring/contingents-report")]
        public HttpResponseMessage GetAllContingentsReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetAllContingentsReport(dateRange, token.GetCompanyId, token.GetStaffId);
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

        //edited report diff from initial report
        [HttpPost]
        [ClaimsAuthorization]
        [Route("monitoring/bond-and-guarantee-report")]
        public HttpResponseMessage GetBondsAndGuaranteeReport(DateRange dateRange)
        {
            try
            {
                var data = repo.GetBondsAndGuaranteeReport(dateRange, token.GetCompanyId, token.GetStaffId);
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
                var data = repo.GetLoanStatement(token.GetCompanyId,id, token.GetStaffId);
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
        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan/document-deferred")]
        public HttpResponseMessage GetLoanDocumentDeferred(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                var data = repo.GetLoanDocumentDeferred(token.GetCompanyId, dateRange, token.GetCompanyId);
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
                var data = repo.GetFCYScheuledLoan(token.GetCompanyId, id, token.GetStaffId);
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
        [Route("monitoring/credit-bureau")]
        public HttpResponseMessage GetCreditBureau(DateRange dateRange)
        {
            try
            {
                var data = repo.GetCreditBureauReport(dateRange);
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
        [Route("corporate-loans-report")]
        public HttpResponseMessage CorporateLoansReport(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.CorporateLoansReport(dateRange);
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
        [Route("Collateral-Perfection")]
        public HttpResponseMessage GetCollateralPerfection([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCollateralPerfection(param);
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
        [Route("disbursed-facility-collateral-report")]
        public HttpResponseMessage GetDisbursedFacilityCollateralReport([FromBody] DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetDisbursedFacilityCollateralReport(param);
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
        [Route("Collateral-Register")]
        public HttpResponseMessage GetCollateralRegister([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCollateralRegister(param);
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
        [Route("collateral-adequacy")]
        public HttpResponseMessage GetCollateralAdequacy([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCollateralAdequacy(param);
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
                var data = repo.GetStakeholdersOnExpirationOfFTP(reportSearchEntity, token.GetCompanyId, token.GetStaffId);
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
        [Route("insider-related-loans")]
        public HttpResponseMessage InsiderRelatedLoansReport(DateRange dateRange)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.InsiderRelatedLoansReport(dateRange);
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
        [Route("monitoring/loan-status-report")]
        public HttpResponseMessage GetLoanStatusReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;
                var data = repo.GetLoanStatusReport(param, token.GetCompanyId, token.GetCompanyId);
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


        [HttpGet]
        [Route("dropdown-loan-status")]
        public HttpResponseMessage GetAllLoanStatus()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = loanRepo.GetAllLoanStatus();
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
                    new { success = true, result = data });  
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
        [HttpGet]
        [ClaimsAuthorization]
        [Route("job-request-report")]
        public HttpResponseMessage GetJobRequestReport(DateRange dateRange)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetJobRequestReport(dateRange, token.GetCompanyId, token.GetCompanyId);
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



        [HttpPost]
        [ClaimsAuthorization]
        [Route("inactive-contigent-liability-report")]
        public HttpResponseMessage GetInActiveContigentLiabilityReport(DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                param.companyId = token.GetCompanyId;
                var data = repo.GetInActiveContigentLiabilityReport(param);
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
        [Route("login-status")]
        public HttpResponseMessage GetLoggingStatus(DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {

                param.companyId = token.GetCompanyId;
                var data = repo.GetLoggingStatus(param);
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
        [Route("middle-office-report")]
        public HttpResponseMessage GetMiddleOfficeReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetMiddleOfficeReport(param);
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
        [Route("analyst-report")]
        public HttpResponseMessage GetAnalystReport(DateRange dateRange)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetAnalystReport(dateRange, token.GetCompanyId, token.GetCompanyId);
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
        [Route("collateral-valuation-report")]
        public HttpResponseMessage GetCollateralValuationReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCollateralValuationReport(param);
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
        [Route("loan-classification-report")]
        public HttpResponseMessage GetLoanClassificationReport(DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetLoanClassificationReport(param);
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
        [Route("age-analysis-report")]
        public HttpResponseMessage GetAgeAnalysisReport(DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetAgeAnalysisReport(param);
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
        [Route("credit-schedule-report")]
        public HttpResponseMessage GetCreditScheduleReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetCreditScheduleReport(param);
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
        [Route("sanction-limit-report")]
        public HttpResponseMessage GetSanctionLimitReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                

                param.companyId = token.GetCompanyId;

                var data = repo.GetSanctionLimitReport(param);
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
        [Route("impaired-watch-list-report")]
        public HttpResponseMessage GetImpairedWatchListReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {


                param.companyId = token.GetCompanyId;

                var data = repo.GetRuniningLoanReport(param);
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
        [Route("insurance-report")]
        public HttpResponseMessage GetInsuranceReport()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetInsuranceReport(token.GetCompanyId);
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
        [Route("expired-report")]
        public HttpResponseMessage GetExpiredReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {


                param.companyId = token.GetCompanyId;

                var data = repo.GetExpiredReport(param);
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
        [Route("excess-report")]
        public HttpResponseMessage GetExcessReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {


                param.companyId = token.GetCompanyId;

                var data = repo.GetExcessReport(param);
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
        [Route("unutilized-facility-report")]
        public HttpResponseMessage GetUnutilizedFacilityReport()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetUnutilizedFacilityReport(token.GetCompanyId);
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
        [Route("runining-loan-report")]
        public HttpResponseMessage GetRuniningLoanReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {


                param.companyId = token.GetCompanyId;

                var data = repo.GetRuniningLoanReport(param);
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
        [Route("disbursal-credit-turnover")]
        public HttpResponseMessage GetDisbursalCreditTurnover([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {


                param.companyId = token.GetCompanyId;

                var data = repo.GetDisbursalCreditTurnover(param);
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
        [Route("loan-booking-report")]
        public HttpResponseMessage GetLoanBookingReport([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.GetLoanBookingReport(param);
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
        [Route("form3800b-report")]
        public HttpResponseMessage Form3800BApprovedFacility([FromBody]DateRange param)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                param.companyId = token.GetCompanyId;

                var data = repo.Form3800BApprovedFacility(param);
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
        [AllowAnonymous]
        [Route("output-document/loanApplicationId/{loanApplicationId}")]
        public HttpResponseMessage GetOutputDocument(int loanApplicationId)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetOutPutDocument(loanApplicationId);
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
        [AllowAnonymous]
        [Route("original-document-submission")]
        public HttpResponseMessage SubmissionOfOriginalDocument([FromBody]DateRange param)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.SubmissionOfOriginalDocument(param);
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
        [AllowAnonymous]
        [Route("submission-of-original-documents")]
        public HttpResponseMessage SubmissionOfOriginalDocuments([FromBody] DateRange param)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.SubmissionOfOriginalDocuments(param);
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
        [AllowAnonymous]
        [Route("corporate-customer-creation")]
        public HttpResponseMessage CorporateCustomerCreation([FromBody] DateRange param)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.CorporateCustomerCreation(param);
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("insurance-spool-report")]
        public HttpResponseMessage InsuranceSpoolReport([FromBody] DateRange param)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.InsuranceSpoolReport(param);
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("security-release-report")]
        public HttpResponseMessage SecurityReleaseReport([FromBody] DateRange param)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.SecurityReleaseReport(param);
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
        [AllowAnonymous]
        [Route("psr-report")]
        public HttpResponseMessage PSR([FromBody]DateRange param)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.PSR(param.psrReportTypeId, param.projectSiteReportId);
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
        [Route("revaluation-report")]
        public HttpResponseMessage GetCollateralRevaluation(DateRange dateRange)
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("risk-assets-report")]
        public HttpResponseMessage GetRiskAssets([FromBody] RiskAssets obj)
        {
           
            var token = new TokenDecryptionHelper();
            try
            {
               var data = repo.RiskAssets(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("interest-income-report")]
        public HttpResponseMessage GetInterestIncomeReport([FromBody] InterestIncome obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetInterestIncome(obj.startDate, obj.endDate);
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
        [Route("get-fixed-deposit-collateral-report/customerCode/{customerCode}")]

        public HttpResponseMessage GetFixedDepositCollateralsReport(string customerCode)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetFixedDepositCollaterals(token.GetCompanyId, customerCode, token.GetCompanyId);
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("get-valid-collaterals-report")]
        public HttpResponseMessage GetValidCollateralsReport([FromBody] InterestIncome obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetValidCollaterals(obj.startDate, obj.endDate);
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("cbn-npl-team-report")]
        public HttpResponseMessage GetcbnNplTeamReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.CbnTeam(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-by-cbn-classification")] 
        public HttpResponseMessage RiskAssetByCbnNplClassification([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetByCbnNplClassification(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("contigent-liability-report")]
        public HttpResponseMessage ContigentLiabilityReportMain([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.ContigentLiabilityReportMain(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("contigent-liability")]
        public HttpResponseMessage ContigentLiabilityReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper(); 
            try
            {
                var data = repo.ContigentLiabilityReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("contigent-liability-main")]
        public HttpResponseMessage ContigentLiabilityReportMain1([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.ContigentLiabilityReportMain1(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("copy-of-risk-asset-main")]
        public HttpResponseMessage CopyOfRiskAssetMain([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.CopyOfRiskAssetMain(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("calc-combine")]
        public HttpResponseMessage RiskAssetCalcCombinedReportTeam([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetCalcCombinedReportTeam(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-contigent")]
        public HttpResponseMessage RiskAssetsContigentReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetsContigentReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-calc-combine")]
        public HttpResponseMessage RiskAssetCalcCombinedReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetCalcCombinedReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("contigent-report")]
        public HttpResponseMessage GetContigentReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.ContigentReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("risk-calc-com-report")]
        //public HttpResponseMessage GetriskAssetCalcComReport([FromBody] RiskAssets obj)
        //{

        //    var token = new TokenDecryptionHelper();
        //    try
        //    {
        //        var data = repo.RiskAssetCalcCombinedReportTeam(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("overline-report")]
        public HttpResponseMessage GetOverline([FromBody] Overline obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.Overline(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("expired-facility-report")]
        public HttpResponseMessage GetExpiredFacilityReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.ExpiredFacilityReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("over-line-report")]
        public HttpResponseMessage GetOverLineReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.OverLineReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("large-exposure-report")]
        public HttpResponseMessage GetLargeExposureReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.LargeExposureReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("extension-report")]
        public HttpResponseMessage GetExtensionReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.ExtensionReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("maturity-report")]
        public HttpResponseMessage GetMaturityReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.MaturityReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("ifrs-classification-report")]
        public HttpResponseMessage GetIfrsClassificationTeamReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.IfrsClassificationReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("rest-classification-report")]
        public HttpResponseMessage GetRiskAssetByIFRSClassificationReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetByIFRSClassificationReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-variance-report")]
        public HttpResponseMessage GetRiskAssetByVarianceReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetByVarianceReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-combined-report")]
        public HttpResponseMessage GetRiskAssetCombinedReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetCombinedReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-distribution-report")]
        public HttpResponseMessage GetRiskAssetDistributionBySectorReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetDistributionBySectorReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-main-report")]
        public HttpResponseMessage GetRiskAssetMainReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetMainReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-main1-report")]
        public HttpResponseMessage GetRiskAssetMain1Report([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetMain1Report(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-asset-team-report")]
        public HttpResponseMessage GetRiskAssetTeamReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetTeamReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("unpaid-obligation-report")]
        public HttpResponseMessage GetUnpaidObligationReport([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.UnpaidObligationReport(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("risk-contigent-main")]
        public HttpResponseMessage GetRiskAssetContigentReportMain([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.RiskAssetContigentReportMain(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("copy-ifrs-classification")]
        public HttpResponseMessage CopyOfRiskAssetByIfrsClassification([FromBody] RiskAssets obj)
        {

            var token = new TokenDecryptionHelper(); 
            try
            {
                var data = repo.CopyOfRiskAssetByIfrsClassification(obj.runDate, obj.level, obj.misCode, obj.exposureType, obj.divisionName, obj.groupName, obj.branchName, obj.regionName);
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
        [Route("get-drawdown-memo-pdf/{referenceNumber}")]
        public HttpResponseMessage GetDrawdownMemoPdf([FromUri] string referenceNumber)
        {
            try
            {
                var response = repo.DrawdownReport(referenceNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [ClaimsAuthorization]
        [Route("get-deferralwaiver-memo-pdf/{operationId}/{targetId}/{loanApplicationDetailId}")]
        public HttpResponseMessage GetDeferralWaiverMemoPdf([FromUri] int operationId, [FromUri] int targetId, [FromUri] int loanApplicationDetailId)
        {
            try
            {
                var response = repo.DeferralWaiverReport(token.GetStaffId, operationId, targetId, loanApplicationDetailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "", result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/getSignatory/{referenceNumber}")]
        public List<SignatoryViewModel> GetSignatory([FromUri] string referenceNumber)
        {
            try
            {
                var response = offerLetterRepo.GetLoanApplicationSignatory(referenceNumber);
                return response;
            }
            catch (SecureException ex)
            {
                return new List<SignatoryViewModel>();
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/generate-offerletter/{referenceNumber}")]
        public IEnumerable<OfferLetterViewModel> GenerateOfferLetter([FromUri] string referenceNumber)
        {

            var response = offerLetterRepo.GenerateOfferLetter(referenceNumber);
            return response;

        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/get-loan-collateral/{referenceNumber}")]
        public List<LoanApplicationCollateralViewModel> GetLoanCollateral([FromUri] string referenceNumber)
        {
            try
            {
                var response = offerLetterRepo.GetLoanCollateral(referenceNumber);
                return response;
            }
            catch (SecureException ex)
            {
                return new List<LoanApplicationCollateralViewModel>();
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/get-application-fee/{referenceNumber}")]
        public List<ProductFeeViewModel> GetLoanApplicationFee([FromUri] string referenceNumber)
        {
            try
            {
                var response = offerLetterRepo.GetLoanApplicationFee(referenceNumber);
                return response;
            }
            catch (SecureException ex)
            {
                return new List<ProductFeeViewModel>();
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/get-application-detail/{referenceNumber}")]
        public List<OfferLetterDetailViewModel> GetLoanApplicationDetail([FromUri] string referenceNumber)
        {
            try
            {
                var response = offerLetterRepo.GetLoanApplicationDetail(referenceNumber);
                return response;
            }
            catch (SecureException ex)
            {
                return new List<OfferLetterDetailViewModel>();
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/get-application-condition-precedent/{referenceNumber}")]
        public IEnumerable<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionPrecident([FromUri] string referenceNumber)
        {

            var response = offerLetterRepo.GetLoanApplicationConditionPrecident(referenceNumber);
            return response;


        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("offerletter/los-condition-dynamics/{referenceNumber}")]
        public List<TransactionDynamicsViewModel> Los_ConditionDynamics([FromUri] string referenceNumber)
        {

            var response = offerLetterRepo.Los_ConditionDynamics(referenceNumber);
            return response;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("trial-balance/{glAccountId}/currency/{currencyCode}")]

        public HttpResponseMessage GetTrialBalanceReport(int glAccountId, int currencyCode)

        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetTrialBalanceReport(glAccountId, currencyCode, token.GetCompanyId, token.GetStaffId);
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

        // remedial asset report

        [HttpPost]
        [ClaimsAuthorization]
        [Route("out-of-court-settlement-report")]
        public HttpResponseMessage GetOutOfCourtSettlementReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetOutOfCourtSettlement(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }else
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data });  
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("collateral-sales-report")]
        public HttpResponseMessage GetCollateralSalesReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetCollateralSales(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-agent-update-report")]
        public HttpResponseMessage GetRecoveryAgentUpdateReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRecoveryAgentUpdate(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-commission-report")]
        public HttpResponseMessage GetRecoveryCommissionReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRecoveryCommission(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-agent-performance-report")]
        public HttpResponseMessage GetRecoveryAgentPerformanceReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRecoveryAgentPerformance(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("litigation-recoveries-report")]
        public HttpResponseMessage GetLitigationRecoveriesReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetLitigationRecoveries(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("revalidation-of-full-and-final-settlement-report")]
        public HttpResponseMessage GetRevalidationOfFullAndFinalSettlementReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRevalidationOfFullAndFinalSettlement(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("idle-assets-sales-report")]
        public HttpResponseMessage GetIdleAssetsSalesReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetIdleAssetsSales(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("full-and-final-settlement-and-waivers-report")]
        public HttpResponseMessage GetFullAndFinalSettlementAndWaiversReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetFullAndFinalSettlementAndWaivers(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-delinquent-accounts-report")]
        public HttpResponseMessage GetRecoveryDelinquentAccountsReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRecoveryDelinquentAccountsReport(obj.startDate, obj.endDate, obj.dpd, obj.amount);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        

      [HttpPost]
        [ClaimsAuthorization]
        [Route("payday-loan-recovery-collection-report")]
        public HttpResponseMessage GetPaydayLoanRecoveryCollectionReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetPaydayLoanRecoveryCollectionReport(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("computation-for-external-agents-report")]
        public HttpResponseMessage GetComputationForExternalAgentsReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetComputationForExternalAgentsReport(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-collection-report")]
        public HttpResponseMessage GetRecoveryCollectionReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetRecoveryCollectionReport(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        

        [HttpPost]
        [ClaimsAuthorization]
        [Route("computation-for-internal-agents-report")]
        public HttpResponseMessage GetComputationForInternalAgentsReport([FromBody] RemedialAssetReport obj)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetComputationForInternalAgentsReport(obj.startDate, obj.endDate);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("outstanding-document-deferred-list-report")]
        public HttpResponseMessage GetOutstandingDocumentDeferralList()
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetOutstandingDocumentDeferredList();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("availment-utilization-ticket")]
        public HttpResponseMessage GetAvailmentUtilizationTicketReport(int customerId)
        {
            var token = new TokenDecryptionHelper();
            try
            {
                var data = repo.GetAvailmentUtilizationTicketReport(customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }
}



