using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.Interfaces.Customer;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/customers")]
    public class CustomerGroupController : BaseController
    {
        private ICustomerGroupRepository repo;

        public CustomerGroupController(ICustomerGroupRepository _repo)
        {
            this.repo = _repo;
        }
        #region Customer Group
        [HttpPost("customer-group")]
        public IActionResult AddCustomerGroup([FromBody] CustomerGroupViewModel entity)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var response = repo.AddCustomerGroup(entity);
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

        [HttpDelete("customer-group/{groupId}")]
        public IActionResult DeleteCustomerGroup(short groupId)

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

                var response = repo.DeleteCustomerGroup(groupId, user);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been deleted successfully" });
                }

                return Ok(new { success = false, message = "There was an error deleting this record" });
            }
            catch (Exception e)
            {

                return Ok(new { success = false, message = $"There was an error deleting this record {e.InnerException}" });
            }
        }

        [HttpGet("customer-group")]
        public IActionResult GetCustomerGroup()
        {

            try
            {
                var response = repo.GetCustomerGroup();
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

        [HttpGet("customer-group/{customerGroupId}")]
        public IActionResult GetCustomerGroupByCustomerId(int customerGroupId)
        {
            try
            {
                var response = repo.GetCustomerGroupByCustomerId(customerGroupId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        [HttpPut("customer-group/{customerGroupId}")]
        public IActionResult UpdateCustomerGroup(int customerGroupId, CustomerGroupViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var response = repo.UpdateCustomerGroup(customerGroupId, entity);

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
        #endregion

        #region Customer Group Mapping
        [HttpPost("customer-group-mapping")]
        public IActionResult AddCustomerGroupMapping([FromBody] CustomerGroupMapppingViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var response = repo.AddCustomerGroupMapping(entity);
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

        [HttpPost("customer-group-mapping/multiple")]
        public IActionResult AddMultipleCustomerGroupMapping([FromBody] List<CustomerGroupMapppingViewModel> customerGroups)
        {
            try
            {
                var response = repo.AddMultipleCustomerGroupMapping(customerGroups);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been created successfully" });

                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.InnerException}" });
            }
        }

        [HttpDelete("customer-group-mapping/{groupMapId}")]
        public IActionResult DeleteCustomerGroupMapping(int groupMapId)

        {
            var token = new TokenDecryptionHelper(this.HttpContext);
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var response = repo.DeleteCustomerGroupMapping(groupMapId, user);                
                
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been deleted successfully" });
                }

                return Ok(new { success = false, message = "There was an error deleting this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error deleting this record {e.InnerException}" });
            }
        }

        [HttpGet("customer-group-mapping")]
        public IActionResult GetCustomerGroupMapping()
        {

            try
            {
                var response = repo.GetCustomerGroupMapping();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        [HttpGet("customer-group-mapping/{groupMapId}")]
        public IActionResult GetCustomerGroupMappingByGroupMapId(int groupMapId)
        {
            try
            {
                var response = repo.GetCustomerGroupMappingByGroupMapId(groupMapId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        [HttpGet("customer-group-mapping/customers/{customerGroupId}")]
        public IActionResult GetCustomerGroupMappingByGroupId(int customerGroupId)
        {
            try
            {
                var response = repo.GetCustomerGroupMappingByGroupId(customerGroupId);
                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpGet("customer-group-mapping/relationship-types")]
        public IActionResult GetCustomerGroupRelationshipTypes()
        {
            try
            {
                var response = repo.GetCustomerGroupRelationshipTypes();
                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("customer-group-mapping/{groupMapId}")]
        public IActionResult UpdateCustomerGroupMaping(int groupMapId, [FromBody] CustomerGroupMapppingViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var response = repo.UpdateCustomerGroupMapping(groupMapId, entity);

                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been Updated successfully" });
                }

                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }
        #endregion
    }
}