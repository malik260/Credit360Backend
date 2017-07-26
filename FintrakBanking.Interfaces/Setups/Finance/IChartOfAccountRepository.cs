using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IChartOfAccountRepository
    {
        IEnumerable<ChartOfAccountViewModel> GetAllAccounts();

        IEnumerable<ChartOfAccountViewModel> GetAccountsByCategory(short accountCategoryId);

        IEnumerable<LookupViewModel> GetFinancialSatementCaptionLookup();

        ChartOfAccountViewModel GetAccountViewModel(short accountId);

        int AddAccount(ChartOfAccountViewModel account);

        bool UpdateAccount(short accountId, ChartOfAccountViewModel account);

        bool DeleteAccount(short accountId, UserInfo user);
    }
}