using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class CountryController : BaseController
    {
        private ICountryRepository repo;
        TokenDecryptionHelper token = null;
        public CountryController(ICountryRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpPost("city")]
        public async Task<IActionResult> AddCity([FromBody] CityViewModel entity)
        {
            try
            {
                var response = await repo.AddCity(entity);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "Created successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("city-class")]
        public IActionResult GetAllCityClass()
        {
            try
            {
                var rank = repo.GetAllCityClass();

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("city")]
        public IActionResult GetCity()
        {
            try
            {
                var rank =  repo.GetCity();//repo.GetCities();

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }



        [HttpGet("city/state/{Id}")]
        public IActionResult GetCityByStateId(int Id)
        {
            try
            {
                var rank = repo.GetCityByStateId(Id).ToList();

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("country")]
        public IActionResult GetCountry()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var rank = repo.GetCountry();
                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("state")]
        public IActionResult GetState()
        {
            try
            {
                var rank = repo.GetState();

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("state/country")]
        public IActionResult GetStateByCountryId()
        {
            token = new TokenDecryptionHelper(this.HttpContext);
            try
            {
                var state = repo.GetStateByCountryId(token.GetCountryId).OrderBy(x=>x.StateName).ToList();

                if (state == null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = state });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("city/country")]
        public IActionResult GetAllCityByCountryId()
        {
            token = new TokenDecryptionHelper(this.HttpContext);
            try
            {
                var cities = repo.GetAllCitiesByContryId(token.GetCountryId).ToList();

                if (cities == null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = cities });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}