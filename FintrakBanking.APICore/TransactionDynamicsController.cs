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

      [HttpGet] [ClaimsAuthorization]  
        [Route("transaction-dynamics")]
        public HttpResponseMessage GetTransactionDynamics()
        {
            try
            {
                var data = repo.GetAllTransactionDynamics();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("transaction-dynamics/application-detail/{detailId}")]
        public HttpResponseMessage GetTransactionDynamicsByDetailId(int detailId)
        {
            try
            {
                var data = repo.GetTransactionDynamicsByDetailId(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        #region CP template

      [HttpGet] [ClaimsAuthorization]  
        [Route("transaction-dynamics-template")]
        public HttpResponseMessage GetTransactionDynamicsTemplate()
        {
            try
            {
                var data = repo.GetTransactionDynamicsTemplate();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpPut]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this record {ex.Message}", error = ex.InnerException });
            }
        }

        #endregion CP template

        [HttpPut]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

        [HttpDelete]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }



      [HttpGet] [ClaimsAuthorization]  
        [Route("transaction-dynamics-template/application-detail/{detailId}")]
        public HttpResponseMessage GetTransactionDynamicsDefaultByDetailId(int detailId)
        {
            try
            {
                List<TransactionDynamicsViewModel> data = repo.GetTransactionDynamicsDefaultByDetailId(detailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}", error = ex.InnerException });
            }
        }

    }
}
