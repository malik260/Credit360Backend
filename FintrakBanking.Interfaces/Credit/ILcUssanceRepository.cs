using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcUssanceRepository
    {
        LcUssanceViewModel GetLcUssance(int id);

        IEnumerable<LcUssanceViewModel> GetLcUssances();

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceApproval(int staffId);

        IEnumerable<LcIssuanceViewModel> GetLcUssanceForLcIssuance();

        LcUssanceViewModel AddLcUssance(LcUssanceViewModel model);

        bool UpdateLcUssance(LcUssanceViewModel model, int id, UserInfo user);

        bool DeleteLcUssance(int id, UserInfo user);
    }
}
