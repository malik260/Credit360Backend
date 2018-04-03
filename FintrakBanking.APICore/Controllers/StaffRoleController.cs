using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class StaffRoleController : ApiControllerBase
    {
        private IStaffRoleRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //public RankController(IRankRepository _repo)
        //{
        //    this.repo = _repo;
        //}
        public StaffRoleController(IStaffRoleRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet][Route("staff-role")]
        public HttpResponseMessage GetStaffRole()
        {
            try
            {
                var data = repo.GetStaffRole();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("staff-role/{staffRoleId}")]
        public HttpResponseMessage GetStaffRole(int rankId)
        {
            try
            {
                var data = repo.GetStaffRole(rankId);

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

        [HttpGet][Route("staff-role/company")]
        public HttpResponseMessage GetStaffRoleByCompanyId()
        {
            try
            {
                var data = repo.GetStaffRoleByCompanyId(token.GetCompanyId);

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