using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcCashBuildUpPlanRepository
    {
        LcCashBuildUpPlanViewModel GetLcCashBuildUpPlan(int id);

        IEnumerable<LcCashBuildUpPlanViewModel> GetLcCashBuildUpPlansByLcIssuanceId(int id);
        IEnumerable<LcCashBuildUpPlanViewModel> GetLcCashBuildUpReferenceTypes();

        bool AddLcCashBuildUpPlan(LcCashBuildUpPlanViewModel model);

        bool UpdateLcCashBuildUpPlan(LcCashBuildUpPlanViewModel model, int id, UserInfo user);

        bool DeleteLcCashBuildUpPlan(int id, UserInfo user);
    }
}
