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

        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentTemplate();

        bool AddConditionPrecedentTemplate(ConditionPrecedentViewModel model);

        bool UpdateConditionPrecedentTemplate(ConditionPrecedentViewModel model, int conditionPrecedentId);

        bool RemoveLoanConditionPrecedent(int id, UserInfo user);

        bool EditLoanConditionPrecedent(int id, ConditionPrecedentViewModel entity);

        IEnumerable<ComplianceTimelineViewModel> GetComplianceTimelineTemplate();

        bool AddComplianceTimelineTemplate(ComplianceTimelineViewModel model);

        bool UpdateComplianceTimelineTemplate(ComplianceTimelineViewModel model, int timelineId);

    }
}
