using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setups")]
    public class CompanyController : ApiControllerBase
    {
        private ICompanyRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
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
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }

        //}


        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-login-company")]
        public HttpResponseMessage GetLoginCompany()
        {
            try
            {
                var company = repo.GetCompanyViewModel(token.GetCompanyId);
                if (company == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = company });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        // POST api/values
        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPut]
        [ClaimsAuthorization]
        [Route("companys/{companyId}")]
        public HttpResponseMessage UpdateCompanies(int companyId, [FromBody] CompanyViewModel model)
        {
            try
            {
                var data = repo.UpdateCompanies(companyId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "Changes Saved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "Saved changes not successfull" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #region Company Director
        [HttpGet]
        [ClaimsAuthorization]
        [Route("company-director")]
        public HttpResponseMessage GetCompanyDirectors()
        {
            try
            {
                var data = repo.GetCompanyDirectors();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("company-director-by-companyId")]
        public HttpResponseMessage GetCompanyDirectorsByCompanyId()
        {
            try
            {
                var data = repo.GetCompanyDirectorsByCompanyId(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("company-director-by-companyId")]
        public HttpResponseMessage GetCustomerCompanyDirectorsByCompanyId(int companyId)
        {
            try
            {
                var data = repo.GetCustomerCompanyDirectorsByCompanyId(companyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("company-director")]
        public HttpResponseMessage AddUpdateCompanyDirector([FromBody]CompanyDirectorsViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.companyDirectorId != 0 || entity.companyDirectorId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateCompanyDirectorBVN(entity.companyId, entity.bvn))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = false, message = $"Company Director with BVN {entity.bvn} already exist for the select company." });
                    }
                    if (repo.ValidateCompanyDirectorEmail(entity.companyId, entity.email))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = false, message = $"Company Director with email {entity.email} already exist for the select company." });
                    }
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateCompanyDirector(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error {createUpdate} this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("company-director")]
        public HttpResponseMessage DeleteCustomer(int companyDirectorId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                };

                var data = repo.DeleteCompanyDirector(companyDirectorId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The record has been deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error deleted this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleted this record {e.Message}" });
            }
        }

        #endregion
    }
}