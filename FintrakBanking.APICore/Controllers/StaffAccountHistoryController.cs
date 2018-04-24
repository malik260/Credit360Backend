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


        [HttpPost]
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
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        //public HttpResponseMessage ApproveStaffAccountHistory(StaffAccountHistoryViewModel entity)
        //{
        //    return accountHistory.ApproveStaffAccountHistory(entity);
        //}

        //public StaffAccountHistoryViewModel GetStaffAccountHistory(StaffAccountHistoryViewModel entity)
        //{
        //    return accountHistory.GetStaffAccountHistory(entity);
        //}

        //public HttpResponseMessage UpdateStaffAccountHistory(StaffAccountHistoryViewModel entity)
        //{
        //    return accountHistory.UpdateStaffAccountHistory(entity);
        //}
    }
}
