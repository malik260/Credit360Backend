using FintrakBanking.APICore.core;
using FintrakBanking.APICore.Filters;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/chargefee")]
    [SecureExceptionFilterAttribute]
    public class LoanFeeChargeController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ILoanFeeChargeRepository repo;
        private ILoanRepository loanRepo;
        public LoanFeeChargeController(ILoanFeeChargeRepository _repo, ILoanRepository _loanRepo)
        {
            this.repo = _repo;
            this.loanRepo = _loanRepo;
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("take-fee/submit")]
        public HttpResponseMessage SubmitTakeFee([FromBody] LoanFeeChargesViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.userBranchId = (short)token.GetBranchId;
                WorkflowResponse response = repo.SubmitTakeFee(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response.responseMessage, result = response.responseMessage });
            }
            catch (ConditionNotMetException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = e.Message });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, inner = ex.InnerException });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("take-fee-approval")]
        public HttpResponseMessage GetTakeFeeAwaitingApproval()
        {
            try
            {
                var data = repo.GetTakeFeeAwaitingApproval(token.GetStaffId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("take-fee-approve")]
        public HttpResponseMessage TakeFeeGoForApproval([FromBody] ApprovalViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.BranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;

                // var data = repo.addApplicationLineTenorChangeApproval(entity);

                WorkflowResponse data = repo.ApproveTakeFee(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.responseMessage, message = data.responseMessage });
               
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }

        }
    }
}
