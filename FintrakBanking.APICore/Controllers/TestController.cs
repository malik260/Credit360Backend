using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
        [Route("")]
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
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
    }
}
