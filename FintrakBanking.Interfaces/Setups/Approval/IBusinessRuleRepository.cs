using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IBusinessRuleRepository
    {
        IEnumerable<BusinessRuleViewModel> GetBusinessRule(int companyId);

        BusinessRuleViewModel GetBusinessRuleById(int businessRuleId);

        bool AddBusinessRule(BusinessRuleViewModel model);

        bool UpdateBusinessRule(BusinessRuleViewModel model, int businessRuleId, UserInfo user);

        bool DeleteBusinessRule(int businessRuleId, UserInfo user);
        bool DeleteDynamicBusinessRule(int businessRuleId, UserInfo user);
    }
}