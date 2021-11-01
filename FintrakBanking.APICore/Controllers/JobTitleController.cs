using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setup")]
    public class JobTitleController : ApiControllerBase
    {
        private IJobTitleRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public JobTitleController(IJobTitleRepository _repo)
        {
            this.repo = _repo;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("jobtitle/{jobtitleid}")]
        public HttpResponseMessage GetJobTitle(int jobTitleId)
        { 
                try
                {
                    var jobtitle = repo.GetJobTitle(jobTitleId);

                    if (jobtitle == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = jobtitle });
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                } 
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("jobtitle/company")]
        public HttpResponseMessage GetJobTitleByCompanyId(HttpRequestMessage request)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();
                    var jobtitle = repo.GetJobTitleByCompanyId(token.GetCompanyId);

                    if (jobtitle == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = jobtitle });
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                }
                  
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("jobtitle")]
        public HttpResponseMessage JobTitle(HttpRequestMessage request)
        { 
                try
                {
                    var jobtitle = repo.JobTitle();

                    if (jobtitle == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                    new { success = false, message = "No record found" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = jobtitle });
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                } 
        }
         [HttpPost] [ClaimsAuthorization]
        [Route("job-title")]
        public HttpResponseMessage AddUpdateJobTitle([FromBody] JobTitleViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.jobTitleId != 0 || entity.jobTitleId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateJobTitle(entity.jobTitle))
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

               
                var data = repo.AddUpdateJobTitle(entity);
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
    }
}