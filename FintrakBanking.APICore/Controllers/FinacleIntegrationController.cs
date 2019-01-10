using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/finacle-integration")]
    public class FinacleIntegrationController : ApiControllerBase
    {
        private IFinacleIntegrationRepository _repo;
        public FinacleIntegrationController(IFinacleIntegrationRepository repo)
        {
            _repo = repo;
        }

        #region
        [HttpGet]
        [Route("batch-posting/detail")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBatchPostingDetail(DateRange model)
        {
            try
            {
                var response = _repo.GetBatchPostingDetail(model.startDate, model.endDate, model.searchInfo);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpGet]
        [Route("batch-posting/main")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBatchPostingMain(DateRange model)
        {
            try
            {
                var response = _repo.GetBatchPostingMain(model.startDate, model.endDate, model.searchInfo);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }
        #endregion
    }
}
