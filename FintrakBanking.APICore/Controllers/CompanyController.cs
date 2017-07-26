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
    [RoutePrefix("api/v1/setups/company")]
    public class CompanyController : ApiControllerBase
    {
        private ICompanyRepository repo;

        public CompanyController(ICompanyRepository _repo)
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
                       new { success = false, message = "No record found" });
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
        [Route("{companyId}")]
        public HttpResponseMessage Get(  int companyId)
        { try
                {
                    //var companys = repo.GetcompanyViewModel(companyId);
                    //return Request.CreateResponse(HttpStatusCode.OK, Ok(companys);

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
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                } 
        }

        // POST api/values
        [HttpPost]
        [Route("")]
        public HttpResponseMessage AddCompany(   [FromBody] CompanyViewModel model)
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
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                } 
        }

        [HttpPut]
        [Route("{companyId}")]
        public HttpResponseMessage UpdateCompany(   int companyId, [FromBody] CompanyViewModel model)
        { 
                if (model == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                var company = repo.GetCompanyViewModel(companyId);
                if (company == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                try
                {
                    repo.UpdateCompany(model);

                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = model.companyId, message = "company has been updated successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
                } 
        }
    }
}