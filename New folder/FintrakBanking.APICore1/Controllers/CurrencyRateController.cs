using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.APICore.JWTAuth;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/admin")]
    public class CurrencyRateController : Controller
    {
        private ICurrencyRateRepository repo;

        public CurrencyRateController(ICurrencyRateRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("currency")]
        public IActionResult GetCurrency()
        {
            try
            {
                var data = repo.GetCurrency();
                return Ok(new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("currency-rate")]
        public IActionResult GetCurrencyRate()
        {
            try
            {
                var data = repo.GetCurrencyRate();
                return Ok(new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("currency-rate/{currencyId}")]
        public IActionResult GetCurrencyRateById(short currencyId)
        {
            try
            {
                var data = repo.GetCurrencyRateById(currencyId);
                return Ok(new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("currency-rate")]
        public IActionResult AddFSRatioCaption([FromBody] CurrencyRateViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;

                var response = repo.AddCurrencyRate(model);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut("currency-rate/{currencyId}")]
        public IActionResult UpdateFSRatioCaption(short currencyId, [FromBody] CurrencyRateViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;

                var response = repo.UpdateCurrencyRate(currencyId, model);

                if (response)
                {

                    return Created("", new { success = true, result = response, message = "The record has been updated successfully" });

                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
    }
}