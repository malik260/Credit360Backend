using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/camsol")]
    public class LoanCamSolController : ApiControllerBase
    {
        private ILaonCamSolRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LoanCamSolController(ILaonCamSolRepository _repo)
        {
            repo = _repo;
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol")]
        public HttpResponseMessage GetAllCamsol()
        {
            try
            {
                var data = repo.GetCamSol();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-search/{searchValue}")]
        public HttpResponseMessage GetLoanCamsolSearch(string searchValue)
        {
            try
            {
                var data = repo.GetCamSol(searchValue);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-approval")]
        public HttpResponseMessage GetLoanCamsolAwaitingApproval ()
        {
            try
            {
                var staff = token.GetCompanyId;
                var companyId = token.GetStaffId;

                var data = repo.CamSolAwaitingApproval(companyId,staff);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-type-id/{id}")]
        public HttpResponseMessage GetLoanCamsolById(int id)
        {
            try
            {
                var data = repo.GetCamSolByType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-customer-code/{customercode}")]
        public HttpResponseMessage GetLoanCamsolByCustomerCode(string  customercode)
        {
            try
            {
                var data = repo.GetCamSolByCustomerCode(customercode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-loan-camsol-type/{id}")]
        public HttpResponseMessage ViewLoanCamsolById(int id)
        {
            try
            {
                var data = repo.ViewCamSolByType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("camsol-approval-type/{id}")]
        public HttpResponseMessage GetCamsolAwaitingApprovalById(int id)
        {
            try
            {
                var data = repo.CamSolAwaitingApprovalById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("go-for-camsol-approval")]
        public HttpResponseMessage goForApproval([FromBody] LoanCAMSOLViewModel data)
        {
            try
            {
                data.companyId = token.GetCompanyId;
                data.createdBy = (short)token.GetStaffId;
                var val = repo.goForApproval(data);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = val });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("loan-camsol-type")]
        public HttpResponseMessage GetLoanCansolType()
        {
            try
            {
                var data = repo.GetCamSolType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("approve-loan-camsol")]
        public HttpResponseMessage ApproveCamsol([FromBody] LoanCAMSOLViewModel updateOptions)
        {
            try
            {
                var data = repo.ApproveCamsol(updateOptions);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
