using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class ProductFeeController : BaseController
    {
        private IProductFeeRepository repo;

        public ProductFeeController(IProductFeeRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("product-fee/all/{productId}")]
        public IActionResult GetFeeByProduct(int productId)
        {
            try
            {
                var data = repo.GetFeeByProduct(productId);
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

        [HttpGet("product-fee/unmapped/{productId}")]
        public IActionResult GetUnmappedFeeToProduct(int productId)
        {
            try
            {
                var data = repo.GetUnmappedFeeToProduct(productId);
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

        [HttpGet("product-fee/{productFeeId}")]
        public IActionResult GetProductFeeViewModel(int productFeeId)
        {
            try
            {
                var data = repo.GetProductFeeViewModel(productFeeId);
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

        // POST api/values
        [HttpPost("product-fee")]
        public IActionResult AddProductFee([FromBody] ProductFeeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                
                var recordId = repo.AddProductFee(model);
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

        [HttpPost("product-fee/multiple")]
        public IActionResult AddMultipleProductFee([FromBody] List<ProductFeeViewModel> model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                
                var recordId = repo.AddMultipleProductFee(model);
                if (recordId >= 1)
                {
                    return Ok(new { success = true, result = recordId, message = "product fee(s) has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product fee not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("product-fee/{productFeeId}")]
        public IActionResult UpdateProductFee(int productFeeId, [FromBody] ProductFeeViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var data = repo.GetProductFeeViewModel(productFeeId);
            if (data == null)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;

                repo.UpdateProductFee(productFeeId, model);

                return Ok(new { success = true, result = productFeeId, message = "product fee has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("product-fee/{productFeeId}")]
        public IActionResult DeleteProductFee(int productFeeId)
        {
            var account = repo.GetProductFeeViewModel(productFeeId);
            if (account == null)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };
                repo.DeleteProductFee(productFeeId, user);

                return Ok(new { success = true, result = productFeeId, message = "product fee has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("product-fee/multiple/{productFeeIds}")]
        public IActionResult DeleteMultipleProductFee(List<int> productFeeIds)
        {
            if (productFeeIds.Count <= 0)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                repo.DeleteMultipleProductFee(productFeeIds);

                return Ok(new { success = true, result = 1, message = "product fee(s) has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}