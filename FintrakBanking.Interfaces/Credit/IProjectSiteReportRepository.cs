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

        #endregion


        #region
        IEnumerable<PsrRecommendationViewModel> GetPsrRecommendations(int id);

        bool AddPsrRecommendation(PsrRecommendationViewModel model);

        bool DeletePsrRecommendation(int id, UserInfo user);

        #endregion

    }
}
