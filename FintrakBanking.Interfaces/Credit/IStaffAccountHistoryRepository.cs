using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
   public  interface IStaffAccountHistoryRepository
    {
        bool AddStaffAccountHistory(StaffAccountHistoryViewModel entity);
        IEnumerable<StaffAccountHistoryViewModel> GetStaffAccountHistory(StaffAccountHistoryViewModel entity);
        bool UpdateStaffAccountHistory(StaffAccountHistoryViewModel entity);
        bool ApproveStaffAccountHistory(StaffAccountHistoryViewModel entity);
    }
}
