using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.APICore.Filters;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/test")]
    public class TestController : ApiControllerBase
    {
        private ICompanyRepository repo;

        public TestController(ICompanyRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [Route("turnover")]
        public async Task<HttpResponseMessage> TestTurnoverAsync()
        {
            var t = await repo.TestTurnover();
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                success = true,
                result = t
            });
        }

        [HttpGet]
        [Route("interest-turnover")]
        public async Task<HttpResponseMessage> TestTurnoverInterestAsync()
        {
            var t = await repo.TestTurnoverInterest();
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                success = true,
                result = t
            });
        }

        /*
         * fromdate
         * cifid
         * todate
         */

        [HttpGet]
        [Route("company")]
        public HttpResponseMessage GetAllCompany()
        {
            try
            {
                var companys = repo.GetAllCompany().ToList();
                if (companys == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, result = companys, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    result = companys

                });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [AdministratorLockoutFilter]
        [Route("unauthorized")]
        public HttpResponseMessage GetUnauthorized()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "sdfgthiuytr mwertyui" });
        }
    }

}
