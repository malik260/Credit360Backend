using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class FeeController : BaseController
    {
        private IFeeRepository repo;

        public FeeController(IFeeRepository _repo)
        {
            this.repo = _repo;
        }

        #region Product Fee

        [HttpGet("fee")]
        public IActionResult GetAllFee()
        {
            try
            {
                var data = repo.GetAllFee();
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

        [HttpGet("fee/{feeId}")]
        public IActionResult GetFeeById(int feeId)
        {
            try
            {
                var data = repo.GetFeeViewModel(feeId);
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

        [HttpPost("fee")]
        public IActionResult AddFee([FromBody] FeeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;

                var recordId = repo.AddFee(model);
                if (recordId >= 1)
                {
                    return Ok(new { success = true, result = recordId, message = "product fee has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product fee not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("fee/{feeId}")]
        public IActionResult UpdateFee(int feeId, [FromBody] FeeViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var data = repo.GetFeeViewModel(feeId);
            if (data == null)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;

                repo.UpdateFee(feeId, model);

                return Ok(new { success = true, result = model.feeId, message = "product fee has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion Product Fee

        [HttpGet("fee/account-category")]
        public IActionResult GetFeeAccountCategory()
        {
            try
            {
                var data = repo.GetFeeAccountCategory();
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

        [HttpGet("fee/fee-type")]
        public IActionResult GetFeeType()
        {
            try
            {
                var data = repo.GetFeeType();
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

        [HttpGet("fee/fee-interval")]
        public IActionResult GetFeeInterval()
        {
            try
            {
                var data = repo.GetFeeInterval();
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

        [HttpGet("fee/fee-target")]
        public IActionResult GetFeeTarget()
        {
            try
            {
                var data = repo.GetFeeTarget();
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
    }
}