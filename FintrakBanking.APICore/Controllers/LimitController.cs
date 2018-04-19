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
using FintrakBanking.Common.Enum;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class LimitController : ApiControllerBase
    {
        private readonly ILimitRepository repo;
        private readonly TokenDecryptionHelper token = new TokenDecryptionHelper();

        public LimitController(ILimitRepository _repo)
        {
            this.repo = _repo;
        }

        #region Limits
        [HttpGet]
        [Route("limit")]
        public HttpResponseMessage GetAllLimit()
        {
            try
            {

                var response = repo.GetAllLimit(token.GetCompanyId);
                if (!response.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("limit/{limitId}")]
        public HttpResponseMessage GetLimitById(int limitId)
        {
            try
            {

                var response = repo.GetLimitById(limitId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("limit")]
        public HttpResponseMessage AddLimit([FromBody] LimitViewModel model)
        {
            try
            {

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

        [HttpPut]
        [Route("limit/{limitId}")]
        public HttpResponseMessage UpdateLimit(int LimitId, [FromBody] LimitViewModel model)
        {
            try
            {
                ;
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

        [HttpDelete]
        [Route("limit/{limitId}")]
        public HttpResponseMessage DeleteLimit(int LimitId)
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
        [HttpGet]
        [Route("limit-detailObligor")]
        public HttpResponseMessage GetLimitDetailObligor()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                int typeId = (int)LimitType.Obligor;
                var response = repo.GetAllLimitDetail(typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("limit-detailCustomerGroup")]
        public HttpResponseMessage GetLimitDetailCustomerGroup()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                int typeId = (int)LimitType.CustomerGroup;
                var response = repo.GetAllLimitDetail(typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("limit-detailSector")]
        public HttpResponseMessage GetLimitDetailSector()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                int typeId = (int)LimitType.Sector;
                var response = repo.GetAllLimitDetail(typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("limit-detailBranch")]
        public HttpResponseMessage GetLimitDetailBranch()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                int typeId = (int)LimitType.Branch;
                var response = repo.GetAllLimitDetail(typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("limit-detailRelationshipManager")]
        public HttpResponseMessage GetLimitDetailRelationshipManager()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                int typeId = (int)LimitType.RelationshipManager;
                var response = repo.GetAllLimitDetail(typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [Route("limit-detailPrelimemaryEvaluationNote")]
        public HttpResponseMessage GetLimitDetailPrelimemaryEvaluationNote()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                int typeId = (int)LimitType.PrelimemaryEvaluationNote;
                var response = repo.GetAllLimitDetail(typeId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("limit-detail/{limitDetailId}")]
        public HttpResponseMessage GetLimitDetailById(int limitDetailId)
        {
            try
            {

                var response = repo.GetLimitDetailById(limitDetailId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("limit-detail")]
        public HttpResponseMessage AddLimitDetail([FromBody] LimitDetailViewModel model)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                if (model.limitTypeId != (int)LimitType.PrelimemaryEvaluationNote && model.targetId == -1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Please select your entry to continue" });
                }
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

        [HttpPost]
        [Route("limit-detail-multiple")]
        public HttpResponseMessage AddMultipleLimitDetail([FromBody] List<LimitDetailViewModel> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.userIPAddress = HttpContext.Current.Request.Url.AbsoluteUri;
                    item.applicationUrl = HttpContext.Current.Request.Path;
                    item.createdBy = token.GetStaffId;
                    item.companyId = token.GetCompanyId;
                }

                var response = repo.AddMultipleLimitDetail(model);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "The records have been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating these records" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating these records {e.Message}" });
            }
        }

        [HttpPut]
        [Route("limit-detail/{limitDetailId}")]
        public HttpResponseMessage UpdateLimitDetail(int limitDetailId, [FromBody] LimitDetailViewModel model)
        {
            try
            {

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

        [HttpDelete]
        [Route("limit-detail/{limitDetailId}")]
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
        [HttpGet]
        [Route("limit-metric")]
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
        [HttpGet]
        [Route("limit-type")]
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
        [HttpGet]
        [Route("limit-value-type")]
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
        [HttpGet]
        [Route("limit-frequency-type")]
        public HttpResponseMessage GetAllFrequencyType()
        {
            try
            {
                var response = repo.GetAllFrequencyType();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Obligor Limit 
        [HttpGet]
        [Route("obligor-limit")]
        public HttpResponseMessage GetAllObligorLimit()
        {
            try
            {
                var response = repo.GetAllObligorLimit();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost]
        [Route("obligor-limit")]
        public HttpResponseMessage AddUpdateObligorLimit([FromBody] ObligorLimitViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.riskRatingId != 0 || entity.riskRatingId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateRiskRating(entity.riskRating))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                               new { success = false, message = "Risk Rating with same Name or Code already exist." });
                    }
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateRiskRating(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Changes Saved Successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Saved Changes not Successfull" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error saving this record {e.Message}" });
            }
        }
        #endregion
    }
}