using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [Authorize]
    [RoutePrefix("api/test")]
    public class TestCollateralController : ApiControllerBase
    {
        private ITestCollateralRepository _testCollateralRepo;

        public TestCollateralController(ITestCollateralRepository testCollateralRepo)
        {
            _testCollateralRepo = testCollateralRepo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-collateral-types/collateral/{collateralId}/type/{typeId}")]
        public HttpResponseMessage GetCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            try
            {
                var res = _testCollateralRepo.GetCustomerCollateralByCollateralId(collateralId, typeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error fetchcing the records. Error - {ex.Message}" });
            }
        }
    }
}
