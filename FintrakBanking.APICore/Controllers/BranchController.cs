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

    //[EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    [RoutePrefix("api/v1/setups")]
    public class BranchController : ApiControllerBase
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

        [HttpGet]
        [Route("branch")]
        public HttpResponseMessage GetBranch()
        {
            try
            {
                var data = branchRepo.GetAllBranch();
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
                var branch = branchRepo.GetBranch(id);
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
                var branch = branchRepo.GetAllBranchByCompanyId(companyid);
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
            token = new TokenDecryptionHelper();

            try
            {
                
                var branch = branchRepo.GetAllBranchByCompanyId(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { result = branch.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

        [HttpPost]
        [Route("branch")]
        public HttpResponseMessage AddBranch([FromBody]AddBranchViewModel model)
        {



            try
            {
                var token = new TokenDecryptionHelper();
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result = branchRepo.AddBranch(model).IsCompleted;
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = result, message = "Branch has been created successfully" });
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
        public HttpResponseMessage UpdateBranch([FromBody] BranchViewModel model, short id)
        {

            try
            {
               var token = new TokenDecryptionHelper();
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
               // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result = branchRepo.UpdateBranch(model, id).IsCompleted;
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, result = result, message = "Branch has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error updating this record" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpDelete]
        [Route("branch/{id}")]
        public HttpResponseMessage DeleteBranch([FromBody] short id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };
                var branch = branchRepo.DeleteBranch(id, user).IsCompleted;
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