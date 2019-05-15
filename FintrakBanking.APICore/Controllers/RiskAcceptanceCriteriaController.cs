using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/rac")] 
    public class RiskAcceptanceCriteriaController : ApiControllerBase
    {
        private IRiskAcceptanceCriteriaRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public RiskAcceptanceCriteriaController(IRiskAcceptanceCriteriaRepository _repo)
        {
            this.repo = _repo;
        }
        

        [HttpGet]
        [ClaimsAuthorization]
        [Route("risk-acceptance-criteria/product/{productId}")]
        public HttpResponseMessage GetRiskAcceptanceCriteriaByProduct(int productId)
        {
            RiskAcceptanceCriteriaViewModel response = repo.GetRiskAcceptanceCriteriaByProduct(productId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.categories.Count() });
        }
        
    }
}
