using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class ProductController : BaseController
    {
        private IProductRepository repo;

        public ProductController(IProductRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("product-category")]
        public IActionResult GetAllProductCategory()
        {
            try
            {
                var data = repo.GetAllProductCategory();
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

        [HttpGet("product-class")]
        public IActionResult GetAllProductClass()
        {
            try
            {
                var data = repo.GetAllProductClass();
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

        #region Product Group

        [HttpGet("product-group")]
        public IActionResult GetAllProductGroup()
        {
            try
            {
                var data = repo.GetAllProductGroup();
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

        [HttpGet("product-group/{productGroupId}")]
        public IActionResult GetProductGroupById(short productGroupId)
        {
            try
            {
                var data = repo.GetProductGroupById(productGroupId);
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

        [HttpPut("product-group/{productGroupId}")]
        public IActionResult UpdateProductGroup(short productGroupId, [FromBody] ProductGroupViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }


            var data = repo.GetProductGroupById(productGroupId);
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
                repo.UpdateProductGroup(productGroupId, model);

                return Ok(new { success = true, result = model.productGroupId, message = "product group has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion Product Group

        #region Product Type

        [HttpGet("product-type")]
        public IActionResult GetAllProductType()
        {
            try
            {
                var data = repo.GetAllProductType();
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

        [HttpGet("product-type/{productTypeId}")]
        public IActionResult GetProductTypeById(short productTypeId)
        {
            try
            {
                var data = repo.GetProductTypeById(productTypeId);
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

        [HttpGet("product-type-by-group/{productGroupId}")]
        public IActionResult GetProductTypeByProductGroup(short productGroupId)
        {
            try
            {
                var data = repo.GetProductTypeByProductGroup(productGroupId);
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
        [HttpPost("product-type")]
        public IActionResult AddProductType([FromBody] ProductTypeViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;

                var recordId = repo.AddProductType(model);
                if (recordId >= 1)
                {
                    return Ok(new { success = true, result = recordId, message = "product type has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product type not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("product-type/{productTypeId}")]
        public IActionResult UpdateProductType(short productTypeId, [FromBody] ProductTypeViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var account = repo.GetProductTypeById(productTypeId);
            if (account == null)
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

                repo.UpdateProductType(productTypeId, model);

                return Ok(new { success = true, result = productTypeId, message = "product type has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion Product Type

        #region Product Region

        [HttpGet("product")]
        public IActionResult GetAllProduct()
        {
            try
            {
                var data = repo.GetAllProduct();
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

        [HttpGet("product/group/{productGroupId}/category/{productCategoryId}")]
        public IActionResult GetProductByGroupAndCategory(short productGroupId, short productCategoryId)
        {
            try
            {
                var data = repo.GetProductByGroupAndCategory(productGroupId, productCategoryId);
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

        [HttpGet("product/type/{productTypeId}/category/{productCategoryId}")]
        public IActionResult GetProductByTypeAndCategory(short productTypeId, short productCategoryId)
        {
            try
            {
                var data = repo.GetProductByTypeAndCategory(productTypeId, productCategoryId);
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

        [HttpGet("product/{productId}")]
        public IActionResult GetProductById(int productId)
        {
            try
            {
                var data = repo.GetProductById(productId);
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

        [HttpPost("product")]
        public IActionResult AddProduct([FromBody] ProductViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.companyId = token.GetCompanyId;

                var record = repo.AddProduct(model);
                if (record != null)
                {
                    return Ok(new { success = true, result = record, message = "product has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("product/{productId}")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var account = repo.GetProductById(productId);
            if (account == null)
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

                repo.UpdateProduct(productId, model);

                return Ok(new { success = true, result = productId, message = "product has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion Product Region

        #region Product Price Index

        [HttpGet("product-price-index")]
        public IActionResult GetAllProductPriceIndex()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var data = repo.GetProductPriceIndex(token.GetCompanyId);
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

        [HttpGet("product-price-index/{productPriceIndexId}")]
        public IActionResult GetProductPriceIndexById(int productPriceIndexId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var data = repo.GetProductPriceIndexById(productPriceIndexId, token.GetBranchId);
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


        [HttpPost("product-price-index")]
        public IActionResult AddProductPriceIndex([FromBody] ProductPriceIndexViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.companyId = token.GetCompanyId;

                var record = repo.AddProductPriceIndex(model);
                if (record != null)
                {
                    return Ok(new { success = true, result = record, message = "product has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "product not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("product-price-index/{productPriceIndexId}")]
        public IActionResult UpdateProductPriceIndex(int productPriceIndexId, [FromBody] ProductPriceIndexViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;

                repo.UpdateProductPriceIndex(productPriceIndexId, model);

                return Ok(new { success = true, result = productPriceIndexId, message = "product Price Index has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }


        [HttpDelete("product-price-index/{productPriceIndexId}")]
        public IActionResult DeleteProductPriceIndex(int productPriceIndexId)
        {

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
                repo.DeleteProductPriceIndex(productPriceIndexId, user);

                return Ok(new { success = true, result = productPriceIndexId, message = "product Price Index has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion Product Price index
    }
}