using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups/risk")]
    public class RiskController : ApiControllerBase
    {
        IErrorLogRepository errorLogger;
        private IRiskSetupRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public RiskController(IRiskSetupRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region Risk Assessment Index

        [HttpPost][Route("risk-assessment-index")]
        public HttpResponseMessage AddRiskAssessmentIndex([FromBody]  RiskAssessmentIndexViewModels entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                //entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                //entity.applicationUrl = Request.Path.Value;

                var data = repo.AddRiskAssessmentIndexs(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                //this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete]
        [Route("risk-assessment-index/{id}")]
        public HttpResponseMessage DeleteForeHeader(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.ApplicationPath,
                   // userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteRiskAssessmentIndex(id, user).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("risk-assessment-index/{id}")]
        public HttpResponseMessage UpdateRiskAssessmentIndex(int id, [FromBody]  RiskAssessmentIndexViewModels entity)
        {
            try
            {
                // token = new TokenDecryptionHelper(HttpContext);
                // entity.userBranchId = (short)token.GetBranchId;
                // entity.companyId = token.GetCompanyId;
                // entity.lastUpdatedBy = token.GetStaffId;
                // entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateRiskAssessmentIndex(id, entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("risk-assessment-index/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexById(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentIndexById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("risk-assessment-index/risktitle/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexByRiskTitle(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentIndexByRiskTitle(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("risk-assessment-index/parent/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexByParent(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentIndexByParent(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        [Route("risk-assessment-index/itemlevel/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexByItemLevel(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentIndexByItemLevel(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Risk Assessment Index

        #region RiskRating

        [HttpPost]
        [Route("risk-rating")]
        public HttpResponseMessage AddRiskRating([FromBody] RiskRatingViewModel entity)
        {
            try
            {
                var data = repo.AddRiskRating(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpDelete]
        [Route("risk-rating/{ratingId}")]
        public HttpResponseMessage DeleteRiskRating(int ratingId, [FromBody] RiskRatingViewModel entity)
        {
            try
            {
                var data = repo.DeleteRiskRating(ratingId, entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet]
        [Route("risk-rating")]
        public HttpResponseMessage GetRiskRating()
        {
            try
            {
                var data = repo.GetRiskRating();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("risk-rating-by-product/{productId}")]
        public HttpResponseMessage GetRiskRatingByProductId(int productId)
        {
            try
            {
                var data = repo.GetRiskRatingByProductId(productId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No activity found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("risk-rating/{ratingId}")]
        public HttpResponseMessage UpdateRiskRating(int ratingId, [FromBody]  RiskRatingViewModel entity)
        {
            try
            {
                var data = repo.UpdateRiskRating(ratingId, entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        #endregion RiskRating


        #region Risk Assessment title
        [HttpPost]
        [Route("risk-assessment-title")]
        public HttpResponseMessage AddRiskAssessmentTitle([FromBody]RiskAssessmentTitleViewModels entity)
        {
            try
            {
                // token = new TokenDecryptionHelper(HttpContext);
                // entity.userBranchId = (short)token.GetBranchId;
                // entity.companyId = token.GetCompanyId;
                // entity.createdBy = token.GetStaffId;
                // entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddRiskAssessmentTitle(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }

        }

        [HttpPut]
        [Route("risk-assessment-title/{id}")]
        public HttpResponseMessage UpdateRiskAssessmentTitle(int id, [FromBody] RiskAssessmentTitleViewModels entity)
        {
            try
            {
                // token = new TokenDecryptionHelper(HttpContext);
                // entity.userBranchId = (short)token.GetBranchId;
                // entity.companyId = token.GetCompanyId;
                // entity.lastUpdatedBy = token.GetStaffId;
                // entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateRiskAssessmentTitle(id, entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete]
        [Route("risk-assessment-title/{id}")]
        public HttpResponseMessage DeleteRiskAssessmentTitle(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                UserInfo user = new UserInfo()
                {
                    // BranchId = token.GetBranchId,
                    // companyId = token.GetCompanyId,
                    // staffId = token.GetStaffId,
                    // applicationUrl = Request.Path.Value,
                    // userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteRiskAssessmentTitle(id, user).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("risk-assessment-title/{id}")]
        public HttpResponseMessage GetRiskAssessmentTitleById(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentTitleById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("risk-assessment-title/product/{id}")]
        public HttpResponseMessage GetRiskAssessmentTitleByProductId(int id)
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentTitleByProductId(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("risk-assessment-title/risktype/{id}")]
        public HttpResponseMessage GetRiskAssessmentTitleByRiskType(int id)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentTitleByRiskType(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("risk-assessment-title")]
        public HttpResponseMessage GetRiskAssessmentTitle()
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetRiskAssessmentTitle(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                // this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

        #endregion Risk Assessment title
    }
}