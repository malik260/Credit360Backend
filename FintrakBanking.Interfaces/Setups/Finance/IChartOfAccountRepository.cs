using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Business;
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
        ChartOfAccountViewModel GetTempAccountDetail(int accountId);
        IEnumerable<ChartOfAccountViewModel> GetAccountsAwaitingApprovals(int accountId, int companyId);
        bool GoForApproval(ApprovalViewModel entity);
        bool AddTempAccount(ChartOfAccountViewModel account);
        bool IsAccountCodeAlreadyExist(string accountCode);
        bool IsAccountExist(string accountCode);
        bool UpdateAccount(short accountId, ChartOfAccountViewModel account);

        bool DeleteAccount(short accountId, UserInfo user);
    }
}