using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.ViewModels.Setups.General;
using System;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class StaffRoleController : ApiControllerBase
    {
        private IStaffRoleRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //public RankController(IRankRepository _repo)
        //{
        //    this.repo = _repo;
        //}
        public StaffRoleController(IStaffRoleRepository _repo)
        {
            this.repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("staff-role")]
        public HttpResponseMessage GetStaffRole()
        { 
            try
            {
                var data = repo.GetStaffRole();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("staff-role-by-staffid")]
        public HttpResponseMessage GetStaffRoleByStaffId()
        {
            try
            {
                
                var data = repo.GetStaffRoleByStaffId(token.GetStaffId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
               
        [HttpGet] [ClaimsAuthorization]  
        [Route("staff-role/{staffRoleId}")]
        public HttpResponseMessage GetStaffRole(int rankId)
        {
            try
            {
                var data = repo.GetStaffRole(rankId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("staff-role/company")]
        public HttpResponseMessage GetStaffRoleByCompanyId()
        {
            try
            {
                var data = repo.GetStaffRoleByCompanyId(token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("default-staff-role")]
        public HttpResponseMessage GetStaffRoles()
        {
            try
            {
                var data = repo.GetStaffRoles();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-flow-type")]
        public HttpResponseMessage GetAllApprovalFlowTypes()
 
            {
                IEnumerable<ApprovalFlowTypeViewModel> response = repo.GetAllApprovalFlowTypes();
                if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }


        [HttpPost] [ClaimsAuthorization]
        [Route("staff-role")]
        public HttpResponseMessage AddUpdateStaffRole([FromBody] StaffRoleViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.staffRoleId != 0 || entity.staffRoleId > 0)
                {
                    createUpdate = "updated";
                    if (repo.ValidateStaffRoleUpdate(entity.staffRoleId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                               new { success = false, message = "This Staff Role is undergoing approval."
                                               + Environment.NewLine + "Approve previous entry to continue."});
                    }
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateStaffRole(entity.staffRoleCode, entity.staffRoleName))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                               new { success = false, message = "Staff Role with same Name or Code already exist." });
                    }
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateStaffRole(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully and sent for approval" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }


        [HttpGet] [ClaimsAuthorization]  
        [Route("staff-role-approval")]
        public HttpResponseMessage GetStaffRoleAwaitingApproval()
        {
            try
            {
                var staffinfo = repo.GetStaffRoleAwaitingApproval(token.GetStaffId, token.GetCompanyId);

                if (staffinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
         [HttpPost] [ClaimsAuthorization]
        [Route("staff-role/approval")]
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

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Staff Role has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("approval-setup")]
        public HttpResponseMessage AddApprovalSetUp([FromBody] ApprovalSetUpViewModel entity)
        {
            try
            {
                var data = repo.AddApprovalSetUp(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "SetUp has been added successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-setup-table")]
        public HttpResponseMessage GetApprovalSetUp()
        {
            try
            {
                var data = repo.GetApprovalSetup();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPut]
        [ClaimsAuthorization]
        [Route("approval-setup-update")]
        public HttpResponseMessage UpdateApprovalSetUp([FromBody] ApprovalSetUpViewModel entity)
        {
            try
            {
                var data = repo.UpdateApprovalSetUp(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "SetUp has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful" });
            }
            catch (SecureException ex)
            {
               
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        


        [HttpGet]
        [ClaimsAuthorization]
        [Route("operation-all")]
        public HttpResponseMessage GetAllOperationOrder()
        {
            try
            {
                var data = repo.GetAllOperationOrder();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("all-operations")]
        public HttpResponseMessage GetAllOperation()
        {
            try
            {
                var data = repo.GetAllOperations();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("flow-order-add")]
        public HttpResponseMessage AddFlowOrder([FromBody] OperationPageOrderViewModel entity)
        {
            try
            {
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                
                bool data = repo.AddFlowOrder(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "FlowOrder has been added successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (SecureException ex)
            {
                //errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-flow-order")]
        public HttpResponseMessage UpdateFlowOrder([FromBody] OperationPageOrderViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                bool data = repo.UpdateFlowOrder(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "SetUp has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }
}