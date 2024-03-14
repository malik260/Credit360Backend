using FintrakBanking.APICore.CFLAuthentication;
using FintrakBanking.APICore.core;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [MyBasicAuthenticationFilter]
    [RoutePrefix("api/v1/subsidiary")]
    public class SubsidiaryToHeadOfficeController : ApiController
    {
       
        private ILoanApplicationRepository repo;
        private readonly FinTrakBankingContext context;
        public SubsidiaryToHeadOfficeController(ILoanApplicationRepository _repo,
            FinTrakBankingContext _context

             
            )
        {
            this.repo = _repo;
            this.context = _context;

        }

        [HttpPost]
        [Route("subsidiary-loan-approval-inputs")]
        public HttpResponseMessage AddApprovalFromSubsidiary([FromBody] HeadOfficeFacilityApprovalViewModel entity)
        {
            try
            {
                var data = repo.AddApprovalFromSubsidiary(entity);
                if(data == true)
                {
                    APIResponse response = new APIResponse();
                    response.responseMessage = $"Record submitted successfully";
                    response.responseCode = "00";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    APIResponse response = new APIResponse();
                    response.responseMessage = $"There was an error creating this record, confirm all requested parameters are captured";
                    response.responseCode = "400";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
                
            }
            catch (Exception ex)
            {
                APIResponse response = new APIResponse();
                response.Message = $"There was an error creating this record, confirm all requested parameters are captured "+ex.Message;
                response.requestId = null;
                response.StatusCode = "99";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
        }
        [HttpPost]
        [Route("subsidiary-lms-approval-inputs")]
        public HttpResponseMessage AddLMSApprovalFromSubsidiary([FromBody] HeadOfficeFacilityApprovalViewModel entity)
        {
            try
            {
                var data = repo.AddLMSApprovalFromSubsidiary(entity);
                if(data == true)
                {
                    APIResponse response = new APIResponse();
                    response.responseMessage = $"Record submitted successfully";
                    response.responseCode = "00";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    APIResponse response = new APIResponse();
                    response.responseMessage = $"There was an error creating this record, confirm all requested parameters are captured";
                    response.responseCode = "400";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
                
            }
            catch (Exception ex)
            {
                APIResponse response = new APIResponse();
                response.Message = $"There was an error creating this record, confirm all requested parameters are captured "+ex.Message;
                response.requestId = null;
                response.StatusCode = "99";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
        }

    }
}
