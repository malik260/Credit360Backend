using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.ViewModels.Credit;
using System.Web;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.APICore.Filters;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Formatting;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/contingent")]
    public class ContingentLoanUsageController : ApiControllerBase
    {
        private IContingentLoanUsageRepository repo;

        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ContingentLoanUsageController(IContingentLoanUsageRepository repo)
        {
            this.repo = repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("getcontingentloan")]
        public HttpResponseMessage GetAllContingentLoans()
        {
          
            try
            {
                var data = repo.GetAllContingentLoans(token.GetStaffId, token.GetCompanyId); 

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("loanusage")]
        public async Task<HttpResponseMessage> SaveContigentLoans(ContingentLoanUsageViewModel incomingData)
        {

            try
            {

                incomingData.applicationUrl = HttpContext.Current.Request.Path;
                incomingData.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                incomingData.userBranchId = (short)token.GetBranchId;
                incomingData.createdBy = token.GetStaffId;
                incomingData.companyId = token.GetCompanyId;
                incomingData.staffId = token.GetStaffId;
                incomingData.userBranchId = (short)token.GetBranchId;

                var data = repo.SaveContigentLoans(incomingData);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (APIErrorException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

            
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("approvals/loanusage")]
        public HttpResponseMessage GetPendingRequest()
        {

            try
            {
                var data = repo.GetPendingRequest(token.GetStaffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("contingent/document/{loanId}")]
        public HttpResponseMessage GetContingentDocument(int loanId)
        {

            try
            {
                var data = repo.GetContingentUsageDocument(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("loanusage-approval")]
        public HttpResponseMessage SaveContigentLoansUsageApproval(ApproveAPSRequestViewModel entity)
        {
            var responseMessage = string.Empty;

            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            entity.userBranchId = (short)token.GetBranchId;
            entity.createdBy = token.GetStaffId;
            entity.companyId = token.GetCompanyId;
            entity.staffId = token.GetStaffId;

            bool response = repo.SaveContigentLoansUsageApproval(entity);
            if(response == true)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = response });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-contingent-usage/{loanId}")]
        public HttpResponseMessage GetContingentUsage(int loanId)
        {
            try
            {
                var data = repo.GetContingentUsage(loanId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
