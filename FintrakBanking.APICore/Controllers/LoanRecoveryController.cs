using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class LoanRecoveryController : ApiControllerBase
    {
        private ILoanRecoverySetupRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanRecoveryController(ILoanRecoverySetupRepository _repo)
        {
            repo = _repo;
        }

        [HttpPost]
        [Route("addloanRecoverySetup")]
        public HttpResponseMessage AddLoanRecovery( [FromBody]LoanRecoverySetupViewModel entity)
        {
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Empty Record" });
            }

            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var LoanRecovery = repo.AddLoanRecoverySetup(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, message = "The record has been created successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       
        [HttpGet]
        [Route("loanRecoverySetup")]
        public HttpResponseMessage GetAllLoanRecovery()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllLoanRecoverySetup().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No LoanRecovery found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

        [HttpGet]
        [Route("product-type")]
        public HttpResponseMessage GetAllProductType()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllProductType().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


        [HttpGet]
        [Route("casa")]
        public HttpResponseMessage GetAllCasa()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllCasa().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }


        [HttpGet]
        [Route("agent")]
        public HttpResponseMessage GetAllAgent()
        {
            var Message = string.Empty;
            try
            {
                var LoanRecovery = repo.GetAllAgent().ToList();
                if (LoanRecovery.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LoanRecovery, count = LoanRecovery.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No Message Type found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }




        [HttpPut]
        [Route("updateloanRecoverySetup/{recoveryPlanId}")]
        public HttpResponseMessage UpdateLoanRecovery(int recoveryPlanId, LoanRecoverySetupViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var data = repo.UpdateLoanRecoverySetup(recoveryPlanId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }


        [HttpGet]
        [Route("getloanRecoverySetup/{recoveryPlanId}")]
        public HttpResponseMessage GetLoanRecoverySetup (int recoveryPlanId)
        {
            var account = repo.GetLoanRecoverySetup(recoveryPlanId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.GetLoanRecoverySetup(recoveryPlanId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = depart });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


    }
}