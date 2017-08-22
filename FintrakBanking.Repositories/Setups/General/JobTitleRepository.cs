using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{

    public class JobTitleRepository : IJobTitleRepository
    {
        private FinTrakBankingContext context;

        public JobTitleRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        public IEnumerable<JobTitleViewModel> JobTitle()
        {
            var jobtitle = from a in context.tbl_Staff_JobTitle
                           select new JobTitleViewModel
                           {
                               jobTitle = a.JobTitleName,
                               companyId = (short)a.CompanyId,
                               jobTitleId = a.JobTitleId
                           };
            return jobtitle.ToList();
        }

        public JobTitleViewModel GetJobTitle(int jobTitleId)
        {
            var jobtitle = (from a in context.tbl_Staff_JobTitle
                            where a.JobTitleId == jobTitleId
                            select new JobTitleViewModel
                            {
                                jobTitle = a.JobTitleName,
                                companyId = (short)a.CompanyId,
                                jobTitleId = a.JobTitleId
                            }).SingleOrDefault();
            return jobtitle;
        }

        public IEnumerable<JobTitleViewModel> GetJobTitleByCompanyId(int companyId)
        {
            var jobtitle = (from a in context.tbl_Staff_JobTitle
                            where (short)a.CompanyId == companyId
                            select new JobTitleViewModel
                            {
                                jobTitle = a.JobTitleName,
                                companyId = (short)a.CompanyId,
                                jobTitleId = a.JobTitleId
                            });
            return jobtitle;
        }
    }
}