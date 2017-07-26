using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels.Risk;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.APICore.JWTAuth;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/risk")] 
    public class RriskAssessmentController : BaseController
    {
        private IRiskImplementation repo;

        public RriskAssessmentController(IRiskImplementation _repo)
        {
            this.repo = _repo;
        }
        [HttpGet("riskassessment/productid/{pid}/risktypeid/{rtid}")]
        public IActionResult GetRiskIndexByRiskTitle(int pid,int rtid)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repo.GetRiskIndexByRiskTitle(token.GetCompanyId, pid, rtid);

                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost("riskassessment")]
        public IActionResult AssessmentRisk(TreeNode treeNode)
        {
            //try
            //{
            //    var token = new TokenDecryptionHelper(this.HttpContext);

            //   var response = repo.GetRiskIndexByRiskTitle(token.GetCompanyId, id);

            //    if (!response.Any())
            //    {
            //        return Ok(new { success = false, message = "No record found" });
            //    }

            //    return Ok(new { success = true, result = response, count = response.Count });
            //}
            //catch (Exception e)
            //{
            //    return Ok(new { success = false, message = $"Error: {e.Message}" });
            //}
            return null;
        }
    }
}