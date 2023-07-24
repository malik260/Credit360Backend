using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IAccountTypeRepository
    {
        bool AddAccountType(AddAccountTypeViewModel accounttype);

        IEnumerable<AccountTypeViewModel> GetAllAccountType();

        bool UpdateAccountType(int accountTypeId, AccountTypeViewModel accounttype);

        AccountTypeViewModel GetAllAccountTypeById(int accountTypeId);

        bool DeleteAcountType(int accountTypeId, UserInfo user);
    }
}