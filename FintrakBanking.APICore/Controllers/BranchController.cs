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
using System.Threading.Tasks;
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

        #region Region Setup
        [HttpGet]
        [Route("region")]
        public HttpResponseMessage GetRegion()
        {
            try
            {
                var data = repo.GetAllRegion();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("region")]
        public HttpResponseMessage AddBranchRegion([FromBody]BranchRegionViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.regionId != 0 || entity.regionId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                }
                if (repo.ValidateRegionName(entity.regionName.Trim()))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"Region with name {entity.regionName} already exist." });
                }
                entity.companyId = token.GetCompanyId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateBranchRegion(entity);
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
        #endregion

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
              new { success = false, message = "There was an error saving this record, Branch Name Exist" });

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


        [HttpPut]
        [Route("branches/{id}")]
        public HttpResponseMessage UpdateBranch([FromBody] BranchViewModel model, short id)
        {

            try
            {
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result =  repo.UpdateBranches(model, id);
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