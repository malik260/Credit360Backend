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

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/test/company")]
    public class TestController : ApiController
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

        [HttpPost]
        [Route("turnover")]
        public async Task<HttpResponseMessage> TestTurnoverInterestAsync([FromBody] InputVM body)
        {
            var t = await repo.TestTurnoverInterest(body);
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

        //[HttpGet]
        //[Route("")]
        //public HttpResponseMessage GetAllCompany()
        //{
        //    try
        //    {
        //        var companys = repo.GetAllCompany().ToList();
        //        if (companys == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //               new { success = false, result = companys, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            success = true,
        //            result = companys

        //        });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }

        //}
    }

}
