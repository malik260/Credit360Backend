using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class LcCashBuildUpPlanController : ApiControllerBase
    {
        private ILcCashBuildUpPlanRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LcCashBuildUpPlanController(ILcCashBuildUpPlanRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-cashbuildupplan/{id}")]
        public HttpResponseMessage GetLcCashBuildUpPlansByLcIssuanceId(int id)
        {
            IEnumerable<LcCashBuildUpPlanViewModel> response = repo.GetLcCashBuildUpPlansByLcIssuanceId(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-cashbuildupreferencetypes")]
        public HttpResponseMessage GetLcCashBuildUpReferenceTypes()
        {
            IEnumerable<LcCashBuildUpPlanViewModel> response = repo.GetLcCashBuildUpReferenceTypes();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("lc-cashbuildupplan/{id}")]
        //public HttpResponseMessage GetLcCashBuildUpPlan(int id)
        //{
        //    LcCashBuildUpPlanViewModel response = repo.GetLcCashBuildUpPlan(id);
        //    if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        //}

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lc-cashbuildupplan")]
        public HttpResponseMessage AddLcCashBuildUpPlan([FromBody] LcCashBuildUpPlanViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            try
            {
                var response = repo.AddLcCashBuildUpPlan(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lc-cashbuildupplan/{id}")]
        public HttpResponseMessage UpdateLcCashBuildUpPlan([FromBody] LcCashBuildUpPlanViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                bool response = repo.UpdateLcCashBuildUpPlan(model,id,user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lc-cashbuildupplan/{id}")]
        public HttpResponseMessage DeleteLcCashBuildUpPlan(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                bool response = repo.DeleteLcCashBuildUpPlan(id,user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
