using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.CRMS;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Finance;
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
        private IEndOfDayRepository repoEOD;
        private ICRMSRegulatories crmsRegulatories;

        public FinacleIntegrationController(IFinacleIntegrationRepository repo, IEndOfDayRepository _repoEOD, ICRMSRegulatories _crmsRegulatories)
        {
            _repo = repo;
            this.repoEOD = _repoEOD;
            this.crmsRegulatories = _crmsRegulatories;
        }

        #region
        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }


        [HttpPost]
        [Route("batch-posting/count")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetBatchPostingCount(DateRange model)
        {
            try
            {
                //var response = repoEOD.GetBatchPostingMain(model.startDate, model.endDate, model.searchInfo);

                var response = repoEOD.RefreshStagingMonitoring(model.startDate, model.endDate);
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: an error occured" });
            }
        }

        [HttpPost]
        [Route("batch-posting/batchposting")]
        [ClaimsAuthorization]
        public HttpResponseMessage GenerateBatchPosting(DateRange model)
        {

            try
            {
                var fileBytes = crmsRegulatories.GenerateBatchPosting(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }

        }

  
         

        [HttpPost]
        [Route("daily-accrual-detail")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetDailyAccrualDetails(DateRange model )
        {

            try
            {
                var fileBytes = _repo.GenerateExcell(model.date, model.loanAcct);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }

        }


        [HttpPost]
        [Route("batch-posting/errorLog")]
        [ClaimsAuthorization]
        public HttpResponseMessage GetEODErrorLogDetail(FinanceEndofdayViewModel model)
        {

            try
            {
                var fileBytes = _repo.GetEODErrorLogDetail(model);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = fileBytes });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { data = "no-record", success = false, message = $"Error: {ce.Message}" });
            }
            catch (BadLogicException be)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: {be.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = $"Error: an error occured" });
            }



        }


        #endregion
    }
}
