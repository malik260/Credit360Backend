using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/creditoperations")]
    public class LoanOperationsController : ApiControllerBase
    {
        private ILoanOperationsRepository repo;
        private ILoanRepository loanRepo;
        private IEndOfDayRepository repoEOD;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanOperationsController(ILoanOperationsRepository _repo,
            IEndOfDayRepository _repoEOD,
            ILoanRepository _loanRepo)
        {
            this.repo = _repo;
            this.repoEOD = _repoEOD;
            this.loanRepo = _loanRepo;
        }

        [HttpGet]
        [Route("getcollateralsearchchargeamount{stateId}")]
        public HttpResponseMessage GetCollateralSearchChargeAmount(int stateId)
        {
            try
            {
                var data = repo.GetCollateralSearchChargeAmount(stateId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-operationtype")]
        public HttpResponseMessage GetOperationType()
        {
            try
            {
                var data = repo.GetOperationType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("loan-operationtypebyoverdraft")]
        public HttpResponseMessage GetOperationTypeByOD()
        {
            try
            {
                var data = repo.GetOperationTypeByOD();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-operationtype/")]
        public HttpResponseMessage GetOperationTypeByLoanId(int productTypeId, int scheduleTypeId)
        {
            try
            {
                var data = repo.GetOperationTypeByLoanId((LoanProductTypeEnum)productTypeId, (LoanScheduleTypeEnum)scheduleTypeId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("loan-search/")]
        public HttpResponseMessage SearchForLoan(string searchQuery)
        {
            try
            {
                var data = loanRepo.SearchForLoan(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("disbursed-loan-details/")]
        public HttpResponseMessage SearchForLoan(int loanId)
        {
            try
            {
                var data = loanRepo.GetDisbursedLoanByLoanId(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-guarantor/")]
        public HttpResponseMessage GetLoanGuarantor(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanGuarantors(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-convenant/")]
        public HttpResponseMessage GetLoanConvenant(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanCovenant(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-chargefee/")]
        public HttpResponseMessage GetLoanChargeFee(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanChargeFee(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("loan-schedule-details/")]
        public HttpResponseMessage GetOperationTypeByLoanId(int loanId)
        {
            try
            {
                var data = loanRepo.GetLoanScheduleByLoanId(loanId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("loan-operation/awaiting-approval")]
        public HttpResponseMessage GetLoanBookingAwaitingApproval()
        {
            try
            {
                var data = repo.GetLoanOperationAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("loan-operation/approved-loan-review")]
        public HttpResponseMessage GetApprovedLoanReviewed()
        {
            try
            {
                var data = repo.GetApprovedLoanOperationReview();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("loan-operation/approval-detail/")]
        public HttpResponseMessage GetApprovalDetails(int loanId, int operationId)
        {
            try
            {
                var data = repo.GetApprovalDetails(loanId, operationId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost]
        [Route("add-loan-review")]
        public HttpResponseMessage AddOperationReview([FromBody] LoanReviewOperationViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                if((int)OperationsEnum.Prepayment == model.operationTypeId)
                {
                    if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The requested operation already exist and going through approval" });
                    }
                    var response = repo.AddOperationReview(model);
                    if (response)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
                else
                {
                if (model.principalFirstPaymentDate < model.proposedEffectiveDate)    
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Principal First Payment Date cannot be less than Effective date" });
                }
                if (model.interestFirstPaymentDate < model.proposedEffectiveDate)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Interest First Payment Date cannot be less than Effective date" });
                }
                    if (repo.DoesOperationExist(model.loanId, model.operationTypeId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false,  message = "The requested operation already exist and going through approval" });
                }
                    var response = repo.AddOperationReview(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully and passed for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        
    }
        [HttpPost]
        [Route("operation-approval")]
        public HttpResponseMessage GoForApproval([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
                var data = repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        [HttpPost]
        [Route("operation-loan-rephrasement")]
        public HttpResponseMessage LoanRephrasementOperation([FromBody]LoanReviewOperationViewModel entity)
        {
            try
            {
       
                if (entity.loanReviewOperationsId == 0 || entity.loanId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Please reconfirm your request and try again" });
                }
                var data = repo.LoanRephasementProcess((short)entity.loanReviewOperationsId, entity.loanId, token.GetStaffId);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Operation successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Operation not successful" });
            }
            catch (System.Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
    }
}