using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IConditionPrecedentRepository
    {
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByApplicationId(int applicationId);

        IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedent();

        bool AddConditionPrecedent(ConditionPrecedentViewModel model);

        bool UpdateConditionPrecedent(ConditionPrecedentViewModel model, int conditionPrecedentId);

        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentTemplate();

        bool AddConditionPrecedentTemplate(ConditionPrecedentViewModel model);

        bool UpdateConditionPrecedentTemplate(ConditionPrecedentViewModel model, int conditionPrecedentId);
    }
}
