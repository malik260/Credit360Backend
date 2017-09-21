using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
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
        
        private ILoanApplicationRepository repoApply;
        private ILoanRepository loanRepository;
        private ICreditLimitValidationsRepository creditLimitValidationsRepository;
        private ILoanPreliminaryEvaluationRepository repoLoanPEN;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanApplicationController(ILoanApplicationRepository _repoApply, ILoanRepository _loanRepository,
            ICreditLimitValidationsRepository _creditLimitValidationsRepository, 
            ILoanPreliminaryEvaluationRepository _repoLoanPEN)
        {
            this.repoApply = _repoApply;
            this.loanRepository = _loanRepository;
          this.creditLimitValidationsRepository = _creditLimitValidationsRepository;
            repoLoanPEN = _repoLoanPEN;
        }

        #region Loan Application
        [HttpGet]
        [Route("loan-application")]
        public HttpResponseMessage GetAllLoanApplications()
        {
            try
            {
                var response = repoApply.GetAllLoanApplications(token.GetCompanyId);
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
        [Route("loan-application/customer/{id}")]
        public HttpResponseMessage ExistingLoanApplication(int id)
        {
            try
            {
                var response = repoApply.ExistingLoanApplication(id, token.GetCompanyId);
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
                var response = repoApply.CheckExistingCertificateOfOwnership(certificateofownership, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpGet]
        [Route("loan-application/{id}")]
        public HttpResponseMessage GetLoanApplicationById(int id)
        {
            try
            {
                var response = repoApply.GetLoanApplicationById(id, token.GetCompanyId);
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
        [Route("loan-product-class")]
        public HttpResponseMessage GetProductClass()
        {
            try
            {
                var response = repoApply.GetProductClass();
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
                var response = repoApply.FindLoanApplication(searchCriteria, token.GetCompanyId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpDelete("loan-application/{aid}/approval-status/{id}")]
        //public IActionResult UpdateApprovalStatus(int aid, ApprovalStatusEnum id)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = HttpContext.Current.Request.Path,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };

        //        repoApply.UpdateApprovalStatus(aid,id, user);

        //        return new { success = true, result = id, message = "record has been deleted successfully" });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [Route("loan/application")]
        public async Task<HttpResponseMessage> LoanBooking([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                //FinTrakBankingContext

                if (creditLimitValidationsRepository.ValidateCamsol(entity.customerId.Value) > 0)
                {
                    throw new Exception("Customer '" + entity.customerName + "' has been CAMSOL");
                }

                if (creditLimitValidationsRepository.ValidateWatchList(entity.customerId.Value) > 0)
                {
                    throw new Exception("Customer '" + entity.customerName + "' has been Watchlisted");
                }

                if (creditLimitValidationsRepository.ValidateBlackList(entity.customerId.Value) > 0)
                {
                    throw new Exception("Customer '" + entity.customerName + "' has been Blacklisted");
                }


                //var model =  creditLimitValidationsRepository.ValidateAmountByBranch1(entity.branchId).Difference;


                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.branchId = (short)token.GetBranchId;

                entity.misCode = "001";
                entity.teamMisCode = "004";

                var response = await repoApply.AddLoanApplication(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Operation completed successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error Occured =>  {e.Message}" });
            }
        }

        [HttpGet, Route("loan-application/pending")]
        public HttpResponseMessage GetPendingLoanApplications([FromUri] int page, [FromUri] int itemsPerPage)
        {
            try
            {
                var data = repoApply.GetPendingLoanApplications(token.GetCountryId, token.GetBranchId, token.GetStaffId)
                    .OrderByDescending(x => x.applicationDate)
                    .ThenByDescending(x => x.loanApplicationId)
                    .Skip(page).Take(itemsPerPage)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpGet]
        //[Route("loan-application/job")]
        //public HttpResponseMessage GetLoanApplicationJobs(int page, int itemsPerPage, int level, int scope)
        //{
        //    try
        //    {
        //        var response = repoApply.GetLoanApplicationJobs(token.GetCompanyId, level, scope);

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


        #endregion

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
                    responseMessage = "Preliminary evaluation note created successfully, now awaiting approval";
                }
                else
                {
                    model.isCurrent = false;
                    responseMessage = "Preliminary evaluation note created successfully";
                }

                var response = await repoLoanPEN.AddPreliminaryEvaluation(model);

                if (response != null)
                {
                    responseMessage = $"Preliminary evaluation note ({response.preliminaryEvaluationCode}) created successfully, now awaiting approval";
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{responseMessage}" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Preliminary evaluation note not created" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("loan/preliminary-evaluation/approval")]
        public async Task<HttpResponseMessage> ApprovePreliminaryEvaluation(ApprovalViewModel model)
        {
            try
            {
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                model.BranchId = (short)token.GetBranchId;
                model.staffId = token.GetStaffId;

                var data = await repoLoanPEN.GoForApproval(model);

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
        [Route("loan-preliminary-evaluation")]
        public HttpResponseMessage GetLoanPreliminaryEvaluations()
        {
            try
            {
                var data = repoLoanPEN.GetAllLoanPreliminaryEvaluations();

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
        [Route("loan/preliminary-evaluation/awaiting-approval")]
        public HttpResponseMessage GetLoanPreliminaryEvaluationsForAppproval()
        {
            try
            {
                var data = repoLoanPEN.GetPreliminaryEvaluationsAwaitingApproval(token.GetStaffId, token.GetCompanyId);

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
        #endregion
    }
}