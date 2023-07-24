using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.credit
{
    public interface IProjectSiteReportRepository
    {
        IEnumerable<LoanApplicationViewModel> ProjectSiteReportLoans(int id);
        bool SubmitAcceptance(ProjectSiteReportViewModel model);
        IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReportApproved(int staffId);
        int UpdatePsrCommentImage(PsrCommentImagesViewModel model, byte[] buffer);
        int UpdatePsrImage(PsrImagesViewModel model, byte[] buffer);
        bool DeletePsrImage(int id, UserInfo user);
        bool DeletePsrCommentImage(int id, UserInfo user);
        int AddPsrCommentImage(PsrCommentImagesViewModel model, byte[] buffer);
        IEnumerable<PsrCommentImagesViewModel> GetPsrCommentImages(int id);
        PsrCommentImagesViewModel GetPsrCommentImage(int id);
        int AddPsrImage(PsrImagesViewModel model, byte[] buffer);
        IEnumerable<PsrImagesViewModel> GetPsrImages(int id);
        PsrImagesViewModel GetPsrImage(int id);
        IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports();

        int AddProjectSiteReport(ProjectSiteReportViewModel model);

        bool UpdateProjectSiteReport(ProjectSiteReportViewModel model, int id, UserInfo user);

        bool DeleteProjectSiteReport(int id, UserInfo user);

        IEnumerable<LoanApplicationViewModel> Search(string searchString);

        IEnumerable<PsrReportTypeViewModel> GetPsrReportTypes();

        PsrReportViewModel GeneratePSRReport(int id);

        IEnumerable<LoanApplicationViewModel> GetFacilities(int id);

        bool ProjectSiteReportGoForApproval(ProjectSiteReportViewModel model);
        PsrReportTypeViewModel GetPsrReportTypesById(int id);


        #region

        IEnumerable<PsrCommentViewModel> GetPsrComments(int id);

        bool AddPsrComment(PsrCommentViewModel model);

        bool DeletePsrComment(int id, UserInfo user);


        #endregion

        #region
        IEnumerable<PsrNextInspectionTaskViewModel> GetPsrNextInspectionTasks(int id);

        bool AddPsrNextInspectionTask(PsrNextInspectionTaskViewModel model);

        bool DeletePsrNextInspectionTask(int id, UserInfo user);

        #endregion

        #region

        IEnumerable<PsrObservationViewModel> GetPsrObservations(int id);

        bool AddPsrObservation(PsrObservationViewModel model);

        bool DeletePsrObservation(int id, UserInfo user);

        #endregion


        #region

        IEnumerable<PsrPerformanceEvaluationViewModel> GetPsrPerformanceEvaluations(int id);

        bool AddPsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model);

        bool DeletePsrPerformanceEvaluation(int id, UserInfo user);

        bool UpdatePsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model, int id);

        IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReportApprovals(int staffId);

        bool SubmitApproval(ProjectSiteReportViewModel model);

        #endregion


        #region
        IEnumerable<PsrRecommendationViewModel> GetPsrRecommendations(int id);

        bool AddPsrRecommendation(PsrRecommendationViewModel model);

        bool DeletePsrRecommendation(int id, UserInfo user);

        bool UpdatePsrRecommendation(PsrRecommendationViewModel model, int id, UserInfo user);

        #endregion

        bool UpdatePsrComment(PsrCommentViewModel model, int id, UserInfo user);

        bool UpdatePsrNextInspectionTask(PsrNextInspectionTaskViewModel model, int id, UserInfo user);

        bool UpdatePsrObservation(PsrObservationViewModel model, int id, UserInfo user);

        IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports(int projectSiteReportId);
        bool DeletePsrPerformanceAnalysis(int id, UserInfo user);
        bool AddPsrPerformanceAnalysis(PsrPerformanceAnalysisViewModel model);
        bool UpdatePsrPerformanceAnalysis(PsrPerformanceAnalysisViewModel model, int id);
        IEnumerable<PsrPerformanceAnalysisViewModel> GetPsrPerformanceAnalysis(int id);
    }
}
