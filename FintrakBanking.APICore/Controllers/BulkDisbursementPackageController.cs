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
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class BulkDisbursementPackageController : ApiControllerBase
    {
        private IBulkDisbursementPackageRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public BulkDisbursementPackageController(IBulkDisbursementPackageRepository _repo)
        {
            repo = _repo;
        }
        public IEnumerable<string> GetAllExceptionMessages(Exception ex)
        {
            Exception currentEx = ex;
            yield return currentEx.Message;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                yield return currentEx.Message;
            }
        }

        // for scheme processing
        [Route("disbursement-package-schemes")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisbursementScheme()
        {
            try
            {
                var data = repo.GetAllBulkDisbursementScheme();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("disbursement-package-scheme-id/{disburseSchemeId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBulkDisbursementSchemeBySchemeId(int disburseSchemeId)
        {
            try
            {
                var data = repo.GetAllBulkDisbursementSchemeByDisburseSchemeId(disburseSchemeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }

        [Route("disbursement-package-scheme-product/{productId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisbursementSchemeByProductId(int productId)
        {
            try
            {
                var data = repo.GetAllBulkDisbursementSchemeByProductId(productId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }

        [Route("disbursement-package-scheme-reference-number/{referenceNumber}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisburseSchemeByApplicationReferenceNumber(string referenceNumber)
        {
            try
            {
                var data = repo.GetAllBulkDisburseSchemeByApplicationReferenceNumber(referenceNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }


        [Route("update-disbursement-package-scheme/{disbursementSchemeId}")]
        [HttpPut]
        [ClaimsAuthorization]
        public HttpResponseMessage UpdateBulkDisbursementScheme([FromUri]int disbursementSchemeId, [FromBody] BulkDisbursementSetupSchemeViewModel bulkDisbursement)
        {
            try
            {
                bulkDisbursement.userBranchId = (short)token.GetBranchId;
                bulkDisbursement.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                bulkDisbursement.applicationUrl = HttpContext.Current.Request.Path;
                bulkDisbursement.lastUpdatedBy = token.GetStaffId;
                bulkDisbursement.companyId = token.GetCompanyId;

                bool response = repo.UpdateBulkDisbursementScheme(disbursementSchemeId, bulkDisbursement);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("delete-disbursement-package-scheme/{disbursementSchemeId}")]
        [HttpDelete]
        [ClaimsAuthorization]
        public HttpResponseMessage DeleteBulkDisbursementScheme(int disbursementSchemeId, UserInfo bulkDisbursement)
        {
            try
            {
                bool response = repo.DeleteBulkDisbursementScheme(disbursementSchemeId, bulkDisbursement);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("add-disbursement-scheme")]
        [HttpPost]
        [ClaimsAuthorization]
        public HttpResponseMessage AddBulkDisbursementScheme(BulkDisbursementSetupSchemeViewModel bulkDisbursementSetup)
        {
            try
            {
                

                bulkDisbursementSetup.staffId = token.GetStaffId;
                bulkDisbursementSetup.userBranchId = (short)token.GetBranchId;
                bulkDisbursementSetup.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                bulkDisbursementSetup.applicationUrl = HttpContext.Current.Request.Path;
                bulkDisbursementSetup.createdBy = token.GetStaffId;
                bulkDisbursementSetup.companyId = token.GetCompanyId;
                
                var data = repo.AddBulkDisbursementScheme(bulkDisbursementSetup);
                if (data == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = data });

                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("scheme-setup-search/search/{searchString}")]
        public HttpResponseMessage GetLoanApplicationSearch([FromUri] string searchString)
        {
            try
            {
                var response = repo.SchemeSearch(searchString);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + searchString, result = response });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("loan-application-details/search")]
        //public HttpResponseMessage SearchLoanApplicationDetails([FromBody] SearchViewModel model)
        //{
        //    try
        //    {

        //       var response = repo.SearchLoanApplicationDetails(token.GetCompanyId, model.searchString);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Search result for " + model.searchString, result = response });
        //    }
        //    catch (SecureException e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }
        //}

    }
}
