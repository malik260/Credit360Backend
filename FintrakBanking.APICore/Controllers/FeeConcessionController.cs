using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/fees")]
    public class FeeConcessionController : ApiControllerBase
    {
         private IFeeConcessionRepository repo;
       

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public FeeConcessionController(IFeeConcessionRepository _repo)
        {
            repo = _repo;
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("fee-concession-type")]
        public HttpResponseMessage GetFeeConcessionType()
        {
            try
            {
                var data = repo.GetConcessionFeeType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("fee-concession-charges")]
        public HttpResponseMessage GetFeeConcessionCharges(int loanApplicationDetailId)
        {
            try
            {
                var data = repo.GetAllLoanFeeChargeByDetailId(loanApplicationDetailId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("fee-concession")]
        public HttpResponseMessage GetFeeConcessionByLoanApplicationDetailId(int loanDetailId)
        {
            try
            {
                var data = repo.GetAllConcessionFee(loanDetailId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("fee-concession-awaiting-approval")]
        public HttpResponseMessage GetAllConcessionFeeAwaitingApproval()
        {
            try
            {
                var data = repo.GetAllConcessionFeeAwaitingApproval(token.GetStaffId, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
         [HttpPost] [ClaimsAuthorization]
        [Route("fee-concession")]
        public HttpResponseMessage AddUpdateFeeConcession([FromBody] FeeConcessionViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.concessionId != 0 || entity.concessionId < 0)
                {
                    createUpdate = "updated";
                    if(repo.ValidateApprovedFeeConcession(entity.concessionId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                                 new { success = false, message = "Approved Record cannot be modified" });
                    }
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateFeeConcession(entity.loanApplicationDetailId, entity.loanChargeFeeId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "Concession Record of the same type is already undergoing approval." });
                    }
                }
              
              
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateFeeConcession(entity);
                if (data > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("fee-concession-approval")]
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

                if (data == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been approved successfully" });
                } else if (data == 2)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "Record has been disapproved successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                }
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error submitting this record {e.Message}" });
            }
        }
    }
}
