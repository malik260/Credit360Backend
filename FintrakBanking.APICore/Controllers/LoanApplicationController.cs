using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LoanApplicationController : ApiControllerBase
    {
        private ILoanApplicationRepository repo;
        private ILoanRepository loanRepository;
        private ICreditLimitValidationsRepository creditLimitValidationsRepository;
        private ILoanPreliminaryEvaluationRepository repoLoanPEN;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IErrorLogRepository errorLogger;

        public LoanApplicationController(
            ILoanApplicationRepository _repo,
            ILoanRepository _loanRepository,
            ICreditLimitValidationsRepository _creditLimitValidationsRepository,
            ILoanPreliminaryEvaluationRepository _repoLoanPEN,
            IErrorLogRepository _errorLogger
            )
        {
            this.repo = _repo;
            this.loanRepository = _loanRepository;
            this.creditLimitValidationsRepository = _creditLimitValidationsRepository;
            repoLoanPEN = _repoLoanPEN;
            errorLogger = _errorLogger;
        }

        #region Loan Application

      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
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

        [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
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

        [HttpGet] [ClaimsAuthorization]  
        [Route("loan-application-list")]
        public HttpResponseMessage GetLoanApplicationByRelationshipOfficerId()
        {
            try
            {
                var data = repo.GetLoanApplicationByRelationshipOfficerId(token.GetStaffId, token.GetCompanyId);

                // var data = response.OrderByDescending(c => c.loanApplicationId)

                // .ToList();
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

      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-application/{id}")]
        public HttpResponseMessage GetLoanAppById(int id)
        {
            try
            { 
                var data = repo.GetLoanAppById(id, token.GetCompanyId);

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
        
      [HttpGet] [ClaimsAuthorization]  
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

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data});
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


        [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-application/search/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {
                var response = repo.FindLoanApplication(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost] [ClaimsAuthorization]
        [Route("loan-application")]
        public HttpResponseMessage UpdateApprovalStatusForApplication([FromBody] int id)
        {
            try
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("loan-application-for-cam")]
        public HttpResponseMessage SubmitLoanApplicationForCam([FromBody] LoanApplicationUpdateViewModel loan)
        {
            try
            {
                var responseMessage = string.Empty;
                var response = repo.SubmitLoanApplicationForCam(loan.applicationId, token.GetStaffId, loan.checkListIndex);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
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

      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
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

         [HttpPost] [ClaimsAuthorization]
        [Route("loan/application")]
        public HttpResponseMessage AddLoanApplication([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
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

         [HttpPost] [ClaimsAuthorization]
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

         [HttpPost] [ClaimsAuthorization]
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

      [HttpGet] [ClaimsAuthorization]  
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
        [Route("customer-pen-code")]
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
        
      [HttpGet] [ClaimsAuthorization]  
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

       [HttpPut] [ClaimsAuthorization]
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

       [HttpPut] [ClaimsAuthorization]
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

         [HttpPost] [ClaimsAuthorization]
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

      [HttpGet] [ClaimsAuthorization]  
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

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-application-details-product/{loanApplicationDetailId}")]
        public HttpResponseMessage GetLoanApplicationDetailsProductProgram([FromUri] int loanApplicationDetailId)
        {
            try
            {
                var response = repo.GetLoanApplicationDetailsProductProgram(loanApplicationDetailId);
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


        [HttpGet] [ClaimsAuthorization]  
        [Route("loan-dedube-check/{customerId}")]
        public HttpResponseMessage GetLoanApplicationDedubeCheck([FromUri] int customerId)
        {
            try
            {
                var response = repo.GetLoanApplicationDedubeCheck(customerId, token.GetCompanyId);
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

         [HttpPost] [ClaimsAuthorization]
        [Route("loan-validate-document-date")]
        public HttpResponseMessage ValidateDocumentDate([FromBody] ValidateDataViewModel data)
        {
            try
            {
                var response = repo.ValidateDocumentDate(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("loan-validate-document-number")]
        public HttpResponseMessage ValidateDocumentNumber([FromBody] ValidateNumberViewModel data)
        {
            try
            {
                var response = repo.ValidateDocumentNumber(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("validate-invoice-details")]
        public HttpResponseMessage ValidateInvoiceDetails([FromBody] ValidateNumberViewModel data)
        {
            try
            {
                var response = repo.ValidateInvoiceDetails(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        

        [HttpGet, Route("loan-application-and-offer/rejected")]
        public HttpResponseMessage GetRejectedLoanApplications()
        {
            try
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
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


         [HttpPost] [ClaimsAuthorization]
        [Route("loan-application/review-request")]
        public HttpResponseMessage ReviewRequest([FromBody] ForwardViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;

                string response = repo.ReviewRequest(model);

                bool ok = response == string.Empty ? false : true;

                return Request.CreateResponse(HttpStatusCode.OK, new { success = ok, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: { e.InnerException }" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-application-fees/{loanDetailId}")]
        public HttpResponseMessage GetLoanApplicationFees(int loanDetailId)
        {
            try
            {
                var response = repo.GetLoanApplicationFees(loanDetailId);
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

       [HttpPut] [ClaimsAuthorization]
        [Route("fee-concession-request")]
        public HttpResponseMessage ProductFeesConcession([FromBody]ProductFeesViewModel entity)
        {
            try
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
            catch (SecureException e)

            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error Occured =>  {e.Message}" });
            }
        }

        
         [HttpPost] [ClaimsAuthorization]
        [Route("loan-application/search")]
        public HttpResponseMessage LoanApplicationSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.Search(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-application/cancellation")]
        public HttpResponseMessage LoanApplicationCancellation()
        {
            try
            {
                var response = repo.GetAllRequestsForLoanCancellation(token.GetStaffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true,  result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application/loan-cancellation")]
        public HttpResponseMessage LoanApplicationCancellationRequest([FromBody] LoanApplicationViewModel data)
        {
            try
            {
                data.createdBy = token.GetStaffId;
                data.companyId = token.GetCompanyId;
                var response = repo.SaveCancelledApplcation(data);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true,  result = response });
            }
            catch (ConditionNotMetException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, warning=true, message = $"Error: {e.Message}" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("loan-application/search")]
        public HttpResponseMessage SearchLoanApplication(string searchString)
        {
            try
            {
                var response = repo.SearchForLoan(searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]
        [Route("loan-application-details/searchtext/{searchString}")]
        public HttpResponseMessage SearchLoanApplicationDetails(string searchString)
        {
            try
            {
                var response = repo.SearchLoanApplicationDetails(token.GetCompanyId, searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("committee-credit-application/{applicationType}")]
        public HttpResponseMessage CommitteeCreditApplications(int applicationType)
        {
            try
            {
                var response = repo.CommitteeCreditApplications(applicationType, token.GetStaffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("tranche-facility/{searchValue}")]
        public HttpResponseMessage TrancheLoanDetails(string searchValue)
        {
            try
            {
                var response = repo.GetLoanApplication(searchValue);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("loanApplicationDetail/{id}")]
        public HttpResponseMessage DeleteLoanApplicationDetail(int id)
        {
            try
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                var response = repo.ViewLaonApplicationCancellationDetails(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-cancellation-approval")]
        public HttpResponseMessage GoForLoanApplicationCancellationApproval([FromBody] LoanApplicationViewModel data)
        {
            try
            {
                data.userBranchId = (short)token.GetBranchId;
                data.companyId = token.GetCompanyId;
                data.createdBy = token.GetStaffId;
                var response = repo.GoForLoanApplicationCancellationApproval(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction/dynamics/{loanApplicationId}")]
        public HttpResponseMessage GetTransactionDynamics(int loanApplicationId)
        {
            try
            {
                var response = repo.GetTrnasactionDynamics(loanApplicationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction/lms-dynamics/{loanApplicationId}")]
        public HttpResponseMessage GetLMSTransactionDynamics(int loanApplicationId)
        {
            try
            {
                var response = repo.GetTrnasactionDynamics(loanApplicationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/condition-precident/{loanApplicationId}")]
        public HttpResponseMessage GetConditionPrecidents(int loanApplicationId)
        {
            try
            {
                var response = repo.GetConditionPrecidents(loanApplicationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan/condition-precident/{loanApplicationId}")]
        public HttpResponseMessage GetLMSConditionPrecidents(int loanApplicationId)
        {
            try
            {
                var response = repo.GetLMSConditionPrecidents(loanApplicationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-application-detail-suggestion")]
        public HttpResponseMessage updateSuggestionsLoanApplicationdetail([FromBody] LoanApplicationDetailViewModel entity)
        {
            try
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"{e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-funding-source")]
        public HttpResponseMessage GetAllCRMSFundingSource()
        {
            try
            {
                var data = repo.GetAllCRMSFundingSource();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-repayment-source")]
        public HttpResponseMessage GetAllCRMSRepaymentSource()
        {
            try
            {
                var data = repo.GetAllCRMSRepaymentSource();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("crms-repayment-agreement/type")]
        public HttpResponseMessage GetAllCRMSRepaymentAgreementType()
        {
            try
            {
                var response = repo.GetAllCRMSRepaymentAgreementType();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        
             [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-syndication-type")]
        public HttpResponseMessage GetAllSyndicationType()
        {
            try
            {
                var response = repo.GetAllSyndicationType();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
    }
}