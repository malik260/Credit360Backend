using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.Enum;
using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class TransactionDynamicsController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ITransactionDynamicsRepository repo;

        public TransactionDynamicsController(ITransactionDynamicsRepository repo)
        {
            this.repo = repo;
        }


        #region SUGGESTED conditions

        [HttpGet]
        [ClaimsAuthorization]
        [Route("suggested-conditions/{applicationDetailsId}")]
        public HttpResponseMessage GetSuggestedConditions(int applicationDetailsId)
        {
                var data = repo.GetSuggestedConditions(applicationDetailsId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("suggested-conditions-byAppId/{applicationId}")]
        public HttpResponseMessage GetSuggestedConditionsByApplicationId(int applicationId)
        {
            var data = repo.GetSuggestedConditionsByApplicationId(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("suggested-conditions")]
        public HttpResponseMessage AddSuggestedConditions([FromBody] SuggestedConditionsViewModel entity)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            var data = repo.AddSuggestedConditions(entity);
                
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
             

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("suggested-conditions-update/{id}")]
        public HttpResponseMessage UpdateSuggestedConditions([FromBody] SuggestedConditionsViewModel entity, int id)
        {
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = token.GetCompanyId;
            entity.createdBy = token.GetStaffId;
            entity.lastUpdatedBy = token.GetStaffId;
            entity.applicationUrl = HttpContext.Current.Request.Path;

            var data = repo.UpdateSuggestedConditions(id, entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been modified successfully" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("suggested-conditions-delete/{conditionId}")]
        public HttpResponseMessage RemoveSuggestedConditions(int conditionId)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                staffId = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
            };
            var data = repo.RemoveSuggestedConditions(conditionId, user);
            
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been removed successfully" });
        }

        #endregion SUGGESTED conditions


        #region DEFAULT dynamics

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction-dynamics-template/application-detail/{detailId}")]
        public HttpResponseMessage GetTransactionDynamicsDefaultByDetailId(int detailId)
        {
            try
            {
                List<TransactionDynamicsViewModel> data = repo.GetTransactionDynamicsDefaultByDetailId(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics-template/application-detail/{detailId}")]
        public HttpResponseMessage GetTransactionDynamicsDefaultByDetailIdLms(int detailId)
        {
            try
            {
                List<TransactionDynamicsViewModel> data = repo.GetTransactionDynamicsDefaultByDetailIdLms(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction-dynamics-template")]
        public HttpResponseMessage GetTransactionDynamicsTemplate()
        {
            try
            {
                var data = repo.GetTransactionDynamicsTemplate();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("transaction-dynamics-template")]
        public HttpResponseMessage AddTransactionDynamicsTemplate([FromBody] TransactionDynamicsViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddTransactionDynamicsTemplate(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("transaction-dynamics-template/{conditionId}")]
        public HttpResponseMessage UpdateTransactionDynamicsTemplate([FromBody] TransactionDynamicsViewModel entity, int conditionId)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.UpdateTransactionDynamicsTemplate(entity, conditionId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics-template/application-detail/{detailId}/operation/{operationId}")]
        public HttpResponseMessage GetTransactionDynamicsDefaultByApplicationIdAndOperationLms(int detailId, int? operationId)
        {
            try
            {
                List<TransactionDynamicsViewModel> data = repo.GetTransactionDynamicsDefaultByApplicationIdAndOperationLms(detailId, operationId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("condition-precedent-template/application-detail/{detailId}/operation/{operationId}")]
        public HttpResponseMessage GetTransactionDynamicsDefaultByApplicationIdAndOperation(int detailId, int? operationId)
        {
            try
            {

                List<TransactionDynamicsViewModel> data = repo.GetTransactionDynamicsDefaultByDetailId(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion DEFAULT dynamics

        #region LOS dynamics

        [HttpGet]
        [ClaimsAuthorization]
        [Route("transaction-dynamics/application-detail/{detailId}")]
        public HttpResponseMessage GetTransactionDynamicsByDetailId(int detailId)
        {
            try
            {
                var data = repo.GetTransactionDynamicsByDetailId(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("transaction-dynamics")]
        public HttpResponseMessage AddTransactionDynamics([FromBody] TransactionDynamicsViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddTransactionDynamics(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("transaction-dynamics-edit/{id}")]
        public HttpResponseMessage EditLoanCditionPrecedent([FromBody] TransactionDynamicsViewModel entity, int id)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.EditLoanTransactionDynamics(id, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been modified successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("transaction-dynamics-remove/{id}")]
        public HttpResponseMessage RemoveLoanTransactionDynamics(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };
                var data = repo.RemoveLoanTransactionDynamics(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been removed successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("transaction-dynamics/selected")]
        public HttpResponseMessage AddSelectedTransactionDynamics([FromBody] SelectedIdsViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                List<TransactionDynamicsViewModel> data = repo.AddSelectedTransactionDynamics(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        #endregion LOS dynamics

        #region LMS dynamics

        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics/application-detail/{detailId}")]
        public HttpResponseMessage GetTransactionDynamicsByDetailIdLms(int detailId)
        {
            try
            {
                var data = repo.GetTransactionDynamicsByDetailIdLms(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics")]
        public HttpResponseMessage AddTransactionDynamicsLms([FromBody] TransactionDynamicsViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddTransactionDynamicsLms(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics-edit/{id}")]
        public HttpResponseMessage EditLoanCditionPrecedentLms([FromBody] TransactionDynamicsViewModel entity, int id)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.EditLoanTransactionDynamicsLms(id, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been modified successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics-remove/{id}")]
        public HttpResponseMessage RemoveLoanTransactionDynamicsLms(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };
                var data = repo.RemoveLoanTransactionDynamicsLms(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been removed successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("lms-transaction-dynamics/selected")]
        public HttpResponseMessage AddSelectedTransactionDynamicsLms([FromBody] SelectedIdsViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                List<TransactionDynamicsViewModel> data = repo.AddSelectedTransactionDynamicsLms(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        #endregion LMS dynamics

    }
}
