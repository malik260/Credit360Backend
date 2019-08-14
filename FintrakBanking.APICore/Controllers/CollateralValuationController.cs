using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-valuation-prerequisite")]
        public HttpResponseMessage AddValuationPrerequisite([FromBody] ValuationPrerequisiteViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;

            var response = _colValuationRepo.AddValuationPrerequisite(model);

            if (response != null)
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-collateral-valuation/{collteralValuationId}/collteralValuationId")]
        public HttpResponseMessage GetCollateralValuation(int collteralValuationId)
        {
            try
            {
                var valuation = _colValuationRepo.GetCollateralValuation(collteralValuationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = valuation });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the record. Error - {ex.Message}" });
            }
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
        [Route("get-valuation-Prerequisite/{valuationPrerequisiteId}/valuationPrerequisiteId")]
        public HttpResponseMessage GetCollateralValuationPrerequisiteById(int valuationPrerequisiteId)
        {
            try
            {
                var Prerequisites = _colValuationRepo.GetCollateralValuationPrerequisiteById(valuationPrerequisiteId);
                int totalItems = Prerequisites.Count();

                Prerequisites = Prerequisites.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = Prerequisites, count = totalItems });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-all-valuation-Prerequisites/{collateralValuationId}/collateralValuationId")]
        public HttpResponseMessage GetAllValuationPrerequisites(int collateralValuationId)
        {
            try
            {
                var Prerequisites = _colValuationRepo.GetAllValuationPrerequisitesById(collateralValuationId);
                int totalItems = Prerequisites.Count();

                Prerequisites = Prerequisites.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = Prerequisites, count = totalItems });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. Error - {ex.Message}" });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("add-valuer")]
        public HttpResponseMessage AddCollateralValuerInfo([FromBody] ValuationPrerequisiteViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = _colValuationRepo.AddCollateralValurerInfo(model);
                //int totalItems = requestTypes.Count();
                //requestTypes = requestTypes.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuer-info")]
        public HttpResponseMessage GetAllCollateralValuerIformation()
        {
            try
            {
                var response = _colValuationRepo.GetAllCollateralValuerIformation();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuer-info/{id}")]
        public HttpResponseMessage GetAllCollateralValuerIformation(int id)
        {
            try
            {
                var response = _colValuationRepo.GetAllCollateralValuerIformation(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuation-waiting-for-approval")]
        public HttpResponseMessage GetCollateralValuationRequestWaitingForApproval()
        {
            try
            {
                var response = _colValuationRepo.GetCollateralValuationRequestWaitingForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response});
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuation-waiting-for-approval/{collateralId}/collateralId")]
        public HttpResponseMessage GetAllValuationRequestWaitingForApproval(int collateralId)
        {
            try
            {
                var response = _colValuationRepo.GetAllValuationRequest(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("submit-approval")]
        public HttpResponseMessage SubmitApproval([FromBody] CollateralValuationViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = _colValuationRepo.SubmitApproval(model);
                //int totalItems = requestTypes.Count();
                //requestTypes = requestTypes.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. Error - {ex.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-valuation-prerequisite/{valuationPrerequisiteId}/valuationPrerequisiteId")]
        public HttpResponseMessage DeleteValuationPrerequisite(int valuationPrerequisiteId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = _colValuationRepo.DeleteValuationPrerequisite(valuationPrerequisiteId, user);

                if (data) {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The valuation prerequisite has been deleted successfully" });
                }
                else {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error deleting this valuation prerequisite" });
                }
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this valuation prerequisite {e.Message}" });
            }
        }

    }
}