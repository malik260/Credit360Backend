using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class CountryController : ApiControllerBase
    {
        private ICountryRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public CountryController(ICountryRepository _repo)
        {
            this.repo = _repo;
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("city")]
        public HttpResponseMessage AddCity([FromBody] CityViewModel entity)
        {
            try
            {
                var data = repo.AddCity(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("post-local-govt")]
        public HttpResponseMessage AddLocalGovt([FromBody] LocalGovtViewModel entity)
        {
            try
            {
                var data = repo.AddLocalGovt(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
       [HttpPut] [ClaimsAuthorization]
        [Route("update-local-govt/{localGovernmentId}")]
        public HttpResponseMessage UpdateLocalGovt([FromBody] LocalGovtViewModel entity, int localGovernmentId)
        {
            try
            {
                var data = repo.UpdateLocalGovt(entity, localGovernmentId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("local-govt")]
        public HttpResponseMessage GetLocalGovt()
        {
            try
            {
                var data = repo.GetLocalGovt();
                if (data!=null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("local-govt/{stateId}")]
        public HttpResponseMessage GetLocalGovtByStateId(int stateId)
        {
            try
            {
                var data = repo.GetLocalGovtByStateId(stateId);
                if (data!=null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
       [HttpPut] [ClaimsAuthorization]
        [Route("city/{id}")]
        public HttpResponseMessage UpdateCity([FromBody] CityViewModel entity, int id)
        {
            try
            {
                var data = repo.UpdateCity(entity ,id);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("city-class")]
        public HttpResponseMessage GetAllCityClass()
        {
            try
            {
                var rank = repo.GetAllCityClass();

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("city-class/{lgaId}")]
        public HttpResponseMessage GetAllCityClass(int lgaId)
        {
            try
            {
                var rank = repo.GetCity(lgaId);

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("city")]
        public HttpResponseMessage GetCity()
        {
            try
            {
                var rank = repo.GetCity();//repo.GetCities();

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }



      [HttpGet] [ClaimsAuthorization]  
        [Route("city/state/{Id}")]
        public HttpResponseMessage GetCityByStateId(int Id)
        {
            try
            {
                var rank = repo.GetCityByStateId(Id).ToList();

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("city/lga/{Id}")]
        public HttpResponseMessage GetCityByLGAId(int Id)
        {
            try
            {
                var rank = repo.GetCityByLGAId(Id).ToList();

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lga/state/{Id}")]
        public HttpResponseMessage GetLGAByStateId(int Id)
        {
            try
            {
                var rank = repo.GetLGAByStateId(Id).ToList();

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("city/lgacity/{Id}")]
        public HttpResponseMessage GetLgaCity(int Id)
        {
            try
            {
                var rank = repo.GetLgaCity(Id);
                
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("country")]
        public HttpResponseMessage GetCountry()
        {
            try
            {
                var rank = repo.GetCountry();
                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("state")]
        public HttpResponseMessage GetState()
        {
            try
            {
                var rank = repo.GetState();

                if (rank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = rank });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
       [HttpPut] [ClaimsAuthorization]
        [Route("state/{id}")]
        public HttpResponseMessage UpdateStates([FromBody] StateViewModel entity, int id)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.UpdateState(entity, id);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "State updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "State not updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("state-by-company")]
        public HttpResponseMessage GetStateByCompanyId()
        {
            
            try
            {
                var state = repo.GetStateByCompanyId(token.GetCompanyId).OrderBy(x => x.StateName).ToList();

                if (state == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = state });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("state/country")]
        public HttpResponseMessage GetStateByCountryId()
        {
            try
            {
                var state = repo.GetStateByCountryId(token.GetCountryId).OrderBy(x => x.StateName).ToList();

                if (state == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = state });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("city/country")]
        public HttpResponseMessage GetAllCityByCountryId()
        {
            try
            {
                var cities = repo.GetAllCitiesByContryId(token.GetCountryId).ToList();

                if (cities == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = cities });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
    }
}