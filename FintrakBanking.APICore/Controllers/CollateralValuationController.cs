using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/valuation")]
    public class CollateralValuationController : ApiController
    {
        private ICollateralValuationRepository _colValuationRepo;
        private IValuationReportRepository _valuationRepo;
        private IValuationRequestTypeRepository _valuationRequestRepo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CollateralValuationController(ICollateralValuationRepository colValuationRepo, IValuationReportRepository valuationRepo, IValuationRequestTypeRepository valuationRequestRepo)
        {
            _colValuationRepo = colValuationRepo;
            _valuationRepo = valuationRepo;
            _valuationRequestRepo = valuationRequestRepo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-valuation-reports")]
        public HttpResponseMessage GetAllValuationReports()
        {
            try {
                var reports = _valuationRepo.GetAllValuationReports();
                int totalItems = reports.Count();

                reports = reports.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = reports, count = totalItems });
            }
            catch (SecureException ex) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-valuation-report")]
        public HttpResponseMessage AddValuationReport([FromBody] ValuationReportViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;

            var response = _valuationRepo.AddValuationReport(model);

            if (response != null)
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-collateral-valuation")]
        public HttpResponseMessage AddCollateralValuation([FromBody] CollateralValuationViewModel model)
        {
            model.userBranchId = (short) token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;

            var response = _colValuationRepo.AddCollateralValuation(model);

            if (response != null)
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-collateral-valuations/{collateralId}/collateralId")]
        public HttpResponseMessage GetAllCollateralValuations(int collateralId)
        {
            try
            {
                var valuations = _colValuationRepo.GetAllCollateralValuations(collateralId);
                int totalItems = valuations.Count();

                valuations = valuations.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = valuations, count = totalItems });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuation-request-types")]
        public HttpResponseMessage GetAllValuationRequestTypes()
        {
            try {
                var requestTypes = _valuationRequestRepo.GetAllValuationRequestTypes();
                int totalItems = requestTypes.Count();

                requestTypes = requestTypes.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = requestTypes, count = totalItems });
            }
            catch (SecureException ex) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("go-for-approval")]
        public HttpResponseMessage GoForApproval([FromBody] CollateralValuationViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = _colValuationRepo.GoForCollateralValuationApproval(model);
                //int totalItems = requestTypes.Count();
                //requestTypes = requestTypes.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error pushing the request for approval. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuation-waiting-for-approval")]
        public HttpResponseMessage GetAllValuationRequestWaitingForApproval()
        {
            try
            {
                var response = _colValuationRepo.GetAllValuationRequestWaitingForApproval(token.GetStaffId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response});
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }
    }
}