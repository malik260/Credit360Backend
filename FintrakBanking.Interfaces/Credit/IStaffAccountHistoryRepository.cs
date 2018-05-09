using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IStaffAccountHistoryRepository
    {
        bool AddStaffAccountHistory(StaffAccountHistoryViewModel entity);
        IEnumerable<StaffAccountHistoryViewModel> GetStaffAccountHistory(int staffId);
        bool ApproveStaffAccountHistory(ReasignedAccountApprovalViewModel entity);
        StaffMISHistoryViewModel GetSelectedLoanDetails(int companyId, int loanId, int productTypeId);
        IEnumerable<StaffAccountHistoryViewModel> GetAllStaffAccountHistory();
        StaffMISHistoryViewModel GetSelectedApprovalLoanDetails(ReasignedAccountApprovalViewModel entity);
     }
}
