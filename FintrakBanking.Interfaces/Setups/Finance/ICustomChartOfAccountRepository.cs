using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface ICustomChartOfAccountRepository
    {
        CustomChartOfAccountViewModel GetCustomChartOfAccount(int customChartOfAccountId);

        IEnumerable<CustomChartOfAccountViewModel> GetAllCustomChartOfAccount();

        IEnumerable<CustomChartOfAccountViewModel> GetAllCustomChartOfAccountByCompanyId(int companyId);

        IEnumerable<CustomChartOfAccountViewModel> GetnostroCustomChartOfAccountByCompanyId(int companyId);

        bool AddCustomChartOfAccount(CustomChartOfAccountViewModel model);

        bool UpdateCustomChartOfAccount(CustomChartOfAccountViewModel model, int customChartOfAccountId);
    }
}