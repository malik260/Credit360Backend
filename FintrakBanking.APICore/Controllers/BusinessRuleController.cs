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
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class BusinessRuleController : ApiControllerBase
    {
        private IBusinessRuleRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public BusinessRuleController(IBusinessRuleRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("level-business-rule")]
        public HttpResponseMessage AddBusinessRule([FromBody] BusinessRuleViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var data = repo.AddBusinessRule(model);
            if (data) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("level-business-rule")]
        public HttpResponseMessage GetBusinessRule()
        {
            IEnumerable<BusinessRuleViewModel> data = repo.GetBusinessRule(token.GetCompanyId);
            if (data == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("level-business-rule/{id}")]
        public HttpResponseMessage GetBusinessRuleById(int id)
        {
            BusinessRuleViewModel data = repo.GetBusinessRuleById(id);
            if (data == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("level-business-rule/{id}")]
        public HttpResponseMessage UpdateBusinessRule([FromBody] BusinessRuleViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool data = repo.UpdateBusinessRule(model, id,user);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1, message = "The record has been updated successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("level-business-rule/{id}")]
        public HttpResponseMessage DeleteBusinessRule(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool data = repo.DeleteBusinessRule(id,user);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1, message = "The record has been deleted successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("dynamic-business-rule/{id}")]
        public HttpResponseMessage DeleteDynamicBusinessRule(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool data = repo.DeleteDynamicBusinessRule(id, user);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1, message = "The record has been deleted successfully" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
        }

    }
}