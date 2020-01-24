using FintrakBanking.APICore.JWTAuth;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common.Extensions;


namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/loan")]
    public class CreditDrawdownController : ApiControllerBase
    {
        private ICreditDrawdownRepository repo;
       // private ICustomerCollateralRepository repoCollateral;
        //private ICustomerRepository repoCustomer;
       // private ILoanScheduleRepository scheduleRepo;
        //private ILoanOperationsRepository loanoperations;
       // private IProductRepository productRepo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ExportDataTableToExcel export = new ExportDataTableToExcel();


        public CreditDrawdownController(ICreditDrawdownRepository _repo
                              //ICustomerCollateralRepository _repoCollateral,
                              //ICustomerRepository _repoCustomer,
                              // ILoanScheduleRepository _scheduleRepo,
                              // IProductRepository _productRepo, ILoanOperationsRepository _loanoperations
                              )
        {
            this.repo = _repo;
            //this.repoCollateral = _repoCollateral;
            //this.repoCustomer = _repoCustomer;
            //this.scheduleRepo = _scheduleRepo;
            //this.productRepo = _productRepo;
            //this.loanoperations = _loanoperations;


        }




        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("loan-transaction-dynamics/{loanApplicationDetailId}")]
        //public HttpResponseMessage GetLoanTransactionDynamics(int loanApplicationDetailId)
        //{
        //    try
        //    {
        //        var data = repo.GetLoanTransactionDynamics(loanApplicationDetailId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        //    }
        //    catch (ConditionNotMetException ce)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
        //    }
        //    catch (BadLogicException be)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
        //    }
        //    catch (Exception)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
        //    }
        //}


        [HttpGet]
        [Route("loan-booking/request/approval")]
        public HttpResponseMessage GetInitiatedLoanApplicationAwaitingApproval()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var data = repo.GetBookingRequestAwaitingApproval(token.GetStaffId, token.GetCompanyId, false);

            if (data.Any() == false)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), message = "No record found" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loan-request/approval/{loanBookingRequestId}")]
        public HttpResponseMessage ApproveInitiatedLoanBooking([FromBody] ApprovalViewModel model, int loanBookingRequestId)
        {
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            model.BranchId = (short)token.GetBranchId;
            model.staffId = token.GetStaffId;

            var responseId = repo.GoForBookingRequestApproval(model, loanBookingRequestId);

            if (responseId == 1)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            else if (responseId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = "Loan request has been successfully approved" });
            }
            else if (responseId == 3)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = true, message = "Loan request was successfully disapproved" });
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Operation unsuccessful, an error occured while saving changes. " });
            }
        }


        [HttpGet]
        [Route("loan-application/availment-completed")]
        public HttpResponseMessage GetAvailedLoanApplicationsDueForInitiateBooking()
        {
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            var response = repo.GetAvailedLoanApplicationsDueForInitiateBooking(token.GetCompanyId, token.GetStaffId, token.GetBranchId);
            if (!response.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("loan-application/request-booking/{applicationId}")]
        //public HttpResponseMessage AddLoanBookingRequest(int applicationId, [FromBody] List<LoanBookingRequestViewModel> models)
        //{
        //    foreach (var model in models)
        //    {
        //        model.userBranchId = (short)token.GetBranchId;
        //        model.applicationUrl = HttpContext.Current.Request.Path;
        //        model.createdBy = token.GetStaffId;
        //        model.companyId = token.GetCompanyId;
        //    }


        //    var data = repo.AddLoanBookingRequest(applicationId, models);
        //    if (data)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //            new { success = true, data = data, message = "Drawdown Request successfully sent for processing!" });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK,

        //        new { success = false, message = "Initiating Drawdown Request was unsuccessful!" });

        //}

        //[HttpGet]
        //[Route("work-flow-tracker/operation/{operationId}/target/{targetId}")]
        //public async Task<HttpResponseMessage> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId)
        //{
        //    var data = await repo.GetApprovalTrailByOperationIdAndTargetId(operationId, targetId, token.GetCompanyId, token.GetStaffId);

        //    if (data == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data, count = data.Count() });
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        //}



    }
}