using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;
using System.Web;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/loanoperation")]
    public class StaffAccountHistoryController : ApiControllerBase
    {
        
        IStaffAccountHistoryRepository accountHistory;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public StaffAccountHistoryController(IStaffAccountHistoryRepository accountHistory)
        {
            this.accountHistory = accountHistory;
        }



         [HttpPost] [ClaimsAuthorization]
        [Route("approve-reasign-account")]
        public HttpResponseMessage ApproveStaffAccountHistory(ReasignedAccountApprovalViewModel entity)
        {

            try
            {
              
                entity.userIPAddress = CommonHelpers.GetUserIP();
                entity.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;

                var data = accountHistory.ApproveStaffAccountHistory(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, message = "The record has been created successfully" });
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
        [Route("reasign-account")]
        public HttpResponseMessage AddStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
            
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = CommonHelpers.GetUserIP();
                entity.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;

                var data = accountHistory.AddStaffAccountHistory(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, message = "The record has been created successfully" });
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

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-reasigned-account-awaiting-approval")]
        public HttpResponseMessage GetStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {

            try
            {
                var response = accountHistory.GetStaffAccountHistory(this.token.GetStaffId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }



        

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-all-reasigned-account")]
        public HttpResponseMessage   GetAllStaffAccountHistory()
        {
            try
            {
                var response = accountHistory.GetAllStaffAccountHistory(); ;
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("get-all-reasigned-account/loan/{loanId}/productType/{productTypeId}")]
        public HttpResponseMessage GetSelectedLoanDetails(int loanId, int productTypeId)
        {              
            try
            {
                var response = accountHistory.GetSelectedLoanDetails(token.GetCompanyId, loanId, productTypeId);  
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }



         [HttpPost] [ClaimsAuthorization]
        [Route("selected-reasigned-account")]
        public HttpResponseMessage GetSelectedApprovalLoanDetails(ReasignedAccountApprovalViewModel entity)
        {
            try
            {
                entity.companyId = token.GetCompanyId;
                var response = accountHistory.GetSelectedApprovalLoanDetails(entity);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
    }
}
