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

        [HttpGet]
        [Route("getcontingentloan")]
        public HttpResponseMessage GetAllContingentLoans()
        {
          
            try
            {
                var data = repo.GetAllContingentLoans(token.GetStaffId, token.GetCompanyId); 

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("loanusage")]
        public HttpResponseMessage SaveContigentLoans(ContingentLoanUsageViewModel entity)
        {              
            try
            {
                var responseMessage = string.Empty;

                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.userBranchId = (short)token.GetBranchId;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.userBranchId = (short)token.GetBranchId;

                var response = repo.SaveContigentLoans(entity, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("approvals/loanusage")]
        public HttpResponseMessage GetPendingRequest()
        {

            try
            {
                var data = repo.GetPendingRequest(token.GetStaffId, token.GetBranchId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
