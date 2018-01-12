using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class PrudentialGuidelineController : ApiController
    {


        private IPrudentialGuidelineSetupRepository repo;
        private TokenDecryptionHelper token = new TokenDecryptionHelper();

        public PrudentialGuidelineController(IPrudentialGuidelineSetupRepository _repo)
        {
            repo = _repo;
        }

        [HttpGet]
        [Route("get-prudential-guidelines")]
        public HttpResponseMessage getAllPrudentialGuidelines()
        {
            try
            {
                var data = repo.GetAllGuidelines(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-prudential-guideline/{Id}")]
        public HttpResponseMessage getprudentialGuideline(int prudentialGuidelineId)
        {
            try
            {
                var data = repo.getGuideline(prudentialGuidelineId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("add-prudential-guideline")]
        public HttpResponseMessage addPrudentialGuideline([FromBody]PrudentialGuidelineViewModel guideline)
        {
            try
            {
                guideline.companyId = token.GetCompanyId;

                var data = repo.AddGuideline(guideline);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [HttpPut]
        [Route("update-prudential-guideline/{prudentialGuidelineId}")]
        public HttpResponseMessage updatePrudentialGuideline(int prudentialGuidelineId, PrudentialGuidelineViewModel guideline)
        {
            try
            {
                var data = repo.UpdateGuideline(guideline, prudentialGuidelineId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }
        [HttpDelete]
        [Route("delete-prudential-guideline/{prudentialGuidelineId}")]
        public HttpResponseMessage deletePrudentialGuideline(int prudentialGuidelineId, PrudentialGuidelineViewModel guideline)
        {
            try
            {
                var data = repo.DeleteGuideline(prudentialGuidelineId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = ex.Message });
            }
        }



    }
}

