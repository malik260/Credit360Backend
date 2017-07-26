using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups/company")]
    public class CompanyController : BaseController
    {
        private ICompanyRepository repo;

        public CompanyController(ICompanyRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("")]
        public IActionResult GetAllCompany()
        {
            try
            {
                var companys = repo.GetAllCompany().ToList();
                if (companys == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new
                {
                    success = true,
                    result = companys
                });  //companys.ToList()
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{companyId}")]
        public IActionResult Get(int companyId)
        {
            try
            {
                //var companys = repo.GetcompanyViewModel(companyId);
                //return Ok(companys);

                var company = repo.GetCompanyViewModel(companyId);
                if (company == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = company });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // POST api/values
        [HttpPost]
        public IActionResult AddCompany([FromBody] CompanyViewModel model)
        {
            try
            {
                var response = repo.AddCompany(model);
                if (response)
                {
                    return Ok(new { success = true, message = "company has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "company not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{companyId}")]
        public IActionResult UpdateCompany(int companyId, [FromBody] CompanyViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var company = repo.GetCompanyViewModel(companyId);
            if (company == null)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                repo.UpdateCompany(model);

                return Ok(new { success = true, result = model.companyId, message = "company has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}