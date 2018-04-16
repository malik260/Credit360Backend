using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.ViewModels.Setups.General;
using System;

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

        [HttpGet]
        [Route("default-staff-role")]
        public HttpResponseMessage GetStaffRoles()
        {
            try
            {
                var data = repo.GetStaffRoles();

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

        [HttpPost]
        [Route("staff-role")]
        public HttpResponseMessage AddUpdateStaffRole([FromBody] StaffRoleViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.staffRoleId != 0 || entity.staffRoleId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateStaffRole(entity.staffRoleCode, entity.staffRoleName))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                               new { success = false, message = "Staff Role with same Name or Code already exist." });
                    }
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateStaffRole(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
      
    }
}