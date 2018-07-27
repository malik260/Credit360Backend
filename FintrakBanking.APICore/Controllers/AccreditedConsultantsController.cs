using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class AccreditedConsultantsController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        private IAccreditedConsultantsRepository repo;
        public AccreditedConsultantsController(IAccreditedConsultantsRepository repo)
        {
            this.repo = repo;
        }
        #region Solicitors
        [HttpGet, Route("accreditedConsultantType")]
        public HttpResponseMessage GetAccreditedConsultantType()
        {
            try
            {
                var response = repo.GetAccreditedConsultantType();
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response});
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("accredited-solicitors")]
        public HttpResponseMessage GetAccreditedSolicitors()
        {
            try
            {
                var response = repo.GetAccreditedConsultants(token.GetCompanyId);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("accredited-solicitors/{stateId}")]
        public HttpResponseMessage GetAccreditedStateConsultantsByStateId(int stateId)
        {
            try
            {
                var response = repo.GetAccreditedStateConsultantsByStateId(token.GetCompanyId,stateId);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost, Route("accredited-solicitors")]
        public async Task<HttpResponseMessage> AddAccreditedSolicitors([FromBody] AccreditedConsultantsViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.countryId = (short)token.GetCountryId;
                var response = await repo.AddAccreditedConsultants(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut, Route("accredited-solicitors/{id}")]
        public async Task<HttpResponseMessage> UpdateAccreditedSolicitors([FromBody] AccreditedConsultantsViewModel entity, int id)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                entity.countryId = (short)token.GetCountryId;
                var response = await repo.UpdateAccreditedConsultants(entity, id);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpDelete, Route("accredited-solicitors/{id}")]
        public async Task<HttpResponseMessage> DeleteAccreditedSolicitors(int id)
        {
            try
            {
                var response = await repo.DeleteAccreditedConsultantStates(id);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
        #endregion
        #region Principals
        //[HttpGet, Route("accredited-principals")]
        //public HttpResponseMessage GetAccreditedPrincipals()
        //{
        //    try
        //    {
        //        var response = repo.GetAccreditedPrincipals(token.GetCompanyId);
        //        if (response != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        //[HttpPost, Route("accredited-principals")]
        //public async Task<HttpResponseMessage> AddAccreditedPrincipals([FromBody] AccreditedPrincipalsViewModel entity)
        //{
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.AddAccreditedPrincipals(entity);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        //[HttpPut, Route("accredited-principlals/{id}")]
        //public async Task<HttpResponseMessage> UpdateAccreditedPrincipals([FromBody] AccreditedPrincipalsViewModel entity, int id)
        //{
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.UpdateAccreditedPrincipals(entity, id);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Updated successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        #endregion
        #region Auditors
        //[HttpGet, Route("accredited-auditors")]
        //public HttpResponseMessage GetAccreditedAuditors()
        //{
        //    try
        //    {
        //        var response = repo.GetAccreditedAuditors(token.GetCompanyId);
        //        if (response != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        //[HttpPost, Route("accredited-auditors")]
        //public async Task<HttpResponseMessage> AddAccreditedAuditors([FromBody] AccreditedAuditorsViewModel entity)
        //{
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.AddAccreditedAuditors(entity);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        //[HttpPut, Route("accredited-auditors/{id}")]
        //public async Task<HttpResponseMessage> UpdateAccreditedAuditors([FromBody] AccreditedAuditorsViewModel entity, int id)
        //{
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.UpdateAccreditedAuditors(entity, id);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Updated successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        #endregion
        #region Recovery Agents
        //[HttpGet, Route("accredited-recoveryagent")]
        //public HttpResponseMessage GetAccreditedRecoveryAgent()
        //{
        //    try
        //    {
        //        var response = repo.GetAccreditedAuditors(token.GetCompanyId);
        //        if (response != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        //[HttpPost, Route("accredited-recoveryagent")]
        //public async Task<HttpResponseMessage> AddAccreditedRecoveryAgents([FromBody] AccreditedRecoveryAgentViewModel entity)
        //{
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.AddAccreditedRecoveryAgents(entity);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        //[HttpPut, Route("accredited-recoveryagent/{id}")]
        //public async Task<HttpResponseMessage> UpdateAccreditedRecoveryAgent([FromBody] AccreditedRecoveryAgentViewModel entity, int id)
        //{
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.UpdateAccreditedRecoveryAgent(entity, id);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Updated successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
        //    }
        //}
        #endregion


    }
}
