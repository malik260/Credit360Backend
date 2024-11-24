using FintrakBanking.APICore.Filters;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FintrakBanking.APICore.ExternalControllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [MiddlewareAuthorizeAttribute]
    [RoutePrefix("api/v1/third-party/general")]
    public class ExternalGeneralController : ApiController
    {

        private IGeneralRepositoryExternal repo;

        public ExternalGeneralController(IGeneralRepositoryExternal _repo)
        {
            this.repo = _repo;
        }
         


        [HttpGet]
        [Route("crms-legal-status")]
        public async Task<HttpResponseMessage> GetAllCRMSLegalStatus()
        {
            try
            {
                var data = await repo.GetAllCRMSLegalStatusAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("crms-relationship-type")]
        public async Task<HttpResponseMessage> GetAllCRMSRelationshipType()
        {
            try
            {
                var data = await repo.GetAllCRMSRelationshipTypeAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("crms-company-size")]
        public async Task<HttpResponseMessage> GetAllCRMSCompanySize()
        {
            try
            {
                var data = await repo.GetAllCRMSCompanySizeAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("countries")]
        public async Task<HttpResponseMessage> GetCountries()
        {
            try
            {
                var data = await repo.GetAllCountries();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("states/{countryId}")]
        public async Task<HttpResponseMessage> GetStates(int countryId)
        {
            try
            {
                var data = await repo.GetStatesByCountryId(countryId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("lgas/{stateId}")]
        public async Task<HttpResponseMessage> GetLgas(int stateId)
        {
            try
            {
                var data = await repo.GetLgasByStateId(stateId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("lgas/{lgaId}/cities")]
        public async Task<HttpResponseMessage> GetCities(int lgaId)
        {
            try
            {
                var data = await repo.GetCitiesByLgaId(lgaId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("address-types")]
        public async Task<HttpResponseMessage> GetAddressTypes()
        {
            try
            {
                var data = await repo.GetAddressTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("companies")]
        public async Task<HttpResponseMessage> GetCompanies()
        {
            try
            {
                var data = await repo.GetAllCompanies();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("companies/{companyId}/directors")]
        public async Task<HttpResponseMessage> GetCompanyDirectors(int companyId)
        {
            try
            {
                var data = await repo.GetCompanyDirectorsByCompanyId(companyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}
