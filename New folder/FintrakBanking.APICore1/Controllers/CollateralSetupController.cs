using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups;
using FintrakBanking.ViewModels.Setups.Credit;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class CollateralSetupController : BaseController
    {
        private ICollateralTypeRepository repo;
        IErrorLogRepository errorLogger;
        public CollateralSetupController(ICollateralTypeRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        //#region collateral category
        //[HttpPost("collateral-category")]
        //public async Task<IActionResult> AddCollateralCategory([FromBody] CollateralCategoryViewModel entity)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.createdBy = token.GetStaffId;

        //        var response = await repo.AddCollateralCategory(entity);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}


        //[HttpPost("collateral-type")]
        //public async Task<IActionResult> AddCollateralTypes([FromBody]  CollateralTypeViewModel entity)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);
                   
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.createdBy = token.GetStaffId;

        //        var response = await repo.AddCollateralTypes(entity);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-type-by-categoryId/{categoryId}")]
        //public IActionResult GetCollateralCategoryByTypeId(int categoryId)
        //{
        //    try
        //    {
        //        var response = repo.GetCollateralTypeByCategoryId(categoryId);

        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpDelete("collateral-category/{categoryId}")]
        //public async Task<IActionResult> DeleteCollateralCategory(int categoryId, [FromBody]  CollateralCategoryViewModel entity)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = Request.Path.Value,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };
        //        var response = await repo.DeleteCollateralCategory(categoryId, entity, user);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Deleted successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpDelete("collateral-type/{typeId}")]
        //public async Task<IActionResult> DeleteCollateralTypes(int typeId, CollateralTypeViewModel entity)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = Request.Path.Value,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };
        //        var response = await repo.DeleteCollateralTypes(typeId, entity, user);

        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Deleted successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-category")]
        //public IActionResult GetCollateralCategory()
        //{
        //    try
        //    {
        //        var response = repo.GetCollateralCategory();

        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-category/{categoryId}")]
        //public IActionResult GetCollateralCategoryById(int categoryId)
        //{
        //    try
        //    {
        //        var response = repo.GetCollateralCategoryById(categoryId);

        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-category-by-productgroupid/{productGroupId}")]
        //public IActionResult GetCollateralCategoryByProductGroupId(int productGroupId)
        //{
        //    try
        //    {
        //        var response = repo.GetCollateralCategoryByProductGroupId(productGroupId);

        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}
        //#endregion collateral category

        #region collateral type
        [HttpGet("collateral-type")]
        public IActionResult GetCollateralTypes()
        {
            try
            {
                var response = repo.GetCollateralTypes();

                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("collateral-type/{typeId}")]
        public IActionResult GetCollateralTypesById(int typeId)
        {
            try
            {
                var response = repo.GetCollateralTypesById(typeId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        //[HttpPut("collateral-category/{categoryId}")]
        //public async Task<IActionResult> UpdateCollateralCategory(int categoryId, [FromBody]  CollateralCategoryViewModel entity)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.createdBy = token.GetStaffId;

        //        var response = await repo.UpdateCollateralCategory(categoryId, entity);
        //        if (!response)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPut("collateral-type/{typeId}")]
        public async Task<IActionResult> UpdateCollateralTypes(int typeId, [FromBody]  CollateralTypeViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = await repo.UpdateCollateralTypes(typeId, entity);
                if (!response)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion collateral type

        //#region collateral location
        //[HttpPost("collateral-location")]
        //public async Task<IActionResult> AddCollateralLocation([FromBody] CollateralLocationsViewModel entity)
        //{
        //    var token = new TokenDecryptionHelper(this.HttpContext);
        //    try
        //    {
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.createdBy = token.GetStaffId;

        //        var response = await repo.AddCollateralLocation(entity);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPut("collateral-location/{id}")]
        //public async Task<IActionResult> UpdateCollateralLocation(int id, [FromBody]  CollateralLocationsViewModel entity)
        //{

        //    var token = new TokenDecryptionHelper(this.HttpContext);
        //    try
        //    {
        //        entity.createdBy = token.GetStaffId;
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        //        var response = await repo.UpdateCollateralLocation(id, entity);
        //        if (!response)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpDelete("collateral-location/{id}")]
        //public async Task<IActionResult> DeleteCollateralLocation(int id)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = Request.Path.Value,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };

        //        var response = await repo.DeleteCollateralLocation(id, user);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        ////[HttpGet("collateral-location")]
        ////public IActionResult GetCollateralLocations()
        ////{
        ////    TokenDecryptionHelper token = null;
        ////    try
        ////    {
        ////        token = new TokenDecryptionHelper(this.HttpContext);

        ////        var response = repo.GetCollateralLocations(token.GetCompanyId);
        ////        if (response == null)
        ////        {
        ////            return NotFound(new { success = false, message = "No record found" });
        ////        }
        ////        return Ok(new { success = true, result = response });
        ////    }
        ////    catch (System.Exception ex)
        ////    {
        ////        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        ////        return Ok(new { success = false, message = ex.Message });
        ////    }
        ////}

        //[HttpGet("collateral-location/{id}")]
        //public IActionResult GetCollateralLocationsByCollateralLocationsId(int id)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralLocationsByCollateralLocationsId(id, token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-location/{collateraltypeId}")]
        //public IActionResult GetCollateralLocationsByCollateralTypeId(int collateraltypeId)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralLocationsByCollateralTypeId(collateraltypeId, token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-location/{countryId}")]
        //public IActionResult GetCollateralLocationsByCountryId(int countryId)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralLocationsByCountryId(countryId, token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}
        //#endregion collateral location


        #region Collateral Custom Fields

        //[HttpPost("collateral-custom-fields-by-id/{id}")]
        //public async Task<IActionResult> AddCollateralCustomFields([FromBody] CollateralCustomFieldsViewModel entity)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        //        var response = await repo.AddCollateralCustomFields(entity);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPut("collateral-custom-fields-by-id/{id}")]
        //public async Task<IActionResult> UpdateCollateralCustomFields(int collateralCustomFieldsId, [FromBody] CollateralCustomFieldsViewModel entity)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);
        //        entity.createdBy = token.GetStaffId;
        //        entity.applicationUrl = Request.Path.Value;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        //        var response = await repo.UpdateCollateralCustomFields(collateralCustomFieldsId, entity);
        //        if (!response)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpDelete("collateral-custom-fields-by-id/{id}")]
        //public async Task<IActionResult> DeleteCollateralCustomFields(int collateralCustomFieldsId)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = Request.Path.Value,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };

        //        var response = await repo.DeleteCollateralCustomFields(collateralCustomFieldsId, user);
        //        if (response)
        //        {
        //            return Created("", new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Ok(new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }

        //}

        //[HttpGet("collateral-custom-fields-by-id/{id}")]
        //public IActionResult CollateralCustomFieldsByCollateralCustomFieldsId(int id)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.CollateralCustomFieldsByCollateralCustomFieldsId(id, token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-custom-fields-by-collateral-typeId/{collateraltypeId}")]
        //public IActionResult CollateralCustomFieldsByCollateralTypeId(int collateralTypeId)
        //{
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.CollateralCustomFieldsByCollateralTypeId(collateralTypeId, token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet("collateral-location/{countryId}")]
        //public IActionResult GetCollateralCustomFields()
        //{ 
        //    TokenDecryptionHelper token = null;
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralCustomFields( token.GetCompanyId);
        //        if (response == null)
        //        {
        //            return NotFound(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}


        #endregion Collateral Custom Fields


    }
}