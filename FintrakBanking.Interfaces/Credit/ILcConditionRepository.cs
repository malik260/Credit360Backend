using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcConditionRepository
    {
        LcConditionViewModel GetLcCondition(int id);

        IEnumerable<LcConditionViewModel> GetLcConditions();

        IEnumerable<LcConditionViewModel> GetLcConditionsBylcIssuanceId(int lcIssuanceId);

        bool AddLcCondition(LcConditionViewModel model);

        bool UpdateLcCondition(LcConditionViewModel model, int id, UserInfo user);

        bool DeleteLcCondition(int id, UserInfo user);
    }
}
