using FintrakBanking.ViewModels.AlertMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.AlertMonitoring
{
  public interface ISLANotification
    {
        IEnumerable<SLANotificationViewModel> StaffSpecificBasedApprovalNotification();

        IEnumerable<SLANotificationViewModel> RoleBasedApprovalNotification();

        IEnumerable<SLANotificationViewModel> StaffSetupBasedApprovalNotification();
     
    }
}
