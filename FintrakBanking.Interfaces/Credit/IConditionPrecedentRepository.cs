using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IConditionPrecedentRepository
    {
        IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedent();

        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByApplicationId(int applicationId);

        bool AddConditionPrecedent(ConditionPrecedentViewModel model);

        bool UpdateConditionPrecedent(ConditionPrecedentViewModel model, int conditionPrecedentId);
    }
}
