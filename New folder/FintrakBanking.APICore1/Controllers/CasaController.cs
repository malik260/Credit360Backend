using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using FintrakBanking.ViewModels.CASA;
using System;
using System.Collections.Generic;
using FintrakBanking.APICore.JWTAuth;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/casa")]
    public class CasaController : BaseController
    {
        private ICasaRepository repo;

        public CasaController(ICasaRepository _repo)
        {
            this.repo = _repo;
        }        


        [HttpGet("{accountId}")]
        public IActionResult GetAccount(int accountId)
        {
            try
            {
                var data = repo.GetAccount(accountId);
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("customer/{customerId}")]
        public IActionResult GetAccountByCustomerId(int customerId)
        {
            try
            {
                var data = repo.GetAccountByCustomerId(customerId);
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("account-number-name/{accountNumberOrName}")]
        public IActionResult FindAccount(string accountNumberOrName)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);                

                var data = repo.FindAccount(accountNumberOrName, token.GetCompanyId);
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("customer/search")]
        public IActionResult SearchCustomer(string q,string t)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var data = repo.SearchCustomer(int.Parse(t), token.GetCompanyId,q);
                if (data == null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}