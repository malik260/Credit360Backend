using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Credit;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit/regulatory")]
    public class CrmsRegulatoryController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ICrmsRegulatoryRepository repo;

        public CrmsRegulatoryController(ICrmsRegulatoryRepository repo)
        {
            this.repo = repo;
        }

        #region CRMS CREDIT TYPE PRODUCT
        [HttpGet]
        [ClaimsAuthorization]
        [Route("regulatory-setup")]
        public HttpResponseMessage GetAllRegulatorySetup()
        {
            try
            {
                var data = repo.GetAllRegulatorySetup();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("regulatory-type/regtype/{crmsTypeId}")]
        public HttpResponseMessage GetRegulatoryByTypeId(int crmsTypeId)
        {
            try
            {
                var data = repo.GetRegulatoryByTypeId(crmsTypeId, token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("regulatory-type")]
        public HttpResponseMessage GetAllRegulatoryType()
        {
            try
            {
                var data = repo.GetAllRegulatoryType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("regulatory-setup")]
        public HttpResponseMessage AddRegulatory([FromBody] CrmsRegulatoryViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.AddRegulatory(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("regulatory-setup/{regulatoryId}")]
        public HttpResponseMessage UpdateRegulatory([FromBody] CrmsRegulatoryViewModel entity, int regulatoryId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.UpdateRegulatory(entity, regulatoryId);
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
        [HttpDelete]
        [ClaimsAuthorization]
        [Route("regulatory-setup/")]
        public HttpResponseMessage DeleteRegulatory(int regulatoryId)
        {
            try
            {
                var userBranchId = (short)token.GetBranchId;
                var companyId = token.GetCompanyId;
                var lastUpdatedBy = token.GetStaffId;
                var applicationUrl = HttpContext.Current.Request.Path;
                var userIPAddress = Request.RequestUri.Host;
                var data = repo.DeleteRegulatory(regulatoryId, userBranchId, companyId, lastUpdatedBy, applicationUrl, userIPAddress);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "The record has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error deleting this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion


    }
}
