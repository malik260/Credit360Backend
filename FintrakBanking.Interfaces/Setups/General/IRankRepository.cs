using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IRankRepository
    {
        IEnumerable<RankViewModel> GetRank();

        RankViewModel GetRank(int rankId);

        IEnumerable<RankViewModel> GetRankByCompanyId(int companyId);
    }
}