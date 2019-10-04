using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Web;
using FintrakBanking.ViewModels;
using FintrakBanking.APICore.JWTAuth;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class AlertController : ApiController
    {
        private readonly IAlertRepository _repo;
        private readonly TokenDecryptionHelper _token = new TokenDecryptionHelper();

        public AlertController(IAlertRepository repo)
        {
            this._repo = repo;
        }

        #region Region Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-title")]
        public HttpResponseMessage GetAlertTitle()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlerts();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-title/{id}")]
        public HttpResponseMessage GetAlertTitleById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-title")]
        public HttpResponseMessage AddAlertTitle([FromBody] AlertViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertTitle(entity);
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
        [Route("alert-title/{id}")]
        public HttpResponseMessage UpdateAlertTitle([FromUri] int id, [FromBody] AlertViewModel entity)
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

                var data = _repo.UpdateAlertTitle(id, entity, user);
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

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("alert-title/{id}")]
        public HttpResponseMessage DeleteOriginalDocumentApproval(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertTitle(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

    }

}
