using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IRankRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RankRepository : IRankRepository
    {
        private FinTrakBankingContext context;

        public RankRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        public RankViewModel GetRank(int jobTitleId)
        {
            var rank = (from a in context.TBL_STAFF_RANK
                        select new RankViewModel
                        {
                            rankName = a.RANKNAME,
                            companyId = (short)a.COMPANYID,
                            rankId = a.RANKID
                        }).SingleOrDefault();
            return rank;
        }

        public IEnumerable<RankViewModel> GetRankByCompanyId(int companyId)
        {
            return from a in context.TBL_STAFF_RANK
                        where a.COMPANYID == companyId
                        select new RankViewModel
                        {
                            rankName = a.RANKNAME,
                            companyId = (short)a.COMPANYID,
                            rankId = a.RANKID
                        };
            
        }

        public IEnumerable<RankViewModel> GetRank()
        {
            var rank = (from a in context.TBL_STAFF_RANK
                        select new RankViewModel
                        {
                            rankName = a.RANKNAME,
                            companyId = (short)a.COMPANYID,
                            rankId = a.RANKID
                        });
            return rank;
        }
    }
}