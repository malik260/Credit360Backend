using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class CountryController : ApiControllerBase
    {
        private ICountryRepository repo;
        public CountryController(ICountryRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpPost]
        [Route("city")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddCityAsync([FromBody] CityViewModel entity)
        {
            try
            {
                var data = await repo.AddCity(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }



        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("state/country")]
        public HttpResponseMessage GetStateByCountryId()
        {
            var token = new TokenDecryptionHelper();
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("city/country")]
        public HttpResponseMessage GetAllCityByCountryId()
        {
            var token = new TokenDecryptionHelper();
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
    }
}