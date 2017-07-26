using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LimitController : ApiControllerBase
    {
        private ILimitRepository repo;

        public LimitController(ILimitRepository _repo)
        {
            this.repo = _repo;
        }

        #region Limits
        [HttpGet][Route("limit")]
        public HttpResponseMessage GetAllLimit()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var response = repo.GetAllLimit(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet][Route("limit/{limitId}")]
        public HttpResponseMessage GetLimitById(int limitId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var response = repo.GetLimitById(limitId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost][Route("limit")]
        public HttpResponseMessage AddLimit([FromBody] LimitViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddLimit(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut][Route("limit/{limitId}")]
        public HttpResponseMessage UpdateLimit(int LimitId, [FromBody] LimitViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper(); ;
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateLimit(LimitId, model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete][Route("limit/{limitId}")]
        public HttpResponseMessage DeleteLimit(int LimitId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                repo.DeleteLimit(LimitId, user);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = LimitId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Limits Details
        [HttpGet][Route("limit-detail")]
        public HttpResponseMessage GetAllLimitDetail()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var response = repo.GetAllLimitDetail();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet][Route("limit-detail/{limitDetailId}")]
        public HttpResponseMessage GetLimitDetailById(int limitDetailId)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var response = repo.GetLimitDetailById(limitDetailId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost][Route("limit-detail")]
        public HttpResponseMessage AddLimitDetail([FromBody] LimitDetailViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper() ;
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.AddLimitDetail(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut][Route("limit-detail/{limitDetailId}")]
        public HttpResponseMessage UpdateLimitDetail(int limitDetailId, [FromBody] LimitDetailViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.UpdateLimitDetail(limitDetailId, model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete][Route("limit-detail/{limitDetailId}")]
        public HttpResponseMessage DeleteLimitDetail(int limitDetailId)
        {
            try
            {
                TokenDecryptionHelper token = null;
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                repo.DeleteLimitDetail(limitDetailId, user);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = limitDetailId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Limits Metric
        [HttpGet][Route("limit-metric")]
        public HttpResponseMessage GetAllLimitMetric()
        {
            try
            {
                var response = repo.GetAllLimitMetric();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Limits Type
        [HttpGet][Route("limit-type")]
        public HttpResponseMessage GetAllLimitType()
        {
            try
            {
                var response = repo.GetAllLimitType();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Limits Value Type
        [HttpGet][Route("limit-value-type")]
        public HttpResponseMessage GetAllLimitValueType()
        {
            try
            {
                var response = repo.GetAllLimitValueType();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Frequency Type
        [HttpGet][Route("limit-frequency-type")]
        public HttpResponseMessage GetAllFrequencyType()
        {
            try
            {
                var response = repo.GetAllFrequencyType();

                return  Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion
    }
}