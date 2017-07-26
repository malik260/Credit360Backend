using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setup")]
    public class JobTitleController : BaseController
    {
        private IJobTitleRepository repo;
        TokenDecryptionHelper token = null;
        public JobTitleController(IJobTitleRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("jobtitle/{jobtitleid}")]
        public IActionResult GetJobTitle(int jobTitleId)
        {
            try
            {
                var jobtitle = repo.GetJobTitle(jobTitleId);

                if (jobtitle == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = jobtitle });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("jobtitle/company")]
        public IActionResult GetJobTitleByCompanyId()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var jobtitle = repo.GetJobTitleByCompanyId(token.GetCompanyId);

                if (jobtitle == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = jobtitle });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("jobtitle")]
        public IActionResult JobTitle()
        {
            try
            {
                var jobtitle = repo.JobTitle();

                if (jobtitle == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = jobtitle });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}