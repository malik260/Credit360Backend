using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setups")]
    public class BranchController : BaseController
    {
        private IGeneralSetupRepository repo;
        private IBranchRepository branchRepo;
        TokenDecryptionHelper token = null;

        public BranchController(IGeneralSetupRepository _repo, IBranchRepository _branchRepo)
        {
            this.repo = _repo;
            this.branchRepo = _branchRepo;
        }

        #region Branch Setup

        [HttpGet("branch")]
        public IActionResult GetBranch()
        {
            try
            {
                var branches = branchRepo.GetAllBranch();
                return Ok(new
                {
                    result = branches,
                    count = branches.Count(),
                    success = true
                });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("branch/{id}")]
        public IActionResult GetBranch(short id)
        {
            try
            {
                var branch = branchRepo.GetBranch(id);
                return Ok(branch);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("branch-by-company/{companyid}")]
        public IActionResult GetAllBranchByCompany(int companyid)
        {
            try
            {
                var branches = branchRepo.GetAllBranchByCompanyId(companyid);
                return Ok(branches);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("branch/company")]
        public IActionResult GetBranchByCompany()
        {
            token = new TokenDecryptionHelper(this.HttpContext);
            try
            {
                var branches = branchRepo.GetAllBranchByCompanyId(token.GetCompanyId);
                return Ok(branches);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("branch")]
        public async Task<IActionResult> AddBranch([FromBody]AddBranchViewModel model)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.companyId = token.GetCompanyId;
                var response = await branchRepo.AddBranch(model);
                if (response)
                {
                    return Ok(new { success = true, result = response, message = "Branch has been created successfully" });
                }
                return Ok(new { success = false, message = "There was an error saving this record" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("branch/{id}")]
        public IActionResult UpdateBranch([FromBody] BranchViewModel model, short id)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.companyId = token.GetCompanyId;
                var response = branchRepo.UpdateBranch(model,id);
                if (response.Result)
                {
                    return Ok(new { success = true, result = response.Result, message = "Branch has been created successfully" });
                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("branch/{id}")]
        public IActionResult DeleteBranch([FromBody] short id)
        {
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
                var branch = branchRepo.DeleteBranch(id, user);
                return Ok(branch);
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion Branch Setup

       
    }
}