using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Setups.General;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/accountsensitivity")]
    public class AccountSensitivity : ApiControllerBase
    {
        private IAccountSensitivityRepository repo;

        public AccountSensitivity(IAccountSensitivityRepository _repo)
        {
            repo = _repo;
        }

        [HttpGet]
        [Route("getaccountsensitivity")]
        public HttpResponseMessage GetSensitivityLevels(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var sensitivityLevel = repo.GetAllAccountSensitivityLevels();
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(sensitivityLevel));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { error = true, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("getaccountsensitivity/{levelId}")]
        public HttpResponseMessage GetSensitivityLevels(HttpRequestMessage request, int levelId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var sensitivityLevel = repo.GetAccountSensitivityLevelsByLevelId(levelId);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = sensitivityLevel }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, (new { error = true, message = ex.Message }));
                }
                return response;
            });
        }
    }
}