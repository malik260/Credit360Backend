using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IAccountCategoryRepository
    {
        bool AddFinanceAccountCategorySetup(AccountCategoryViewModel category);

        IEnumerable<AccountCategoryViewModel> GetAllAccountCategory();

        AccountCategoryViewModel GetAccountCategoryById(int categoryId);
    }
}