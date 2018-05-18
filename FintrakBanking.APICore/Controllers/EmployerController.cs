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

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class EmployerController : ApiController
    {
       
        private IEmployerRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public EmployerController(IEmployerRepository _repo)
        {
            repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("employers")]
        public HttpResponseMessage getEmployer()
        {
            try
            {
                var data = repo.getEmployer(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
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
            catch (Exception ex)
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
                employer.userBranchId = (short)token.GetBranchId;

                var data = repo.addEmployer(employer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
    }
}
