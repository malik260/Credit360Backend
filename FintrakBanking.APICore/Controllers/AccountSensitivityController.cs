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
        public HttpResponseMessage GetSensitivityLevels( )
        {  
                try
                {
                    var sensitivityLevel = repo.GetAllAccountSensitivityLevels();
                    return Request.CreateResponse(HttpStatusCode.OK, Ok(sensitivityLevel));
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { error = true, message = ex.Message });
                }
                  
        }

        [HttpGet]
        [Route("getaccountsensitivity/{levelId}")]
        public HttpResponseMessage GetSensitivityLevels(  int levelId)
        { 
                try
                {
                    var sensitivityLevel = repo.GetAccountSensitivityLevelsByLevelId(levelId);
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = sensitivityLevel });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { error = true, message = ex.Message });
                }
                 
        }
    }
}