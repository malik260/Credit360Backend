using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LoanApplicationController : ApiControllerBase
    {
        private ILoanApplicationRepository repo;
        //private ILoanRepository loanRepository;
        //private ICreditLimitValidationsRepository creditLimitValidationsRepository;
        private ILoanPreliminaryEvaluationRepository repoLoanPEN;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        //private IErrorLogRepository errorLogger;
        //private IRepaymentTermsRepository repaymentRepo;

        public LoanApplicationController(
            ILoanApplicationRepository _repo,
            // ILoanRepository _loanRepository,
            // ICreditLimitValidationsRepository _creditLimitValidationsRepository,
            ILoanPreliminaryEvaluationRepository _repoLoanPEN
            //IErrorLogRepository _errorLogger
            // IRepaymentTermsRepository _repaymentRepo
            )
        {
            this.repo = _repo;
            //  this.loanRepository = _loanRepository;
            // this.creditLimitValidationsRepository = _creditLimitValidationsRepository;
            repoLoanPEN = _repoLoanPEN;

            // repaymentRepo = _repaymentRepo;
        }

        #region Loan Application

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application")]
        public HttpResponseMessage GetAllLoanApplications()
        {
            try
            {
                var response = repo.GetAllLoanApplications(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet, Route("loan-application/operation/{operationId}/class/{classId}")]
        public HttpResponseMessage GetLoanApplicationsByOperation(int operationId, int? classId)
        {
            try
            {
                IQueryable<LoanApplicationViewModel> items;

                items = repo.GetLoanApplicationsByOperation(operationId, classId, token.GetBranchId, token.GetStaffId);

                var data = items.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/customer/{id}")]
        public HttpResponseMessage ExistingLoanApplication(int id)
        {
            try
            {
                var response = repo.ExistingLoanApplication(id, token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("check-exiting-certificate-of-ownership/{certificateofownership}")]
        public HttpResponseMessage CheckExitingCertificateOfOwnership(string certificateofownership)
        {
            try
            {
                var response = repo.CheckExistingCertificateOfOwnership(certificateofownership, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/{loanApplicationId}")]
        public HttpResponseMessage GetLoanApplicationById([FromUri] int loanApplicationId)
        {
            try
            {
                var response = repo.GetLoanApplicationById(loanApplicationId, token.GetCompanyId);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("single-loan-application/{loanApplicationId}")]
        public HttpResponseMessage GetSingleLoanApplicationById([FromUri] int loanApplicationId)
        {
            try
            {
                var response = repo.GetSingleLoanApplicationById(loanApplicationId, token.GetCompanyId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-detail/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationDetailById([FromUri] int loanApplicationDetailId)
        {
            try
            {
                var response = repo.GetLoanApplicationDetailById(loanApplicationDetailId, token.GetCompanyId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }



        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-detail/application/{loanApplicationId}")]
        public HttpResponseMessage GetLoanApplicationDetailByLoanApplicationId([FromUri] int loanApplicationId)
        {
            try
            {
                var response = repo.GetLoanApplicationDetailByLoanApplicationId(loanApplicationId, token.GetCompanyId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-list")]
        public HttpResponseMessage GetLoanApplicationByRelationshipOfficerId()
        {
            var data = repo.GetLoanApplicationByRelationshipOfficerId(token.GetStaffId, token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("send-to-edit/{loanApplicationId}/{operationId}")]
        public HttpResponseMessage SendApplicationToEdit(int loanApplicationId, int operationId)
        {
            //try
            //{
            var data = repo.SendApplicationToEdit(loanApplicationId, operationId, token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Sent To Applications List For Modifications" });
            //}

            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            //}
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-info/application/{id}")]
        public HttpResponseMessage GetLoanApplicationInfo(int id)
        {
            try
            {
                var data = repo.GetLoanApplicationById(id, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpGet] [ClaimsAuthorization]  
        //  [Route("loan-application/{id}")]
        //  public HttpResponseMessage GetLoanAppById(int id)
        //  {
        //      try
        //      { 
        //          var data = repo.GetLoanAppById(id, token.GetCompanyId);

        //          if (data == null)
        //          {
        //              return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //          }

        //          return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //      }
        //      catch (SecureException e)
        //      {
        //          return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //      }
        //  }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-eligibility/loanApplicationId/{id}")]
        public HttpResponseMessage GetLoanApplicationsDetails(int id)
        {
            try
            {
                var data = repo.GetLoanApplicationsDetails(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-eligibility/loanApplicationDetailId/{id}")]
        public HttpResponseMessage GetSingleLoanApplicationsDetails(int id)
        {
            try
            {
                var data = repo.GetSingleLoanApplicationsDetails(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-details/loanApplicationId/{id}")]
        public HttpResponseMessage GetAllLoanApplicationsDetails(int id)
        {
            try
            {
                var data = repo.GetAllLoanApplicationsDetailsById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-applications-details")]
        public HttpResponseMessage GetLoanApplicationByRelationshipOfficerId([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var response = repo.GetLoanApplicationByRelationshipOfficerId(token.GetStaffId, token.GetCompanyId);

                var data = response.OrderByDescending(c => c.loanApplicationId)
                      .Take(itemsPerPage)
                      .Skip(page)
                      .ToList();
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-product-class")]
        public HttpResponseMessage GetProductClass()
        {
            try
            {
                var response = repo.GetProductClass();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpGet] [ClaimsAuthorization]  
        //  [Route("loan-application/search/{searchCriteria}")]
        //  public HttpResponseMessage FindLoan(string searchCriteria)
        //  {
        //      try
        //      {
        //          var response = repo.FindLoanApplication(searchCriteria, token.GetCompanyId);

        //          return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        //      }
        //      catch (SecureException e)
        //      {
        //          return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //      }
        //  }

        [HttpGet]
        [Route("customer-by-application/{applicationId}/{processtype}")]
        public HttpResponseMessage GetCustomerByApplicationId(int applicationId, string processtype)
        {
            try
            {
                var status = repo.GetCustomerByApplicationId(applicationId, processtype);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("customer-transactions/{customerId}/{applicationId}")]
        public HttpResponseMessage GetCustomerTransactions(int customerId, int applicationId)
        {
            try
            {
                var status = repo.GetCustomerTransactions(customerId, applicationId, false);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("customer-transactions-accounts/{customerId}/{applicationId}")]
        public HttpResponseMessage GetCustomerTransactionsAccounts(int customerId, int applicationId)
        {
            try
            {
                var status = repo.GetCustomerTransactionsAccounts(customerId, applicationId, false);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("lms-customer-transactions-filtered/{customerId}/{applicationId}/{accountnumber}/{fromYear}/{fromMonth}/{toYear}/{toMonth}")]
        public HttpResponseMessage GetLmsCustomerTransactions(int customerId, int applicationId, string accountnumber, int? fromYear, int fromMonth, int? toYear, int toMonth)
        {
            try
            {
                var status = repo.GetCustomerTransactionsFiltered(customerId, applicationId, accountnumber, fromYear, fromMonth, toYear, toMonth, true);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("customer-transactions-filter/{customerId}/{applicationId}/{froma}/{to}/{fYear}/{tYear}")]
        public HttpResponseMessage GetCustomerTransactionsByFilter(int customerId, int applicationId, int froma, int to, int fYear, int tYear)
        {
            try
            {
                var status = repo.GetCustomerTransactionsByFilterLogic(customerId, applicationId, froma, to, fYear, tYear, false);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("retail-recovery-report/{startDate}/{endDate}/{accreditedConsultantId}/{customer}")]
        public HttpResponseMessage GetRetailRecoveryReporting([FromUri] DateTime startDate, [FromUri] DateTime endDate, [FromUri] string customer, [FromUri] int accreditedConsultantId)
        {
                var records = repo.GetRetailRecoveryReporting(startDate, endDate, accreditedConsultantId, customer);
            if (records != null) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = records });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record(s) found" });
            }
            
        }




        [HttpGet]
        [Route("customer-ratios/{customerId}/{applicationId}")]
        public HttpResponseMessage GetCustomerRatios(int customerId, int applicationId)
        {
            try
            {
                var status = repo.GetCustomerRatios(customerId, applicationId, false);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("lms-customer-transactions/{customerId}/{applicationId}")]
        public HttpResponseMessage GetLmsCustomerTransactions(int customerId, int applicationId)
        {
            try
            {
                var status = repo.GetCustomerTransactions(customerId, applicationId, true);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = status });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application")]
        public HttpResponseMessage UpdateApprovalStatusForApplication([FromBody] int id)
        {
            var responseMessage = string.Empty;

            //model.applicationUrl = HttpContext.Current.Request.Path;
            //model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            //model.userBranchId = (short)token.GetBranchId;
            //model.createdBy = token.GetStaffId;
            //model.companyId = token.GetCompanyId;
            //model.branchId = (short)token.GetBranchId;

            var response = repo.UpdateApprovalStatusForApplication(id, token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("load-customer-turnover/{loanApplicationId}")]
        public HttpResponseMessage LoadCustomerTurnover(int loanApplicationId)
        {
            try
            {
                repo.LoadCustomerTurnover(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Successful!" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, errorCode = "99" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Core Banking API error, Failed to Load Customer Turnover!" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-ratios-basel/{loanApplicationId}")]
        public HttpResponseMessage GetCustomerRatiosFromBasel(int loanApplicationId)
        {
            try
            {
                repo.GetCustomerRatiosFromBasel(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Successful!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Core Banking API error, Failed to Load Customer Ratios!" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-group-ratios-basel/{loanApplicationId}")]
        public HttpResponseMessage GetCustomerGroupRatiosFromBasel(int loanApplicationId)
        {
            try
            {
                repo.GetCustomerGroupRatiosFromBasel(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Successful!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Core Banking API error, Failed to Load Customer Group Ratios!" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-corporate-customer-rating-basel/{loanApplicationId}")]
        public HttpResponseMessage GetCorporateCustomerRatingFromBasel(int loanApplicationId)
        {
            try
            {
                repo.GetCorporateCustomerRatingFromBasel(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Successful!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Core Banking API error, Failed to Load Corporate Customer Rating!" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-facility-rating-basel/{loanApplicationId}")]
        public HttpResponseMessage GetFacilityRatingFromBasel(int loanApplicationId)
        {
            try
            {
                repo.GetFacilityRatingFromBasel(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Successful!" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Core Banking API error, Failed to Load Facility Rating!" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("load-customer-turnover-lms/{loanApplicationId}")]
        public HttpResponseMessage LoadCustomerTurnoverLms(int loanApplicationId)
        {
            try
            {
                repo.LoadCustomerTurnoverLms(loanApplicationId, token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = "Successful!" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, errorCode = "99" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Core Banking API error, Failed to Load Customer Turnover!" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-facility-rating/{loanApplicationDetailId}")]
        public HttpResponseMessage GetFacilityRating(int loanApplicationDetailId)
        {
            try
            {
                var facility = repo.GetFacilityRating(loanApplicationDetailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = facility });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("loan-application-for-cam")]
        public HttpResponseMessage SubmitLoanApplicationForCam([FromBody] LoanApplicationUpdateViewModel loan)
        {
            try
            {
                var responseMessage = string.Empty;
                var feed = repo.SubmitLoanApplicationForCam(loan.applicationId, token.GetStaffId, loan.checkListIndex);
                var response = (feed == 1 || feed == 2) ? true : false;
                var jumptoDrawdown = feed == 2;
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, jumptoDrawdown = jumptoDrawdown, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-loan-application")]
        public HttpResponseMessage UpdateLoanApplicationDetails([FromBody]LoanApplicationDatailViewModel entity)
        {
            try
            {

                var user = new UserInfo
                {
                    BranchId = (short)token.GetBranchId,
                    createdBy = token.GetStaffId,
                    companyId = token.GetCompanyId,
                };


                var response = repo.UpdateLoanApplicationDetails(entity, user);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application completed successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)

            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error Occured =>  {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-product-fees/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationProductFees(int loanApplicationDetailId)
        {
            try
            {
                var response = repo.GetLoanApplicationProductFees(loanApplicationDetailId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/collateralrequirement/{applicationId}/{collateralCurrencyId}")]
        public HttpResponseMessage GetCollateralRequirements(int applicationId, int? collateralCurrencyId)
        {
            try
            {
                var response = repo.GetCollateralRequirements(applicationId, collateralCurrencyId, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/validate-application")]
        public HttpResponseMessage ValidateDuplicateLoanApplication([FromBody] LoanApplicationViewModel entity)
        {
            var isDuplicate = repo.ValidateDuplicateLoanApplication(entity);
            if (isDuplicate)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = isDuplicate, result = isDuplicate });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = isDuplicate });
        }

        [HttpPost]
        // [ClaimsAuthorization]
        [Route("loan/application")]
        public HttpResponseMessage AddLoanApplication([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                var loanDetail = entity.LoanApplicationDetail;
                string msg = "";
                if (entity.productClassId == (short)ProductClassEnum.BondAndGuarantees)
                {
                    foreach (var item in loanDetail)
                    {
                        var bond = item.bondDetails;
                        if (bond == null)
                        {
                            msg = "Kindly Enter Records Into Compulsary Fields";
                            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{msg}" });
                        }
                    }

                }
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;  //FinTrakBankingContext
                entity.branchId = (short)token.GetBranchId;

                entity.misCode = "001";
                entity.teamMisCode = "004";
                //if( entity.LoanApplicationDetail.Count == 0)
                //     return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No facility detail is provided" });

                var response = repo.AddLoanApplication(entity);
                if (response != null)
                {
                    if (response.jumpedDestination) { return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application completed successfully. proceeds to drawdown." }); }

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application completed successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });

            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-reference-number")]
        public HttpResponseMessage GetRefrenceNumber()
        {
            try
            {
                var response = repo.GetRefrenceNumber();
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application completed successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{e.Message}" });
            }
        }

        //[HttpGet]
        //[Route("loan-application/job")]
        //public HttpResponseMessage GetLoanApplicationJobs(int page, int itemsPerPage, int level, int scope)
        //{
        //    try
        //    {
        //        var response = repo.GetLoanApplicationJobs(token.GetCompanyId, level, scope);

        //        int totalItems = response.Count();

        //        response = response
        //            .Skip(page).Take(itemsPerPage)
        //            .ToList();

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, totalItems = totalItems, message = "Empty result" });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

        #endregion Loan Application

        #region Loan Preliminary Evaluation

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan/preliminary-evaluation")]
        public async Task<HttpResponseMessage> AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model)
        {
            try
            {
                var responseMessage = string.Empty;

                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;

                if (model.sendForEvaluation)
                {
                    model.isCurrent = true;
                }
                else
                {
                    model.isCurrent = false;
                }

                var response = await repoLoanPEN.AddPreliminaryEvaluation(model);

                if (response != null)
                {
                    if (response.sendForEvaluation)
                    {
                        responseMessage = $"Preliminary evaluation note ({response.preliminaryEvaluationCode}) created successfully, now awaiting approval";
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, message = $"{responseMessage}" });
                    }
                    else
                    {
                        responseMessage = $"Preliminary evaluation note ({response.preliminaryEvaluationCode}) saved successfully";
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, message = $"{responseMessage}" });
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Preliminary evaluation note not created" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: Preliminary Evaluation Note failed to save." });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan/preliminary-evaluation/approval")]
        public HttpResponseMessage ApprovePreliminaryEvaluation(ApprovalViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = (short)token.GetBranchId;
                model.staffId = token.GetStaffId;

                var data = repoLoanPEN.GoForApproval(model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                            new { success = true, message = "Preliminary evaluation note has been approved successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-preliminary-evaluation/loan-type/{loanTypeId}")]
        public HttpResponseMessage GetAllLoanPreliminaryEvaluationsByLoanType(int loanTypeId)
        {
            try
            {
                var data = repoLoanPEN.GetLoanPreliminaryEvaluationsByLoanTypeId(loanTypeId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-preliminary-evaluation/application/{applicationId}")]
        public HttpResponseMessage GetLoanApplicationPreliminaryEvaluations(int applicationId)
        {
            var data = repoLoanPEN.GetLoanApplicationPreliminaryEvaluations(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-preliminary-evaluation-mapped-to-application")]
        public HttpResponseMessage GetLoanPreliminaryEvaluationMappedToApplication()
        {
            var data = repoLoanPEN.GetLoanPreliminaryEvaluationMappedToApplication();

            if (!data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data.ToList() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        //[Route("customer-pen-code")]
        [Route("customer-pen-code/{customerId}/{loanTypeId}/{customerGroupId}")]

        public HttpResponseMessage GetCustomerLoanPreliminaryEvaluations(int customerId, int loanTypeId, int customerGroupId = 0)
        {
            try
            {
                var data = repoLoanPEN.GetCustomerLoanPreliminaryEvaluations(customerId, loanTypeId, customerGroupId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, count = data.Count(), result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/preliminary-evaluation/awaiting-approval/loan-type/{loanTypeId}")]
        public HttpResponseMessage GetLoanPreliminaryEvaluationsForAppprovalByLoanType(int loanTypeId)
        {
            try
            {
                var data = repoLoanPEN.GetLoanPreliminaryEvaluationsAwaitingApprovalByLoanTypeId(token.GetStaffId, token.GetCompanyId, loanTypeId);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("loan/preliminary-evaluation/{loanPenId}")]
        public async Task<HttpResponseMessage> UpdateLoanPreliminaryEvaluation(int loanPenId, LoanPreliminaryEvaluationViewModel model)
        {
            try
            {
                var responseMessage = string.Empty;

                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;

                responseMessage = "Preliminary evaluation note updated successfully";

                if (model.sendForEvaluation)
                {
                    model.isCurrent = true;
                    responseMessage = "Preliminary evaluation note updated successfully, now awaiting approval";
                }
                else
                {
                    model.isCurrent = false;
                }

                var response = await repoLoanPEN.UpdatePreliminaryEvaluation(loanPenId, model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{responseMessage}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Preliminary evaluation note not updated" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("loan/preliminary-evaluation/{loanPenId}/loan-application")]
        public HttpResponseMessage SendPreliminaryEvaluationForLoanApplication(int loanPenId, LoanPreliminaryEvaluationViewModel model)
        {
            try
            {
                var responseMessage = string.Empty;

                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.branchId = (short)token.GetBranchId;

                responseMessage = "Preliminary evaluation note updated successfully";

                var response = repoLoanPEN.SendPreliminaryEvaluationForLoanApplication(loanPenId, model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{responseMessage}" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Preliminary evaluation note not updated" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion Loan Preliminary Evaluation

        #region Loan Collateral

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/collateral")]
        public HttpResponseMessage SaveLoanApplicationCollateral([FromBody] List<LoanApplicationCollateralViewModel> entity)
        {
            try
            {
                foreach (var item in entity)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.companyId = token.GetCompanyId;
                    item.createdBy = token.GetStaffId;
                    item.applicationUrl = HttpContext.Current.Request.Path;
                    item.userIPAddress = Request.RequestUri.Host;
                    item.createdBy = token.GetStaffId;
                }


                if (entity != null)
                {
                    var response = repo.AddLoanApplicationCollateral(entity);

                    if (response)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Collateral saved successfully" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Collateral not successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Collateral not successfully" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-collateral/loan-application/{Id}")]
        public HttpResponseMessage GetLoanApplicationCollateral(int id)
        {
            try
            {
                var response = repo.GetLoanApplicationCollateral(id);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found!" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #endregion Loan Collateral

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-details-product/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationDetailsProductProgram([FromUri] int loanApplicationDetailId)
        {
            var response = repo.GetLoanApplicationDetailsProductProgram(loanApplicationDetailId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-dedube-check/{customerId}")]
        public HttpResponseMessage GetLoanApplicationDedubeCheck([FromUri] int customerId)
        {
            var response = repo.GetLoanApplicationDedubeCheck(customerId, token.GetCompanyId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-validate-document-date")]
        public HttpResponseMessage ValidateDocumentDate([FromBody] ValidateDataViewModel data)
        {


            var response = repo.ValidateDocumentDate(data);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-validate-document-number")]
        public HttpResponseMessage ValidateDocumentNumber([FromBody] ValidateNumberViewModel data)
        {


            var response = repo.ValidateDocumentNumber(data);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });

            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });


        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("validate-invoice-details")]
        public HttpResponseMessage ValidateInvoiceDetails([FromBody] ValidateNumberViewModel data)
        {


            var response = repo.ValidateInvoiceDetails(data);
            if (response == true)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("validate-bulk-invoice-details")]
        //public HttpResponseMessage ValidateBulkLoanInvoice([FromBody] byte[] data)
        //{
        //    try
        //    {
        //        var response = repo.ValidateBulkLoanInvoice(data);
        //        if (response.Count > 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //        }

        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Upload file was not found!" });
        //        }      

        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

        [HttpGet, Route("loan-application-and-offer/rejected")]
        public HttpResponseMessage GetRejectedLoanApplications()
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            IQueryable<LoanApplicationViewModel> items;

            items = repo.GetRejectedLoanApplications(user);

            var data = items.ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });

        }

        [HttpGet, Route("loan-review-application-and-offer/rejected")]
        public HttpResponseMessage GetRejectedReviewLoanApplications()
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            IQueryable<LoanReviewApplicationViewModel> items;

            items = repo.GetRejectedReviewLoanApplications(user);

            var data = items.ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });

        }

        [HttpGet, Route("loan-application-and-offer/rejected/arch")]
        public HttpResponseMessage GetRejectedLoanApplicationsArch()
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };

            IQueryable<LoanApplicationViewModel> items;

            items = repo.GetRejectedLoanApplicationsArch(user);

            var data = items.ToList();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = items.Count() });

        }

        //[HttpPut]
        //[Route("loan-application-for-cam")]
        //public HttpResponseMessage SubmitLoanApplicationForCam([FromBody] dynamic model)
        //{
        //    try
        //    {
        //        var response = repo.SubmitLoanApplicationForCam(model.id, token.GetStaffId, model.checkListIndex);

        //        bool ok = !response.isdone  ? false : true;

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = ok, result = response });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: { e.InnerException }" });
        //    }
        //}


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/review-request")]
        public HttpResponseMessage ReviewRequest([FromBody] ForwardViewModel model)
        {


            model.userBranchId = (short)token.GetBranchId;
            model.companyId = token.GetCompanyId;
            model.createdBy = token.GetStaffId;
            model.applicationUrl = HttpContext.Current.Request.Path;

            string response = repo.ReviewRequest(model);

            bool ok = response == string.Empty ? false : true;

            return Request.CreateResponse(HttpStatusCode.OK, new { success = ok, result = response });

            //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-fees/{loanDetailId}")]
        public HttpResponseMessage GetLoanApplicationFees(int loanDetailId)
        {
            var response = repo.GetLoanApplicationFees(loanDetailId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("fee-concession-request")]
        public HttpResponseMessage ProductFeesConcession([FromBody]ProductFeesViewModel entity)
        {
            var user = new UserInfo
            {
                BranchId = (short)token.GetBranchId,
                createdBy = token.GetStaffId,
                companyId = token.GetCompanyId,
            };
            var response = repo.ProductFeesConcession(entity, user);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The Fee concession request completed successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-detail-search")]
        public HttpResponseMessage LoanApplicationSearch([FromBody] SearchViewModel model)
        {
            var response = repo.Search(model.searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("drawdown-application-detail-search")]
        public HttpResponseMessage DrawDownApplicationSearch([FromBody] SearchViewModel model)
        {
            var response = repo.SearchDrawDown(model.searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("search-booked-loans/{loanApplicationDetailId}")]
        public HttpResponseMessage SearchBookedLoans(int loanApplicationDetailId)
        {
            var response = repo.SearchBookedLoans(loanApplicationDetailId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + loanApplicationDetailId, result = response });

        }

        // search loan application by either reference number or name =======by benjamin
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-search/search/{searchString}")]
        public HttpResponseMessage GetLoanApplicationSearch([FromUri] string searchString)
        {
            var response = repo.LoanSearch(searchString);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + searchString, result = response });
            
            
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-details/search")]
        public HttpResponseMessage SearchLoanApplicationDetails([FromBody] SearchViewModel model)
        {
            var response = repo.GetLoanApplicationDetailsByReference(model.searchString, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
           
            
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("approved-loan-application-details/search")]
        public HttpResponseMessage SearchApprovedLoanApplicationDetails([FromBody] SearchViewModel model)
        {
            var response = repo.SearchApprovedLoanApplicationDetails(model.searchString, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
           
           
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-total-bank-exposure-and-limit")]
        public HttpResponseMessage GetBankTotalExposure()
        {
            var response = repo.GetTotalBankExposure();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
           
            
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("modify-facility/{loanApplicationDetailId}")]
        public HttpResponseMessage ModifyFacility([FromBody] FacilityModificationViewModel model, int loanApplicationDetailId)
        {
            var response = repo.ModifyFacility(model, loanApplicationDetailId);
            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Facility Has Been Modified Successfully" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "Facility Modification was not Successful" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/cancellation")]
        public HttpResponseMessage LoanApplicationCancellation()
        {
            var response = repo.GetAllRequestsForLoanCancellation(token.GetStaffId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
           
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-loan-application/cancellation")]
        public HttpResponseMessage LmsLoanApplicationCancellation()
        {
            var response = repo.GetAllLmsRequestsForLoanCancellation(token.GetStaffId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/loan-cancellation")]
        public HttpResponseMessage LoanApplicationCancellationRequest([FromBody] LoanApplicationViewModel data)
        {
            data.createdBy = token.GetStaffId;
            data.companyId = token.GetCompanyId;
            data.userBranchId = (short)token.GetBranchId;
            var response = repo.SaveCancelledApplcation(data);
            if (response == 1)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Loan Application Has been Cancelled Successfully" });
            }
            else if (response == 2)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Loan Application Has been Cancelled Successfully" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, result = response, message = "An error Occured while cancelling this loan Application" });
            }
            
            //if()
            //return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            //try
            //{
            //    data.createdBy = token.GetStaffId;
            //    data.companyId = token.GetCompanyId;
            //    var response = repo.SaveCancelledApplcation(data);
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            //}
            //catch (ConditionNotMetException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, warning = true, message = $"Error: {e.Message}" });
            //}
            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            //}
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/lms-loan-cancellation")]
        public HttpResponseMessage LmsLoanApplicationCancellationRequest([FromBody] LoanReviewApplicationViewModel data)
        {
            data.createdBy = token.GetStaffId;
            data.companyId = token.GetCompanyId;
            data.userBranchId = (short)token.GetBranchId;
            var response = repo.SaveLMSCancelledApplcation(data);
            if (response == 1)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Loan Application Has been Cancelled Successfully" });
            }
            else if (response == 2)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Loan Application Has been saved Successfully and sent for approval" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, result = response, message = "An error Occured while cancelling this loan Application" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/search")]
        public HttpResponseMessage SearchLoanApplication(string searchString)
        {
            var response = repo.SearchForLoan(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + searchString, result = response });
            
           
        }

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("loan-detail-by-application-reference/{searchString}")]
        //public HttpResponseMessage LoanApplicationDetailByApplicationRef(string searchString)
        //{
        //    var response = repo.SearchLoanApplicationDetails(token.GetCompanyId, searchString);

        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //}

        [HttpGet]
        [ClaimsAuthorization]
        [Route("committee-credit-application/{applicationType}")]
        public HttpResponseMessage CommitteeCreditApplications(int applicationType)
        {
            var response = repo.CommitteeCreditApplications(applicationType, token.GetStaffId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
           
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("tranche-facility/{searchValue}")]
        public HttpResponseMessage TrancheLoanDetails(string searchValue)
        {
            var response = repo.GetLoanApplication(searchValue);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
           
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("loanApplication/{id}")]
        public HttpResponseMessage DeleteLoanApplication(int id)
        {
            var token = new TokenDecryptionHelper();

            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            var result = repo.DeleteLoanApplication(id);
            // if(result)
            return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = result, message = "loan Application was removed successfully" });
        }




        [HttpDelete]
        [ClaimsAuthorization]
        [Route("loanApplicationDetail/{id}")]
        public HttpResponseMessage DeleteLoanApplicationDetail(int id)
        {
            var token = new TokenDecryptionHelper();

            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            var result = repo.DeleteLoanApplicationDetail(id);
            // if(result)
            return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = result, message = "loan Application was removed successfully" });
        }

        [HttpPost]
        [Route("reroute-workflow-target")]
        public HttpResponseMessage RerouteWorkflowTarget([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.RerouteWorkflowTarget(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Reroute done." });
        }

        [HttpPost]
        [Route("route-workflow-target")]
        public HttpResponseMessage RouteWorkflowTarget([FromBody] ForwardViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.RouteWorkflowTarget(entity);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Reroute done." });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-cancellation")]
        public HttpResponseMessage ViewLaonApplicationCancellationDetails([FromBody] LoanApplicationViewModel data)
        {
            var response = repo.ViewLaonApplicationCancellationDetails(data);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
            
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-loan-application-cancellation")]
        public HttpResponseMessage ViewLmsLaonApplicationCancellationDetails([FromBody] LoanReviewApplicationViewModel data)
        {
            var response = repo.ViewLmsLaonApplicationCancellationDetails(data);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });


        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-cancellation-approval")]
        public HttpResponseMessage GoForLoanApplicationCancellationApproval([FromBody] LoanApplicationViewModel data)
        {
            data.userBranchId = (short)token.GetBranchId;
            data.companyId = token.GetCompanyId;
            data.createdBy = token.GetStaffId;
            var response = repo.GoForLoanApplicationCancellationApproval(data);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
            
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-loan-application-cancellation-approval")]
        public HttpResponseMessage GoForLmsLoanApplicationCancellationApproval([FromBody] LoanReviewApplicationViewModel data)
        {
            data.userBranchId = (short)token.GetBranchId;
            data.companyId = token.GetCompanyId;
            data.createdBy = token.GetStaffId;
            var response = repo.GoForLmsLoanApplicationCancellationApproval(data);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });


        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction/dynamics/{loanApplicationId}")]
        public HttpResponseMessage GetTransactionDynamics(int loanApplicationId)
        {
            var response = repo.GetTrnasactionDynamics(loanApplicationId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
           
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction/lms-dynamics/{loanApplicationId}")]
        public HttpResponseMessage GetLMSTransactionDynamics(int loanApplicationId)
        {
            var response = repo.GetTrnasactionDynamics(loanApplicationId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
           
           
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/condition-precident/{loanApplicationId}")]
        public HttpResponseMessage GetConditionPrecidents(int loanApplicationId)
        {
            var response = repo.GetConditionPrecidents(loanApplicationId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
            
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/lms-condition-precident/{loanApplicationId}")]
        public HttpResponseMessage GetLMSConditionPrecidents(int loanApplicationId)
        {
            var response = repo.GetLMSConditionPrecidents(loanApplicationId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
           
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-detail-suggestion")]
        public HttpResponseMessage updateSuggestionsLoanApplicationdetail([FromBody] LoanApplicationDetailViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;  //FinTrakBankingContext
            entity.userBranchId = (short)token.GetBranchId;

            var response = repo.updateSuggestionsLoanApplicationdetail(entity);
            if (response == true)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Suggestions updated successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-funding-source")]
        public HttpResponseMessage GetAllCRMSFundingSource()
        {
            var data = repo.GetAllCRMSFundingSource();

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-repayment-source")]
        public HttpResponseMessage GetAllCRMSRepaymentSource()
        {
            var data = repo.GetAllCRMSRepaymentSource();

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-repayment-agreement/type")]
        public HttpResponseMessage GetAllCRMSRepaymentAgreementType()
        {
            var response = repo.GetAllCRMSRepaymentAgreementType();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
           
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-syndication-type")]
        public HttpResponseMessage GetAllSyndicationType()
        {
            var response = repo.GetAllSyndicationType();

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-details/reference/{reference}")]
        public HttpResponseMessage GetLoanApplicationDetailsByReference(string reference)
        {
            var data = repo.GetLoanApplicationDetailsByReference(reference, token.GetCompanyId);

            var id = 0;
            foreach (var d in data)
            {
                id = d.loanApplicationId;
                break;
            }
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                success = true,
                result = data,
                count = data.Count(),
                loanApplicationId = id
            });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-exceptional-loans-for-approval")]
        public HttpResponseMessage GetExceptionalLoansForApproval()
        {
            var data = repo.GetExceptionalLoansForApproval(token.GetStaffId);

            if (data == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [Route("exceptional-loan/forward-for-approval")]
        public HttpResponseMessage GoForApprovalExceptionalLoan([FromBody] ExceptionalLoanViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.GoForApprovalExceptionalLoan(entity);

            if (response.responseMessage != "") {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, $"EXCEPTIONAL LOAN - {response.responseMessage}") });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "EXCEPTIONAL LOAN") });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("application-detail-fields/{id}")]
        public HttpResponseMessage GetLoanApplicationDetailFields(int id)
        {
            var data = repo.GetLoanApplicationDetailFields(id);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("application-detail/{loanApplicationId}")]
        public HttpResponseMessage GetLoanApplicationDetailsById(int loanApplicationId)
        {
            var data = repo.GetLoanApplicationDetailsById(loanApplicationId, token.GetCompanyId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }
        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("application-detail-lms/{loanApplicationId}")]
        //public HttpResponseMessage GetLmsLoanApplicationDetailsById(int loanApplicationId)
        //{
        //    var data = repo.GetLmsLoanApplicationDetailsById(loanApplicationId, token.GetCompanyId);

        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        //}


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-tags/{id}")]
        public HttpResponseMessage GetLoanApplicationTags(int id)
        {
            LoanApplicationTagsViewModel response = repo.GetLoanApplicationTags(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-tags-lms/{id}")]
        public HttpResponseMessage GetLoanApplicationTagsLMS(int id)
        {
            LoanApplicationTagsLMSViewModel response = repo.GetLoanApplicationTagsLMS(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("revised-process-flow-by-product-class/{productClassId}/{productId}/{productTypeId}")]
        public HttpResponseMessage getFacilityApplicationRevisedProcessFlowByProductClassId(short productClassId, short productId, short productTypeId)
        {
            var response = repo.getFacilityApplicationRevisedProcessFlowByProductClassId(productClassId, productId, productTypeId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("cash-collaterized-process-flow")]
        public HttpResponseMessage getCashCollaterizedProcessFlowBy()
        {
            var response = repo.getCashCollaterizedProcessFlowBy();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("revised-process-flow")]
        public HttpResponseMessage getFacilityApplicationRevisedProcessFlow()
        {
            var response = repo.getFacilityApplicationRevisedProcessFlow();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        //RevisedProcessFlowModel
        [HttpPut]
        [ClaimsAuthorization]
        [Route("loan-application-tags/{id}")]
        public HttpResponseMessage UpdateLoanApplicationTags([FromBody] LoanApplicationTagsViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateLoanApplicationTags(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("loan-application-tags-lms/{id}")]
        public HttpResponseMessage UpdateLoanApplicationTagsLMS([FromBody] LoanApplicationTagsLMSViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateLoanApplicationTagsLMS(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("facility-by-loanApplicationId/{loanApplicationId}")]
        public HttpResponseMessage GetFacilityByApplicationId(int loanApplicationId)
        {
            var response = repo.GetFacilityByApplicationId(loanApplicationId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("facility-by-loanApplication-detail-id-los/{loanApplicationDetailId}")] 
        public HttpResponseMessage GetFacilityByApplicationDetailIdLos(int loanApplicationDetailId)
        {
            var response = repo.GetFacilityByApplicationDetailIdLos(loanApplicationDetailId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("facility-by-loanApplication-detail-id-lms/{loanApplicationDetailId}")]
        public HttpResponseMessage GetFacilityByApplicationDetailIdLms(int loanApplicationDetailId)
        {
            var response = repo.GetFacilityByApplicationDetailIdLms(loanApplicationDetailId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-failedrac-loan-application/{loanApplicationDetailId}")]
        public HttpResponseMessage DeleteLoanApplicationThatFailedRAC(int loanApplicationDetailId)
        {
            var response = repo.DeleteLoanApplicationThatFailedRAC(loanApplicationDetailId, token.GetStaffId);
            if (!response) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-flowchange/{loanApplicationId}")]
        public HttpResponseMessage LoanApplicationFlowChange(int loanApplicationId)
        {
            var response = repo.LoanApplicationFlowChange(loanApplicationId);
            if (!response) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-flow-change")]
        public HttpResponseMessage LoanApplicationFlowChange()
        {          
                 var response = repo.GetLoanApplicationFlowChange();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application-flow-change/{id}")]
        public HttpResponseMessage GetLoanApplicationFlowChange(int id)
        {
            LoanApplicationFlowChangeViewModel response =repo.GetLoanAppicationFlowChange(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

       


        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-flow-change")]
        public HttpResponseMessage AddLoanApplicationFlowChange([FromBody] LoanApplicationFlowChangeViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress; 
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId; 
            model.companyId = token.GetCompanyId;
            var response = repo.AddLoanApplicationFlowChange(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("loan-application-flow-change/{id}")]
        public HttpResponseMessage UpdateLoanApplicationFlowChange([FromUri] int id, [FromBody] LoanApplicationFlowChangeViewModel model)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateLoanApplicationFlowChange(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("loan-application-flow-change/{id}")]
        public HttpResponseMessage DeleteLoanApplicationFlowChange(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteLoanApplicationFlowChange(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("multiple-invoice")]
        public async Task<HttpResponseMessage> GetBulkLoanInvoice()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            //int uploadType;
            //if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "File Type is invalid.");
            //}


            byte[] pass = Convert.FromBase64String(provider.FormData["loginStaffPassCode"]);
            string password = Encoding.UTF8.GetString(pass);

            var entity = new UserInfo
            {
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                BranchId = (short)token.GetBranchId,
            };

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }




            var file = provider.Contents.FirstOrDefault();
            var buffer = await file.ReadAsByteArrayAsync();

            var data = repo.GetBulkLoanInvoice(buffer, entity);

            if (data != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "bulk invoice data was successfully uploaded" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error uploading bulk invoice data" });
        }

        #region LIEN

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien")]
        public HttpResponseMessage GetApplicationDetailLien()
        {
            IEnumerable<LoanApplicationLienViewModel> response = repo.GetApplicationDetailLien();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-id/{id}")]
        public HttpResponseMessage GetApplicationDetailLien(int id)
        {
            LoanApplicationLienViewModel response = repo.GetApplicationDetailLien(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-applicationDetailId/{applicationDetailId}")]
        public HttpResponseMessage GetLienByApplicationDetailId(int applicationDetailId)
        {
            IEnumerable<LoanApplicationLienViewModel> response = repo.GetLienByApplicationDetailId(applicationDetailId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-collateralId/{collateralId}")]
        public HttpResponseMessage GetLienByCollateralId(int collateralId)
        {
            IEnumerable<LoanApplicationLienViewModel> response = repo.GetLienByCollateralId(collateralId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lien-accountNo/{accountNo}")]
        public HttpResponseMessage GetApplicationDetailLienByAccountNo(string accountNo)
        {
            IEnumerable<LoanApplicationLienViewModel> response = repo.GetApplicationDetailLienByAccountNo(accountNo);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lien")]
        public HttpResponseMessage AddLoanApplicationDetailLien([FromBody] LoanApplicationLienViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddLoanApplicationDetailLien(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Lien has been proposed successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lien/{id}")]
        public HttpResponseMessage UpdateLoanApplicationDetailLien([FromUri] int id, [FromBody] LoanApplicationLienViewModel model)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateLoanApplicationDetailLien(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lien/{id}")]
        public HttpResponseMessage DeleteLoanApplicationDetailLien(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteLoanApplicationDetailLien(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "Lien has been unproposed successfully" });
        }
        #endregion LIEN

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-review-type")]
        public HttpResponseMessage GetAllLoanDetailReviewTypes()
        {
            IEnumerable<LoanDetailReviewTypeViewModel> response = repo.GetAllLoanDetailReviewTypes();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approved-trade-cycle")]
        public HttpResponseMessage GetAllApprovedTradeCycles()
        {
            IEnumerable<ApprovedTradeCycleViewModel> response = repo.GetAllApprovedTradeCycles();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("property-type")]
        public HttpResponseMessage GetAllPropertyTypes()
        {
            IEnumerable<PropertyTypeViewModel> response = repo.GetAllPropertyTypes();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }
    }
}