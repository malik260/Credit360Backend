using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
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
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanApplicationController(
            ILoanApplicationRepository _repoApply,
            ILoanRepository _loanRepository,
            ICreditLimitValidationsRepository _creditLimitValidationsRepository,
            ILoanPreliminaryEvaluationRepository _repoLoanPEN
            )
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
        [Route("loan-application-list")]
        public HttpResponseMessage GetLoanApplicationByRelationshipOfficerId()
        {
            try
            {
                var data = repoApply.GetLoanApplicationByRelationshipOfficerId(token.GetStaffId, token.GetCompanyId);

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
        [Route("loan-application-eligibility/loanApplicationId/{id}")]
        public HttpResponseMessage GetLoanApplicationsDetails(int id)
        {
            try
            {
                var data = repoApply.GetLoanApplicationsDetails(id, token.GetCompanyId);
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
                var response = repoApply.GetLoanApplicationByRelationshipOfficerId(token.GetStaffId, token.GetCompanyId);

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



                var response = repoApply.UpdateApprovalStatusForApplication(id);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"{responseMessage}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Updated successful" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
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
        public HttpResponseMessage LoanBooking([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                //FinTrakBankingContext
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

                    if (entity.customerId.HasValue && creditLimitValidationsRepository.ValidateBlackList(entity.customerId.Value) > 0)
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

                var response = repoApply.AddLoanApplication(entity);
                if (response > 0)
                {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The loan application completed successfully" });
                }

               // return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
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

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/approved-loans")]
        public HttpResponseMessage GetCamProcessedLoanApplications()
        {
            try
            {
                var response = repoApply.GetApplicationsDueForOfferLetterGeneration(token.GetCompanyId);
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

        [HttpPut]
        [Route("loan-application/send-for-availment/{applicationRefNumber}/statusId/{applicationStatusId}")]
        public HttpResponseMessage UpdateApplicationStatus(int applicationRefNumber, short applicationStatusId)
        {
            try
            {
                var response = repoApply.UpdateLoanApplicationStatus(applicationRefNumber.ToString(), applicationStatusId);

                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "record not updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "record updated successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-review")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForReview()
        {
            try
            {
                var response = repoApply.GetApplicationsForReviewFromCreditUnit(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList(), count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/credit-assessment-memorandum/due-for-availment")]
        public HttpResponseMessage GetCamProcessedLoanApplicationsDueForAvailment()
        {
            try
            {
                var response = repoApply.GetApplicationsDueForAvailment(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response.ToList(), count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

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
                var response = repoApply.Search(model.searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter-template")]
        public HttpResponseMessage GenerateOfferLetterTemplate(string applicationRefNumber)
        {
            try
            {
                var response = repoApply.GenerateOfferLetterTemplate(applicationRefNumber);

                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "No record found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("loan-application/prepared-offer-letter")]
        public HttpResponseMessage SaveDraftOfferLetter([FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                var response = repoApply.SaveDraftOfferLetter(model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document saved successfully" });

                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document not saved successfully" });

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("loan-application/prepared-offer-letter/{documentId}")]
        public HttpResponseMessage UpdateDraftOfferLetter(int documentId, [FromBody] OfferLetterTemplateViewModel model)
        {
            try
            {
                var response = repoApply.UpdateDraftOfferLetter(documentId, model);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document updated successfully" });

                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document not updated successfully" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("loan-application/prepared-offer-letter/all")]
        public HttpResponseMessage GetAllPreparedOfferLetters()
        {
            try
            {
                var response = repoApply.GetAllPreparedOfferLetters().ToList();

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

        [HttpGet]
        [Route("loan-application/prepared-offer-letter")]
        public HttpResponseMessage GetPreparedOfferLetterByApplRefNumber(string applicationRefNumber)
        {
            try
            {
                var response = repoApply.GetPreparedOfferLetterByApplRefNumber(applicationRefNumber);

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

    }


}