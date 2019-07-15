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
        private ILcUssanceRepository ussanceRepo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LcIssuanceController(
            ILcIssuanceRepository _repo,
            ILcConditionRepository _conditionRepo,
            ILcDocumentRepository _documentRepo,
            ILcShippingRepository _shippingRepo,
            ILcUssanceRepository _ussanceRepo
            )
        {
            this.repo = _repo;
            this.conditionRepo = _conditionRepo;
            this.documentRepo = _documentRepo;
            this.shippingRepo = _shippingRepo;
            this.ussanceRepo = _ussanceRepo;
        }

        #region LCISSUANCE
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance")]
        public HttpResponseMessage GetLcIssuances()
        {
            try
            {
                IEnumerable<LcIssuanceViewModel> response = repo.GetLcIssuances();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-search/{searchString}")]
        public HttpResponseMessage SearchLc(string searchString)
        {
            try
            {
                List<LcIssuanceApprovalViewModel> response = repo.SearchLc(searchString);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/approval")]
        public HttpResponseMessage GetLcIssuancesForApproval()
        {
            try
            {
                IEnumerable<LcIssuanceApprovalViewModel> response = repo.GetLcIssuancesForApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/lines/{customerId}")]
        public HttpResponseMessage GetIFFLinesForLCByCustomerId(int customerId)
        {
            try
            {
                IEnumerable<CamProcessedLoanViewModel> response = repo.GetIFFLinesForLCByCustomerId(customerId, token.GetCompanyId, token.GetStaffId, token.GetBranchId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                var response = repo.AddLcIssuance(model);
                if (response.lcIssuanceId > 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                bool response = repo.UpdateLcIssuance(model, id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                bool response = repo.DeleteLcIssuance(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion LCISSUANCE

        #region LCDOCUMENT
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-document")]
        public HttpResponseMessage GetLcDocuments()
        {
            try
            {
                IEnumerable<LcDocumentViewModel> response = documentRepo.GetLcDocuments();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-documents/{lcIssuanceId}")]
        public HttpResponseMessage GetLcDocumentsByIssuanceId(int lcIssuanceId)
        {
            try
            {
                IEnumerable<LcDocumentViewModel> response = documentRepo.GetLcDocumentsBylcIssuanceId(lcIssuanceId);
                if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-document/{id}")]
        public HttpResponseMessage GetLcDocument(int id)
        {
            try
            {
                LcDocumentViewModel response = documentRepo.GetLcDocument(id);
                if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                var response = documentRepo.AddLcDocument(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                bool response = documentRepo.UpdateLcDocument(model, id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                bool response = documentRepo.DeleteLcDocument(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion LCDOCUMENT

        #region SHIPPING
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-shipping")]
        public HttpResponseMessage GetLcShippings()
        {
            try
            {
                IEnumerable<LcShippingViewModel> response = shippingRepo.GetLcShippings();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-shippings/{lcIssuanceId}")]
        public HttpResponseMessage GetLcShippingsByLcIssuanceId(int lcIssuanceId)
        {
            try
            {
                IEnumerable<LcShippingViewModel> response = shippingRepo.GetLcShippingsByIssuanceId(lcIssuanceId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-shipping/{id}")]
        public HttpResponseMessage GetLcShipping(int id)
        {
            try
            {
                LcShippingViewModel response = shippingRepo.GetLcShipping(id);
                if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                var response = shippingRepo.AddLcShipping(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                bool response = shippingRepo.UpdateLcShipping(model, id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
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
            try
            {
                bool response = shippingRepo.DeleteLcShipping(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }
        #endregion SHIPPING

        #region LCCONDITIONS
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-condition")]
        public HttpResponseMessage GetLcConditions()
        {
            try
            {
                IEnumerable<LcConditionViewModel> response = conditionRepo.GetLcConditions();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-conditions/{lcIssuanceId}")]
        public HttpResponseMessage GetLcConditionsBylcIssuanceId(int lcIssuanceId)
        {
            try
            {
                IEnumerable<LcConditionViewModel> response = conditionRepo.GetLcConditionsBylcIssuanceId(lcIssuanceId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-condition/{id}")]
        public HttpResponseMessage GetLcCondition(int id)
        {
            try
            {
                LcConditionViewModel response = conditionRepo.GetLcCondition(id);
                if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
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
            try
            {
                var response = conditionRepo.AddLcCondition(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
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
            try
            {
                bool response = conditionRepo.UpdateLcCondition(model, id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
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
            try
            {
                bool response = conditionRepo.DeleteLcCondition(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }
        #endregion LCCONDITIONS

        #region RELEASEOFSHIPPINGDOCUMENTS
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/release")]
        public HttpResponseMessage GetLcIssuancesForRelease()
        {
            try
            {
                IEnumerable<LcIssuanceViewModel> response = repo.GetLcIssuancesForRelease();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/release-approval")]
        public HttpResponseMessage GetLcIssuancesForReleaseApproval()
        {
            try
            {
                IEnumerable<LcIssuanceApprovalViewModel> response = repo.GetLcIssuancesForReleaseApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }
        #endregion RELEASEOFSHIPPINGDOCUMENTS

        #region LCUSSANCE
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-ussance/{lcIssuanceId}")]
        public HttpResponseMessage GetLcUssanceByLCIssuanceId(int lcIssuanceId)
        {
            try
            {
                LcUssanceViewModel response = ussanceRepo.GetLcUssanceByLCIssuanceId(lcIssuanceId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lc-ussance")]
        public HttpResponseMessage AddLcUssance([FromBody] LcUssanceViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            try
            {
                var response = ussanceRepo.AddLcUssance(model);
                if (response.lcUssanceId > 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lc-ussance/{id}")]
        public HttpResponseMessage UpdateLcUssance([FromBody] LcUssanceViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            try
            {
                bool response = ussanceRepo.UpdateLcUssance(model, id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1, message = "The record has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/ussance")]
        public HttpResponseMessage GetLcIssuancesForUssance()
        {
            try
            {
                IEnumerable<LcIssuanceViewModel> response = ussanceRepo.GetLcIssuancesForUssance();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lc-issuance/ussance-approval")]
        public HttpResponseMessage GetLcIssuancesForUssanceApproval()
        {
            try
            {
                IEnumerable<LcIssuanceApprovalViewModel> response = ussanceRepo.GetLcIssuancesForUssanceApproval(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        #endregion LCUSSANCE
    }
}
