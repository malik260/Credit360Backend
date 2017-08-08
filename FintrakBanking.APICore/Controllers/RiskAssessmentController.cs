using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels.Risk;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/risk")] 
    public class RiskAssessmentController : ApiControllerBase
    {
        private IRiskImplementation repo;

        public RiskAssessmentController(IRiskImplementation _repo)
        {
            this.repo = _repo;
        }

        [HttpGet][Route("risk-assessment/productid/{pid}/risktypeid/{rtid}")]
        public HttpResponseMessage GetRiskIndexByRiskTitle(int pid,int rtid)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskIndexByRiskTitle(token.GetCompanyId, pid, rtid);

                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost][Route("risk-assessment")]
        public HttpResponseMessage AssessmentRisk(TreeNode treeNode)
        {
            //try
            //{
            //    TokenDecryptionHelper token = new TokenDecryptionHelper();

            //   var data = repo.GetRiskIndexByRiskTitle(token.GetCompanyId, id);

            //    if (!data.Any())
            //    {
            //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            //    }

            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count });
            //}
            //catch (Exception e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            //}
            return null;
        }
    }
}