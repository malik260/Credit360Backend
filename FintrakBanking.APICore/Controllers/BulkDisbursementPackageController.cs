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

        // for package processing
        [Route("disbursement-packages")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisbursementPackage()
        {
            try
            {
                var data = repo.GetAllBulkDisbursementPackageByCompany();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("disbursement-packages-group-id/{groupCustomerId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisbursementPackageByGroupId(int groupCustomerId)
        {
            try
            {
                var data = repo.GetAllBulkDisbursementPackageByGroupCustomerId(groupCustomerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("disbursement-package/{disbursementPackageId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBulkDisbursementPackageById(int disbursementPackageId)
        {
            try
            {
                var data = repo.GetBulkDisbursementPackageById(disbursementPackageId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }
        [Route("update-disbursement-package/{disbursementPackageId}")]
        [HttpPut]
        [ClaimsAuthorization]
        public HttpResponseMessage UpdateBulkDisbursementPackage(int disbursementPackageId, BulkDisbursementSetupPackageViewModel bulkDisbursement)
        {
            try
            {
                bulkDisbursement.companyId = token.GetCompanyId;

                bool response = repo.UpdateBulkDisbursementPackage(disbursementPackageId, bulkDisbursement);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("delete-disbursement-package/{disbursementPackageId}")]
        [HttpDelete]
        [ClaimsAuthorization]
        public HttpResponseMessage DeleteBulkDisbursementPackage(int disbursementPackageId, UserInfo bulkDisbursement)
        {
            try
            {
                bool response = repo.DeleteBulkDisbursementPackage(disbursementPackageId, bulkDisbursement);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("add-disbursement-package")]
        [HttpPost]
        [ClaimsAuthorization]
        public HttpResponseMessage AddBulkDisbursementPackage(BulkDisbursementSetupPackageViewModel bulkDisbursementSetup)
        {
            try
            {
                bulkDisbursementSetup.companyId = token.GetCompanyId;

                var data = repo.AddBulkDisbursementPackage(bulkDisbursementSetup);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
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
                var data = repo.GetAllBulkDisbursementScheme(token.GetCompanyId);
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

        [Route("disbursement-package-scheme-package-id/{disbursementPackageId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisbursementSchemeByPackageId(int disbursementPackageId)
        {
            try
            {
                var data = repo.GetAllBulkDisbursementSchemeByPackageId(disbursementPackageId);
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
        public HttpResponseMessage UpdateBulkDisbursementScheme(int disbursementSchemeId, BulkDisbursementSetupSchemeViewModel bulkDisbursement)
        {
            try
            {
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
        public HttpResponseMessage GetAllBulkDisbursementScheme(BulkDisbursementSetupSchemeViewModel bulkDisbursementSetup)
        {
            try
            {
                bulkDisbursementSetup.companyId = token.GetCompanyId;

                var data = repo.AddBulkDisbursementScheme(bulkDisbursementSetup);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        // for scheme fees processing
        [Route("disbursement-package-schemes-fees-disburse-id/{disburseSchemeId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetAllBulkDisbursementSchemeFees(int disburseSchemeId)
        {
            try
            {
                var data = repo.GetAllBulkDisbursementSchemeFeesDisburseSchemeId(disburseSchemeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("disbursement-package-scheme-fees")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBulkDisbursementSchemeFees()
        {
            try
            {
                var data = repo.GetAllBulkDisbursementSchemeFees();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }

        [Route("disbursement-package-scheme-fees-id/{schemeFeeId}")]
        [HttpGet]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBulkDisbursementSchemeFeesById(int schemeFeeId)
        {
            try
            {
                var data = repo.GetBulkDisbursementSchemeFeesById(schemeFeeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }

        }

        [Route("update-disbursement-package-scheme-fees/{schemeFeeId}")]
        [HttpPut]
        [ClaimsAuthorization]
        public HttpResponseMessage UpdateBulkDisbursementSchemeFees(int schemeFeeId, BulkDisbursementSetupSchemeFeesViewModel bulkDisbursement)
        {
            try
            {
                bulkDisbursement.companyId = token.GetCompanyId;

                bool response = repo.UpdateBulkDisbursementSchemeFees(schemeFeeId, bulkDisbursement);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [Route("delete-disbursement-scheme-fees/{schemeFeeId}")]
        [HttpDelete]
        [ClaimsAuthorization]
        public HttpResponseMessage DeleteBulkDisbursementSchemeFees(int schemeFeeId, UserInfo bulkDisbursement)
        {
            try
            {
                bool response = repo.DeleteBulkDisbursementSchemeFees(schemeFeeId, bulkDisbursement);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [Route("add-disbursement-package-scheme-fees")]
        [HttpPost]
        [ClaimsAuthorization]
        public HttpResponseMessage AddBulkDisbursementSchemeFees(BulkDisbursementSetupSchemeFeesViewModel bulkDisbursementSetup)
        {
            try
            {
                bulkDisbursementSetup.companyId = token.GetCompanyId;

                var data = repo.AddBulkDisbursementSchemeFees(bulkDisbursementSetup);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
    }
}
