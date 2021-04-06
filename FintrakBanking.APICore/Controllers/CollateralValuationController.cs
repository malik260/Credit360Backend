using FintrakBanking.APICore.core;
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
    public class CollateralValuationController : ApiControllerBase
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

        [HttpPut]
        [Route("edit-valuation-prerequisite/{valuationPrerequisiteId}/valuationPrerequisiteId")]
        public HttpResponseMessage UpdateAppraisalMemorandum(int valuationPrerequisiteId, [FromBody] ValuationPrerequisiteViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = _colValuationRepo.UpdateValuationPrerequisite(valuationPrerequisiteId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
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
        [Route("get-all-collateral-valuations-request-list")]
        public HttpResponseMessage GetAllValuationRequestList()
        {
            
                var valuations = _colValuationRepo.GetAllValuationRequestList();
                int totalItems = valuations.Count();
                valuations = valuations.OrderBy(x => x.dateTimeCreated).ToList();
            if (valuations != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = valuations, count = totalItems });
            }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Record(s) not found" });
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuation-Prerequisite/{valuationPrerequisiteId}/valuationPrerequisiteId")]
        public HttpResponseMessage GetCollateralValuationPrerequisiteById(int valuationPrerequisiteId)
        {
            try
            {
                var Prerequisite = _colValuationRepo.GetCollateralValuationPrerequisiteById(token.GetStaffId, valuationPrerequisiteId);
                //int totalItems = Prerequisites.Count();

                //Prerequisites = Prerequisites.OrderBy(x => x.dateTimeCreated).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = Prerequisite });
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
                var Prerequisites = _colValuationRepo.GetAllValuationPrerequisitesById(token.GetStaffId, collateralValuationId);
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
        [Route("get-all-valuation-Prerequisites-list/{collateralValuationId}/collateralValuationId")]
        public HttpResponseMessage GetAllValuationPrerequisitesList(int collateralValuationId)
        {
            try
            {
                var Prerequisites = _colValuationRepo.GetAllValuationPrerequisitesListById(token.GetStaffId, collateralValuationId);
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
        [Route("go-for-collateral-valuation-approval")]
        public HttpResponseMessage GoForApproval([FromBody] ValuationPrerequisiteViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = _colValuationRepo.GoForCollateralValuationApproval(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = _colValuationRepo.ResponseMessage(response, $"COLLATERAL VALUATION ({response.responseMessage})") });
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("update-valuer")]
        public HttpResponseMessage UpdateCollateralValuerInfo([FromBody] ValuationPrerequisiteViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = _colValuationRepo.UpdateCollateralValurerInfo(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res, message = $"Success" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record. Error - {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("update-valuer-narration")]
        public HttpResponseMessage UpdateCollateralNarration([FromBody] ValuationPrerequisiteViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = _colValuationRepo.UpdateCollateralNarration(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res, message = $"Success" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record. Error - {ex.Message}" });
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
        [Route("get-single-valuer-info/{id}")]
        public HttpResponseMessage GetCollateralValuerIformations(int id)
        {
            try
            {
                var response = _colValuationRepo.GetCollateralValuerIformations(id);
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
        public HttpResponseMessage GetCollateralValuerIformation(int id)
        {
            try
            {
                var response = _colValuationRepo.GetCollateralValuerIformation(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-valuer-info-detail/{id}")]
        public HttpResponseMessage GetAllCollateralValuerIformationById(int id)
        {
            try
            {
                var response = _colValuationRepo.GetAllCollateralValuerIformationById(id);
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
        [Route("search-for-collateral-valuation/{searchString}")]
        public HttpResponseMessage SearchForCollateralValuation(string searchString)
        {
            try
            {
                List<ValuationPrerequisiteViewModel> response = _colValuationRepo.SearchForCollateralValuation(searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
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
        [Route("submit-collateral-valuation-for-approval")]
        public HttpResponseMessage SubmitApproval([FromBody] ValuationPrerequisiteViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = _colValuationRepo.SubmitApproval(model);
              
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = _colValuationRepo.ResponseMessage(response, $"COLLATERAL VALUATION ({response.responseMessage})") });
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

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-valuation-prerequisite-status/{valuationPrerequisiteId}/valuationPrerequisiteId")]
        public HttpResponseMessage UpdateValuationPrerequisiteStatus(int valuationPrerequisiteId, [FromBody] ValuationPrerequisiteViewModel model)
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

                var data = _colValuationRepo.UpdateValuationPrerequisiteStatus(valuationPrerequisiteId, user);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The valuation prerequisite has been deleted successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating this valuation prerequisite" });
                }
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this valuation prerequisite {e.Message}" });
            }
        }
    }
}