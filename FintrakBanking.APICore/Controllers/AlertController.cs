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
using FintrakBanking.APICore.JWTAuth;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class AlertController : ApiController
    {
        private readonly IAlertRepository _repo;
        readonly TokenDecryptionHelper _token = new TokenDecryptionHelper();

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
        #endregion

    }

}
