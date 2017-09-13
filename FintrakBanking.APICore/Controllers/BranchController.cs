using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class BranchController : ApiControllerBase
    {
        private IBranchRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public BranchController(IBranchRepository repo)
        {
            this.repo = repo;
        }

        #region Branch Setup

        [HttpGet]
        [Route("branch")]
        public HttpResponseMessage GetBranch()
        {
            try
            {
                var data = repo.GetAllBranch();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("branch/{id}")]
        public HttpResponseMessage GetBranch(short id)
        {
            try
            {
                var branch = repo.GetBranch(id);
                return Request.CreateResponse<BranchViewModel>(HttpStatusCode.OK, branch);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("branch-by-company/{companyid}")]
        public HttpResponseMessage GetAllBranchByCompany(int companyid)
        {
            try
            {
                var branch = repo.GetAllBranchByCompanyId(companyid);
                return Request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("branch/company")]
        public HttpResponseMessage GetBranchByCompany()
        {
            try
            {
                var branch = repo.GetAllBranchByCompanyId(token.GetCompanyId);
                return Request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());

            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("branch")]
        public async Task<HttpResponseMessage> AddBranchAsync([FromBody]AddBranchViewModel model)
        {
            try
            {
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result = await repo.AddBranch(model);
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = result, message = "Branch has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = "There was an error saving this record" });

            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("branch/{id}")]
        public async Task<HttpResponseMessage> UpdateBranchAsync([FromBody] BranchViewModel model, short id)
        {

            try
            {
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result = await repo.UpdateBranch(model, id);
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = result, message = "Branch has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }

        }

        [HttpDelete]
        [Route("branch/{id}")]
        public async Task<HttpResponseMessage> DeleteBranchAsync([FromBody] short id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };
                var branch = await repo.DeleteBranch(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, Ok(branch));
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        #endregion Branch Setup
    }
}