using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/fintrak")]
    public class CashFlowLendingController : ApiController
    { 
        private readonly ICashFlowLendingRepository repo;


        public CashFlowLendingController(ICashFlowLendingRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("customer")]
        public HttpResponseMessage AddCustomer([FromBody] CustomerViewModels entity)
        {
            try
            {
                //entity.userBranchId = (short)token.GetBranchId;
                //entity.companyId = token.GetCompanyId;
                //entity.createdBy = token.GetStaffId;
                //entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddCustomer(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully, now waiting for approval" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }


    }
}
