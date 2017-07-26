using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/customers")]
    public class CustomerController : BaseController
    {
        private ICustomerRepository repo;

        public CustomerController(ICustomerRepository _repo)
        {
            this.repo = _repo;
        }


        [HttpPost("customer")]
        public async Task<IActionResult> AddCustomer([FromBody]CustomerViewModels entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = await repo.AddCustomer(entity);
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

        [HttpDelete("customer/{customerId}")]
        public async Task<IActionResult> DeleteCustomer(int customerId)
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

                var response = await repo.DeleteCustomer(customerId, user);
                if (response)
                {
                    return Created("", new { success = true, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet("customer/{customerId}")]
        public IActionResult GetCustomer(int custormerId)
        {
            try
            {
                var tokenHelper = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCustomer(custormerId, tokenHelper.GetCompanyId );
                if (response!= null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-by-branch/{branchId}")]
        public IActionResult GetCustomerByBranchId(int branchId)
        {
            try
            {
                var token  = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.GetCustomerByBranchId(branchId, token.GetCompanyId);
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

        [HttpGet("customer-by-branch")]
        public IActionResult GetCustomerByBranchId()
        {
            try
            {
                var tokenHelper = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetCustomerByBranchId(tokenHelper.GetBranchId, tokenHelper.GetCompanyId );
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

        [HttpGet("customer")]
        public IActionResult SearchCustomer(string search)
        {
            try
            {
                var tokenHelper = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.CustomerSearch(tokenHelper.GetCompanyId, search);
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

        [HttpPost("customer-search")]
        public IActionResult SearchCustomer([FromBody] CustomerSearchItemViewModels search)
        {
            try
            {
                var tokenHelper = new TokenDecryptionHelper(this.HttpContext);
                var response = repo.CustomerSearch(tokenHelper.GetCompanyId, search);
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

        [HttpGet("customer-by-company/{companyId}")]
        public IActionResult GetCustomerByCompanyId(int companyId)
        {
            try
            {
                var response = repo.GetCustomerByCompanyId(companyId);
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

        [HttpGet("customer-by-customer-type/{customertypeId}")]
        public IActionResult GetCustomerByType(int customertypeId)
        {
            try
            {
                var response = repo.GetCustomerByTypeId(customertypeId);
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

        [HttpGet("customertype")]
        public IActionResult GetCustomerType()
        {
            try
            {
                var response = repo.GetCustomerType();
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

        [HttpPut("customer/{customerId}")]
        public async Task<IActionResult> UpdateCustomer(int customerId, CustomerViewModels entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = await repo.UpdateCustomer(customerId,entity);
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
    }
}
