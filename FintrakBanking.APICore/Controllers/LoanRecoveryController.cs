using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class LoanRecoveryController : ApiControllerBase
    {
        private ILoanRecoverySetupRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanRecoveryController(ILoanRecoverySetupRepository _repo)
        {
            repo = _repo;
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("addloanRecoverySetup")]
        public HttpResponseMessage AddLoanRecovery( [FromBody]LoanRecoverySetupViewModel entity)
        {
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Empty Record" });
            }

            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var LoanRecovery = repo.AddLoanRecoverySetup(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, message = "The record has been created successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
     
      [HttpGet] [ClaimsAuthorization]  
        [Route("loanRecoverySetup")]
        public HttpResponseMessage GetAllLoanRecovery()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllLoanRecoverySetup().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("product-type")]
        public HttpResponseMessage GetAllProductType()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllProductType().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("casa")]
        public HttpResponseMessage GetAllCasa()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllCasa().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("agent")]
        public HttpResponseMessage GetAllAgent()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllAgent().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("updateloanRecoverySetup/{recoveryPlanId}")]
        public HttpResponseMessage UpdateLoanRecovery(int recoveryPlanId, LoanRecoverySetupViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var data = repo.UpdateLoanRecoverySetup(recoveryPlanId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getloanRecoverySetup/{recoveryPlanId}")]
        public HttpResponseMessage GetLoanRecoverySetup (int recoveryPlanId)
        {
            var account = repo.GetLoanRecoverySetup(recoveryPlanId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.GetLoanRecoverySetup(recoveryPlanId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = depart });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("addloanRecoveryPaymentPlan")]
        public HttpResponseMessage AddLoanRecoveryPaymentPlan ([FromBody]LoanRecoverySetupViewModel entity)
        {
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Empty Record" });
            }

            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var LoanRecovery = repo.AddLoanRecoveryPaymentPlan(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, message = "The record has been created successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("updateloanRecoveryPaymentPlan/{recoveryPaymentPlanId}")]
        public HttpResponseMessage UpdateLoanRecoveryPaymentPlan(int recoveryPaymentPlanId, LoanRecoverySetupViewModel entity)
        {
            try
            { 
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var data = repo.UpdateLoanRecoveryPaymentPlan(recoveryPaymentPlanId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getDistinctLoanRecoveryPaymentPlan")]
        public HttpResponseMessage GetDistinctLoanRecoveryPaymentPlan()
        {
            var Message = string.Empty;
            try 
            {
                var LoanRecoveryPaymentPlan = repo.GetDistinctLoanRecoveryPaymentPlan().ToList();
                if (LoanRecoveryPaymentPlan.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecoveryPaymentPlan, count = LoanRecoveryPaymentPlan.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecoveryPaymentPlan found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getLoanRecoveryPaymentPlan/{recoveryPaymentPlanId}")]
        public HttpResponseMessage GetLoanRecoveryPaymentPlan(int recoveryPaymentPlanId)
        {
            var account = repo.GetLoanRecoveryPaymentPlan(recoveryPaymentPlanId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.GetLoanRecoveryPaymentPlan(recoveryPaymentPlanId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = depart });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getAllLoanRecoveryPaymentPlan")]
        public HttpResponseMessage GetAllLoanRecoveryPaymentPlan()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecoveryPaymentPlan = repo.GetAllLoanRecoveryPaymentPlan().ToList();
                if (LoanRecoveryPaymentPlan.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecoveryPaymentPlan, count = LoanRecoveryPaymentPlan.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-recovery-new")]
        public HttpResponseMessage AddLaonRecoveryPayment(LoanRecoveryPaymentViewModel model)
        {
            var Message = string.Empty;
            try
            {
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var success = repo.AddLaonRecoveryPayment(model);
                if (success)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = success });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-amount-paid/{loanReviewOperationId}")]
        public HttpResponseMessage GetLoanRecoverySearch(int loanReviewOperationId)
        {
            var Message = string.Empty;
            try
            {
                var LoanRecoveryPayment = repo.GetTotalRecoveryPayments(loanReviewOperationId);
                if (LoanRecoveryPayment != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecoveryPayment });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-payment-schedule/{loanReviewOperationId}")]
        public HttpResponseMessage GetAllLoanRecoveryPayments(int loanReviewOperationId)
        {
            var Message = string.Empty;
            try
            {
                var LoanRecoveryPaymentPlan = repo.GetRecoveryPaymentSchedule(loanReviewOperationId).ToList();
                if (LoanRecoveryPaymentPlan.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecoveryPaymentPlan, count = LoanRecoveryPaymentPlan.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-recovery-search/{searchValue}")]
        public HttpResponseMessage GetLoanRecoverySearch(string searchValue)
        {
            var Message = string.Empty;
            try
            {
                var LoanRecoveryPaymentPlan = repo.GetLoanRecoveryPayment(searchValue).ToList();
                if (LoanRecoveryPaymentPlan.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecoveryPaymentPlan, count = LoanRecoveryPaymentPlan.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("recovery-repayment-go-for-approval")]
        public HttpResponseMessage RecoveryPaymentGoForApproval(LoanRecoveryPaymentViewModel entity)
        {
            var Message = string.Empty;
            try
            {
                entity.staffId = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var data = repo.RecoveryPaymentGoForApproval(entity);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("recovery-repayment-approval")]
        public HttpResponseMessage LoanRecoveryPaymentWaitingForApproval()
        {
            var Message = string.Empty;
            try
            {
                var data = repo.LoanRecoveryPaymentWaitingForApproval(token.GetStaffId, token.GetCompanyId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


    }
}