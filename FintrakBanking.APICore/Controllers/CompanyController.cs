using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class CompanyController : ApiControllerBase
    {
        private ICompanyRepository repo;

        public CompanyController(ICompanyRepository _repo)
        {
            this.repo = _repo;
        }

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
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }

        //}


        [HttpGet]
        [Route("company")]
        public HttpResponseMessage GetCompanies()
        {
            try
            {
                var companys = repo.GetCompanies().ToList();
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

        [HttpGet]
        [Route("company/{companyId}")]
        public HttpResponseMessage Get(int companyId)
        {
            try
            {

                var company = repo.GetCompanyViewModel(companyId);
                if (company == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = company });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("languages")]
        public HttpResponseMessage GetLanguages()
        {
            try
            {
                var data = repo.GetLanguages();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("nature-of-business")]
        public HttpResponseMessage GetNatureOfBusiness()
        {
            try
            {
                var data = repo.GetNatureOfBusiness();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        // POST api/values
        [HttpPost]
        [Route("company")]
        public HttpResponseMessage AddCompany([FromBody] CompanyViewModel model)
        {
            try
            {
                var data = repo.AddCompany(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = true, message = "company has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "company not created" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("company/{companyId}")]
        public HttpResponseMessage UpdateCompany(int companyId, [FromBody] CompanyViewModel model)
        {
            try
            {
                var data = repo.UpdateCompany(companyId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "company has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "company has not been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPut]
        [Route("companys/{companyId}")]
        public HttpResponseMessage UpdateCompanies(int companyId, [FromBody] CompanyViewModel model)
        {
            try
            {
                var data = repo.UpdateCompanies(companyId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "company has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "company has not been updated successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}