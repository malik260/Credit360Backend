using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IChartOfAccountRepository
    {
        IEnumerable<ChartOfAccountViewModel> GetAllAccounts();

        IEnumerable<ChartOfAccountViewModel> GetAccountsByCategory(short accountCategoryId);

        IEnumerable<LookupViewModel> GetFinancialSatementCaptionLookup();

        ChartOfAccountViewModel GetAccountByAccountId(short accountId);
        ChartOfAccountViewModel GetTempAccountDetail(int accountId);
        IEnumerable<ChartOfAccountViewModel> GetAccountsAwaitingApprovals(int staffId, int companyId);
        bool GoForApproval(ApprovalViewModel entity);
        bool AddTempAccount(ChartOfAccountViewModel account);
        bool IsAccountCodeAlreadyExist(string accountCode);
        bool IsTempAccountExist(string accountCode);
        bool UpdateAccount(short accountId, ChartOfAccountViewModel account);

        bool DeleteAccount(short accountId, UserInfo user);

        IEnumerable<ChartOfAccountClassViewModel> GetChartOfAccountClasses();

        string GetAccountNameByAccountCode(string accountCode);

        int GetAccountDefaultCurrency(int glAccountId, int companyId);
    }
}
