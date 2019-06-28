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
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class ProjectSiteReportController : ApiControllerBase
    {
        private IProjectSiteReportRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ProjectSiteReportController(IProjectSiteReportRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("project-site-report/{id}")]
        public HttpResponseMessage GetProjectSiteReport(int id)
        {
            try
            {
                IEnumerable<ProjectSiteReportViewModel> response = repo.GetProjectSiteReports(id);
                if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "No record found" }); }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-customer-account/{searchString}")]
        public HttpResponseMessage Search(string searchString)
        {
            IEnumerable<LoanApplicationViewModel> response = repo.Search(searchString);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("project-site-report")]
        public HttpResponseMessage AddProjectSiteReport([FromBody] ProjectSiteReportViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddProjectSiteReport(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "There was an error creating this record" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("project-site-report/{id}")]
        public HttpResponseMessage UpdateProjectSiteReport([FromBody] ProjectSiteReportViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateProjectSiteReport(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("project-site-report/{id}")]
        public HttpResponseMessage DeleteProjectSiteReport(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteProjectSiteReport(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-report-type")]
        public HttpResponseMessage GetPsrReportTypes()
        {
            IEnumerable<PsrReportTypeViewModel> response = repo.GetPsrReportTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-next-inspection-task/{id}")]
        public HttpResponseMessage GetPsrNextInspectionTasks(int id)
        {
            IEnumerable<PsrNextInspectionTaskViewModel> response = repo.GetPsrNextInspectionTasks(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-next-inspection-task")]
        public HttpResponseMessage AddPsrNextInspectionTask([FromBody] PsrNextInspectionTaskViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddPsrNextInspectionTask(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "There was an error creating this record" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-next-inspection-task/{id}")]
        public HttpResponseMessage DeletePsrNextInspectionTask(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrNextInspectionTask(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-comment/{id}")]
        public HttpResponseMessage GetPsrComments(int id)
        {
            IEnumerable<PsrCommentViewModel> response = repo.GetPsrComments(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-comment")]
        public HttpResponseMessage AddPsrComment([FromBody] PsrCommentViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddPsrComment(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "There was an error creating this record" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-comment/{id}")]
        public HttpResponseMessage DeletePsrComment(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrComment(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-observation/{id}")]
        public HttpResponseMessage GetPsrObservations(int id)
        {
            IEnumerable<PsrObservationViewModel> response = repo.GetPsrObservations(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-observation")]
        public HttpResponseMessage AddPsrObservation([FromBody] PsrObservationViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddPsrObservation(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "There was an error creating this record" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-observation/{id}")]
        public HttpResponseMessage DeletePsrObservation(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrObservation(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-performance-evaluation/{id}")]
        public HttpResponseMessage GetPsrPerformanceEvaluations(int id)
        {
            IEnumerable<PsrPerformanceEvaluationViewModel> response = repo.GetPsrPerformanceEvaluations(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-performance-evaluation")]
        public HttpResponseMessage AddPsrPerformanceEvaluation([FromBody] PsrPerformanceEvaluationViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddPsrPerformanceEvaluation(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "There was an error creating this record" });
            }
        }


        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-performance-evaluation/{id}")]
        public HttpResponseMessage DeletePsrPerformanceEvaluation(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrPerformanceEvaluation(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-recommendation/{id}")]
        public HttpResponseMessage GetPsrRecommendations(int id)
        {
            IEnumerable<PsrRecommendationViewModel> response = repo.GetPsrRecommendations(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-recommendation")]
        public HttpResponseMessage AddPsrRecommendation([FromBody] PsrRecommendationViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddPsrRecommendation(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = "There was an error creating this record" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-recommendation/{id}")]
        public HttpResponseMessage DeletePsrRecommendation(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrRecommendation(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion
    }
}
