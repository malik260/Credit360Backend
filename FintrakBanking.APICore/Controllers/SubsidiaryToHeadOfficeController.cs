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
using FintrakBanking.ViewModels.Customer;
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
    public class SubsidiaryToHeadOfficeController : ApiControllerBase
    {
        private ILoanApplicationRepository repo;

        public SubsidiaryToHeadOfficeController(
            ILoanApplicationRepository _repo
            )
        {
            this.repo = _repo;
        }

        [HttpPost]
        [Route("subsidiary-loan-approval-inputs")]
        public HttpResponseMessage IncomingLoanApplicationApproval([FromBody] HeadOfficeFacilityApprovalViewModel data)
        {
            try
            {
                var resp = repo.IncomingLoanApplicationApproval(data);
                APIResponse response = new APIResponse();
                if (resp == true)
                {
                    response.responseMessage = $"Record submitted successfully";
                    response.responseCode = 200;
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = response});
                }
                else
                {
                    response.responseMessage = $"There was an error creating this record, confirm all requested parameters are captured";
                    response.responseCode = 400;
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
                }
               
            }
            catch (Exception ex)
            {
                APIResponse response = new APIResponse();
                response.Message = $"There was an error creating this record, confirm all requested parameters are captured";
                response.requestId = null;
                response.StatusCode = "400";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }

        }

              
    }
}