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
        private IBranchRepository repo;

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
            TokenDecryptionHelper token = new TokenDecryptionHelper();

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
        public HttpResponseMessage AddBranch([FromBody]AddBranchViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result = repo.AddBranch(model).IsCompleted;
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
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
               // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.companyId = token.GetCompanyId;
                var result = repo.UpdateBranch(model, id).IsCompleted;
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
            TokenDecryptionHelper token = new TokenDecryptionHelper();
            try
            {
                UserInfo user = new UserInfo()
                {
                BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };
                var branch = repo.DeleteBranch(id, user).IsCompleted;
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