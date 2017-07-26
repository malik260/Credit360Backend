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
    public class ProductCollateralTypeController : BaseController
    {
        private IProductCollateralTypeRepository repo;

        public ProductCollateralTypeController(IProductCollateralTypeRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("product-collateral-type/all/{productId}")]
        public IActionResult GetCollateralTypeByProduct(int productId)
        {
            try
            {
                var data = repo.GetCollateralTypeByProduct(productId);
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

        [HttpGet("product-collateral-type/unmapped/{productId}")]
        public IActionResult GetUnmappedCollateralToProduct(int productId)
        {
            try
            {
                var data = repo.GetUnmappedCollateralToProduct(productId);
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

        [HttpGet("product-collateral-type/{productCollateralTypeId}")]
        public IActionResult GetProductCollateralTypeViewModel(int productCollateralTypeId)
        {
            try
            {
                var data = repo.GetProductCollateralTypeViewModel(productCollateralTypeId);
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
        [HttpPost("product-collateral-type")]
        public IActionResult AddProductCollateralType([FromBody] ProductCollateralTypeViewModel model)
        {
            try
            {
                //Get staffId and coyId from token
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var recordId = repo.AddProductCollateralType(model);
                if (recordId >= 1)
                {
                    return Ok(new { success = true, result = recordId, message = "product collateral type has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product collateral type not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("product-collateral-type/multiple")]
        public IActionResult AddMultipleProductCollateralType([FromBody] List<ProductCollateralTypeViewModel> model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var recordId = repo.AddMultipleProductCollateralType(model);
                if (recordId >= 1)
                {
                    return Ok(new { success = true, result = recordId, message = "product collateral type(s) has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product collateral type not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("product-collateral-type/{productCollateralTypeId}")]
        public IActionResult DeleteProductCollateralType(int productCollateralTypeId)
        {
            //if (!repo.DoesProductCollateralExist(productCollateralTypeId))            
            //{
            //    return NotFound(new { success = false, message = "No record found" });
            //}
           
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
                repo.DeleteProductCollateralType(productCollateralTypeId, user);

                return Ok(new { success = true, result = productCollateralTypeId, message = "product collateral type has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("product-collateral-type/multiple/{productCollateralTypeIds}")]
        public IActionResult DeleteMultipleProductCollateralType(List<int> productCollateralTypeIds)
        {
            if (productCollateralTypeIds.Count <= 0)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                UserInfo user = new UserInfo();
                user.BranchId = token.GetBranchId;
                user.companyId = token.GetCompanyId;
                user.staffId = token.GetStaffId;
                user.applicationUrl = Request.Path.Value;
                user.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();


                repo.DeleteMultipleProductCollateralType(productCollateralTypeIds, user);

                return Ok(new { success = true, result = 1, message = "product collateral type(s) has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}