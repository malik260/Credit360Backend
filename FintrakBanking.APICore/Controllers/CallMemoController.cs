using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
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
    [RoutePrefix("api/v1/credit")]
    public class CallMemoController : ApiControllerBase
    {
        private readonly ICallMemoRepository repo;
        private readonly TokenDecryptionHelper token = new TokenDecryptionHelper();
        public CallMemoController(ICallMemoRepository _repo)
        {
            repo = _repo;
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("loan-search/")]
        public HttpResponseMessage SearchForCallMemoLoan(string searchQuery)
        {
            try
            {
                var data = repo.SearchForCallMemoLoan(token.GetStaffId, searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $" {ce.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
            

        }
        #region "Call Limit"
      [HttpGet] [ClaimsAuthorization]  
        [Route("call-limit-type")]
        public HttpResponseMessage GetCallLimitType()
        {
            try
            {

                var response = repo.GetCallLimitType();
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("call-limit")]
        public HttpResponseMessage GetAllCallLimit()
        {
            try
            {

                var response = repo.GetAllCallLimit(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("call-limit-type/{limitId}")]
        public HttpResponseMessage GetCallLimitByTypeId(int limitId)
        {
            try
            {

                var response = repo.GetCallLimitByTypeId(limitId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("call-limit")]
        public HttpResponseMessage AddCallLimit([FromBody] CallLimitViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                if (repo.isLimitExist(model))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Call Limit setup already exist for the selected role" });
                }
                var response = repo.AddCallLimit(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("call-limit/{limitId}")]
        public HttpResponseMessage UpdateCallLimit(int LimitId, [FromBody] CallLimitViewModel model)
        {
            try
            {
                ;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateCallLimit(LimitId, model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("call-limit/{limitId}")]
        public HttpResponseMessage DeleteCallLimit(int LimitId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };
                repo.DeleteCallLimit(LimitId, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LimitId, message = "record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion
        #region "Call Memo"

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-call-memo/{customerId}/customerId")]
        public HttpResponseMessage GetCustomerCallMemo(int customerId)
        {
            try
            {
                var response = repo.GetCustomerCallMemo(token.GetStaffId, customerId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-approved-call-memo/{customerId}/customerId")]
        public HttpResponseMessage GetCustomerApprovedCallMemo(int customerId)
        {
            try
            {
                var response = repo.GetCustomerApprovedCallMemo(token.GetStaffId, customerId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-call-memo-waiting-for-approval")]
        public HttpResponseMessage GetCallMemoWaitingForApproval()
        {
            try
            {
                var response = repo.GetCallMemoWaitingForApproval(token.GetStaffId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("call-getMemo/{callMemoId}/callMemoId")]
        public HttpResponseMessage GetCallMemoById(int callMemoId)
        {
            try
            {
                var response = repo.GetCallMemoById(callMemoId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("search-call-memo")]
        public HttpResponseMessage SearchCallMemo([FromBody] CallMemoViewModel model)
        {
            try
            {
                var response = repo.SearchCallMemo(token.GetStaffId, model);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("call-getMemo")]
        public HttpResponseMessage GetAllCallMemo()
        {
            try
            {

                IEnumerable<CallMemoViewModel> response = repo.GetAllCallMemo(token.GetStaffId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("call-memo")]
        public HttpResponseMessage AddCallMemo([FromBody] CallMemoViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.staffId = token.GetStaffId;
                model.companyId = token.GetCompanyId;
        
                var response = repo.AddCallMemo(model);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("call-memo/{memoId}")]
        public HttpResponseMessage UpdateCallMemo(int memoId, [FromBody] CallMemoViewModel model)
        {
            try
            {
                //;
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateCallMemo(memoId, model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("go-for-approval")]
        public HttpResponseMessage GoForApproval([FromBody] CallMemoViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = repo.GoForCallMemoApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("submit-approval")]
        public HttpResponseMessage SubmitApproval([FromBody] CallMemoViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = repo.SubmitApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. Error - {ex.Message}" });
            }
        }
        #endregion
    }
}
