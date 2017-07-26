using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors; 
using FintrakBanking.APICore.JWTAuth;
 
using FintrakBanking.Common.Enum; 
using FintrakBanking.ViewModels.Business;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/business")]
    public class ApprovalWorkFlowController : BaseController
    {
        private IWorkFlowRepository workFlow;

        public ApprovalWorkFlowController(IWorkFlowRepository _workFlow)
        {
            this.workFlow = _workFlow;
        }


        #region test
        [HttpPost("workflow")]
        public  IActionResult GoForApproval([FromBody] ApprovalViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                 
                entity.targetId = 33;
                entity.approvalStatusId = entity.approvalStatusId ;
                entity.operationId =entity.operationId ;

                entity.staffId = token.GetStaffId;
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.applicationUrl = Request.Path.Value;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
               
                var response = workFlow.GoForApproval(entity);
                //var response = await workFlow .GoForApproval(entity);
                //if (response)
                //{
                    return Ok(new { success = true, message = "Operation completed successfully" });
                //}

               // return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        #endregion
    }
}