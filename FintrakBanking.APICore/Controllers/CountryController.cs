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
        TokenDecryptionHelper token = null;
        public CountryController(ICountryRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpPost]
        [Route("city")]
        public HttpResponseMessage AddCity(HttpRequestMessage request, [FromBody] CityViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.AddCity(entity).IsCompleted;
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = true, result = response, message = "Created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = "An unknown error has occured" }));
                }
                catch (Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("city-class")]
        public HttpResponseMessage GetAllCityClass(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var rank = repo.GetAllCityClass();

                    if (rank == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = rank }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("city")]
        public HttpResponseMessage GetCity(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var rank = repo.GetCity();//repo.GetCities();

                    if (rank == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = rank }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }



        [HttpGet]
        [Route("city/state/{Id}")]
        public HttpResponseMessage GetCityByStateId(HttpRequestMessage request, int Id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var rank = repo.GetCityByStateId(Id).ToList();

                    if (rank == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = rank }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("country")]
        public HttpResponseMessage GetCountry(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var rank = repo.GetCountry();
                    if (rank == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = rank }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("state")]
        public HttpResponseMessage GetState(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var rank = repo.GetState();

                    if (rank == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = rank }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("state/country")]
        public HttpResponseMessage GetStateByCountryId(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                var token = new TokenDecryptionHelper();
                try
                {
                    var state = repo.GetStateByCountryId(token.GetCountryId).OrderBy(x => x.StateName).ToList();

                    if (state == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = state }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("city/country")]
        public HttpResponseMessage GetAllCityByCountryId(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                var token = new TokenDecryptionHelper();
                try
                {
                    var cities = repo.GetAllCitiesByContryId(token.GetCountryId).ToList();

                    if (cities == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = cities }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
    }
}