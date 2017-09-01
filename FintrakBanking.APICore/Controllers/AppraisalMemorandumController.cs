using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class AppraisalMemorandumController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private IAppraisalMemorandumRepository repo;

        public AppraisalMemorandumController(IAppraisalMemorandumRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        [Route("appraisal-memorandum/loan-application/{loanApplicationId}")]
        public HttpResponseMessage GetAppraisalMemorandumByLoanApplicationId(int loanApplicationId)
        {
            try
            {
                var data = repo.GetAppraisalMemorandumByLoanApplicationId(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("appraisal-memorandum")]
        public HttpResponseMessage AddAppraisalMemorandum([FromBody] AppraisalMemorandumViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddAppraisalMemorandum(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPut]
        [Route("appraisal-memorandum/{appraisalMemorandumId}")]
        public HttpResponseMessage UpdateAppraisalMemorandum([FromBody] AppraisalMemorandumViewModel entity, int appraisalMemorandumId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateAppraisalMemorandum(entity, appraisalMemorandumId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("appraisal-memorandum/forward")]
        public async Task<HttpResponseMessage> ForwardAppraisalMemorandum([FromBody] ForwardViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var response = await repo.ForwardAppraisalMemorandum(entity);

                if (response == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/trail/{loanApplicationId}")]
        public HttpResponseMessage GetAppraisalMemorandumTrail(int loanApplicationId)
        {
            try
            {
                var data = repo.GetAppraisalMemorandumTrail(loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("appraisal-memorandum/privilege/{loanApplicationId}")]
        public HttpResponseMessage GetUserPrivilege(int loanApplicationId)
        {
            try
            {
                var data = repo.GetUserPrivilege(token.GetStaffId, loanApplicationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }
}
