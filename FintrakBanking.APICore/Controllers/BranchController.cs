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
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class BranchController : ApiControllerBase
    {
        private readonly IBranchRepository _repo;
        readonly TokenDecryptionHelper _token = new TokenDecryptionHelper();

        public BranchController(IBranchRepository repo)
        {
            this._repo = repo;
        }

        #region Region Setup
      [HttpGet] [ClaimsAuthorization]  
        [Route("region")]
        public HttpResponseMessage GetRegion()
        {
            try
            {
                var branchRegionViewModels = _repo.GetAllRegion();
                var data = branchRegionViewModels;
                var regionViewModels = data as BranchRegionViewModel[] ?? data.ToArray();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = regionViewModels, count = regionViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
         [HttpPost] [ClaimsAuthorization]
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
                    if (_repo.ValidateRegionName(entity.regionName.Trim()))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = $"Region with name {entity.regionName} already exist." });
                    }
                }
              
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddUpdateBranchRegion(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        #endregion

        #region Branch Setup

      [HttpGet] [ClaimsAuthorization]  
        [Route("branch")]
        public HttpResponseMessage GetBranch()
        {
            try
            {
                var data = _repo.GetAllBranch();
                var branchViewModels = data as BranchViewModel[] ?? data.ToArray();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = branchViewModels, count = branchViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("branch/{id}")]
        public HttpResponseMessage GetBranch(short id)
        {
            try
            {
                var branch = _repo.GetBranch(id);
                return Request.CreateResponse<BranchViewModel>(HttpStatusCode.OK, branch);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("branch-by-company/{companyid}")]
        public HttpResponseMessage GetAllBranchByCompany(int companyid)
        {
            try
            {
                var branch = _repo.GetAllBranchByCompanyId(companyid);
                return Request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("branch/company")]
        public HttpResponseMessage GetBranchByCompany()
        {
            try
            {
                var branch = _repo.GetAllBranchByCompanyId(_token.GetCompanyId);
                return Request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("branch")]
        public async Task<HttpResponseMessage> AddBranchAsync([FromBody]AddBranchViewModel model)
        {
            try
            {
                model.createdBy = _token.GetStaffId;
                model.userBranchId = (short)_token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = _token.GetCompanyId;
                var result = await _repo.AddBranch(model);
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = result, message = "Branch has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = "There was an error saving this record, Branch Name Exist" });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("branch/{id}")]
        public async Task<HttpResponseMessage> UpdateBranchAsync([FromBody] BranchViewModel model, short id)
        {

            try
            {
                model.createdBy = _token.GetStaffId;
                model.userBranchId = (short)_token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = _token.GetCompanyId;
                var result = await _repo.UpdateBranch(model, id);
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = result, message = "Branch has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }

        }


       [HttpPut] [ClaimsAuthorization]
        [Route("branches/{id}")]
        public HttpResponseMessage UpdateBranch([FromBody] BranchViewModel model, short id)
        {

            try
            {
                model.createdBy = _token.GetStaffId;
                model.userBranchId = (short)_token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = _token.GetCompanyId;
                var result =  _repo.UpdateBranches(model, id);
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = result, message = "Changes saved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Saved changes not successfull" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }

        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("branch/{id}")]
        public async Task<HttpResponseMessage> DeleteBranchAsync([FromBody] short id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    staffId = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };
                var branch = await _repo.DeleteBranch(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, Ok(branch));
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("branch-search")]
        public HttpResponseMessage SearchForBranch(string searchQuery)
        {
            try
            {
                var data = _repo.GetSearchedBranch(searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }
        }
        #endregion Branch Setup
    }
}