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
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{


    [RoutePrefix("api/v1/admin")]
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
        public HttpResponseMessage GetBranch(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var branch = branchRepo.GetAllBranch();
                    response = request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());
                }
                catch (Exception ex)
                {

                     response = request.CreateResponse(HttpStatusCode.OK,
                         Ok(new { success = false, message = ex.Message }));
                }
                return response;

            });

        }
            

        [HttpGet][Route("branch/{id}")]
        public HttpResponseMessage GetBranch(HttpRequestMessage request,short id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var branch = branchRepo.GetBranch(id);
                    response = request.CreateResponse<BranchViewModel>(HttpStatusCode.OK, branch);                    
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse (HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("branch-by-company/{companyid}")]
        public HttpResponseMessage GetAllBranchByCompany(HttpRequestMessage request, int companyid)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var branch = branchRepo.GetAllBranchByCompanyId(companyid);
                    response = request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });

        }

        [HttpGet][Route("branch/company")]
        public HttpResponseMessage GetBranchByCompany(HttpRequestMessage request)
        {
           // token = new TokenDecryptionHelper(this.HttpContext);
           

            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var branch = branchRepo.GetAllBranchByCompanyId(token.GetCompanyId);
                    response = request.CreateResponse<List<BranchViewModel>>(HttpStatusCode.OK, branch.ToList());
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPost][Route("branch")]
        public  HttpResponseMessage  AddBranch(HttpRequestMessage request,[FromBody]AddBranchViewModel model)
        {
            HttpResponseMessage response = null;
            //  token = new TokenDecryptionHelper(this.HttpContext);
            // model.createdBy = token.GetStaffId;
            // model.userBranchId = (short)token.GetBranchId;
            // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            //  model.applicationUrl = Request.Path.Value;
            // model.companyId = token.GetCompanyId;
            return    GetHttpResponse(request, () =>
            {
                try
                {

                    var result =   branchRepo.AddBranch(model).IsCompleted;
                    if (result)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                         Ok(new { success = true, result = response, message = "Branch has been created successfully" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = false, message = "There was an error saving this record" }));

                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
            
        }

        [HttpPut]
        [Route("branch/{id}")]
        public HttpResponseMessage UpdateBranch(HttpRequestMessage request, [FromBody] BranchViewModel model, short id)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    //    token = new TokenDecryptionHelper(this.HttpContext);
                    //model.createdBy = token.GetStaffId;
                    //model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    //model.companyId = token.GetCompanyId;
                    var result = branchRepo.UpdateBranch(model, id).IsCompleted;
                    if (result)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                               Ok(new { success = true, result = result, message = "Branch has been created successfully" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error updating this record" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpDelete][Route("branch/{id}")]
        public HttpResponseMessage DeleteBranch(HttpRequestMessage request, [FromBody] short id)
        { HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        //applicationUrl = Request.Path.Value,
                        //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };
                    var branch = branchRepo.DeleteBranch(id, user).IsCompleted;
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(branch));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        #endregion Branch Setup

       
    }
}