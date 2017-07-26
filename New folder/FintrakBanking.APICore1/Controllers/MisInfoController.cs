using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setup")]
    public class MisInfoController : BaseController
    {
        private IMisInfoRepository repo;
        TokenDecryptionHelper token = null;
        public MisInfoController(IMisInfoRepository _repo)
        {
            repo = _repo;
        }

        #region MisInfo

        [HttpPost("misInfo")]
        public async Task<IActionResult> AddMisInfo(MisInfoViewModel entity)
        {
            try
            {
                var misInfo = await repo.AddMisInfo(entity);
                if (misInfo)
                {
                    return Ok(new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("misinfo/{misInfoId}")]
        public async Task<IActionResult> DeleteMisInfo(int misInfoId)
        {
            try
            {
                var misInfo = await repo.DeleteMisInfo(misInfoId);
                if (misInfo)
                {
                    return Ok(new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("misinfo")]
        public IActionResult GetAllMisInfo()
        {
            try
            {
                var misInfo = repo.GetAllMisInfo();
                if (misInfo != null)
                {
                    return Ok(new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }


        [HttpGet("misinfo/company")]
        public IActionResult GetMisInfoByCoyId()
        {
            
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var misInfo = repo.GetMisInfoByCompanyId(token.GetCompanyId);
                if (misInfo != null)
                {
                    return Ok(new { success = true, result = misInfo });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("misinfo/{misInfoId}")]
        public IActionResult GetMisInfoById(int misInfoId)
        {
            try
            {
                var misInfo = repo.GetMisInfoById(misInfoId);
                if (misInfo != null)
                {
                    return Ok(new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("misinfo/{misinfoid}")]
        public async Task<IActionResult> UpdateMisInfo(int misinfoid, [FromBody] MisInfoViewModel entity)
        {
            try
            {
                var misInfo = await repo.UpdateMisInfo(misinfoid, entity);
                if (misInfo)
                {
                    return Ok(new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion MisInfo

        #region MisType

        [HttpPost("mistype")]
        public async Task<IActionResult> AddMisTypeAsync(MisTypeViewModel entity)
        {
            try
            {
                var misInfo = await repo.AddMisType(entity);
                if (misInfo)
                {
                    return Ok(new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("mistype/{mistypeid}")]
        public async Task<IActionResult> DeleteMisTypeAsync(int misInfoId)
        {
            try
            {
                var misType = await repo.DeleteMisType(misInfoId);
                if (misType)
                {
                    return Ok(new { success = true, result = misType, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("mistype")]
        public IActionResult GetAllMisType()
        {
            try
            {
                var misType = repo.GetAllMisType(); ;
                if (misType != null)
                {
                    return Ok(new { success = true, result = misType, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("mistype/{mistypeid}")]
        public IActionResult GetMisTypeById(int misInfoId)
        {
            try
            {
                var misType = repo.GetMisTypeById(misInfoId);
                if (misType != null)
                {
                    return Ok(new { success = true, result = misType, message = "mis has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "mis not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("mistype/{mistypeid}")]
        public async Task<IActionResult> UpdateMisType(int mistypeid, [FromBody] MisTypeViewModel entity)
        {
            if (entity == null)
            {
                return BadRequest();
            }

            var account = await repo.UpdateMisType(mistypeid, entity);
            if (account)
            {
                return NotFound(new { success = false, message = "No record found" });
            }

            try
            {
                await repo.UpdateMisType(mistypeid, entity);

                return Ok(new { success = true, result = entity, message = "account has been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion MisType
    }
}