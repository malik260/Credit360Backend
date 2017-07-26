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
            var rank = (from a in context.tbl_Staff_Rank
                        select new RankViewModel
                        {
                            rankName = a.RankName,
                            companyId = (short)a.CompanyId,
                            rankId = a.RankId
                        }).SingleOrDefault();
            return rank;
        }

        public IEnumerable<RankViewModel> GetRankByCompanyId(int companyId)
        {
            return from a in context.tbl_Staff_Rank
                        where a.CompanyId == companyId
                        select new RankViewModel
                        {
                            rankName = a.RankName,
                            companyId = (short)a.CompanyId,
                            rankId = a.RankId
                        };
            
        }

        public IEnumerable<RankViewModel> GetRank()
        {
            var rank = (from a in context.tbl_Staff_Rank
                        select new RankViewModel
                        {
                            rankName = a.RankName,
                            companyId = (short)a.CompanyId,
                            rankId = a.RankId
                        });
            return rank;
        }
    }
}