using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setup")]
    public class RankController : BaseController
    {
        private IRankRepository repo;
        TokenDecryptionHelper token = null;
        public RankController(IRankRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet("rank")]
        public IActionResult GetRank()
        {
            try
            {
                var rank = repo.GetRank();

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("rank/{rankId}")]
        public IActionResult GetRank(int rankId)
        {
            try
            {
                var rank = repo.GetRank(rankId);

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("rank/company")]
        public IActionResult GetRankByCompanyId()
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var rank = repo.GetRankByCompanyId(token.GetCompanyId);

                if (rank == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = rank });
            }
            catch (System.Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}