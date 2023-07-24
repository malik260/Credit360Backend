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
        List<LcUssanceViewModel> GetLcUssanceByLCIssuanceId(int lcIssuanceId);

        LcUssanceViewModel GetLcUssanceByLCUsanceId(int lcUsanceId);
        LcUssanceViewModel GetLcUssanceExtensionByTempLcUsanceId(int tempLcUsanceId);
        List<LcUssanceViewModel> GetLcUssanceExtensionsByLcUsanceId(int lcUsanceId);

        IEnumerable<LcUssanceViewModel> GetLcUssances();

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceExtensionApproval(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceApproval(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceExtension(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssance(int staffId);

        LcUssanceViewModel AddLcUssanceExtension(LcUssanceViewModel model);
        LcUssanceViewModel AddLcUssance(LcUssanceViewModel model);

        bool UpdateLcUsanceExtension(LcUssanceViewModel model, int id, UserInfo user);
        bool UpdateLcUssance(LcUssanceViewModel model, int id, UserInfo user);

        bool DeleteLcUssance(int id, UserInfo user);
    }
}
