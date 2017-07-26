using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setup")]
    public class JobTitleController : ApiControllerBase
    {
        private IJobTitleRepository repo;
        TokenDecryptionHelper token = null;
        public JobTitleController(IJobTitleRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("jobtitle/{jobtitleid}")]
        public HttpResponseMessage GetJobTitle(HttpRequestMessage request, int jobTitleId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var jobtitle = repo.GetJobTitle(jobTitleId);

                    if (jobtitle == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = jobtitle }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("jobtitle/company")]
        public HttpResponseMessage GetJobTitleByCompanyId(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var jobtitle = repo.GetJobTitleByCompanyId(token.GetCompanyId);

                    if (jobtitle == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = jobtitle }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("jobtitle")]
        public HttpResponseMessage JobTitle(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var jobtitle = repo.JobTitle();

                    if (jobtitle == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                    Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = jobtitle }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
    }
}