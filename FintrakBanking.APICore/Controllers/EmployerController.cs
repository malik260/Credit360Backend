using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class EmployerController : ApiControllerBase
    {
       
        private IEmployerRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public EmployerController(IEmployerRepository _repo)
        {
            repo = _repo;
        }

        #region employer

        [HttpGet] [ClaimsAuthorization]  
        [Route("employers")]
        public HttpResponseMessage getEmployer()
        {
            try
            {
                var data = repo.getEmployer(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-pending-employers")]
        public HttpResponseMessage getAllPendingEmployers()
        {
            try
            {
                var data = repo.getAllPendingEmployers(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-approved-employers")]
        public HttpResponseMessage getAllApprovedEmployers()
        {
            try
            {
                var data = repo.getAllApprovedEmployers(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("forward-for-related-employer-approval")]
        public HttpResponseMessage ForwardRelatedEmployerForApproval([FromBody] EmployerViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.staffId = token.GetStaffId;
            //entity.applicationUrl = HttpContext.Current.Request.Path;

            WorkflowResponse response = repo.ForwardRelatedEmployerForApproval(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "RELATED EMPLOYER") });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-related-employer-waiting-for-approval")]
        public HttpResponseMessage GetRelatedEmployersWaitingForApproval()
        {
            IEnumerable<EmployerViewModel> response = repo.GetRelatedEmployersWaitingForApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("employer")]
        public HttpResponseMessage getEmployer(int employerId)
        {
            try
            {
                var data = repo.getEmployer(employerId,token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("add-employer")]
        public HttpResponseMessage addEmployer(EmployerViewModel employer)
        {
            try
            {
                employer.companyId = token.GetCompanyId;
                employer.staffId = token.GetStaffId;
                employer.createdBy = token.GetStaffId;
                employer.userBranchId = (short)token.GetBranchId;

                var data = repo.addEmployer(employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
       [HttpPut] [ClaimsAuthorization]
        [Route("update-employer/{employerId}")]
        public HttpResponseMessage updateEmployer(int employerId,EmployerViewModel employer)
        {
            try
            {
                employer.companyId = token.GetCompanyId;
                employer.staffId = token.GetStaffId;
                employer.userBranchId = (short)token.GetBranchId;

                var data = repo.updateEmployer(employerId,employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            } 
        }
        [HttpDelete] [ClaimsAuthorization]
        [Route("delete-employer/{employerId}")]
        public HttpResponseMessage deleteEmployer(int employerId, EmployerViewModel employer)
        {
            try
            {
                var data = repo.deleteEmployer(employerId, employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }


        [HttpGet] [ClaimsAuthorization]  
        [Route("employer-type")]
        public HttpResponseMessage getEmployerType()
        {
            try
            {
                var data = repo.getEmployerType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("employer-sub-type/{EmployerTypeId}")]
        public HttpResponseMessage getEmployerSubType(int employerTypeId)
        {
            try
            {
                var data = repo.getEmployerSunType(employerTypeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-employer-sub-types")]
        public HttpResponseMessage getAllEmployerSubTypes()
        {
            try
            {
                var data = repo.getAllEmployerSubTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region employer type


        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-employer-type")]
        public HttpResponseMessage addEmployerType(EmployerViewModel employerType)
        {
            try
            {
                employerType.companyId = token.GetCompanyId;
                employerType.staffId = token.GetStaffId;
                employerType.userBranchId = (short)token.GetBranchId;

                var data = repo.addEmployerType(employerType);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-employer-type/{employerTypeId}")]
        public HttpResponseMessage updateEmployerType(int employerTypeId, EmployerViewModel employer)
        {
            try
            {
                employer.companyId = token.GetCompanyId;
                employer.staffId = token.GetStaffId;
                employer.userBranchId = (short)token.GetBranchId;

                var data = repo.updateEmployerType(employerTypeId, employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-employer-type/{employerTypeId}")]

        public HttpResponseMessage deleteEmployerType(int employerTypeId, EmployerViewModel employer)
        {
            try
            {
                var data = repo.deleteEmployerType(employerTypeId, employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        #endregion


        #region employer sub type
        [HttpGet]
        [ClaimsAuthorization]
        [Route("all-employer-sub-type")]
        public HttpResponseMessage getEmployerSubTypes()
        {
            try
            {
                var data = repo.getEmployerSubType(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-employer-sub-type")]
        public HttpResponseMessage addEmployerSubType(EmployerViewModel employerSubType)
        {
            try
            {
                employerSubType.companyId = token.GetCompanyId;
                employerSubType.staffId = token.GetStaffId;
                employerSubType.userBranchId = (short)token.GetBranchId;

                var data = repo.addEmployerSubType(employerSubType);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-employer-sub-type/{employerSubTypeId}")]
        public HttpResponseMessage updateEmployerSubType(int employerSubTypeId, EmployerViewModel employer)
        {
            try
            {
                employer.companyId = token.GetCompanyId;
                employer.staffId = token.GetStaffId;
                employer.userBranchId = (short)token.GetBranchId;

                var data = repo.updateEmployerSubType(employerSubTypeId, employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-employer-type/{employerTypeId}")]

        public HttpResponseMessage deleteEmployerSubType(int employerSubTypeId, EmployerViewModel employer)
        {
            try
            {
                var data = repo.deleteEmployerSubType(employerSubTypeId, employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}
