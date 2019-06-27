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
        ProjectSiteReportViewModel GetProjectSiteReport(int id);

        IEnumerable<ProjectSiteReportViewModel> GetProjectSiteReports();

        bool AddProjectSiteReport(ProjectSiteReportViewModel model);

        bool UpdateProjectSiteReport(ProjectSiteReportViewModel model, int id, UserInfo user);

        bool DeleteProjectSiteReport(int id, UserInfo user);

        IEnumerable<LoanApplicationViewModel> Search(string searchString);

        IEnumerable<PsrReportTypeViewModel> GetPsrReportTypes();


        #region

        IEnumerable<PsrCommentViewModel> GetPsrComment(int id);

        bool AddPsrComment(PsrCommentViewModel model);

        bool UpdatePsrComment(PsrCommentViewModel model, int id);

        bool DeletePsrComment(int id);


        #endregion

        #region
        IEnumerable<PsrNextInspectionTaskViewModel> GetPsrNextInspectionTask(int id);

        bool AddPsrNextInspectionTask(PsrNextInspectionTaskViewModel model);

        bool UpdatePsrNextInspectionTask(PsrNextInspectionTaskViewModel model, int id);

        bool DeletePsrNextInspectionTask(int id);

        #endregion

        #region

        IEnumerable<PsrObservationViewModel> GetPsrObservations(int id);

        bool AddPsrObservation(PsrObservationViewModel model);

        bool UpdatePsrObservation(PsrObservationViewModel model, int id);

        bool DeletePsrObservation(int id);

        #endregion


        #region

        IEnumerable<PsrPerformanceEvaluationViewModel> GetPsrPerformanceEvaluations(int id);

        bool AddPsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model);

        bool UpdatePsrPerformanceEvaluation(PsrPerformanceEvaluationViewModel model, int id);

        bool DeletePsrPerformanceEvaluation(int id);

        #endregion


        #region
        IEnumerable<PsrRecommendationViewModel> GetPsrRecommendations(int id);

        bool AddPsrRecommendation(PsrRecommendationViewModel model);

        bool UpdatePsrRecommendation(PsrRecommendationViewModel model, int id);

        bool DeletePsrRecommendation(int id);

        #endregion

    }
}
