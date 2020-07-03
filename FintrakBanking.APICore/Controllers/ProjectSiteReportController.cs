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
        [Route("project-site-report")]
        public HttpResponseMessage GetProjectSiteReport()
        {
                IEnumerable<ProjectSiteReportViewModel> response = repo.GetProjectSiteReports();
            if (response == null) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("project-site-report/{projectSiteReportId}/projectSiteReportId")]
        public HttpResponseMessage GetProjectSiteReport(int projectSiteReportId)
        {
            
                IEnumerable<ProjectSiteReportViewModel> response = repo.GetProjectSiteReports(projectSiteReportId);
            if (response == null) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("facilities/{projectSiteReportId}")]
        public HttpResponseMessage GetFacilities(int projectSiteReportId)
        {
                IEnumerable<LoanApplicationViewModel> response = repo.GetFacilities(projectSiteReportId);
            if (response == null) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        
        [HttpPost]
        [ClaimsAuthorization]
        [Route("project-site-gofor-approval")]
        public HttpResponseMessage SubmitApproval([FromBody] ProjectSiteReportViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.SubmitApproval(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("project-site-gofor-acceptance")]
        public HttpResponseMessage SubmitAcceptance([FromBody] ProjectSiteReportViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.SubmitAcceptance(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("project-site-schedule-for-approval")]
        public HttpResponseMessage ProjectSiteReportGoForApproval([FromBody] ProjectSiteReportViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.ProjectSiteReportGoForApproval(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("project-site-report-approval")]
        public HttpResponseMessage GetProjectSiteReportApproval()
        {
                IEnumerable<ProjectSiteReportViewModel> response = repo.GetProjectSiteReportApprovals(token.GetStaffId);
            if (response == null) {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("project-site-report-approved")]
        public HttpResponseMessage GetProjectSiteReportApproved()
        {
            IEnumerable<ProjectSiteReportViewModel> response = repo.GetProjectSiteReportApproved(token.GetStaffId);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-customer-account/{searchString}")]
        public HttpResponseMessage Search(string searchString)
        {
            IEnumerable<LoanApplicationViewModel> response = repo.Search(searchString);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-customer-loans/{id}")]
        public HttpResponseMessage ProjectSiteReportLoans(int id)
        {
            IEnumerable<LoanApplicationViewModel> response = repo.ProjectSiteReportLoans(id);
            if (response == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            else
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
                if (response != 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch(SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = ex.Message });

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

        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-report-type-by-id/{id}")]
        public HttpResponseMessage GetPsrReportTypesById(int id)
        {
            PsrReportTypeViewModel response = repo.GetPsrReportTypesById(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response });
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

        [HttpPut]
        [ClaimsAuthorization]
        [Route("psr-next-inspection-task/{id}")]
        public HttpResponseMessage UpdatePsrNextInspectionTask([FromBody] PsrNextInspectionTaskViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdatePsrNextInspectionTask(model,id,user);
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

        [HttpPut]
        [ClaimsAuthorization]
        [Route("psr-comment/{id}")]
        public HttpResponseMessage UpdatePsrComment([FromBody] PsrCommentViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdatePsrComment(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
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
        [HttpPut]
        [ClaimsAuthorization]
        [Route("psr-observation/{id}")]
        public HttpResponseMessage UpdatePsrObservation([FromBody] PsrObservationViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdatePsrObservation(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
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

        [HttpPut]
        [ClaimsAuthorization]
        [Route("psr-performance-evaluation/{id}")]
        public HttpResponseMessage GetPsrPerformanceEvaluations(int id, [FromBody] PsrPerformanceEvaluationViewModel model)
        {
            model.BranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;


           var response = repo.UpdatePsrPerformanceEvaluation(model, id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
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
        [HttpPut]
        [ClaimsAuthorization]
        [Route("psr-recommendation/{id}")]
        public HttpResponseMessage UpdatePsrRecommendation([FromBody] PsrRecommendationViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdatePsrRecommendation(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
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

        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-report")]
        public HttpResponseMessage GeneratePSRReport(int id)
        {
            try
            {
             
                var response = repo.GeneratePSRReport(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response});
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

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-performance-analysis/{id}")]
        public HttpResponseMessage GetPsrPerformanceAnalysis(int id)
        {
            IEnumerable<PsrPerformanceAnalysisViewModel> response = repo.GetPsrPerformanceAnalysis(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("psr-performance-analysis/{id}")]
        public HttpResponseMessage GetPsrPerformanceAnalysis(int id, [FromBody] PsrPerformanceAnalysisViewModel model)
        {
            model.BranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;

            var response = repo.UpdatePsrPerformanceAnalysis(model, id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-performance-analysis")]
        public HttpResponseMessage AddPsrPerformanceAnalysis([FromBody] PsrPerformanceAnalysisViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddPsrPerformanceAnalysis(model);
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
        [Route("psr-performance-analysis/{id}")]
        public HttpResponseMessage DeletePsrPerformanceAnalysis(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrPerformanceAnalysis(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-images/{id}")]
        public HttpResponseMessage GetPsrImages(int id)
        {
            IEnumerable<PsrImagesViewModel> response = repo.GetPsrImages(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("psr-comment-images/{id}")]
        public HttpResponseMessage GetPsrCommentImages(int id)
        {
            IEnumerable<PsrCommentImagesViewModel> response = repo.GetPsrCommentImages(id);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-image")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddPsrImagec()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try
            {
                var entity = new PsrImagesViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.fileSizeUnit = provider.FormData["fileSizeUnit"];
                entity.imageCaption = provider.FormData["imageCaption"];
                entity.projectSiteReportId = Convert.ToInt32(provider.FormData["projectSiteReportId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                int response = repo.AddPsrImage(entity, buffer);


                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-image/{id}")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdatePsrImagec(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try
            {
                var entity = new PsrImagesViewModel();
                entity.psrImageId = id;
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.fileSizeUnit = provider.FormData["fileSizeUnit"];
                entity.imageCaption = provider.FormData["imageCaption"];
                entity.projectSiteReportId = Convert.ToInt32(provider.FormData["projectSiteReportId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                int response = repo.UpdatePsrImage(entity, buffer);


                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-comment-image")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> AddPsrCommentImage()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try
            {
                var entity = new PsrCommentImagesViewModel();
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.fileSizeUnit = provider.FormData["fileSizeUnit"];
                entity.imageCaption = provider.FormData["imageCaption"];
                entity.projectSiteReportId = Convert.ToInt32(provider.FormData["projectSiteReportId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                int response = repo.AddPsrCommentImage(entity, buffer);


                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("psr-comment-image/{id}")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdatePsrCommentImage(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            try
            {
                var entity = new PsrCommentImagesViewModel();
                entity.psrCommentImageId = id;
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                entity.fileSize = Convert.ToInt32(provider.FormData["fileSize"]);
                entity.fileSizeUnit = provider.FormData["fileSizeUnit"];
                entity.imageCaption = provider.FormData["imageCaption"];
                entity.projectSiteReportId = Convert.ToInt32(provider.FormData["projectSiteReportId"]);
                entity.overwrite = provider.FormData["overwrite"] == "true";
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                int response = repo.UpdatePsrCommentImage(entity, buffer);


                if (response == 2) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file has been uploaded successfully" });
                if (response == 3) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The file already exist" });
            }
            catch (Exception ex) { return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file:  " + ex.Message }); }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this file" });

        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-comment-image/{id}")]
        public HttpResponseMessage DeletePsrCommentImage(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrCommentImage(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("psr-image/{id}")]
        public HttpResponseMessage DeletePsrImage(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeletePsrImage(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

    }
}
