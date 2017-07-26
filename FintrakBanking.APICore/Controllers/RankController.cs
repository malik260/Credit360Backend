using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setup")]
    public class RankController : ApiControllerBase
    {
        private IRankRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public RankController(IRankRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("rank")]
        public HttpResponseMessage GetRank()
        {
            try
            {
                var data = repo.GetRank();

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("rank/{rankId}")]
        public HttpResponseMessage GetRank(int rankId)
        {
            try
            {
                var data = repo.GetRank(rankId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("rank/company")]
        public HttpResponseMessage GetRankByCompanyId()
        {
            try
            {
                // token = new TokenDecryptionHelper(this.HttpContext);
                var data = repo.GetRankByCompanyId(token.GetCompanyId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}