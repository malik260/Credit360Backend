using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.ErrorLogger;
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

        [HttpGet]
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
            catch (Exception e)
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("check-exiting-certificate-of-ownership/{certificateofownership}")]
        public HttpResponseMessage CheckExitingCertificateOfOwnership(string certificateofownership)
        {
            try
            {
                var response = repo.CheckExistingCertificateOfOwnership(certificateofownership, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application-eligibility/loanApplicationId/{id}")]
        public HttpResponseMessage GetLoanApplicationsDetails(int id)
        {
            try
            {
                var data = repo.GetLoanApplicationsDetails(id, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/search/{searchCriteria}")]
        public HttpResponseMessage FindLoan(string searchCriteria)
        {
            try
            {
                var response = repo.FindLoanApplication(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("update-loan-application-application/application")]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

       
        [HttpGet]
        [Route("loan/collateralrequirement/{applicationId}/{collateralCurrencyId}")]
        public HttpResponseMessage GetCollateralRequirements(int applicationId, int? collateralCurrencyId)
        {
            try
            {
                var response = repo.GetCollateralRequirements(applicationId, collateralCurrencyId, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan/application")]
        public HttpResponseMessage LoanBooking([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                if (entity.customerId.HasValue)
                {
                    if (creditLimitValidationsRepository.ValidateCamsol(entity.customerId.Value) > 0)
                    {
                        throw new Exception("Customer '" + entity.customerName + "' has been CAMSOL");
                    }

                    if (entity.customerId.HasValue && creditLimitValidationsRepository.ValidateWatchList(entity.customerId.Value) > 0)
                    {
                        throw new Exception("Customer '" + entity.customerName + "' has been Watchlisted");
                    }

                    if (entity.customerId.HasValue && creditLimitValidationsRepository.ValidateBlackList(entity.customerCode) > 0)
                    {
                        throw new Exception("Customer '" + entity.customerName + "' has been Blacklisted");
                    }
                }

                //var model =  creditLimitValidationsRepository.ValidateAmountByBranch1(entity.branchId).Difference;

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.branchId = (short)token.GetBranchId;

                entity.misCode = "001";
                entity.teamMisCode = "004";

                var response = repo.AddLoanApplication(entity);
                if (response > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application completed successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)

            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error Occured =>  {e.Message}" });
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
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

        #endregion Loan Application

        #region Loan Preliminary Evaluation

        [HttpPost]
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

                if (model.sentForEvaluation)
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
                    responseMessage = $"Preliminary evaluation note ({response.preliminaryEvaluationCode}) created successfully, now awaiting approval";
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{responseMessage}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Preliminary evaluation note not created" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
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

                if (model.sentForEvaluation)
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion Loan Preliminary Evaluation

        [HttpPost]
        [Route("loan-application/search")]
        public HttpResponseMessage LoanApplicationSearch([FromBody] SearchViewModel model)
        {
            try
            {
                var response = repo.Search(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #region Loan Collateral

        [HttpPost]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #endregion Loan Collateral

        [HttpGet]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan-validate-document-date")]
        public HttpResponseMessage ValidateDocumentDate([FromBody] ValidateDataViewModel data)
        {
            try
            {
                var response = repo.ValidateDocumentDate(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost]
        [Route("loan-validate-document-number")]
        public HttpResponseMessage ValidateDocumentNumber([FromBody] ValidateNumberViewModel data)
        {
            try
            {
                var response = repo.ValidateDocumentNumber(data);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception e)
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: { e.InnerException }" });
            }
        }
    }
}