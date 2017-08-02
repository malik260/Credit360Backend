using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{ 
    [RoutePrefix("api/v1/setups")]
    public class ApprovalLevelStaffController : ApiControllerBase
    {
        private IApprovalLevelStaffRepository repo;

        public ApprovalLevelStaffController(IApprovalLevelStaffRepository _repo)
        {
            this.repo = _repo;
        }

        #region Approval Level Staff
        [HttpPost][Route("approval-level-staff")]
        public HttpResponseMessage AddApprovalLevelStaff([FromBody] ApprovalLevelStaffViewModel model)
        {   try
                {
                    var token =  new TokenDecryptionHelper();

                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.AddApprovalLevelStaff(model);
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

        [HttpGet][Route("approval-level-staff/operations/{operationMappingId}")]
        public HttpResponseMessage GetAllApprovalLevelStaff(int operationMappingId)
        {
               try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllApprovalLevelStaffByOperationId(operationMappingId, token.GetCompanyId);
                    if (!data.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                 new { success = false, message = "No record found" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                                 new { success = true, result = data, count = data.Count() });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                 new { success = false, message = $"Error: {e.Message}" });
                }
                  
        }

        [HttpGet][Route("approval-level-staff/staff-level/{id}")]
        public HttpResponseMessage GetApprovalLevelStaffById(int id)
        { 
                try
                {
                    TokenDecryptionHelper token = new TokenDecryptionHelper();

                    var data = repo.GetApprovalLevelStaffById(id, token.GetCompanyId);
                    return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = true, result = data, count = 1 });
                }
                catch (System.Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = false, message = $"There was an error updating this record {ex.Message}" });
                }
            
         
        }

        [HttpPut][Route("approval-level-staff/{id}")]
        public HttpResponseMessage UpdateApprovalLevelStaff([FromBody] ApprovalLevelStaffViewModel model, int id)
        {
              
                try
                {
                    var token =  new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.UpdateApprovalLevelStaff(id, model);

                    if (data)
                    {

                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, result = data, message = "The record has been updated successfully" });

                    }
                    return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "There was an error updating this record" });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = $"There was an error updating this record {e.Message}" });
                }
                 
            
        }

        [HttpDelete][Route("approval-level-staff/{StaffLevelId}")]
        public HttpResponseMessage DeleteApprovalLevelStaff(int StaffLevelId)
        {
               try
                {
                    var token =   new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        //applicationUrl = Request.Path.Value,
                        //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repo.DeleteApprovalLevelStaff(StaffLevelId, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                             new { success = true, result = StaffLevelId, message = "record has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                             new { success = false, message = ex.Message });
                }
                  
        }
        #endregion
    }
}