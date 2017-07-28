using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Notification
{
    [RoutePrefix("api/v1/notification")]
    public interface INotificationRepository
    {
        [HttpGet]
        [Route("fee")]
        IEnumerable<NotificationViewModel> GetNotification(int staffId, int companyId);
    }
}
