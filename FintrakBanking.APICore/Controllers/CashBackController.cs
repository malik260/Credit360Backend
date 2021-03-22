using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")]
    public class CashBackController : ApiControllerBase
    {
        private readonly ICashBackRepository _repo;
        private readonly TokenDecryptionHelper _token = new TokenDecryptionHelper();
        public CashBackController(ICashBackRepository repo)
        {
            this._repo = repo;
        }


        #region cashback
        [HttpGet]
        [ClaimsAuthorization]
        [Route("cashback")]
        public HttpResponseMessage GetCashbackSectionByApplicationDetailId(int loanApplicationDetailId)
        {
            try
            {
                var alertViewModels = _repo.GetCashbackSectionByApplicationDetailId(loanApplicationDetailId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("cashback")]
        public HttpResponseMessage AddCashbackSection([FromBody] CashBackViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddCashbackSection(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("cashback/{id}")]
        public HttpResponseMessage UpdateCashbackSection([FromUri] int id, [FromBody] CashBackViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    createdBy = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = _repo.UpdateCashbackSection(id, entity, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record {e.Message}" });
            }
        }
        #endregion

    }
}
