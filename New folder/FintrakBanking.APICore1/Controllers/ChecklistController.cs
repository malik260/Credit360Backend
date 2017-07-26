using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Setups;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class ChecklistController : BaseController
    {
        private IChecklistRepository repo;

        public ChecklistController(IChecklistRepository _repo)
        {
            this.repo = _repo;
        }

        #region Checklist Definition
        [HttpPost("checklist-definition")]
        public IActionResult AddChecklistDefinition([FromBody] ChecklistDefinitionViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                
                model.userBranchId = (short) token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddChecklistDefinition(model);
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


        [HttpPost("checklist-definition/multiple")]
        public IActionResult AddMultipleChecklistDefinition([FromBody] List<ChecklistDefinitionViewModel> model)

        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var recordId = repo.AddMultipleChecklistDefinition(model);
                if (recordId)
                {
                    return Ok(new { success = true, result = recordId, message = "Checklist Definitions has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "Checklist Definition not created" });
            }
          
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating these records {e.Message}" });
            }
        }

        [HttpPost("checklist-definition/multiple-items")]
        public IActionResult AddMultipleChecklistDefinitionWithMultipleItems([FromBody] ChecklistDefinitionViewModel model)

        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var recordId = repo.AddMultipleChecklistDefinitionWithMultipleItems(model);
                if (recordId)
                {
                    return Ok(new { success = true, result = recordId, message = "Checklist Definitions have been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "Checklist Definitions not created" });
            }

            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating these records {e.Message}" });
            }
        }

        [HttpGet("checklist-definition")]
        public IActionResult GetAllChecklistDefinition()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllChecklistDefinition();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("checklist-definition/{CheckListDefinitionId}")]
        public IActionResult GetAllChecklistDefinitionById(short CheckListDefinitionId)
        {
            try
            {
                var response = repo.GetAllChecklistDefinitionById(CheckListDefinitionId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("checklist-definition/{CheckListDefinitionId}")]
        public IActionResult UpdateChecklistDefinition(short CheckListDefinitionId, [FromBody] ChecklistDefinitionViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateChecklistDefinition(CheckListDefinitionId, model);

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

        [HttpDelete("checklist-definition/{CheckListDefinitionId}")]
        public IActionResult DeleteChecklistDefinition(short CheckListDefinitionId)
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

                repo.DeleteChecklistDefinition(CheckListDefinitionId, user);

                return Ok(new { success = true, result = CheckListDefinitionId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Checklist Detail
        [HttpPost("checklist-detail")]
        public IActionResult AddChecklistDetail([FromBody] ChecklistDetailViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddChecklistDetail(model);
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

        [HttpGet("checklist-detail")]
        public IActionResult GetAllChecklistDetail()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllChecklistDetail();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("checklist-detail/{ChecklistId}")]
        public IActionResult GetAllChecklistById(int ChecklistId)
        {
            try
            {
                var response = repo.GetAllChecklistDetailById(ChecklistId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("checklist-detail/{ChecklistId}")]
        public IActionResult UpdateChecklistDetail(int ChecklistId, [FromBody] ChecklistDetailViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateChecklistDetail(ChecklistId, model);

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

        [HttpDelete("checklist-detail/{ChecklistId}")]
        public IActionResult DeleteLoanChecklist(int ChecklistId)
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

                repo.DeleteChecklistDetail(ChecklistId, user);

                return Ok(new { success = true, result = ChecklistId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region CheckList Items
        [HttpPost("checklist-item")]
        public IActionResult AddChecklistItem([FromBody] ChecklistItemViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddChecklistItem(model);
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

        [HttpPost("checklist-item/multiple")]
        public IActionResult AddMultipleChecklistItem([FromBody] List<ChecklistItemViewModel> model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var recordId = repo.AddMultipleChecklistItem(model);
                if (recordId)
                {
                    return Ok(new { success = true, result = recordId, message = "Checklist items has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "Checklist items not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("checklist-item")]
        public IActionResult GetAllChecklistItem()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllChecklistItem();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("checklist-item/{ChecklistId}")]
        public IActionResult GetAllChecklistItemById(int CheckListItemId)
        {
            try
            {
                var response = repo.GetAllChecklistItemById(CheckListItemId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("checklist-item/{CheckListItemId}")]
        public IActionResult UpdateChecklistItem(int CheckListItemId, [FromBody] ChecklistItemViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateChecklistItem(CheckListItemId, model);

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

        [HttpDelete("checklist-item/{CheckListItemId}")]
        public IActionResult DeleteChecklistItem(int CheckListItemId)
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

                repo.DeleteChecklistItem(CheckListItemId, user);

                return Ok(new { success = true, result = CheckListItemId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region CheckList Select List
        [HttpGet("checklist-status")]
        public IActionResult GetAllChecklistStatus()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllChecklistStatus();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("checklist-target-type")]
        public IActionResult GetAllChecklistTargetType()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetAllChecklistTargetType();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion
    }
}