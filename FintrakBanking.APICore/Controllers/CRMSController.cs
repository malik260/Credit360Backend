using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.CRMS;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/crms")]
    public class CRMSController : ApiController
    {
        private ICRMSRegulatories repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CRMSController(ICRMSRegulatories _repo)
        {
            this.repo = _repo;
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("regulatory/crms-code")]
        public HttpResponseMessage AddCRMSCode([FromBody] CRMSViewModel customer)
        {
            try
            {
                customer.companyId=  token.GetCompanyId;
                var data = repo.AddCRMSCode(customer);
                if (data ==null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data});
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { data ="no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("regulatory/all-loan-with-crms-code")]
        public HttpResponseMessage GetAllLoans([FromBody]CRMSViewModel model)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                var data = repo.GetAllLoansForCRMS(model);
                var dataCount = repo.LoanCountsByLegalStatus(data);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count= dataCount });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("regulatory/export")]
        public HttpResponseMessage ExportScheduleToExcel([FromBody] CRMSViewModel model)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                var fileBytes = repo.GenerateCBNReport(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record",  success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }

            }
        

        [HttpPost]
        [ClaimsAuthorization]
        [Route("postingdetail/export")]
        public HttpResponseMessage ExportPostingDetailToExcel([FromBody] CRMSViewModel model)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                var fileBytes = repo.GenerateCBNReport(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }

        }

    }
}
