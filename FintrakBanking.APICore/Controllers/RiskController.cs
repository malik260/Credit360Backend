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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups/risk")]
    public class RiskController : ApiControllerBase
    {
        //IErrorLogRepository errorLogger;
        private IRiskSetupRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public RiskController(IRiskSetupRepository _repo
            //, IErrorLogRepository _errorLogger
            )
        {
            this.repo = _repo;
            //errorLogger = _errorLogger;
        }

        #region Risk Assessment Index

         [HttpPost] [ClaimsAuthorization][Route("risk-assessment-index")]
        public async Task<HttpResponseMessage> AddRiskAssessmentIndex([FromBody]  RiskAssessmentIndexViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.AddRiskAssessmentIndexs(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("risk-assessment-index/{id}")]
        public async Task<HttpResponseMessage> DeleteForeHeader(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.ApplicationPath,
                };

                var data = await repo.DeleteRiskAssessmentIndex(id, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("risk-assessment-index/{id}")]
        public async Task<HttpResponseMessage> UpdateRiskAssessmentIndex(int id, [FromBody]  RiskAssessmentIndexViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.UpdateRiskAssessmentIndex(id, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-index/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexById(int id)
        {
            try
            {
                var data = repo.GetRiskAssessmentIndexById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-index/risktitle/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexByRiskTitle(int id)
        {
            try
            {
                var data = repo.GetRiskAssessmentIndexByRiskTitle(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-index/parent/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexByParent(int id)
        {
            try
            {
                var data = repo.GetRiskAssessmentIndexByParent(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-index/itemlevel/{id}")]
        public HttpResponseMessage GetRiskAssessmentIndexByItemLevel(int id)
        {
            try
            {
                var data = repo.GetRiskAssessmentIndexByItemLevel(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Risk Assessment Index

        #region RiskRating

         [HttpPost] [ClaimsAuthorization]
        [Route("risk-rating")]
        public async Task<HttpResponseMessage> AddRiskRating([FromBody] RiskRatingViewModel entity)
        {
            try
            {
                var data = await repo.AddRiskRating(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("risk-rating/{ratingId}")]
        public async Task<HttpResponseMessage> DeleteRiskRating(int ratingId, [FromBody] RiskRatingViewModel entity)
        {
            try
            {
                var data = await repo.DeleteRiskRating(ratingId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

       [HttpPut] [ClaimsAuthorization]
        [Route("risk-rating/{ratingId}")]
        public async Task<HttpResponseMessage> UpdateRiskRating(int ratingId, [FromBody]  RiskRatingViewModel entity)
        {
            try
            {
                var data = await repo.UpdateRiskRating(ratingId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        #endregion RiskRating


        #region Risk Assessment title
         [HttpPost] [ClaimsAuthorization]
        [Route("risk-assessment-title")]
        public async Task<HttpResponseMessage> AddRiskAssessmentTitleAsync([FromBody]RiskAssessmentTitleViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.AddRiskAssessmentTitle(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }

        }

       [HttpPut] [ClaimsAuthorization]
        [Route("risk-assessment-title/{id}")]
        public async Task<HttpResponseMessage> UpdateRiskAssessmentTitle(int id, [FromBody] RiskAssessmentTitleViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = await repo.UpdateRiskAssessmentTitle(id, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete] [ClaimsAuthorization]
        [Route("risk-assessment-title/{id}")]
        public async Task<HttpResponseMessage> DeleteRiskAssessmentTitle(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress,
                };

                var data = await repo.DeleteRiskAssessmentTitle(id, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-title/{id}")]
        public HttpResponseMessage GetRiskAssessmentTitleById(int id)
        {
            try
            {
                var data = repo.GetRiskAssessmentTitleById(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-title/product/{id}")]
        public HttpResponseMessage GetRiskAssessmentTitleByProductId(int id)
        {

            try
            {
                var data = repo.GetRiskAssessmentTitleByProductId(id, token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("risk-assessment-title")]
        public HttpResponseMessage GetRiskAssessmentTitle()
        {

            try
            {
                var data = repo.GetRiskAssessmentTitle(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

        #endregion Risk Assessment title
    }
}