using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Notification
{
   public  class NotificationViewModel
    {
        public int messageCount { get; set; }
        public string message { get; set; }
        public string operationURL { get; set; }
        public long notificationId { get; set; }
        public int staffId { get; set; }
        public string actionUrl { get; set; }
        public bool isActive { get; set; }
    }
}
