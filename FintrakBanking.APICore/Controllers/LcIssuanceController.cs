using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class LcIssuanceController : ApiControllerBase
    {
        private ILcIssuanceRepository repo;
        private ILcConditionRepository conditionRepo;
        private ILcDocumentRepository documentRepo;
        private ILcShippingRepository shippingRepo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LcIssuanceController(
            ILcIssuanceRepository _repo,
            ILcConditionRepository _conditionRepo,
            ILcDocumentRepository _documentRepo,
            ILcShippingRepository _shippingRepo
            )
        {
            this.repo = _repo;
            this.conditionRepo = _conditionRepo;
            this.documentRepo = _documentRepo;
            this.shippingRepo = _shippingRepo;
        }

        #region LCISSUANCE
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance")]
        public HttpResponseMessage GetLcIssuances()
        {
            IEnumerable<LcIssuanceViewModel> response = repo.GetLcIssuances();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-search")]
        public HttpResponseMessage SearchLc(string searchString)
        {
            IEnumerable<LcIssuanceApprovalViewModel> response = repo.SearchLc(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/approval")]
        public HttpResponseMessage GetLcIssuancesForApproval()
        {
            IEnumerable<LcIssuanceApprovalViewModel> response = repo.GetLcIssuancesForApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/lines/{customerId}")]
        public HttpResponseMessage GetIFFLinesForLCByCustomerId(int customerId)
        {
            IEnumerable<CamProcessedLoanViewModel> response = repo.GetIFFLinesForLCByCustomerId(customerId, token.GetCompanyId, token.GetStaffId, token.GetBranchId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("lc-issuance/{id}")]
        //public HttpResponseMessage GetLcIssuance(int id)
        //{
        //    LcIssuanceViewModel response = repo.GetLcIssuance(id);
        //    if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        //}

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lc-issuance")]
        public HttpResponseMessage AddLcIssuance([FromBody] LcIssuanceViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddLcIssuance(model);
            if (response.lcIssuanceId > 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lc-issuance/{id}")]
        public HttpResponseMessage UpdateLcIssuance([FromBody] LcIssuanceViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateLcIssuance(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lc-issuance/{id}")]
        public HttpResponseMessage DeleteLcIssuance(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteLcIssuance(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
        }
        #endregion LCISSUANCE

        #region LCDOCUMENT
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-document")]
        public HttpResponseMessage GetLcDocuments()
        {
            IEnumerable<LcDocumentViewModel> response = documentRepo.GetLcDocuments();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-documents/{lcIssuanceId}")]
        public HttpResponseMessage GetLcDocumentsByIssuanceId(int lcIssuanceId)
        {
            IEnumerable<LcDocumentViewModel> response = documentRepo.GetLcDocumentsBylcIssuanceId(lcIssuanceId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-document/{id}")]
        public HttpResponseMessage GetLcDocument(int id)
        {
            LcDocumentViewModel response = documentRepo.GetLcDocument(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lc-document")]
        public HttpResponseMessage AddLcDocument([FromBody] LcDocumentViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = documentRepo.AddLcDocument(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lc-document/{id}")]
        public HttpResponseMessage UpdateLcDocument([FromBody] LcDocumentViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = documentRepo.UpdateLcDocument(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lc-document/{id}")]
        public HttpResponseMessage DeleteLcDocument(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = documentRepo.DeleteLcDocument(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
        }
        #endregion LCDOCUMENT

        #region SHIPPING
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-shipping")]
        public HttpResponseMessage GetLcShippings()
        {
            IEnumerable<LcShippingViewModel> response = shippingRepo.GetLcShippings();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-shippings/{lcIssuanceId}")]
        public HttpResponseMessage GetLcShippingsByLcIssuanceId(int lcIssuanceId)
        {
            IEnumerable<LcShippingViewModel> response = shippingRepo.GetLcShippingsByIssuanceId(lcIssuanceId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-shipping/{id}")]
        public HttpResponseMessage GetLcShipping(int id)
        {
            LcShippingViewModel response = shippingRepo.GetLcShipping(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lc-shipping")]
        public HttpResponseMessage AddLcShipping([FromBody] LcShippingViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = shippingRepo.AddLcShipping(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lc-shipping/{id}")]
        public HttpResponseMessage UpdateLcShipping([FromBody] LcShippingViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = shippingRepo.UpdateLcShipping(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lc-shipping/{id}")]
        public HttpResponseMessage DeleteLcShipping(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = shippingRepo.DeleteLcShipping(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
        }
        #endregion SHIPPING

        #region LCCONDITIONS
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-condition")]
        public HttpResponseMessage GetLcConditions()
        {
            IEnumerable<LcConditionViewModel> response = conditionRepo.GetLcConditions();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-conditions/{lcIssuanceId}")]
        public HttpResponseMessage GetLcConditionsBylcIssuanceId(int lcIssuanceId)
        {
            IEnumerable<LcConditionViewModel> response = conditionRepo.GetLcConditionsBylcIssuanceId(lcIssuanceId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-condition/{id}")]
        public HttpResponseMessage GetLcCondition(int id)
        {
            LcConditionViewModel response = conditionRepo.GetLcCondition(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lc-condition")]
        public HttpResponseMessage AddLcCondition([FromBody] LcConditionViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = conditionRepo.AddLcCondition(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lc-condition/{id}")]
        public HttpResponseMessage UpdateLcCondition([FromBody] LcConditionViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = conditionRepo.UpdateLcCondition(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lc-condition/{id}")]
        public HttpResponseMessage DeleteLcCondition(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = conditionRepo.DeleteLcCondition(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
        }
        #endregion LCCONDITIONS

        #region RELEASEOFSHIPPINGDOCUMENTS
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/release")]
        public HttpResponseMessage GetLcIssuancesForRelease()
        {
            IEnumerable<LcIssuanceViewModel> response = repo.GetLcIssuancesForRelease();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/release-approval")]
        public HttpResponseMessage GetLcIssuancesForReleaseApproval()
        {
            IEnumerable<LcIssuanceApprovalViewModel> response = repo.GetLcIssuancesForReleaseApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }
        #endregion RELEASEOFSHIPPINGDOCUMENTS
    }
}
