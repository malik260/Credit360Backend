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
   
    [MyBasicAuthenticationFilter] // Authorization: Basic ZmludHJhayZAIyQ6ZmludHJhayZAIzM0OA==
    [RoutePrefix("api/v1/fintrak")]
    public class CashFlowLendingController : ApiController
    {
       
        private ICashFlowLendingRepository repo;
        private  IGeneralSetupRepository genSetup;
        private readonly FinTrakBankingContext context;
        public CashFlowLendingController(ICashFlowLendingRepository _repo,
            FinTrakBankingContext _context

             
            )
        {
            this.repo = _repo;
            this.context = _context;

        }

        [HttpPost]
        //[ClaimsAuthorization]
        [Route("customer")]
        public HttpResponseMessage AddCustomer([FromBody] IncomingCustomerViewModels entity)
        {
            try
            {
                //entity.userBranchId = (short)token.GetBranchId;
                //entity.companyId = token.GetCompanyId;
                //entity.createdBy = token.GetStaffId;
                //entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddCustomer(entity);

                return Request.CreateResponse(HttpStatusCode.OK,  data );
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


        [HttpPost]
        //[ClaimsAuthorization]
        [Route("cfl-loan-request")]
        public HttpResponseMessage submitRequest([FromBody] CflLoanApplication entity)
        {

            try
            {
                //entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = 1; // token.GetCompanyId;
                //entity.createdBy = token.GetStaffId;
                entity.applicationUrl = Request.RequestUri.AbsoluteUri;
                var data = repo.submitRequest(entity);
                return Request.CreateResponse(HttpStatusCode.OK, data );
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
