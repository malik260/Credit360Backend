using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setup")]
    public class DepartmentController : BaseController
    {
        private IDepartmentRepository repo;

        public DepartmentController(IDepartmentRepository _repo)
        {
            repo = _repo;
        }

        [HttpPost("department")]
        public IActionResult AddDepartment(DepartmentViewModel entity)
        {
            if (entity == null)
            {
                return NotFound(new { success = false, message = "empty record" });
            }

            try
            {
                var depart = repo.AddDepartment(entity);
                return Ok(depart);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpPut("department")]
        public IActionResult DeleteDepartment(int departmentId)
        {
            var account = repo.GetDepartment(departmentId);
            if (account == null)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.DeleteDepartment(departmentId);

                return Ok(new { success = true, result = departmentId, message = "account has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("department")]
        public IActionResult GetAllDepartment()
        {
            string returnMessage = string.Empty;
            try
            {
                var department = repo.GetAllDepartment().ToList();
                if (department.Any())
                {
                    return Ok(new { success = true, result = department, count = department.Count });
                }

                return Ok(new { success = false, message = "No department found" });
            }
            catch (Exception e)
            {
                returnMessage = e.Message;
            }
            return Ok(new { success = false, message = $"There was error from the endpoint {returnMessage}" });
        }

        [HttpPost("department/{departmentId}")]
        public IActionResult GetDepartment(int departmentId)
        {
            var account = repo.GetDepartment(departmentId);
            if (account == null)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.GetDepartment(departmentId);
                return Ok(depart);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpPost("department/{departmentId}")]
        public IActionResult UpdateDepartment(int departmentId, DepartmentViewModel entity)
        {
            try
            {
                var response = repo.UpdateDepartment(departmentId, entity);
                if (response)
                {
                    return Created("", new { success = true, result = response });
                }

                return Ok(new { success = false, message = $"There was an error updating this group {response}" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }
    }
}