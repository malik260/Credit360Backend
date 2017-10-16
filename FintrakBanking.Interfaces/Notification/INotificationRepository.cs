
using FintrakBanking.ViewModels.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Notification
{
   
    public interface INotificationRepository
    {
      
        IEnumerable<NotificationViewModel> GetNotification(int staffId, int companyId);

        IEnumerable<NotificationViewModel> GetNotificationForFinalState(int staffId, int companyId);
    }
}
