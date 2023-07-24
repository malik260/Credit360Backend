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
        IEnumerable<CallMemoViewModel> GetCustomerCallMemo(int staffId, int customerId);
        IEnumerable<CallMemoViewModel> GetAllCallMemo(int staffId);
        bool GoForCallMemoApproval(CallMemoViewModel entity);
        bool SubmitApproval(CallMemoViewModel model);
        int AddCallMemo(CallMemoViewModel model);
        bool UpdateCallMemo(int limitId, CallMemoViewModel model);
        CallMemoViewModel GetCallMemoById(int callMemoID);

        IEnumerable<CallMemoViewModel> GetCallMemoWaitingForApproval(int staffId);
        IEnumerable<CallMemoViewModel> SearchCallMemo(int staffId, CallMemoViewModel model);
        IEnumerable<CallMemoViewModel> GetCustomerApprovedCallMemo(int staffId, int customerId);
        #endregion
    }
}
