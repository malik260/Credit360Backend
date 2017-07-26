using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using Microsoft.AspNetCore.Cors;
using System;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class CustomFieldController : BaseController
    {
        TokenDecryptionHelper token = null;
        private ICustomFieldsRepository repo;
        IErrorLogRepository errorLogger;
        public CustomFieldController(ICustomFieldsRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region   Custom Fields

        // ossy

        [HttpPost("custom-field")]
        public async Task<IActionResult> AddCustomField([FromBody] AddCustomFieldViewModel model)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                var response = await repo.AddCustomField(model);
                if (response)
                {
                    return Created("", new { success = true, result = model, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpPut("custom-field")]
        public async Task<IActionResult> UpdateCustomField([FromBody] AddCustomFieldViewModel model, int id)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.lastUpdatedBy = token.GetStaffId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var response = await repo.UpdateCustomField(model, id);

            if (response)
            {
                return Created("", new { success = true, result = model, message = "The record has been Update successfully" });
            }

            return Ok(new { success = false, message = "There was an error Update this record" });
        }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

            //ossy


        [HttpPost("custom-field-multiple")]
        public async Task<IActionResult> AddCustomFields([FromBody] List<CustomFieldViewModel> listEntity)
        {

            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                foreach (var entity in listEntity)
                {
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.createdBy = token.GetStaffId;
                    entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = Request.Path.Value;
                }
                var response = await repo.AddCustomFields(listEntity);
                if (response)
                {
                    return Created("", new { success = true, result = listEntity, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete("custom-field")]
        public async Task<IActionResult> DeleteCustomFields([FromBody] List<CustomFieldViewModel> customFields)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var response = await repo.DeleteCustomFields(customFields, user);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "Deleted successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("custom-field-multiple")]
        public async Task<IActionResult> UpdateCustomFields([FromBody] List<CustomFieldViewModel> listEntity)
        {

            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                foreach (var entity in listEntity)
                {

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.lastUpdatedBy = token.GetStaffId;
                    entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = Request.Path.Value;
                }
                var response = await repo.UpdateCustomFields(listEntity);

                if (response)
                {
                    return Created("", new { success = true, result = listEntity, message = "The record has been Update successfully" });
                }

                return Ok(new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

        [HttpGet("custom-field/hostPage/{id}")]
        public IActionResult GetCustomFieldsByHostPageId(int id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.CustomFieldsByHostPageId(id, token.GetCompanyId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion   Custom Fields
        #region   Custom Fields Data
        [HttpPost("custom-field-data")]
        public async   Task<IActionResult> AddCustomFieldsData([FromBody] List<CustomFieldsDataViewModel> listEntity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                foreach (var entity in listEntity)
                {
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.createdBy = token.GetStaffId;
                    entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = Request.Path.Value;
                }
                var response = await repo.AddCustomFieldsData(listEntity);
                if (response)
                {
                    return Created("", new { success = true, result = listEntity, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete("custom-field-data")]
        public async Task<IActionResult> DeleteCustomFieldsData([FromBody] List<CustomFieldsDataViewModel> listEntity)
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var response = await repo.DeleteCustomFieldsData(listEntity, user);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "Deleted successfully" });
                }

                return Ok(new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("custom-field-data/hostpage/{id}/{customerId}")]
        public IActionResult GetCustomFieldsDataByCustomField(int id,  int customerId)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCustomFieldsDataByHostPage(id, customerId, token.GetCompanyId);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("custom-field-data")]
        public async Task<IActionResult> UpdateCustomFieldsData([FromBody] List<CustomFieldsDataViewModel> listEntity)
        {
            try
            {
                token = new TokenDecryptionHelper(HttpContext);
                foreach (var entity in listEntity)
                {
                    entity.userBranchId = (short)token.GetBranchId;
                    entity.companyId = token.GetCompanyId;
                    entity.lastUpdatedBy = token.GetStaffId;
                    entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = Request.Path.Value;
                }
                var response = await repo.UpdateCustomFieldsData(listEntity);
                if (response)
                {
                    return Created("", new { success = true, result = listEntity, message = "The record has been Update successfully" });
                }

                return Ok(new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }
        #endregion   Custom Fields Data

        #region   host page 

        [HttpGet("hostPage/hostpage/{id}")]
        public IActionResult GetHostPagesChildrenOnly(int id)
        {

            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetHostPagesChildrenOnly(id);
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("hostPage")]
        public IActionResult GetHostPagesParentOnly()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetHostPagesParentOnly();
                if (response == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion   host page
    }
}
