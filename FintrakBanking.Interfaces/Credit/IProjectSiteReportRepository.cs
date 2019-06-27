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
    }
}
