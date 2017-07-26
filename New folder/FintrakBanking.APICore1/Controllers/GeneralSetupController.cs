using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class GeneralSetupController : BaseController
    {
        private IGeneralSetupRepository repo;

        public GeneralSetupController(IGeneralSetupRepository _repo)
        {
            this.repo = _repo;
        }

        #region General Setups

        [HttpGet("calculate-maturity-date/effective-date/{effectiveDate}/tenor-mode/{tenorModeId}/tenor/{tenor}")]
        public IActionResult GetMaturityDate(DateTime effectiveDate, short tenorModeId, int tenor)
        {
            try
            {
                //var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.CalculateMaturityDate(effectiveDate, (TenorModeEnum) tenorModeId, tenor);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("tenor-mode")]
        public IActionResult GetAllTenorMode()
        {
            try
            {
                var data = repo .GetAllTenorMode();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("currency")]
        public IActionResult GetAllCurrency()
        {
            try
            {
                var data = repo.GetAllCurrency();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("customer-type")]
        public IActionResult GetAllCustomerType()
        {
            try
            {
                var data = repo.GetAllCustomerType();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }


        [HttpGet("deal-classification-type")]
        public IActionResult GetAllDealClassificationType()
        {
            try
            {
                var data = repo.GetAllDealClassificationType();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("application-date")]
        public IActionResult GetApplicaionDate()
        {
            try
            {
                var data = repo.GetApplicaionDate();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("day-count")]
        public IActionResult GetAllDayCount()
        {
            try
            {
                var data = repo.GetAllDayCount();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("fee-amortisation-type")]
        public IActionResult GetAllFeeAmortisationType()
        {
            try
            {
                var data = repo.GetAllFeeAmortisationType();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("deal-types")]
        public IActionResult GetAllDealTypes()
        {
            try
            {
                var data = repo.GetAllDealTypes();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("fs-types")]
        public IActionResult GetAllFSTypes()
        {
            try
            {
                var data = repo.GetAllFSTypes();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("frequency-types")]
        public IActionResult GetAllFrequencyTypes()
        {
            try
            {
                var data = repo.GetAllFrequencyTypes();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("operation-types")]
        public IActionResult GetAllOperationTypes()
        {
            try
            {
                var data = repo.GetAllOperationTypes();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("operation")]
        public IActionResult GetAllOperations()
        {
            try
            {
                var data = repo.GetAllOperations();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("operation/{operationTypeId}")]
        public IActionResult GetOperations(short operationTypeId)
        {
            try
            {
                var data = repo.GetOperations(operationTypeId);
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("casa/account-status")]
        public IActionResult GetCasaAccountStatus()
        {
            try
            {
                var data = repo.GetCasaAccountStatus();
                if (data == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = data.ToList() }); 
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        //[HttpGet("productgroups", Name = "GroupGet")]
        //public IActionResult GetProductGroup()
        //{
        //    try
        //    {
        //        var productGroups = repo.GetAllProductGroup();
        //        return Ok(productGroups);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return BadRequest(new { error = true, message = ex.Message });
        //    }
        //}

        //[HttpPost("productgroup/add")]
        //public IActionResult SaveProductGroup([FromBody]ProductGroupViewModel model)
        //{
        //    try
        //    {
        //        if (repo.SaveProductGroup(model))
        //        {
        //            return Created("", model);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return BadRequest();
        //    }

        //    return BadRequest();
        //}

        #endregion General Setups
    }
}