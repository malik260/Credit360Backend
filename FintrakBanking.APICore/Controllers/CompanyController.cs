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
        public HttpResponseMessage GetAllCompany(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var companys = repo.GetAllCompany().ToList();
                    if (companys == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new
                    {
                        success = true,
                        result = companys
                    }));  //companys.ToList()
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("{companyId}")]
        public HttpResponseMessage Get(HttpRequestMessage request, int companyId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    //var companys = repo.GetcompanyViewModel(companyId);
                    //response = request.CreateResponse(HttpStatusCode.OK, Ok(companys);

                    var company = repo.GetCompanyViewModel(companyId);
                    if (company == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = company }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        // POST api/values
        [HttpPost]
        [Route("")]
        public HttpResponseMessage AddCompany(HttpRequestMessage request, [FromBody] CompanyViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.AddCompany(model);
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                    Ok(new { success = true, message = "company has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                                    Ok(new { success = false, message = "company not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPut]
        [Route("{companyId}")]
        public HttpResponseMessage UpdateCompany(HttpRequestMessage request, int companyId, [FromBody] CompanyViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (model == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "No record found" }));
                }

                var company = repo.GetCompanyViewModel(companyId);
                if (company == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    repo.UpdateCompany(model);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = model.companyId, message = "company has been updated successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
    }
}