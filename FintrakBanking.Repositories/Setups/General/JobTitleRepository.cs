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
            var jobtitle = from a in context.TBL_STAFF_JOBTITLE
                           select new JobTitleViewModel
                           {
                               jobTitle = a.JOBTITLENAME,
                               companyId = (short)a.COMPANYID,
                               jobTitleId = a.JOBTITLEID
                           };
            return jobtitle.ToList();
        }

        public JobTitleViewModel GetJobTitle(int jobTitleId)
        {
            var jobtitle = (from a in context.TBL_STAFF_JOBTITLE
                            where a.JOBTITLEID == jobTitleId
                            select new JobTitleViewModel
                            {
                                jobTitle = a.JOBTITLENAME,
                                companyId = (short)a.COMPANYID,
                                jobTitleId = a.JOBTITLEID
                            }).SingleOrDefault();
            return jobtitle;
        }

        public IEnumerable<JobTitleViewModel> GetJobTitleByCompanyId(int companyId)
        {
            var jobtitle = (from a in context.TBL_STAFF_JOBTITLE
                            where (short)a.COMPANYID == companyId
                            select new JobTitleViewModel
                            {
                                jobTitle = a.JOBTITLENAME,
                                companyId = (short)a.COMPANYID,
                                jobTitleId = a.JOBTITLEID
                            });
            return jobtitle;
        }
    }
}