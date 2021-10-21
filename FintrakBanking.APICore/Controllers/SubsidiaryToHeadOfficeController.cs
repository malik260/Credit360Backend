using FintrakBanking.APICore.CFLAuthentication;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Authentication;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
   
    [RoutePrefix("api/v1/fintrak")]
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
                    response.responseCode = 200;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    APIResponse response = new APIResponse();
                    response.responseMessage = $"There was an error creating this record, confirm all requested parameters are captured";
                    response.responseCode = 400;
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
                
            }
            catch (Exception ex)
            {
                APIResponse response = new APIResponse();
                response.Message = $"There was an error creating this record, confirm all requested parameters are captured";
                response.requestId = null;
                response.StatusCode = "99";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
        }

  }
}
