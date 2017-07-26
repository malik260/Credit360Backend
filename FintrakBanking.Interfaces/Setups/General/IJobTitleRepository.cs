using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IJobTitleRepository
    {
        IEnumerable<JobTitleViewModel> JobTitle();

        JobTitleViewModel GetJobTitle(int jobTitleId);

        IEnumerable<JobTitleViewModel> GetJobTitleByCompanyId(int companyId);
    }
}