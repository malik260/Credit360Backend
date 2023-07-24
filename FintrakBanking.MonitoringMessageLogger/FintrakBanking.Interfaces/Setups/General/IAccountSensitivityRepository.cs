using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IAccountSensitivityRepository
    {
        IEnumerable<AccountSensitivityViewModel> GetAllAccountSensitivityLevels();

        AccountSensitivityViewModel GetAccountSensitivityLevelsByLevelId(int sensitivityId);
    }
}