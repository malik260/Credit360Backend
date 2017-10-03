using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICallMemoRepository
    {
        #region "Call Limit"
        bool isLimitExist(CallLimitViewModel model);
        IEnumerable<CallMemoTypeViewModel> GetCallLimitType();
        IEnumerable<CallLimitViewModel> GetAllCallLimit(int companyId);
        List<CallLimitViewModel> GetCallLimitByTypeId(int limitId);
        bool AddCallLimit(CallLimitViewModel model);
        bool UpdateCallLimit(int limitId, CallLimitViewModel model);
        bool DeleteCallLimit(int limitId, UserInfo user);
        #endregion

        #region "Call Memo"
        IQueryable<CallMemoLoanSearchViewModel> SearchForCallMemoLoan(int staffId, string searchQuery);
        IEnumerable<CallMemoViewModel> GetAllCallMemo(int staffId);
        bool AddCallMemo(CallMemoViewModel model);
        bool UpdateCallMemo(int limitId, CallMemoViewModel model);
        #endregion
    }
}
