using FintrakBanking.Interfaces.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/accountsensitivity")]
    public class AccountSensitivity : BaseController
    {
        private IAccountSensitivityRepository repo;

        public AccountSensitivity(IAccountSensitivityRepository _repo)
        {
            repo = _repo;
        }

        [HttpGet("getaccountsensitivity")]
        public ActionResult GetSensitivityLevels()
        {
            try
            {
                var sensitivityLevel = repo.GetAllAccountSensitivityLevels();
                return Ok(sensitivityLevel);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpGet("getaccountsensitivity/{levelId}")]
        public ActionResult GetSensitivityLevels(int levelId)
        {
            try
            {
                var sensitivityLevel = repo.GetAccountSensitivityLevelsByLevelId(levelId);
                return Ok(sensitivityLevel);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }
    }
}